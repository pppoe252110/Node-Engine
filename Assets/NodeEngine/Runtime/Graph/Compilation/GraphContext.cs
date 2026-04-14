using System;
using System.Buffers;
using System.Collections.Generic;

/// <summary>
/// Runtime execution context for a compiled node graph.
/// Holds instruction array, memory slots, and execution stack.
/// </summary>
public class GraphContext : IDisposable
{
    /// <summary>Compiled node execution delegates, indexed by runtime node ID.</summary>
    public Func<GraphContext, ExecutionResult>[] Instructions;

    /// <summary>Per‑node persistent state storage (e.g., for throttle nodes).</summary>
    public Dictionary<BaseNode, object> NodeState = new();

    // Memory slots for data passing between nodes.
    private object[] _memory;
    private readonly int _memorySize;

    // Stack for deferred flow execution (LIFO order).
    private Stack<int> _executionStack = new Stack<int>();

    // Shared pool to reuse memory arrays across recompiles, reducing GC pressure.
    private static readonly ArrayPool<object> MemoryPool = ArrayPool<object>.Shared;

    private bool _disposed;

    /// <summary>
    /// Initializes a new execution context with a pre‑allocated memory array.
    /// </summary>
    /// <param name="memorySize">Number of memory slots required for data ports.</param>
    public GraphContext(int memorySize)
    {
        _memorySize = memorySize;
        _memory = MemoryPool.Rent(memorySize);
        // Clear the rented segment (may contain stale data from previous usage).
        Array.Clear(_memory, 0, memorySize);
    }

    /// <summary>
    /// Returns the rented memory array to the pool.
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            if (_memory != null)
            {
                MemoryPool.Return(_memory);
                _memory = null;
            }
            _disposed = true;
        }
    }

    /// <summary>
    /// Schedules a flow for later execution by pushing its instruction index onto the stack.
    /// Used for asynchronous branching (e.g., Sequence node).
    /// </summary>
    /// <param name="index">Runtime index of the target node.</param>
    public void PushFlow(int index)
    {
        if (index >= 0 && index < Instructions.Length)
            _executionStack.Push(index);
    }

    /// <summary>
    /// Clears the stack and begins execution from the given start node.
    /// </summary>
    /// <param name="startIndex">Runtime index of the entry node.</param>
    public void ExecuteFlow(int startIndex)
    {
        _executionStack.Clear();
        _executionStack.Push(startIndex);
        ExecutePending();
    }

    /// <summary>
    /// Executes a sub‑graph synchronously, blocking the current node until the branch completes.
    /// Essential for loops where each iteration must run fully before the next begins.
    /// </summary>
    /// <param name="startIndex">Runtime index of the first node in the sub‑graph.</param>
    public void ExecuteSubFlow(int startIndex)
    {
        if (startIndex < 0 || startIndex >= Instructions.Length) return;

        // Remember current stack depth so we only process this branch,
        // then restore the previous stack afterwards.
        int initialDepth = _executionStack.Count;
        _executionStack.Push(startIndex);

        while (_executionStack.Count > initialDepth)
        {
            int ip = _executionStack.Pop();
            if (ip < 0 || ip >= Instructions.Length) continue;

            var result = Instructions[ip](this);

            if (result.Type == ExecutionResultType.Continue && result.NextIndex >= 0)
                _executionStack.Push(result.NextIndex);
            else if (result.Type == ExecutionResultType.Halt)
            {
                // Halt stops all execution immediately.
                _executionStack.Clear();
                break;
            }
        }
    }

    /// <summary>
    /// Executes multiple flows concurrently by pushing all start indices onto the stack.
    /// </summary>
    /// <param name="startIndices">Collection of node indices to execute.</param>
    public void ExecuteMultipleFlows(IEnumerable<int> startIndices)
    {
        _executionStack.Clear();
        foreach (int idx in startIndices)
            if (idx >= 0 && idx < Instructions.Length)
                _executionStack.Push(idx);
        ExecutePending();
    }

    /// <summary>
    /// Processes the execution stack until it is empty or a Halt result occurs.
    /// </summary>
    private void ExecutePending()
    {
        while (_executionStack.Count > 0)
        {
            int ip = _executionStack.Pop();
            if (ip < 0 || ip >= Instructions.Length)
                continue;

            var result = Instructions[ip](this);
            if (result.Type == ExecutionResultType.Continue && result.NextIndex >= 0)
                _executionStack.Push(result.NextIndex);
            else if (result.Type == ExecutionResultType.Halt)
            {
                _executionStack.Clear();
                break;
            }
        }
    }

    /// <summary>
    /// Reads a value from a memory slot, with optional fallback and type conversion.
    /// </summary>
    /// <typeparam name="T">Expected type of the value.</typeparam>
    /// <param name="memoryId">Memory slot index.</param>
    /// <param name="fallback">Value to return if the slot is empty or invalid.</param>
    public T Read<T>(int memoryId, T fallback = default)
    {
        if (memoryId < 0 || memoryId >= _memorySize)
            return fallback;

        object val = _memory[memoryId];
        if (val is T t)
            return t;
        // Attempt conversion for numeric / compatible types.
        if (val is IConvertible)
        {
            try { return (T)Convert.ChangeType(val, typeof(T)); }
            catch { }
        }
        return fallback;
    }

    /// <summary>
    /// Writes a typed value to a memory slot.
    /// </summary>
    public void Write<T>(int memoryId, T value)
    {
        if (memoryId >= 0 && memoryId < _memorySize)
            _memory[memoryId] = value;
    }

    /// <summary>
    /// Writes an untyped object to a memory slot.
    /// </summary>
    public void Write(int memoryId, object value)
    {
        if (memoryId >= 0 && memoryId < _memorySize)
            _memory[memoryId] = value;
    }
}
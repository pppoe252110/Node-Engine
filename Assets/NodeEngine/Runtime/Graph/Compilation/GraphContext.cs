using System;
using System.Buffers;
using System.Collections.Generic;

public class GraphContext : IDisposable
{
    public Func<GraphContext, ExecutionResult>[] Instructions;
    public Dictionary<BaseNode, object> NodeState = new();

    private object[] _memory;
    private readonly int _memorySize;
    private Stack<int> _executionStack = new Stack<int>();

    // ArrayPool for reusing memory arrays across recompiles
    private static readonly ArrayPool<object> MemoryPool = ArrayPool<object>.Shared;

    private bool _disposed;

    public GraphContext(int memorySize)
    {
        _memorySize = memorySize;
        _memory = MemoryPool.Rent(memorySize);
        // Clear the rented segment (may contain old data)
        Array.Clear(_memory, 0, memorySize);
    }

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

    public void PushFlow(int index)
    {
        if (index >= 0 && index < Instructions.Length)
            _executionStack.Push(index);
    }

    public void ExecuteFlow(int startIndex)
    {
        _executionStack.Clear();
        _executionStack.Push(startIndex);
        ExecutePending();
    }

    public void ExecuteMultipleFlows(IEnumerable<int> startIndices)
    {
        _executionStack.Clear();
        foreach (int idx in startIndices)
            if (idx >= 0 && idx < Instructions.Length)
                _executionStack.Push(idx);
        ExecutePending();
    }

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

    public T Read<T>(int memoryId, T fallback = default)
    {
        if (memoryId < 0 || memoryId >= _memorySize)
            return fallback;

        object val = _memory[memoryId];
        if (val is T t)
            return t;
        if (val is IConvertible)
        {
            try { return (T)Convert.ChangeType(val, typeof(T)); }
            catch { }
        }
        return fallback;
    }

    public void Write<T>(int memoryId, T value)
    {
        if (memoryId >= 0 && memoryId < _memorySize)
            _memory[memoryId] = value;
    }

    public void Write(int memoryId, object value)
    {
        if (memoryId >= 0 && memoryId < _memorySize)
            _memory[memoryId] = value;
    }
}

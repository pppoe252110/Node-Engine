using System;
using System.Collections.Generic;
using UnityEngine;

public class GraphContext
{
    public Func<GraphContext, ExecutionResult>[] Instructions;
    public Dictionary<BaseNode, object> NodeState = new();

    private object[] _memory;
    private Stack<int> _executionStack = new Stack<int>();

    public GraphContext(int memorySize)
    {
        _memory = new object[memorySize];
    }

    /// <summary>
    /// Safely pushes a flow target onto the execution stack without clearing it.
    /// Use this inside node delegates to schedule multiple branches.
    /// </summary>
    public void PushFlow(int index)
    {
        if (index >= 0 && index < Instructions.Length)
            _executionStack.Push(index);
    }

    /// <summary>
    /// Starts execution from a specific node. Clears any pending execution first.
    /// </summary>
    public void ExecuteFlow(int startIndex)
    {
        _executionStack.Clear();
        _executionStack.Push(startIndex);

        while (_executionStack.Count > 0)
        {
            int ip = _executionStack.Pop();
            if (ip < 0 || ip >= Instructions.Length)
                continue;

            var result = Instructions[ip](this);

            if (result.Type == ExecutionResultType.Continue && result.NextIndex >= 0)
            {
                _executionStack.Push(result.NextIndex);
            }
            else if (result.Type == ExecutionResultType.Halt)
            {
                _executionStack.Clear();
                break;
            }
        }
    }

    public T Read<T>(int memoryId, T fallback = default)
    {
        if (memoryId >= 0 && memoryId < _memory.Length)
        {
            var val = _memory[memoryId];
            if (val is T castedVal) return castedVal;
            try { if (val is IConvertible) return (T)Convert.ChangeType(val, typeof(T)); } catch { }
        }
        return fallback;
    }

    public void Write(int memoryId, object value)
    {
        if (memoryId >= 0 && memoryId < _memory.Length)
            _memory[memoryId] = value;
    }
}

public enum ExecutionResultType
{
    Continue,
    Stop,
    Halt
}

public struct ExecutionResult
{
    public ExecutionResultType Type;
    public int NextIndex;

    public static ExecutionResult Continue(int nextIndex) => new()
    {
        Type = ExecutionResultType.Continue,
        NextIndex = nextIndex
    };

    public static ExecutionResult Stop() => new()
    {
        Type = ExecutionResultType.Stop
    };

    public static ExecutionResult Halt() => new()
    {
        Type = ExecutionResultType.Halt
    };
}
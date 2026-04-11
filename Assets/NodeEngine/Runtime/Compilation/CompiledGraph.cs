using System;
using System.Collections.Generic;

public class GraphContext
{
    public object[] Memory;
    public Func<GraphContext, ExecutionResult>[] Instructions;

    /// <summary>
    /// Executes the graph synchronously starting from the given instruction index.
    /// </summary>
    public void ExecuteFlow(int startIndex)
    {
        Stack<int> callStack = new Stack<int>();
        callStack.Push(startIndex);

        while (callStack.Count > 0)
        {
            int ip = callStack.Pop();
            if (ip < 0 || ip >= Instructions.Length)
                continue;

            var result = Instructions[ip](this);

            switch (result.Type)
            {
                case ExecutionResultType.Continue:
                    if (result.NextIndex >= 0)
                        callStack.Push(result.NextIndex);
                    break;

                case ExecutionResultType.Stop:
                    callStack.Clear();
                    break;
            }
        }
    }
}

public enum ExecutionResultType
{
    Continue,
    Stop
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
}
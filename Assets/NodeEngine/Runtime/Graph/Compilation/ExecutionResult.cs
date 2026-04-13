public enum ExecutionResultType { Continue, Stop, Halt }

public struct ExecutionResult
{
    public ExecutionResultType Type;
    public int NextIndex;

    public static ExecutionResult Continue(int next) => new() { Type = ExecutionResultType.Continue, NextIndex = next };
    public static ExecutionResult Stop() => new() { Type = ExecutionResultType.Stop };
    public static ExecutionResult Halt() => new() { Type = ExecutionResultType.Halt };
}
public static class NodeEngine
{
    public static  bool IsExecuting => _isExecuting;
    private static bool _isExecuting;

    public static void SetIsExecuting(bool isExecuting)
    {
        _isExecuting = isExecuting;
    }
}

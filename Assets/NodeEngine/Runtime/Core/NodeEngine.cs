namespace NodeEngine.Core
{
    public static class NodeEngine
    {
        private static bool _isExecuting;
        public static bool IsExecuting => _isExecuting;
        public static void SetIsExecuting(bool isExecuting) => _isExecuting = isExecuting;

        private static bool _isClearingGraph;
        public static bool IsClearingGraph => _isClearingGraph;
        public static void SetIsClearingGraph(bool value) => _isClearingGraph = value;
    }
}
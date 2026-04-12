using System;
using System.Collections.Generic;
using NodeEngine.Compilation;

public static class NodeCompiler
{
    private static readonly CompilationPipeline _pipeline = new();

    public static CompiledGraph Compile(
        List<BaseNode> nodes,
        List<DataConnection> dataConnections,
        List<FlowConnection> flowConnections)
    {
        return _pipeline.Compile(nodes, dataConnections, flowConnections);
    }

    public class CompiledGraph
    {
        public GraphContext Context { get; }
        public Dictionary<BaseNode, int> NodeToIndex { get; }

        // Store compilation contexts for external data injection
        private readonly Dictionary<BaseNode, NodeCompilationContext> _nodeContexts;

        public CompiledGraph(
            Func<GraphContext, ExecutionResult>[] instructions,
            int memorySize,
            Dictionary<BaseNode, int> nodeToIndex,
            Dictionary<BaseNode, NodeCompilationContext> nodeContexts)
        {
            Context = new GraphContext(memorySize)
            {
                Instructions = instructions
            };
            NodeToIndex = nodeToIndex;
            _nodeContexts = nodeContexts;
        }

        public void ExecuteNode(BaseNode node)
        {
            if (NodeToIndex.TryGetValue(node, out int idx))
                Context.ExecuteFlow(idx);
        }

        /// <summary>
        /// Sets external input values for a node before execution.
        /// </summary>
        public void SetNodeInputData(BaseNode node, Dictionary<string, object> inputData)
        {
            if (!_nodeContexts.TryGetValue(node, out var context))
                return;

            foreach (var kvp in inputData)
            {
                string portName = kvp.Key;
                object value = kvp.Value;

                // Find the port index
                if (!node.TryGetPortIndex(portName, isInput: true, out int portIdx))
                    continue;

                // Get memory index from context
                if (context.InputMemoryIndices == null || portIdx >= context.InputMemoryIndices.Length)
                    continue;

                int memIdx = context.InputMemoryIndices[portIdx];

                Context.Write(memIdx, value);
            }
        }
    }
}
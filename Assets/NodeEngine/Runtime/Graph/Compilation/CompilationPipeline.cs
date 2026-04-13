using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NodeEngine.Compilation
{
    public class CompilationPipeline
    {
        private readonly MemoryLayoutBuilder _memoryBuilder = new();
        private readonly FlowRouter _flowRouter = new();
        private readonly DependencyResolver _dependencyResolver = new();

        public NodeCompiler.CompiledGraph Compile(
            IReadOnlyList<BaseNode> nodes,
            IReadOnlyList<DataConnection> dataConnections,
            IReadOnlyList<FlowConnection> flowConnections)
        {
            if (nodes == null) throw new ArgumentNullException(nameof(nodes));
            if (dataConnections == null) throw new ArgumentNullException(nameof(dataConnections));
            if (flowConnections == null) throw new ArgumentNullException(nameof(flowConnections));

            // Filter out null entries
            nodes = nodes.Where(n => n != null).ToList();
            dataConnections = dataConnections.Where(c => c != null && c.SourceNode != null && c.TargetNode != null).ToList();
            flowConnections = flowConnections.Where(c => c != null && c.SourceNode != null && c.TargetNode != null).ToList();

            // Step 1: Assign runtime IDs
            var nodeToIndex = new Dictionary<BaseNode, int>();
            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].RuntimeId = i;
                nodeToIndex[nodes[i]] = i;
            }

            // Step 2: Build memory layout – now returns a result object
            var memoryLayout = _memoryBuilder.BuildMemoryLayout(nodes, dataConnections);
            int memorySize = memoryLayout.MemorySize;

            // Step 3: Route flow connections and get mapping
            var flowTargetsMap = _flowRouter.RouteFlows(nodes, flowConnections, nodeToIndex);

            // Step 4: Build compilation contexts and compile each node
            var contexts = new NodeCompilationContext[nodes.Count];
            var baseInstructions = new Func<GraphContext, ExecutionResult>[nodes.Count];

            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                var context = new NodeCompilationContext(node)
                {
                    InputMemoryIndices = memoryLayout.InputIndices[node],
                    OutputMemoryIndices = memoryLayout.OutputIndices[node],
                    FlowTargets = flowTargetsMap[node]
                };
                context.BuildLookups();
                contexts[i] = context;

                try
                {
                    baseInstructions[i] = node.Compile(context);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[CompilationPipeline] Failed to compile node {node.GetType().Name}: {ex.Message}");
                    baseInstructions[i] = _ => ExecutionResult.Stop();
                }
            }

            // Step 5: Wrap flow nodes with dependency pre-execution
            var instructions = new Func<GraphContext, ExecutionResult>[nodes.Count];
            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                int capturedIndex = i;
                var capturedNode = node;

                if (_dependencyResolver.IsFlowNode(node))
                {
                    var deps = _dependencyResolver.GetDataDependencies(node, dataConnections, nodeToIndex);
                    instructions[capturedIndex] = ctx =>
                    {
                        try
                        {
                            foreach (int depIdx in deps)
                                baseInstructions[depIdx](ctx);
                            return baseInstructions[capturedIndex](ctx);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"[CompilationPipeline] Runtime error in flow node '{capturedNode.GetType().Name}': {ex.Message} \n {ex.StackTrace}");
                            return ExecutionResult.Stop();
                        }
                    };
                }
                else
                {
                    instructions[capturedIndex] = ctx =>
                    {
                        try
                        {
                            return baseInstructions[capturedIndex](ctx);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"[CompilationPipeline] Runtime error in data node '{capturedNode.GetType().Name}': {ex.Message}");
                            return ExecutionResult.Stop();
                        }
                    };
                }
            }

            return new NodeCompiler.CompiledGraph(instructions, memorySize, nodeToIndex, contexts.ToDictionary(c => c.Node, c => c));
        }
    }
}
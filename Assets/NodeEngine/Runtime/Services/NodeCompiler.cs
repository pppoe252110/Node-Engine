using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class NodeCompiler
{
    public static CompiledGraph Compile(
        List<BaseNode> nodes,
        List<DataConnection> dataConnections,
        List<FlowConnection> flowConnections)
    {
        if (nodes == null) throw new ArgumentNullException(nameof(nodes));
        if (dataConnections == null) throw new ArgumentNullException(nameof(dataConnections));
        if (flowConnections == null) throw new ArgumentNullException(nameof(flowConnections));

        nodes = nodes.Where(n => n != null).ToList();
        dataConnections = dataConnections.Where(c => c != null && c.SourceNode != null && c.TargetNode != null).ToList();
        flowConnections = flowConnections.Where(c => c != null && c.SourceNode != null && c.TargetNode != null).ToList();

        // 1. Memory slots for output data ports
        var portToMem = new Dictionary<string, int>();
        int nextMem = 0;
        foreach (var node in nodes)
        {
            string nodeKey = !string.IsNullOrEmpty(node.NodeId) ? node.NodeId : node.GetHashCode().ToString();
            foreach (var port in node.Ports)
            {
                if (!port.IsFlow && !port.IsInput)
                {
                    string key = $"{nodeKey}_{port.Name}";
                    if (!portToMem.ContainsKey(key))
                        portToMem[key] = nextMem++;
                }
            }
        }

        // 2. Map input ports to the same slot
        foreach (var conn in dataConnections)
        {
            string sourceKey = $"{conn.SourceNode.NodeId}_{conn.OutputPortName}";
            string targetKey = $"{conn.TargetNode.NodeId}_{conn.InputPortName}";
            if (portToMem.TryGetValue(sourceKey, out int memIndex))
                portToMem[targetKey] = memIndex;
        }

        // 3. Fill InputMemoryIndices / OutputMemoryIndices
        foreach (var node in nodes)
        {
            string nodeKey = !string.IsNullOrEmpty(node.NodeId) ? node.NodeId : node.GetHashCode().ToString();
            int[] inputs = node.Ports
                .Where(p => p.IsInput && !p.IsFlow)
                .Select(p => portToMem.TryGetValue($"{nodeKey}_{p.Name}", out int idx) ? idx : -1)
                .ToArray();
            int[] outputs = node.Ports
                .Where(p => !p.IsInput && !p.IsFlow)
                .Select(p => portToMem.TryGetValue($"{nodeKey}_{p.Name}", out int idx) ? idx : -1)
                .ToArray();
            node.SetMemoryIndices(inputs, outputs);
        }

        // 4. Map each node to an instruction index
        var nodeToIndex = new Dictionary<BaseNode, int>();
        for (int i = 0; i < nodes.Count; i++)
        {
            nodes[i].RuntimeId = i;
            nodeToIndex[nodes[i]] = i;
        }

        // 5. Resolve flow routing
        foreach (var node in nodes)
        {
            var targets = new Dictionary<string, int>();
            foreach (var conn in flowConnections.Where(c => c.SourceNode == node))
            {
                if (nodeToIndex.TryGetValue(conn.TargetNode, out int targetIdx))
                    targets[conn.SourcePortName] = targetIdx;
            }
            node.SetFlowTargets(targets);
        }

        // 6. Compile base instructions (sync)
        var baseInstructions = new Func<GraphContext, ExecutionResult>[nodes.Count];
        for (int i = 0; i < nodes.Count; i++)
        {
            try
            {
                baseInstructions[i] = nodes[i].Compile();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[NodeCompiler] Failed to compile node {nodes[i].GetType().Name}: {ex.Message}");
                baseInstructions[i] = (ctx) => ExecutionResult.Stop();
            }
        }

        // 7. Wrap with data-dependency pre‑execution
        var instructions = new Func<GraphContext, ExecutionResult>[nodes.Count];
        for (int i = 0; i < nodes.Count; i++)
        {
            var node = nodes[i];
            int capturedIndex = i;
            var capturedNode = node;

            if (IsFlowNode(node))
            {
                List<int> deps = GetDataDependencies(node, dataConnections, nodeToIndex);

                instructions[capturedIndex] = (ctx) =>
                {
                    try
                    {
                        foreach (var depIdx in deps)
                            baseInstructions[depIdx](ctx);
                        return baseInstructions[capturedIndex](ctx);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[NodeCompiler] Runtime error in flow node '{capturedNode.GetType().Name}': {ex.Message}");
                        return ExecutionResult.Stop();
                    }
                };
            }
            else
            {
                instructions[capturedIndex] = (ctx) =>
                {
                    try
                    {
                        return baseInstructions[capturedIndex](ctx);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[NodeCompiler] Runtime error in data node '{capturedNode.GetType().Name}': {ex.Message}");
                        return ExecutionResult.Stop();
                    }
                };
            }
        }

        return new CompiledGraph(instructions, nextMem, nodeToIndex);
    }

    private static bool IsFlowNode(BaseNode node) => node?.Ports?.Any(p => p.IsFlow) ?? false;

    private static List<int> GetDataDependencies(BaseNode target, List<DataConnection> dataConnections, Dictionary<BaseNode, int> nodeToIndex)
    {
        var deps = new List<int>();
        var visited = new HashSet<BaseNode>();
        var recursionStack = new HashSet<BaseNode>();

        void Traverse(BaseNode n)
        {
            if (n == null) return;
            if (recursionStack.Contains(n))
            {
                Debug.LogError($"[NodeCompiler] Cycle detected involving {n.GetType().Name}");
                return;
            }
            if (visited.Contains(n)) return;

            visited.Add(n);
            recursionStack.Add(n);

            var inputs = dataConnections.Where(c => c.TargetNode == n).ToList();
            foreach (var input in inputs)
            {
                var source = input.SourceNode;
                Traverse(source);
                if (nodeToIndex.TryGetValue(source, out int srcIdx) && !deps.Contains(srcIdx))
                    deps.Add(srcIdx);
            }

            recursionStack.Remove(n);
        }

        Traverse(target);
        if (nodeToIndex.TryGetValue(target, out int targetIdx))
            deps.Remove(targetIdx);
        return deps;
    }

    public class CompiledGraph
    {
        public GraphContext Context;
        public Dictionary<BaseNode, int> NodeToIndex;

        public CompiledGraph(Func<GraphContext, ExecutionResult>[] instructions, int memorySize, Dictionary<BaseNode, int> nodeToIndex)
        {
            Context = new GraphContext
            {
                Memory = new object[memorySize],
                Instructions = instructions
            };
            NodeToIndex = nodeToIndex;
        }

        public void ExecuteNode(BaseNode node)
        {
            if (NodeToIndex.TryGetValue(node, out int idx))
                Context.ExecuteFlow(idx);
        }
    }
}
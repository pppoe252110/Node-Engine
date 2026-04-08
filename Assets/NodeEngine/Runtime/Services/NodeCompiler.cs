// ===== F:\Unity\Node Engine\Assets\NodeEngine\Runtime\Services\NodeCompiler.cs =====
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class NodeCompiler
{
    /// <summary>
    /// Compiles a set of nodes and connections into an executable graph.
    /// </summary>
    public static CompiledGraph Compile(
        List<BaseNode> nodes,
        List<DataConnection> dataConnections,
        List<FlowConnection> flowConnections)
    {
        if (nodes == null) throw new ArgumentNullException(nameof(nodes));
        if (dataConnections == null) throw new ArgumentNullException(nameof(dataConnections));
        if (flowConnections == null) throw new ArgumentNullException(nameof(flowConnections));

        // Filter out any null entries that might have crept in
        nodes = nodes.Where(n => n != null).ToList();
        dataConnections = dataConnections.Where(c => c != null && c.SourceNode != null && c.TargetNode != null).ToList();
        flowConnections = flowConnections.Where(c => c != null && c.SourceNode != null && c.TargetNode != null).ToList();

        Debug.Log($"[NodeCompiler] Compiling {nodes.Count} nodes, {dataConnections.Count} data edges, {flowConnections.Count} flow edges.");

        // --------------------------------------------------------------------
        // 1. Assign memory slots to output data ports using stable NodeId.
        // --------------------------------------------------------------------
        var portToMem = new Dictionary<string, int>();
        int nextMem = 0;

        foreach (var node in nodes)
        {
            string nodeKey = !string.IsNullOrEmpty(node.NodeId) ? node.NodeId : node.GetHashCode().ToString();

            foreach (var port in node.Ports)
            {
                if (!port.IsFlow && !port.IsInput) // output data port
                {
                    string key = $"{nodeKey}_{port.Name}";
                    if (!portToMem.ContainsKey(key))
                        portToMem[key] = nextMem++;
                }
            }
        }

        // --------------------------------------------------------------------
        // 2. Map input ports to the same memory slot as their connected output.
        // --------------------------------------------------------------------
        foreach (var conn in dataConnections)
        {
            string sourceKey = $"{conn.SourceNode.NodeId}_{conn.OutputPortName}";
            string targetKey = $"{conn.TargetNode.NodeId}_{conn.InputPortName}";

            if (portToMem.TryGetValue(sourceKey, out int memIndex))
            {
                portToMem[targetKey] = memIndex;
            }
            else
            {
                Debug.LogWarning($"[NodeCompiler] Data connection {conn.SourceNode.GetType().Name}.{conn.OutputPortName} -> {conn.TargetNode.GetType().Name}.{conn.InputPortName} has no memory slot.");
            }
        }

        // --------------------------------------------------------------------
        // 3. Fill InputMemoryIndices / OutputMemoryIndices for every node.
        // --------------------------------------------------------------------
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

        // --------------------------------------------------------------------
        // 4. Map each node to an instruction index.
        // --------------------------------------------------------------------
        var nodeToIndex = new Dictionary<BaseNode, int>();
        for (int i = 0; i < nodes.Count; i++)
            nodeToIndex[nodes[i]] = i;

        // --------------------------------------------------------------------
        // 5. Resolve flow routing (call AssignFlowIndices on each node).
        // --------------------------------------------------------------------
        foreach (var node in nodes)
        {
            var targets = new Dictionary<string, int>();
            foreach (var conn in flowConnections.Where(c => c.SourceNode == node))
            {
                if (nodeToIndex.TryGetValue(conn.TargetNode, out int targetIdx))
                    targets[conn.SourcePortName] = targetIdx;
                else
                    Debug.LogWarning($"[NodeCompiler] Flow target {conn.TargetNode?.GetType().Name} not found for {node.GetType().Name}.{conn.SourcePortName}");
            }
            node.AssignFlowIndices(targets);
        }

        // --------------------------------------------------------------------
        // 6. Compile base instructions (one delegate per node).
        // --------------------------------------------------------------------
        var baseInstructions = new Func<GraphContext, int>[nodes.Count];
        for (int i = 0; i < nodes.Count; i++)
        {
            try
            {
                baseInstructions[i] = nodes[i].Compile();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[NodeCompiler] Failed to compile node {nodes[i].GetType().Name} (ID: {nodes[i].NodeId}): {ex.Message}");
                baseInstructions[i] = (ctx) => -1; // fallback no-op
            }
        }

        // --------------------------------------------------------------------
        // 7. Wrap instructions with data-dependency pre‑execution and error handling.
        // --------------------------------------------------------------------
        var instructions = new Func<GraphContext, int>[nodes.Count];

        for (int i = 0; i < nodes.Count; i++)
        {
            var node = nodes[i];
            int capturedIndex = i;
            var capturedNode = node;

            if (IsFlowNode(node))
            {
                // Pre‑compute data dependencies (with cycle detection)
                List<int> deps;
                try
                {
                    deps = GetDataDependencies(node, dataConnections, nodeToIndex);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[NodeCompiler] Dependency error for {node.GetType().Name}: {ex.Message}");
                    deps = new List<int>();
                }

                instructions[capturedIndex] = (ctx) =>
                {
                    try
                    {
                        // Execute dependent data nodes
                        foreach (var depIdx in deps)
                            baseInstructions[depIdx](ctx);

                        return baseInstructions[capturedIndex](ctx);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[NodeCompiler] Runtime error in flow node '{capturedNode.GetType().Name}': {ex.Message}");
                        return -1; // Halt execution on error
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
                        return -1;
                    }
                };
            }
        }

        Debug.Log($"[NodeCompiler] Compilation finished. Memory slots: {nextMem}");
        return new CompiledGraph(instructions, nextMem, nodeToIndex);
    }

    private static bool IsFlowNode(BaseNode node)
    {
        return node?.Ports?.Any(p => p.IsFlow) ?? false;
    }

    /// <summary>
    /// Returns indices of all data nodes that must be executed before the target flow node.
    /// Includes cycle detection to prevent infinite recursion.
    /// </summary>
    private static List<int> GetDataDependencies(
        BaseNode target,
        List<DataConnection> dataConnections,
        Dictionary<BaseNode, int> nodeToIndex)
    {
        var deps = new List<int>();
        var visited = new HashSet<BaseNode>();
        var recursionStack = new HashSet<BaseNode>(); // Cycle detection

        void Traverse(BaseNode n)
        {
            if (n == null) return;
            if (recursionStack.Contains(n))
            {
                Debug.LogError($"[NodeCompiler] Cycle detected in data dependencies involving node {n.GetType().Name}");
                return; // Stop recursion on cycle
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

        // Remove the target itself if accidentally added
        if (nodeToIndex.TryGetValue(target, out int targetIdx))
            deps.Remove(targetIdx);

        return deps;
    }
}
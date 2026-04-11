using System.Collections.Generic;
using System.Linq;
using System;

namespace NodeEngine.Compilation
{
    /// <summary>
    /// Result of memory layout building: total memory size and per-node memory indices.
    /// </summary>
    public class MemoryLayoutResult
    {
        public int MemorySize { get; set; }
        public Dictionary<BaseNode, int[]> InputIndices { get; } = new();
        public Dictionary<BaseNode, int[]> OutputIndices { get; } = new();
    }

    public class MemoryLayoutBuilder
    {
        private readonly Dictionary<(BaseNode node, string portName), int> _portToMemoryIndex = new();
        private int _nextMemorySlot;

        public MemoryLayoutResult BuildMemoryLayout(
            List<BaseNode> nodes,
            List<DataConnection> dataConnections)
        {
            var result = new MemoryLayoutResult();
            _portToMemoryIndex.Clear();
            _nextMemorySlot = 0;

            // Assign memory slots for all output data ports
            foreach (var node in nodes)
            {
                // Guard: node must have a valid ID
                if (string.IsNullOrEmpty(node.NodeId))
                    throw new InvalidOperationException($"Node {node.GetType().Name} has no NodeId");

                foreach (var port in node.Ports.Where(p => !p.IsFlow && !p.IsInput))
                {
                    var key = (node, port.Name);
                    if (!_portToMemoryIndex.ContainsKey(key))
                        _portToMemoryIndex[key] = _nextMemorySlot++;
                }
            }

            // Map input ports to the same slot as connected output ports
            foreach (var conn in dataConnections)
            {
                var sourceKey = (conn.SourceNode, conn.OutputPortName);
                var targetKey = (conn.TargetNode, conn.InputPortName);
                if (_portToMemoryIndex.TryGetValue(sourceKey, out int memIndex))
                    _portToMemoryIndex[targetKey] = memIndex;
            }

            // Build per-node index arrays
            foreach (var node in nodes)
            {
                var inputs = node.Ports
                    .Where(p => p.IsInput && !p.IsFlow)
                    .Select(p => _portToMemoryIndex.TryGetValue((node, p.Name), out int idx) ? idx : -1)
                    .ToArray();
                var outputs = node.Ports
                    .Where(p => !p.IsInput && !p.IsFlow)
                    .Select(p => _portToMemoryIndex.TryGetValue((node, p.Name), out int idx) ? idx : -1)
                    .ToArray();

                result.InputIndices[node] = inputs;
                result.OutputIndices[node] = outputs;
            }

            result.MemorySize = _nextMemorySlot;
            return result;
        }
    }
}
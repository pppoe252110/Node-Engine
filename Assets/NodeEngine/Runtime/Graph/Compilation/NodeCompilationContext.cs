using System.Collections.Generic;

namespace NodeEngine.Compilation
{
    /// <summary>
    /// Holds per-node data produced during compilation (memory layout, flow routing).
    /// This separates the pure node definition from runtime execution details.
    /// </summary>
    public class NodeCompilationContext
    {
        public BaseNode Node { get; }

        // Memory indices for data ports (index in GraphContext.Memory)
        public int[] InputMemoryIndices { get; set; }
        public int[] OutputMemoryIndices { get; set; }

        // Flow targets: output flow port name -> node instruction index
        public Dictionary<string, int> FlowTargets { get; set; } = new();

        // Convenience maps for quick lookup by port name
        private Dictionary<string, int> _inputMap;
        private Dictionary<string, int> _outputMap;

        public NodeCompilationContext(BaseNode node)
        {
            Node = node;
        }

        /// <summary>
        /// Called after indices are assigned to build fast lookup dictionaries.
        /// </summary>
        public void BuildLookups()
        {
            _inputMap = new Dictionary<string, int>();
            _outputMap = new Dictionary<string, int>();

            var ports = Node.Ports;
            int inIdx = 0, outIdx = 0;
            foreach (var port in ports)
            {
                if (port.IsFlow) continue;
                if (port.IsInput)
                {
                    if (InputMemoryIndices != null && inIdx < InputMemoryIndices.Length)
                        _inputMap[port.Name] = InputMemoryIndices[inIdx++];
                }
                else
                {
                    if (OutputMemoryIndices != null && outIdx < OutputMemoryIndices.Length)
                        _outputMap[port.Name] = OutputMemoryIndices[outIdx++];
                }
            }
        }

        public int GetInputId(string portName) =>
            _inputMap != null && _inputMap.TryGetValue(portName, out int id) ? id : -1;

        public int GetOutputId(string portName) =>
            _outputMap != null && _outputMap.TryGetValue(portName, out int id) ? id : -1;

        public int GetFlowId(string portName) =>
            FlowTargets != null && FlowTargets.TryGetValue(portName, out int id) ? id : -1;
    }
}
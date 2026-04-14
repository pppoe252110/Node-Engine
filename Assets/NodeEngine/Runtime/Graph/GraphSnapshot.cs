using System;
using System.Collections.Generic;
using UnityEngine;

namespace NodeEngine.GraphPersistence
{
    /// <summary>
    /// Represents a complete snapshot of a graph, ready for serialization.
    /// </summary>
    [Serializable]
    public class GraphSnapshot
    {
        public string version;
        public List<NodeData> nodes = new();
        public List<ConnectionData> connections = new();

        [Serializable]
        public class NodeData
        {
            public string nodeId;
            public string nodeType;
            public Vector2 position;
            public string serializedValue;
        }

        [Serializable]
        public class ConnectionData
        {
            public string sourceNodeId;
            public string sourcePortName;
            public string targetNodeId;
            public string targetPortName;
            public bool isFlow;
        }
    }
}
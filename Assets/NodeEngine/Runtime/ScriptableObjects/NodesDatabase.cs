using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

[CreateAssetMenu(fileName = "NodesDatabase", menuName = "Node Engine/NodesDatabase", order = 1)]
public class NodesDatabase : ScriptableObject
{
    [SerializeField] private SerializableNode[] _serializableNodes = Array.Empty<SerializableNode>();

    /// <summary>Returns all serialized node metadata.</summary>
    public IEnumerable<SerializableNode> GetAllNodeData() => _serializableNodes;

    /// <summary>Applies the stored name and icon to a node instance.</summary>
    public void ApplyMetadata(BaseNode node)
    {
        var typeName = node.GetType().AssemblyQualifiedName;
        var data = _serializableNodes.FirstOrDefault(n => n.nodeType == typeName);
        if (data != null)
        {
            node.SetName(data.nodeName);
            node.SetIcon(data.nodeIcon);
        }
    }

    [Button("Auto fill", "AutoFill")]
    public void AutoFill()
    {
        var subclassTypes = Assembly.GetAssembly(typeof(BaseNode))
            .GetTypes()
            .Where(t => t.IsSubclassOf(typeof(BaseNode)) && !t.IsAbstract)
            .ToArray();

        var existingNodes = _serializableNodes?.ToList() ?? new List<SerializableNode>();

        foreach (var type in subclassTypes)
        {
            if (!existingNodes.Any(n => n?.nodeType == type.AssemblyQualifiedName))
            {
                var newNode = new SerializableNode();
                newNode.Initialize(type, type.Name.Replace("Node", ""));
                existingNodes.Add(newNode);
            }
        }

        _serializableNodes = existingNodes.ToArray();
    }
}
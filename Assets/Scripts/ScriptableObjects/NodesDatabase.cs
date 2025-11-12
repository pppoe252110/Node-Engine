using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

[CreateAssetMenu(fileName = "NodesDatabase", menuName = "ScriptableObjects/NodesDatabase", order = 1)]
public class NodesDatabase : ScriptableObject
{
    [SerializeField] private SerializableNode[] _serializableNodes = new SerializableNode[0];

    public NodeBase[] GetNodes()
    {
        if (_serializableNodes == null) return new NodeBase[0];
        var result = new NodeBase[_serializableNodes.Length];
        for (int i = 0; i < _serializableNodes.Length; i++)
        {
            result[i] = _serializableNodes[i].CreateInstance();
            result[i].SetName(_serializableNodes[i].nodeName);
            result[i].SetIcon(_serializableNodes[i].nodeIcon);
        }
        return result;
    }

    [Button("Auto fill", "AutoFill")]
    public void AutoFill()
    {
        var subclassTypes = Assembly
           .GetAssembly(typeof(NodeBase))
           .GetTypes()
           .Where(t => t.IsSubclassOf(typeof(NodeBase)) && !t.IsAbstract)
           .ToArray();

        // Convert existing array to a list for easier manipulation
        var existingNodes = _serializableNodes?.ToList() ?? new System.Collections.Generic.List<SerializableNode>();

        foreach (var type in subclassTypes)
        {
            // Check if a node with this type already exists
            bool typeAlreadyExists = existingNodes.Any(node =>
                node != null &&
                node.nodeType != null &&
                node.nodeType == type.AssemblyQualifiedName);

            if (!typeAlreadyExists)
            {
                // Add the new type if it doesn't exist
                var newNode = new SerializableNode();
                newNode.Initialize(type, type.Name.Replace("Node", ""));
                existingNodes.Add(newNode);
            }
        }

        // Convert back to array
        _serializableNodes = existingNodes.ToArray();
    }
}
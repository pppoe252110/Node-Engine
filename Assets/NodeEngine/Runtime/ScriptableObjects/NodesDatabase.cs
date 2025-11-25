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
            
            var nodeType = Type.GetType(_serializableNodes[i].nodeType);
            if (nodeType != null)
            {
                result[i] = Activator.CreateInstance(nodeType) as NodeBase;
                result[i].SetName(_serializableNodes[i].nodeName);
                result[i].SetIcon(_serializableNodes[i].nodeIcon);
            }
            else
            {
                Debug.LogError($"Could not create node type: {_serializableNodes[i].nodeType}");
            }
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

        
        var existingNodes = _serializableNodes?.ToList() ?? new System.Collections.Generic.List<SerializableNode>();

        foreach (var type in subclassTypes)
        {
            
            bool typeAlreadyExists = existingNodes.Any(node =>
                node != null &&
                node.nodeType != null &&
                node.nodeType == type.AssemblyQualifiedName);

            if (!typeAlreadyExists)
            {
                
                var newNode = new SerializableNode();
                newNode.Initialize(type, type.Name.Replace("Node", ""));
                existingNodes.Add(newNode);
            }
        }

        
        _serializableNodes = existingNodes.ToArray();
    }
}
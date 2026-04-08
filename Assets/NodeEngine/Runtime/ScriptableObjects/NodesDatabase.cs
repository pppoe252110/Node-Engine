using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

[CreateAssetMenu(fileName = "NodesDatabase", menuName = "Node Engine/NodesDatabase", order = 1)]
public class NodesDatabase : ScriptableObject
{
    [SerializeField] private SerializableNode[] _serializableNodes = new SerializableNode[0];

    public BaseNode[] GetNodes()
    {
        if (_serializableNodes == null) return new BaseNode[0];
        var result = new BaseNode[_serializableNodes.Length];
        for (int i = 0; i < _serializableNodes.Length; i++)
        {

            var nodeType = Type.GetType(_serializableNodes[i].nodeType);
            if (nodeType != null)
            {
                result[i] = Activator.CreateInstance(nodeType) as BaseNode;
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

    public BaseNode GetClone(BaseNode node)
    {
        var clone = Activator.CreateInstance(node.GetType()) as BaseNode;

        var setupNode = _serializableNodes.FirstOrDefault(s => s.nodeType == node.GetType().AssemblyQualifiedName);
        if (setupNode != null)
        {
            clone.SetName(setupNode.nodeName);
            clone.SetIcon(setupNode.nodeIcon);
        }

        return clone;
    }

    [Button("Auto fill", "AutoFill")]
    public void AutoFill()
    {
        var subclassTypes = Assembly
           .GetAssembly(typeof(BaseNode))
           .GetTypes()
           .Where(t => t.IsSubclassOf(typeof(BaseNode)) && !t.IsAbstract)
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

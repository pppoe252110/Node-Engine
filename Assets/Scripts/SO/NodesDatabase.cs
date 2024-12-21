using Sirenix.OdinInspector;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

[CreateAssetMenu(fileName = "NodesDatabase", menuName = "ScriptableObjects/NodesDatabase", order = 1)]
public class NodesDatabase : SerializedScriptableObject
{
    public NodeBase[] nodes;

    [Button("Auto search")]
    private void Reset()
    {
        var subclassTypes = Assembly
           .GetAssembly(typeof(NodeBase))
           .GetTypes()
           .Where(t => t.IsSubclassOf(typeof(NodeBase)));
        nodes = new NodeBase[subclassTypes.Count()];
        for (int i = 0; i < subclassTypes.Count(); i++)
        {
            nodes[i] = (NodeBase)Activator.CreateInstance(subclassTypes.ElementAt(i));
            nodes[i].SetName(nodes[i].ToString().Replace("Node", ""));
        }
    }
}

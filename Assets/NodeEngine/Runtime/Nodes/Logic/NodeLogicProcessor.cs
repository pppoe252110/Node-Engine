using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NodeLogicProcessor : MonoBehaviour
{
    public static NodeLogicProcessor Instance
    {
        get
        {
            if (instance == null)
                instance = FindFirstObjectByType<NodeLogicProcessor>();
            return instance;
        }
    }
    private static NodeLogicProcessor instance;

    public List<NodeLogic> nodes => _nodes;

    private List<NodeLogic> _nodes = new();

    public void AddNode(NodeLogic node)
    {
        if (!_nodes.Contains(node))
            _nodes.Add(node);
    }

    public void RemoveNode(NodeLogic node)
    {
        _nodes.Remove(node);
    }

    public void Process()
    {
        var node = _nodes.FirstOrDefault(s => s.Node is UpdateNode);
        if (node)
        {
            node.Process();
        }
    }
}
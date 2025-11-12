using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class NodeBase : INode, ICloneable
{
    public string NodeName => _nodeName;
    public Sprite NodeSprite => _nodeIcon;

    [SerializeField] private string _nodeName = "Basic";
    [SerializeField] private Sprite _nodeIcon;

    public List<NodeFieldBase> inputFields = new();
    public List<Connector> inputConnectors = new();

    public List<NodeFieldBase> outputFields = new();
    public List<Connector> outputConnectors = new();

    [System.NonSerialized]
    private int _guid = 0;

    private static HashSet<NodeBase> _processingNodes = new HashSet<NodeBase>();

    public abstract void Setup();

    public void Initialize(NodeLogic nodeLogic, int guid)
    {
        _guid = guid;
        Setup();
    }

    internal void SetName(string name)
    {
        _nodeName = name;
    }
    internal void SetIcon(Sprite icon)
    {
        _nodeIcon = icon;
    }

    public virtual void Process(List<Connector> fromConnectors = null)
    {
        if (fromConnectors == null) fromConnectors = new List<Connector>();
        // Cycle detection: Skip if already processing this node
        if (_processingNodes.Contains(this)) return;
        _processingNodes.Add(this);
        try
        {
            // Process inputs first (upstream dependencies)
            foreach (var connector in inputConnectors)
            {
                if (!fromConnectors.Contains(connector))
                {
                    connector.Field.ProceedValue();
                    fromConnectors.Add(connector);
                    // Recurse to connected upstream nodes (though inputs are usually from outputs of others, this ensures full traversal)
                    foreach (var connected in connector.Connections)
                    {
                        connected.Node.Process(fromConnectors);
                    }
                }
            }

            // Process outputs (downstream)
            foreach (var connector in outputConnectors)
            {
                if (!fromConnectors.Contains(connector) && connector.ConnectionsCount > 0)
                {
                    connector.Field.ProceedValue();
                    fromConnectors.Add(connector);
                    // Recurse to connected downstream nodes (this builds the "tree" branches)
                    foreach (var connected in connector.Connections)
                    {
                        connected.Node.Process(fromConnectors);
                    }
                }
            }
            // Execute self (if applicable)
            if (this is ExecutableNode executableNode)
            {
                executableNode.Execute();
            }
        }
        finally
        {
            _processingNodes.Remove(this);  // Always remove after processing
        }
    }

    public object Clone()
    {
        var clone = MemberwiseClone() as NodeBase;

        clone.inputFields = new List<NodeFieldBase>();
        clone.inputConnectors = new List<Connector>();
        clone.outputFields = new List<NodeFieldBase>();
        clone.outputConnectors = new List<Connector>();

        return clone;
    }
}
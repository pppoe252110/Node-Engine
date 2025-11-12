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

    // Add processing tracking
    public bool IsProcessing { get; set; }

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
        if (_processingNodes.Contains(this) || IsProcessing)
        {
            return;
        }

        _processingNodes.Add(this);
        IsProcessing = true;

        try
        {
            // Process inputs: Update data values from connected outputs (passive propagation)
            foreach (var connector in inputConnectors)
            {
                if (!fromConnectors.Contains(connector) && connector.ValueType != typeof(void))  // Only data inputs
                {
                    connector.Field.ProceedValue();
                    fromConnectors.Add(connector);

                    // Recurse to connected upstream nodes for value updates
                    foreach (var connected in connector.Connections)
                    {
                        connected.Node.Process(fromConnectors);
                    }
                }
            }

            // Process data outputs to ensure they have current values
            foreach (var connector in outputConnectors)
            {
                if (connector.ValueType != typeof(void))  // Data outputs
                {
                    connector.Field.ProceedValue();
                }
            }

            // Skip ALL void event propagation for ExecutableNode to prevent re-triggering execution or extra calls
            if (this is ExecutableNode) return;

            // Propagate void events from outputs (fire downstream)
            foreach (var connector in outputConnectors)
            {
                if (connector.ValueType == typeof(void))
                {
                    connector.Field.ProceedValue();
                }
            }
        }
        finally
        {
            _processingNodes.Remove(this);
            IsProcessing = false;
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
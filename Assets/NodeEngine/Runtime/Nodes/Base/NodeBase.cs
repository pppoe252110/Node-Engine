using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public abstract class NodeBase : INode
{
    public int Guid => _guid;
    public string NodeName => _nodeName;
    public Sprite NodeSprite => _nodeIcon;

    [SerializeField] private string _nodeName = "Basic";
    [SerializeField] private Sprite _nodeIcon;

    public List<NodeFieldBase> inputFields;
    public List<Connector> inputConnectors;
    public List<NodeFieldBase> outputFields;
    public List<Connector> outputConnectors;

    [SerializeField]
    private int _guid = 0;

    public bool IsProcessing { get; set; }

    private static HashSet<NodeBase> _processingNodes = new HashSet<NodeBase>();

    public abstract void Setup();

    public void Initialize(NodeLogic nodeLogic, int guid)
    {
        inputFields = new();
        inputConnectors = new();
        outputFields = new();
        outputConnectors = new();
        _guid = guid;
        Setup();
        Initialized();
    }

    protected virtual void Initialized() { }

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

        
        if (_processingNodes.Contains(this) || IsProcessing)
        {
            return;
        }

        _processingNodes.Add(this);
        IsProcessing = true;

        try
        {
            
            foreach (var connector in inputConnectors)
            {
                if (!fromConnectors.Contains(connector) && connector.ValueType != typeof(void))  
                {
                    connector.Field.ProceedValue();
                    fromConnectors.Add(connector);

                    
                    foreach (var connected in connector.Connections)
                    {
                        connected.Node.Process(fromConnectors);
                    }
                }
            }

            
            foreach (var connector in outputConnectors)
            {
                if (connector.ValueType != typeof(void))  
                {
                    Debug.LogError("DADAD");
                    connector.Field.ProceedValue();
                }
            }

            
            if (this is ExecutableNodeBase) return;

            
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
}
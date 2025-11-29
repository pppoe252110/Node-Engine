using System;
using System.Collections.Generic;
using System.Linq;
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

    protected static HashSet<NodeBase> _processingNodes = new HashSet<NodeBase>();

    public abstract void Setup();

    public void Initialize(NodeLogic nodeLogic, int guid)
    {
        inputFields = new();
        inputConnectors = new();
        outputFields = new();
        outputConnectors = new();
        _guid = guid;
        Setup();
        // FIXED: Removed InitializeConnectorValues() call here (moved to NodeLogic after connectors are set)
        Initialized();
    }

    // FIXED: Made public so it can be called from NodeLogic after connectors are set
    public void InitializeConnectorValues()
    {
        // Initialize input connectors
        foreach (var field in inputFields)
        {
            if (field.Connector != null && field.Connector.GetConnectorValue() == null)
            {
                var attribute = field.GetAttribute();
                if (attribute != null)
                {
                    var connectorValue = CreateConnectorValue(attribute.type);
                    field.Connector.SetConnectorValue(connectorValue);
                }
            }
        }

        // Initialize output connectors
        foreach (var field in outputFields)
        {
            if (field.Connector != null && field.Connector.GetConnectorValue() == null)
            {
                var attribute = field.GetAttribute();
                if (attribute != null)
                {
                    var connectorValue = CreateConnectorValue(attribute.type);
                    field.Connector.SetConnectorValue(connectorValue);
                }
            }
        }
    }

    // FIXED: Added helper method (copied from full codebase)
    private IConnectorValue CreateConnectorValue(Type type)
    {
        if (type == typeof(int))
            return new ConnectorValueInt(0);
        if (type == typeof(float))
            return new ConnectorValueFloat(0f);
        if (type == typeof(bool))
            return new ConnectorValueBool(false);
        if (type == typeof(string))
            return new ConnectorValueString("");
        if (type == typeof(Vector3))
            return new ConnectorValueVector3(Vector3.zero);
        if (type == typeof(void))
            return ConnectorValueVoid.Instance;

        // Default to object
        return new ConnectorValueObject(null);
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
            // Process non-void inputs
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

            // Process non-void outputs
            foreach (var connector in outputConnectors)
            {
                if (connector.ValueType != typeof(void))
                {
                    connector.Field.ProceedValue();
                }
            }

            // Skip execution for ExecutableNodeBase
            if (this is ExecutableNodeBase) return;

            // Process void outputs
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
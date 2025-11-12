using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

[Serializable]
public abstract class NodeBase : ICloneable
{
    public string NodeName => _nodeName;
    public Sprite NodeSprite => _nodeIcon;
    private bool isInPlaymode => Application.isPlaying;

    [SerializeField] private string _nodeName = "Basic";
    [SerializeField] private Sprite _nodeIcon;

    [ShowIf("isInPlaymode")]
    public List<NodeFieldBase> inputFields = new();
    [ShowIf("isInPlaymode")]
    public List<Connector> inputConnectors = new();

    [ShowIf("isInPlaymode")]
    public List<NodeFieldBase> outputFields = new();
    [ShowIf("isInPlaymode")]
    public List<Connector> outputConnectors = new();

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


    public virtual void Process(List<Connector> fromConnectors = null)
    {
        if (_processingNodes.Contains(this)) return;  // Prevent infinite recursion in cycles

        _processingNodes.Add(this);

        if (fromConnectors == null)
            fromConnectors = new List<Connector>();

        // Process inputs first to set local variables
        for (int i = 0; i < inputConnectors.Count; i++)
        {
            if (!fromConnectors.Contains(inputConnectors[i]))
            {
                inputConnectors[i].Field.ProceedValue();
                inputConnectors[i].Process(fromConnectors);
            }
        }
        // Process only outputs that have connections
        for (int i = 0; i < outputConnectors.Count; i++)
        {
            if (!fromConnectors.Contains(outputConnectors[i]) && outputConnectors[i].ConnectionsCount > 0)
            {
                outputConnectors[i].Field.ProceedValue();
                outputConnectors[i].Process(fromConnectors);
            }
        }
        if (this is ExecutableNode executableNode)
        {
            executableNode.Execute();
        }
        _processingNodes.Remove(this);  // Remove after processing
    }


    public object Clone()
    {
        var clone = MemberwiseClone() as NodeBase;

        clone.inputFields = new();
        clone.inputConnectors = new();
        clone.outputFields = new();
        clone.outputConnectors = new();

        return clone;
    }
}

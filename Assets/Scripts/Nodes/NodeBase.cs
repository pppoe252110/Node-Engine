using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
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
    public List<NodeFieldBase> outputFields = new();

    private int _guid = 0;

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

    public object Clone()
    {
        return MemberwiseClone();
    }
}

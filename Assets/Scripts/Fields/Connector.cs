using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static NodeValueAttribute;
using static UnityEngine.Rendering.DebugUI;

public class Connector : MonoBehaviour
{
    public NodeBase Node => _node;
    public Type ValueType => _valueAttribute.type;
    public Color Color => _valueAttribute.attributeColor;
    public Connections ConnectionsType => _valueAttribute.connections;
    public Vector3 DragPoint => _connectorImage.rectTransform.position;
    public Vector3 AnchoredPositionPoint => _connectorImage.rectTransform.position;

    public int ConnectionsCount => _connections.Count;

    public List<Connector> Connections
    {
        get
        {
            return _connections;
        }
        set
        {
            _connections = value;
        }
    }

    [SerializeField] private TextMeshProUGUI _name;
    [SerializeField] private Image _connectorImage;
    [SerializeField] private Image _connectorImageFill;
    private NodeBase _node;

    private NodeValueAttribute _valueAttribute;
    [SerializeField] private List<Connector> _connections = new();

    public void SetData(NodeValueAttribute attribute)
    {
        _valueAttribute = attribute;
        _name.text = $"{attribute.attributeName}\n<size=8>({attribute.type.Name})</size>";
        _connectorImage.color = attribute.attributeColor;
        _connectorImageFill.color = attribute.attributeColor;
        SetConnectorFilled(false);
    }

    public void AddConnection(Connector connector)
    {
        _connections.Add(connector);
    }

    public void SetConnectorFilled(bool filled)
    {
        _connectorImageFill.gameObject.SetActive(filled);
    }

    public void SetNode(NodeBase node)
    {
        _node = node;
    }

    internal void UpdateFilled()
    {
        SetConnectorFilled(_connections.Count > 0);
    }
}

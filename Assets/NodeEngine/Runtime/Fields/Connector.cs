using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Connector : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private Image _connectorImage;
    [SerializeField] private Image _connectorImageFill;

    private NodeBase _node;
    private NodeFieldBase _field;
    private NodeValueAttribute _valueAttribute;
    private List<Connector> _connections = new List<Connector>();

    // Public properties
    public NodeBase Node => _node;
    public NodeFieldBase Field => _field;
    public Type ValueType => Field is VariableNodeField ? ((VariableNodeField)Field).GetValueType() : _valueAttribute?.type ?? typeof(object);
    public Color Color => _valueAttribute?.attributeColor ?? Color.gray;
    public Vector3 DragPoint => _connectorImage.rectTransform.position;
    public Vector3 AnchoredPositionPoint => _connectorImage.rectTransform.position;
    public int ConnectionsCount => _connections.Count;
    public List<Connector> Connections => _connections;

    public void SetData(NodeValueAttribute attribute)
    {
        if (attribute == null)
        {
            Debug.LogWarning("Connector.SetData called with null attribute, using defaults");
            attribute = CreateDefaultAttribute();
        }

        _valueAttribute = attribute;

        if (_nameText != null)
        {
            _nameText.text = $"{attribute.attributeName}\n<size=8>({attribute.type.Name})</size>";
        }

        if (_connectorImage != null)
        {
            _connectorImage.color = attribute.attributeColor;
        }

        if (_connectorImageFill != null)
        {
            _connectorImageFill.color = attribute.attributeColor;
        }

        SetConnectorFilled(false);
    }

    private NodeValueAttribute CreateDefaultAttribute()
    {
        return new NodeValueAttribute(
            "Default",
            typeof(object),
            System.Drawing.KnownColor.Gray
        );
    }

    public void AddConnection(Connector connector) => _connections.Add(connector);
    public void RemoveConnection(Connector connector) => _connections.Remove(connector);

    public void SetConnectorFilled(bool filled)
    {
        if (_connectorImageFill != null)
            _connectorImageFill.gameObject.SetActive(filled);
    }

    public void SetNode(NodeBase node) => _node = node;

    public void UpdateFilled() => SetConnectorFilled(_connections.Count > 0);

    public void Process(List<Connector> connectors)
    {
        if (connectors.Contains(this)) return;

        foreach (var connection in _connections)
        {
            connectors.Add(connection);
            connection.Node.Process(connectors);
        }
    }

    public void SetField(NodeFieldBase field)
    {
        _field = field;
        _field.Connector = this;
    }

    public void ClearConnections()
    {
        _connections.Clear();
        UpdateFilled();
    }
}
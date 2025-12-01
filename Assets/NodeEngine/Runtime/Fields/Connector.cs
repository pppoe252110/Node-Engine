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
    private static ConnectorColorDatabase _colorDatabase;

    private IConnectorValue _connectorValue;

    public NodeBase Node => _node;
    public NodeFieldBase Field => _field;
    public Type ValueType { get; private set; }
    public Color Color => GetConnectorColor();
    public Vector3 DragPoint => _connectorImage.rectTransform.position;
    public Vector3 AnchoredPositionPoint => _connectorImage.rectTransform.position;
    public int ConnectionsCount => _connections.Count;
    public List<Connector> Connections => _connections;

    public void SetConnectorValue(IConnectorValue value)
    {
        _connectorValue = value;
        UpdateVisuals();
    }

    public IConnectorValue GetConnectorValue()
    {
        return _connectorValue;
    }

    public void SetColorDatabase(ConnectorColorDatabase colorDatabase)
    {
        _colorDatabase = colorDatabase;
    }

    private Color GetConnectorColor()
    {
        if (_colorDatabase != null)
            return _colorDatabase.GetColorForType(ValueType);

        return Color.gray;
    }

    public void SetData(NodeValueAttribute attribute)
    {
        _valueAttribute = attribute ?? CreateDefaultAttribute();
        CalculateAndCacheValueType();

        if (_nameText != null)
        {
            _nameText.text = $"{_valueAttribute.attributeName}\n<size=8>({_valueAttribute.type.Name})</size>";
        }

        UpdateVisuals();
        SetConnectorFilled(false);
    }

    private NodeValueAttribute CreateDefaultAttribute()
    {
        return new NodeValueAttribute("Default", typeof(object));
    }

    private void UpdateVisuals()
    {
        Color connectorColor = Color;

        if (_connectorImage != null)
            _connectorImage.color = connectorColor;

        if (_connectorImageFill != null)
            _connectorImageFill.color = connectorColor;
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

    public void SetField(NodeFieldBase field)
    {
        _field = field;
        _field.Connector = this;
        CalculateAndCacheValueType();
    }

    private void CalculateAndCacheValueType()
    {
        ValueType = Field is VariableNodeField ?
            ((VariableNodeField)Field).GetValueType() :
            _valueAttribute?.type ?? typeof(object);
    }

    public void ClearConnections()
    {
        _connections.Clear();
        UpdateFilled();
    }
}

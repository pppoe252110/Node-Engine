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
    private ConnectorColorDatabase _colorDatabase;

    private IConnectorValue _connectorValue;

    // Public properties for UI elements
    public Image ConnectorImage => _connectorImage;
    public Image ConnectorImageFill => _connectorImageFill;
    public TextMeshProUGUI NameText => _nameText;

    public NodeBase Node => _node;
    public NodeFieldBase Field => _field;
    public Type ValueType { get; private set; }
    public Color Color => GetConnectorColor();
    public Vector3 DragPoint => _connectorImage.rectTransform.position;
    public Vector3 AnchoredPositionPoint => _connectorImage.rectTransform.position;
    public int ConnectionsCount => _connections.Count;
    public List<Connector> Connections => _connections;
    public ConnectorColorDatabase ColorDatabase => _colorDatabase;

    private List<IConnectionListener> _listeners = new List<IConnectionListener>();

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
        UpdateVisuals();
    }

    private Color GetConnectorColor()
    {
        if (_colorDatabase != null)
        {
            Color color = _colorDatabase.GetColorForType(ValueType);
            return color;
        }

        Debug.LogWarning($"GetConnectorColor(): No color database for type {ValueType?.Name}");
        return Color.gray;
    }

    // In Connector.cs, modify the SetData method:
    public void SetData(NodeValueAttribute attribute)
    {
        _valueAttribute = attribute ?? CreateDefaultAttribute();

        // FIX: Use the attribute's type directly instead of recalculating from field
        ValueType = _valueAttribute.type;

        // Also update the field's connector value if needed
        if (_connectorValue == null || _connectorValue.InnerType != ValueType)
        {
            _connectorValue = CreateConnectorValue(ValueType);
        }

        if (_nameText != null)
        {
            _nameText.text = $"{_valueAttribute.attributeName}\n<size=8>({_valueAttribute.type.Name})</size>";
        }

        UpdateVisuals();
        SetConnectorFilled(false);
    }

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
        if (type == typeof(Type))
            return new ConnectorValueType(typeof(object));

        return new ConnectorValueObject(null);
    }

    // Also update the CalculateAndCacheValueType method:
    private void CalculateAndCacheValueType()
    {
        // Priority: Use attribute type if available, otherwise field type
        if (_valueAttribute != null)
        {
            ValueType = _valueAttribute.type;
        }
        else if (Field != null)
        {
            ValueType = Field.GetValueType();
        }
        else
        {
            ValueType = typeof(object);
        }
    }

    private NodeValueAttribute CreateDefaultAttribute()
    {
        return new NodeValueAttribute("Default", typeof(object));
    }

    // In Connector.cs, update the UpdateVisuals method:
    public void UpdateVisuals()
    {
        Color connectorColor = Color;

        if (_connectorImage != null)
            _connectorImage.color = connectorColor;

        if (_connectorImageFill != null)
            _connectorImageFill.color = connectorColor;

        // Update line renderer colors for all connections
        if (LineRenderersController.Instance != null)
        {
            LineRenderersController.UpdateConnectionColors(this);
        }
    }


    public void AddConnectionListener(IConnectionListener listener)
    {
        if (!_listeners.Contains(listener))
            _listeners.Add(listener);
    }

    public void RemoveConnectionListener(IConnectionListener listener)
    {
        _listeners.Remove(listener);
    }

    public void AddConnection(Connector connector)
    {
        _connections.Add(connector);

        foreach (var listener in _listeners)
        {
            listener.OnConnected(this, connector);
        }

        if (connector.Node is IConnectionListener otherListener)
        {
            otherListener.OnConnected(connector, this);
        }

        UpdateFilled();
    }

    public void RemoveConnection(Connector connector)
    {
        _connections.Remove(connector);

        foreach (var listener in _listeners)
        {
            listener.OnDisconnected(this, connector);
        }

        if (connector.Node is IConnectionListener otherListener)
        {
            otherListener.OnDisconnected(connector, this);
        }

        UpdateFilled();
    }

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

    public void ClearConnections()
    {
        _connections.Clear();
        UpdateFilled();
    }

    // Helper method to directly update UI with a specific color
    public void ForceUpdateVisuals(Color color)
    {
        if (_connectorImage != null)
            _connectorImage.color = color;

        if (_connectorImageFill != null)
            _connectorImageFill.color = color;
    }
}
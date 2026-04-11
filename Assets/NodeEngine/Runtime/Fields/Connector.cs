using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class Connector : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private Image _connectorImage;
    [SerializeField] private Image _connectorImageFill;

    private ConnectorColorDatabase _colorDatabase;
    private LineRenderersController _lineRenderersController;

    // --- Core Metadata ---
    public BaseNode Node { get; private set; }
    public string PortName { get; private set; }
    public Type ValueType { get; private set; }
    public bool IsInput { get; private set; }
    public bool IsFlow { get; private set; }

    // --- Visual State ---
    public List<Connector> Connections { get; } = new();
    public int ConnectionsCount => Connections.Count;
    public Vector3 DragPoint => _connectorImage.rectTransform.position;
    public Vector3 AnchoredPositionPoint => _connectorImage.rectTransform.position;
    public Color Color => GetConnectorColor();

    [Inject]
    public void Construct(LineRenderersController lineRenderersController)
    {
        _lineRenderersController = lineRenderersController;
    }

    public void Setup(string portName, Type type, bool isInput, bool isFlow, BaseNode owner)
    {
        PortName = portName;
        ValueType = type;
        IsInput = isInput;
        IsFlow = isFlow;
        Node = owner;

        if (_nameText != null)
        {
            string typeName = isFlow ? "Flow" : TypeSerializer.GetTypeDisplayName(type);
            _nameText.text = $"{portName}\n<size=8>({typeName})</size>";
        }

        SetConnectorFilled(false);
    }

    public void SetValueType(Type newType)
    {
        ValueType = newType;
        if (_nameText != null)
        {
            string typeName = IsFlow ? "Flow" : newType.Name;
            _nameText.text = $"{PortName}\n<size=8>({typeName})</size>";
        }
        UpdateVisuals();
    }

    public void SetConnectorFilled(bool filled)
    {
        if (_connectorImageFill != null)
            _connectorImageFill.gameObject.SetActive(filled);
    }

    public void SetColorDatabase(ConnectorColorDatabase colorDatabase)
    {
        _colorDatabase = colorDatabase;
        UpdateVisuals();
    }

    private Color GetConnectorColor()
    {
        if (_colorDatabase != null) return _colorDatabase.GetColorForType(ValueType);
        return Color.gray;
    }

    public void UpdateVisuals()
    {
        Color color = Color;
        if (_connectorImage != null) _connectorImage.color = color;
        if (_connectorImageFill != null) _connectorImageFill.color = color;

        _lineRenderersController?.UpdateConnectionColors(this);
    }

    public void AddVisualConnection(Connector other)
    {
        if (!Connections.Contains(other))
            Connections.Add(other);
        UpdateFilled();
    }

    public void RemoveVisualConnection(Connector other)
    {
        Connections.Remove(other);
        UpdateFilled();
    }

    public void UpdateFilled()
    {
        if (_connectorImageFill != null)
            _connectorImageFill.gameObject.SetActive(ConnectionsCount > 0);
    }
}
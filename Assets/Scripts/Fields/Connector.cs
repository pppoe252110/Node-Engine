using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Connector : MonoBehaviour
{
    public NodeBase Node => _node;
    public Type ValueType => _valueAttribute.type;
    public Color Color => _valueAttribute.attributeColor;
    public Vector3 DragPoint => _connectorImage.rectTransform.position;
    public Vector3 AnchoredPositionPoint => _connectorImage.rectTransform.position;

    [SerializeField] private TextMeshProUGUI _name;
    [SerializeField] private Image _connectorImage;
    [SerializeField] private Image _connectorImageFill;
    private NodeBase _node;
    
    private NodeValueAttribute _valueAttribute;

    public void SetData(NodeValueAttribute attribute)
    {
        _valueAttribute = attribute;
        _name.text = $"{attribute.attributeName}\n<size=8>({attribute.type.Name})</size>";
        _connectorImage.color = attribute.attributeColor;
        _connectorImageFill.color = attribute.attributeColor;
        SetConnectorFilled(false);
    }

    public void SetConnectorFilled(bool filled)
    {
        _connectorImageFill.gameObject.SetActive(filled);
    }

    public void SetNode(NodeBase node)
    {
        _node = node;
    }
}

using Sirenix.OdinInspector;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NodeLogic : SerializedMonoBehaviour
{
    [SerializeField] private NodeBase _node;
    [Header("Connectors")]
    [SerializeField] private Connector _rightConnectorPrefab;
    [SerializeField] private Connector _leftConnectorPrefab;
    [SerializeField] private RectTransform _rightConnectorsParent;
    [SerializeField] private RectTransform _leftConnectorsParent;
    [Header("Properties")]
    [SerializeField] private Image _image;
    [SerializeField] private Image _nodeIcon;
    [SerializeField] private TextMeshProUGUI _nodeName;

    public void SetNodeBase(NodeBase nodeBase)
    {
        _node = nodeBase.Clone() as NodeBase;
        _node.Initialize(this, gameObject.GetInstanceID());

        _nodeName.text = _node.NodeName;
        _nodeIcon.sprite = _node.NodeSprite;
        _nodeIcon.color = _node.NodeSprite ? Color.white : Color.clear;

        GenerateInputConnectors();
        GenerateOutputConnectors();


        _image.rectTransform.sizeDelta = new Vector2(_image.rectTransform.sizeDelta.x, 57 + (Mathf.Max(_node.inputFields.Count, _node.outputFields.Count)) * 25);
        _image.material = new Material(_image.material);

        RecalculateMaterial();
    }

    private void GenerateInputConnectors()
    {
        foreach (NodeFieldBase field in _node.inputFields)
        {
            var connector = Instantiate(_leftConnectorPrefab, _leftConnectorsParent);
            ProceedField(field, connector);
        }
    }

    private void ProceedField(NodeFieldBase field, Connector connector)
    {
        var attribute = field.GetAttribute();
        connector.SetNode(_node);
        connector.SetData(attribute);
    }

    private void RecalculateMaterial()
    {
        _image.material.SetFloat("_ScaleRatio", _image.rectTransform.rect.width / _image.rectTransform.rect.height);
    }

    private void GenerateOutputConnectors()
    {
        foreach (NodeFieldBase field in _node.outputFields)
        {
            var connector = Instantiate(_rightConnectorPrefab, _rightConnectorsParent);
            ProceedField(field, connector);
        }
    }
}

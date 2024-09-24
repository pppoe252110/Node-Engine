using Radishmouse;
using System;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class NodeBase : MonoBehaviour
{
    [SerializeField] private string nodeName = "Basic";

    [Header("Connectors")]
    [SerializeField] private Connector _rightConnectorPrefab;
    [SerializeField] private Connector _leftConnectorPrefab;
    [SerializeField] private RectTransform _rightConnectorsParent;
    [SerializeField] private RectTransform _leftConnectorsParent;
    [Header("Properties")]
    [SerializeField] private Image _image;
    [SerializeField] private TextMeshProUGUI _nodeName;

    public List<NodeFieldBase> inputFields = new();
    public List<NodeFieldBase> outputFields = new();

    private int _guid = 0;

    private void Awake()
    {
        Initialize();
    }

    private void Reset()
    {
        if (TryGetComponent(out NodeBase nb))
        {
            _rightConnectorPrefab = nb._rightConnectorPrefab;
            _leftConnectorPrefab = nb._leftConnectorPrefab;

            _rightConnectorsParent = nb._rightConnectorsParent;
            _leftConnectorsParent = nb._leftConnectorsParent;

            _image = nb._image;
            _nodeName = nb._nodeName;
        }
    }

    public abstract void Setup();

    private void Initialize()
    {
        if(_guid == 0)
            _guid = gameObject.GetInstanceID();

        _nodeName.text = nodeName;

        Setup();

        GenerateInputConnectors();
        GenerateOutputConnectors();

        _image.rectTransform.sizeDelta = new Vector2(_image.rectTransform.sizeDelta.x, 57 + (Mathf.Max(inputFields.Count, outputFields.Count)) * 25);
        _image.material = new Material(_image.material);
        RecalculateMaterial();
    }

    private void GenerateInputConnectors()
    {
        foreach(NodeFieldBase field in inputFields)
        {
            var connector = Instantiate(_leftConnectorPrefab, _leftConnectorsParent);
            ProceedField(field, connector);
        }
    }
    
    private void GenerateOutputConnectors()
    {
        foreach(NodeFieldBase field in outputFields)
        {
            var connector = Instantiate(_rightConnectorPrefab, _rightConnectorsParent);
            ProceedField(field, connector);
        }
    }

    private void ProceedField(NodeFieldBase field, Connector connector)
    {
        var attribute = field.GetAttribute();
        connector.SetNode(this);
        connector.SetData(attribute);

    }

    private void RecalculateMaterial()
    {
        _image.material.SetFloat("_ScaleRatio", _image.rectTransform.rect.width / _image.rectTransform.rect.height);
    }
}

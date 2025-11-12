using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NodeLogic : MonoBehaviour
{
    public NodeBase Node => _node;
    public VariableDatabase VariableDatabase { get; set; }

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
        _node.Initialize(this, gameObject.GetEntityId());

        _nodeName.text = _node.NodeName;
        _nodeIcon.sprite = _node.NodeSprite;
        _nodeIcon.color = _node.NodeSprite ? Color.white : Color.clear;

        gameObject.name = Node.NodeName;

        if (_node is VariableNode varNode && VariableDatabase != null)
        {
            SpawnVariableUI(varNode);
        }
        else
        {
            GenerateInputConnectors();
        }
        GenerateOutputConnectors();

        _image.rectTransform.sizeDelta = new Vector2(_image.rectTransform.sizeDelta.x, 57 + (Mathf.Max(_node.inputFields.Count, _node.outputFields.Count)) * 25);
        _image.material = new Material(_image.material);

        RecalculateMaterial();

        NodeLogicProcessor.Instance.AddNode(this);
    }

    // Add this method to your existing NodeLogic class if not already there
    public void DeleteNode()
    {
        // Remove all connections first
        RemoveAllConnections();

        // Remove from processor
        NodeLogicProcessor.Instance?.RemoveNode(this);

        // Destroy the game object
        Destroy(gameObject);
    }

    private void RemoveAllConnections()
    {
        // Remove input connections
        foreach (var connector in _node.inputConnectors)
        {
            foreach (var connectedConnector in connector.Connections.ToArray())
            {
                LineRenderersController.Remove(connector, connectedConnector);
                connectedConnector.Connections.Remove(connector);
                connectedConnector.UpdateFilled();
            }
            connector.Connections.Clear();
            connector.UpdateFilled();
        }

        // Remove output connections  
        foreach (var connector in _node.outputConnectors)
        {
            foreach (var connectedConnector in connector.Connections.ToArray())
            {
                LineRenderersController.Remove(connector, connectedConnector);
                connectedConnector.Connections.Remove(connector);
                connectedConnector.UpdateFilled();
            }
            connector.Connections.Clear();
            connector.UpdateFilled();
        }
    }

    // Rest of your existing methods...
    private void SpawnVariableUI(VariableNode varNode)
    {
        var prefab = VariableDatabase.GetPrefabForType(varNode.VariableType);
        if (prefab == null) return;

        var uiElement = Instantiate(prefab, _leftConnectorsParent);
        uiElement.Initialize(VariableDatabase, varNode.VariableType);
        varNode.UIElement = uiElement;

        var boxHeight = 30f;
        _image.rectTransform.sizeDelta = new Vector2(_image.rectTransform.sizeDelta.x,
            Mathf.Max(_image.rectTransform.sizeDelta.y, boxHeight + 20f));
    }

    private void GenerateInputConnectors()
    {
        foreach (NodeFieldBase field in _node.inputFields)
        {
            var connector = Instantiate(_leftConnectorPrefab, _leftConnectorsParent);
            ProceedField(field, connector);
            _node.inputConnectors.Add(connector);
        }
    }

    private void GenerateOutputConnectors()
    {
        foreach (NodeFieldBase field in _node.outputFields)
        {
            var connector = Instantiate(_rightConnectorPrefab, _rightConnectorsParent);
            ProceedField(field, connector);
            _node.outputConnectors.Add(connector);
        }
    }

    private void ProceedField(NodeFieldBase field, Connector connector)
    {
        var attribute = field.GetAttribute();
        connector.SetField(field);
        connector.SetNode(_node);
        connector.SetData(attribute);
    }

    private void RecalculateMaterial()
    {
        _image.material.SetFloat("_ScaleRatio", _image.rectTransform.rect.width / _image.rectTransform.rect.height);
    }

    internal void Process()
    {
        _node.Process();
    }
}
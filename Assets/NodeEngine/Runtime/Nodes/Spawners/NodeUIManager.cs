using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NodeUIManager : MonoBehaviour
{
    public RectTransform RightConnectorsParent => _rightConnectorsParent;
    public RectTransform LeftConnectorsParent => _leftConnectorsParent;
    public ConnectorColorDatabase ColorDatabase => _colorDatabase;

    [SerializeField] private ConnectorColorDatabase _colorDatabase;
    [SerializeField] private VariableDatabase _variableDatabase;
    [SerializeField] private Connector _rightConnectorPrefab;
    [SerializeField] private Connector _leftConnectorPrefab;
    [SerializeField] private RectTransform _rightConnectorsParent;
    [SerializeField] private RectTransform _leftConnectorsParent;

    public void CreateConnectors(BaseNode node, List<BaseNode.NodePortInfo> inputPorts, List<BaseNode.NodePortInfo> outputPorts,
                                 List<Connector> inputConnectors, List<Connector> outputConnectors)
    {
        GenerateInputConnectors(node, inputPorts, inputConnectors);
        GenerateOutputConnectors(node, outputPorts, outputConnectors);
    }

    public void GenerateInputConnectors(BaseNode node, List<BaseNode.NodePortInfo> inputPorts, List<Connector> inputConnectors)
    {
        foreach (var port in inputPorts)
        {
            var connector = Instantiate(_leftConnectorPrefab, _leftConnectorsParent);
            connector.Setup(port.Name, port.ValueType, port.IsInput, port.IsFlow, node);
            connector.SetColorDatabase(_colorDatabase);
            inputConnectors.Add(connector);
        }
    }

    public void GenerateOutputConnectors(BaseNode node, List<BaseNode.NodePortInfo> outputPorts, List<Connector> outputConnectors)
    {
        foreach (var port in outputPorts)
        {
            var connector = Instantiate(_rightConnectorPrefab, _rightConnectorsParent);
            connector.Setup(port.Name, port.ValueType, port.IsInput, port.IsFlow, node);
            connector.SetColorDatabase(_colorDatabase);
            outputConnectors.Add(connector);
        }
    }

    public void CreateVariableUI(VariableNode varNode, Image nodeImage)
    {
        if (varNode == null || _variableDatabase == null) return;
        var prefab = _variableDatabase.GetPrefabForType(varNode.VariableType);
        if (prefab == null) return;

        var uiElement = Instantiate(prefab, _leftConnectorsParent);
        uiElement.Initialize(varNode, varNode.VariableType);
        varNode.UIElement = uiElement;

        varNode.SyncUIWithCachedValue();
        UpdateNodeSizeForVariableUI(nodeImage);
    }

    public void CreateConverterUI(TypeVariableNode converterNode, Image nodeImage)
    {
        if (converterNode == null || _variableDatabase == null) return;
        var prefab = _variableDatabase.ConverterUIPrefab;
        if (prefab == null) return;

        var uiElement = Instantiate(prefab, LeftConnectorsParent);
        uiElement.Initialize(converterNode);
        converterNode.UIElement = uiElement;

        converterNode.SyncUIWithCachedValue();
        UpdateNodeSizeForVariableUI(nodeImage);
    }

    private void UpdateNodeSizeForVariableUI(Image nodeImage)
    {
        const float boxHeight = 30f;
        const float padding = 20f;
        nodeImage.rectTransform.sizeDelta = new Vector2(nodeImage.rectTransform.sizeDelta.x, Mathf.Max(nodeImage.rectTransform.sizeDelta.y, boxHeight + padding));
    }
}
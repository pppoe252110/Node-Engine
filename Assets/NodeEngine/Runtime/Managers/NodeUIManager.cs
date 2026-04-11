using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class NodeUIManager : MonoBehaviour
{
    public RectTransform RightConnectorsParent => _rightConnectorsParent;
    public RectTransform LeftConnectorsParent => _leftConnectorsParent;
    public ConnectorColorDatabase ColorDatabase => _colorDatabase;

    [SerializeField] private ConnectorColorDatabase _colorDatabase;
    [SerializeField] private Connector _rightConnectorPrefab;
    [SerializeField] private Connector _leftConnectorPrefab;
    [SerializeField] private RectTransform _rightConnectorsParent;
    [SerializeField] private RectTransform _leftConnectorsParent;

    [Inject] private VariableUIRegistry _variableUIRegistry;

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

    public void CreateVariableUI(IVariableNode varNode, Image nodeImage)
    {
        var prefab = _variableUIRegistry.GetPrefabForNode(varNode);
        if (prefab == null) return;

        var ui = Instantiate(prefab, LeftConnectorsParent);
        ui.Bind(varNode);

        // Adjust node size
        UpdateNodeSizeForVariableUI(nodeImage);
    }

    private void UpdateNodeSizeForVariableUI(Image nodeImage)
    {
        const float boxHeight = 30f;
        const float padding = 20f;
        nodeImage.rectTransform.sizeDelta = new Vector2(nodeImage.rectTransform.sizeDelta.x, Mathf.Max(nodeImage.rectTransform.sizeDelta.y, boxHeight + padding));
    }
}
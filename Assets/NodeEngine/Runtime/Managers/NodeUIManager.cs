using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class NodeUIManager : MonoBehaviour
{
    public RectTransform RightConnectorsParent => _rightConnectorsParent;
    public RectTransform LeftConnectorsParent => _leftConnectorsParent;

    [Header("Connector Prefabs")]
    [SerializeField] private Connector _rightConnectorPrefab;
    [SerializeField] private Connector _leftConnectorPrefab;
    [SerializeField] private ConnectorColorDatabase _colorDatabase;

    [Header("Parents")]
    [SerializeField] private RectTransform _rightConnectorsParent;
    [SerializeField] private RectTransform _leftConnectorsParent;

    [Inject] private VariableUIRegistry _variableUIRegistry;
    [Inject] private IObjectResolver _resolver;

    public void SetupNodeVisuals(NodeLogic nodeLogic, BaseNode node)
    {
        // Basic text/icon setup
        nodeLogic.NodeNameText.text = node.NodeName;
        nodeLogic.NodeTypeText.text = GetNodeTypeFromPath(node);
        nodeLogic.NodeIcon.sprite = node.NodeSprite;
        nodeLogic.NodeIcon.color = node.NodeSprite ? Color.white : Color.clear;

        // Generate connectors
        GenerateConnectors(nodeLogic, node);

        // Variable UI if applicable
        if (node is IVariableNode varNode)
            CreateVariableUI(varNode, nodeLogic.BackgroundImage);

        // Size calculation
        int inputCount = node.Ports.Count(p => p.IsInput);
        int outputCount = node.Ports.Count(p => !p.IsInput);
        float height = 57 + Mathf.Max(inputCount, outputCount) * 25;
        nodeLogic.BackgroundImage.rectTransform.sizeDelta =
            new Vector2(nodeLogic.BackgroundImage.rectTransform.sizeDelta.x, height);

        nodeLogic.BackgroundImage.material = new Material(nodeLogic.BackgroundImage.material);
        nodeLogic.RecalculateMaterial();
    }

    private void GenerateConnectors(NodeLogic nodeLogic, BaseNode node)
    {
        foreach (var port in node.Ports)
        {
            var prefab = port.IsInput ? _leftConnectorPrefab : _rightConnectorPrefab;
            var parent = port.IsInput ? nodeLogic.LeftConnectorsParent : nodeLogic.RightConnectorsParent;

            var connector = Instantiate(prefab, parent);
            connector.Setup(port.Name, port.ValueType, port.IsInput, port.IsFlow, node);
            connector.SetColorDatabase(_colorDatabase);

            if (port.IsInput)
                nodeLogic.InputConnectors.Add(connector);
            else
                nodeLogic.OutputConnectors.Add(connector);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(nodeLogic.LeftConnectorsParent);
        LayoutRebuilder.ForceRebuildLayoutImmediate(nodeLogic.RightConnectorsParent);
    }

    public void CreateVariableUI(IVariableNode varNode, Image nodeImage)
    {
        var prefab = _variableUIRegistry.GetPrefabForNode(varNode);
        if (prefab == null) return;

        var ui = Instantiate(prefab, _leftConnectorsParent);
        _resolver.Inject(ui);
        ui.Bind(varNode);

        // Adjust node size for variable UI
        const float boxHeight = 30f;
        const float padding = 20f;
        nodeImage.rectTransform.sizeDelta = new Vector2(
            nodeImage.rectTransform.sizeDelta.x,
            Mathf.Max(nodeImage.rectTransform.sizeDelta.y, boxHeight + padding));
    }

    private string GetNodeTypeFromPath(BaseNode node)
    {
        var pathAttr = node.GetType().GetCustomAttribute<NodePathAttribute>();
        if (pathAttr != null && !string.IsNullOrEmpty(pathAttr.Path))
        {
            int slash = pathAttr.Path.IndexOf('/');
            return slash >= 0 ? pathAttr.Path.Substring(0, slash) : pathAttr.Path;
        }
        return node.GetType().Name.Replace("Node", "");
    }
}
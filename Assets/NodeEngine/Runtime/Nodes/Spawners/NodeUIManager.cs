using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NodeUIManager : MonoBehaviour
{
    public RectTransform RightConnectorsParent => _rightConnectorsParent;
    public RectTransform LeftConnectorsParent => _leftConnectorsParent;
    public ConnectorColorDatabase ColorDatabase => _colorDatabase; // Expose the database

    [SerializeField] private ConnectorColorDatabase _colorDatabase;

    [SerializeField] private Connector _rightConnectorPrefab;
    [SerializeField] private Connector _leftConnectorPrefab;
    [SerializeField] private RectTransform _rightConnectorsParent;
    [SerializeField] private RectTransform _leftConnectorsParent;

    public void CreateConnectors(NodeBase node, List<NodeFieldBase> inputFields, List<NodeFieldBase> outputFields,
                                 List<Connector> inputConnectors, List<Connector> outputConnectors)
    {
        GenerateInputConnectors(node, inputFields, inputConnectors);
        GenerateOutputConnectors(node, outputFields, outputConnectors);
    }

    public void GenerateInputConnectors(NodeBase node, List<NodeFieldBase> inputFields, List<Connector> inputConnectors)
    {
        foreach (var field in inputFields)
        {
            var connector = Instantiate(_leftConnectorPrefab, _leftConnectorsParent);
            connector.SetField(field);
            connector.SetNode(node);
            connector.SetColorDatabase(_colorDatabase);

            var attribute = field.GetAttribute();
            connector.SetData(attribute);

            inputConnectors.Add(connector);
        }
    }

    public void GenerateOutputConnectors(NodeBase node, List<NodeFieldBase> outputFields, List<Connector> outputConnectors)
    {
        foreach (var field in outputFields)
        {
            var connector = Instantiate(_rightConnectorPrefab, _rightConnectorsParent);
            connector.SetField(field);
            connector.SetNode(node);
            connector.SetColorDatabase(_colorDatabase);

            var attribute = field.GetAttribute();
            connector.SetData(attribute);

            outputConnectors.Add(connector);
        }
    }

    public void CreateVariableUI(VariableNode varNode, VariableDatabase database, Image nodeImage)
    {
        if (varNode == null || database == null) return;

        var prefab = database.GetPrefabForType(varNode.VariableType);
        if (prefab == null) return;

        var uiElement = Instantiate(prefab, _leftConnectorsParent);
        uiElement.Initialize(varNode, varNode.VariableType);
        varNode.UIElement = uiElement;

        varNode.SyncUIWithCachedValue();

        UpdateNodeSizeForVariableUI(nodeImage);
    }

    private void UpdateNodeSizeForVariableUI(Image nodeImage)
    {
        const float boxHeight = 30f;
        const float padding = 20f;
        nodeImage.rectTransform.sizeDelta = new Vector2(
            nodeImage.rectTransform.sizeDelta.x,
            Mathf.Max(nodeImage.rectTransform.sizeDelta.y, boxHeight + padding)
        );
    }
}
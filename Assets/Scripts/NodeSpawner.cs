// Scripts/NodeSpawner.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NodeSpawner : MonoBehaviour
{
    [SerializeField] private Connector _rightConnectorPrefab;
    [SerializeField] private Connector _leftConnectorPrefab;
    [SerializeField] private RectTransform _rightConnectorsParent;
    [SerializeField] private RectTransform _leftConnectorsParent;  // Reused for VariableNode UI

    public void SpawnConnectors(NodeBase node, List<NodeFieldBase> inputFields, List<NodeFieldBase> outputFields, List<Connector> inputConnectors, List<Connector> outputConnectors)
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
            connector.SetData(field.GetAttribute());
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
            connector.SetData(field.GetAttribute());
            outputConnectors.Add(connector);
        }
    }

    public void SpawnVariableUI(VariableNode varNode, VariableDatabase database, Image nodeImage)
    {
        if (varNode == null)
        {
            Debug.LogError("VariableNode is null in SpawnVariableUI");
            return;
        }

        if (database == null)
        {
            Debug.LogError("VariableDatabase is null in SpawnVariableUI");
            return;
        }

        var prefab = database.GetPrefabForType(varNode.VariableType);
        if (prefab == null)
        {
            Debug.LogWarning($"No prefab found for VariableType {varNode.VariableType} in VariableDatabase");
            return;
        }

        var uiElement = Instantiate(prefab, _leftConnectorsParent);
        if (uiElement == null)
        {
            Debug.LogError("Failed to instantiate UI element in SpawnVariableUI");
            return;
        }

        uiElement.Initialize(database, varNode.VariableType);
        varNode.UIElement = uiElement;

        // Update node size
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
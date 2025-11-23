using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NodeSpawner : MonoBehaviour
{
    [SerializeField] private Connector _rightConnectorPrefab;
    [SerializeField] private Connector _leftConnectorPrefab;
    [SerializeField] private RectTransform _rightConnectorsParent;
    [SerializeField] private RectTransform _leftConnectorsParent;

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

            var attribute = field.GetAttribute();
            if (attribute != null)
            {
                connector.SetData(attribute);
            }
            else
            {
                // Create a default attribute for fields without one
                var defaultAttribute = CreateDefaultAttribute(field);
                connector.SetData(defaultAttribute);
            }

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

            var attribute = field.GetAttribute();
            if (attribute != null)
            {
                connector.SetData(attribute);
            }
            else
            {
                // Create a default attribute for fields without one
                var defaultAttribute = CreateDefaultAttribute(field);
                connector.SetData(defaultAttribute);
            }

            outputConnectors.Add(connector);
        }
    }

    private NodeValueAttribute CreateDefaultAttribute(NodeFieldBase field)
    {
        var valueType = field.GetValueType();
        var color = GetDefaultColorForType(valueType);

        return new NodeValueAttribute(
            GetDefaultNameForType(valueType),
            valueType,
            GetKnownColorFromUnityColor(color)
        );
    }

    private string GetDefaultNameForType(System.Type type)
    {
        return type.Name switch
        {
            "Int32" => "Value",
            "Single" => "Value",
            "Boolean" => "Value",
            "String" => "Value",
            "Void" => "Execute",
            _ => "Object"
        };
    }

    private Color GetDefaultColorForType(System.Type type)
    {
        return type.Name switch
        {
            "Int32" => Color.red,
            "Single" => Color.green,
            "Boolean" => Color.blue,
            "String" => Color.yellow,
            "Void" => new Color(0.5f, 0f, 0.5f), // Purple
            _ => Color.gray
        };
    }

    private System.Drawing.KnownColor GetKnownColorFromUnityColor(Color color)
    {
        // Map Unity colors to System.Drawing known colors
        if (color == Color.red) return System.Drawing.KnownColor.Red;
        if (color == Color.green) return System.Drawing.KnownColor.Green;
        if (color == Color.blue) return System.Drawing.KnownColor.Blue;
        if (color == Color.yellow) return System.Drawing.KnownColor.Yellow;
        if (color == new Color(0.5f, 0f, 0.5f)) return System.Drawing.KnownColor.BlueViolet;
        return System.Drawing.KnownColor.Gray;
    }

    public void SpawnVariableUI(VariableNode varNode, VariableDatabase database, Image nodeImage)
    {
        if (varNode == null || database == null)
        {
            Debug.LogError("VariableNode or VariableDatabase is null in SpawnVariableUI");
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

        uiElement.Initialize(varNode, varNode.VariableType);
        varNode.UIElement = uiElement;

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
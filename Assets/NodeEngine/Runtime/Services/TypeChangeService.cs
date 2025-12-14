using System;
using System.Collections.Generic;
using UnityEngine;

public static class TypeChangeService
{
    private static bool _isInUpdate = false;

    public static bool TryChangeConnectorType(Connector connector, Type newType)
    {
        if (_isInUpdate)
        {
            Debug.LogWarning("TypeChangeService: Already updating a connector, preventing recursion");
            return false;
        }

        if (connector == null || connector.Field == null)
            return false;

        _isInUpdate = true;

        try
        {
            var field = connector.Field;

            // Check if field supports type changes
            if (!(field is ITypeChangeable typeChangeableField))
            {
                Debug.LogWarning($"Field does not support type changes: {field.GetType().Name}");
                return false;
            }

            // Check if type is actually changing
            Type currentType = field.GetValueType();
            if (currentType == newType)
            {
                return true;
            }

            // Try to change the type directly on the field
            bool success = typeChangeableField.ChangeOutputType(newType);

            if (success)
            {
                // Update connector visuals
                UpdateConnectorVisuals(connector, newType);
            }
            else
            {
                Debug.LogWarning($"TypeChangeService: Failed to change type from {currentType.Name} to {newType.Name}");
            }

            return success;
        }
        finally
        {
            _isInUpdate = false;
        }
    }

    // In TypeChangeService.cs, update the UpdateConnectorVisuals method:
    private static void UpdateConnectorVisuals(Connector connector, Type newType)
    {
        if (connector == null) return;

        try
        {
            // Store old type for comparison
            Type oldType = connector.ValueType;

            // Update the current value's inner type if it's ConnectorValueObject
            if (connector.Field is NodeFieldTyped<ConnectorValueObject> fieldTyped &&
                fieldTyped.GetCurrentValue() is ConnectorValueObject connectorObject)
            {
                // Create default value for the new type
                object defaultValue = CreateDefaultValue(newType);
                connectorObject.SetInnerValue(defaultValue);
            }

            // Create new attribute with the correct type
            var attribute = connector.Field?.GetAttribute();
            if (attribute != null)
            {
                var newAttribute = new NodeValueAttribute(attribute.attributeName, newType);
                connector.SetData(newAttribute);
            }

            // Get the new color from the connector
            Color newColor = connector.Color;

            // Directly update the UI components using public properties
            UpdateConnectorUIComponents(connector, newColor, newType);

            // Restore the fill state
            RestoreConnectorFillState(connector);

            // IMPORTANT: Check for incompatible connections BEFORE updating line colors
            TypeChangeLogic.CheckAndDisconnectIncompatible(connector, newType);

            // Update line colors for all remaining connections
            UpdateConnectionLineColors(connector, newColor);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error updating connector visuals: {e.Message}");
        }
    }

    private static void UpdateConnectionLineColors(Connector connector, Color newColor)
    {
        if (connector == null || LineRenderersController.Instance == null) return;

        try
        {
            // Get the connections list from LineRenderersController (using reflection since it's private)
            var controllerType = typeof(LineRenderersController);
            var connectionsField = controllerType.GetField("_connections",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (connectionsField == null) return;

            var connections = connectionsField.GetValue(LineRenderersController.Instance)
                as List<LineRenderersController.ConnectionData>;

            if (connections == null) return;

            foreach (var connection in connections)
            {
                if (!connection.IsValid) continue;

                bool isOurConnection = connection.ConnectorA == connector || connection.ConnectorB == connector;

                if (isOurConnection)
                {
                    var lineRenderer = connection.LineRenderer;
                    if (lineRenderer != null && lineRenderer.material != null)
                    {
                        // Get the other connector's color
                        Color otherColor = connection.ConnectorA == connector ?
                            connection.ConnectorB.Color : connection.ConnectorA.Color;

                        // Update material colors
                        lineRenderer.material.SetColor("_Color1", connection.ConnectorA == connector ? newColor : otherColor);
                        lineRenderer.material.SetColor("_Color2", connection.ConnectorB == connector ? newColor : otherColor);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Failed to update line colors: {e.Message}");
        }
    }

    private static object CreateDefaultValue(Type type)
    {
        if (type == typeof(int)) return 0;
        if (type == typeof(float)) return 0f;
        if (type == typeof(bool)) return false;
        if (type == typeof(string)) return "";
        if (type == typeof(Vector3)) return Vector3.zero;
        if (type == typeof(Type)) return typeof(object);
        return null;
    }

    private static void UpdateConnectorUIComponents(Connector connector, Color newColor, Type newType)
    {
        if (connector == null) return;

        // Update connector image using public property
        if (connector.ConnectorImage != null)
        {
            connector.ConnectorImage.color = newColor;
        }

        // Update connector image fill using public property
        if (connector.ConnectorImageFill != null)
        {
            connector.ConnectorImageFill.color = newColor;
        }

        // Update the text
        if (connector.NameText != null && connector.Field?.GetAttribute() != null)
        {
            connector.NameText.text = $"{connector.Field.GetAttribute().attributeName}\n<size=8>({newType.Name})</size>";
        }
    }

    private static void RestoreConnectorFillState(Connector connector)
    {
        if (connector == null) return;

        // Use the public method
        connector.UpdateFilled();
    }
}
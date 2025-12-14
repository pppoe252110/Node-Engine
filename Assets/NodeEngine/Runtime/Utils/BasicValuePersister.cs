using System;
using UnityEngine;

public class BasicValuePersister : IValuePersister
{
    public bool CanPersist(VariableType variableType)
    {
        return variableType == VariableType.Bool ||
               variableType == VariableType.Int ||
               variableType == VariableType.Single ||
               variableType == VariableType.String;
    }

    public string PersistValue(object value)
    {
        if (value == null) return string.Empty;

        if (value is float floatValue)
        {
            // Use invariant culture for floats to avoid comma issues
            return floatValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        return value.ToString();
    }

    public object RestoreValue(string serializedValue, VariableType variableType)
    {
        if (string.IsNullOrEmpty(serializedValue))
            return GetDefaultValue(variableType);

        try
        {
            switch (variableType)
            {
                case VariableType.Bool:
                    return bool.Parse(serializedValue);
                case VariableType.Int:
                    return int.Parse(serializedValue);
                case VariableType.Single:
                    // Try parsing with invariant culture first
                    if (float.TryParse(serializedValue, System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out float floatValue))
                    {
                        return floatValue;
                    }
                    // Fallback to current culture
                    return float.Parse(serializedValue);
                case VariableType.String:
                    return serializedValue;
                default:
                    return GetDefaultValue(variableType);
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Failed to restore {variableType} value '{serializedValue}': {e.Message}");
            return GetDefaultValue(variableType);
        }
    }

    private object GetDefaultValue(VariableType type)
    {
        switch (type)
        {
            case VariableType.Bool: return false;
            case VariableType.Int: return 0;
            case VariableType.Single: return 0f;
            case VariableType.String: return "";
            default: return null;
        }
    }
}
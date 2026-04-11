using System;
using UnityEngine;

public static class TypeSerializer
{
    public static string SerializeType(Type type)
    {
        if (type == null)
            return string.Empty;

        // Use AssemblyQualifiedName for complete type information
        return type.AssemblyQualifiedName ?? string.Empty;
    }

    public static Type DeserializeType(string serializedType)
    {
        if (string.IsNullOrEmpty(serializedType))
            return typeof(object); // Default type

        try
        {
            var type = Type.GetType(serializedType);
            return type ?? typeof(object);
        }
        catch (Exception)
        {
            // Fallback to try parsing without assembly version
            try
            {
                var typeName = ExtractTypeName(serializedType);
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    var type = assembly.GetType(typeName);
                    if (type != null)
                        return type;
                }
            }
            catch (Exception)
            {
                // If all fails, return default
            }

            return typeof(object);
        }
    }

    private static string ExtractTypeName(string assemblyQualifiedName)
    {
        // Extract just the type name without assembly info
        var parts = assemblyQualifiedName.Split(',');
        return parts.Length > 0 ? parts[0].Trim() : assemblyQualifiedName;
    }

    public static string GetTypeDisplayName(Type type)
    {
        if (type == null)
            return "Object";

        // Return simplified name for UI
        if (type == typeof(ComparisonOperation)) return "Compare";
        if (type == typeof(float)) return "Float";
        if (type == typeof(int)) return "Integer";
        if (type == typeof(bool)) return "Boolean";
        if (type == typeof(string)) return "String";
        if (type == typeof(Vector3)) return "Vector3";
        if (type == typeof(GameObject)) return "GameObject";

        return type.Name;
    }
}
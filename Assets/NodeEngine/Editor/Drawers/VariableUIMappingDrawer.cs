using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(VariableUIRegistry.VariableUIMapping))]
public class VariableUIMappingDrawer : PropertyDrawer
{
    // Cache the available variable node types for the dropdown
    private static List<Type> _cachedTypes;
    private static string[] _cachedTypeDisplayNames;
    private static string[] _cachedTypeAssemblyNames;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // Find the nodeTypeName property
        var typeNameProp = property.FindPropertyRelative("nodeTypeName");
        var prefabProp = property.FindPropertyRelative("uiPrefab");

        // Calculate rects
        float lineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = EditorGUIUtility.standardVerticalSpacing;

        Rect typeRect = new Rect(position.x, position.y, position.width, lineHeight);
        Rect prefabRect = new Rect(position.x, position.y + lineHeight + spacing, position.width, lineHeight);

        // Refresh type cache if needed
        RefreshTypeCache();

        // Draw the dropdown for type selection
        string currentTypeName = typeNameProp.stringValue;
        int currentIndex = Array.IndexOf(_cachedTypeAssemblyNames, currentTypeName);
        if (currentIndex == -1 && !string.IsNullOrEmpty(currentTypeName))
        {
            // Type not in cache (maybe from a different assembly), show as disabled
            EditorGUI.LabelField(typeRect, "Node Type", "Unknown: " + currentTypeName);
        }
        else
        {
            int newIndex = EditorGUI.Popup(typeRect, "Node Type", currentIndex, _cachedTypeDisplayNames);
            if (newIndex >= 0 && newIndex < _cachedTypeAssemblyNames.Length)
            {
                typeNameProp.stringValue = _cachedTypeAssemblyNames[newIndex];
            }
        }

        // Draw the prefab field
        EditorGUI.PropertyField(prefabRect, prefabProp, new GUIContent("UI Prefab"));

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 2;
    }

    private static void RefreshTypeCache()
    {
        if (_cachedTypes != null) return;

        _cachedTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(asm => {
                try { return asm.GetTypes(); }
                catch { return new Type[0]; }
            })
            .Where(t => t.IsClass && !t.IsAbstract && IsSubclassOfRawGeneric(typeof(VariableNode<>), t))
            .OrderBy(t => t.Name)
            .ToList();

        _cachedTypeDisplayNames = _cachedTypes.Select(t => t.Name).ToArray();
        _cachedTypeAssemblyNames = _cachedTypes.Select(t => t.AssemblyQualifiedName).ToArray();
    }

    private static bool IsSubclassOfRawGeneric(Type generic, Type toCheck)
    {
        while (toCheck != null && toCheck != typeof(object))
        {
            var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
            if (generic == cur)
                return true;
            toCheck = toCheck.BaseType;
        }
        return false;
    }
}
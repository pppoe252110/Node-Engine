using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

[CreateAssetMenu(fileName = "VariableUIRegistry", menuName = "Node Engine/Variable UI Registry", order = 5)]
public class VariableUIRegistry : ScriptableObject
{
    [Serializable]
    public class VariableUIMapping
    {
        [Tooltip("The node type this UI element supports (e.g., FloatVariableNode, TypeVariableNode)")]
        public string nodeTypeName;

        [Tooltip("Prefab containing a VariableUIElement component")]
        public VariableUIElement uiPrefab;
    }

    [SerializeField] private List<VariableUIMapping> _mappings = new();

    private Dictionary<Type, VariableUIElement> _prefabCache;

    /// <summary>
    /// Returns the registered UI prefab for a given variable node instance.
    /// </summary>
    public VariableUIElement GetPrefabForNode(IVariableNode varNode)
    {
        if (varNode == null) return null;
        return GetPrefabForType(varNode.GetType());
    }

    /// <summary>
    /// Returns the registered UI prefab for a specific node type.
    /// </summary>
    public VariableUIElement GetPrefabForType(Type nodeType)
    {
        if (nodeType == null) return null;

        BuildCacheIfNeeded();
        return _prefabCache.TryGetValue(nodeType, out var prefab) ? prefab : null;
    }

    private void BuildCacheIfNeeded()
    {
        if (_prefabCache != null) return;

        _prefabCache = new Dictionary<Type, VariableUIElement>();
        foreach (var mapping in _mappings)
        {
            if (string.IsNullOrEmpty(mapping.nodeTypeName) || mapping.uiPrefab == null)
                continue;

            var type = Type.GetType(mapping.nodeTypeName);
            if (type != null)
                _prefabCache[type] = mapping.uiPrefab;
        }
    }

    /// <summary>
    /// Scans the assembly for all concrete VariableNode<T> types and adds missing entries to the mapping list.
    /// </summary>
    [Button("Auto Fill Types")]
    public void AutoFillVariableNodeTypes()
    {
#if UNITY_EDITOR
        UnityEditor.Undo.RecordObject(this, "Auto Fill Variable Node Types");
#endif

        // Find all non-abstract types that inherit from VariableNode<>
        var variableNodeTypes = Assembly.GetAssembly(typeof(VariableNode<>))
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && IsSubclassOfRawGeneric(typeof(VariableNode<>), t))
            .ToList();

        // Add entries for types not already present
        foreach (var type in variableNodeTypes)
        {
            string typeName = type.AssemblyQualifiedName;
            if (!_mappings.Any(m => m.nodeTypeName == typeName))
            {
                _mappings.Add(new VariableUIMapping
                {
                    nodeTypeName = typeName,
                    uiPrefab = null   // Assign prefab manually in inspector
                });
            }
        }

        // Sort alphabetically by type name for easier navigation
        _mappings = _mappings.OrderBy(m => m.nodeTypeName).ToList();

        // Invalidate cache so it rebuilds with new entries
        _prefabCache = null;

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    // Helper to check if a type inherits from a generic type definition
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

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Invalidate cache when mappings change in the Editor
        _prefabCache = null;
    }
#endif
}
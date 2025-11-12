using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static Codice.CM.Common.CmCallContext;

[CustomPropertyDrawer(typeof(SerializableNode))]
public class SerializableNodeDrawer : PropertyDrawer
{
    private const float LINE_HEIGHT = 18f;
    private const float PADDING = 4f;
    private const float HEADER_HEIGHT = 40f;
    private const float EXPANDED_HEIGHT = 80f;
    private const float ICON_SIZE = 24f;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        var nodeNameProp = property.FindPropertyRelative("nodeName");
        var nodeTypeProp = property.FindPropertyRelative("nodeType");
        var nodeIconProp = property.FindPropertyRelative("nodeIcon");

        // Calculate indent level manually to avoid EditorGUI.IndentedRect issues
        int indentLevel = GetIndentLevel(property);

        // Header rect
        var headerRect = new Rect(position.x, position.y, position.width, HEADER_HEIGHT);

        // Background for header
        var bgRect = new Rect(headerRect.x, headerRect.y,
                             headerRect.width, headerRect.height);
        EditorGUI.DrawRect(bgRect, new Color(0.2f, 0.2f, 0.2f, 0.8f));

        // Foldout (positioned at indent level)
        var foldoutRect = new Rect(headerRect.x + 16, headerRect.y + 11f, 15f, 15f);
        property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, GUIContent.none, true);

        // Icon
        var iconRect = new Rect(foldoutRect.xMax, headerRect.y + (HEADER_HEIGHT - ICON_SIZE) / 2,
                               ICON_SIZE, ICON_SIZE);
        if (nodeIconProp.objectReferenceValue != null)
        {
            Texture2D textureToDraw = null;

            if (nodeIconProp.objectReferenceValue is Sprite sprite)
            {
                textureToDraw = sprite.texture;
            }
            else if (nodeIconProp.objectReferenceValue is Texture2D texture2D)
            {
                textureToDraw = texture2D;
            }

            if (textureToDraw != null)
            {
                GUI.DrawTexture(iconRect, textureToDraw, ScaleMode.ScaleToFit, true, 1.0f);
            }
            else
            {
                EditorGUI.DrawRect(iconRect, new Color(0.3f, 0.3f, 0.3f, 0.5f));
            }
        }
        else
        {
            EditorGUI.DrawRect(iconRect, new Color(0.3f, 0.3f, 0.3f, 0.3f));
        }

        // Name and type labels
        float textStartX = iconRect.xMax;
        var nameRect = new Rect(textStartX + 4, headerRect.y + 6f,
                               headerRect.width - textStartX, 16f);

        string displayName = string.IsNullOrEmpty(nodeNameProp.stringValue) ?
                           "Unnamed Node" : nodeNameProp.stringValue;
        EditorGUI.LabelField(nameRect, displayName, EditorStyles.boldLabel);

        var typeRect = new Rect(textStartX + 4, nameRect.yMax,
                               headerRect.width - textStartX, 14f);
        string typeName = GetSimpleTypeName(nodeTypeProp.stringValue);
        EditorGUI.LabelField(typeRect, typeName, EditorStyles.miniLabel);

        // Expanded content
        if (property.isExpanded)
        {
            // Background for expanded content
            var expandedBgRect = new Rect(headerRect.x + 0,
                                         headerRect.yMax,
                                         headerRect.width,
                                         EXPANDED_HEIGHT);
            EditorGUI.DrawRect(expandedBgRect, new Color(0.18f, 0.18f, 0.18f, 0.7f));

            float currentY = headerRect.yMax + PADDING;
            float fieldStartX = headerRect.x + PADDING + 16;
            float fieldWidth = headerRect.width - PADDING * 2 - 24;

            // Name field
            var nameFieldRect = new Rect(fieldStartX, currentY, fieldWidth, LINE_HEIGHT);
            EditorGUI.PropertyField(nameFieldRect, nodeNameProp, new GUIContent("Name"));
            currentY += LINE_HEIGHT + 2f;

            // Type dropdown
            var typeFieldRect = new Rect(fieldStartX, currentY, fieldWidth, LINE_HEIGHT);
            DrawTypeDropdown(typeFieldRect, nodeTypeProp);
            currentY += LINE_HEIGHT + 2f;

            // Icon field
            var iconFieldRect = new Rect(fieldStartX, currentY, fieldWidth, LINE_HEIGHT);
            EditorGUI.PropertyField(iconFieldRect, nodeIconProp, new GUIContent("Icon"));
            currentY += LINE_HEIGHT + 2f;
        }

        EditorGUI.EndProperty();
    }
    
    private int GetIndentLevel(SerializedProperty property)
    {
        // Count the number of parent properties to get proper indent level
        string path = property.propertyPath;
        int count = 0;
        for (int i = 0; i < path.Length; i++)
        {
            if (path[i] == '.')
                count++;
        }
        return count;
    }

    private void DrawTypeDropdown(Rect position, SerializedProperty typeProperty)
    {
        var currentType = typeProperty.stringValue;
        var nodeTypes = GetNodeTypes();

        var options = new List<GUIContent> { new GUIContent("Select Type...") };
        var typeValues = new List<string> { string.Empty };

        foreach (var typeName in nodeTypes)
        {
            var simpleName = GetSimpleTypeName(typeName);
            options.Add(new GUIContent(simpleName));
            typeValues.Add(typeName);
        }

        int currentIndex = typeValues.IndexOf(currentType);
        if (currentIndex == -1) currentIndex = 0;

        EditorGUI.BeginChangeCheck();
        int newIndex = EditorGUI.Popup(position, currentIndex, options.ToArray());

        if (EditorGUI.EndChangeCheck() && newIndex >= 0 && newIndex < typeValues.Count)
        {
            typeProperty.stringValue = typeValues[newIndex];
        }
    }

    private List<string> _cachedNodeTypes;
    private double _lastCacheTime;
    private const double CACHE_DURATION = 2.0;

    private List<string> GetNodeTypes()
    {
        if (_cachedNodeTypes != null && EditorApplication.timeSinceStartup - _lastCacheTime < CACHE_DURATION)
        {
            return _cachedNodeTypes;
        }

        _cachedNodeTypes = new List<string>();

        foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
        {
            try
            {
                var types = assembly.GetTypes()
                    .Where(t => t.IsSubclassOf(typeof(NodeBase)) && !t.IsAbstract && !t.IsGenericType)
                    .Select(t => t.AssemblyQualifiedName);

                _cachedNodeTypes.AddRange(types);
            }
            catch (System.Reflection.ReflectionTypeLoadException)
            {
                continue;
            }
        }

        _cachedNodeTypes.Sort();
        _lastCacheTime = EditorApplication.timeSinceStartup;

        return _cachedNodeTypes;
    }

    private string GetSimpleTypeName(string assemblyQualifiedName)
    {
        if (string.IsNullOrEmpty(assemblyQualifiedName))
            return "None";

        var parts = assemblyQualifiedName.Split(',');
        var typeName = parts[0];
        var lastDot = typeName.LastIndexOf('.');
        return lastDot >= 0 ? typeName.Substring(lastDot + 1) : typeName;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return property.isExpanded ? HEADER_HEIGHT + EXPANDED_HEIGHT + PADDING : HEADER_HEIGHT;
    }
}
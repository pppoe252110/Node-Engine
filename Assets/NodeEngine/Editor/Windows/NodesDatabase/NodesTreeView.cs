using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

public class NodesTreeView : TreeView<int>
{
    public enum SortMode { Original, NameAscending, NameDescending }
    public SortMode CurrentSortMode = SortMode.Original;

    private readonly SerializedProperty _nodesProperty;

    public NodesTreeView(TreeViewState<int> state, SerializedProperty nodesProperty) : base(state)
    {
        _nodesProperty = nodesProperty;
        showAlternatingRowBackgrounds = true;
        showBorder = true;
        Reload();
    }

    protected override TreeViewItem<int> BuildRoot()
    {
        var root = new TreeViewItem<int> { id = -1, depth = -1, displayName = "Root" };
        var folderDict = new Dictionary<string, TreeViewItem<int>>();

        for (int i = 0; i < _nodesProperty.arraySize; i++)
        {
            var nodeProp = _nodesProperty.GetArrayElementAtIndex(i);
            string typeName = nodeProp.FindPropertyRelative("nodeType").stringValue;
            string customName = nodeProp.FindPropertyRelative("nodeName").stringValue;

            string displayName = string.IsNullOrWhiteSpace(customName)
                ? GetFallbackNameFromType(typeName)
                : customName;

            string path = GetPathFromType(typeName, displayName);
            string[] pathParts = path.Split('/');

            TreeViewItem<int> parentItem = root;
            string currentPath = "";

            for (int p = 0; p < pathParts.Length - 1; p++)
            {
                currentPath += (currentPath == "" ? "" : "/") + pathParts[p];

                if (!folderDict.TryGetValue(currentPath, out TreeViewItem<int> folderItem))
                {
                    folderItem = new TreeViewItem<int>
                    {
                        id = currentPath.GetHashCode(),
                        depth = p,
                        displayName = pathParts[p]
                    };
                    parentItem.AddChild(folderItem);
                    folderDict.Add(currentPath, folderItem);
                }
                parentItem = folderItem;
            }

            var leafItem = new TreeViewItem<int>
            {
                id = i,
                depth = pathParts.Length - 1,
                displayName = displayName,
                icon = GetIconFromProperty(nodeProp)
            };
            parentItem.AddChild(leafItem);
        }

        SortChildrenRecursive(root);
        SetupDepthsFromParentsAndChildren(root);
        return root;
    }

    private string GetFallbackNameFromType(string assemblyQualifiedName)
    {
        if (string.IsNullOrEmpty(assemblyQualifiedName)) return "<Unnamed>";
        Type type = Type.GetType(assemblyQualifiedName);
        return type != null ? type.Name : "<Missing>";
    }

    private void SortChildrenRecursive(TreeViewItem<int> item)
    {
        if (!item.hasChildren) return;

        if (CurrentSortMode == SortMode.NameAscending)
        {
            item.children = item.children
                .OrderByDescending(c => c.hasChildren)
                .ThenBy(c => c.displayName).ToList();
        }
        else if (CurrentSortMode == SortMode.NameDescending)
        {
            item.children = item.children
                .OrderByDescending(c => c.hasChildren)
                .ThenByDescending(c => c.displayName).ToList();
        }

        foreach (var child in item.children)
            SortChildrenRecursive(child);
    }

    protected override void RowGUI(RowGUIArgs args)
    {
        var item = args.item;
        Rect rowRect = args.rowRect;

        Rect contentRect = rowRect;
        contentRect.x += GetContentIndent(item);
        contentRect.width -= GetContentIndent(item);

        if (item.icon != null)
        {
            Rect iconRect = new Rect(contentRect.x, contentRect.y, 16f, 16f);
            GUI.DrawTexture(iconRect, item.icon, ScaleMode.ScaleToFit);
        }

        Rect textRect = contentRect;
        textRect.x += 18f;
        textRect.width -= 18f;

        GUIStyle labelStyle = item.hasChildren ? EditorStyles.boldLabel : EditorStyles.label;
        EditorGUI.LabelField(textRect, item.displayName, labelStyle);
    }

    public TreeViewItem<int> GetItem(int id) => FindItem(id, rootItem);

    private string GetPathFromType(string assemblyQualifiedName, string fallbackName)
    {
        if (string.IsNullOrEmpty(assemblyQualifiedName))
            return $"Unassigned/{fallbackName}";

        Type type = Type.GetType(assemblyQualifiedName);
        if (type == null)
            return $"Missing Script/{fallbackName}";

        var attribute = type.GetCustomAttributes(true)
            .FirstOrDefault(a => a.GetType().Name == "NodePathAttribute");

        if (attribute != null)
        {
            var pathProp = attribute.GetType().GetProperty("Path",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                ?? attribute.GetType().GetProperty("path",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (pathProp != null)
                return pathProp.GetValue(attribute) as string;

            var pathField = attribute.GetType().GetField("Path",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                ?? attribute.GetType().GetField("path",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (pathField != null)
                return pathField.GetValue(attribute) as string;
        }

        return $"Uncategorized/{fallbackName}";
    }

    private Texture2D GetIconFromProperty(SerializedProperty nodeProp)
    {
        var iconProp = nodeProp.FindPropertyRelative("nodeIcon");
        if (iconProp.objectReferenceValue != null)
        {
            if (iconProp.objectReferenceValue is Sprite sprite)
                return sprite.texture;
            if (iconProp.objectReferenceValue is Texture2D tex)
                return tex;
        }
        return null;
    }
}
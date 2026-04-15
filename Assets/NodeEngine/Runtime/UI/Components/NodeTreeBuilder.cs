using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class NodeTreeBuilder
{
    private class NodeGroup
    {
        public string Name;
        public NodeGroup Parent;
        public Dictionary<string, NodeGroup> Children = new();
        public List<(NodesListItem item, int originalIndex)> Items = new();
        public RectTransform UIContainer;
        public NodesListGroup UIHeader;

        public string GetFullPath()
        {
            if (Parent == null) return Name;
            var parentPath = Parent.GetFullPath();
            return string.IsNullOrEmpty(parentPath) ? Name : $"{parentPath}/{Name}";
        }
    }
    public class ExternalNodeEntry
    {
        public string CategoryPath;
        public string DisplayName;
        public object UserData;
    }

    private NodeGroup _rootGroup = new() { Name = "Root" };
    private Dictionary<string, NodesListGroup> _groupUIElements = new();
    private NodesListGroup _groupPrefab;
    private NodesListItem _itemPrefab;
    private RectTransform _parent;
    private NodesDatabase _database;

    public NodeTreeBuilder(NodesListGroup groupPrefab, NodesListItem itemPrefab, RectTransform parent, NodesDatabase database)
    {
        _groupPrefab = groupPrefab;
        _itemPrefab = itemPrefab;
        _parent = parent;
        _database = database;
    }

    public void BuildTree(
        Action<NodesListItem, int> onDatabaseItemCreated,
        IEnumerable<ExternalNodeEntry> externalEntries = null,
        Action<NodesListItem, object> onExternalItemCreated = null)
    {
        Clear();

        // Process database nodes
        var allNodeData = _database.GetAllNodeData().ToList();
        for (int i = 0; i < allNodeData.Count; i++)
        {
            var nodeData = allNodeData[i];
            var (path, itemName) = GetNodePathAndName(nodeData);
            var group = EnsureGroupExists(path);

            var item = UnityEngine.Object.Instantiate(_itemPrefab, group.UIContainer ?? _parent);
            item.SetNodeName(itemName);
            onDatabaseItemCreated?.Invoke(item, i);
            group.Items.Add((item, i));
        }

        // Process external entries (subgraphs)
        if (externalEntries != null)
        {
            foreach (var entry in externalEntries)
            {
                var group = EnsureGroupExists(entry.CategoryPath);
                var item = UnityEngine.Object.Instantiate(_itemPrefab, group.UIContainer ?? _parent);
                item.SetNodeName(entry.DisplayName);
                onExternalItemCreated?.Invoke(item, entry.UserData);
                group.Items.Add((item, -1)); // -1 marker
            }
        }

        CreateGroupUI(_rootGroup, _parent, 0);
    }

    public void AddExternalItems(IEnumerable<(string categoryPath, string displayName, object userData)> items, Action<NodesListItem, object> onItemCreated)
    {
        foreach (var (path, name, userData) in items)
        {
            var group = EnsureGroupExists(path);
            var item = UnityEngine.Object.Instantiate(_itemPrefab, group.UIContainer ?? _parent);
            item.SetNodeName(name);
            onItemCreated?.Invoke(item, userData);
            group.Items.Add((item, -1)); // -1 indicates not from database
        }
    }

    public void Clear()
    {
        _rootGroup = new NodeGroup { Name = "Root" };
        _groupUIElements.Clear();
        foreach (Transform child in _parent)
            UnityEngine.Object.Destroy(child.gameObject);
    }

    public void ApplySearchFilter(string searchText, out HashSet<NodesListItem> visibleItems)
    {
        visibleItems = new HashSet<NodesListItem>();

        foreach (var group in GetAllGroups())
        {
            foreach (var (item, _) in group.Items)
            {
                bool shouldShow = string.IsNullOrEmpty(searchText) ||
                    item.nodeName.text.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0;
                item.gameObject.SetActive(shouldShow);
                if (shouldShow) visibleItems.Add(item);
            }
        }

        UpdateGroupVisibility(_rootGroup, visibleItems);
    }

    private bool UpdateGroupVisibility(NodeGroup group, HashSet<NodesListItem> visibleItems)
    {
        bool hasVisibleContent = false;

        foreach (var (item, _) in group.Items)
            if (visibleItems.Contains(item)) hasVisibleContent = true;

        foreach (var child in group.Children.Values)
            if (UpdateGroupVisibility(child, visibleItems)) hasVisibleContent = true;

        if (group.UIHeader != null)
        {
            group.UIHeader.gameObject.SetActive(hasVisibleContent);
            if (group.UIContainer != null)
                group.UIContainer.gameObject.SetActive(hasVisibleContent && group.UIHeader.IsExpanded);
        }

        return hasVisibleContent;
    }

    private IEnumerable<NodeGroup> GetAllGroups()
    {
        var stack = new Stack<NodeGroup>();
        stack.Push(_rootGroup);
        while (stack.Count > 0)
        {
            var current = stack.Pop();
            yield return current;
            foreach (var child in current.Children.Values)
                stack.Push(child);
        }
    }

    private (string path, string itemName) GetNodePathAndName(SerializableNode nodeData)
    {
        Type nodeType = Type.GetType(nodeData.nodeType);
        if (nodeType == null) return ("Other", nodeData.nodeName);

        var pathAttr = nodeType.GetCustomAttributes(typeof(NodePathAttribute), false)
                              .FirstOrDefault() as NodePathAttribute;

        if (pathAttr != null && !string.IsNullOrEmpty(pathAttr.Path))
        {
            var path = pathAttr.Path;
            var lastSlash = path.LastIndexOf('/');
            return lastSlash >= 0
                ? (path.Substring(0, lastSlash), nodeData.nodeName)
                : ("", nodeData.nodeName);
        }

        return ("Other", nodeData.nodeName);
    }

    private NodeGroup EnsureGroupExists(string path)
    {
        if (string.IsNullOrEmpty(path)) return _rootGroup;

        var parts = path.Split('/');
        var current = _rootGroup;
        foreach (var part in parts)
        {
            if (!current.Children.TryGetValue(part, out var child))
            {
                child = new NodeGroup { Name = part, Parent = current };
                current.Children[part] = child;
            }
            current = child;
        }
        return current;
    }

    private void CreateGroupUI(NodeGroup group, RectTransform parent, int indentLevel)
    {
        if (group != _rootGroup)
        {
            var groupUI = UnityEngine.Object.Instantiate(_groupPrefab, parent);
            groupUI.SetGroupName(group.Name);
            groupUI.SetIndent(indentLevel);
            groupUI.OnExpansionChanged += (g, expanded) =>
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(parent);
                LayoutRebuilder.ForceRebuildLayoutImmediate(_parent);
            };

            group.UIContainer = groupUI.Container;
            group.UIHeader = groupUI;
            _groupUIElements[group.GetFullPath()] = groupUI;
            indentLevel++;
        }

        foreach (var child in group.Children.Values.OrderBy(c => c.Name))
            CreateGroupUI(child, group.UIContainer ?? parent, indentLevel);

        foreach (var item in group.Items.OrderBy(i => i.item.nodeName.text))
        {
            item.item.transform.SetParent(group.UIContainer ?? parent);
            item.item.SetIndent(indentLevel - 2);
        }
    }
}
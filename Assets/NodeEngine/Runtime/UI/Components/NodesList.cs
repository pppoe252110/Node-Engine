using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using VContainer;

public class NodesList : MonoBehaviour
{
    [SerializeField] private NodeLogic _nodeLogicPrefab;
    [SerializeField] private RectTransform _nodesListView;
    [SerializeField] private RectTransform _nodesListParent;
    [SerializeField] private NodesListItem _nodesListItem;
    [SerializeField] private NodesListGroup _nodesListGroupPrefab;

    [Header("Search")]
    [SerializeField] private TMP_InputField _searchInputField;

    // Tree structure for nested groups
    private NodeGroup _rootGroup = new NodeGroup { Name = "Root" };
    private Dictionary<string, NodesListGroup> _groupUIElements = new();
    private List<NodesListItem> _allItems = new();

    private string _currentSearch = "";

    // Dependencies
    private INodeFactory _nodeFactory;
    private NodeSpawnerService _nodeSpawnerService;
    private NodesDatabase _nodesDatabase;

    [Inject]
    public void Construct(INodeFactory nodeFactory, NodeSpawnerService nodeSpawner, NodesDatabase nodesDatabase)
    {
        _nodeFactory = nodeFactory;
        _nodeSpawnerService = nodeSpawner;
        _nodesDatabase = nodesDatabase;
    }

    private void Start()
    {
        _nodesListView.gameObject.SetActive(false);

        if (_searchInputField != null)
            _searchInputField.onValueChanged.AddListener(OnSearchValueChanged);

        BuildNodeList();
    }

    private void Update()
    {
        if (Keyboard.current.spaceKey.wasReleasedThisFrame)
        {
            _nodesListView.gameObject.SetActive(!_nodesListView.gameObject.activeSelf);
            _nodesListView.position = Mouse.current.position.value;
            _searchInputField.ActivateInputField();

            if (_nodesListView.gameObject.activeSelf)
                ClearSearch();
        }
    }

    public void BuildNodeList()
    {
        var allNodeData = _nodesDatabase.GetAllNodeData().ToList();

        // Clear existing
        _rootGroup = new NodeGroup { Name = "Root" };
        _groupUIElements.Clear();
        _allItems.Clear();

        foreach (Transform child in _nodesListParent)
            Destroy(child.gameObject);

        // Build the tree structure
        for (int i = 0; i < allNodeData.Count; i++)
        {
            var nodeData = allNodeData[i];
            var (path, itemName) = GetNodePathAndName(nodeData);

            // Add to tree
            var group = EnsureGroupExists(path);

            var item = Instantiate(_nodesListItem, group.UIContainer ?? _nodesListParent);
            item.SetUp(this, i);
            item.SetNodeName(itemName);

            group.Items.Add((item, i));
            _allItems.Add(item);
        }

        // Create UI for all groups
        CreateGroupUI(_rootGroup, _nodesListParent, 0);

        ApplySearchFilter();
    }

    private (string path, string itemName) GetNodePathAndName(SerializableNode nodeData)
    {
        Type nodeType = Type.GetType(nodeData.nodeType);
        if (nodeType == null)
            return ("Other", nodeData.nodeName);

        var pathAttr = nodeType.GetCustomAttributes(typeof(NodePathAttribute), false)
                              .FirstOrDefault() as NodePathAttribute;

        if (pathAttr != null && !string.IsNullOrEmpty(pathAttr.Path))
        {
            var path = pathAttr.Path;
            var lastSlash = path.LastIndexOf('/');

            if (lastSlash >= 0)
            {
                string groupPath = path.Substring(0, lastSlash);
                return (groupPath, nodeData.nodeName);
            }
            else
            {
                return ("", nodeData.nodeName); // Root level
            }
        }

        return ("Other", nodeData.nodeName);
    }

    private NodeGroup EnsureGroupExists(string path)
    {
        if (string.IsNullOrEmpty(path))
            return _rootGroup;

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

    private void CreateGroupUI(NodeGroup group, Transform parent, int indentLevel)
    {
        if (group != _rootGroup)
        {
            var groupUI = Instantiate(_nodesListGroupPrefab, parent);
            groupUI.SetGroupName(group.Name);
            groupUI.SetIndent(indentLevel); // indentLevel for root groups = 0
            groupUI.OnExpansionChanged += OnGroupExpansionChanged;

            group.UIContainer = groupUI.Container;
            group.UIHeader = groupUI;
            _groupUIElements[group.GetFullPath()] = groupUI;

            // Items inside this group should be indented one more level
            indentLevel = indentLevel + 1;
        }

        // Create child groups
        foreach (var child in group.Children.Values.OrderBy(c => c.Name))
        {
            CreateGroupUI(child, group.UIContainer ?? parent, indentLevel);
        }

        // Add items directly in this group with proper indentation
        foreach (var item in group.Items.OrderBy(i => i.item.nodeName.text))
        {
            item.item.transform.SetParent(group.UIContainer ?? parent);
            item.item.SetIndent(indentLevel-2); // Now indentLevel is correct for items
        }
    }

    private void OnGroupExpansionChanged(NodesListGroup group, bool isExpanded)
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(_nodesListView);
    }

    private void OnSearchValueChanged(string searchText)
    {
        _currentSearch = searchText.Trim();
        ApplySearchFilter();
    }

    private void ApplySearchFilter()
    {
        // First, determine which items are visible
        var visibleItems = new HashSet<NodesListItem>();

        foreach (var item in _allItems)
        {
            if (item == null) continue;

            bool shouldShow = string.IsNullOrEmpty(_currentSearch) ||
                              item.nodeName.text.IndexOf(_currentSearch, StringComparison.OrdinalIgnoreCase) >= 0;

            item.gameObject.SetActive(shouldShow);
            if (shouldShow)
                visibleItems.Add(item);
        }

        // Then, show/hide groups based on whether they have visible children
        UpdateGroupVisibility(_rootGroup, visibleItems);
        LayoutRebuilder.MarkLayoutForRebuild(_nodesListParent);
    }

    private bool UpdateGroupVisibility(NodeGroup group, HashSet<NodesListItem> visibleItems)
    {
        bool hasVisibleContent = false;

        // Check direct items
        foreach (var (item, _) in group.Items)
        {
            if (visibleItems.Contains(item))
                hasVisibleContent = true;
        }

        // Check children
        foreach (var child in group.Children.Values)
        {
            if (UpdateGroupVisibility(child, visibleItems))
                hasVisibleContent = true;
        }

        // Update UI
        if (group.UIHeader != null)
        {
            group.UIHeader.gameObject.SetActive(hasVisibleContent);
            if (group.UIContainer != null)
                group.UIContainer.gameObject.SetActive(hasVisibleContent && group.UIHeader.IsExpanded);
        }

        return hasVisibleContent;
    }

    private void ClearSearch()
    {
        if (_searchInputField != null)
            _searchInputField.text = "";
        _currentSearch = "";
        ApplySearchFilter();
    }

    internal void SpawnNodeFromOriginalIndex(int originalIndex)
    {
        var allNodeData = _nodesDatabase.GetAllNodeData().ToList();
        if (originalIndex < 0 || originalIndex >= allNodeData.Count)
            return;

        var nodeData = allNodeData[originalIndex];
        Type nodeType = Type.GetType(nodeData.nodeType);
        if (nodeType == null)
        {
            Debug.LogError($"Cannot resolve type: {nodeData.nodeType}");
            return;
        }

        BaseNode nodeInstance = _nodeFactory.CreateNode(nodeType);
        _nodesDatabase.ApplyMetadata(nodeInstance);
        _nodeSpawnerService.SpawnNode(nodeInstance, _nodesListView.localPosition);
        _nodesListView.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        foreach (var group in _groupUIElements.Values)
        {
            if (group != null)
                group.OnExpansionChanged -= OnGroupExpansionChanged;
        }
    }

    // Helper class for tree structure
    private class NodeGroup
    {
        public string Name;
        public NodeGroup Parent;
        public Dictionary<string, NodeGroup> Children = new();
        public List<(NodesListItem item, int originalIndex)> Items = new();
        public Transform UIContainer;
        public NodesListGroup UIHeader;

        public string GetFullPath()
        {
            if (Parent == null || Parent == this) return Name;
            var parentPath = Parent.GetFullPath();
            return string.IsNullOrEmpty(parentPath) ? Name : $"{parentPath}/{Name}";
        }
    }
}
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
    [SerializeField] private Transform _nodesListParent;
    [SerializeField] private NodesListItem _nodesListItem;
    [SerializeField] private NodesListGroup _nodesListGroupPrefab;
    [SerializeField] private NodesDatabase _nodesDatabase;

    [Header("Search")]
    [SerializeField] private TMP_InputField _searchInputField;

    private Dictionary<string, List<(NodesListItem item, int originalIndex)>> _groupedItems = new();
    private Dictionary<string, NodesListGroup> _groupHeaders = new();
    private Dictionary<string, RectTransform> _groupContainers = new();
    private string _currentSearch = "";

    // Dependencies
    private INodeFactory _nodeFactory;
    private NodeSpawnerService _nodeSpawnerService;

    [Inject]
    public void Construct(INodeFactory nodeFactory, NodeSpawnerService nodeSpawner)
    {
        _nodeFactory = nodeFactory;
        _nodeSpawnerService = nodeSpawner;
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

    /// <summary>
    /// Builds the UI list using SerializableNode data from the database.
    /// </summary>
    public void BuildNodeList()
    {
        var allNodeData = _nodesDatabase.GetAllNodeData().ToList();
        _groupedItems.Clear();
        _groupHeaders.Clear();
        _groupContainers.Clear();

        // Clear existing UI
        foreach (Transform child in _nodesListParent)
            Destroy(child.gameObject);

        for (int i = 0; i < allNodeData.Count; i++)
        {
            var nodeData = allNodeData[i];
            var (groupName, itemName) = GetNodeGroupAndName(nodeData);

            if (!_groupedItems.ContainsKey(groupName))
            {
                _groupedItems[groupName] = new List<(NodesListItem item, int originalIndex)>();
                CreateGroupHeader(groupName);
            }

            var item = Instantiate(_nodesListItem, _nodesListParent);
            item.SetUp(this, i);
            item.SetNodeName(itemName);

            _groupedItems[groupName].Add((item, originalIndex: i));

            if (_groupContainers.TryGetValue(groupName, out var container))
                item.transform.SetParent(container);
        }

        var sortedGroups = _groupedItems.OrderBy(g => g.Key).ToList();
        ReorganizeHierarchy(sortedGroups);
        ApplySearchFilter();
    }

    private (string groupName, string itemName) GetNodeGroupAndName(SerializableNode nodeData)
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
                return (path.Substring(0, lastSlash), nodeData.nodeName);
            else
                return ("Other", nodeData.nodeName);
        }

        return ("Other", nodeData.nodeName);
    }

    private void CreateGroupHeader(string groupName)
    {
        var groupUI = Instantiate(_nodesListGroupPrefab, _nodesListParent);
        groupUI.SetGroupName(groupName);
        groupUI.OnExpansionChanged += OnGroupExpansionChanged;

        _groupHeaders[groupName] = groupUI;
        _groupContainers[groupName] = groupUI.Container;
    }

    private void OnGroupExpansionChanged(NodesListGroup group, bool isExpanded)
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(_nodesListView);
    }

    private void ReorganizeHierarchy(List<KeyValuePair<string, List<(NodesListItem item, int originalIndex)>>> sortedGroups)
    {
        foreach (var group in sortedGroups)
        {
            var groupHeader = _groupHeaders[group.Key];
            var groupContainer = _groupContainers[group.Key];

            groupHeader.transform.SetAsLastSibling();
            groupContainer.transform.SetAsLastSibling();

            var sortedItems = group.Value.OrderBy(x => x.item.nodeName.text).ToList();
            foreach (var tuple in sortedItems)
                tuple.item.transform.SetParent(groupContainer);
        }
    }

    private void OnSearchValueChanged(string searchText)
    {
        _currentSearch = searchText.Trim();
        ApplySearchFilter();
    }

    private void ApplySearchFilter()
    {
        foreach (var group in _groupedItems)
        {
            bool hasVisibleItemsInGroup = false;

            foreach (var tuple in group.Value)
            {
                if (tuple.item == null) continue;

                bool shouldShow = string.IsNullOrEmpty(_currentSearch) ||
                                  tuple.item.nodeName.text.IndexOf(_currentSearch, StringComparison.OrdinalIgnoreCase) >= 0;

                tuple.item.gameObject.SetActive(shouldShow);
                if (shouldShow) hasVisibleItemsInGroup = true;
            }

            if (_groupHeaders.TryGetValue(group.Key, out var header))
            {
                bool showGroup = hasVisibleItemsInGroup;
                header.gameObject.SetActive(showGroup);
                if (_groupContainers.TryGetValue(group.Key, out var container))
                    container.gameObject.SetActive(showGroup && header.IsExpanded);
            }
        }
    }

    private void ClearSearch()
    {
        if (_searchInputField != null)
            _searchInputField.text = "";
        _currentSearch = "";
        ApplySearchFilter();
    }

    /// <summary>
    /// Called when a node is selected from the list.
    /// Creates a new node instance via factory, applies metadata, and spawns it.
    /// </summary>
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

        // Create a fresh node instance
        BaseNode nodeInstance = _nodeFactory.CreateNode(nodeType);

        // Apply name and icon from the database
        _nodesDatabase.ApplyMetadata(nodeInstance);

        // Spawn the visual node at the list's current position
        _nodeSpawnerService.SpawnNode(nodeInstance, _nodesListView.localPosition);

        _nodesListView.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        foreach (var group in _groupHeaders.Values)
        {
            if (group != null)
                group.OnExpansionChanged -= OnGroupExpansionChanged;
        }
    }
}
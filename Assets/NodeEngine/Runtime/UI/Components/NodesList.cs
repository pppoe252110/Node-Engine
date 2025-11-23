using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class NodesList : MonoBehaviour
{
    [SerializeField] private NodeLogic _nodeLogicPrefab;
    [SerializeField] private RectTransform _nodesListView;
    [SerializeField] private Transform _nodesListParent;
    [SerializeField] private NodesListItem _nodesListItem;
    [SerializeField] private NodesListGroup _nodesListGroupPrefab;
    [SerializeField] private NodesDatabase _nodesDatabase;
    [SerializeField] private VariableDatabase _variablesDatabase;

    [Header("Search")]
    [SerializeField] private TMP_InputField _searchInputField;

    private Dictionary<string, List<(NodesListItem item, int originalIndex)>> _groupedItems = new Dictionary<string, List<(NodesListItem item, int originalIndex)>>();
    private Dictionary<string, NodesListGroup> _groupHeaders = new Dictionary<string, NodesListGroup>();
    private Dictionary<string, RectTransform> _groupContainers = new Dictionary<string, RectTransform>();
    private string _currentSearch = "";

    private void Start()
    {
        _nodesListView.gameObject.SetActive(false);

        if (_searchInputField != null)
        {
            _searchInputField.onValueChanged.AddListener(OnSearchValueChanged);
        }

        SpawnNodes();
    }

    private void Update()
    {
        if (Keyboard.current.spaceKey.wasReleasedThisFrame)
        {
            _nodesListView.gameObject.SetActive(!_nodesListView.gameObject.activeSelf);
            _nodesListView.position = Mouse.current.position.value;
            _searchInputField.ActivateInputField();

            if (_nodesListView.gameObject.activeSelf)
            {
                ClearSearch();
            }
        }
    }

    public void SpawnNodes()
    {
        var nodes = _nodesDatabase.GetNodes();
        _groupedItems.Clear();
        _groupHeaders.Clear();
        _groupContainers.Clear();

        // Clear existing items
        foreach (Transform child in _nodesListParent)
        {
            Destroy(child.gameObject);
        }

        // Group nodes by their path
        for (int i = 0; i < nodes.Length; i++)
        {
            var node = nodes[i];
            var (groupName, itemName) = GetNodeGroupAndName(node);

            if (!_groupedItems.ContainsKey(groupName))
            {
                _groupedItems[groupName] = new List<(NodesListItem item, int originalIndex)>();
                CreateGroupHeader(groupName);
            }

            var item = Instantiate(_nodesListItem, _nodesListParent);
            item.SetUp(this, i);
            item.SetNodeName(itemName);

            _groupedItems[groupName].Add((item: item, originalIndex: i));

            // Register item with group container
            if (_groupContainers.ContainsKey(groupName))
            {
                item.transform.SetParent(_groupContainers[groupName]);
            }
        }

        // Sort groups alphabetically
        var sortedGroups = _groupedItems.OrderBy(g => g.Key).ToList();

        // Reorganize hierarchy to maintain group->item structure
        ReorganizeHierarchy(sortedGroups);

        ApplySearchFilter();
    }

    private (string groupName, string itemName) GetNodeGroupAndName(NodeBase node)
    {
        var nodeType = node.GetType();
        var pathAttribute = nodeType.GetCustomAttributes(typeof(NodePathAttribute), false)
                                  .FirstOrDefault() as NodePathAttribute;

        // Always use the NodeName from the database for the item display name
        string itemName = node.NodeName;

        if (pathAttribute != null && !string.IsNullOrEmpty(pathAttribute.Path))
        {
            var path = pathAttribute.Path;
            var lastSlash = path.LastIndexOf('/');

            if (lastSlash >= 0)
            {
                // Split into group and item name
                string groupName = path.Substring(0, lastSlash);
                return (groupName, itemName);
            }
            else
            {
                // No slash - use as item name, group is "Other"
                return ("Other", itemName);
            }
        }
        else
        {
            // No path attribute - use the NodeName property directly
            return ("Other", itemName);
        }
    }

    private void CreateGroupHeader(string groupName)
    {
        // Create group header
        var groupUI = Instantiate(_nodesListGroupPrefab, _nodesListParent);
        groupUI.SetGroupName(groupName);
        groupUI.OnExpansionChanged += OnGroupExpansionChanged;

        _groupHeaders[groupName] = groupUI;
        _groupContainers[groupName] = groupUI.Container;
    }

    private void OnGroupExpansionChanged(NodesListGroup group, bool isExpanded)
    {
        // The group already handles its own items visibility
        // We just need to ensure the layout updates properly
        LayoutRebuilder.ForceRebuildLayoutImmediate(_nodesListView);
    }

    private void ReorganizeHierarchy(List<KeyValuePair<string, List<(NodesListItem item, int originalIndex)>>> sortedGroups)
    {
        foreach (var group in sortedGroups)
        {
            var groupHeader = _groupHeaders[group.Key];
            var groupContainer = _groupContainers[group.Key];

            // Set hierarchy order: Header -> Container
            groupHeader.transform.SetAsLastSibling();
            groupContainer.transform.SetAsLastSibling();

            // Sort items within group alphabetically
            var sortedItems = group.Value.OrderBy(x => x.item.nodeName.text).ToList();

            foreach (var tuple in sortedItems)
            {
                tuple.item.transform.SetParent(groupContainer);
            }
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

                if (shouldShow)
                {
                    hasVisibleItemsInGroup = true;
                }
            }

            // Show/hide group header based on whether it has visible items
            if (_groupHeaders.ContainsKey(group.Key))
            {
                bool shouldShowGroup = hasVisibleItemsInGroup;
                _groupHeaders[group.Key].gameObject.SetActive(shouldShowGroup);

                // Also show/hide the container
                if (_groupContainers.ContainsKey(group.Key))
                {
                    _groupContainers[group.Key].gameObject.SetActive(shouldShowGroup && _groupHeaders[group.Key].IsExpanded);
                }
            }
        }
    }

    private void ClearSearch()
    {
        if (_searchInputField != null)
        {
            _searchInputField.text = "";
        }
        _currentSearch = "";
        ApplySearchFilter();
    }

    internal void SpawnNodeFromOriginalIndex(int originalIndex)
    {
        var nodes = _nodesDatabase.GetNodes();

        if (originalIndex >= 0 && originalIndex < nodes.Length)
        {
            var targetNode = nodes[originalIndex];
            var nodeLogic = Instantiate(_nodeLogicPrefab, UIZoomPan.NodesParent);
            nodeLogic.transform.position = _nodesListView.position;
            nodeLogic.VariableDatabase = _variablesDatabase;
            nodeLogic.SetNodeBase(targetNode);
        }

        _nodesListView.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        // Clean up event subscriptions
        foreach (var group in _groupHeaders.Values)
        {
            if (group != null)
            {
                group.OnExpansionChanged -= OnGroupExpansionChanged;
            }
        }
    }
}
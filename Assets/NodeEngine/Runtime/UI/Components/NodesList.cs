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


        foreach (Transform child in _nodesListParent)
        {
            Destroy(child.gameObject);
        }


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


            if (_groupContainers.ContainsKey(groupName))
            {
                item.transform.SetParent(_groupContainers[groupName]);
            }
        }


        var sortedGroups = _groupedItems.OrderBy(g => g.Key).ToList();


        ReorganizeHierarchy(sortedGroups);

        ApplySearchFilter();
    }

    private (string groupName, string itemName) GetNodeGroupAndName(NodeBase node)
    {
        var nodeType = node.GetType();
        var pathAttribute = nodeType.GetCustomAttributes(typeof(NodePathAttribute), false)
                                  .FirstOrDefault() as NodePathAttribute;


        string itemName = node.NodeName;

        if (pathAttribute != null && !string.IsNullOrEmpty(pathAttribute.Path))
        {
            var path = pathAttribute.Path;
            var lastSlash = path.LastIndexOf('/');

            if (lastSlash >= 0)
            {

                string groupName = path.Substring(0, lastSlash);
                return (groupName, itemName);
            }
            else
            {

                return ("Other", itemName);
            }
        }
        else
        {

            return ("Other", itemName);
        }
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


            if (_groupHeaders.ContainsKey(group.Key))
            {
                bool shouldShowGroup = hasVisibleItemsInGroup;
                _groupHeaders[group.Key].gameObject.SetActive(shouldShowGroup);


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
            var nodeLogic = NodeSpawnerService.Instance.SpawnNode(_nodesDatabase.GetClone(targetNode), _nodesListView.position, -1, true);
        }

        _nodesListView.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        foreach (var group in _groupHeaders.Values)
        {
            if (group != null)
            {
                group.OnExpansionChanged -= OnGroupExpansionChanged;
            }
        }
    }
}
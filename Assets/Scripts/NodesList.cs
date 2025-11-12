using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NodesList : MonoBehaviour
{
    [SerializeField] private NodeLogic _nodeLogicPrefab;
    [SerializeField] private RectTransform _nodesListView;
    [SerializeField] private Transform _nodesListParent;
    [SerializeField] private NodesListItem _nodesListItem;
    [SerializeField] private NodesDatabase _nodesDatabase;
    [SerializeField] private VariableDatabase _variablesDatabase;

    [Header("Search")]
    [SerializeField] private TMP_InputField _searchInputField;

    private NodesListItem[] _allItems;
    private string _currentSearch = "";

    private void Start()
    {
        _nodesListView.gameObject.SetActive(false);

        // Setup search functionality
        if (_searchInputField != null)
        {
            _searchInputField.onValueChanged.AddListener(OnSearchValueChanged);
        }

        SpawnNodes();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _nodesListView.gameObject.SetActive(!_nodesListView.gameObject.activeSelf);
            _nodesListView.position = Input.mousePosition;
            _searchInputField.ActivateInputField();

            // Clear search when opening the list
            if (_nodesListView.gameObject.activeSelf)
            {
                ClearSearch();
            }
        }
    }

    public void SpawnNodes()
    {
        var nodes = _nodesDatabase.GetNodes();
        _allItems = new NodesListItem[nodes.Length];

        // Clear existing items
        foreach (Transform child in _nodesListParent)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < nodes.Length; i++)
        {
            var item = Instantiate(_nodesListItem, _nodesListParent);
            item.SetUp(this, i); // Pass the original index 'i'
            item.SetNodeName(nodes[i].NodeName);
            _allItems[i] = item;
        }

        // Apply current search filter if any
        ApplySearchFilter();
    }

    private void OnSearchValueChanged(string searchText)
    {
        _currentSearch = searchText.Trim();
        ApplySearchFilter();
    }

    private void ApplySearchFilter()
    {
        if (_allItems == null) return;

        foreach (var item in _allItems)
        {
            if (item == null) continue;

            bool shouldShow = string.IsNullOrEmpty(_currentSearch) ||
                            item.nodeName.text.IndexOf(_currentSearch, StringComparison.OrdinalIgnoreCase) >= 0;

            item.gameObject.SetActive(shouldShow);
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

    internal void SpawnNode(int visibleIndex)
    {
        var nodes = _nodesDatabase.GetNodes();

        // Get all currently visible items
        var visibleItems = _allItems.Where(item => item != null && item.gameObject.activeInHierarchy).ToArray();

        if (visibleIndex >= 0 && visibleIndex < visibleItems.Length)
        {
            // Find the visible item at the clicked position
            var clickedItem = visibleItems[visibleIndex];

            // Find the original index in the _allItems array
            var originalIndex = Array.IndexOf(_allItems, clickedItem);

            if (originalIndex >= 0 && originalIndex < nodes.Length)
            {
                var targetNode = nodes[originalIndex];
                var nodeLogic = Instantiate(_nodeLogicPrefab, UIZoomPan.NodesParent);
                nodeLogic.transform.position = _nodesListView.position;
                nodeLogic.VariableDatabase = _variablesDatabase;
                nodeLogic.SetNodeBase(targetNode);
            }
            else
            {
                Debug.LogWarning($"Could not find original node index for visible index {visibleIndex}");
            }
        }
        else
        {
            Debug.LogWarning($"Invalid visible index: {visibleIndex}. Visible items count: {visibleItems.Length}");
        }

        _nodesListView.gameObject.SetActive(false);
    }
}
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using VContainer;

public class NodesList : MonoBehaviour
{
    [SerializeField] private RectTransform _nodesListView;
    [SerializeField] private RectTransform _nodesListParent;
    [SerializeField] private NodesListItem _nodesListItem;
    [SerializeField] private NodesListGroup _nodesListGroupPrefab;
    [SerializeField] private TMP_InputField _searchInputField;

    private NodeTreeBuilder _treeBuilder;
    private INodeFactory _nodeFactory;
    private NodeSpawnerService _nodeSpawnerService;
    private NodesDatabase _nodesDatabase;

    private string _currentSearch = "";
    private Dictionary<NodesListItem, int> _itemIndexMap = new();
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
        _searchInputField.onValueChanged.AddListener(OnSearchValueChanged);

        _treeBuilder = new NodeTreeBuilder(_nodesListGroupPrefab, _nodesListItem, _nodesListParent, _nodesDatabase);
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

    private void BuildNodeList()
    {
        _itemIndexMap.Clear();
        _treeBuilder.BuildTree((item, originalIndex) =>
        {
            item.SetUp(this, originalIndex);
            _itemIndexMap[item] = originalIndex;
        });
    }

    private void OnSearchValueChanged(string searchText)
    {
        _currentSearch = searchText.Trim();
        _treeBuilder.ApplySearchFilter(_currentSearch, out _);
        LayoutRebuilder.MarkLayoutForRebuild(_nodesListParent);
    }

    private void ClearSearch()
    {
        _searchInputField.text = "";
        _currentSearch = "";
        _treeBuilder.ApplySearchFilter("", out _);
    }

    internal void SpawnNodeFromOriginalIndex(int originalIndex)
    {
        var allNodeData = _nodesDatabase.GetAllNodeData().ToList();
        if (originalIndex < 0 || originalIndex >= allNodeData.Count) return;

        var nodeData = allNodeData[originalIndex];
        var nodeType = System.Type.GetType(nodeData.nodeType);
        if (nodeType == null) return;

        var nodeInstance = _nodeFactory.CreateNode(nodeType);
        _nodesDatabase.ApplyMetadata(nodeInstance);
        _nodeSpawnerService.SpawnNode(nodeInstance, _nodesListView.localPosition);
        _nodesListView.gameObject.SetActive(false);
    }
}
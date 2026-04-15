using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using VContainer;
using static NodeTreeBuilder;

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
    private SubgraphLibraryService _subgraphLibrary;

    private string _currentSearch = "";
    private Dictionary<NodesListItem, int> _itemIndexMap = new(); // for built-in nodes
    private Dictionary<NodesListItem, string> _subgraphItemMap = new(); // subgraphId for subgraph items

    [Inject]
    public void Construct(INodeFactory nodeFactory, NodeSpawnerService nodeSpawner, NodesDatabase nodesDatabase, SubgraphLibraryService subgraphLibraryService)
    {
        _nodeFactory = nodeFactory;
        _nodeSpawnerService = nodeSpawner;
        _nodesDatabase = nodesDatabase;
        _subgraphLibrary = subgraphLibraryService;
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
        _subgraphItemMap.Clear();

        var externalEntries = _subgraphLibrary.GetAllSubgraphInfos()
            .Select(info => new ExternalNodeEntry
            {
                CategoryPath = info.CategoryPath,
                DisplayName = info.DisplayName,
                UserData = info.SubgraphId
            });

        _treeBuilder.BuildTree(
            onDatabaseItemCreated: (item, index) =>
            {
                item.SetUp(this, index);
                _itemIndexMap[item] = index;
            },
            externalEntries: externalEntries,
            onExternalItemCreated: (item, userData) =>
            {
                string subgraphId = (string)userData;
                item.SetUpForSubgraph(this, subgraphId);
                _subgraphItemMap[item] = subgraphId;
            }
        );
    }

    public void OnItemClicked(NodesListItem item)
    {
        if (_itemIndexMap.TryGetValue(item, out int originalIndex))
        {
            SpawnNodeFromOriginalIndex(originalIndex);
        }
        else if (_subgraphItemMap.TryGetValue(item, out string subgraphId))
        {
            SpawnSubgraphNode(subgraphId);
        }
    }

    public void SpawnNodeFromOriginalIndex(int originalIndex)
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

    public void SpawnSubgraphNode(string subgraphId)
    {
        var definition = _subgraphLibrary.GetDefinition(subgraphId);
        if (definition == null) return;

        var node = _nodeFactory.CreateNode(typeof(SubgraphNode)) as SubgraphNode;
        node.Definition = definition;

        _nodeSpawnerService.SpawnNode(node, _nodesListView.localPosition);
        _nodesListView.gameObject.SetActive(false);
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
}
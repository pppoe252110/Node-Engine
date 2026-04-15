using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class SubgraphPanel : BasePanel
{
    [Header("UI References")]
    [SerializeField] private Transform _listContent;
    [SerializeField] private SubgraphListItem _itemPrefab;
    [SerializeField] private Button _createButton;
    [SerializeField] private TMP_InputField _newNameInput;
    [SerializeField] private Button _refreshButton;

    [Header("Editor Reference")]
    [SerializeField] private SubgraphEditorManager _editorManager;

    private SubgraphLibraryService _library;
    private NodeSpawnerService _spawner;

    [Inject]
    public void Construct(SubgraphLibraryService library, NodeSpawnerService spawner)
    {
        _library = library;
        _spawner = spawner;
    }

    private void Start()
    {
        _createButton.onClick.AddListener(CreateNewSubgraph);
        _refreshButton.onClick.AddListener(RefreshList);
        RefreshList();

        if (_editorManager != null)
        {
            _editorManager.OnEditStarted += () => ClosePanel();
            _editorManager.OnEditEnded += RefreshList;
        }
    }

    private void CreateNewSubgraph()
    {
        string name = _newNameInput.text.Trim();
        if (string.IsNullOrEmpty(name)) name = "New Subgraph";
        var def = _library.CreateNewSubgraph(name);
        RefreshList();
        // Optionally open editor immediately
        _editorManager?.OpenSubgraphEditor(def);
    }

    public void RefreshList()
    {
        // Clear existing items
        foreach (Transform child in _listContent)
            Destroy(child.gameObject);

        foreach (var def in _library.LoadedDefinitions.Values)
        {
            var item = Instantiate(_itemPrefab, _listContent);
            item.Initialize(def, OnEdit, OnDelete, OnInstantiate);
        }
    }

    private void OnEdit(SubgraphDefinition def)
    {
        _editorManager?.OpenSubgraphEditor(def);
    }

    private void OnDelete(SubgraphDefinition def)
    {
        _library.DeleteDefinition(def.subgraphId);
        RefreshList();
    }

    private void OnInstantiate(SubgraphDefinition def)
    {
        // Create a SubgraphNode and spawn it at mouse position (or center of view)
        var node = new SubgraphNode();
        node.Definition = def;
        Vector2 spawnPos = GetSpawnPosition();
        _spawner.SpawnNode(node, spawnPos);
        ClosePanel();
    }

    private Vector2 GetSpawnPosition()
    {
        // Convert mouse position to local position in nodes parent
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            UIZoomPan.NodesParent,
            Input.mousePosition,
            null,
            out Vector2 localPoint);
        return localPoint;
    }
}
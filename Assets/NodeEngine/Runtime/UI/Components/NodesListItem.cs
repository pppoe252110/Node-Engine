using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NodesListItem : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] public TextMeshProUGUI nodeName;
    [SerializeField] private Button _button;

    [Header("Indentation")]
    [SerializeField] private float _indentSize = 20f;

    private NodesList _nodesList;
    private int _originalIndex;
    private string _subgraphId;
    private bool _isSubgraph;

    private void Awake()
    {
        if (_button != null)
        {
            _button.onClick.AddListener(AddNode);
        }
    }

    public void SetUp(NodesList nodesList, int originalIndex)
    {
        _nodesList = nodesList;
        _originalIndex = originalIndex;
        _isSubgraph = false;
    }

    public void SetUpForSubgraph(NodesList nodesList, string subgraphId)
    {
        _nodesList = nodesList;
        _subgraphId = subgraphId;
        _isSubgraph = true;
    }

    public void SetNodeName(string name)
    {
        if (nodeName != null)
        {
            nodeName.text = name;
        }
    }

    public void SetIndent(int indentLevel)
    {
        nodeName.rectTransform.anchoredPosition += Vector2.right * indentLevel * _indentSize;
    }

    private void AddNode()
    {
        if (_isSubgraph)
            _nodesList?.SpawnSubgraphNode(_subgraphId);
        else
            _nodesList?.SpawnNodeFromOriginalIndex(_originalIndex);
    }

    private void OnDestroy()
    {
        if (_button != null)
        {
            _button.onClick.RemoveListener(AddNode);
        }
    }
}
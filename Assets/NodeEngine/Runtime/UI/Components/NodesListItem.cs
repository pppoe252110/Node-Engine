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
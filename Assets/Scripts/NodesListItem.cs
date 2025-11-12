using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NodesListItem : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] public TextMeshProUGUI nodeName;
    private int _originalIndex; // Store the original index
    private NodesList _list;

    internal void SetUp(NodesList list, int originalIndex)
    {
        _list = list;
        _originalIndex = originalIndex;

        button.onClick.AddListener(AddNode);
    }

    public void AddNode()
    {
        // Pass the original index instead of the visible index
        _list.SpawnNodeFromOriginalIndex(_originalIndex);
    }

    internal void SetNodeName(string nodeName)
    {
        this.nodeName.text = nodeName;
    }
}
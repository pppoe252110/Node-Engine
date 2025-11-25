using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NodesListItem : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] public TextMeshProUGUI nodeName;
    private int _originalIndex; 
    private NodesList _list;

    internal void SetUp(NodesList list, int originalIndex)
    {
        _list = list;
        _originalIndex = originalIndex;

        button.onClick.AddListener(AddNode);
    }

    public void AddNode()
    {
        
        _list.SpawnNodeFromOriginalIndex(_originalIndex);
    }

    internal void SetNodeName(string nodeName)
    {
        this.nodeName.text = nodeName;
    }
}
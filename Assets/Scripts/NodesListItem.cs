using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NodesListItem : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] public TextMeshProUGUI nodeName;
    private int _id;
    private NodesList _list;

    internal void SetUp(NodesList list, int id)
    {
        _list = list;
        _id = id;

        button.onClick.AddListener(AddNode);
    }

    public void AddNode()
    {
        _list.SpawnNode(_id);
    }

    internal void SetNodeName(string nodeName)
    {
        this.nodeName.text = nodeName;
    }
}

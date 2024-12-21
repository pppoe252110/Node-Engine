using System;
using UnityEngine;
using UnityEngine.UI;

public class NodesList : MonoBehaviour
{
    [SerializeField] private NodeLogic _nodeLogicPrefab;
    [SerializeField] private RectTransform _nodesListView;
    [SerializeField] private Transform _nodesListParent;
    [SerializeField] private NodesListItem _nodesListItem;
    [SerializeField] private NodesDatabase _nodesDatabase;

    private void Start()
    {
        _nodesListView.gameObject.SetActive(false);
        SpawnNodes();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _nodesListView.gameObject.SetActive(!_nodesListView.gameObject.activeSelf);
            _nodesListView.position = Input.mousePosition;
        }
    }

    public void SpawnNodes()
    {
        for (int i = 0; i < _nodesDatabase.nodes.Length; i++)
        {
            var item = Instantiate(_nodesListItem, _nodesListParent);
            var a = i;
            item.SetUp(this, a);
            item.SetNodeName(_nodesDatabase.nodes[i].NodeName);
        }
    }

    internal void SpawnNode(int id)
    {
        var targetNode = _nodesDatabase.nodes[id];

        var node = Instantiate(_nodeLogicPrefab, UIZoomPan.NodesParent);
        node.transform.position = _nodesListView.position;

        node.SetNodeBase(targetNode);
        
        _nodesListView.gameObject.SetActive(false);
    }
}

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
        var nodes = _nodesDatabase.GetNodes();

        for (int i = 0; i < nodes.Length; i++)
        {
            Debug.Log(nodes[i].GetType());
            var item = Instantiate(_nodesListItem, _nodesListParent);
            var a = i;
            item.SetUp(this, a);
            item.SetNodeName(nodes[i].NodeName);
        }
    }

    internal void SpawnNode(int id)
    {
        var nodes = _nodesDatabase.GetNodes();

        var targetNode = nodes[id];

        var node = Instantiate(_nodeLogicPrefab, UIZoomPan.NodesParent);
        node.transform.position = _nodesListView.position;

        node.SetNodeBase(targetNode);
        
        _nodesListView.gameObject.SetActive(false);
    }
}

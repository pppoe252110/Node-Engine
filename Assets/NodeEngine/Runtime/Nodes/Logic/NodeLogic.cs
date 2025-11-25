using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NodeLogic : MonoBehaviour
{
    public NodeBase Node => _node;
    public VariableDatabase VariableDatabase { get; set; }

    [SerializeField] private NodeBase _node;

    [Header("Connectors")]
    [SerializeField] private NodeSpawner _nodeSpawner;  
    [SerializeField] private NodeDeleter _nodeDeleter;  

    [Header("Properties")]
    [SerializeField] private Image _image;
    [SerializeField] private Image _nodeIcon;
    [SerializeField] private TextMeshProUGUI _nodeName;

    public void SetNodeBase(NodeBase nodeBase)
    {
        if (nodeBase == null)
        {
            Debug.LogError("NodeBase is null in SetNodeBase");
            return;
        }

        _node = nodeBase.Clone() as NodeBase;
        if (_node == null)
        {
            Debug.LogError("Failed to clone NodeBase");
            return;
        }

        _node.Initialize(this, gameObject.GetEntityId());

        _nodeName.text = _node.NodeName;
        _nodeIcon.sprite = _node.NodeSprite;
        _nodeIcon.color = _node.NodeSprite ? Color.white : Color.clear;

        
        if (_node is VariableNode varNode && VariableDatabase != null)
        {
            _nodeSpawner.SpawnVariableUI(varNode, VariableDatabase, _image);
            
            _nodeSpawner.GenerateOutputConnectors(_node, _node.outputFields, _node.outputConnectors);
        }
        else
        {
            
            _nodeSpawner.SpawnConnectors(_node, _node.inputFields, _node.outputFields, _node.inputConnectors, _node.outputConnectors);
        }

        
        _image.rectTransform.sizeDelta = new Vector2(_image.rectTransform.sizeDelta.x, 57 + (Mathf.Max(_node.inputFields.Count, _node.outputFields.Count)) * 25);
        _image.material = new Material(_image.material);

        RecalculateMaterial();

        
        NodeLogicProcessor.Instance?.AddNode(this);
    }

    public void DeleteNode()
    {
        if (_node == null)
        {
            Debug.LogWarning("Node is null in DeleteNode");
            return;
        }

        
        _nodeDeleter.DeleteNode(_node, _node.inputConnectors, _node.outputConnectors, NodeLogicProcessor.Instance);
    }

    private void RecalculateMaterial()
    {
        _image.material.SetFloat("_ScaleRatio", _image.rectTransform.rect.width / _image.rectTransform.rect.height);
    }

    internal void Process()
    {
        _node.Process();
    }
}

using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq; 

public class NodeLogic : MonoBehaviour
{
    public NodeBase Node => _node;
    public VariableDatabase VariableDatabase { get; set; }

    public RectTransform RightConnectorsParent => _nodeSpawner.RightConnectorsParent;
    public RectTransform LeftConnectorsParent => _nodeSpawner.LeftConnectorsParent;

    [SerializeField] private NodeBase _node;

    [Header("Connectors")]
    [SerializeField] private NodeSpawner _nodeSpawner;
    [SerializeField] private NodeDeleter _nodeDeleter;

    [Header("Properties")]
    [SerializeField] private Image _image;
    [SerializeField] private Image _nodeIcon;
    [SerializeField] private TextMeshProUGUI _nodeName;
    [SerializeField] private TextMeshProUGUI _nodeType;

    public void SetNodeBase(NodeBase nodeBase)
    {
        if (nodeBase == null)
        {
            Debug.LogError("NodeBase is null in SetNodeBase");
            return;
        }

        _node = nodeBase;
        if (_node == null)
        {
            Debug.LogError("Failed to clone NodeBase");
            return;
        }

        _node.Initialize(this, gameObject.GetEntityId());

        _nodeName.text = _node.NodeName;
        _nodeType.text = GetNodeTypeFromPath(_node); 
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
    }

    private string GetNodeTypeFromPath(NodeBase node)
    {
        var nodeType = node.GetType();
        var pathAttribute = nodeType.GetCustomAttributes(typeof(NodePathAttribute), false)
                                  .FirstOrDefault() as NodePathAttribute;

        if (pathAttribute != null && !string.IsNullOrEmpty(pathAttribute.Path))
        {
            
            var path = pathAttribute.Path;
            var firstSlash = path.IndexOf('/');

            if (firstSlash >= 0)
            {
                return path[..firstSlash]; 
            }
            else
            {
                return path; 
            }
        }

        
        return nodeType.Name.Replace("Node", "");
    }

    public void DeleteNode()
    {
        if (_node == null)
        {
            Debug.LogWarning("Node is null in DeleteNode");
            return;
        }

        
        _nodeDeleter.DeleteNode(_node, _node.inputConnectors, _node.outputConnectors);
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
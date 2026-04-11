using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class NodeLogic : MonoBehaviour
{
    public BaseNode Node => _node;
    public RectTransform RightConnectorsParent => _nodeUIManager.RightConnectorsParent;
    public RectTransform LeftConnectorsParent => _nodeUIManager.LeftConnectorsParent;
    public List<Connector> InputConnectors { get; private set; } = new List<Connector>();
    public List<Connector> OutputConnectors { get; private set; } = new List<Connector>();

    public Image BackgroundImage => _image;
    public Image NodeIcon => _nodeIcon;
    public TextMeshProUGUI NodeNameText => _nodeName;
    public TextMeshProUGUI NodeTypeText => _nodeType;
    public NodeUIManager UIManager => _nodeUIManager;

    private BaseNode _node;
    private NodeSpawnerService _nodeSpawnerService;

    [Header("References")]
    [SerializeField] private NodeUIManager _nodeUIManager;
    [SerializeField] private Image _image;
    [SerializeField] private Image _nodeIcon;
    [SerializeField] private TextMeshProUGUI _nodeName;
    [SerializeField] private TextMeshProUGUI _nodeType;

    [Inject]
    public void Construct(NodeSpawnerService spawnerService)
    {
        _nodeSpawnerService = spawnerService;
    }

    public void SetNodeBase(BaseNode nodeBase, string guid)
    {
        if (nodeBase == null) return;
        _node = nodeBase;
        _node.Initialize(this, guid);
    }

    public void RecalculateMaterial()
    {
        _image.material.SetFloat("_ScaleRatio", _image.rectTransform.rect.width / _image.rectTransform.rect.height);
    }

    public void DeleteNode()
    {
        if (_node == null) return;
        _nodeSpawnerService?.DeleteNode(this);
    }
}
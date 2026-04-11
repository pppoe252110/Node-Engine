using UniMediator.Runtime;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using VContainer;

public class UIZoomPan : MonoBehaviour
{
    public static UIZoomPan Instance
    {
        get
        {
            if (instance == null)
                instance = FindAnyObjectByType<UIZoomPan>();

            return instance;
        }
    }
    private static UIZoomPan instance;

    public static RectTransform NodesParent => Instance._nodesParent;

    [Header("Zoom")]
    [SerializeField] private float _zoomSpeed = 0.1f;
    [SerializeField] private float _minZoom = 0.1f;
    [SerializeField] private float _maxZoom = 5f;

    [Header("Properties")]
    [SerializeField] private Image _background;
    [SerializeField] private Material _mat;

    [SerializeField] private RectTransform _nodesParent;
    private RectTransform _rectTransform;

    private Vector2 _lastMousePos;

    private CanvasService _canvasService;
    private IMediator _mediator;

    [Inject]
    public void Construct(CanvasService canvasService, IMediator mediator)
    {
        _canvasService = canvasService;
        _mediator = mediator;
    }

    void Start()
    {
        _mat = new Material(_mat);
        _background.material = _mat;
        _rectTransform = transform as RectTransform;
    }

    void Update()
    {
        float scrollDelta = Mouse.current.scroll.value.y;
        Vector2 mousePos = Mouse.current.position.value / _canvasService.NodesCanvas.scaleFactor;

        if (scrollDelta != 0.0f)
        {
            Vector2 mouseDir = mousePos - new Vector2(_canvasService.NodesCanvas.renderingDisplaySize.x / 2f, _canvasService.NodesCanvas.renderingDisplaySize.y / 2f) / _canvasService.NodesCanvas.scaleFactor;

            var targetScale = _rectTransform.localScale.x * (1f + scrollDelta * _zoomSpeed);

            var targetScaleClamped = Mathf.Clamp(targetScale, _minZoom, _maxZoom);

            var scaleDelta = (targetScaleClamped - targetScale) / _rectTransform.localScale.x;

            var clamp = Mathf.Abs(scaleDelta) / (scrollDelta * _zoomSpeed);

            _rectTransform.localScale = targetScaleClamped * Vector3.one;
            _rectTransform.anchoredPosition -= (1f - (1f + scrollDelta * -_zoomSpeed)) * (1f - Mathf.Abs(clamp)) * (mouseDir - _rectTransform.anchoredPosition);

            _mat.SetVector("_GridOffset", -_rectTransform.anchoredPosition);
            _mat.SetFloat("_GridScaleOffset", 1f / _rectTransform.localScale.x);

            _canvasService.CanvasSize = _rectTransform.localScale;

            _mediator.Publish(new UpdateLinesNotification());
        }

        if (Mouse.current.middleButton.wasPressedThisFrame)
        {
            _lastMousePos = mousePos;
        }

        if (Mouse.current.middleButton.isPressed)
        {
            Vector2 mouseMoveDir = mousePos - _lastMousePos;
            _rectTransform.anchoredPosition += mouseMoveDir;

            _lastMousePos = mousePos;

            _mat.SetVector("_GridOffset", -_rectTransform.anchoredPosition);
            _mat.SetFloat("_GridScaleOffset", 1f / _rectTransform.localScale.x);
            
            _canvasService.CanvasSize = _rectTransform.localScale;
            
            _mediator.Publish(new UpdateLinesNotification());
        }

    }
}

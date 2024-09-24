using UnityEngine;

public class UIZoomPan : MonoBehaviour
{
    [Header("Zoom")]
    [SerializeField] private float _zoomSpeed = 0.1f;
    [SerializeField] private float _minZoom = 0.1f;
    [SerializeField] private float _maxZoom = 5f;

    [Header("Properties")]
    [SerializeField] private Material _mat;

    private RectTransform _rectTransform;

    private Vector2 _lastMousePos;

    void Start()
    {
        _rectTransform = transform as RectTransform;
    }

    void Update()
    {
        float scrollDelta = Input.mouseScrollDelta.y;
        Vector2 mousePos = Input.mousePosition / UIInstance.NodesCanvas.scaleFactor;

        if (scrollDelta != 0.0f)
        {
            Vector2 mouseDir = mousePos - new Vector2(UIInstance.NodesCanvas.renderingDisplaySize.x / 2f, UIInstance.NodesCanvas.renderingDisplaySize.y / 2f) / UIInstance.NodesCanvas.scaleFactor;

            var targetScale = _rectTransform.localScale.x * (1f + scrollDelta * _zoomSpeed);

            var targetScaleClamped = Mathf.Clamp(targetScale, _minZoom, _maxZoom);

            var scaleDelta = (targetScaleClamped - targetScale) / _rectTransform.localScale.x;

            var clamp = Mathf.Abs(scaleDelta) / (scrollDelta * _zoomSpeed);

            _rectTransform.localScale = targetScaleClamped * Vector3.one;
            _rectTransform.anchoredPosition -= (1f - (1f + scrollDelta * -_zoomSpeed)) * (1f - Mathf.Abs(clamp)) * (mouseDir - _rectTransform.anchoredPosition);

        }

        if (Input.GetMouseButtonDown(2))
        {
            _lastMousePos = mousePos;
        }

        if (Input.GetMouseButton(2))
        {
            Vector2 mouseMoveDir = mousePos - _lastMousePos;
            _rectTransform.anchoredPosition += mouseMoveDir;

            _lastMousePos = mousePos;
        }
        UIInstance.SetNodesCanvasSize(_rectTransform.localScale.x);
        _mat.SetVector("_GridOffset", -_rectTransform.anchoredPosition);
        _mat.SetFloat("_GridScaleOffset", 1f / _rectTransform.localScale.x);
    }
}

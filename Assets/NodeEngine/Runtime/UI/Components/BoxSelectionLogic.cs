using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using VContainer;

public class BoxSelectionLogic : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private RectTransform _selectionBoxVisual;
    [SerializeField] private RectTransform _container;

    private SelectionService _selectionService;
    private NodeSpawnerService _nodeSpawner;

    private Vector2 _startLocalPos;
    private RectTransform _nodesParent;

    [Inject]
    public void Construct(SelectionService selectionService, NodeSpawnerService nodeSpawner)
    {
        _selectionService = selectionService;
        _nodeSpawner = nodeSpawner;
    }

    private void Start()
    {
        _nodesParent = UIZoomPan.NodesParent;
        if (_selectionBoxVisual != null)
            _selectionBoxVisual.gameObject.SetActive(false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Clicking the empty canvas clears the selection
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            _selectionService.Clear();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left || _selectionBoxVisual == null) return;

        _selectionBoxVisual.gameObject.SetActive(true);

        RectTransform parentRect = _selectionBoxVisual.parent as RectTransform;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect,
            eventData.position,
            eventData.pressEventCamera,
            out _startLocalPos);

        UpdateBoxVisual(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left || _selectionBoxVisual == null) return;

        UpdateBoxVisual(eventData);
        EvaluateSelection();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left || _selectionBoxVisual == null) return;

        _selectionBoxVisual.gameObject.SetActive(false);
    }

    private void UpdateBoxVisual(PointerEventData eventData)
    {
        // 1. Always convert to the local space of the Selection Box's DIRECT parent
        RectTransform parentRect = _selectionBoxVisual.parent as RectTransform;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect, eventData.position, eventData.pressEventCamera, out Vector2 currentLocalPos);

        // 2. Ensure _startLocalPos was also recorded relative to parentRect in OnBeginDrag
        Vector2 lowerLeft = Vector2.Min(_startLocalPos, currentLocalPos);
        Vector2 upperRight = Vector2.Max(_startLocalPos, currentLocalPos);

        // 3. Apply to visual
        _selectionBoxVisual.anchoredPosition = lowerLeft;
        _selectionBoxVisual.sizeDelta = upperRight - lowerLeft;
    }

    private void EvaluateSelection()
    {
        // 1. Get the Selection Box Rect in Screen Space
        // We use the visual's corners to get the actual screen pixels it covers
        Vector3[] corners = new Vector3[4];
        _selectionBoxVisual.GetWorldCorners(corners);

        // Convert world corners to a screen-space Rect
        // GetWorldCorners returns: [0] BottomLeft, [1] TopLeft, [2] TopRight, [3] BottomRight
        Vector2 min = corners[0];
        Vector2 max = corners[2];
        Rect screenSelectionRect = new Rect(min.x, min.y, max.x - min.x, max.y - min.y);

        bool isAdditive = Keyboard.current.leftShiftKey.isPressed || Keyboard.current.leftCtrlKey.isPressed;
        if (!isAdditive) _selectionService.Clear();

        foreach (var nodeLogic in _nodeSpawner.GetAllNodes())
        {
            RectTransform nodeRT = nodeLogic.transform as RectTransform;

            // 2. Get the Node's Rect in Screen Space
            Vector3[] nodeCorners = new Vector3[4];
            nodeRT.GetWorldCorners(nodeCorners);

            Vector2 nMin = nodeCorners[0];
            Vector2 nMax = nodeCorners[2];
            Rect nodeScreenRect = new Rect(nMin.x, nMin.y, nMax.x - nMin.x, nMax.y - nMin.y);

            // 3. Compare screen pixels to screen pixels
            if (screenSelectionRect.Overlaps(nodeScreenRect))
            {
                _selectionService.Select(nodeLogic, true);
            }
        }
    }
}
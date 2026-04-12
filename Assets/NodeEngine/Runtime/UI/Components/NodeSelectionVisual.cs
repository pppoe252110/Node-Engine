using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using VContainer;

public class NodeSelectionVisual : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image _selection;
    [SerializeField] private NodeLogic _nodeLogic;

    private SelectionService _selectionService;
    private bool _isHovered;

    [Inject]
    public void Construct(SelectionService selectionService)
    {
        _selectionService = selectionService;
    }

    private void OnEnable()
    {
        _selectionService.OnSelectionChanged += UpdateVisual;
        UpdateVisual();
    }

    private void OnDisable() => _selectionService.OnSelectionChanged -= UpdateVisual;

    public void OnPointerEnter(PointerEventData eventData)
    {
        _isHovered = true;
        UpdateVisual();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isHovered = false;
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        bool isSelected = _selectionService.IsSelected(_nodeLogic);

        // Determine target alpha
        float targetAlpha = 0f;
        if (isSelected)
        {
            targetAlpha = 1f; // Full alpha when selected
        }
        else if (_isHovered)
        {
            targetAlpha = 0.5f; // Half alpha when hovered
        }

        // Apply alpha to the image
        Color color = _selection.color;
        color.a = targetAlpha;
        _selection.color = color;

        // Optimization: Disable the object if alpha is 0 so it's not drawn
        _selection.gameObject.SetActive(targetAlpha > 0);
    }
}
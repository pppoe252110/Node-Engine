using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NodesListGroup : MonoBehaviour
{
    public RectTransform Container => itemsContainer;

    [SerializeField] private TextMeshProUGUI groupNameText;
    [SerializeField] private TextMeshProUGUI expandCollapseButtonText;
    [SerializeField] private Button expandCollapseButton;
    [SerializeField] private RectTransform itemsContainer;

    public string GroupName { get; private set; }
    public bool IsExpanded { get; private set; } = false;

    // Event for when expansion state changes
    public System.Action<NodesListGroup, bool> OnExpansionChanged;

    private void Awake()
    {
        if (expandCollapseButton != null)
        {
            expandCollapseButton.onClick.AddListener(ToggleExpanded);
        }

        UpdateVisuals();
    }

    public void SetGroupName(string name)
    {
        GroupName = name;
        UpdateVisuals();
    }

    private void ToggleExpanded()
    {
        SetExpanded(!IsExpanded);
    }

    public void SetExpanded(bool expanded)
    {
        if (IsExpanded == expanded) return;

        IsExpanded = expanded;
        UpdateVisuals();

        OnExpansionChanged?.Invoke(this, IsExpanded);
    }

    private void UpdateVisuals()
    {
        UpdateButtonText();
        UpdateItemsVisibility();
    }

    private void UpdateButtonText()
    {
        if (groupNameText != null)
        {
            // The arrow character is already set in the inspector
            // We just need to show/hide the items and potentially rotate the arrow
            string arrow = IsExpanded ? "▼" : "►";
            Container.gameObject.SetActive(IsExpanded);
            // If you want to keep the arrow static in inspector and just rotate it:
            // We'll use the text from inspector and just update expansion state visually
            // Alternatively, you can set the text directly:
            expandCollapseButtonText.text = arrow;
            groupNameText.text = GroupName;
        }
    }

    private void UpdateItemsVisibility()
    {
        if (itemsContainer != null)
        {
            itemsContainer.gameObject.SetActive(IsExpanded);
        }
    }

    // Call this when items are added to the group
    public void RegisterItem(Transform itemTransform)
    {
        if (itemsContainer != null)
        {
            itemTransform.SetParent(itemsContainer);
        }
    }

    private void OnDestroy()
    {
        if (expandCollapseButton != null)
        {
            expandCollapseButton.onClick.RemoveAllListeners();
        }
    }
}
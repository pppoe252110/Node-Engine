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
            
            
            string arrow = IsExpanded ? "▼" : "►";
            Container.gameObject.SetActive(IsExpanded);
            
            
            
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
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NodesListGroup : MonoBehaviour
{
    public RectTransform Container => itemsContainer;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI groupNameText;
    [SerializeField] private TextMeshProUGUI expandCollapseButtonText;
    [SerializeField] private Button expandCollapseButton;
    [SerializeField] private RectTransform itemsContainer;

    [Header("Indentation")]
    [SerializeField] private RectTransform _leftSpacer; // Add this to your prefab
    [SerializeField] private float _indentSize = 20f;

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
        if (groupNameText != null)
        {
            groupNameText.text = name;
        }
    }

    public void SetIndent(int indentLevel)
    {
        if (_leftSpacer != null)
        {
            _leftSpacer.sizeDelta = new Vector2(indentLevel * _indentSize, _leftSpacer.sizeDelta.y);
        }
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
        if (expandCollapseButtonText != null)
        {
            expandCollapseButtonText.text = IsExpanded ? "▼" : "►";
        }

        if (Container != null)
        {
            Container.gameObject.SetActive(IsExpanded);
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
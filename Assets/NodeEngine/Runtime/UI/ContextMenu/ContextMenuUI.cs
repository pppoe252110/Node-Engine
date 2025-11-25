using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ContextMenuUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button deleteButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private TMP_Text deleteButtonText;
    [SerializeField] private TMP_Text cancelButtonText;

    [Header("Visual Settings")]
    [SerializeField] private Color deleteButtonColor = new Color(1f, 0.3f, 0.3f, 1f);
    [SerializeField] private Color cancelButtonColor = new Color(0.5f, 0.5f, 0.5f, 1f);
    [SerializeField] private Color buttonTextColor = Color.white;

    
    public System.Action onDeleteClicked;
    public System.Action onCancelClicked;

    private void Awake()
    {
        InitializeUI();
    }

    private void InitializeUI()
    {
        SetupButton(deleteButton, deleteButtonColor, deleteButtonText, "Delete Node", OnDeleteButtonClicked);
        SetupButton(cancelButton, cancelButtonColor, cancelButtonText, "Cancel", OnCancelButtonClicked);
    }

    private void SetupButton(Button button, Color color, TMP_Text text, string label, UnityEngine.Events.UnityAction action)
    {
        if (button == null) return;

        
        var buttonColors = button.colors;
        buttonColors.normalColor = color;
        buttonColors.highlightedColor = new Color(color.r + 0.2f, color.g + 0.2f, color.b + 0.2f, 1f);
        buttonColors.pressedColor = new Color(color.r - 0.2f, color.g - 0.2f, color.b - 0.2f, 1f);
        button.colors = buttonColors;

        
        if (text != null)
        {
            text.text = label;
            text.color = buttonTextColor;
        }

        
        button.onClick.AddListener(action);
    }

    private void OnDeleteButtonClicked()
    {
        onDeleteClicked?.Invoke();
    }

    private void OnCancelButtonClicked()
    {
        onCancelClicked?.Invoke();
    }

    public void CloseMenu()
    {
        Destroy(gameObject);
    }

    public void SetDeleteButtonText(string text)
    {
        if (deleteButtonText != null)
            deleteButtonText.text = text;
    }

    public void SetCancelButtonText(string text)
    {
        if (cancelButtonText != null)
            cancelButtonText.text = text;
    }

    private void OnDestroy()
    {
        if (deleteButton != null)
            deleteButton.onClick.RemoveAllListeners();

        if (cancelButton != null)
            cancelButton.onClick.RemoveAllListeners();
    }
}
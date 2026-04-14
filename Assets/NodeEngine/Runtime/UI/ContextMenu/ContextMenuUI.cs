using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ContextMenuUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button deleteButton;
    [SerializeField] private Button duplicateButton;
    [SerializeField] private Button cancelButton;

    public System.Action onDeleteClicked;
    public System.Action onCancelClicked;
    public System.Action onDuplicateClicked;

    private void Awake()
    {
        InitializeUI();
    }

    private void InitializeUI()
    {
        SetupButton(deleteButton, OnDeleteButtonClicked);
        SetupButton(cancelButton, OnCancelButtonClicked);
        SetupButton(cancelButton, OnDuplicateButtonClicked);
    }

    private void SetupButton(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button == null) return;
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
    private void OnDuplicateButtonClicked()
    {
        onDuplicateClicked?.Invoke();
    }

    public void CloseMenu()
    {
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (deleteButton != null)
            deleteButton.onClick.RemoveAllListeners();

        if (cancelButton != null)
            cancelButton.onClick.RemoveAllListeners();

        if (duplicateButton != null)
            duplicateButton.onClick.RemoveAllListeners();
    }
}

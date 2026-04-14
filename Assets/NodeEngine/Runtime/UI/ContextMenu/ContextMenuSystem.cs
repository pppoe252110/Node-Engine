using Cysharp.Threading.Tasks;
using System.ComponentModel.Design;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using VContainer;

public class ContextMenuSystem : MonoBehaviour
{
    [Header("UI Prefabs")]
    [SerializeField] private GameObject contextMenuPrefab;
    [SerializeField] private GameObject confirmationDialogPrefab;

    [Header("References")]
    [SerializeField] private Canvas targetCanvas;
    [SerializeField] private Transform menusParent;

    [Header("Settings")]
    [SerializeField] private bool debugMode = false;

    private GameObject currentContextMenu;
    private GameObject currentDialog;
    private CancellationTokenSource cancellationTokenSource;

    public static ContextMenuSystem Instance { get; private set; }

    public bool IsContextMenuOpen => currentContextMenu != null;
    public bool IsDialogOpen => currentDialog != null;

    [Inject] private SelectionService _selectionService;
    [Inject] private NodeSpawnerService _nodeSpawner;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        cancellationTokenSource = new CancellationTokenSource();

        if (targetCanvas == null)
        {
            targetCanvas = GetComponentInParent<Canvas>() ?? FindAnyObjectByType<Canvas>();
        }

        if (debugMode) Log("ContextMenuSystem initialized");
    }

    public async UniTask ShowContextMenu(Vector2 screenPosition, NodeLogic targetNode)
    {
        if (!ValidatePrerequisites(contextMenuPrefab, targetNode))
            return;

        HideContextMenu();

        try
        {
            currentContextMenu = Instantiate(contextMenuPrefab, menusParent);
            PositionContextMenuBottomLeft(screenPosition);

            SetupContextMenuUI(targetNode);
            await WaitForContextMenuClose();
        }
        catch (System.Exception e)
        {
            LogError($"Error showing context menu: {e.Message}");
            HideContextMenu();
        }
    }

    private void PositionContextMenuBottomLeft(Vector2 screenPosition)
    {
        if (currentContextMenu == null) return;

        var rectTransform = currentContextMenu.GetComponent<RectTransform>();
        if (rectTransform == null) return;

        rectTransform.pivot = Vector2.zero;

        rectTransform.position = screenPosition;

        ClampToScreenBounds(rectTransform);
    }

    private void ClampToScreenBounds(RectTransform rectTransform)
    {
        
        Vector2 menuSize = rectTransform.rect.size;

        Vector2 screenPos = rectTransform.position;

        if (screenPos.x + menuSize.x > Screen.width)
        {
            
            screenPos.x = Screen.width - menuSize.x;
        }

        if (screenPos.y + menuSize.y > Screen.height)
        {
            
            screenPos.y = Screen.height - menuSize.y;
        }

        rectTransform.position = screenPos;
    }

    public void HideContextMenu()
    {
        if (currentContextMenu != null)
        {
            Destroy(currentContextMenu);
            currentContextMenu = null;
        }
    }

    public async UniTask<bool> ShowDeleteConfirmationDialog(string nodeName)
    {
        return await ShowConfirmationDialog(
            "Delete Node",
            $"Are you sure you want to delete '{nodeName}'?",
            "Delete",
            "Cancel"
        );
    }

    public async UniTask<bool> ShowConfirmationDialog(string title, string message, string confirmText, string cancelText)
    {
        return await CreateRuntimeConfirmationDialog(title, message, confirmText, cancelText);
    }

    public void CloseAll()
    {
        HideContextMenu();
        HideDialog();
    }

    private void SetupContextMenuUI(NodeLogic targetNode)
    {
        var contextMenuUI = currentContextMenu.GetComponent<ContextMenuUI>();
        if (contextMenuUI != null)
        {
            contextMenuUI.onDeleteClicked = () => OnDeleteClicked(targetNode);
            contextMenuUI.onCancelClicked = HideContextMenu;
            contextMenuUI.onDuplicateClicked = () => OnDuplicateClicked(targetNode);
        }
        else
        {
            SetupContextMenuButtonsFallback(targetNode);
        }
    }

    private async UniTask WaitForContextMenuClose()
    {
        if (currentContextMenu == null) return;

        try
        {
            while (currentContextMenu != null)
            {
                await UniTask.Yield(cancellationTokenSource.Token);

                if ((Mouse.current.leftButton.wasPressedThisFrame || Mouse.current.rightButton.wasPressedThisFrame) && !IsClickOnContextMenu(Mouse.current.position.value)
                    || Keyboard.current.escapeKey.wasReleasedThisFrame)
                {
                    HideContextMenu();
                    break;
                }
            }
        }
        catch (System.OperationCanceledException)
        {
            
        }
    }

    private bool IsClickOnContextMenu(Vector2 clickPosition)
    {
        if (currentContextMenu == null) return false;
        var rectTransform = currentContextMenu.GetComponent<RectTransform>();
        return rectTransform != null && RectTransformUtility.RectangleContainsScreenPoint(rectTransform, clickPosition);
    }

    private void SetupContextMenuButtonsFallback(NodeLogic targetNode)
    {
        if (currentContextMenu == null) return;
        SetupButton("DeleteButton", () => OnDeleteClicked(targetNode));
        SetupButton("CancelButton", HideContextMenu);
        SetupButton("Duplicate", () => OnDuplicateClicked(targetNode));
    }

    private void SetupButton(string buttonName, System.Action action)
    {
        var buttonTransform = currentContextMenu.transform.Find(buttonName);
        if (buttonTransform != null && buttonTransform.TryGetComponent<Button>(out var button))
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => action());
        }
    }

    private async UniTask<bool> CreateRuntimeConfirmationDialog(string title, string message, string confirmText, string cancelText)
    {
        if (!ValidatePrerequisites(confirmationDialogPrefab, null))
        {
            return await CreateFallbackConfirmationDialog(title, message, confirmText, cancelText);
        }

        try
        {
            currentDialog = Instantiate(confirmationDialogPrefab, menusParent);
            CenterDialogOnScreen(currentDialog);

            var dialogComponent = currentDialog.GetComponent<ConfirmationDialog>();
            if (dialogComponent != null)
            {
                dialogComponent.Setup(title, message, confirmText, cancelText);
                var result = await dialogComponent.WaitForResult();
                HideDialog();
                return result;
            }
        }
        catch (System.Exception e)
        {
            LogError($"Error creating confirmation dialog: {e.Message}");
        }

        return await CreateFallbackConfirmationDialog(title, message, confirmText, cancelText);
    }

    private void CenterDialogOnScreen(GameObject dialog)
    {
        if (targetCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            dialog.transform.position = new Vector2(Screen.width / 2, Screen.height / 2);
        }
        else
        {
            var rectTransform = dialog.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = Vector2.zero;
            }
        }
    }

    private async UniTask<bool> CreateFallbackConfirmationDialog(string title, string message, string confirmText, string cancelText)
    {
        Log($"{title}: {message} (Press Y to confirm, N to cancel)");

        while (true)
        {
            if (Keyboard.current.yKey.wasReleasedThisFrame) return true;
            if (Keyboard.current.nKey.wasReleasedThisFrame) return false;
            await UniTask.Yield(cancellationTokenSource.Token);
        }
    }

    private void HideDialog()
    {
        if (currentDialog != null)
        {
            Destroy(currentDialog);
            currentDialog = null;
        }
    }

    private void OnDuplicateClicked(NodeLogic targetNode)
    {
        HideContextMenu();
        var selected = _selectionService.SelectedNodes.ToList();
        Vector2 offset = new Vector2(30, -30);
        var clones = _nodeSpawner.DuplicateNodes(selected, offset);

        _selectionService.Clear();
        foreach (var clone in clones)
            _selectionService.Select(clone, additive: true);
    }

    private void OnDeleteClicked(NodeLogic targetNode)
    {
        // If the clicked node is part of a multi‑selection, delete all selected nodes.
        if (_selectionService.SelectedNodes.Count > 1 && _selectionService.IsSelected(targetNode))
        {
            foreach (var node in _selectionService.SelectedNodes.ToList())
            {
                node.DeleteNode();
            }
            _selectionService.Clear();
        }
        else
        {
            targetNode.DeleteNode();
        }

        HideContextMenu();
    }

    private bool ValidatePrerequisites(GameObject prefab, NodeLogic targetNode)
    {
        if (prefab == null)
        {
            LogError("Prefab reference is null");
            return false;
        }

        if (targetCanvas == null)
        {
            LogError("Target canvas is null");
            return false;
        }

        if (targetNode != null && targetNode.Node == null)
        {
            LogError("Target node is invalid");
            return false;
        }

        return true;
    }

    private void Log(string message)
    {
        if (debugMode) Debug.Log($"[ContextMenuSystem] {message}");
    }

    private void LogError(string message)
    {
        Debug.LogError($"[ContextMenuSystem] {message}");
    }

    private void Update()
    {
        if (Keyboard.current.escapeKey.wasReleasedThisFrame)
        {
            if (IsContextMenuOpen) HideContextMenu();
            else if (IsDialogOpen) HideDialog();
        }
    }

    private void OnDestroy()
    {
        cancellationTokenSource?.Cancel();
        cancellationTokenSource?.Dispose();
        CloseAll();

        if (Instance == this) Instance = null;
        if (debugMode) Log("ContextMenuSystem destroyed");
    }
}

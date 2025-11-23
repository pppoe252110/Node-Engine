using Cysharp.Threading.Tasks;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ConfirmationDialog : MonoBehaviour
{
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private TMP_Text confirmButtonText;
    [SerializeField] private TMP_Text cancelButtonText;

    private UniTaskCompletionSource<bool> completionSource;
    private CancellationTokenSource cancellationTokenSource;

    private void Awake()
    {
        cancellationTokenSource = new CancellationTokenSource();
    }

    public void Setup(string title, string message, string confirmText, string cancelText)
    {
        if (titleText != null) titleText.text = title;
        if (messageText != null) messageText.text = message;
        if (confirmButtonText != null) confirmButtonText.text = confirmText;
        if (cancelButtonText != null) cancelButtonText.text = cancelText;

        completionSource = new UniTaskCompletionSource<bool>();

        if (confirmButton != null)
            confirmButton.onClick.AddListener(() => Complete(true));

        if (cancelButton != null)
            cancelButton.onClick.AddListener(() => Complete(false));
    }

    public UniTask<bool> WaitForResult()
    {
        return completionSource.Task;
    }

    private void Complete(bool result)
    {
        completionSource?.TrySetResult(result);
    }

    private void Update()
    {
        // Close on Escape key
        if (Keyboard.current.escapeKey.wasReleasedThisFrame)
        {
            Complete(false);
        }
    }

    private void OnDestroy()
    {
        cancellationTokenSource?.Cancel();
        cancellationTokenSource?.Dispose();
        completionSource?.TrySetResult(false);
    }
}
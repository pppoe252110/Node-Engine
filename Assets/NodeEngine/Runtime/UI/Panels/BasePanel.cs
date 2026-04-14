using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public abstract class BasePanel : MonoBehaviour
{
    [Header("Panel Settings")]
    [SerializeField] protected float fadeDuration = 0.3f;
    [SerializeField] protected CanvasGroup canvasGroup;
    [SerializeField] protected GameObject panelObject;
    [SerializeField] protected bool isOpenOnStart = false;
    protected bool isPanelOpen = false;
    protected Coroutine fadeCoroutine;

    public event Action<BasePanel> OnPanelOpened;
    public event Action<BasePanel> OnPanelClosed;

    protected virtual void Awake()
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if (panelObject == null) panelObject = gameObject;

        if (isOpenOnStart)
        {
            OpenPanelImmediate();
        }
        else
        {
            ClosePanelImmediate();
        }
    }

    public virtual void TogglePanel()
    {
        if (isPanelOpen)
        {
            ClosePanel();
        }
        else
        {
            OpenPanel();
        }
    }

    public virtual void OpenPanel()
    {
        if (isPanelOpen) return;

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        panelObject.SetActive(true);
        fadeCoroutine = StartCoroutine(FadePanel(true));
        isPanelOpen = true;

        OnPanelOpened?.Invoke(this);
        OnPanelOpenedAction();
    }

    public virtual void OpenPanelImmediate()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        panelObject.SetActive(true);
        isPanelOpen = true;

        // Optionally fire the opened event (depends on desired behavior)
        OnPanelOpened?.Invoke(this);
        OnPanelOpenedAction();
    }

    public virtual void ClosePanel()
    {
        if (!isPanelOpen) return;

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        fadeCoroutine = StartCoroutine(FadePanel(false));
        isPanelOpen = false;

        OnPanelClosed?.Invoke(this);
        OnPanelClosedAction();
    }

    public void ClosePanelImmediate()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        panelObject.SetActive(false);
        isPanelOpen = false;
    }

    protected virtual IEnumerator FadePanel(bool fadeIn)
    {
        if (fadeIn)
        {
            panelObject.SetActive(true);
        }

        float startAlpha = canvasGroup.alpha;
        float endAlpha = fadeIn ? 1f : 0f;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = endAlpha;
        canvasGroup.interactable = fadeIn;
        canvasGroup.blocksRaycasts = fadeIn;

        if (!fadeIn)
        {
            panelObject.SetActive(false);
        }

        fadeCoroutine = null;
    }

    protected virtual void OnPanelOpenedAction() { }
    protected virtual void OnPanelClosedAction() { }

    public bool IsOpen => isPanelOpen;
}
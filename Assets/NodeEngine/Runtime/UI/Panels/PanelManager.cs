using System;
using System.Collections.Generic;
using UnityEngine;

public class PanelManager : MonoBehaviour
{
    public static PanelManager Instance { get; private set; }

    [SerializeField] private BasePanel _nodesPanel;

    private Dictionary<Type, BasePanel> panels = new Dictionary<Type, BasePanel>();
    private Stack<BasePanel> panelStack = new Stack<BasePanel>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void RegisterPanel<T>(T panel) where T : BasePanel
    {
        Type panelType = typeof(T);
        if (!panels.ContainsKey(panelType))
        {
            panels[panelType] = panel;

            // Subscribe to panel events
            panel.OnPanelOpened += OnAnyPanelOpened;
            panel.OnPanelClosed += OnAnyPanelClosed;
        }
    }

    public T GetPanel<T>() where T : BasePanel
    {
        Type panelType = typeof(T);
        if (panels.TryGetValue(panelType, out BasePanel panel))
        {
            return panel as T;
        }
        return null;
    }

    public void TogglePanel<T>() where T : BasePanel
    {
        T panel = GetPanel<T>();
        if (panel != null)
        {
            if (panel.IsOpen)
            {
                ClosePanel<T>();
            }
            else
            {
                OpenPanel<T>();
            }
        }
    }

    public void OpenPanel<T>() where T : BasePanel
    {
        T panel = GetPanel<T>();
        if (panel != null && !panel.IsOpen)
        {
            panel.OpenPanel();
        }
    }

    public void ClosePanel<T>() where T : BasePanel
    {
        T panel = GetPanel<T>();
        if (panel != null && panel.IsOpen)
        {
            panel.ClosePanel();
        }
    }

    public void CloseLastPanel()
    {
        if (panelStack.Count > 0)
        {
            BasePanel lastPanel = panelStack.Peek();
            if (lastPanel != null && lastPanel.IsOpen)
            {
                lastPanel.ClosePanel();
            }
        }
    }

    public void CloseAllPanels()
    {
        var panelsToClose = new List<BasePanel>(panelStack);

        foreach (var panel in panelsToClose)
        {
            if (panel != null && panel.IsOpen)
            {
                panel.ClosePanel();
            }
        }
    }

    private void OnAnyPanelOpened(BasePanel panel)
    {
        if (!panelStack.Contains(panel))
        {
            panelStack.Push(panel);
        }
    }

    private void OnAnyPanelClosed(BasePanel panel)
    {
        var newStack = new Stack<BasePanel>();
        bool found = false;

        while (panelStack.Count > 0)
        {
            var currentPanel = panelStack.Pop();
            if (currentPanel == panel && !found)
            {
                found = true;
                continue;
            }
            newStack.Push(currentPanel);
        }

        panelStack = newStack;
    }

    public void SwitchNodeWindow()
    {
        _nodesPanel.TogglePanel();
    }
}
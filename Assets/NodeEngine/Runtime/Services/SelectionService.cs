using System;
using System.Collections.Generic;

public class SelectionService
{
    // HashSet ensures we don't double-add nodes
    public HashSet<NodeLogic> SelectedNodes { get; } = new();

    public event Action OnSelectionChanged;

    public void Select(NodeLogic node, bool additive = false)
    {
        if (!additive) Clear();

        if (SelectedNodes.Add(node))
        {
            // Optional: Trigger a visual update on the node here (e.g., outline color)
            OnSelectionChanged?.Invoke();
        }
    }

    public void Deselect(NodeLogic node)
    {
        if (SelectedNodes.Remove(node))
        {
            // Optional: Revert visual update here
            OnSelectionChanged?.Invoke();
        }
    }

    public void Clear()
    {
        if (SelectedNodes.Count == 0) return;

        // Optional: Loop through and revert visuals for all before clearing
        SelectedNodes.Clear();
        OnSelectionChanged?.Invoke();
    }

    public bool IsSelected(NodeLogic node) => SelectedNodes.Contains(node);
}
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class NodeContextMenu : MonoBehaviour, IPointerClickHandler
{
    private NodeLogic nodeLogic;

    private void Awake()
    {
        nodeLogic = GetComponent<NodeLogic>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            // Check if we're clicking on a connector first
            if (!IsMouseOverConnector(eventData))
            {
                ShowContextMenu(eventData.position).Forget();
            }
        }
        else if (eventData.button == PointerEventData.InputButton.Left)
        {
            ContextMenuSystem.Instance?.HideContextMenu();
        }
    }

    private bool IsMouseOverConnector(PointerEventData eventData)
    {
        // Method 1: Check hovered objects (most common case)
        foreach (var hoveredObject in eventData.hovered)
        {
            if (IsObjectConnector(hoveredObject))
                return true;
        }

        // Method 2: Perform an additional raycast to be sure
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var result in results)
        {
            if (IsObjectConnector(result.gameObject))
                return true;
        }

        return false;
    }

    private bool IsObjectConnector(GameObject obj)
    {
        if (obj == null) return false;

        // Check if the object itself is a connector
        if (obj.TryGetComponent<Connector>(out _))
            return true;

        // Check if any parent is a connector
        Transform current = obj.transform;
        while (current != null)
        {
            if (current.TryGetComponent<Connector>(out _))
                return true;
            current = current.parent;
        }

        return false;
    }

    private async UniTaskVoid ShowContextMenu(Vector2 screenPosition)
    {
        if (ContextMenuSystem.Instance != null && nodeLogic != null)
        {
            await ContextMenuSystem.Instance.ShowContextMenu(screenPosition, nodeLogic);
        }
    }

    private void OnDestroy()
    {
        ContextMenuSystem.Instance?.HideContextMenu();
    }
}
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
        
        foreach (var hoveredObject in eventData.hovered)
        {
            if (IsObjectConnector(hoveredObject))
                return true;
        }

        
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

        
        if (obj.TryGetComponent<Connector>(out _))
            return true;

        
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
using UnityEngine;
using UnityEngine.EventSystems;
using Cysharp.Threading.Tasks;

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
            ShowContextMenu(eventData.position).Forget();
        }
        else if (eventData.button == PointerEventData.InputButton.Left)
        {
            ContextMenuSystem.Instance?.HideContextMenu();
        }
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
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

public class NodeKeyboardShortcuts : MonoBehaviour
{
    [Inject] private SelectionService _selectionService;
    [Inject] private NodeSpawnerService _nodeSpawner;

    private void Update()
    {
        var eventSystem = UnityEngine.EventSystems.EventSystem.current;
        if (eventSystem == null) return;

        GameObject selectedGo = eventSystem.currentSelectedGameObject;

        if (selectedGo != null && selectedGo.TryGetComponent<TMPro.TMP_InputField>(out _))
        {
            return;
        }

        if (Keyboard.current.deleteKey.wasPressedThisFrame)
        {
            DeleteSelectedNodes();
        }

        if (Keyboard.current.ctrlKey.isPressed && Keyboard.current.dKey.wasPressedThisFrame)
        {
            DuplicateSelectedNodes();
        }
    }

    private void DeleteSelectedNodes()
    {
        // Copy to list because deleting will modify the selection set
        var nodesToDelete = new System.Collections.Generic.List<NodeLogic>(_selectionService.SelectedNodes);

        foreach (var node in nodesToDelete)
        {
            _nodeSpawner.DeleteNode(node);
        }

        _selectionService.Clear();
    }

    private void DuplicateSelectedNodes()
    {
        var selected = _selectionService.SelectedNodes.ToList();
        Vector2 offset = new Vector2(30, -30);
        var clones = _nodeSpawner.DuplicateNodes(selected, offset);

        _selectionService.Clear();
        foreach (var clone in clones)
            _selectionService.Select(clone, additive: true);
    }
}
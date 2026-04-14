using UnityEngine;
using UnityEngine.InputSystem;

public class UIController : MonoBehaviour
{
    private void Update()
    {
        if (Keyboard.current.escapeKey.wasReleasedThisFrame)
        {
            PanelManager.Instance.CloseLastPanel();
        }
    }
}
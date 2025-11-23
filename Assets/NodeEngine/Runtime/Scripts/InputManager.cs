using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections.Generic;

public class InputManager : MonoBehaviour
{
    // Singleton instance
    public static InputManager Instance { get; private set; }

    // Input actions
    private InputActions _inputActions;

    // Static events for global access
    public static event Action<Vector2> OnMove;
    public static event Action<Vector2> OnLook;
    public static event Action OnAttack;
    public static event Action OnInteract;
    public static event Action OnCrouch;
    public static event Action OnJump;
    public static event Action OnPrevious;
    public static event Action OnNext;
    public static event Action OnSprint;

    // UI events
    public static event Action<Vector2> OnNavigate;
    public static event Action OnSubmit;
    public static event Action OnCancel;
    public static event Action<Vector2> OnPoint;
    public static event Action OnClick;
    public static event Action OnRightClick;
    public static event Action OnMiddleClick;
    public static event Action<Vector2> OnScrollWheel;

    // Static properties for polling
    public static Vector2 MoveInput { get; private set; }
    public static Vector2 LookInput { get; private set; }
    public static Vector2 MousePosition => Mouse.current?.position.ReadValue() ?? Vector2.zero;
    public static Vector2 MouseDelta => Mouse.current?.delta.ReadValue() ?? Vector2.zero;
    public static float MouseScroll => Mouse.current?.scroll.ReadValue().y ?? 0f;
    public static bool IsGamepadConnected => Gamepad.current != null;

    // Input state
    private Dictionary<string, bool> _inputStates = new Dictionary<string, bool>();

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeInput();
    }

    private void InitializeInput()
    {
        _inputActions = new InputActions();

        // Player actions
        _inputActions.Player.Move.performed += OnMovePerformed;
        _inputActions.Player.Move.canceled += OnMoveCanceled;

        _inputActions.Player.Look.performed += OnLookPerformed;
        _inputActions.Player.Look.canceled += OnLookCanceled;

        _inputActions.Player.Attack.performed += OnAttackPerformed;
        _inputActions.Player.Interact.performed += OnInteractPerformed;
        _inputActions.Player.Crouch.performed += OnCrouchPerformed;
        _inputActions.Player.Jump.performed += OnJumpPerformed;
        _inputActions.Player.Previous.performed += OnPreviousPerformed;
        _inputActions.Player.Next.performed += OnNextPerformed;
        _inputActions.Player.Sprint.performed += OnSprintPerformed;

        // UI actions
        _inputActions.UI.Navigate.performed += OnNavigatePerformed;
        _inputActions.UI.Submit.performed += OnSubmitPerformed;
        _inputActions.UI.Cancel.performed += OnCancelPerformed;
        _inputActions.UI.Point.performed += OnPointPerformed;
        _inputActions.UI.Click.performed += OnClickPerformed;
        _inputActions.UI.RightClick.performed += OnRightClickPerformed;
        _inputActions.UI.MiddleClick.performed += OnMiddleClickPerformed;
        _inputActions.UI.ScrollWheel.performed += OnScrollWheelPerformed;

        // Enable both action maps
        _inputActions.Player.Enable();
        _inputActions.UI.Enable();
    }

    private void OnEnable() => _inputActions?.Enable();
    private void OnDisable() => _inputActions?.Disable();

    private void OnDestroy()
    {
        if (_inputActions != null)
        {
            // Player actions
            _inputActions.Player.Move.performed -= OnMovePerformed;
            _inputActions.Player.Move.canceled -= OnMoveCanceled;
            _inputActions.Player.Look.performed -= OnLookPerformed;
            _inputActions.Player.Look.canceled -= OnLookCanceled;
            _inputActions.Player.Attack.performed -= OnAttackPerformed;
            _inputActions.Player.Interact.performed -= OnInteractPerformed;
            _inputActions.Player.Crouch.performed -= OnCrouchPerformed;
            _inputActions.Player.Jump.performed -= OnJumpPerformed;
            _inputActions.Player.Previous.performed -= OnPreviousPerformed;
            _inputActions.Player.Next.performed -= OnNextPerformed;
            _inputActions.Player.Sprint.performed -= OnSprintPerformed;

            // UI actions
            _inputActions.UI.Navigate.performed -= OnNavigatePerformed;
            _inputActions.UI.Submit.performed -= OnSubmitPerformed;
            _inputActions.UI.Cancel.performed -= OnCancelPerformed;
            _inputActions.UI.Point.performed -= OnPointPerformed;
            _inputActions.UI.Click.performed -= OnClickPerformed;
            _inputActions.UI.RightClick.performed -= OnRightClickPerformed;
            _inputActions.UI.MiddleClick.performed -= OnMiddleClickPerformed;
            _inputActions.UI.ScrollWheel.performed -= OnScrollWheelPerformed;

            _inputActions.Dispose();
        }
    }

    #region Player Input Handlers
    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        MoveInput = context.ReadValue<Vector2>();
        OnMove?.Invoke(MoveInput);
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        MoveInput = Vector2.zero;
        OnMove?.Invoke(Vector2.zero);
    }

    private void OnLookPerformed(InputAction.CallbackContext context)
    {
        LookInput = context.ReadValue<Vector2>();
        OnLook?.Invoke(LookInput);
    }

    private void OnLookCanceled(InputAction.CallbackContext context)
    {
        LookInput = Vector2.zero;
        OnLook?.Invoke(Vector2.zero);
    }

    private void OnAttackPerformed(InputAction.CallbackContext context)
    {
        OnAttack?.Invoke();
    }

    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        OnInteract?.Invoke();
    }

    private void OnCrouchPerformed(InputAction.CallbackContext context)
    {
        OnCrouch?.Invoke();
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        OnJump?.Invoke();
    }

    private void OnPreviousPerformed(InputAction.CallbackContext context)
    {
        OnPrevious?.Invoke();
    }

    private void OnNextPerformed(InputAction.CallbackContext context)
    {
        OnNext?.Invoke();
    }

    private void OnSprintPerformed(InputAction.CallbackContext context)
    {
        OnSprint?.Invoke();
    }
    #endregion

    #region UI Input Handlers
    private void OnNavigatePerformed(InputAction.CallbackContext context)
    {
        OnNavigate?.Invoke(context.ReadValue<Vector2>());
    }

    private void OnSubmitPerformed(InputAction.CallbackContext context)
    {
        OnSubmit?.Invoke();
    }

    private void OnCancelPerformed(InputAction.CallbackContext context)
    {
        OnCancel?.Invoke();
    }

    private void OnPointPerformed(InputAction.CallbackContext context)
    {
        OnPoint?.Invoke(context.ReadValue<Vector2>());
    }

    private void OnClickPerformed(InputAction.CallbackContext context)
    {
        OnClick?.Invoke();
    }

    private void OnRightClickPerformed(InputAction.CallbackContext context)
    {
        OnRightClick?.Invoke();
    }

    private void OnMiddleClickPerformed(InputAction.CallbackContext context)
    {
        OnMiddleClick?.Invoke();
    }

    private void OnScrollWheelPerformed(InputAction.CallbackContext context)
    {
        OnScrollWheel?.Invoke(context.ReadValue<Vector2>());
    }
    #endregion

    // Public methods for input state checking
    public bool GetButton(string actionName)
    {
        return _inputStates.ContainsKey(actionName) && _inputStates[actionName];
    }

    public bool GetButtonDown(string actionName)
    {
        // For button down, you'd need to track previous frame state
        // This is a simplified version
        return _inputStates.ContainsKey(actionName) && _inputStates[actionName];
    }

    // Switch action maps
    public void EnablePlayerInput() => _inputActions.Player.Enable();
    public void DisablePlayerInput() => _inputActions.Player.Disable();
    public void EnableUIInput() => _inputActions.UI.Enable();
    public void DisableUIInput() => _inputActions.UI.Disable();
}
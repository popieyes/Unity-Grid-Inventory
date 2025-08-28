using System;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(menuName = "Input/Processor")]
public class InputProcessorSO : ScriptableObject, InputSystem_Actions.IInventoryActions
{
    private InputSystem_Actions _input;
    public Action<Vector2> OnNavigateEvent;
    public Action OnRotateEvent;
    public Action OnActionEvent;

    public void Enable()
    {
        if (_input == null)
        {
            _input = new InputSystem_Actions();
            _input.Inventory.SetCallbacks(this);
        }   
        _input.Enable();
    }

    public void Disable() => _input.Disable();
    
    #region ---- INPUT CALLBACKS ---- 
    public void OnNavigate(InputAction.CallbackContext context)
    {
        if (context.performed)
            OnNavigateEvent.Invoke(context.ReadValue<Vector2>());

    }

    public void OnRotate(InputAction.CallbackContext context)
    {
        if (context.performed)
            OnRotateEvent.Invoke();
    }

    public void OnAction(InputAction.CallbackContext context)
    {
        if (context.performed)
            OnActionEvent.Invoke();
    }
    #endregion

}

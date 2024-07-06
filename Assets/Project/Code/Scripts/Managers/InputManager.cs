using System;
using UnityEngine.InputSystem;
using UnityEngine;

public class InputManager : MonoSingleton<InputManager>
{
    private PlayerController pc;
    private CameraController cc;
    private WeaponController wc;
    private Vector2 moveInput;
    [SerializeField] private bool inputDisabled;

    private void Start()
    {
        pc = MonoSingleton<PlayerController>.Instance;
        cc = MonoSingleton<CameraController>.Instance;
        wc = MonoSingleton<WeaponController>.Instance;
    }

    private void OnEnable()
    {
        EnableInput();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (inputDisabled) return;

        pc.moveInput = context.ReadValue<Vector2>();
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (inputDisabled) return;

        if (context.started)
        {
            Debug.Log("Dash");
            // TODO: Implement dash
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (inputDisabled) return;

            if (context.started)
            {
                pc.Jump();
            }
    }

    public void OnPrimaryFire(InputAction.CallbackContext context)
    {
        if (inputDisabled) return;

        if (context.started)
        {
            wc.PrimaryFire();
        }
    }

    public void OnSecondaryFire(InputAction.CallbackContext context)
    {
        if (inputDisabled) return;

        if (context.started)
        {
            wc.SecondaryFire();
        }
    }

    public void OnReload(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            // TODO: Implement reload (or maybe not because reloading is boring)

            // reload scene for now
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
            // StartCoroutine(pc.Respawn());
        }
    }

    // switch weapons
    public void OnSwitchWeapon1(InputAction.CallbackContext context)
    {
        if (inputDisabled) return;

        if (context.started)
        {
            wc.SwitchWeapon(0);
        }
    }

    public void OnSwitchWeapon2(InputAction.CallbackContext context)
    {
        if (inputDisabled) return;

        if (context.started)
        {
            wc.SwitchWeapon(1);
        }
    }

    public void OnSwitchWeapon3(InputAction.CallbackContext context)
    {
        if (inputDisabled) return;

        if (context.started)
        {
            wc.SwitchWeapon(2);
        }
    }

    public void OnSwitchWeapon4(InputAction.CallbackContext context)
    {
        if (inputDisabled) return;

        if (context.started)
        {
            wc.SwitchWeapon(3);
        }
    }

    public void OnSwitchWeapon5(InputAction.CallbackContext context)
    {
        if (inputDisabled) return;

        if (context.started)
        {
            wc.SwitchWeapon(4);
        }
    }

    public void OnSwitchWeapon6(InputAction.CallbackContext context)
    {
        if (inputDisabled) return;

        if (context.started)
        {
            wc.SwitchWeapon(5);
        }
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        if (inputDisabled) return;

        cc.look = context.ReadValue<Vector2>();
    }

    public void EnableInput()
    {
        inputDisabled = false;
    }

    public void DisableInput()
    {
        pc.moveInput = Vector2.zero;
        cc.look = Vector2.zero;

        inputDisabled = true;
    }
}

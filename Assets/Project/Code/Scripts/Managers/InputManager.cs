using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class InputManager : MonoSingleton<InputManager>
{
    [SerializeField] private bool inputDisabled;

    public bool jumpPressed;
    private CameraController cc;
    private Vector2 moveInput;
    private PlayerController pc;
    private Controls playerControls;
    private WeaponController wc;

    private new void Awake()
    {
        playerControls = new Controls();
    }

    private void Start()
    {
        pc = MonoSingleton<PlayerController>.Instance;
        cc = MonoSingleton<CameraController>.Instance;
        wc = MonoSingleton<WeaponController>.Instance;
    }

    // TODO: replace this with a proper pause menu and new input system
    private void Update()
    {
        if (inputDisabled) return;

        if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();

        if (playerControls.Player.PrimaryFire.ReadValue<float>() > 0.5f) wc.PrimaryFire();
        if (playerControls.Player.Jump.ReadValue<float>() > 0.5f) pc.Jump();

        jumpPressed = playerControls.Player.Jump.ReadValue<float>() > 0.5f;

        DeflectInput();
    }

    private void OnEnable()
    {
        playerControls.Enable();
        EnableInput();
    }

    private void OnDisable()
    {
        playerControls.Disable();
    }

    private void DeflectInput()
    {
        var deflectPressed = playerControls.Player.Deflect.ReadValue<float>() > 0.5f;

        if (deflectPressed && !wc.isDeflecting && CooldownManager.Instance.CheckCooldown("Deflect")) wc.Deflect();
        if (!deflectPressed && wc.isDeflecting) wc.StopDeflect();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (inputDisabled) return;

        pc.moveInput = context.ReadValue<Vector2>();
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (inputDisabled) return;

        if (context.started) pc.Dash();
        // TODO: Implement dash
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (inputDisabled) return;

        if (context.started) pc.WallJump();
    }

    // public void OnPrimaryFire(InputAction.CallbackContext context)
    // {
    //     if (inputDisabled) return;
    //
    //     bool isButtonHeld = context.ReadValue<float>() > 0.1f;
    //
    //     if (isButtonHeld)
    //     {
    //         wc.PrimaryFire();
    //     }
    // }

    public void OnSecondaryFire(InputAction.CallbackContext context)
    {
        if (inputDisabled) return;

        if (context.started) wc.SecondaryFire();
    }

    public void OnReload(InputAction.CallbackContext context)
    {
        if (context.started)
            // TODO: Implement reload (or maybe not because reloading is boring)
            // reload scene for now
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        // StartCoroutine(pc.Respawn());
    }

    // switch weapons
    public void OnSwitchWeapon1(InputAction.CallbackContext context)
    {
        if (inputDisabled) return;

        if (context.started) wc.SwitchWeaponIndex(0);
    }

    public void OnSwitchWeapon2(InputAction.CallbackContext context)
    {
        if (inputDisabled) return;

        if (context.started) wc.SwitchWeaponIndex(1);
    }

    public void OnSwitchWeapon3(InputAction.CallbackContext context)
    {
        if (inputDisabled) return;

        if (context.started) wc.SwitchWeaponIndex(2);
    }

    public void OnSwitchWeapon4(InputAction.CallbackContext context)
    {
        if (inputDisabled) return;

        if (context.started) wc.SwitchWeaponIndex(3);
    }

    public void OnSwitchWeapon5(InputAction.CallbackContext context)
    {
        if (inputDisabled) return;

        if (context.started) wc.SwitchWeaponIndex(4);
    }

    public void OnSwitchWeapon6(InputAction.CallbackContext context)
    {
        if (inputDisabled) return;

        if (context.started) wc.SwitchWeaponIndex(5);
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        if (inputDisabled) return;

        cc.look = context.ReadValue<Vector2>();
    }

    // public void OnDeflect(InputAction.CallbackContext context)
    // {
    //     if (inputDisabled) return;
    //
    //     if (context.started && !wc.isDeflecting)
    //     {
    //         wc.Deflect();
    //     }
    //
    //     if (context.canceled && wc.isDeflecting)
    //     {
    //         wc.Deflect();
    //     }
    //
    // }

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
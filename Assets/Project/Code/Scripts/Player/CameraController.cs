using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

public class CameraController : MonoSingleton<CameraController>
{
    [Header("Camera Settings")] [SerializeField]
    private float sensitivity;

    [SerializeField] private Camera cam;

    [FormerlySerializedAs("fov")] [SerializeField]
    private float defaultFov;

    public Vector2 look;
    [SerializeField] private float fovTransitionTime;
    [SerializeField] private float dashFovDif;
    private float currentFov;
    private float lookRotation;
    private PlayerController pc;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        pc = MonoSingleton<PlayerController>.Instance;
        currentFov = defaultFov;
    }

    private void Update()
    {
        RotateCamera();
        DashFov();
        if (pc.movementState != MovementState.WALLRUNNING) LeanCameraTowardsPlayerMovement(pc.moveInput);
    }

    private void DashFov()
    {
        if (pc.dashing)
        {
            if (Vector3.Dot(pc.dashDirection, transform.forward) > 0.95f)
                SetFOV(defaultFov + dashFovDif);
            else if (Vector3.Dot(pc.dashDirection, transform.forward) < 0.95f) SetFOV(defaultFov - dashFovDif);
        }
        else
        {
            SetFOV(defaultFov);
        }
    }

    private void LeanCameraTowardsPlayerMovement(Vector2 moveInput)
    {
        var multiplier = 1f;
        if (!pc.gc.touchingGround) multiplier *= 2f;
        transform.DOLocalRotate(new Vector3(transform.localEulerAngles.x, 0f, -moveInput.x * 1.5f * multiplier), 0.2f);
    }

    public void LeanCameraByAngle(float angle)
    {
        transform.DOLocalRotate(new Vector3(transform.localEulerAngles.x, 0f, -angle), 0.2f);
    }

    public void ResetLean()
    {
        transform.DOLocalRotate(new Vector3(transform.localEulerAngles.x, 0f, 0f), 0.2f);
    }

    public void ResetCamera()
    {
        look = Vector2.zero;
        lookRotation = 0f;
        transform.eulerAngles = new Vector3(0f, 0f, 0f);
        transform.DOKill(this);
    }

    private void RotateCamera()
    {
        look *= sensitivity / 100;
        pc.rb.MoveRotation(pc.rb.rotation * Quaternion.Euler(0f, look.x, 0f));
        lookRotation += -look.y;
        lookRotation = Math.Clamp(lookRotation, -89.5f, 89.5f);

        var cameraObj = transform.GetChild(0);

        cameraObj.transform.localEulerAngles
            = new Vector3(
                lookRotation,
                0f,
                transform.localEulerAngles.z
            );
    }

    private void SetFOV(float fov)
    {
        DOVirtual.Float(currentFov, fov, fovTransitionTime, CurrentFov);
        cam.fieldOfView = currentFov;
    }

    private void CurrentFov(float value)
    {
        currentFov = value;
    }

    public void ShowSpeedLines()
    {
    }
}
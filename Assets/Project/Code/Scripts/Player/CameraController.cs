using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.InputSystem;

public class CameraController : MonoSingleton<CameraController>
{
    [Header("Camera Settings")]
    [SerializeField] private float sensitivity;
    private PlayerController pc;
    [SerializeField] private Camera cam;
    [SerializeField] private float fov;
    private float currentFov;
    private float lookRotation;
    public Vector2 look;
    [SerializeField] private float fovTransitionTime;
    [SerializeField] private float dashFovDif;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        pc = MonoSingleton<PlayerController>.Instance;
        currentFov = fov;
    }

    private void Update()
    {
        RotateCamera();
        DashFov();
        LeanCameraTowardsPlayerMovement(pc.moveInput);
    }

    private void DashFov()
    {
        if (pc.dashing)
        {
            if (Vector3.Dot(pc.dashDirection, transform.forward) > 0.95f)
            {
                SetFOV(fov + dashFovDif);
            }
            else if (Vector3.Dot(pc.dashDirection, transform.forward) < 0.95f)
            {
                SetFOV(fov - dashFovDif);
            }
        }
        else
        {
            SetFOV(fov);
        }
    }

    public void LeanCameraTowardsPlayerMovement(Vector2 moveInput)
    {
        if (moveInput != Vector2.zero/* && !pc.gc.touchingGround*/)
        {
            var multiplier = 1f;
            if (!pc.gc.touchingGround) multiplier *= 2f;
            transform.DOLocalRotate(new Vector3(transform.localEulerAngles.x, 0f, -moveInput.x * 2f * multiplier), 0.2f);
        }
        else
        {
            transform.DOLocalRotate(new Vector3(transform.localEulerAngles.x, 0f, 0f), 0.2f);
        }
    }

    public void ResetCamera()
    {
        look = Vector2.zero;
        lookRotation = 0f;
        transform.eulerAngles = new Vector3(0f,0f, 0f);
        transform.DOKill(this);
    }

    private void RotateCamera()
    {
        look *= sensitivity / 100;
        pc.rb.MoveRotation(pc.rb.rotation * Quaternion.Euler(0f, look.x, 0f));
        lookRotation += -look.y;
        lookRotation = Math.Clamp(lookRotation, -89.5f, 89.5f);
        transform.localEulerAngles
            = new Vector3(
                lookRotation,
                0f,
                transform.localEulerAngles.z
            );
    }

    private float SetFOV(float fov)
    {

        DOVirtual.Float(currentFov, fov, fovTransitionTime, CurrentFov);
        cam.fieldOfView = currentFov;
        return cam.fieldOfView;
    }

    private void CurrentFov(float value)
    {
        currentFov = value;
    }

    public void ShowSpeedLines()
    {

    }
}
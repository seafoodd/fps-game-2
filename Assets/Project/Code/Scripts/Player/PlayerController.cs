using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class PlayerController : MonoSingleton<PlayerController>
{
    public Rigidbody rb;
    private InputManager im;

    [Header("Movement Settings")]
    [SerializeField] private float speed = 10.0f;
    [SerializeField] private float jumpForce;
    [SerializeField] private float movementSmoothing;
    public MovementState movementState;
    [HideInInspector] public bool dashing;
    [HideInInspector] public Vector3 dashDirection;
    private bool isGrounded;
    public Vector2 moveInput;
    private GroundCheck gc;
    private bool jumping;
    private Vector3 movementDirectionNormalized;
    private Vector3 targetVelocity;
    private Vector3 currentVelocityHorizontal;
    private bool externalForcesApplied;
    private int health = 100;
    private CapsuleCollider col;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource aud;
    [SerializeField] private AudioClip[] stepSounds;
    [SerializeField] private AudioClip[] jumpSounds;
    [SerializeField] private AudioClip dashSound;
    public bool dead;
    private UIController uic;
    [SerializeField] private float dashLength = 1f;
    private CooldownManager cm;


    private void Start()
    {
        col = GetComponent<CapsuleCollider>();
        aud = GetComponent<AudioSource>();
        gc = GetComponentInChildren<GroundCheck>();
        rb = GetComponent<Rigidbody>();
        im = MonoSingleton<InputManager>.Instance;
        uic = MonoSingleton<UIController>.Instance;
        cm = CooldownManager.Instance;

        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void FixedUpdate()
    {
        if (dead) return;

        Move();
    }

    public void GetHit(int damage)
    {
        health -= damage;
        uic.UpdateHealth(health);

        Debug.Log("Damage taken: " + damage + ", health remaining: " + health);
        if (health <= 0)
        {
            Die();
        }
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        var spawnPoint = GameObject.FindWithTag("SpawnPoint");
        StartCoroutine(Respawn(spawnPoint == null ? null : spawnPoint.transform));
    }

    // respawn player
    public IEnumerator Respawn(Transform spawnPoint = null)
    {
        yield return new WaitForEndOfFrame();

        health = 100;
        dead = false;
        rb.freezeRotation = true;
        transform.position = spawnPoint == null ? Vector3.zero : spawnPoint.position;
        transform.rotation = Quaternion.Euler(0, 0, 0);
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        im.EnableInput();
        uic.UpdateHealth(health);
        uic.HideDeathScreen();
        cm.ResetAllCharges();
        cm.ResetAllCooldowns();

        MonoSingleton<CameraController>.Instance.ResetCamera();
        MonoSingleton<GoreZone>.Instance.ResetGore();
    }

    private void Die()
    {
        if (dead) return;
        dead = true;
        im.DisableInput();
        rb.freezeRotation = false;
        rb.useGravity = true;
        rb.AddForce(Vector3.up * 10, ForceMode.Impulse);
        rb.AddTorque(Vector3.right * 20, ForceMode.Impulse);
        StartCoroutine(uic.ShowDeathScreen());
    }

    private void Move()
    {
        movementDirectionNormalized = new Vector3(moveInput.x, 0, moveInput.y).normalized;
        movementDirectionNormalized = transform.TransformDirection(movementDirectionNormalized);
        targetVelocity = movementDirectionNormalized * speed;

        currentVelocityHorizontal = Vector3.ProjectOnPlane(rb.velocity, transform.up);

        rb.velocity = Vector3.Lerp(rb.velocity, new Vector3(targetVelocity.x, rb.velocity.y, targetVelocity.z),
            movementSmoothing);
        if (moveInput.x == 0f && moveInput.y == 0f && gc.touchingGround)
        {
            if (!jumping && !dashing && !externalForcesApplied)
            {
                rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            }

            rb.useGravity = false;
            movementState = MovementState.IDLE;
        }
        else
        {
            rb.useGravity = true;
            if (gc.touchingGround && !jumping && !dashing && !externalForcesApplied)
            {
                PlayStepSounds();
                movementState = MovementState.WALKING;
            }
            else
            {
                movementState = MovementState.FALLING;
            }
        }
    }

    private void PlayStepSounds()
    {
        if (aud.isPlaying) return;
        aud.pitch = 1f;
        // aud.volume = 0.2f;
        var clip = stepSounds[UnityEngine.Random.Range(0, stepSounds.Length)];
        aud.PlayOneShot(clip, 0.2f);
    }

    private void PlayJumpSound()
    {
        // im stopping it to prevent the dash sound from being too loud
        aud.pitch = 1f;
        var clip = jumpSounds[UnityEngine.Random.Range(0, jumpSounds.Length)];
        aud.PlayOneShot(clip, 0.7f);
    }

    public void Jump()
    {
        if (!gc.touchingGround || jumping)
        {
            return;
        }
        jumping = true;
        Invoke("NotJumping", 0.25f);
        var velocity = rb.velocity;
        velocity = new Vector3(velocity.x, 0, velocity.z);
        rb.velocity = velocity;
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        PlayJumpSound();

        movementState = MovementState.FALLING;
    }

    private void NotJumping()
    {
        jumping = false;
    }

    public void AirFreeze()
    {
        // if (!CooldownManager.Instance.CheckCooldown("airFreeze")) return;
        // CooldownManager.Instance.AddCooldown("airFreeze", 0.1f);
        if (movementState != MovementState.FALLING) return;
        rb.velocity = Vector3.zero;
        rb.AddForce(Vector3.up * 2f, ForceMode.Impulse);
    }

    public void Dash()
    {
        if (CooldownManager.Instance.dashCharges < 1) return;

        Debug.Log("Dash charges: " + CooldownManager.Instance.dashCharges);
        var camTransform = CameraController.Instance.transform;
        var camDirection = camTransform.forward;
        camDirection.Normalize();

        // TODO: fix this so it doesn't require locking vertical camera at 89.5 degrees max to work
        var direction = movementDirectionNormalized == Vector3.zero ? camDirection : Vector3.ProjectOnPlane(movementDirectionNormalized, Camera.main.transform.up).normalized;

        var targetPosition = transform.position + direction * dashLength;
        RaycastHit hit;

        // check with 3 rays from head, center and feet
        var raycastOrigins = new Vector3[3];
        raycastOrigins[0] = transform.position;
        // var colHeight = col.height / 2f;
        var colHeight = 0.5f;
        Debug.Log(colHeight);
        raycastOrigins[1] = transform.position + Vector3.up * colHeight;
        raycastOrigins[2] = transform.position + Vector3.down * colHeight;

        int layerMask = 1 << LayerMask.NameToLayer("Ground");


        bool hitSomething = false;
        var distance = dashLength;
        foreach (var origin in raycastOrigins)
        {
            if (Physics.Raycast(origin, direction, out hit, dashLength, layerMask))
            {
                hitSomething = true;
                if (hit.distance < distance) distance = hit.distance;
            }
        }

        if (distance < 0.4f) return;

        if (hitSomething)
        {
            targetPosition = transform.position + direction * distance;
            targetPosition -= direction * 0.33f;
        }

        // move the player to the target position very quickly but smoothly
        StartCoroutine(BlinkToPosition(targetPosition, 0.05f));
    }

    private IEnumerator BlinkToPosition(Vector3 targetPosition, float f)
    {
        aud.pitch = 0.5f + 0.25f / (int)CooldownManager.Instance.dashCharges;
        aud.PlayOneShot(dashSound, 0.15f);
        CooldownManager.Instance.dashCharges--;
        rb.velocity = Vector3.zero;
        rb.useGravity = false;
        col.enabled = false;
        dashing = true;

        var initialHeight = col.height;
        var initialCenter = col.center;
        col.height = 1f;
        col.center = new Vector3(0, 0.5f, 0);

        var startPosition = transform.position;
        var t = 0f;
        while (t < 1)
        {
            t += Time.deltaTime / f;
            transform.position = Vector3.LerpUnclamped(startPosition, targetPosition, t);
            yield return null;
        }

        col.height = initialHeight;
        col.center = initialCenter;
        rb.useGravity = true;
        col.enabled = true;
        dashing = false;
    }
}
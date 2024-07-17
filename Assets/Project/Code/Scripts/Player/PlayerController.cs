using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class PlayerController : MonoSingleton<PlayerController>
{
    public Rigidbody rb;
    private InputManager im;

    [Header("Movement Settings")] [SerializeField]
    private float speed = 10.0f;

    [SerializeField] private float jumpForce;
    [SerializeField] private float movementSmoothing;
    public MovementState movementState;
    [HideInInspector] public bool dashing;
    [HideInInspector] public Vector3 dashDirection;
    private bool isGrounded;
    public Vector2 moveInput;
    public GroundCheck gc;
    private bool jumping;
    private Vector3 movementDirectionNormalized;
    private Vector3 targetVelocity;
    private Vector3 currentVelocityHorizontal;
    private bool externalForcesApplied;
    private int health = 100;
    private CapsuleCollider col;
    private bool isWallRunning;

    [Header("Audio Settings")] [SerializeField]
    private AudioSource aud;

    [SerializeField] private AudioClip[] stepSounds;
    [SerializeField] private AudioClip[] jumpSounds;
    [SerializeField] private AudioClip dashSound;
    public bool dead;
    private UIController uic;
    [SerializeField] private float dashLength = 1f;
    private CooldownManager cm;
    private WallCheck wac;
    private float wallRunningSeconds;
    private Vector3 wallRunDir;
    private Vector3 wallRunDirNew;
    private Vector3 movementDirection;
    public float airTime;
    public float fallTime;
    private CameraController cc;


    private void Start()
    {
        col = GetComponent<CapsuleCollider>();
        aud = GetComponent<AudioSource>();
        gc = GetComponentInChildren<GroundCheck>();
        rb = GetComponent<Rigidbody>();
        im = MonoSingleton<InputManager>.Instance;
        uic = MonoSingleton<UIController>.Instance;
        cm = CooldownManager.Instance;
        wac = MonoSingleton<WallCheck>.Instance;
        cc = MonoSingleton<CameraController>.Instance;

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

        // Debug.Log("Damage taken: " + damage + ", health remaining: " + health);
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
        wac.ResetWallCheck();
        gc.ResetGroundCheck();
        fallTime = 0f;
        airTime = 0f;
        wallRunningSeconds = 0f;

        cc.ResetCamera();
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
        movementDirection = new Vector3(moveInput.x, 0, moveInput.y);
        movementDirection = transform.TransformDirection(movementDirection);
        movementDirectionNormalized = movementDirection.normalized;
        targetVelocity = movementDirectionNormalized * speed;

        currentVelocityHorizontal = Vector3.ProjectOnPlane(rb.velocity, transform.up);

        if (moveInput.x == 0f && moveInput.y == 0f && gc.touchingGround)
        {
            if (!jumping && !dashing && !externalForcesApplied)
            {
                rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            }

            rb.useGravity = false;
            SetMovementState(MovementState.IDLE);
            rb.velocity = Vector3.Lerp(rb.velocity, new Vector3(targetVelocity.x, rb.velocity.y, targetVelocity.z),
                movementSmoothing);
        }
        else if (moveInput.y != 0f && cm.CheckCooldown("wallRun") /*&& im.jumpPressed */&& wac.touchingWall && !gc.touchingGround && !jumping &&
                 !dashing && !externalForcesApplied)
        {
            WallRun();
        }
        else
        {
            rb.useGravity = true;
            if (gc.touchingGround && !jumping && !dashing && !externalForcesApplied)
            {
                PlayStepSounds();
                SetMovementState(MovementState.WALKING);
                rb.velocity = Vector3.Lerp(rb.velocity, new Vector3(targetVelocity.x, rb.velocity.y, targetVelocity.z),
                    movementSmoothing);
            }
            else
            {
                SetMovementState(MovementState.FALLING);
                // rb.velocity = Vector3.Lerp(rb.velocity,
                //     new Vector3(targetVelocity.x, rb.velocity.y, targetVelocity.z),
                //     movementSmoothing / 10f);


                var momentum = rb.velocity.magnitude / 5f;
                momentum = Mathf.Clamp(momentum, 1f, 5f);

                if (rb.velocity == Vector3.zero) return;
                var airControl = currentVelocityHorizontal.magnitude * 10f;
                airControl = Mathf.Clamp(airControl, 1f, 20f);
                bool isSlowingDown = Vector3.Dot(currentVelocityHorizontal, targetVelocity) < 0;


                targetVelocity += targetVelocity * momentum;
                if (targetVelocity.magnitude < 20f) rb.AddForce(movementDirection * 5f, ForceMode.Force);
                // if (targetVelocity.magnitude < currentVelocityHorizontal.magnitude)
                //     targetVelocity += targetVelocity * momentum;

                Debug.Log(currentVelocityHorizontal.magnitude + " " + momentum + " " + targetVelocity.magnitude);
                targetVelocity = Vector3.ClampMagnitude(targetVelocity, 20f);

                if (targetVelocity.magnitude > 0f)
                {
                    Vector3 force = movementDirection * 12f;
                    Vector3.ClampMagnitude(force, 20f);
                    // Debug.Log(currentVelocityHorizontal.magnitude + " " + airControl);
                    rb.AddForce(force);
                    rb.velocity = Vector3.Lerp(rb.velocity, new Vector3(targetVelocity.x, rb.velocity.y, targetVelocity.z),
                        movementSmoothing / airControl * (isSlowingDown ? 3f : 1f));
                }

                // falling
                // Debug.Log(rb.velocity.y);

                airTime += Time.deltaTime;
                if (rb.velocity.y < 0)
                {
                    fallTime += Time.deltaTime;
                    rb.AddForce(Vector3.down * fallTime * 4f);
                    // Debug.Log("fallTime " + fallTime + " " + rb.velocity.y);
                    return;
                }

                fallTime = 0f;
            }
        }

        // set max speed
        Vector3.ClampMagnitude(rb.velocity,  25f);
    }

    private void WallRun()
    {
        rb.useGravity = false;
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        var lookDir = Camera.main.transform.forward;
        // lookDir.y = 0;

        var wallParallel = Vector3.Cross(wac.hitInfo.normal, Vector3.up);
        wallRunDir = Vector3.Project(lookDir, wallParallel).normalized;
        wallRunDirNew = Vector3.ProjectOnPlane(lookDir, wac.hitInfo.normal);

        // backwards wallrun
        wallRunDirNew *= moveInput.y;

        // only run up the wall
        if (wallRunDirNew.y < 0) wallRunDirNew.y = 0;


        wallRunDirNew.Normalize();


        // bool left = Vector3.Dot(wallRunDirNew, wallParallel) > 0;
        // Debug.Log(left);
        // var dir = Vector3.Project(wallRunDirNew, wac.hitInfo.normal).normalized;
        // cc.LeanCameraTowardsDirection(dir, 15f);

        if (wallRunningSeconds < 3f) wallRunningSeconds += Time.deltaTime;
        SetMovementState(MovementState.WALLRUNNING);
        PlayStepSounds();
        targetVelocity = wallRunDirNew * (speed * 1.25f + wallRunningSeconds * 1.5f);

        var leanAngle = 8f * Vector3.Dot(wallRunDirNew, wallParallel);

        // var leanAngle = 8f * Vector3.Dot(lookDir, wallRunDirNew) * moveInput.y;
        if (Vector3.Dot(lookDir, wallRunDirNew) < 0.4f)
        {
            // leanAngle *= Vector3.Dot(lookDir, wallRunDirNew);
            targetVelocity *= 0.6f;
        }
        leanAngle *= Vector3.Dot(lookDir, wallRunDirNew);
        cc.LeanCameraByAngle(leanAngle);

        rb.velocity = Vector3.Lerp(rb.velocity, new Vector3(targetVelocity.x, targetVelocity.y, targetVelocity.z),
            movementSmoothing);
    }

    public void WallJump()
    {
        if(movementState != MovementState.WALLRUNNING) return;
        rb.useGravity = true;
        // rb.velocity = Vector3.zero;
        rb.AddForce(wac.hitInfo.normal * 5f, ForceMode.Impulse);
        rb.AddForce(Vector3.up * 5f, ForceMode.Impulse);

        aud.pitch = 1f;
        aud.PlayOneShot(jumpSounds[0], 0.7f);
        SetMovementState(MovementState.FALLING);
    }

    private void SetMovementState(MovementState state)
    {
        var previousState = movementState;
        if (previousState == state) return;

        // if (state == MovementState.WALLRUNNING)
        // {
        //     // var angle = Vector3.Angle(wac.hitInfo.normal, Vector3.up);
        //     var angle = 3f;
        //
        //
        //     cc.LeanCameraTowardsDirection(wac.hitInfo.normal, angle);
        // }

        if (previousState == MovementState.WALLRUNNING && state == MovementState.FALLING)
        {
            // WallJump();
            cc.ResetLean();
            wallRunningSeconds = 0f;
            cm.AddCooldown("wallRun", 0.5f);
        }

        if (previousState == MovementState.FALLING)
        {
            airTime = 0f;
            fallTime = 0f;
        }

        movementState = state;
    }

    // private void WallRun()
    // {
    //     if (wac.touchingWall)
    //     {
    //         rb.AddForce(Vector3.up * 2f, ForceMode.Impulse);
    //     }
    // }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        // Gizmos.DrawRay(transform.position, wallRunDirection * 2);
        // Gizmos.DrawRay(Camera.main.transform.position, camForward * 2);
        // Gizmos.DrawRay(transform.position, wallNormal * 2);
        // Gizmos.color = Color.blue;
        // var dir = Camera.main.transform.forward;
        // // dir.x = 0;
        // dir.y = 0;
        // // dir.z = 0;
        // dir.Normalize();
        //
        // // dir = Vector3.Cross(dir,  wac.hitInfo.normal);
        //
        // var n = wac.hitInfo.normal;
        // n.y = 0;
        // n.Normalize();
        //
        // dir = Vector3.Project(dir, n).normalized;
        //
        Gizmos.DrawRay(transform.position, wallRunDirNew * 4);
        // Gizmos.DrawRay(Camera.main.transform.position, dir * 4);
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
        if (!gc.touchingGround || jumping || movementState == MovementState.WALLRUNNING)
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

        SetMovementState(MovementState.FALLING);
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

        // rb.velocity = Vector3.zero;
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(Vector3.up * 2f, ForceMode.Impulse);
    }

    public void Dash()
    {
        if (CooldownManager.Instance.dashCharges < 1) return;

        // Debug.Log("Dash charges: " + CooldownManager.Instance.dashCharges);
        var camTransform = CameraController.Instance.transform;
        var camDirection = camTransform.forward;
        camDirection.Normalize();

        // TODO: fix this so it doesn't require locking vertical camera at 89.5 degrees max to work
        var direction = movementDirectionNormalized == Vector3.zero
            ? camDirection
            : Vector3.ProjectOnPlane(movementDirectionNormalized, Camera.main.transform.up).normalized;

        var targetPosition = transform.position + direction * dashLength;
        RaycastHit hit;

        // check with 3 rays from head, center and feet
        var raycastOrigins = new Vector3[3];
        raycastOrigins[0] = transform.position;
        // var colHeight = col.height / 2f;
        var colHeight = 0.5f;
        // Debug.Log(colHeight);
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
        // var initialVelocity = rb.velocity;
        // initialVelocity.y = 0f;
        rb.velocity = Vector3.zero;
        fallTime = 0f;
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
        rb.velocity = Vector3.zero;
        // rb.velocity = Vector3.Project(initialVelocity, targetPosition - startPosition);
        rb.useGravity = true;
        col.enabled = true;
        dashing = false;
    }
}
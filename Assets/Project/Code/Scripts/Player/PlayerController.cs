using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoSingleton<PlayerController>
{
    public Rigidbody rb;

    [Header("Movement Settings")] [SerializeField]
    private float speed = 10.0f;

    [SerializeField] private float jumpForce;
    [SerializeField] private float movementSmoothing;
    public MovementState movementState;
    [HideInInspector] public bool dashing;
    [HideInInspector] public Vector3 dashDirection;
    public Vector2 moveInput;
    public GroundCheck gc;

    [Header("Audio Settings")] [SerializeField]
    private AudioSource aud;

    [SerializeField] private AudioClip[] stepSounds;
    [SerializeField] private AudioClip[] jumpSounds;
    [SerializeField] private AudioClip dashSound;
    public bool dead;
    [SerializeField] private float dashLength = 1f;
    public float airTime;
    public float fallTime;
    private CameraController cc;
    private CooldownManager cm;
    private CapsuleCollider col;
    private Vector3 currentVelocityHorizontal;
    private bool externalForcesApplied;
    private int health = 100;
    private InputManager im;
    private bool isGrounded;
    private bool isWallRunning;
    private bool jumping;
    private Vector3 movementDirection;
    private Vector3 movementDirectionNormalized;
    private Vector3 targetVelocity;
    private UIController uic;
    private WallCheck wac;
    private Vector3 wallRunDirNew;
    private float wallRunningSeconds;


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

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void FixedUpdate()
    {
        if (dead) return;

        Move();
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

    public void GetHit(int damage)
    {
        health -= damage;
        uic.UpdateHealth(health);

        if (health <= 0) Die();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        var spawnPoint = GameObject.FindWithTag("SpawnPoint");
        StartCoroutine(Respawn(spawnPoint == null ? null : spawnPoint.transform));
    }

    // respawn player
    public IEnumerator Respawn(Transform spawnPoint = null)
    {
        yield return new WaitForEndOfFrame();

        StopAllCoroutines();
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
        col.height = 1.65f;
        col.center = Vector3.zero;
        // rb.useGravity = true;
        col.enabled = true;
        dashing = false;

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
                rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            rb.useGravity = false;
            SetMovementState(MovementState.IDLE);
            rb.velocity = Vector3.Lerp(rb.velocity, new Vector3(targetVelocity.x, rb.velocity.y, targetVelocity.z),
                movementSmoothing);
        }
        else if (moveInput.y != 0f && cm.CheckCooldown("wallRun") && wac.touchingWall && !gc.touchingGround &&
                 !jumping &&
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

                var momentum = rb.velocity.magnitude / 5f;
                momentum = Mathf.Clamp(momentum, 1f, 5f);

                if (rb.velocity == Vector3.zero) return;
                var airControl = currentVelocityHorizontal.magnitude * 10f;
                airControl = Mathf.Clamp(airControl, 1f, 20f);
                var isSlowingDown = Vector3.Dot(currentVelocityHorizontal, targetVelocity) < 0;


                targetVelocity += targetVelocity * momentum;
                if (targetVelocity.magnitude < 20f) rb.AddForce(movementDirection * 5f, ForceMode.Force);
                // if (targetVelocity.magnitude < currentVelocityHorizontal.magnitude)
                //     targetVelocity += targetVelocity * momentum;

                // Debug.Log(currentVelocityHorizontal.magnitude + " " + momentum + " " + targetVelocity.magnitude);
                targetVelocity = Vector3.ClampMagnitude(targetVelocity, 20f);

                if (targetVelocity.magnitude > 0f)
                {
                    var force = movementDirection * 12f;
                    Vector3.ClampMagnitude(force, 20f);
                    // Debug.Log(currentVelocityHorizontal.magnitude + " " + airControl);
                    rb.AddForce(force);
                    rb.velocity = Vector3.Lerp(rb.velocity,
                        new Vector3(targetVelocity.x, rb.velocity.y, targetVelocity.z),
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
        Vector3.ClampMagnitude(rb.velocity, 25f);
    }

    private void WallRun()
    {
        rb.useGravity = false;
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        if (Camera.main == null) return;
        var lookDir = Camera.main.transform.forward;

        var wallParallel = Vector3.Cross(wac.hitInfo.normal, Vector3.up);
        wallRunDirNew = Vector3.ProjectOnPlane(lookDir, wac.hitInfo.normal);

        // backwards wallrun
        wallRunDirNew *= moveInput.y;

        // only run up the wall
        if (wallRunDirNew.y < 0) wallRunDirNew.y = 0;

        wallRunDirNew.Normalize();


        if (wallRunningSeconds < 3f) wallRunningSeconds += Time.deltaTime;
        SetMovementState(MovementState.WALLRUNNING);
        PlayStepSounds();
        targetVelocity = wallRunDirNew * (speed * 1.25f + wallRunningSeconds * 1.5f);

        // make climbing faster
        if (wallRunDirNew.y > 0.5f) targetVelocity += Vector3.up * 10f;

        var leanAngle = 8f * Vector3.Dot(wallRunDirNew, wallParallel);

        if (Vector3.Dot(lookDir, wallRunDirNew) < 0.4f) targetVelocity *= 0.6f;
        leanAngle *= Vector3.Dot(lookDir, wallRunDirNew);
        cc.LeanCameraByAngle(leanAngle);

        rb.velocity = Vector3.Lerp(rb.velocity, new Vector3(targetVelocity.x, targetVelocity.y, targetVelocity.z),
            movementSmoothing);
    }

    public void WallJump()
    {
        if (movementState != MovementState.WALLRUNNING) return;
        rb.useGravity = true;
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

    private void PlayStepSounds()
    {
        if (aud.isPlaying) return;
        aud.pitch = 1f;
        var clip = stepSounds[Random.Range(0, stepSounds.Length)];
        aud.PlayOneShot(clip, 0.2f);
    }

    private void PlayJumpSound()
    {
        // im stopping it to prevent the dash sound from being too loud
        aud.pitch = 1f;
        var clip = jumpSounds[Random.Range(0, jumpSounds.Length)];
        aud.PlayOneShot(clip, 0.7f);
    }

    public void Jump()
    {
        if (!gc.touchingGround || jumping || movementState == MovementState.WALLRUNNING) return;

        jumping = true;
        Invoke(nameof(NotJumping), 0.25f);
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
        if (movementState != MovementState.FALLING) return;
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(Vector3.up * 2f, ForceMode.Impulse);

        // slow down the player on air hit
        rb.velocity *= 0.85f;
    }

    public void Dash()
    {
        if (CooldownManager.Instance.dashCharges < 1) return;

        var camTransform = CameraController.Instance.transform;
        // var camDirection = camTransform.forward;
        // camDirection.Normalize();

        // TODO: fix this so it doesn't require locking vertical camera at 89.5 degrees max to work
        var direction = Vector3
            .ProjectOnPlane(
                movementDirectionNormalized == Vector3.zero ? transform.forward : movementDirectionNormalized,
                Camera.main.transform.up).normalized;

        var targetPosition = transform.position + direction * dashLength;
        RaycastHit hit;

        // check with 3 rays from head, center and feet
        var raycastOrigins = new Vector3[3];
        raycastOrigins[0] = transform.position;
        var colHeight = 0.5f;
        raycastOrigins[1] = transform.position + Vector3.up * colHeight;
        raycastOrigins[2] = transform.position + Vector3.down * colHeight;

        var layerMask = 1 << LayerMask.NameToLayer("Ground");


        var hitSomething = false;
        var distance = dashLength;
        foreach (var origin in raycastOrigins)
            if (Physics.Raycast(origin, direction, out hit, dashLength, layerMask))
            {
                hitSomething = true;
                if (hit.distance < distance) distance = hit.distance;
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
        aud.PlayOneShot(dashSound, 0.2f);
        CooldownManager.Instance.dashCharges--;
        var initialVelocity = currentVelocityHorizontal;
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
            rb.MovePosition(Vector3.LerpUnclamped(startPosition, targetPosition, t));
            yield return null;
        }

        col.height = initialHeight;
        col.center = initialCenter;
        rb.velocity = (targetPosition - startPosition).normalized * 10f;
        rb.useGravity = true;
        col.enabled = true;
        dashing = false;
    }
}
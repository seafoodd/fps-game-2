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

    [Header("Audio Settings")]
    [SerializeField] private AudioSource aud;
    [SerializeField] private AudioClip[] stepSounds;
    [SerializeField] private AudioClip[] jumpSounds;
    public bool dead;
    private UIController uic;


    private void Start()
    {
        aud = GetComponent<AudioSource>();
        gc = GetComponentInChildren<GroundCheck>();
        rb = GetComponent<Rigidbody>();
        im = MonoSingleton<InputManager>.Instance;
        uic = MonoSingleton<UIController>.Instance;

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
        aud.volume = 0.2f;
        aud.clip = stepSounds[UnityEngine.Random.Range(0, stepSounds.Length)];
        aud.Play();
    }
    
    private void PlayJumpSound()
    {
        aud.volume = 0.7f;
        aud.clip = jumpSounds[UnityEngine.Random.Range(0, jumpSounds.Length)];
        aud.Play();
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
}
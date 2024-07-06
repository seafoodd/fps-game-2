using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Zombie : Damageable
{

    [SerializeField] private Transform shootPoint;
    [SerializeField] private Animator anim;
    [SerializeField] private float attackRange = 5.0f;
    [SerializeField] private float viewRange = 10.0f;
    private bool canAttack = true;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private AudioClip[] shootSounds;
    [SerializeField] private AudioSource aud;
    public MovementState movementState;
    private GroundCheck gc;

    const string IDLE = "Idle";
    const string WALKING = "Walking";
    const string ATTACK = "Attack";

    public void GetHit(GameObject target, int damage, float bulletForce, Vector3 hitPoint)
    {
        string hitLimb = target.gameObject.tag;

        Rigidbody rb = target.GetComponent<Rigidbody>();
        PlayerController player = MonoSingleton<PlayerController>.Instance;

        if (!dead)
        {
            if (hitLimb == "Head")
            {
                damage *= 2;
                Debug.Log("Headshot!");
                InstantiateBloodEffect(hitPoint, 20, true);
            }
            else
            {
                InstantiateBloodEffect(hitPoint);
            }

            health -= damage;
            Debug.Log($"Damage taken: {damage}, health remaining: {health}");

            if (health <= 0)
            {
                Die();
            }

            if (dead)
            {
                if (rb != null)
                {
                    rb.AddForceAtPosition(-(player.transform.position - hitPoint).normalized * bulletForce, hitPoint, ForceMode.Impulse);
                }
            }
            return;
        }

        if (hitLimb is "Head")
        {
            //TODO: add effects for destroying the head

            InstantiateBloodEffect(hitPoint, 20, true);
            Destroy(target);
        }

        else if (hitLimb is "Limb")
        {
            //TODO: add effects for destroying the limb

            // destroy the limb without destroying its children
            if (target.transform.childCount > 0)
            {
                foreach (Transform child in target.transform)
                {
                    // if the limb has some small details without rigidbody, destroy them
                     if (child.GetComponent<Rigidbody>() != null)
                    {
                        Destroy(child.GetComponent<CharacterJoint>());
                        child.parent = transform;
                    }
                }
            }

            InstantiateBloodEffect(hitPoint);
            Destroy(target);
        }

        else
        {
            InstantiateBloodEffect(hitPoint);
        }

        if (rb != null)
        {
            rb.AddForceAtPosition(-(player.transform.position - hitPoint).normalized * bulletForce, hitPoint, ForceMode.Impulse);
        }
    }

    private void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        gc = GetComponentInChildren<GroundCheck>();
    }

    public void GetHit(int damage)
    {
        health -= damage;
        AudioSource.PlayClipAtPoint(damageSound, transform.position);
        // Debug.Log("Damage taken: " + damage + ", health remaining: " + health);
        if (health <= 0 && !dead)
        {
            Die();
        }
    }

    private void Die()
    {
        dead = true;

        Destroy(nma);
        Destroy(rb);
        Destroy(col);
        Destroy(aud, 3f);

        if (deathSound != null)
        {
            AudioSource.PlayClipAtPoint(deathSound, transform.position);
        }
        // Disable the ragdoll components if they exist
        EnableRagdoll er = GetComponentInChildren<EnableRagdoll>();

        if (er != null) er.EnableRagdollComponents();
        // StartCoroutine(PlayDeathAnimation());

        StopAllCoroutines();
    }

    private void Update()
    {
        if (dead) return;

        if (!canAttack)
        {
            if(nma.isStopped) transform.DOLookAt(MonoSingleton<PlayerController>.Instance.transform.position, 0.1f, AxisConstraint.Y);
            return;
        }

        if (Vector3.Distance(transform.position, MonoSingleton<PlayerController>.Instance.transform.position) > viewRange)
        {
            ChangeAnimationState(IDLE);
            movementState = MovementState.IDLE;
            return;
        }

        if (Vector3.Distance(transform.position, MonoSingleton<PlayerController>.Instance.transform.position) <= attackRange)
        {
            Attack();
            return;
        }

        // if (nma.remainingDistance <= nma.stoppingDistance)
        // {
        //     nma.isStopped = true;
        //     ChangeAnimationState(IDLE);
        //     return;
        // }

        nma.isStopped = false;
        nma.SetDestination(MonoSingleton<PlayerController>.Instance.transform.position);
        ChangeAnimationState(WALKING);
        movementState = MovementState.WALKING;

    }

    private void Attack()
    {
        if (dead || !canAttack) return;
        canAttack = false;

        StartCoroutine(AttackRoutine());
    }

    private IEnumerator AttackRoutine()
    {
        nma.isStopped = true;
        ChangeAnimationState(ATTACK);
        movementState = MovementState.IDLE;
        yield return new WaitForSeconds(1f);

        // shoot the player 10 times
        for (int i = 0; i < 10; i++)
        {
            InstantiateProjectile(shootPoint.position, MonoSingleton<PlayerController>.Instance.transform.position, 5, 50, 100);
            aud.pitch = Random.Range(0.8f, 1f);
            aud.PlayOneShot(shootSounds[Random.Range(0, shootSounds.Length)]);
            yield return new WaitForSeconds(0.08f);
        }

        ChangeAnimationState(WALKING);
        movementState = MovementState.WALKING;
        // StartCoroutine(ResetAttack(5f));
        nma.isStopped = false;
        Vector3 randomPoint = MonoSingleton<PlayerController>.Instance.transform.position + new Vector3(Random.Range(-15, 15), 0, Random.Range(-15, 15));
        nma.SetDestination(randomPoint);

        // on destination reached or after 5 seconds, attack again
        yield return new WaitUntil(() => nma.remainingDistance <= nma.stoppingDistance);
        ChangeAnimationState(IDLE);
        movementState = MovementState.IDLE;
        canAttack = true;
    }

    // private IEnumerator ResetAttack(float time)
    // {
    //     yield return new WaitForSeconds(time);
    //     canAttack = true;
    // }

    private void InstantiateProjectile(Vector3 shootPoint, Vector3 target, int damage, float speed, float bulletForce)
    {
        GameObject projectile = Instantiate(projectilePrefab, shootPoint, Quaternion.identity);
        Projectile proj = projectile.GetComponent<Projectile>();

        // add spread based on the distance to the player
        float distance = Vector3.Distance(shootPoint, target);
        float spread = distance / 100;
        Vector3 randomDirection = new Vector3(Random.Range(-spread, spread), Random.Range(-spread, spread), Random.Range(-spread, spread));

        proj.damage = damage;
        proj.direction = (target - shootPoint).normalized;
        proj.direction += randomDirection;

        proj.bulletForce = bulletForce;
        proj.speed = speed;
    }

    private void ChangeAnimationState(string newState)
    {
        if (anim.GetCurrentAnimatorStateInfo(0).IsName(newState)) return;
        anim.Play(newState);
    }
}

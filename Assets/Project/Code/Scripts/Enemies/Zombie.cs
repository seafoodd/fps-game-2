using System.Collections;
using DG.Tweening;
using UnityEngine;

public class Zombie : Damageable
{
    private const string IDLE = "Idle";
    private const string WALKING = "Walking";
    private const string ATTACK = "Attack";
    [SerializeField] private Transform shootPoint;
    [SerializeField] private Animator anim;
    [SerializeField] private float attackRange = 5.0f;
    [SerializeField] private float viewRange = 10.0f;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private AudioClip[] shootSounds;
    [SerializeField] private AudioSource aud;
    public MovementState movementState;
    private bool canAttack = true;
    private GroundCheck gc;

    private void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        gc = GetComponentInChildren<GroundCheck>();
    }

    private void Update()
    {
        if (dead) return;

        if (!canAttack)
        {
            if (nma.isStopped)
                transform.DOLookAt(MonoSingleton<PlayerController>.Instance.transform.position, 0.1f, AxisConstraint.Y);
            return;
        }

        if (Vector3.Distance(transform.position, MonoSingleton<PlayerController>.Instance.transform.position) >
            viewRange)
        {
            ChangeAnimationState(IDLE);
            movementState = MovementState.IDLE;
            return;
        }

        if (Vector3.Distance(transform.position, MonoSingleton<PlayerController>.Instance.transform.position) <=
            attackRange)
        {
            Attack();
            return;
        }

        nma.isStopped = false;
        nma.SetDestination(MonoSingleton<PlayerController>.Instance.transform.position);
        ChangeAnimationState(WALKING);
        movementState = MovementState.WALKING;
    }

    public void GetHit(GameObject target, int damage, float bulletForce, Vector3 hitPoint)
    {
        var hitLimb = target.gameObject.tag;

        var limbRb = target.GetComponent<Rigidbody>();
        var player = MonoSingleton<PlayerController>.Instance;

        if (!dead)
        {
            if (hitLimb == "Head")
            {
                damage *= 2;
                InstantiateBloodEffect(hitPoint, 20, true);
            }
            else
            {
                InstantiateBloodEffect(hitPoint);
            }

            health -= damage;

            if (health <= 0) Die();

            if (dead)
                if (limbRb != null)
                    limbRb.AddForceAtPosition(-(player.transform.position - hitPoint).normalized * bulletForce,
                        hitPoint, ForceMode.Impulse);

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
                foreach (Transform child in target.transform)
                    // if the limb has some small details without rigidbody, destroy them
                    if (child.GetComponent<Rigidbody>() != null)
                    {
                        Destroy(child.GetComponent<CharacterJoint>());
                        child.parent = transform;
                    }

            InstantiateBloodEffect(hitPoint);
            Destroy(target);
        }

        else
        {
            InstantiateBloodEffect(hitPoint);
        }

        if (limbRb != null)
            limbRb.AddForceAtPosition(-(player.transform.position - hitPoint).normalized * bulletForce, hitPoint,
                ForceMode.Impulse);
    }

    public void GetHit(int damage)
    {
        health -= damage;
        AudioSource.PlayClipAtPoint(damageSound, transform.position);
        if (health <= 0 && !dead) Die();
    }

    private void Die()
    {
        dead = true;

        Destroy(nma);
        Destroy(rb);
        Destroy(col);
        Destroy(aud, 3f);

        if (deathSound != null) AudioSource.PlayClipAtPoint(deathSound, transform.position);

        // Disable the ragdoll components if they exist
        var er = GetComponentInChildren<EnableRagdoll>();

        if (er != null) er.EnableRagdollComponents();

        StopAllCoroutines();

        transform.parent = MonoSingleton<GoreZone>.Instance.goreZone;
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
        for (var i = 0; i < 10; i++)
        {
            InstantiateProjectile(shootPoint.position, MonoSingleton<PlayerController>.Instance.transform.position, 4,
                35, 100);
            aud.pitch = Random.Range(0.8f, 1f);
            aud.PlayOneShot(shootSounds[Random.Range(0, shootSounds.Length)]);
            yield return new WaitForSeconds(0.08f);
        }

        ChangeAnimationState(WALKING);
        movementState = MovementState.WALKING;
        nma.isStopped = false;
        var randomPoint = MonoSingleton<PlayerController>.Instance.transform.position +
                          new Vector3(Random.Range(-15, 15), 0, Random.Range(-15, 15));
        nma.SetDestination(randomPoint);

        // on destination reached or after 5 seconds, attack again
        yield return new WaitUntil(() => nma.remainingDistance <= nma.stoppingDistance);
        ChangeAnimationState(IDLE);
        movementState = MovementState.IDLE;
        canAttack = true;
    }

    private void InstantiateProjectile(Vector3 shootPoint, Vector3 target, int damage, float speed, float bulletForce)
    {
        var projectile = Instantiate(projectilePrefab, shootPoint, Quaternion.identity);
        var proj = projectile.GetComponent<Projectile>();

        // add spread based on the distance to the player
        var distance = Vector3.Distance(shootPoint, target);
        var spread = distance / 100;
        var randomDirection = new Vector3(Random.Range(-spread, spread), Random.Range(-spread, spread),
            Random.Range(-spread, spread));

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
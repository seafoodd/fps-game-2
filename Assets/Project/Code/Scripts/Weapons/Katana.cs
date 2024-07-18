using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Katana : MonoBehaviour
{
    [SerializeField] private int defaultDamage = 10;
    [SerializeField] private Animator anim;
    [SerializeField] private GameObject trail;
    [SerializeField] private Collider attackCollider;

    private readonly string[] AttacksLR =
    {
        "Attack L-R 1"
    };

    private readonly string[] AttacksRL =
    {
        "Attack R-L 1"
    };

    private readonly Dictionary<Transform, int> hitEnemiesTimes = new();

    private readonly string IDLE = "Idle";
    private readonly string SELECT = "Select";

    private bool attackRight = true;

    private CooldownManager cm;
    private bool hitSomething;

    private void Start()
    {
        cm = CooldownManager.Instance;
        anim = GetComponent<Animator>();
        attackCollider.enabled = false;
    }

    private void OnEnable()
    {
        anim.Play(SELECT);
        trail.SetActive(false);
        attackCollider.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("Limb")) return;
        var hitEnemy = other.transform;

        while (hitEnemy.parent != null && hitEnemy.parent.gameObject.name != "Gore" &&
               hitEnemy.parent.gameObject.name != "Enemies") hitEnemy = hitEnemy.parent;

        if (!hitEnemiesTimes.TryAdd(hitEnemy, 1))
        {
            hitEnemiesTimes[hitEnemy]++;
            if (hitEnemiesTimes[hitEnemy] > 3) return;
        }

        if (!hitEnemy.GetComponent<Zombie>().dead) hitSomething = true;
        DealDamage(other.gameObject, defaultDamage, 300, other.transform.position);
    }

    public void PrimaryFire()
    {
        if (!cm.CheckCooldown("Katana")) return;
        StopAllCoroutines();
        StartCoroutine(SlashAttack());
    }

    private IEnumerator SlashAttack()
    {
        cm.AddCooldown("Katana", 0.21f);
        attackCollider.enabled = true;
        hitSomething = false;
        var currentAnimation = attackRight
            ? AttacksRL[Random.Range(0, AttacksRL.Length)]
            : AttacksLR[Random.Range(0, AttacksLR.Length)];

        attackRight = !attackRight;
        anim.Play(currentAnimation);
        trail.SetActive(true);

        // wait until the animation is done
        yield return new WaitForSeconds(0.2f);

        trail.SetActive(false);
        anim.Play(SELECT);
        hitEnemiesTimes.Clear();
        attackCollider.enabled = false;
        if (hitSomething)
        {
            PlayerController.Instance.AirFreeze();
            hitSomething = false;
        }

        // after select animation set the animation back to idle
        yield return new WaitForSeconds(0.3f);

        anim.Play(IDLE);
    }

    public void SecondaryFire()
    {
        // TODO: Implement Secondary Fire
    }

    private void DealDamage(GameObject target, int damage, float bulletForce, Vector3 hitPoint)
    {
        var ei = target.GetComponentInParent<EnemyIdentifier>();
        if (ei != null) ei.GetHit(target, damage, bulletForce, hitPoint);
    }
}
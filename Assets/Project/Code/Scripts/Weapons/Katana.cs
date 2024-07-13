using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class Katana : MonoBehaviour
{
    [SerializeField] private int damage = 10;
    [SerializeField] private Animator anim;
    private CooldownManager cm;
    [SerializeField] private GameObject trail;

    private string IDLE = "Idle";
    private string SELECT = "Select";
    private string[] AttacksRL = {
        "Attack R-L 1",
    };
    private string[] AttacksLR = {
        "Attack L-R 1",
    };

    private bool attackRight = true;

    // HashSet<Transform> hitEnemies = new HashSet<Transform>();
    private Dictionary<Transform, int> hitEnemiesTimes = new Dictionary<Transform, int>();
    [SerializeField] private Collider attackCollider;
    private bool hitSomething;

    private void Start()
    {
        cm = CooldownManager.Instance;
        anim = GetComponent<Animator>();
        attackCollider.enabled = false;
    }

    public void PrimaryFire()
    {
        if (!cm.CheckCooldown("Katana"))  return;
        StopAllCoroutines();
        StartCoroutine(SlashAttack());
    }

    private void OnEnable()
    {
        anim.Play(SELECT);
        trail.SetActive(false);
        attackCollider.enabled = false;
    }

    private IEnumerator SlashAttack()
    {
        cm.AddCooldown("Katana", 0.2f);
        attackCollider.enabled = true;
        hitSomething = false;
        string currentAnimation = "";
        if (attackRight)
        {
            currentAnimation = AttacksRL[Random.Range(0, AttacksRL.Length)];
        }
        else
        {
            currentAnimation = AttacksLR[Random.Range(0, AttacksLR.Length)];
        }

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
        if (!cm.CheckCooldown("Katana")) return;
        // StopAllCoroutines();
        // StartCoroutine(Deflect());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("Limb")) return;
        Transform hitEnemy = other.transform;

        while (hitEnemy.parent != null && hitEnemy.parent.gameObject.name != "Gore" && hitEnemy.parent.gameObject.name != "Enemies")
        {
            hitEnemy = hitEnemy.parent;
        }

        if (hitEnemiesTimes.ContainsKey(hitEnemy))
        {
            hitEnemiesTimes[hitEnemy]++;
            if (hitEnemiesTimes[hitEnemy] > 3)
            {
                return;
            }
        }
        else
        {
            hitEnemiesTimes.Add(hitEnemy, 1);
        }

        if (!hitEnemy.GetComponent<Zombie>().dead) hitSomething = true;
        DealDamage(other.gameObject, damage, 300, other.transform.position);
    }

    private void DealDamage(GameObject target, int damage, float bulletForce, Vector3 hitPoint)
    {
        EnemyIdentifier ei = target.GetComponentInParent<EnemyIdentifier>();
        if (ei != null)
        {
            ei.GetHit(target, damage, bulletForce, hitPoint);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Damageable : MonoBehaviour
{
    [SerializeField] protected Rigidbody rb;
    [SerializeField] protected Collider col;
    [SerializeField] protected int health = 100;
    [SerializeField] protected AudioClip deathSound;
    [SerializeField] protected AudioClip damageSound;
    public bool dead;
    [SerializeField] protected GameObject bloodEffectPrefab;
    protected NavMeshAgent nma;


    private void Start()
    {
        if (nma == null)
        {
            nma = GetComponent<NavMeshAgent>();
        }
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }
        if (col == null)
        {
            col = GetComponent<Collider>();
        }
    }

    protected void InstantiateBloodEffect(Vector3 hitPoint, int amount = 10, bool isCritical = false)
    {
        GameObject blood = Instantiate(bloodEffectPrefab, hitPoint, Quaternion.identity);
        BloodSplatter bs = blood.GetComponent<BloodSplatter>();
        bs.Emit(amount, isCritical);
    }

    private IEnumerator PlayDeathAnimation()
    {
        // Rotate the object 360 degrees over 1 second and make it shrink
        Rigidbody rb = GetComponent<Rigidbody>();
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }
        if (rb != null)
        {
            rb.freezeRotation = false;
            rb.useGravity = false;
        }
        float elapsed = 0f;
        while (elapsed < 1f)
        {
            transform.Rotate(Vector3.forward, 720 * Time.deltaTime);
            transform.localScale -= Vector3.one * Time.deltaTime;
            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}

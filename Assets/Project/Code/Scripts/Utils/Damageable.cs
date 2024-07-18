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
        if (nma == null) nma = GetComponent<NavMeshAgent>();
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (col == null) col = GetComponent<Collider>();
    }

    protected void InstantiateBloodEffect(Vector3 hitPoint, int amount = 10, bool isCritical = false)
    {
        var blood = Instantiate(bloodEffectPrefab, hitPoint, Quaternion.identity);
        var bs = blood.GetComponent<BloodSplatter>();
        bs.Emit(amount, isCritical);
    }
}
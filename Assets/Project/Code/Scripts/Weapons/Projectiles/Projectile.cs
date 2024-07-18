using UnityEngine;

public class Projectile : MonoBehaviour
{
    public int damage;
    public float bulletForce;
    public float speed;
    public Vector3 direction;
    private bool isDeflected;


    private void Start()
    {
        transform.parent = MonoSingleton<GoreZone>.Instance.goreZone;
    }

    private void Update()
    {
        if (Physics.Raycast(transform.position, direction, out var hit, speed * Time.deltaTime))
            HandleCollision(hit.collider);

        Move();
    }

    private void Move()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    private void HandleCollision(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
            Destroy(gameObject);

        if (other.gameObject.CompareTag("Player") && !isDeflected)
        {
            if (WeaponController.Instance.isDeflecting) return;
            other.gameObject.GetComponent<PlayerController>().GetHit(damage);
            Destroy(gameObject);
        }

        else if (other.gameObject.CompareTag("Enemy") && isDeflected)
        {
            var hitPoint = other.ClosestPointOnBounds(transform.position);
            other.gameObject.GetComponent<EnemyIdentifier>().GetHit(gameObject, damage, 0, hitPoint);
            Destroy(gameObject);
        }
    }

    public void Deflect(Vector3 direction, int damageMultiplier = 2, float speedMultiplier = 1.5f)
    {
        isDeflected = true;
        this.direction = direction;
        damage *= damageMultiplier;
        speed *= speedMultiplier;
    }
}
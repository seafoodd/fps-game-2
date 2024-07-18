using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private bool isDeflected;
    public int damage;
    public float bulletForce;
    public float speed;
    public Vector3 direction;


    private void Start()
    {
        transform.parent = MonoSingleton<GoreZone>.Instance.goreZone;
    }

    private void Update()
    {
        if (Physics.Raycast(transform.position, direction, out RaycastHit hit, speed * Time.deltaTime))
        {
            // If the ray hits something, handle the collision
            HandleCollision(hit.collider);
        }
        Move();
    }

    private void Move()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    private void HandleCollision(Collider other)
    {
        if(other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            // Debug.Log("Hit ground");
            Destroy(gameObject);
        }

        if(other.gameObject.CompareTag("Player") && !isDeflected)
        {
            if (WeaponController.Instance.isDeflecting) return;
            other.gameObject.GetComponent<PlayerController>().GetHit(damage);
            Destroy(gameObject);
        }

        else if (other.gameObject.CompareTag("Enemy") && isDeflected)
        {
            Vector3 hitPoint = other.ClosestPointOnBounds(transform.position);
            other.gameObject.GetComponent<EnemyIdentifier>().GetHit(gameObject, damage, 0, hitPoint);
            Destroy(gameObject);
        }

    }

    public void Deflect(Vector3 direction, int damageMultiplier = 2, float speedMultiplier = 1.5f)
    {
        // GetComponent<TrailRenderer>().enabled = false;
        isDeflected = true;
        this.direction = direction;
        damage *= damageMultiplier;
        // transform.position = hitPoint;
        speed *= speedMultiplier;
        // GetComponent<TrailRenderer>().enabled = true;
    }
}

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


    private void Update()
    {
        Move();
    }

    private void Move()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {


        if(other.gameObject.CompareTag("Player"))
        {
            other.gameObject.GetComponent<PlayerController>().GetHit(10);
        }
        
        else if (other.gameObject.CompareTag("Enemy") && isDeflected)
        {
            Vector3 hitPoint = other.ClosestPointOnBounds(transform.position);
            other.gameObject.GetComponent<EnemyIdentifier>().GetHit(gameObject, 1, 0, hitPoint);
        }

        Destroy(gameObject);
    }
}

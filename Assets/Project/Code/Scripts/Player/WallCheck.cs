using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallCheck : MonoSingleton<WallCheck>
{
    public bool touchingWall;
    private PlayerController pc;
    [SerializeField] private List<Collider> cols = new List<Collider>();
    public RaycastHit hitInfo;
    private Vector3 pos1;
    private Vector3 pos2;


    void Start()
    {
        pc = MonoSingleton<PlayerController>.Instance;
    }

    private void OnDisable()
    {
        touchingWall = false;
        cols.Clear();
    }
    private void OnTriggerEnter(Collider other)
    {
        // check for wall
        if (ColliderIsWall(other) && !cols.Contains(other))
        {
            cols.Add(other);
            touchingWall = true;
            // Vector3 wallCenter = other.bounds.center;
            // hitInfo.normal = (other.ClosestPoint(transform.position) - wallCenter).normalized;

            RaycastHit hit;
            Vector3 directionToWall = other.ClosestPoint(transform.position) - transform.position;
            if (Physics.Raycast(transform.position, directionToWall, out hit, 10f, layerMask: 1 << LayerMask.NameToLayer("Ground")))
            {
                // Store the normal of the hit surface
                Debug.Log("hit " + hit.collider.name);
                hitInfo.normal = hit.normal;
            }

            pos1 = transform.position;
            pos2 = other.bounds.center;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // check for wall
        if (ColliderIsWall(other))
        {
            if(!cols.Contains(other)) cols.Add(other);
            touchingWall = true;
            // Vector3 wallCenter = other.bounds.center;
            // hitInfo.normal = (other.ClosestPoint(transform.position) - wallCenter).normalized;

            RaycastHit hit;
            Vector3 directionToWall = other.ClosestPoint(transform.position) - transform.position;
            if (Physics.Raycast(transform.position, directionToWall, out hit, 10f, layerMask: 1 << LayerMask.NameToLayer("Ground")))
            {
                // Store the normal of the hit surface
                Debug.Log("hit " + hit.collider.name);
                hitInfo.normal = hit.normal;
            }

            pos1 = transform.position;
            pos2 = other.bounds.center;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, Vector3.Cross(hitInfo.normal, Vector3.up) * 5f);

        // var direction = hitInfo.normal;
        // direction.y = 0;
        // direction.Normalize();
        //
        // Gizmos.DrawRay(transform.position, direction * 3f);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(pos1, pos2);
    }

    private void OnTriggerExit(Collider other)
    {
        // check for wall
        if (ColliderIsWall(other) && cols.Contains(other))
        {
            cols.Remove(other);
        }

        if (cols.Count == 0)
        {
            touchingWall = false;
        }
    }

    private bool ColliderIsWall(Collider col)
    {
        return col.gameObject.layer == LayerMask.NameToLayer("Ground");
    }
}

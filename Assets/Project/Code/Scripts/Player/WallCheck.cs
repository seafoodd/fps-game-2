using System.Collections.Generic;
using UnityEngine;

public class WallCheck : MonoSingleton<WallCheck>
{
    public bool touchingWall;
    [SerializeField] private List<Collider> cols = new();
    public RaycastHit hitInfo;

    private void OnDisable()
    {
        touchingWall = false;
        cols.Clear();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, Vector3.Cross(hitInfo.normal, Vector3.up) * 5f);
    }

    private void OnTriggerEnter(Collider other)
    {
        // check for wall
        if (ColliderIsWall(other) && !cols.Contains(other))
        {
            cols.Add(other);
            touchingWall = true;

            var directionToWall = other.bounds.center - transform.position;
            if (Physics.Raycast(transform.position, directionToWall, out var hit, 10f,
                    1 << LayerMask.NameToLayer("Ground")))
                // Store the normal of the hit surface
                // Debug.Log("hit " + hit.collider.name);
                hitInfo.normal = hit.normal;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // check for wall
        if (ColliderIsWall(other) && cols.Contains(other)) cols.Remove(other);

        if (cols.Count == 0) touchingWall = false;
    }

    private void OnTriggerStay(Collider other)
    {
        // check for wall
        if (ColliderIsWall(other))
        {
            if (!cols.Contains(other)) cols.Add(other);
            touchingWall = true;

            var directionToWall = other.bounds.center - transform.position;
            if (Physics.Raycast(transform.position, directionToWall, out var hit, 10f,
                    1 << LayerMask.NameToLayer("Ground")))
                // Store the normal of the hit surface
                // Debug.Log("hit " + hit.collider.name);
                hitInfo.normal = hit.normal;
        }
    }

    private bool ColliderIsWall(Collider col)
    {
        return col.gameObject.layer == LayerMask.NameToLayer("Ground");
    }

    public void ResetWallCheck()
    {
        touchingWall = false;
        cols.Clear();
    }
}
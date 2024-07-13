using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallCheck : MonoSingleton<WallCheck>
{
    public bool onWall;
    public bool touchingWall;
    public bool canWallJump;
    private PlayerController pc;
    [SerializeField] private List<Collider> cols = new List<Collider>();
    private Collider currentWallCol;
    public RaycastHit hitInfo;


    void Start()
    {
        pc = MonoSingleton<PlayerController>.Instance;
    }

    private void OnDisable()
    {
        touchingWall = false;
        cols.Clear();
        canWallJump = false;
    }
    private void OnTriggerEnter(Collider other)
    {
        // check for wall
        if (ColliderIsWall(other) && !cols.Contains(other))
        {
            cols.Add(other);
            touchingWall = true;
            hitInfo.normal = (other.ClosestPoint(transform.position) - other.transform.position).normalized;
        }
        // check for enemies
        else if (!other.gameObject.CompareTag("Slippery") && other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            currentWallCol = other;
            canWallJump = true;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // check for wall
        if (ColliderIsWall(other) && !cols.Contains(other))
        {
            cols.Add(other);
            touchingWall = true;
            hitInfo.normal = (other.ClosestPoint(transform.position) - other.transform.position).normalized;
        }
        // check for enemies
        else if (!other.gameObject.CompareTag("Slippery") && other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            currentWallCol = other;
            canWallJump = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // check for wall
        if (ColliderIsWall(other) && cols.Contains(other))
        {
            cols.Remove(other);
            touchingWall = false;
        }
        // check for enemies
        else if (!other.gameObject.CompareTag("Slippery") && other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            currentWallCol = null;
            canWallJump = false;
        }
    }

    private bool ColliderIsWall(Collider col)
    {
        return col.gameObject.layer == LayerMask.NameToLayer("Ground");
    }
}

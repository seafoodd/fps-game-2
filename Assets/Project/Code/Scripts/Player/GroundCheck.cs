using System.Collections.Generic;
using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    public bool onGround;
    public bool touchingGround;
    public bool canJump;
    public bool slopeCheck;
    [SerializeField] private List<Collider> cols = new();
    private Collider currentEnemyCol;

    private void OnDisable()
    {
        touchingGround = false;
        cols.Clear();
        canJump = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        // check for ground
        if (ColliderIsGround(other) && !cols.Contains(other))
        {
            cols.Add(other);
            touchingGround = true;
        }
        // check for enemies
        else if (!other.gameObject.CompareTag("Slippery") && other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            currentEnemyCol = other;
            canJump = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // check for ground
        if (ColliderIsGround(other) && cols.Contains(other))
        {
            if (cols.IndexOf(other) == cols.Count - 1)
            {
                cols.Remove(other);
                if (cols.Count > 0)
                    for (var i = cols.Count - 1; i >= 0; i--)
                        cols.RemoveAt(i);
            }
            else
            {
                cols.Remove(other);
            }

            if (cols.Count == 0) touchingGround = false;
        }
        else if (!other.gameObject.CompareTag("Slippery") && other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            currentEnemyCol = null;
            canJump = false;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // check for ground
        if (ColliderIsGround(other) && !cols.Contains(other))
        {
            cols.Add(other);
            touchingGround = true;
        }
        // check for enemies
        else if (!other.gameObject.CompareTag("Slippery") && other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            currentEnemyCol = other;
            canJump = true;
        }
    }

    private bool ColliderIsGround(Collider other)
    {
        return !other.isTrigger && !other.gameObject.CompareTag("Slippery") &&
               (other.gameObject.layer == LayerMask.NameToLayer("Ground") ||
                other.gameObject.layer == LayerMask.NameToLayer("Default"));
    }

    public void ResetGroundCheck()
    {
        touchingGround = false;
        cols.Clear();
        canJump = false;
    }
}
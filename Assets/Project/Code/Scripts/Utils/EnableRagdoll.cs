using UnityEngine;

public class EnableRagdoll : MonoBehaviour
{
    private Animator anim;
    private Collider[] cols;
    private Rigidbody[] rbs;

    private void Start()
    {
        rbs = GetComponentsInChildren<Rigidbody>();
        cols = GetComponentsInChildren<Collider>();
        anim = GetComponent<Animator>();
    }

    public void EnableRagdollComponents()
    {
        foreach (var rb in rbs)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        foreach (var col in cols)
            // make the collider ignore player collisions but still collide with the environment
            Physics.IgnoreCollision(col, PlayerController.Instance.GetComponent<Collider>(), true);
        // make the collider ignore enemy collisions but still collide with the environment
        anim.enabled = false;
    }
}
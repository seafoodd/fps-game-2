using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class EnableRagdoll : MonoBehaviour
{
    private Rigidbody[] rbs;
    private Collider[] cols;
    private Animator anim;

    private void Start()
    {
        rbs = GetComponentsInChildren<Rigidbody>();
        cols = GetComponentsInChildren<Collider>();
        anim = GetComponent<Animator>();
    }

    public void EnableRagdollComponents()
    {
        foreach (Rigidbody rb in rbs)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }
        foreach (Collider col in cols)
        {
            // make the collider ignore player collisions but still collide with the environment
            Physics.IgnoreCollision(col, PlayerController.Instance.GetComponent<Collider>(), true);
            // make the collider ignore enemy collisions but still collide with the environment
        }
        anim.enabled = false;
    }
}
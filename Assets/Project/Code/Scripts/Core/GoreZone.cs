using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoreZone : MonoSingleton<GoreZone>
{
    public void Combine()
    {
        StaticBatchingUtility.Combine(goreZone.gameObject);
    }

    public Transform goreZone;
    private MeshFilter tempFilter;

    public void ResetGore()
    {
        foreach (Transform child in goreZone)
        {
            Destroy(child.gameObject);
        }
    }
}

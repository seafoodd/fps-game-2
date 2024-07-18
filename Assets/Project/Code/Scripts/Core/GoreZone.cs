using UnityEngine;

public class GoreZone : MonoSingleton<GoreZone>
{
    public Transform goreZone;
    private MeshFilter tempFilter;

    public void Combine()
    {
        StaticBatchingUtility.Combine(goreZone.gameObject);
    }

    public void ResetGore()
    {
        foreach (Transform child in goreZone) Destroy(child.gameObject);
    }
}
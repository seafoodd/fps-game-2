using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyIdentifier : MonoBehaviour
{
    public EnemyType type;
    private Zombie zombie;

    private void Start()
    {
        if (type == EnemyType.Zombie)
        {
            zombie = GetComponent<Zombie>();
        }
    }

    public void GetHit(GameObject target, int damage, float bulletForce, Vector3 hitPoint)
    {
        if (type == EnemyType.Zombie)
        {
            zombie.GetHit(target, damage, bulletForce, hitPoint);
        }
    }
}

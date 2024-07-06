using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponIdentifier : MonoBehaviour
{
    public enum WeaponType
    {
        Pistol,
        Katana
    }

    public WeaponType weaponType;

    public void PrimaryFire()
    {
        switch (weaponType)
        {
            case WeaponType.Pistol:
                gameObject.GetComponent<Pistol>().PrimaryFire();
                break;
            case WeaponType.Katana:
                gameObject.GetComponent<Katana>().PrimaryFire();
                break;
        }
    }

    public void SecondaryFire()
    {
        switch (weaponType)
        {
            case WeaponType.Pistol:
                gameObject.GetComponent<Pistol>().SecondaryFire();
                break;
            case WeaponType.Katana:
                gameObject.GetComponent<Katana>().SecondaryFire();
                break;
        }
    }
}

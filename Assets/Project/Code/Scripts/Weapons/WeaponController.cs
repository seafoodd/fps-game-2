using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoSingleton<WeaponController>
{

    public GameObject[] weapons;
    [SerializeField] private int currentWeaponIndex = 0;

    private void Start()
    {
        // get all children of the weapon controller as weapons
        weapons = new GameObject[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            weapons[i] = transform.GetChild(i).gameObject;
            weapons[i].SetActive(false);
        }

        // set the first weapon active
        weapons[currentWeaponIndex].SetActive(true);
    }

    public void SwitchWeapon(int index)
    {
        weapons[currentWeaponIndex].SetActive(false);
        currentWeaponIndex = index;
        weapons[currentWeaponIndex].SetActive(true);
    }

    public void PrimaryFire()
    {
        weapons[currentWeaponIndex].GetComponent<WeaponIdentifier>().PrimaryFire();
    }

    public void SecondaryFire()
    {
        weapons[currentWeaponIndex].GetComponent<WeaponIdentifier>().SecondaryFire();
    }
}

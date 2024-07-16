using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoSingleton<WeaponController>
{

    public GameObject[] weapons;
    [SerializeField] private int currentWeaponIndex = 0;
    public bool isDeflecting;
    private PlayerController pc;
    [SerializeField] private bool fullDeflect;
    private int previousWeaponIndex;
    [SerializeField] private AudioClip deflectSound;
    [SerializeField] private AudioSource aud;
    private InputManager im;

    private void Start()
    {
       im = MonoSingleton<InputManager>.Instance;
       pc = MonoSingleton<PlayerController>.Instance;
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

    public void SwitchWeaponIndex(int index)
    {
        if (index > weapons.Length - 1 || index < 0) return;
        if (weapons[index].name == "Deflect") return;
        // if (isDeflecting) return;
        if (isDeflecting)
        {
            StopAllCoroutines();
            isDeflecting = false;
            fullDeflect = false;
        }

        weapons[currentWeaponIndex].SetActive(false);
        currentWeaponIndex = index;
        weapons[currentWeaponIndex].SetActive(true);
    }

    public void SwitchWeaponName(string weaponName)
    {
        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i].name == weaponName)
            {
                weapons[currentWeaponIndex].SetActive(false);
                currentWeaponIndex = i;
                weapons[currentWeaponIndex].SetActive(true);
                break;
            }
        }
    }

    public void PrimaryFire()
    {
        if (isDeflecting) return;
        // TODO: maybe make it switch to katana if trying to deflect
        weapons[currentWeaponIndex].GetComponent<WeaponIdentifier>().PrimaryFire();
    }

    public void SecondaryFire()
    {
        if (isDeflecting) return;
        // TODO: maybe make it switch to katana if trying to deflect
        weapons[currentWeaponIndex].GetComponent<WeaponIdentifier>().SecondaryFire();
    }

    // public void Deflect()
    // {
    //     if (im.deflectPressed)
    //     {
    //         StopAllCoroutines();
    //         if (!isDeflecting && CooldownManager.Instance.CheckCooldown("Deflect"))
    //         {
    //             StartCoroutine(DeflectCoroutine());
    //         }
    //         return;
    //     }
    //
    //     if (isDeflecting)
    //     {
    //         StopAllCoroutines();
    //         StartCoroutine(StopDeflectCoroutine());
    //     }
    // }

    public void Deflect()
    {
        isDeflecting = true;
        previousWeaponIndex = currentWeaponIndex;
        SwitchWeaponName("Deflect");
        weapons[currentWeaponIndex].GetComponent<Animator>().Play("Deflect");

        Invoke(nameof(StopDeflect), 3f);
    }

    public void StopDeflect()
    {
        isDeflecting = false;
        // fullDeflect = false;
        SwitchWeaponIndex(previousWeaponIndex);
        CooldownManager.Instance.AddCooldown("Deflect", 0.5f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isDeflecting)
        {
            if (other.gameObject.CompareTag("Projectile"))
            {
                // TimeManager.Instance.FreezeTime(0.05f);
                TimeManager.Instance.SlowMotion(0.05f, 0.05f);
                aud.pitch = UnityEngine.Random.Range(0.75f, 1f);
                aud.PlayOneShot(deflectSound);

                var direction = Camera.main.transform.forward;
                direction += new Vector3(UnityEngine.Random.Range(-0.02f, 0.02f), UnityEngine.Random.Range(-0.02f, 0.02f), UnityEngine.Random.Range(-0.02f, 0.02f));
                other.gameObject.GetComponent<Projectile>().Deflect(direction);
            }
        }
    }
}

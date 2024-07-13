using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using Random = UnityEngine.Random;

public class Pistol : MonoBehaviour
{
    [SerializeField] private Transform firePoint;
    [SerializeField] private float range = 100f;
    [SerializeField] private GameObject laserPrefab;
    [SerializeField] private float fireRate = 0.5f;
    [SerializeField] private AudioClip shootSound;
    [SerializeField] private AudioClip bounceSound;
    [SerializeField] private AudioSource aud;
    [SerializeField] private int primaryDamage = 25;
    [SerializeField] private int secondaryDamage = 40;
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private float aimAssist = 0.2f;
    [SerializeField] private bool enableRicochet;
    [SerializeField] private float laserWidth;
    private CooldownManager cm;

    private void Start()
    {
        cm = CooldownManager.Instance;
        firePoint = transform.Find("FirePoint");
        aud = GetComponent<AudioSource>();
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        transform.localRotation = Quaternion.Euler(Vector3.zero);
    }

    // TODO: Add a reload animation
    private IEnumerator PlayReloadAnimation(float recoilDuration = 0.1f, float returnDuration = 0.4f, float angle = 45f)
    {
        // Increase the rotation amount for a more noticeable change
        Vector3 rotationAmount = new Vector3(-angle, 0, 0);

        // Play the recoil animation
        float elapsed = 0f;
        while (elapsed < recoilDuration)
        {
            Vector3 step = (rotationAmount / recoilDuration) * Time.deltaTime;
            transform.Rotate(step);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Play the return animation
        elapsed = 0f;
        while (elapsed < returnDuration)
        {
            Vector3 step = (-rotationAmount / returnDuration) * Time.deltaTime;
            transform.Rotate(step);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure the gun is returned to its original position
        transform.localRotation = Quaternion.Euler(Vector3.zero);
    }

    public void PrimaryFire()
    {
        if (!cm.CheckCooldown("Pistol")) return;

        aud.pitch = Random.Range(1f, 1.1f);
        aud.PlayOneShot(shootSound);
        RaycastHit hit;
        Vector3 endPos = firePoint.position + firePoint.forward * range;

        if (Physics.Raycast(Camera.main.transform.position, firePoint.forward, out hit, range, LayerMask.GetMask("Limb")))
        {
            DealDamage(hit.transform.gameObject, primaryDamage, 300, hit.point);
            endPos = hit.point;
        }

        else if (Physics.BoxCast(Camera.main.transform.position, Vector3.one * aimAssist, firePoint.forward, out hit, firePoint.rotation, range, LayerMask.GetMask("Limb")))
        {
            // choose the best limb to hit (closest to the cursor(dot product of the forward vector and the vector from the camera to the hit point))
            RaycastHit[] hits = Physics.BoxCastAll(Camera.main.transform.position, Vector3.one * aimAssist, firePoint.forward, firePoint.rotation, range, LayerMask.GetMask("Limb"));

            var flag = false;
            foreach (RaycastHit h in hits)
            {
                if (h.transform.gameObject.CompareTag("Head"))
                {
                    hit = h;
                    flag = true;
                    break;
                }
            }

            if (!flag)
            {
                // TODO: make this work
                float closestDistance = float.MaxValue;
                Vector3 cursorPositionInWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                foreach (RaycastHit h in hits)
                {
                    float distance = Vector3.Distance(h.point, cursorPositionInWorld);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        hit = h;
                    }
                }
            }

            DealDamage(hit.transform.gameObject, primaryDamage, 300, hit.point);
            endPos = hit.point;
        }

        else if (Physics.Raycast(Camera.main.transform.position, firePoint.forward, out hit, range, layerMask))
        {

            endPos = hit.point;
        }

        InstantiateLaser(firePoint.position, endPos, Color.cyan, Color.white);

        StartCoroutine(PlayReloadAnimation());
        cm.AddCooldown("Pistol", fireRate);
    }

    public void SecondaryFire()
    {
        if (!cm.CheckCooldown("Pistol")) return;

        if (enableRicochet)
        {
            StartCoroutine(RicochetShot());
        }
        else
        {
            StartCoroutine(LaserShot());
        }

        StartCoroutine(PlayReloadAnimation(0.15f, 0.8f, 100f));
        cm.AddCooldown("Pistol", fireRate * 2);
    }

    private IEnumerator LaserShot()
    {
        aud.pitch = Random.Range(.8f, .9f);
        aud.PlayOneShot(shootSound);
        RaycastHit hit;
        Vector3 startPos = firePoint.position;
        Vector3 endPos = firePoint.position + firePoint.forward * range;

        if (Physics.Raycast(Camera.main.transform.position, firePoint.forward, out hit, range, layerMask))
        {
            endPos = hit.point;
        }

        InstantiateLaser(startPos, endPos, Color.white, new Color(173,0,153,1), 0.45f);

        if (Physics.BoxCast(Camera.main.transform.position, Vector3.one * laserWidth , firePoint.forward, out hit, firePoint.rotation, range, LayerMask.GetMask("Limb")))
        {
            HashSet<Transform> hitEnemies = new HashSet<Transform>();
            RaycastHit[] hits = Physics.BoxCastAll(Camera.main.transform.position, Vector3.one * laserWidth, firePoint.forward, firePoint.rotation, range, LayerMask.GetMask("Limb"));

            // sort the hits by distance from the camera
            System.Array.Sort(hits, (x, y) => Vector3.Distance(x.point, Camera.main.transform.position).CompareTo(Vector3.Distance(y.point, Camera.main.transform.position)));

            foreach (RaycastHit h in hits)
            {
                Transform hitEnemy = h.transform;

                while (hitEnemy.parent != null && hitEnemy.parent.gameObject.name != "Gore" && hitEnemy.parent.gameObject.name != "Enemies")
                {
                    hitEnemy = hitEnemy.parent;
                }

                if (hitEnemies.Count > 4)
                {
                    break;
                }

                if (hitEnemies.Contains(hitEnemy))
                {
                    continue;
                }

                hitEnemies.Add(hitEnemy);


                if (hitEnemy.GetComponent<Zombie>().dead)
                {
                    DealDamage(h.transform.gameObject, secondaryDamage, 1000, h.point);
                    continue;
                }

                DealDamage(h.transform.gameObject, secondaryDamage, 1000, h.point);
                Debug.Log("HIT" + hitEnemies.Count);
                TimeManager.Instance.FreezeTime(0.09f);
                yield return new WaitForSecondsRealtime(0.1f);

            }
        }
        yield return null;
    }


    // TODO: Fix it and make it work similar to the laser shot
    private IEnumerator RicochetShot()
    {
        aud.pitch = Random.Range(.8f, .9f);
        aud.PlayOneShot(shootSound);
        RaycastHit hit;
        Vector3 startPos = firePoint.position;
        Vector3 endPos = firePoint.position + firePoint.forward * range;
        Vector3 newDir = firePoint.forward;
        if (Physics.Raycast(Camera.main.transform.position, newDir, out hit, range, layerMask))
        {
            if(hit.transform.gameObject.layer == LayerMask.NameToLayer("Limb"))
            {
                DealDamage(hit.transform.gameObject, secondaryDamage, 1000, hit.point);
            }
            else
            {
                newDir = Vector3.Reflect(newDir, hit.normal);
            }
            endPos = hit.point;
        }
        InstantiateLaser(startPos, endPos, Color.white, Color.red, 0.1f, 0.6f);
        int a = 0;
        while (a < 4)
        {
            startPos = endPos;
            RaycastHit hit2;
            endPos = startPos + newDir * range; // Use newDir instead of firePoint.forward
            if (Physics.Raycast(startPos, newDir, out hit2, range)) // Use startPos instead of endPos
            {
                if(hit2.transform.gameObject.layer == LayerMask.NameToLayer("Limb"))
                {
                    DealDamage(hit2.transform.gameObject, secondaryDamage, 1000, hit2.point);
                }
                else
                {
                    newDir = Vector3.Reflect(newDir, hit2.normal); // Use hit2.normal instead of hit.normal
                    InstantiateImpactEffect(hit2);
                    a++;
                }
                endPos = hit2.point;

            }
            InstantiateLaser(startPos, endPos, Color.white, Color.red, 0.1f, 0.6f);
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void InstantiateImpactEffect(RaycastHit hit2)
    {
        // TODO: instantiate impact effect (for a wall or something)


        // play impact sound
        AudioSource bounceSoundSource = new GameObject("BounceSound").AddComponent<AudioSource>();
        bounceSoundSource.outputAudioMixerGroup = aud.outputAudioMixerGroup;
        bounceSoundSource.volume = 0.15f;
        bounceSoundSource.pitch = Random.Range(0.9f, 1);
        bounceSoundSource.spatialBlend = 0f;
        bounceSoundSource.transform.position = hit2.point;
        bounceSoundSource.clip = bounceSound;
        bounceSoundSource.Play();

        Destroy(bounceSoundSource.gameObject, bounceSound.length);
    }


    private void DealDamage(GameObject target, int damage, float bulletForce, Vector3 hitPoint)
    {
        EnemyIdentifier ei = target.GetComponentInParent<EnemyIdentifier>();
        if (ei != null)
        {
            ei.GetHit(target, damage, bulletForce, hitPoint);
        }
    }

    private void InstantiateLaser(Vector3 pos1, Vector3 pos2, Color startColor, Color endColor, float width = .015f, float duration = .35f)
    {
        GameObject laser = Instantiate(laserPrefab, pos1, Quaternion.identity);
        LineRenderer lineRenderer = laser.GetComponent<LineRenderer>();
        lineRenderer.startColor = startColor;
        lineRenderer.endColor = endColor;
        lineRenderer.SetPosition(0, pos1);
        lineRenderer.SetPosition(1, pos2);
        lineRenderer.widthMultiplier = width;
    }
}

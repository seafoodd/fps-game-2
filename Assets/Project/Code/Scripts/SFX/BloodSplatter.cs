using System.Collections.Generic;
using UnityEngine;

public class BloodSplatter : MonoBehaviour
{
    [SerializeField] private ParticleSystem ps;
    [SerializeField] private AudioSource aud;
    [SerializeField] private GameObject surfaceBloodPrefab;
    [SerializeField] private Material[] materials;
    [SerializeField] private int bloodAmount = 50;
    [SerializeField] private AudioClip[] splatterSounds;
    [SerializeField] private AudioClip[] CrticalSplatterSounds;
    private List<ParticleCollisionEvent> collisionEvents;

    private void Start()
    {
        transform.parent = MonoSingleton<GoreZone>.Instance.goreZone;
        ps = GetComponent<ParticleSystem>();
        aud = GetComponent<AudioSource>();
        collisionEvents = new List<ParticleCollisionEvent>();
    }

    private void OnParticleCollision(GameObject other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            // for some reason, this unused variable is necessary for it to work
            // ReSharper disable once UnusedVariable
            var numCollisionEvents = ps.GetCollisionEvents(other, collisionEvents);
            var surfaceBlood = Instantiate(surfaceBloodPrefab, collisionEvents[0].intersection, Quaternion.identity);
            surfaceBlood.transform.forward = -collisionEvents[0].normal;
            var mr = surfaceBlood.GetComponent<MeshRenderer>();
            mr.material = materials[Random.Range(0, materials.Length - 1)];
            surfaceBlood.transform.Rotate(Vector3.forward, Random.Range(0, 359), Space.Self);
            surfaceBlood.transform.parent = MonoSingleton<GoreZone>.Instance.goreZone;
        }
    }

    public void Emit(int amount, bool isCritical = false)
    {
        aud.pitch = Random.Range(0.9f, 1.1f);
        aud.volume = bloodAmount / 500f;
        var sound = splatterSounds[Random.Range(0, splatterSounds.Length - 1)];
        if (isCritical)
        {
            amount *= 2;
            aud.volume *= 2;
            sound = CrticalSplatterSounds[Random.Range(0, CrticalSplatterSounds.Length - 1)];
        }

        ps.Emit(amount);
        aud.PlayOneShot(sound);
    }
}
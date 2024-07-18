using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoSingleton<AudioManager>
{
    [SerializeField] private AudioMixer mixer;
    private AudioMixerGroup effectsMixer;
    private AudioMixerGroup musicMixer;

    private void Start()
    {
        effectsMixer = mixer.FindMatchingGroups("Effects")[0];
        musicMixer = mixer.FindMatchingGroups("Music")[0];
    }

    public void ChangePitch(float pitch)
    {
        mixer.SetFloat("Pitch", pitch);
    }

    public void ChangePitchForSeconds(float pitch, float seconds)
    {
        StartCoroutine(ChangePitch(pitch, seconds));
    }

    private IEnumerator ChangePitch(float pitch, float seconds)
    {
        mixer.SetFloat("Pitch", pitch);
        yield return new WaitForSecondsRealtime(seconds);
        mixer.SetFloat("Pitch", 1);
    }
}
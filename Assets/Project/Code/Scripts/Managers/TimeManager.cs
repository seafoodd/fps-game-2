using System.Collections;
using UnityEngine;

public class TimeManager : MonoSingleton<TimeManager>
{
    public void SetTimeScale(float timeScale)
    {
        Time.timeScale = timeScale;
    }

    public void ResetTimeScale()
    {
        Time.timeScale = 1;
    }

    public void PauseGame()
    {
        Time.timeScale = 0;
    }

    public void ResumeGame()
    {
        Time.timeScale = 1;
    }

    public void SlowMotion(float timeScale, float duration, bool changePitch = false, float pitch = 0.5f)
    {
        // StopAllCoroutines();
        StartCoroutine(SlowMotionCoroutine(timeScale, duration, changePitch, pitch));
    }

    private IEnumerator SlowMotionCoroutine(float timeScale, float duration, bool changePitch = false,
        float pitch = 0.5f)
    {
        Time.timeScale = timeScale;
        if (changePitch) MonoSingleton<AudioManager>.Instance.ChangePitchForSeconds(pitch, duration);
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1;
    }

    public void FreezeTime(float duration)
    {
        StartCoroutine(FreezeTimeCoroutine(duration));
    }

    private IEnumerator FreezeTimeCoroutine(float duration)
    {
        Time.timeScale = 0;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1;
    }
}
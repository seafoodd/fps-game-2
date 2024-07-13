using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour
{

    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private float duration = 0.35f;
    private float initialWidth;

    private void Start()
    {
        transform.parent = MonoSingleton<GoreZone>.Instance.goreZone;
        initialWidth = lineRenderer.widthMultiplier;
        StartCoroutine(Shrink(lineRenderer, initialWidth, duration));
    }

    private IEnumerator Shrink(LineRenderer lineRenderer, float initialWidth, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float width = Mathf.Lerp(initialWidth, 0, elapsed / duration);
            lineRenderer.widthMultiplier = width;
            elapsed += Time.deltaTime;
            yield return null;
        }

        lineRenderer.widthMultiplier = 0;
        Destroy(gameObject);
    }
}

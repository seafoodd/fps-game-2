using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoSingleton<UIController>
{
    public TextMeshProUGUI healthText;
    public Image deathScreen;
    public TextMeshProUGUI deathText;
    public Image[] dashCharges;
    [SerializeField] private Color dashChargeColor;

    public void UpdateHealth(int health)
    {
        healthText.text = "Health: " + health;
    }

    public void UpdateDashCharges(float charges)
    {
        for (int i = 0; i < dashCharges.Length; i++)
        {
            if (i+1 <= charges)
            {
                if (dashCharges[i].enabled) continue;
                dashCharges[i].color = Color.clear;
                dashCharges[i].enabled = true;
                StartCoroutine(FadeIn(dashCharges[i], 0.2f));
            }
            else
            {
                dashCharges[i].enabled = false;
            }
        }
    }

    private IEnumerator FadeIn(Image image, float duration)
    {
        float elapsedTime = 0.0f; // time elapsed since the start of the fade

        while (elapsedTime < duration)
        {
            // calculate the fraction of the total duration that has passed
            float t = elapsedTime / duration;

            // image.color = Color.Lerp(Color.clear, dashChargeColor, t);
            // lerp the alpha value of the color from 0 to 1
            image.color = new Color(dashChargeColor.r, dashChargeColor.g, dashChargeColor.b, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        image.color =dashChargeColor; // ensure the color is set to black
    }

    public IEnumerator ShowDeathScreen()
    {
        // slowly fade from red transparent to black with text
        deathScreen.gameObject.SetActive(true);
        deathText.gameObject.SetActive(true);

        var startColor = new Color(0, 0, 0, 0.3f);
        float duration = 15.0f; // duration of the fade
        float elapsedTime = 0.0f; // time elapsed since the start of the fade

        while (elapsedTime < duration)
        {
            // calculate the fraction of the total duration that has passed
            float t = elapsedTime / duration;

            deathScreen.color = Color.Lerp(startColor, Color.black, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // deathScreen.color = Color.black; // ensure the color is set to black

    }
    public void HideDeathScreen()
    {
        StopAllCoroutines();
        deathScreen.gameObject.SetActive(false);
        deathText.gameObject.SetActive(false);
    }
}

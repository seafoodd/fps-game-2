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

    public void UpdateHealth(int health)
    {
        healthText.text = "Health: " + health;
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

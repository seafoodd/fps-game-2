using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CooldownManager : MonoSingleton<CooldownManager>
{
    private Dictionary<string, float> cooldowns = new Dictionary<string, float>();
    public float dashCharges = 4;

    public void AddCooldown(string key, float value)
    {
        if (cooldowns.ContainsKey(key))
        {
            cooldowns[key] = value;
        }
        else
        {
            cooldowns.Add(key, value);
        }
    }

    public bool CheckCooldown(string key)
    {
        if (cooldowns.ContainsKey(key))
        {
            if (cooldowns[key] <= 0)
            {
                return true;
            }

            return false;
        }

        return true;
    }

    private void Update()
    {
        List <string> keys = new List<string>(cooldowns.Keys);

        foreach (var key in keys)
        {
            if (cooldowns[key] > 0)
            {
                cooldowns[key] -= Time.deltaTime;
            }
        }

        if (dashCharges < 4)
        {
            dashCharges += Time.deltaTime * 0.75f;
            UIController.Instance.UpdateDashCharges(dashCharges);
        }
    }

    public void ResetAllCharges()
    {
        dashCharges = 4;
        UIController.Instance.UpdateDashCharges(dashCharges);
    }

    public void ResetCooldown(string key)
    {
        // if (cooldowns.ContainsKey(key))
        // {
        //     cooldowns[key] = 0;
        // }
        if (cooldowns.ContainsKey(key))
        {
            cooldowns.Remove(key);
        }
    }

    public void ResetAllCooldowns()
    {
        // foreach (var key in cooldowns.Keys)
        // {
        //     cooldowns[key] = 0;
        // }
        cooldowns.Clear();
    }
}

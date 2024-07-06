using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CooldownManager : MonoSingleton<CooldownManager>
{
    private Dictionary<string, float> cooldowns = new Dictionary<string, float>();

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
    }

    public void ResetCooldown(string key)
    {
        if (cooldowns.ContainsKey(key))
        {
            cooldowns[key] = 0;
        }
    }

    public void ResetAllCooldowns()
    {
        foreach (var key in cooldowns.Keys)
        {
            cooldowns[key] = 0;
        }
    }
}

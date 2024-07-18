using System.Collections.Generic;
using UnityEngine;

public class CooldownManager : MonoSingleton<CooldownManager>
{
    public float dashCharges = 4;
    private readonly Dictionary<string, float> cooldowns = new();

    private void Update()
    {
        var keys = new List<string>(cooldowns.Keys);

        foreach (var key in keys)
            if (cooldowns[key] > 0)
                cooldowns[key] -= Time.deltaTime;
            else
                cooldowns.Remove(key);

        if (dashCharges < 4)
        {
            dashCharges += Time.deltaTime * 0.75f;
            UIController.Instance.UpdateDashCharges(dashCharges);
        }
    }

    public void AddCooldown(string key, float value)
    {
        cooldowns[key] = value;
    }

    public bool CheckCooldown(string key)
    {
        return !cooldowns.ContainsKey(key);
    }

    public void ResetAllCharges()
    {
        dashCharges = 4;
        UIController.Instance.UpdateDashCharges(dashCharges);
    }

    public void ResetCooldown(string key)
    {
        if (cooldowns.ContainsKey(key)) cooldowns.Remove(key);
    }

    public void ResetAllCooldowns()
    {
        cooldowns.Clear();
    }
}
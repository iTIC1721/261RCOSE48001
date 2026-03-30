using System.Collections.Generic;
using UnityEngine;

public class BTBlackboard
{
    private Dictionary<string, object> data = new Dictionary<string, object>();

    public void SetValue(string key, object value)
    {
        if (data.ContainsKey(key)) data[key] = value;
        else data[key] = value;
    }

    public T GetValue<T>(string key)
    {
        if (data.TryGetValue(key, out var value)) return (T)value;

        Log.LogWarning($"No value of key \"{key}\"");
        return default(T);
    }
}

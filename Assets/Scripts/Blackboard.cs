using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public static class Blackboard
{
    private static Dictionary<string, object> _blackboard = new Dictionary<string, object>();
    
    public static void AddObject<T>(string key, T obj)
    {
        // Sanity Check.
        if(_blackboard.ContainsKey(key))
        {
            Debug.LogWarning("Adding to blackboard with existing key " + key + "! Discarding old object.");
            _blackboard.Remove(key);
        }

        // Set the object
        _blackboard[key] = obj;
    }

    public static T GetObject<T>(string key)
    {
        // Sanity Check. Return something if there's nothing there.
        if (!_blackboard.ContainsKey(key))
        {
            Debug.LogError("Attempting to retrieve nonexistent key "+key+" from blackboard!!!");
            return (T)(new object());
        }

        // Return the object
        return (T)_blackboard[key];
    }
}

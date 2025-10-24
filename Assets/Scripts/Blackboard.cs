using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public static class Blackboard
{
    private static Dictionary<string, string> _blackboard = new Dictionary<string, string>();

    public static void AddObject(string key, string obj)
    {
        // Sanity Check.
        if (_blackboard.ContainsKey(key))
        {
            Debug.LogWarning("Adding to blackboard with existing key " + key + "! Discarding old object.");
            _blackboard.Remove(key);
        }

        // Set the object
        _blackboard[key] = obj;
    }

    public static string GetObject(string key)
    {
        // Sanity Check. Return something if there's nothing there.
        if (!_blackboard.ContainsKey(key))
        {
            Debug.LogError("Attempting to retrieve nonexistent key " + key + " from blackboard!!!");
            return "null";
        }

        // Return the object
        return _blackboard[key];
    }
    public static bool HasObject(string key)
    {
        return _blackboard.ContainsKey(key);
    }

    public static void Wipe()
    {
        _blackboard.Clear();
        _blackboard = new Dictionary<string, string>();
    }
}

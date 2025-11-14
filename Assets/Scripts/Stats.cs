using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;
[System.Serializable]
public class CharacterStats
{
    public int strength;
    public int dexterity;
    public int constitution;
    public int charisma;
    public int magic;
    public int karma;

    public static bool RandomChanceByDifference(CharacterStats _this, CharacterStats other)
    {
        // we tried
        return true;
    }
    // Return true if every number in this is more than every number in other.
    public static bool operator >(CharacterStats _this, CharacterStats other)
    {
        FieldInfo[] fields = _this.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (FieldInfo field in fields)
        {
            if ((int)field.GetValue(_this) < (int)field.GetValue(other))
                return false;
        }
        return RandomChanceByDifference(_this, other);
    }
    public static bool IsEqual(CharacterStats a, CharacterStats b)
    {
        FieldInfo[] fields = a.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (FieldInfo field in fields)
        {
            if ((int)field.GetValue(a) != (int)field.GetValue(b))
                return false;
        }
        return true;
    }
    public static bool operator <(CharacterStats _this, CharacterStats other)
    {
        FieldInfo[] fields = _this.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (FieldInfo field in fields)
        {
            if ((int)field.GetValue(_this) > (int)field.GetValue(other))
                return false;
        }
        return RandomChanceByDifference(_this, other);
    }
    public void Set( CharacterStats other)
    {
        FieldInfo[] fields = this.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (FieldInfo field in fields)
        {
            field.SetValue(this, field.GetValue(other));
        }
    }
    public static CharacterStats operator -(CharacterStats _this, CharacterStats other)
    {
        FieldInfo[] fields = _this.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (FieldInfo field in fields)
        {
            int total = (int)field.GetValue(_this) - (int)field.GetValue(other);
            field.SetValue(_this, total);
        }

        return _this;
    }
    public static CharacterStats operator +(CharacterStats _this, CharacterStats other)
    {
        FieldInfo[] fields = _this.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (FieldInfo field in fields)
        {
            int total = (int)field.GetValue(_this) + (int)field.GetValue(other);
            field.SetValue(_this, total);
        }

        return _this;
    }

    public bool IsZero()
    {
        FieldInfo[] fields = this.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (FieldInfo field in fields)
        {
            if ((int)field.GetValue(this) != 0)
                return false;
        }
        return true;
    }
}
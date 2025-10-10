using System.Reflection;
public struct CharacterStats
{
    int health;
    int defence;
    int strength;
    int intelligence;
    int agility;
    int charisma;
    // Return true if every number in this is more than every number in other.
    public static bool operator>(CharacterStats _this, CharacterStats other)
    {
        FieldInfo[] fields = _this.GetType().GetFields();
        foreach(FieldInfo field in fields)
        {
            if ((int)field.GetValue(_this) < (int)field.GetValue(other))
                return false;
        }
        return true;
    }
    public static bool operator <(CharacterStats _this, CharacterStats other)
    {
        FieldInfo[] fields = _this.GetType().GetFields();
        foreach(FieldInfo field in fields)
        {
            if ((int)field.GetValue(_this) > (int)field.GetValue(other))
                return false;
        }
        return true;
    }
}
using System.Reflection;
public struct CharacterStats
{
    int strength;
    int dexterity;
    int constitution;
    int charisma;
    int magic;
    int karma;
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
    public static CharacterStats operator -(CharacterStats _this, CharacterStats other)
    {
        CharacterStats curr = new CharacterStats();
		FieldInfo[] fields = _this.GetType().GetFields();
        foreach(FieldInfo field in fields)
        {
            field.SetValue(curr, (int)field.GetValue(_this) - (int)field.GetValue(other));
		}
        return curr;
    }
    public static CharacterStats operator +(CharacterStats _this, CharacterStats other)
    {
        CharacterStats curr = new CharacterStats();
		FieldInfo[] fields = _this.GetType().GetFields();
        foreach(FieldInfo field in fields)
        {
            field.SetValue(curr, (int)field.GetValue(_this) + (int)field.GetValue(other));
		}
        return curr;
    }
}
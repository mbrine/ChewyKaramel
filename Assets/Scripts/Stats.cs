using System.Reflection;
[System.Serializable]
public struct CharacterStats
{
    public int strength;
    public int dexterity;
    public int constitution;
    public int charisma;
    public int magic;
    public int karma;
    // Return true if every number in this is more than every number in other.
    public static bool operator>(CharacterStats _this, CharacterStats other)
    {
        FieldInfo[] fields = _this.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach(FieldInfo field in fields)
        {
            if ((int)field.GetValue(_this) < (int)field.GetValue(other))
                return false;
        }
        return true;
    }
    public static bool operator <(CharacterStats _this, CharacterStats other)
    {
        FieldInfo[] fields = _this.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach(FieldInfo field in fields)
        {
            if ((int)field.GetValue(_this) > (int)field.GetValue(other))
                return false;
        }
        return true;
    }
    public static CharacterStats operator -(CharacterStats _this, CharacterStats other)
    {
		FieldInfo[] fields = _this.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
		object boxed = new CharacterStats();
		foreach (FieldInfo field in fields)
		{
			int total = (int)field.GetValue(_this) - (int)field.GetValue(other);
			field.SetValue(boxed, total);
		}
		_this = (CharacterStats)boxed;

		return _this;
	}
	public static CharacterStats operator +(CharacterStats _this, CharacterStats other)
    {
		FieldInfo[] fields = _this.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        object boxed = new CharacterStats();
        foreach(FieldInfo field in fields)
        {
            int total = (int)field.GetValue(_this) + (int)field.GetValue(other);
            field.SetValue(boxed, total);
		}
        _this = (CharacterStats)boxed;

        return _this;
    }
}
public static class ColorCodes
{
    public static string goldHighlight = "#FFF0AA";
    public static string statLost = "#FFAAAA";
    public static string statGained = "#AAFFAA";

    public static string Apply(string text,string color)
    {
        return $"<color={color}>{text}</color>";
    }
}
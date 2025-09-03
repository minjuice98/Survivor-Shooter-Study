using UnityEngine;

public enum Languages
{
    Korean,
    English,
    Japanese,
}

public static class DataTableIds
{
    public static readonly string[] StringTableIds =
    {
        "StringTableKr",
        "StringTableEn",
        "StringTableJp",
    };

    public static string String => StringTableIds[(int)Variables.language];
}

public static class Variables //전역변수용 static class, 현재 언어 설정
{
    public static Languages language = Languages.Korean;
}


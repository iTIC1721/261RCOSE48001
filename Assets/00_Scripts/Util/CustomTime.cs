using System;
using UnityEngine;

public static class CustomTime
{
    public static bool useCustomTime = false;
    public static DateTime customToday = DateTime.Now;

    public static DateTime GetTimeNow()
    {
        if (useCustomTime)
        {
            return customToday;
        }
        else
        {
            return DateTime.Now;
        }
    }
}

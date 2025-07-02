using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SkillRangeHelper
{
    /// <summary>
    /// 직선
    /// </summary>
    /// <param name="dir"></param>
    /// <param name="length"></param>
    /// <param name="color"></param>
    /// <returns></returns>
    public static List<(int dq, int dr, Color color)> GetLine((int dq, int dr) dir, int length, Color color)
    {
        var result = new List<(int, int, Color)>();
        int q = 0, r = 0;
        for (int i = 1; i <= length; i++)
        {
            q += dir.dq;
            r += dir.dr;
            result.Add((q, r, color));
        }
        return result;
    }

    /// <summary>
    /// 원형
    /// </summary>
    /// <param name="radius"></param>
    /// <param name="color"></param>
    /// <returns></returns>
    public static List<(int dq, int dr, Color color)> GetRing(int radius, Color color)
    {
        var result = new List<(int, int, Color)>();
        for (int dq = -radius; dq <= radius; dq++)
        {
            for (int dr = Mathf.Max(-radius, -dq - radius); dr <= Mathf.Min(radius, -dq + radius); dr++)
            {
                int ds = -dq - dr;
                if (Mathf.Abs(dq) + Mathf.Abs(dr) + Mathf.Abs(ds) == radius * 2)
                    result.Add((dq, dr, color));
            }
        }
        return result;
    }
}
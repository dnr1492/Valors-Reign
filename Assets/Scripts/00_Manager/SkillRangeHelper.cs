using System.Collections.Generic;
using UnityEngine;
using static EnumClass;

public static class SkillRangeHelper
{
    //표준 axial 6방향 (시계 방향)
    private static readonly (int dq, int dr)[] AX_DIRS = new (int, int)[]
    {
        (+1, 0),  //0
        (+1,-1),  //1
        ( 0,-1),  //2 (Up/Forward)
        (-1, 0),  //3
        (-1, 1),  //4
        ( 0, 1),  //5
    };

    //rangeType에 따른 패턴을 매핑 (직진 디폴트는 AX_DIRS[2] = (0,-1))
    public static List<(int dq, int dr, Color color)> GetPattern(
        SkillRangeType type, int param,
        Color colLine, Color colPlus, Color colBox, Color colRing)
    {
        switch (type)
        {
            case SkillRangeType.LineForward1: return GetLine(AX_DIRS[2], 1, colLine);
            case SkillRangeType.LineForward2: return GetLine(AX_DIRS[2], 2, colLine);
            case SkillRangeType.LineForward3: return GetLine(AX_DIRS[2], 3, colLine);
            case SkillRangeType.LineForward4: return GetLine(AX_DIRS[2], 4, colLine);

            case SkillRangeType.Ring1: return GetRing(1, colRing);
            case SkillRangeType.Ring2: return GetRing(2, colRing);
            case SkillRangeType.Ring3: return GetRing(3, colRing);

            case SkillRangeType.Cone2: return GetCone(2, colLine);
            case SkillRangeType.Cone3: return GetCone(3, colLine);

            case SkillRangeType.Plus1: return GetPlus1(colPlus);
            case SkillRangeType.Box3x3: return GetBox3x3(colBox);

            case SkillRangeType.Custom:  // ===== TODO: 추후 커스텀 호출 방식 수정하기 ===== //
            case SkillRangeType.None:
            default:
                if (param > 0) return GetLine(AX_DIRS[2], param, colLine);
                return new List<(int, int, Color)>();
        }
    }

    //커스텀 오프셋
    public static List<(int dq, int dr, Color color)> GetCustom(List<(int dq, int dr)> offsets, Color color)
    {
        var result = new List<(int, int, Color)>(offsets?.Count ?? 0);
        if (offsets != null)
            foreach (var (dq, dr) in offsets)
                result.Add((dq, dr, color));
        return result;
    }

    //직선
    private static List<(int dq, int dr, Color color)> GetLine((int dq, int dr) dir, int length, Color color)
    {
        var result = new List<(int, int, Color)>(length);
        int q = 0, r = 0;
        for (int i = 0; i < length; i++)
        {
            q += dir.dq;
            r += dir.dr;
            result.Add((q, r, color));
        }
        return result;
    }

    //원형
    private static List<(int dq, int dr, Color color)> GetRing(int radius, Color color)
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

    //부채꼴(Cone)
    private static List<(int dq, int dr, Color color)> GetCone(int depth, Color color)
    {
        var F = AX_DIRS[2];  //Up
        var L = AX_DIRS[5];  //Up-Left
        var R = AX_DIRS[1];  //Up-Right

        var result = new List<(int, int, Color)>();
        for (int d = 1; d <= depth; d++)
        {
            for (int k = -(d - 1); k <= (d - 1); k++)
            {
                int q = F.dq * d, r = F.dr * d;
                if (k < 0) { q += L.dq * (-k); r += L.dr * (-k); }
                else if (k > 0) { q += R.dq * k; r += R.dr * k; }
                result.Add((q, r, color));
            }
        }
        return result;
    }

    //십자(Plus) – 반경 1
    private static List<(int dq, int dr, Color color)> GetPlus1(Color color)
    {
        var result = new List<(int, int, Color)>(6);
        foreach (var d in AX_DIRS)
            result.Add((d.dq, d.dr, color));
        return result;
    }

    //3x3 박스 (시전자 주변 8칸)
    private static List<(int dq, int dr, Color color)> GetBox3x3(Color color)
    {
        var result = new List<(int, int, Color)>(8);
        for (int dq = -1; dq <= 1; dq++)
            for (int dr = -1; dr <= 1; dr++)
                if (!(dq == 0 && dr == 0))
                    result.Add((dq, dr, color));
        return result;
    }
}

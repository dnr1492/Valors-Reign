using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static EnumClass;

public static class SkillStepCsvImporterTool
{
    private const string CSV_PATH = "Assets/Resources/Datas/skillCard_steps.csv";
    private const string TEST_CSV_PATH = "Assets/Resources/Datas/test_skillCard_steps.csv";
    private const string SKILL_JSON_PATH = "Assets/Resources/Datas/skillCard_data.json";

    private static readonly Dictionary<string, SkillDuration> DurationMap = new(StringComparer.OrdinalIgnoreCase)
    {
        { "ThisRound", SkillDuration.ThisRound },
        { "UpToNextRound", SkillDuration.UpToNextRound },
        { "UpToSecondRound", SkillDuration.UpToSecondRound },
        { "UntilThisTurn", SkillDuration.UntilThisTurn },
        { "None", SkillDuration.None },
        { "", SkillDuration.None }
    };

    [MenuItem("정재욱/Import/skillCard_steps.csv → steps 적용")]
    public static void ImportSteps()
    {
        if (!File.Exists(SKILL_JSON_PATH))
        {
            Debug.Log($"스킬 JSON이 없습니다: {SKILL_JSON_PATH}\n먼저 Tool 메뉴에서 skillCard_data를 생성하세요.");
            return;
        }
        var json = File.ReadAllText(SKILL_JSON_PATH);
        var listWrapper = JsonUtility.FromJson<SkillCardDataList>(json);
        if (listWrapper == null || listWrapper.skillCardDatas == null)
        {
            Debug.Log("스킬 JSON 파싱 실패");
            return;
        }
        var dic = listWrapper.skillCardDatas.ToDictionary(c => c.id, c => c);

        //우선 CSV_PATH, 없으면 TEST_CSV_PATH를 자동 선택
        var csvPath = ResolveCsvPath(preferMain: true);
        if (string.IsNullOrEmpty(csvPath))
        {
            Debug.Log($"CSV가 없습니다. 메인({CSV_PATH})와 테스트({TEST_CSV_PATH}) 모두 확인 실패");
            return;
        }

        var rows = ParseCsv(CSV_PATH);

        var unknown = rows.Where(r => !dic.ContainsKey(r.cardId)).Select(r => r.cardId).Distinct().ToList();
        if (unknown.Count > 0) Debug.Log($"[CSV Import] JSON에 없는 cardId 존재: {string.Join(",", unknown)}");
        foreach (var g in rows.GroupBy(r => (r.cardId, r.stepIdx)))
            if (g.Count() > 1)
                Debug.Log($"[CSV Import] 중복 stepIdx 감지 cardId={g.Key.cardId}, stepIdx={g.Key.stepIdx}");

        //반영
        SkillDataLoader.ApplyStepsFromRows(rows, dic);

        //저장
        var pretty = JsonUtility.ToJson(listWrapper, true);
        File.WriteAllText(SKILL_JSON_PATH, pretty);
        AssetDatabase.Refresh();
        Debug.Log($"steps 적용 완료 → {SKILL_JSON_PATH}");
    }

    [MenuItem("정재욱/Import/test_skillCard_steps.csv → steps 적용")]
    public static void ImportSteps_TestOnly()
    {
        if (!File.Exists(SKILL_JSON_PATH))
        {
            Debug.LogError($"스킬 JSON이 없습니다: {SKILL_JSON_PATH}\n먼저 Tool 메뉴에서 skillCard_data를 생성하세요.");
            return;
        }

        var json = File.ReadAllText(SKILL_JSON_PATH);
        var listWrapper = JsonUtility.FromJson<SkillCardDataList>(json);
        if (listWrapper?.skillCardDatas == null)
        {
            Debug.LogError("스킬 JSON 파싱 실패");
            return;
        }
        var dic = listWrapper.skillCardDatas.ToDictionary(c => c.id, c => c);

        //우선 TEST_CSV_PATH, 없으면 CSV_PATH를 자동 선택
        var csvPath = ResolveCsvPath(preferMain: false);
        if (string.IsNullOrEmpty(csvPath))
        {
            Debug.Log($"CSV가 없습니다. 테스트({TEST_CSV_PATH})와 메인({CSV_PATH}) 모두 확인 실패");
            return;
        }

        var rows = ParseCsv(csvPath);

        SkillDataLoader.ApplyStepsFromRows(rows, dic);

        var pretty = JsonUtility.ToJson(listWrapper, true);
        File.WriteAllText(SKILL_JSON_PATH, pretty);
        AssetDatabase.Refresh();
        Debug.Log($"[TEST] steps 적용 완료 → {SKILL_JSON_PATH} (source: {csvPath})");
    }

    //CSV → StepRow 리스트
    private static List<SkillDataLoader.StepRow> ParseCsv(string path)
    {
        var lines = File.ReadAllLines(path);
        var rows = new List<SkillDataLoader.StepRow>();
        if (lines.Length <= 1) return rows;

        var header = SplitCsvLine(lines[0]);
        int idx(string name) => header.FindIndex(h => string.Equals(h.Trim(), name, StringComparison.OrdinalIgnoreCase));

        //필수
        int iCardId = idx("cardId");
        int iStepIdx = idx("stepIdx");
        int iTargetKind = idx("targetKind");
        int iTrigger = idx("trigger");
        int iEffectType = idx("effectType");
        int iAmount = idx("amount");
        int iStatusId = idx("statusId");
        int iDuration = idx("duration");

        //조건(선택)
        int iC1 = idx("c1"), iOp1 = idx("op1"), iC1_i1 = idx("c1_i1"), iC1_i2 = idx("c1_i2"), iC1_s1 = idx("c1_s1");
        int iC2 = idx("c2"), iOp2 = idx("op2"), iC2_i1 = idx("c2_i1"), iC2_i2 = idx("c2_i2"), iC2_s1 = idx("c2_s1");
        int iC3 = idx("c3"), iOp3 = idx("op3"), iC3_i1 = idx("c3_i1"), iC3_i2 = idx("c3_i2"), iC3_s1 = idx("c3_s1");

        //스킬
        int iRangeType = idx("rangeType");       //LineForward3 / Ring2 / Custom ...
        int iRangeParam = idx("rangeParam");     //1,2,3...
        int iCustomOffs = idx("customOffsets");  //"dq,dr; dq,dr; ..."
        int iIncludeSelf = idx("includeSelf");   //true/false

        for (int li = 1; li < lines.Length; li++)
        {
            var cols = SplitCsvLine(lines[li]);
            if (cols.Count == 0) continue;

            var r = new SkillDataLoader.StepRow();

            r.cardId = ParseInt(cols, iCardId, li);
            r.stepIdx = ParseInt(cols, iStepIdx, li);
            r.targetKind = ParseEnum(cols, iTargetKind, SkillTargetKind.None, li);
            r.trigger = ParseEnum(cols, iTrigger, SkillTriggerType.Instant, li);
            r.effectType = ParseEnum(cols, iEffectType, SkillEffectType.Custom, li);
            r.amount = ParseInt(cols, iAmount, li);
            r.statusId = Get(cols, iStatusId);
            r.duration = ParseDurationEnum(cols, iDuration, li);

            //조건 c1~c3
            r.c1 = ParseEnum(cols, iC1, SkillConditionType.None, li);
            r.op1 = ParseEnum(cols, iOp1, SkillLogicOp.And, li);
            r.c1_i1 = ParseInt(cols, iC1_i1, li);
            r.c1_i2 = ParseInt(cols, iC1_i2, li);
            r.c1_s1 = Get(cols, iC1_s1);

            r.c2 = ParseEnum(cols, iC2, SkillConditionType.None, li);
            r.op2 = ParseEnum(cols, iOp2, SkillLogicOp.And, li);
            r.c2_i1 = ParseInt(cols, iC2_i1, li);
            r.c2_i2 = ParseInt(cols, iC2_i2, li);
            r.c2_s1 = Get(cols, iC2_s1);

            r.c3 = ParseEnum(cols, iC3, SkillConditionType.None, li);
            r.op3 = ParseEnum(cols, iOp3, SkillLogicOp.And, li);
            r.c3_i1 = ParseInt(cols, iC3_i1, li);
            r.c3_i2 = ParseInt(cols, iC3_i2, li);
            r.c3_s1 = Get(cols, iC3_s1);

            //스킬
            r.rangeType = ParseEnum(cols, iRangeType, SkillRangeType.None, li);
            r.rangeParam = ParseInt(cols, iRangeParam, li);
            r.includeSelf = ParseBool(cols, iIncludeSelf);
            r.customOffsets = ParseOffsets(Get(cols, iCustomOffs));

            rows.Add(r);
        }
        return rows;
    }

    private static List<(int dq, int dr)> ParseOffsets(string raw)
    {
        var list = new List<(int, int)>();
        if (string.IsNullOrWhiteSpace(raw)) return list;

        foreach (var token in raw.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
        {
            var p = token.Trim().Split(',');
            if (p.Length != 2) continue;
            if (int.TryParse(p[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int dq) &&
                int.TryParse(p[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int dr))
            {
                list.Add((dq, dr));
            }
        }
        return list;
    }

    private static bool ParseBool(List<string> cols, int i)
    {
        if (i < 0 || i >= cols.Count) return false;
        var s = (cols[i] ?? "").Trim();
        return s.Equals("true", StringComparison.OrdinalIgnoreCase) || s == "1" || s.Equals("y", StringComparison.OrdinalIgnoreCase);
    }

    private static SkillDuration ParseDurationEnum(List<string> cols, int i, int li)
    {
        if (i < 0 || i >= cols.Count) return SkillDuration.None;
        var raw = (cols[i] ?? "").Trim();

        if (raw.Length == 0) return SkillDuration.None;

        if (int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out int v))
        {
            return v switch
            {
                1 => SkillDuration.ThisRound,
                2 => SkillDuration.UpToNextRound,
                3 => SkillDuration.UpToSecondRound,
                4 => SkillDuration.UntilThisTurn,
                _ => SkillDuration.None
            };
        }

        if (Enum.TryParse<SkillDuration>(raw, true, out var direct)) return direct;
        if (DurationMap.TryGetValue(raw, out var mapped)) return mapped;

        Debug.Log($"[CSV Import] Duration 변환 실패 row={li + 1}, val='{raw}' → None 처리");
        return SkillDuration.None;
    }

    private static List<string> SplitCsvLine(string line)
    {
        var list = new List<string>();
        bool inQuotes = false;
        var cur = "";
        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (c == '"') { inQuotes = !inQuotes; continue; }
            if (c == ',' && !inQuotes) { list.Add(cur); cur = ""; }
            else cur += c;
        }
        list.Add(cur);
        return list.Select(s => s.Trim()).ToList();
    }

    private static string Get(List<string> cols, int i) => (i >= 0 && i < cols.Count) ? cols[i] : "";

    private static int ParseInt(List<string> cols, int i, int li)
    {
        if (i < 0 || i >= cols.Count) return 0;
        if (!int.TryParse(cols[i], NumberStyles.Integer, CultureInfo.InvariantCulture, out int v))
            Debug.Log($"[CSV Import] 정수 변환 실패 row={li + 1}, col={i}, val='{cols[i]}'");
        return v;
    }

    private static T ParseEnum<T>(List<string> cols, int i, T def, int li) where T : struct
    {
        if (i < 0 || i >= cols.Count) return def;
        var s = cols[i];
        return Enum.TryParse<T>(s, true, out var v) ? v : def;
    }

    private static string ResolveCsvPath(bool preferMain = true)
    {
        var main = CSV_PATH;
        var test = TEST_CSV_PATH;

        if (preferMain)
        {
            if (File.Exists(main)) return main;
            if (File.Exists(test))
            {
                Debug.LogWarning($"[CSV Import] 메인 CSV가 없어 테스트 CSV로 대체합니다: {test}");
                return test;
            }
        }
        else
        {
            if (File.Exists(test)) return test;
            if (File.Exists(main))
            {
                Debug.LogWarning($"[CSV Import] 테스트 CSV가 없어 메인 CSV로 대체합니다: {main}");
                return main;
            }
        }

        return null;  //둘 다 없음
    }
}
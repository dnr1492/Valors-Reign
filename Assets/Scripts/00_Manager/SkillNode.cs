using System.Collections.Generic;
using System.Linq;
using static EnumClass;

[System.Serializable]
public class TargetSelector
{
    public SkillTargetKind kind;
    public SkillRangeType rangeType;
    public int rangeParam;  //타입별 파라미터 (예: LineForwardN의 N 등)
    public List<(int dq, int dr)> customOffsets;
    public bool includeSelf;
}

[System.Serializable]
public class SkillCondition { public SkillConditionType type; public int i1; public int i2; public string s1; }

[System.Serializable]
public class ConditionBlock { public SkillLogicOp op; public SkillCondition cond; }

[System.Serializable]
public class EffectPayload { public SkillEffectType type; public int amount; public string statusId; public SkillDuration duration; }

[System.Serializable]
public class EffectStep
{
    public TargetSelector target;
    public List<ConditionBlock> conditions = new();
    public EffectPayload effect;
    public SkillTriggerType trigger = SkillTriggerType.Instant;
}

public static class SkillDataLoader
{
    public class StepRow
    {
        public int cardId, stepIdx, amount, c1_i1, c1_i2, c2_i1, c2_i2, c3_i1, c3_i2;

        public SkillTargetKind targetKind;
        public bool includeSelf;  //자기 칸 포함 여부
        public SkillRangeType rangeType;
        public int rangeParam;
        public List<(int dq, int dr)> customOffsets;

        public SkillTriggerType trigger;
        public SkillEffectType effectType;
        public string statusId;
        public SkillDuration duration;

        public SkillConditionType c1; public SkillLogicOp op1; public string c1_s1;
        public SkillConditionType c2; public SkillLogicOp op2; public string c2_s1;
        public SkillConditionType c3; public SkillLogicOp op3; public string c3_s1;
    }

    public static void ApplyStepsFromRows(IEnumerable<StepRow> rows, Dictionary<int, SkillCardData> dic)
    {
        foreach (var g in rows.GroupBy(r => r.cardId))
        {
            if (!dic.TryGetValue(g.Key, out var card)) continue;
            card.steps.Clear();

            foreach (var row in g.OrderBy(r => r.stepIdx))
            {
                var tsel = new TargetSelector
                {
                    kind = row.targetKind,
                    includeSelf = row.includeSelf,
                    rangeType = row.rangeType,
                    rangeParam = row.rangeParam,
                    customOffsets = (row.customOffsets != null) ? new List<(int, int)>(row.customOffsets) : null
                };

                var step = new EffectStep
                {
                    target = tsel,
                    trigger = row.trigger,
                    effect = new EffectPayload
                    {
                        type = row.effectType,
                        amount = row.amount,
                        statusId = row.statusId,
                        duration = row.duration
                    },
                    conditions = new List<ConditionBlock>()
                };

                if (row.c1 != SkillConditionType.None)
                    step.conditions.Add(new ConditionBlock { op = row.op1, cond = new SkillCondition { type = row.c1, i1 = row.c1_i1, i2 = row.c1_i2, s1 = row.c1_s1 } });
                if (row.c2 != SkillConditionType.None)
                    step.conditions.Add(new ConditionBlock { op = row.op2, cond = new SkillCondition { type = row.c2, i1 = row.c2_i1, i2 = row.c2_i2, s1 = row.c2_s1 } });
                if (row.c3 != SkillConditionType.None)
                    step.conditions.Add(new ConditionBlock { op = row.op3, cond = new SkillCondition { type = row.c3, i1 = row.c3_i1, i2 = row.c3_i2, s1 = row.c3_s1 } });

                card.steps.Add(step);
            }
        }
    }
}

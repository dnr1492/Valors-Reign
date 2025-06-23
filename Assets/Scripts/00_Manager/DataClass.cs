using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TokenPack
{
    public List<TokenSlotData> tokenSlots = new();
}

[System.Serializable]
public class TokenSlotData
{
    public int tokenKey;  //캐릭터 고유 ID
    public int col, row;  //배치된 위치
    public List<SkillCountData> skillCounts = new();  //skillId, count
}

[System.Serializable]
public class SkillCountData
{
    public int skillId;
    public int count;
}
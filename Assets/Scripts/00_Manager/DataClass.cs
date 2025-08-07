using System.Collections.Generic;
using static EnumClass;

[System.Serializable]
public class DeckPack
{
    public string guid;  //덱 고유 ID
    public string deckName = "";  //덱 이름
    public CharacterRace race;  //덱 종족
    public List<TokenSlotData> tokenSlots = new();
}

[System.Serializable]
public class TokenSlotData
{
    public int tokenKey;
    public int col, row;  //좌표
    public List<SkillCountData> skillCounts = new();  //skillId, count
}

[System.Serializable]
public class SkillCountData
{
    public int skillId;
    public int count;
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

//[System.Serializable]
//public class UserCardInventory
//{
//    public List<int> ownedCharacterCardIds = new();  //보유한 캐릭터 카드 ID 목록
//    public Dictionary<int, int> ownedSkillCardCounts = new();  //스킬카드 ID, 개수
//}

//[System.Serializable]
//public class UserData
//{
//    public string userId;
//    public UserCardInventory inventory = new();
//    public List<DeckPack> savedDecks = new();  //유저가 만든 덱들
//}
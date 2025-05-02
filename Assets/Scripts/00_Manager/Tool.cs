using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using static EnumClass;

public class Tool
{
    private static void LoadDataFromJSON<T>(T data, string fileName)
    {
        //Resources/Datas 경로
        string path = Path.Combine(Application.dataPath, "Resources/Datas");
        string jsonPath = Path.Combine(path, fileName);

        //JSON 파일 저장
        string jsonData = JsonUtility.ToJson(data, true);
        File.WriteAllText(jsonPath, jsonData);

        Debug.Log($"JSON data saved at: {jsonPath}");
        AssetDatabase.Refresh();
    }

    #region 게임 플레이 데이터
    [MenuItem("정재욱/Generate gamePlay_data")]
    private static void GenerateGamePlayData()
    {
        GamePlayData gamePlayData = new GamePlayData {
            maxCost = 10,
            maxLeaderCount = 1,
            maxHighTierCount = 2,
            maxMiddleTierCount = 3,
            maxLowTierCount = 5,
            limitMiddle = 2,
            limitLow = 3,
        };

        LoadDataFromJSON(gamePlayData, "gamePlay_data.json");
    }
    #endregion

    #region 캐릭터 카드 데이터
    [MenuItem("정재욱/Generate characterCard_data")]
    private static void GenerateCharacterCardData()
    {
        //JSON 데이터 생성
        CharacterCardDataList list = new CharacterCardDataList {
            characterCardDatas = new List<CharacterCardData>()
        };

        // ===== 임시 데이터 =====
        list.characterCardDatas.Add(new CharacterCardData { id = 0, name = "그림자 속 암살자", skills = new List<int> { 1000, 1001 }, tier = CharacterTierAndCost.Captain, race = CharacterRace.Primordial, job = CharacterJob.Dealer });
        list.characterCardDatas.Add(new CharacterCardData { id = 1, name = "하얀 검사", skills = new List<int> { 1000, 1001 }, tier = CharacterTierAndCost.Captain, race = CharacterRace.Primordial, job = CharacterJob.Dealer });
        list.characterCardDatas.Add(new CharacterCardData { id = 2, name = "그림자 속 수호자", skills = new List<int> { 1002, 1003 }, tier = CharacterTierAndCost.Captain, race = CharacterRace.Primordial, job = CharacterJob.Tanker });
        list.characterCardDatas.Add(new CharacterCardData { id = 3, name = "어설픈 방패병", skills = new List<int> { 1002, 1003 }, tier = CharacterTierAndCost.Captain, race = CharacterRace.Primordial, job = CharacterJob.Tanker });
        list.characterCardDatas.Add(new CharacterCardData { id = 4, name = "최전선의 창병", skills = new List<int> { 1000, 1003 }, tier = CharacterTierAndCost.Captain, race = CharacterRace.Primordial, job = CharacterJob.Tanker });

        LoadDataFromJSON(list, "characterCard_data.json");
    }
    #endregion

    #region 스킬 카드 데이터
    [MenuItem("정재욱/Generate skillCard_data")]
    private static void GenerateSkillCardData()
    {
        //JSON 데이터 생성
        SkillCardDataList list = new SkillCardDataList {
            skillCardDatas = new List<SkillCardData>()
        };

        // ===== 임시 데이터 =====
        list.skillCardDatas.Add(new SkillCardData { id = 1000, name = "Skill Test 1", rank = (int)SkillCardRankAndMpConsum.Rank1, type = SkillCardType.Move });
        list.skillCardDatas.Add(new SkillCardData { id = 1001, name = "Skill Test 1", rank = (int)SkillCardRankAndMpConsum.Rank2, type = SkillCardType.Attack });
        list.skillCardDatas.Add(new SkillCardData { id = 1002, name = "Skill Test 2", rank = (int)SkillCardRankAndMpConsum.Rank1, type = SkillCardType.Move });
        list.skillCardDatas.Add(new SkillCardData { id = 1003, name = "Skill Test 2", rank = (int)SkillCardRankAndMpConsum.Rank2, type = SkillCardType.Attack });
        list.skillCardDatas.Add(new SkillCardData { id = 1004, name = "Skill Test 3", rank = (int)SkillCardRankAndMpConsum.Rank1, type = SkillCardType.Buff });

        LoadDataFromJSON(list, "skillCard_data.json");
    }
    #endregion
}

#region 게임 플레이 데이터
[Serializable]
public class GamePlayData
{
    public int maxCost;
    public int maxLeaderCount;
    public int maxHighTierCount;
    public int maxMiddleTierCount;
    public int maxLowTierCount;
    public int limitMiddle;
    public int limitLow;
}
#endregion

#region 캐릭터 카드 데이터
[Serializable]
public class CharacterCardDataList
{
    public List<CharacterCardData> characterCardDatas;
}

[Serializable]
public class CharacterCardData
{
    public int id;
    public string name;
    public CharacterTierAndCost tier;
    //public int cost;  //Tier에 각 Cost의 값이 정의되어 있음
    public int hp;
    public int mp;
    public CharacterRace race;
    public CharacterJob job;
    public List<int> skills;  //ex) 1001, 1002, 1003)
}
#endregion

#region 스킬 카드 데이터
[Serializable]
public class SkillCardDataList
{
    public List<SkillCardData> skillCardDatas;
}

[Serializable]
public class SkillCardData
{
    public int id;  //CharacterCardData의 skills가 키값, ex) 1001
    public string name;
    public int rank;
    //public int mpConsum;  //MP소모량은 Rank랑 동일
    public SkillCardType type;
}
#endregion
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using static EnumClass;

public class Tool
{
#if UNITY_EDITOR
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

        list.characterCardDatas.Add(new CharacterCardData { id = 101, name = "어설픈 방패병", tier = CharacterTierAndCost.Low, hp = 4, mp = 1, race = CharacterRace.Primordial, job = CharacterJob.Tanker, skills = new List<int> { 1001, 3001 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 102, name = "최전선의 창병", tier = CharacterTierAndCost.Low, hp = 3, mp = 1, race = CharacterRace.Primordial, job = CharacterJob.Tanker, skills = new List<int> { 1001, 2001, 3002 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 103, name = "그림자속 수호자", tier = CharacterTierAndCost.Low, hp = 2, mp = 2, race = CharacterRace.Primordial, job = CharacterJob.Tanker, skills = new List<int> { 1001, 3003, 3004 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 104, name = "침착한 수비수", tier = CharacterTierAndCost.Middle, hp = 4, mp = 2, race = CharacterRace.Primordial, job = CharacterJob.Tanker, skills = new List<int> { 1001, 3005, 2002 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 105, name = "나태한 문지기", tier = CharacterTierAndCost.Middle, hp = 3, mp = 3, race = CharacterRace.Primordial, job = CharacterJob.Tanker, skills = new List<int> { 1001, 3006 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 106, name = "마법방패를 두른 수호자", tier = CharacterTierAndCost.Middle, hp = 2, mp = 4, race = CharacterRace.Primordial, job = CharacterJob.Tanker, skills = new List<int> { 1001, 4001, 3007 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 107, name = "산군 -강수-", tier = CharacterTierAndCost.High, hp = 6, mp = 3, race = CharacterRace.Primordial, job = CharacterJob.Tanker, skills = new List<int> { 1001, 3008, 2003, 3009 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 108, name = "은결의 마녀 -루안-", tier = CharacterTierAndCost.High, hp = 2, mp = 6, race = CharacterRace.Primordial, job = CharacterJob.Tanker, skills = new List<int> { 1001, 5001, 5002, 3010, 3011 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 109, name = "블러드가드 -로간-", tier = CharacterTierAndCost.High, hp = 6, mp = 3, race = CharacterRace.Primordial, job = CharacterJob.Tanker, skills = new List<int> { 1001, 4002, 2004, 3012 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 110, name = "검은 장미 -엘-", tier = CharacterTierAndCost.Boss, hp = 2, mp = 1, race = CharacterRace.Primordial, job = CharacterJob.Tanker, skills = new List<int> { 1001, 3013 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 111, name = "빛의 방패 -세노티-", tier = CharacterTierAndCost.Boss, hp = 2, mp = 1, race = CharacterRace.Primordial, job = CharacterJob.Tanker, skills = new List<int> { 1001, 4003 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 112, name = "철의 성벽 -우르-", tier = CharacterTierAndCost.Boss, hp = 3, mp = 1, race = CharacterRace.Primordial, job = CharacterJob.Tanker, skills = new List<int> { 1001, 3014 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 113, name = "하얀검사", tier = CharacterTierAndCost.Low, hp = 3, mp = 2, race = CharacterRace.Primordial, job = CharacterJob.Dealer, skills = new List<int> { 1001, 2005 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 114, name = "그림자 속 암살자", tier = CharacterTierAndCost.Low, hp = 2, mp = 2, race = CharacterRace.Primordial, job = CharacterJob.Dealer, skills = new List<int> { 1001, 2006, 2007 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 115, name = "죽음과 함께 걷는 암살자", tier = CharacterTierAndCost.Low, hp = 2, mp = 3, race = CharacterRace.Primordial, job = CharacterJob.Dealer, skills = new List<int> { 1001, 2008 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 116, name = "섬광의 명궁", tier = CharacterTierAndCost.Middle, hp = 4, mp = 2, race = CharacterRace.Primordial, job = CharacterJob.Dealer, skills = new List<int> { 1001, 2009, 2010 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 117, name = "푸른눈의 저격수", tier = CharacterTierAndCost.Middle, hp = 2, mp = 3, race = CharacterRace.Primordial, job = CharacterJob.Dealer, skills = new List<int> { 1001, 4004, 1002, 2011 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 118, name = "석양의 보안관", tier = CharacterTierAndCost.Middle, hp = 3, mp = 2, race = CharacterRace.Primordial, job = CharacterJob.Dealer, skills = new List<int> { 1001, 2012, 2013, 2014 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 119, name = "카이른 블레이즈", tier = CharacterTierAndCost.High, hp = 6, mp = 3, race = CharacterRace.Primordial, job = CharacterJob.Dealer, skills = new List<int> { 1001, 2015, 2016, 2017 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 120, name = "광전사 -가르칸-", tier = CharacterTierAndCost.High, hp = 9, mp = -1, race = CharacterRace.Primordial, job = CharacterJob.Dealer, skills = new List<int> { 1001, 2018, 2019, 2020 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 121, name = "광무 -잔영-", tier = CharacterTierAndCost.High, hp = 5, mp = 4, race = CharacterRace.Primordial, job = CharacterJob.Dealer, skills = new List<int> { 1001, 2021, 2022, 2023 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 122, name = "검은불꽃 -청현-", tier = CharacterTierAndCost.Boss, hp = 2, mp = 1, race = CharacterRace.Primordial, job = CharacterJob.Dealer, skills = new List<int> { 1001, 2024, 2025 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 123, name = "이계의 창 -라이트-", tier = CharacterTierAndCost.Boss, hp = 1, mp = 2, race = CharacterRace.Primordial, job = CharacterJob.Dealer, skills = new List<int> { 1001, 2026 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 124, name = "기사단장 -자스안-", tier = CharacterTierAndCost.Boss, hp = 2, mp = 1, race = CharacterRace.Primordial, job = CharacterJob.Dealer, skills = new List<int> { 1001, 2027 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 125, name = "자유의 치유사", tier = CharacterTierAndCost.Low, hp = 1, mp = 4, race = CharacterRace.Primordial, job = CharacterJob.Supporter, skills = new List<int> { 1001, 4005 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 126, name = "행운의 치료사", tier = CharacterTierAndCost.Low, hp = 2, mp = 3, race = CharacterRace.Primordial, job = CharacterJob.Supporter, skills = new List<int> { 1001, 4006 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 127, name = "검은 마력의 저주사", tier = CharacterTierAndCost.Low, hp = 2, mp = 2, race = CharacterRace.Primordial, job = CharacterJob.Supporter, skills = new List<int> { 1001, 5003, 5004 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 128, name = "노련한 전략가", tier = CharacterTierAndCost.Middle, hp = 2, mp = 3, race = CharacterRace.Primordial, job = CharacterJob.Supporter, skills = new List<int> { 1001, 1003, 4007, 3015 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 129, name = "교활한 정보원", tier = CharacterTierAndCost.Middle, hp = 3, mp = 3, race = CharacterRace.Primordial, job = CharacterJob.Supporter, skills = new List<int> { 1001, 4008, 1004 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 130, name = "현자의 돌을 지닌 연금술사", tier = CharacterTierAndCost.Middle, hp = 2, mp = 4, race = CharacterRace.Primordial, job = CharacterJob.Supporter, skills = new List<int> { 1001, 4009, 3016 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 131, name = "로열 에이드", tier = CharacterTierAndCost.High, hp = 3, mp = 5, race = CharacterRace.Primordial, job = CharacterJob.Supporter, skills = new List<int> { 1001, 4010, 5005, 4011, 4012 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 132, name = "북방의 마법현자 -와이즈-", tier = CharacterTierAndCost.High, hp = 5, mp = 4, race = CharacterRace.Primordial, job = CharacterJob.Supporter, skills = new List<int> { 1001, 4013, 4014, 5006 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 133, name = "순풍의 가인 -라에나-", tier = CharacterTierAndCost.High, hp = 3, mp = 6, race = CharacterRace.Primordial, job = CharacterJob.Supporter, skills = new List<int> { 1001, 4015, 4016, 4017 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 134, name = "시간을 걷는 자 -노아-", tier = CharacterTierAndCost.Boss, hp = 1, mp = 2, race = CharacterRace.Primordial, job = CharacterJob.Supporter, skills = new List<int> { 1001, 4018, 5007 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 135, name = "천상의 울림 -벨-", tier = CharacterTierAndCost.Boss, hp = 2, mp = 2, race = CharacterRace.Primordial, job = CharacterJob.Supporter, skills = new List<int> { 1001, 4019 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 136, name = "망각 -루누스-", tier = CharacterTierAndCost.Boss, hp = 2, mp = 1, race = CharacterRace.Primordial, job = CharacterJob.Supporter, skills = new List<int> { 1001, 5008 } });

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

        list.skillCardDatas.Add(new SkillCardData { id = 1001, name = "어설픈 방패병 (이동)", rank = (int)SkillCardRankAndMpConsum.Zero, effect = "", cardType = SkillCardType.Move, rangeType = SkillRangeType.None });

        LoadDataFromJSON(list, "skillCard_data.json");
    }
    #endregion
#endif
}

#region 유저 데이터

#endregion

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
    public int id;  //CharacterCardData의 skills가 키값 ex) 1001
    public string name;
    public int rank;
    //public int mpConsum;  //MP소모량은 Rank랑 동일
    public string effect;
    public SkillCardType cardType;
    public SkillRangeType rangeType;
    public List<(int dq, int dr, Color color)> customOffsetRange;

    // ↓↓↓ Damage, Count, 이동 불가 (enum) 등으로 추가하기 ↓↓↓ //
    // ↓↓↓ Damage, Count, 이동 불가 (enum) 등으로 추가하기 ↓↓↓ //
    // ↓↓↓ Damage, Count, 이동 불가 (enum) 등으로 추가하기 ↓↓↓ //
}
#endregion
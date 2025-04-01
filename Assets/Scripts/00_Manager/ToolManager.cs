using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.U2D.Animation;
using UnityEngine;

public class ToolManager : MonoBehaviour
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
            maxCost = 10
        };

        LoadDataFromJSON(gamePlayData, "gamePlay_data.json");
    }
    #endregion

    #region 카드 데이터
    [MenuItem("정재욱/Generate card_data")]
    private static void GenerateCardData()
    {
        //JSON 데이터 생성
        CardDataList list = new CardDataList {
            cardDatas = new List<CardData>()
        };

        list.cardDatas.Add(new CardData { });

        LoadDataFromJSON(list, "card_data.json");
    }
    #endregion

    #region 스킬 데이터
    [MenuItem("정재욱/Generate skill_data")]
    private static void GenerateSkillData()
    {
        //JSON 데이터 생성
        SkillDataList list = new SkillDataList {
            skillDatas = new List<SkillData>()
        };

        list.skillDatas.Add(new SkillData { });

        LoadDataFromJSON(list, "skill_data.json");
    }
    #endregion

    #region 스킬 랭크 데이터
    [MenuItem("정재욱/Generate skillRank_data")]
    private static void GenerateSkillRankData()
    {
        //JSON 데이터 생성
        SkillRankDataList list = new SkillRankDataList {
            skillRankDatas = new List<SkillRankData>()
        };

        list.skillRankDatas.Add(new SkillRankData { });

        LoadDataFromJSON(list, "skillRank_data.json");
    }
    #endregion
}

#region 게임 플레이 데이터
[Serializable]
public class GamePlayData
{
    public int maxCost;
}
#endregion

#region 카드 데이터
[Serializable]
public class CardDataList
{
    public List<CardData> cardDatas;
}

[Serializable]
public class CardData
{
    public int id;
    public string name;
    public string tier;
    public int cost;
    public int hp;
    public int mp;
    public string race;
    public string job;
    public int attackRange;
    public int attackDirection;
    public List<int> skills;  //ex) 101, 102, 103)
}
#endregion

#region 스킬 데이터
[Serializable]
public class SkillDataList
{
    public List<SkillData> skillDatas;
}

[Serializable]
public class SkillData
{
    public int id;  //CardData의 skills가 키값, ex) 101
    public string name;
    public int mpConsum;
    public int rank;
}
#endregion

#region 스킬 랭크 데이터
[Serializable]
public class SkillRankDataList
{
    public List<SkillRankData> skillRankDatas;
}

[Serializable]
public class SkillRankData
{
    public int id;  //SkillData의 rank가 키값
    public string name;
    public string type;
    public string effect;
    public int range;
    public int direction;
}
#endregion
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class DataManager
{
    private static DataManager instance = null;

    private DataManager() { }

    public static DataManager GetInstance()
    {
        if (instance == null) instance = new DataManager();
        return instance;
    }

    private GamePlayData _gamePlayData;
    private Dictionary<int, CardData> dicCardData = new Dictionary<int, CardData>();
    private Dictionary<int, SkillData> dicSkillData = new Dictionary<int, SkillData>();
    private Dictionary<int, SkillRankData> dicSkillRankData = new Dictionary<int, SkillRankData>();

    public void LoadGamePlayData()
    {
        _gamePlayData = new GamePlayData();
        string json = Resources.Load<TextAsset>("Datas/gamePlay_data").text;
        var gamePlayData = JsonConvert.DeserializeObject<GamePlayData>(json);
        _gamePlayData = gamePlayData;
    }

    public void LoadCardData()
    {
        dicCardData = new Dictionary<int, CardData>();
        string json = Resources.Load<TextAsset>("Datas/card_data").text;
        CardDataList cardDataList = JsonConvert.DeserializeObject<CardDataList>(json);
        foreach (var data in cardDataList.cardDatas)
        {
            if (!dicCardData.ContainsKey(data.id)) {
                dicCardData.Add(data.id, data);
                Debug.Log($"Card Data : {dicCardData[data.id]} = {data.name}");
            }
        }
    }

    public void LoadSkillData()
    {
        dicSkillData = new Dictionary<int, SkillData>();
        string json = Resources.Load<TextAsset>("Datas/skill_data").text;
        SkillDataList skillDataList = JsonConvert.DeserializeObject<SkillDataList>(json);
        foreach (var data in skillDataList.skillDatas)
        {
            if (!dicSkillData.ContainsKey(data.id)) {
                dicSkillData.Add(data.id, data);
                Debug.Log($"Skill Data : {dicSkillData[data.id]} = {data.name}");
            }
        }
    }

    public void LoadSkillRankData()
    {
        dicSkillRankData = new Dictionary<int, SkillRankData>();
        string json = Resources.Load<TextAsset>("Datas/skillRank_data").text;
        SkillRankDataList skillRankDataList = JsonConvert.DeserializeObject<SkillRankDataList>(json);
        foreach (var data in skillRankDataList.skillRankDatas)
        {
            if (!dicSkillRankData.ContainsKey(data.id)) {
                dicSkillRankData.Add(data.id, data);
                Debug.Log($"Skill Rank Data : {dicSkillRankData[data.id]} = {data.name}");
            }
        }
    }
}
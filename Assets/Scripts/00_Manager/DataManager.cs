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

    public GamePlayData _gamePlayData;
    public Dictionary<int, CharacterCardData> dicCharacterCardData = new Dictionary<int, CharacterCardData>();
    public Dictionary<int, SkillCardData> dicSkillCardData = new Dictionary<int, SkillCardData>();

    public void LoadGamePlayData()
    {
        _gamePlayData = new GamePlayData();
        string json = Resources.Load<TextAsset>("Datas/gamePlay_data").text;
        var gamePlayData = JsonConvert.DeserializeObject<GamePlayData>(json);
        _gamePlayData = gamePlayData;
    }

    public void LoadCharacterCardData()
    {
        dicCharacterCardData = new Dictionary<int, CharacterCardData>();
        string json = Resources.Load<TextAsset>("Datas/characterCard_data").text;
        CharacterCardDataList characterCardDataList = JsonConvert.DeserializeObject<CharacterCardDataList>(json);
        foreach (var data in characterCardDataList.characterCardDatas)
        {
            if (!dicCharacterCardData.ContainsKey(data.id)) {
                dicCharacterCardData.Add(data.id, data);
            }
        }
    }

    public void LoadSkillCardData()
    {
        dicSkillCardData = new Dictionary<int, SkillCardData>();
        string json = Resources.Load<TextAsset>("Datas/skillCard_data").text;
        SkillCardDataList skillCardDataList = JsonConvert.DeserializeObject<SkillCardDataList>(json);
        foreach (var data in skillCardDataList.skillCardDatas)
        {
            if (!dicSkillCardData.ContainsKey(data.id)) {
                dicSkillCardData.Add(data.id, data);
            }
        }
    }
}
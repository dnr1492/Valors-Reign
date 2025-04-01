using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainScene : MonoBehaviour
{
    private void Awake()
    {
        DataManager.GetInstance().LoadGamePlayData();
        DataManager.GetInstance().LoadCardData();
        DataManager.GetInstance().LoadSkillData();
        DataManager.GetInstance().LoadSkillRankData();
    }
}

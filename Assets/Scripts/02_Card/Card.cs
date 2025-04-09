using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    public enum State { None = -1, Front, Back }
    //public enum CardType { None = -1, Character, Skill }

    [SerializeField] GameObject front, back;
    [SerializeField] Text skillCardName;

    public void SetCardData(SkillCardData skillCardData, State state = State.Back)
    {
        skillCardName.text = skillCardData.name;
    }
}

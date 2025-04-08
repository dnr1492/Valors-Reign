using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    [SerializeField] GameObject front, back;
    [SerializeField] Text skillCardName;

    public void SetCardData(SkillCardData skillCardData)
    {
        skillCardName.text = skillCardData.name;
    }
}

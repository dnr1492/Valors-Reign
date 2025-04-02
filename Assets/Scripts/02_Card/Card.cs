using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    [SerializeField] Text cardName;

    public void SetCardData(SkillCardData cardData)
    {
        cardName.text = cardData.name;
    }
}

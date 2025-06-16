using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillCard : Card
{
    [SerializeField] TextMeshProUGUI txtName, txtSkillEffect;
    [SerializeField] Image imgSkill;

    protected override void SetSkillCard(Sprite sprite, SkillCardData cardData)
    {
        txtName.text = cardData.name;
        txtSkillEffect.text = cardData.effect;

        imgSkill.sprite = sprite;
    }
}

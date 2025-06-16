using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UISkillCard : UISkillInfoPopup
{
    [SerializeField] TextMeshProUGUI txtName, txtSkillEffect;
    [SerializeField] Image imgSkill;

    public void Set(Sprite sprite, SkillCardData skillCardData)
    {
        txtName.text = skillCardData.name;
        imgSkill.sprite = sprite;
        txtSkillEffect.text = skillCardData.effect;
    }
}

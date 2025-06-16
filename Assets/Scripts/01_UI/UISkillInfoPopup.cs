using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISkillInfoPopup : MonoBehaviour
{
    public void Init(SkillCardData skillCardData)
    {
        var sprite = SpriteManager.Instance.dicSkillSprite[skillCardData.name];
        GetComponentInChildren<UISkillCard>().Set(sprite, skillCardData);
    }
}

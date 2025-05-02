using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CharacterCard : Card
{
    [SerializeField] TextMeshProUGUI txtName, txtTierAndJob;
    [SerializeField] TextMeshProUGUI txtHp, txtMp;
    [SerializeField] TextMeshProUGUI[] txtCounts, txtRanks, txtNames;
    [SerializeField] Image imgCharacter;

    protected override void SetCharacterCard(Sprite sprite, CharacterCardData cardData)
    {
        foreach (var txt in txtCounts) txt.gameObject.SetActive(false);
        foreach (var txt in txtRanks) txt.gameObject.SetActive(false);
        foreach (var txt in txtNames) txt.gameObject.SetActive(false);

        txtName.text = cardData.name;
        txtTierAndJob.text = cardData.tier.ToString()[0] + " / " + cardData.job.ToString()[0];
        txtHp.text = cardData.hp.ToString();
        txtMp.text = cardData.mp.ToString();

        for (int i = 0; i < cardData.skills.Count; i++) {
            var skillCardData = GetSkillCardData(cardData.skills[i]);
            txtCounts[i].gameObject.SetActive(true);
            txtCounts[i].text = skillCardData.id.ToString();
            txtRanks[i].gameObject.SetActive(true);
            txtRanks[i].text = skillCardData.rank.ToString();
            txtNames[i].gameObject.SetActive(true);
            txtNames[i].text = skillCardData.name;
        }

        imgCharacter.sprite = sprite;
    }

    private SkillCardData GetSkillCardData(int key)
    {
        var skillCardData = DataManager.Instance.dicSkillCardData;
        return skillCardData[key];
    }
}

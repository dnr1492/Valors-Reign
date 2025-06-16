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
    [SerializeField] Button[] btnSkills;
    [SerializeField] Image imgCharacter;

    protected override void SetCharacterCard(Sprite sprite, CharacterCardData cardData)
    {
        foreach (var txt in txtCounts) txt.gameObject.SetActive(false);
        foreach (var txt in txtRanks) txt.gameObject.SetActive(false);
        foreach (var txt in txtNames) txt.gameObject.SetActive(false);
        foreach (var btn in btnSkills) {
            btn.onClick.RemoveAllListeners();
            btn.interactable = false;
        }

        txtName.text = cardData.name;
        txtTierAndJob.text = cardData.tier.ToString()[0] + " / " + cardData.job.ToString()[0];
        txtHp.text = cardData.hp.ToString();
        txtMp.text = cardData.mp.ToString();

        var skillCardData = DataManager.Instance.dicSkillCardData;
        for (int i = 0; i < cardData.skills.Count; i++) 
        {
            var data = skillCardData[cardData.skills[i]];
            if (data == null) continue;

            txtCounts[i].gameObject.SetActive(true);
            txtCounts[i].text = data.id.ToString();

            txtRanks[i].gameObject.SetActive(true);
            txtRanks[i].text = data.rank.ToString();

            txtNames[i].gameObject.SetActive(true);
            txtNames[i].text = data.name;

            btnSkills[i].interactable = true;
            btnSkills[i].onClick.AddListener(() => { Debug.Log("Skill Name: " + data.name); });
        }

        imgCharacter.sprite = sprite;
    }
}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CharacterCard : Card
{
    [SerializeField] GameObject skillInfoPopupPrefab;
    [SerializeField] TextMeshProUGUI txtName, txtTierAndJob;
    [SerializeField] TextMeshProUGUI txtHp, txtMp;
    [SerializeField] TextMeshProUGUI[] txtCounts, txtRanks, txtNames;
    [SerializeField] Button[] btnSkills;
    [SerializeField] Image imgCharacter;

    protected override void SetCharacterCard(Sprite sprite, CharacterCardData characterCardData)
    {
        foreach (var txt in txtCounts) txt.gameObject.SetActive(false);
        foreach (var txt in txtRanks) txt.gameObject.SetActive(false);
        foreach (var txt in txtNames) txt.gameObject.SetActive(false);
        foreach (var btn in btnSkills) btn.onClick.RemoveAllListeners();

        txtName.text = characterCardData.name;
        txtTierAndJob.text = characterCardData.tier.ToString()[0] + " / " + characterCardData.job.ToString()[0];
        txtHp.text = characterCardData.hp.ToString();
        txtMp.text = characterCardData.mp.ToString();

        var skillCardData = DataManager.Instance.dicSkillCardData;
        for (int i = 0; i < characterCardData.skills.Count; i++) 
        {
            var data = skillCardData[characterCardData.skills[i]];
            if (data == null) continue;

            txtCounts[i].gameObject.SetActive(true);
            txtCounts[i].text = data.id.ToString();

            txtRanks[i].gameObject.SetActive(true);
            txtRanks[i].text = data.rank.ToString();

            txtNames[i].gameObject.SetActive(true);
            txtNames[i].text = data.name;

            btnSkills[i].onClick.AddListener(() => { OpenSkillInfoPopup(data); });
        }

        imgCharacter.sprite = sprite;
    }

    private void OpenSkillInfoPopup(SkillCardData skillCardData)
    {
        var skillInfoPopup = Instantiate(skillInfoPopupPrefab, transform);
        skillInfoPopup.GetComponent<UISkillInfoPopup>().Init(skillCardData);
    }
}

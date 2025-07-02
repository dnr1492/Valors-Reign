using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static EnumClass;

public class CharacterCard : Card
{
    [SerializeField] SkillSlotCollection skillSlotCollection;
    [SerializeField] TextMeshProUGUI txtName;
    [SerializeField] TextMeshProUGUI txtHp, txtMp;
    [SerializeField] TextMeshProUGUI[] txtCounts, txtNames;
    [SerializeField] Button[] btnCounts, btnSkills;
    [SerializeField] Image imgCharacter;
    [SerializeField] Button btnConfirm;

    private int[] skillCardCounts;
    private const int maxSkillCardCount = 4;  //최대 개수
    private CharacterToken curClickedToken;  //현재 선택된 토큰

    protected override void SetCharacterCard(CharacterToken clickedToken, Sprite sprite, CharacterCardData characterCardData)
    {
        ResetUI();
        curClickedToken = clickedToken;

        btnConfirm.onClick.RemoveAllListeners();
        btnConfirm.onClick.AddListener(() => {
            SaveCurrentSkillCountsToToken();
            ControllerRegister.Get<CharacterTokenController>().OnClickConfirm(clickedToken);
            skillSlotCollection.Refresh();
        });

        txtName.text = characterCardData.name;
        txtHp.text = characterCardData.hp.ToString();
        txtMp.text = characterCardData.mp.ToString();
        imgCharacter.sprite = sprite;

        var skillCardData = DataManager.Instance.dicSkillCardData;
        skillCardCounts = new int[btnCounts.Length];

        for (int i = 0; i < characterCardData.skills.Count; i++)
        {
            var skillId = characterCardData.skills[i];
            var skillData = skillCardData[skillId];
            if (skillData == null) continue;

            int index = i;

            //토큰에 저장된 스킬 매수 불러오기 (확정 상태가 아니어도 저장된 값 사용)
            int savedCount = clickedToken.GetSkillCount(skillId);
            skillCardCounts[index] = savedCount;

            txtNames[index].gameObject.SetActive(true);
            txtNames[index].text = skillData.name;

            txtCounts[index].gameObject.SetActive(true);
            txtCounts[index].text = savedCount.ToString();

            //버튼 초기화: 클릭 시 스킬 개수 증가 & 토큰에 반영
            btnCounts[index].onClick.AddListener(() =>
            {
                skillCardCounts[index] = (skillCardCounts[index] + 1) % (maxSkillCardCount + 1);
                txtCounts[index].text = skillCardCounts[index].ToString();

                if (clickedToken.State == CharacterTokenState.Confirm) clickedToken.SetSkillCount(skillId, skillCardCounts[index]);

                skillSlotCollection.Refresh();
            });

            btnSkills[index].onClick.AddListener(() => {
                UIManager.Instance.ShowPopup<UISkillInfoPopup>("UISkillInfoPopup", false).Init(skillData);
            });
        }
    }

    //현재 UI에 표시된 스킬 매수를 토큰에 저장
    private void SaveCurrentSkillCountsToToken()
    {
        if (curClickedToken == null) return;

        var characterCardData = DataManager.Instance.dicCharacterCardData[curClickedToken.Key];
        if (characterCardData == null) return;

        for (int i = 0; i < characterCardData.skills.Count && i < skillCardCounts.Length; i++)
        {
            var skillId = characterCardData.skills[i];
            curClickedToken.SetSkillCount(skillId, skillCardCounts[i]);
        }
    }

    private void ResetUI()
    {
        foreach (var txt in txtCounts)
        {
            txt.gameObject.SetActive(false);
            txt.text = "0";
        }

        foreach (var txt in txtNames)
        {
            txt.gameObject.SetActive(false);
            txt.text = string.Empty;
        }

        foreach (var btn in btnSkills) btn.onClick.RemoveAllListeners();

        foreach (var btn in btnCounts) btn.onClick.RemoveAllListeners();
    }
}
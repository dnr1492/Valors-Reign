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
        List<SkillCardData> skillDataList = new();
        skillCardCounts = new int[btnCounts.Length];

        for (int i = 0; i < characterCardData.skills.Count; i++)
        {
            var skillId = characterCardData.skills[i];
            var skillData = skillCardData[skillId];
            if (skillData == null) continue;
            skillDataList.Add(skillData);

            int index = i;
            int savedCount = clickedToken.GetSkillCount(skillId);  //토큰에 저장된 스킬 매수 불러오기 (확정 상태가 아니어도 저장된 값 사용)
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
        }

        //어떤 스킬 버튼을 클릭하든지 모든 스킬을 스킬 정보 팝업에 표시
        for (int i = 0; i < skillDataList.Count; i++)
        {
            int index = i;
            btnSkills[index].onClick.AddListener(() =>
            {
                UIManager.Instance.ShowPopup<UISkillInfoPopup>("UISkillInfoPopup", false)
                    .Init(skillDataList, this);
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

    //현재 UI에 표시된 스킬 매수를 가져오기
    public int GetSkillCount(int skillId)
    {
        var characterCardData = DataManager.Instance.dicCharacterCardData[curClickedToken.Key];
        if (characterCardData == null) return 0;

        for (int i = 0; i < characterCardData.skills.Count; i++)
        {
            if (characterCardData.skills[i] == skillId)
                return skillCardCounts[i];
        }

        return 0;
    }

    //스킬 매수를 외부에서 수동으로 반영
    public void SetSkillCountManually(int skillId, int count)
    {
        var characterCardData = DataManager.Instance.dicCharacterCardData[curClickedToken.Key];
        if (characterCardData == null) return;

        for (int i = 0; i < characterCardData.skills.Count; i++)
        {
            if (characterCardData.skills[i] == skillId)
            {
                //UI 업데이트
                skillCardCounts[i] = count;
                txtCounts[i].text = count.ToString();

                //Confirm 상태면 Token 및 Collection에도 반영
                if (curClickedToken.State == CharacterTokenState.Confirm)
                {
                    curClickedToken.SetSkillCount(skillId, count);
                    skillSlotCollection.Refresh();
                }

                return;
            }
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
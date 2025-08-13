using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static EnumClass;

public class CharacterCard : MonoBehaviour
{
    [SerializeField] SkillSlotCollection skillSlotCollection;
    [SerializeField] TextMeshProUGUI txtName;
    [SerializeField] TextMeshProUGUI txtHp, txtMp;
    [SerializeField] TextMeshProUGUI[] txtCounts, txtNames;
    [SerializeField] Button[] btnCounts, btnSkills;
    [SerializeField] Image imgCharacter;
    [SerializeField] Button btnConfirm;
    [SerializeField] GameObject cost;
    [SerializeField] TextMeshProUGUI txtCost;
    [SerializeField] GameObject front, back;

    private int[] skillCardCounts;
    private const int maxSkillCardCount = 4;  //최대 개수
    private CharacterToken curClickedToken;  //현재 선택된 토큰
    private readonly List<int> displayedSkillIds = new();  //UI에 표시된 스킬 id 목록

    public void SetCharacterCard(CharacterToken clickedToken, Sprite sprite, CharacterCardData characterCardData)
    {
        ResetUI();

        curClickedToken = clickedToken;
        curClickedToken.SetCardToFront();

        btnConfirm.onClick.RemoveAllListeners();
        btnConfirm.onClick.AddListener(() => {
            if (curClickedToken.State != CharacterTokenState.Confirm)
            {
                bool canConfirm = UIManager.Instance.GetPopup<UIEditorDeckPhase3>("UIEditorDeckPhase3").CheckCost(curClickedToken);
                if (!canConfirm) return;
            }
            SaveCurrentSkillCountsToToken();
            ControllerRegister.Get<CharacterTokenController>().OnClickConfirm(clickedToken);
            skillSlotCollection.Refresh();
        });

        txtName.text = characterCardData.name;
        txtHp.text = characterCardData.hp.ToString();
        txtMp.text = characterCardData.mp.ToString();
        imgCharacter.sprite = sprite;

        cost.SetActive(true);
        txtCost.text = curClickedToken.Cost.ToString();

        var skillCardData = DataManager.Instance.dicSkillCardData;
        List<SkillCardData> skillDataList = new();
        skillCardCounts = new int[maxSkillCardCount];

        //배열 길이 확인 (안전 체크)
        if (txtNames.Length < maxSkillCardCount || txtCounts.Length < maxSkillCardCount ||
            btnCounts.Length < maxSkillCardCount || btnSkills.Length < maxSkillCardCount)
        {
            Debug.Log("스킬 슬롯 UI 배열이 maxSkillCardCount보다 작습니다.");
            return;
        }

        //모든 슬롯을 초기 상태로 세팅 ("-")
        for (int i = 0; i < maxSkillCardCount; i++)
        {
            txtNames[i].gameObject.SetActive(true);
            txtNames[i].text = "-";

            txtCounts[i].gameObject.SetActive(true);
            txtCounts[i].text = "-";

            btnCounts[i].onClick.RemoveAllListeners();
            btnCounts[i].interactable = false;

            btnSkills[i].onClick.RemoveAllListeners();
            btnSkills[i].interactable = false;
        }

        //UI 슬롯 인덱스
        int uiIndex = 0;

        //캐릭터 스킬을 순회하며 이동카드 제외 후 UI 슬롯 채우기
        foreach (var skillId in characterCardData.skills)
        {
            if (skillId == 1000) continue;  //이동카드 스킵

            if (!skillCardData.TryGetValue(skillId, out SkillCardData skillData) || skillData == null)
                continue;

            displayedSkillIds.Add(skillId);
            skillDataList.Add(skillData);

            int savedCount = clickedToken.GetSkillCount(skillId);  //토큰에 저장된 스킬 매수 불러오기 (확정 상태가 아니어도 저장된 값 사용)
            skillCardCounts[uiIndex] = savedCount;

            txtNames[uiIndex].text = skillData.name;
            txtCounts[uiIndex].text = savedCount.ToString();

            //버튼 초기화: 클릭 시 스킬 개수 증가 & 토큰에 반영
            int capturedIndex = uiIndex;
            btnCounts[uiIndex].interactable = true;
            btnCounts[uiIndex].onClick.AddListener(() =>
            {
                skillCardCounts[capturedIndex] = (skillCardCounts[capturedIndex] + 1) % (maxSkillCardCount + 1);
                txtCounts[capturedIndex].text = skillCardCounts[capturedIndex].ToString();

                if (clickedToken.State == CharacterTokenState.Confirm)
                    clickedToken.SetSkillCount(skillId, skillCardCounts[capturedIndex]);

                skillSlotCollection.Refresh();
            });

            //어떤 스킬 버튼을 클릭하든지 모든 스킬을 스킬 정보 팝업에 표시
            btnSkills[uiIndex].interactable = true;
            btnSkills[uiIndex].onClick.AddListener(() =>
            {
                //방어적 코드로서 새(new) 리스트를 넘겨 참조 꼬임 방지
                UIManager.Instance.ShowPopup<UISkillInfoPopup>("UISkillInfoPopup", false)
                    .Init(new List<SkillCardData>(skillDataList), this);
            });

            uiIndex++;
            if (uiIndex >= maxSkillCardCount) break;  //슬롯 초과 방지
        }
    }

    //현재 UI에 표시된 스킬 매수를 토큰에 저장
    private void SaveCurrentSkillCountsToToken()
    {
        if (curClickedToken == null) return;

        for (int i = 0; i < displayedSkillIds.Count && i < skillCardCounts.Length; i++)
        {
            int skillId = displayedSkillIds[i];
            curClickedToken.SetSkillCount(skillId, skillCardCounts[i]);
        }
    }

    //현재 UI에 표시된 스킬 매수를 가져오기
    public int GetSkillCount(int skillId)
    {
        int idx = displayedSkillIds.IndexOf(skillId);
        if (idx >= 0 && idx < skillCardCounts.Length) return skillCardCounts[idx];
        return 0;
    }

    //스킬 매수를 외부에서 수동으로 반영
    public void SetSkillCountManually(int skillId, int count)
    {
        int idx = displayedSkillIds.IndexOf(skillId);
        if (idx < 0 || idx >= skillCardCounts.Length) return;

        //UI 업데이트
        skillCardCounts[idx] = count;
        txtCounts[idx].text = count.ToString();

        //Confirm 상태면 Token 및 Collection에도 반영
        if (curClickedToken.State == CharacterTokenState.Confirm)
        {
            curClickedToken.SetSkillCount(skillId, count);
            skillSlotCollection.Refresh();
        }
    }

    //카드 상태를 설정
    public void SetCardState(CardState curCardState)
    {
        front.SetActive(curCardState == CardState.Front);
        back.SetActive(curCardState == CardState.Back);
    }

    private void ResetUI()
    {
        foreach (var txt in txtCounts) txt.text = "-";
        foreach (var txt in txtNames) txt.text = "-";
        foreach (var btn in btnSkills) btn.onClick.RemoveAllListeners();
        foreach (var btn in btnCounts) btn.onClick.RemoveAllListeners();

        displayedSkillIds.Clear();
    }
}
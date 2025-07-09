using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static EnumClass;

public class UIEditorDeckPhase3 : UIPopupBase
{
    [SerializeField] Button btn_back, btn_filter, btn_save, btn_reset;
    [SerializeField] CharacterCard characterCard;
    [SerializeField] SkillSlotCollection skillSlotCollection; 

    [Header("Hex Grid")]
    [SerializeField] RectTransform hexParantRt /*map*/, battleFieldRt;
    [SerializeField] GameObject hexPrefab;  //육각형 모양의 이미지가 있는 UI 프리팹

    [Header("Cost")]
    [SerializeField] Slider sliCost;
    private int maxCost, sumCost;

    [Header("DeckName")]
    [SerializeField] TMP_InputField inputFieldDeckName;
    [SerializeField] Button btnDeckNameEditor;
    private string currentDeckName = "";

    private void Awake()
    {
        btn_back.onClick.AddListener(() => {
            UIManager.Instance.ShowPopup<UIEditorDeckPhase2>("UIEditorDeckPhase2");
        });

        btn_filter.onClick.AddListener(() => {
            UIManager.Instance.ShowPopup<UIFilterPopup>("UIFilterPopup", false);
        });

        btn_save.onClick.AddListener(OnClickSave);

        btn_reset.onClick.AddListener(() => {
            GridManager.Instance.ResetUIDeckPhase3(characterCard);
            skillSlotCollection.Refresh();
            ResetDeckName();
        });

        btnDeckNameEditor.onClick.AddListener(StartInputDeckName);
    }

    private void Start()
    {
        GridManager.Instance.CreateHexGrid(battleFieldRt, hexPrefab, hexParantRt);

        maxCost = DataManager.Instance.gamePlayData.maxCost;
        sliCost.value = 0;
        sliCost.maxValue = maxCost;

        //덱 이름 입력 필드 초기화
        if (inputFieldDeckName != null)
        {
            inputFieldDeckName.text = currentDeckName;
            inputFieldDeckName.interactable = false;
            inputFieldDeckName.onEndEdit.AddListener(EndInputDeckName);
        }
    }

    protected override void ResetUI()
    {
        GridManager.Instance.ResetUIDeckPhase3(characterCard);
        ResetDeckName();
        skillSlotCollection.Refresh();
    }

    public async void ApplyDeckPack(DeckPack deckPack)
    {
        await GridManager.Instance.ApplyDeckPack(deckPack, characterCard);

        currentDeckName = deckPack.deckName;
        inputFieldDeckName.text = currentDeckName;

        skillSlotCollection.Refresh();
    }

    public void SetMaxCost(int cost)
    {
        int newCost = sumCost + cost;
        if (newCost < 0 || newCost > maxCost) return;

        sumCost = newCost;
        sliCost.value = sumCost;
    }

    private void StartInputDeckName()
    {
        inputFieldDeckName.interactable = true;
        inputFieldDeckName.Select();
        inputFieldDeckName.ActivateInputField();
    }

    private void EndInputDeckName(string text)
    {
        inputFieldDeckName.interactable = false;

        string trimmed = text.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            UIManager.Instance.ShowPopup<UIModalPopup>("UIModalPopup", false)
                .Set("덱 이름 오류", "덱 이름을 입력해주세요.");
            inputFieldDeckName.text = currentDeckName;  //이전 이름 복구
            return;
        }

        currentDeckName = trimmed;
        inputFieldDeckName.text = currentDeckName;
    }

    /// <summary>
    /// 덱 이름 리셋
    /// </summary>
    private void ResetDeckName()
    {
        currentDeckName = "";
        if (inputFieldDeckName != null) inputFieldDeckName.text = "";
    }

    public void OnClickSave()
    {
        if (!CheckEnabledSave()) return;

        //DeckPack 생성 및 저장
        DeckPack deckPack = GridManager.Instance.CreateDeckPack(currentDeckName);
        DeckHandler.Save(deckPack);

        //UIEditorDeckPhase1에 DeckPack 전달
        var phase1Popup = UIManager.Instance.GetPopup<UIEditorDeckPhase1>("UIEditorDeckPhase1");
        phase1Popup.OnClickSave(deckPack);
        
        skillSlotCollection.Refresh();
        
        Debug.Log($"덱 저장 완료: {deckPack.deckName}");
    }

    /// <summary>
    /// 저장이 가능한 지 확인
    /// </summary>
    /// <returns></returns>
    private bool CheckEnabledSave()
    {
        var tokens = ControllerRegister.Get<CharacterTokenController>().GetAllCharacterToken();
        int confirmTokenCount = 0;
        int totalSkillCount = 0;
        foreach (var token in tokens)
        {
            if (token.State != CharacterTokenState.Confirm) continue;
            confirmTokenCount++;
            foreach (var count in token.GetAllSkillCounts().Values) totalSkillCount += count;
        }

        if (confirmTokenCount == 0)
        {
            UIManager.Instance.ShowPopup<UIModalPopup>("UIModalPopup", false)
                .Set("저장 오류", "하나 이상의 캐릭터를 배치해주세요.");
            return false;
        }

        int maxSkillCount = confirmTokenCount * 4;
        if (totalSkillCount > maxSkillCount) {
            UIManager.Instance.ShowPopup<UIModalPopup>("UIModalPopup", false)
                .Set("스킬 개수 초과", $"확정된 캐릭터 {confirmTokenCount}개 x 4 = {maxSkillCount}개 이하로 설정해주세요.");
            return false;
        }

        if (string.IsNullOrEmpty(currentDeckName))
        {
            UIManager.Instance.ShowPopup<UIModalPopup>("UIModalPopup", false)
                .Set("저장 오류", "덱 이름을 먼저 입력해주세요.");
            return false;
        }

        if (DeckHandler.Load(currentDeckName) != null)
        {
            UIManager.Instance.ShowPopup<UIModalPopup>("UIModalPopup", false)
                .Set("중복된 덱 이름", "이미 존재하는 덱 이름입니다. 다른 이름을 입력해주세요.");
            return false;
        }

        return true;
    }
}


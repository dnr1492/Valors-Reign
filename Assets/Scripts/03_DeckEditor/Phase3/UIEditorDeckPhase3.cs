using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static EnumClass;
using Cysharp.Threading.Tasks;

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
    private string previousDeckName = "";
    private bool isNewSave = true;

    [Header("Deck Cancel")]
    [SerializeField] GameObject cancelZoneObj;

    private void Awake()
    {
        btn_back.onClick.AddListener(() => {
            UIManager.Instance.ShowPopup<UIEditorDeckPhase2>("UIEditorDeckPhase2");
        });

        btn_filter.onClick.AddListener(() => {
            UIManager.Instance.ShowPopup<UIFilterPopup>("UIFilterPopup", false);
        });

        btn_save.onClick.AddListener(() => { UniTask.Void(OnClickSaveAsync); });

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

    public void SetCost(int cost)
    {
        int newCost = sumCost + cost;
        if (newCost < 0 || newCost > maxCost) return;

        sumCost = newCost;
        sliCost.value = sumCost;
    }

    public bool CheckCost(CharacterToken clickedToken)
    {
        int clickedTokenCost = clickedToken.Tier != CharacterTierAndCost.Boss ? (int)clickedToken.Tier : 0;

        if (sumCost + clickedTokenCost > maxCost) {
            UIManager.Instance.ShowPopup<UIModalPopup>("UIModalPopup", false)
                .Set("코스트 초과", $"현재 코스트: {sumCost} + {clickedTokenCost} > 최대 코스트 {maxCost}");
            return false;
        }

        return true;
    }

    private void StartInputDeckName()
    {
        previousDeckName = currentDeckName;

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
            inputFieldDeckName.text = previousDeckName;
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

    private async UniTaskVoid OnClickSaveAsync()
    {
        if (!await CheckEnabledSaveAsync()) return;

        //DeckPack 생성 및 저장
        DeckPack deckPack = GridManager.Instance.CreateDeckPack(currentDeckName);
        BackendManager.Instance.SaveDeck(deckPack, isNewSave);  //서버 저장
        //DeckHandler.Save(deckPack);  //로컬 저장

        //UIEditorDeckPhase1에 DeckPack 전달
        var phase1Popup = UIManager.Instance.GetPopup<UIEditorDeckPhase1>("UIEditorDeckPhase1");
        phase1Popup.OnClickSave(deckPack);
        
        skillSlotCollection.Refresh();

        UIManager.Instance.ShowPopup<UIModalPopup>("UIModalPopup", false)
                .Set("덱 저장 완료", $"덱 이름: {deckPack.deckName}");
    }

    /// <summary>
    /// 저장이 가능한 지 확인
    /// </summary>
    /// <returns></returns>
    private async UniTask<bool> CheckEnabledSaveAsync()
    {
        var tokens = ControllerRegister.Get<CharacterTokenController>().GetAllCharacterToken();
        int confirmTokenCount = 0;
        int bossCount = 0;
        List<string> invalidCharacters = new();
        foreach (var token in tokens)
        {
            if (token.State != CharacterTokenState.Confirm) continue;
            confirmTokenCount++;

            if (token.Tier == CharacterTierAndCost.Boss) bossCount++;

            int skillCount = 0;
            foreach (var count in token.GetAllSkillCounts().Values) skillCount += count;

            if (skillCount != 4) {
                string charName = DataManager.Instance.dicCharacterCardData.TryGetValue(token.Key, out var cardData)
                    ? cardData.name : $"ID: {token.Key}";
                invalidCharacters.Add($"{charName} ({skillCount}개)");
            }
        }

        if (bossCount == 0)
        {
            UIManager.Instance.ShowPopup<UIModalPopup>("UIModalPopup", false)
                .Set("보스 캐릭터 누락", "덱에는 반드시 Boss 티어 캐릭터가 1명 포함되어야 합니다.");
            return false;
        }
        else if (bossCount > 1)
        {
            UIManager.Instance.ShowPopup<UIModalPopup>("UIModalPopup", false)
                .Set("보스 캐릭터 중복", $"Boss 티어 캐릭터가 {bossCount}명 배치되어 있습니다.\nBoss는 1명만 포함해야 합니다.");
            return false;
        }

        if (invalidCharacters.Count > 0)
        {
            string joined = string.Join("\n", invalidCharacters);
            UIManager.Instance.ShowPopup<UIModalPopup>("UIModalPopup", false)
                .Set("스킬 설정 오류", $"다음 캐릭터들의 스킬 수가 4개가 아닙니다:\n{joined}");
            return false;
        }

        if (confirmTokenCount == 0)
        {
            UIManager.Instance.ShowPopup<UIModalPopup>("UIModalPopup", false)
                .Set("저장 오류", "하나 이상의 캐릭터를 배치해주세요.");
            return false;
        }

        if (string.IsNullOrEmpty(currentDeckName))
        {
            UIManager.Instance.ShowPopup<UIModalPopup>("UIModalPopup", false)
                .Set("저장 오류", "덱 이름을 먼저 입력해주세요.");
            return false;
        }

        //서버에서 모든 덱 불러오기
        //덱 이름 중복 검사 + 현재 편집 중인 덱은 예외
        var phase1Popup = UIManager.Instance.GetPopup<UIEditorDeckPhase1>("UIEditorDeckPhase1");
        var currentDeckPack = phase1Popup?.GetCurrentDeckPack();
        string currentGuid = currentDeckPack?.guid;
        var allDecks = await BackendManager.Instance.LoadAllDecksAsync();
        foreach (var (guid, pack) in allDecks)
        {
            if (pack.deckName == currentDeckName && guid != currentGuid)
            {
                UIManager.Instance.ShowPopup<UIModalPopup>("UIModalPopup", false)
                    .Set("중복된 덱 이름", "이미 존재하는 덱 이름입니다. 다른 이름을 입력해주세요.");

                inputFieldDeckName.text = previousDeckName;
                currentDeckName = previousDeckName;
                inputFieldDeckName.DeactivateInputField();
                inputFieldDeckName.interactable = false;
                return false;
            }
        }
        isNewSave = string.IsNullOrEmpty(currentGuid);

        ////로컬에서 모든 덱 불러오기
        //var allDecks = DeckHandler.LoadAll();
        //foreach (var (guid, pack) in allDecks)
        //{
        //    if (pack.deckName == currentDeckName)
        //    {
        //        //같은 이름의 다른 덱이 이미 존재한다면
        //        var phase1Popup = UIManager.Instance.GetPopup<UIEditorDeckPhase1>("UIEditorDeckPhase1");
        //        var currentDeckPack = phase1Popup?.GetCurrentDeckPack();
        //        if (currentDeckPack == null || currentDeckPack.guid != guid)
        //        {
        //            UIManager.Instance.ShowPopup<UIModalPopup>("UIModalPopup", false)
        //                .Set("중복된 덱 이름", "이미 존재하는 덱 이름입니다. 다른 이름을 입력해주세요.");

        //            //기존 설정된 덱 이름으로 되돌리기
        //            inputFieldDeckName.text = previousDeckName;
        //            currentDeckName = previousDeckName;
        //            inputFieldDeckName.DeactivateInputField();
        //            inputFieldDeckName.interactable = false;

        //            return false;
        //        }
        //    }
        //}

        return true;
    }

    public GameObject GetCancelZone() => cancelZoneObj;
}


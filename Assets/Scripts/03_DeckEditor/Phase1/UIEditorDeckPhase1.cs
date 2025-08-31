using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIEditorDeckPhase1 : UIPopupBase
{
    [SerializeField] Button btn_back;
    [SerializeField] GameObject deckPrefab;
    [SerializeField] Transform deckContainer;

    private Deck selectedDeck = null;  //현재 선택된 덱
    private bool isEditMode = true;
    private GameObject newDeckSlotObj;  //새 덱 슬롯

    public Action onApplyBattleDeck;

    private void Awake()
    {
        btn_back.onClick.AddListener(OnClickBack);
    }

    private void OnClickBack()
    {
        UIManager.Instance.ShowPopup<UILobbyPopup>("UILobbyPopup");
    }

    public void LoadSavedDecks()
    {
        //기존 슬롯 전부 제거 (이벤트 누적 방지)
        ClearDeckContainerImmediate();

        selectedDeck = null;
        newDeckSlotObj = null;

        //저장된 덱 슬롯 생성
        foreach (var (guid, pack) in BackendManager.Instance.GetSortedDecks())
        {
            GameObject obj = Instantiate(deckPrefab, deckContainer);
            Deck deck = obj.GetComponent<Deck>();
            deck.InitSavedDeck(pack);
            deck.OnApplyDeck += OnApplyDeck;
        }

        //새 덱 슬롯 보장 및 표시 상태 반영
        EnsureNewDeckSlot();
        ActiveNewDeckSlot();
    }

    /// <summary>
    /// 새 덱 슬롯이 없는 경우에만 하나 생성
    /// </summary>
    private void EnsureNewDeckSlot()
    {
        bool hasNewSlot = false;

        foreach (Transform child in deckContainer)
        {
            Deck deck = child.GetComponent<Deck>();
            if (deck != null && deck.GetDeckPack() == null) {
                hasNewSlot = true;
                break;
            }
        }

        if (!hasNewSlot) CreateNewDeck();
    }

    private void CreateNewDeck()
    {
        GameObject obj = Instantiate(deckPrefab, deckContainer);
        Deck deck = obj.GetComponent<Deck>();
        deck.InitNewDeck();
        deck.SetOnCreateNewDeck(() => {
            //빈 DeckPack 생성
            DeckPack emptyDeckPack = new DeckPack {
                deckName = "",
                guid = Guid.NewGuid().ToString(),
                tokenSlots = new List<TokenSlotData>()
            };

            var popup = UIManager.Instance.GetPopup<UIEditorDeckPhase3>("UIEditorDeckPhase3");
            popup.ApplyDeckPack(emptyDeckPack);

            selectedDeck = deck;

            UIManager.Instance.ShowPopup<UIEditorDeckPhase2>("UIEditorDeckPhase2");
        });

        newDeckSlotObj = obj;
    }

    public void OnClickSave(DeckPack deckPack)
    {
        if (selectedDeck != null)
        {
            selectedDeck.InitSavedDeck(deckPack);
            selectedDeck.OnApplyDeck += OnApplyDeck;

            EnsureNewDeckSlot();  //새 덱 슬롯이 없으면 하나 추가
        }
    }

    private void OnApplyDeck(Deck deck)
    {
        if (deck == null || deck.GetDeckPack() == null) return;

        selectedDeck = deck;

        if (isEditMode) {
            var popup = UIManager.Instance.ShowPopup<UIEditorDeckPhase3>("UIEditorDeckPhase3");
            popup.ApplyDeckPack(GetSelectedDeckPack());
        }
        else {
            onApplyBattleDeck?.Invoke();
            UIManager.Instance.ShowPopup<UIBattleReady>("UIBattleReady");
        }
    }

    public DeckPack GetSelectedDeckPack() => selectedDeck.GetDeckPack();

    public void SetEditMode(bool edit)
    {
        isEditMode = edit;

        ActiveNewDeckSlot();
    }

    /// <summary>
    /// 새 덱 슬롯의 활성화 또는 비활성화
    /// </summary>
    private void ActiveNewDeckSlot()
    {
        if (newDeckSlotObj != null)
            newDeckSlotObj.SetActive(isEditMode);
    }

    /// <summary>
    /// 기존 덱 전부 제거 (이벤트 누적 방지)
    /// </summary>
    private void ClearDeckContainerImmediate()
    {
        for (int i = deckContainer.childCount - 1; i >= 0; --i)
        {
            var child = deckContainer.GetChild(i) as RectTransform;
            if (!child) continue;

            //Destroy()가 프레임 끝에 호출되므로 deckContainer의 계층에서 분리시켜서 해결
            child.SetParent(null, false);
            Destroy(child.gameObject);
        }
    }

    protected override void ResetUI() { }
}

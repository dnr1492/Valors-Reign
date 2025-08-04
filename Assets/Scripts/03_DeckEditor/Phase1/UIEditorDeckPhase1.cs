using System;
using UnityEngine;
using UnityEngine.UI;

public class UIEditorDeckPhase1 : UIPopupBase
{
    [SerializeField] Button btn_back;
    [SerializeField] GameObject deckPrefab;
    [SerializeField] Transform deckContainer;

    private Deck currentDeck = null;  //현재 덱
    private bool isEditMode = true;
    private GameObject newDeckSlotObj;  //새 덱 슬롯

    public Action onApplyBattleDeck;

    private void Awake()
    {
        btn_back.onClick.AddListener(OnClickBack);
    }

    private void Start()
    {
        LoadSavedDecks();  //최초 저장된 덱들을 모두 생성
    }

    private void OnClickBack()
    {
        UIManager.Instance.ShowPopup<UILobbyPopup>("UILobbyPopup");
    }

    private void LoadSavedDecks()
    {
        foreach (var (guid, pack) in BackendManager.Instance.GetSortedDecks())
        {
            GameObject obj = Instantiate(deckPrefab, deckContainer);
            Deck deck = obj.GetComponent<Deck>();
            deck.InitSavedDeck(pack);
            deck.OnApplyDeck += OnApplyDeck;
        }

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
            currentDeck = deck;
            UIManager.Instance.ShowPopup<UIEditorDeckPhase2>("UIEditorDeckPhase2");
        });

        newDeckSlotObj = obj;
    }

    public void OnClickSave(DeckPack deckPack)
    {
        if (currentDeck != null)
        {
            currentDeck.InitSavedDeck(deckPack);
            currentDeck.OnApplyDeck += OnApplyDeck;

            EnsureNewDeckSlot();  //새 덱 슬롯이 없으면 하나 추가
        }
    }

    private void OnApplyDeck(Deck deck)
    {
        if (deck == null || deck.GetDeckPack() == null) return;

        currentDeck = deck;

        if (isEditMode) {
            var popup = UIManager.Instance.ShowPopup<UIEditorDeckPhase3>("UIEditorDeckPhase3");
            popup.ApplyDeckPack(GetCurrentDeckPack());
        }
        else {
            onApplyBattleDeck?.Invoke();
            UIManager.Instance.ShowPopup<UIBattleReady>("UIBattleReady");
        }
    }

    public DeckPack GetCurrentDeckPack() => currentDeck.GetDeckPack();

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

    protected override void ResetUI() { }
}

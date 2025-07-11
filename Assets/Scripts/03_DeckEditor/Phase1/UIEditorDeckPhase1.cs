using UnityEngine;
using UnityEngine.UI;

public class UIEditorDeckPhase1 : UIPopupBase
{
    [SerializeField] Button btn_back;
    [SerializeField] GameObject deckPrefab;
    [SerializeField] Transform deckContainer;

    private Deck currentDeck = null;  //현재 작업 중인 덱

    private void Awake()
    {
        btn_back.onClick.AddListener(OnClickBack);
    }

    private void Start()
    {
        LoadSavedDecks();  //저장된 덱들을 모두 생성
        EnsureNewDeckSlot();
    }

    private void OnClickBack()
    {
        UIManager.Instance.ShowPopup<UILoginPopup>("UILoginPopup");
    }

    private void LoadSavedDecks()
    {
        ////로컬에서 모든 덱 불러오기
        //foreach (var (_, pack) in DeckHandler.LoadAll())
        //{
        //    GameObject obj = Instantiate(deckPrefab, deckContainer);
        //    Deck deck = obj.GetComponent<Deck>();
        //    deck.InitSavedDeck(pack);
        //    deck.OnApplyDeck += OnApplyDeck;
        //}

        //서버에서 모든 덱 불러오기
        BackendManager.Instance.LoadAllDecks(result =>
        {
            foreach (var (_, pack) in result)
            {
                GameObject obj = Instantiate(deckPrefab, deckContainer);
                Deck deck = obj.GetComponent<Deck>();
                deck.InitSavedDeck(pack);
                deck.OnApplyDeck += OnApplyDeck;
            }
        });
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
    }

    public void OnClickSave(DeckPack deckPack)
    {
        if (currentDeck != null)
        {
            currentDeck.InitSavedDeck(deckPack);
            currentDeck.OnApplyDeck += OnApplyDeck;
            currentDeck = null;

            EnsureNewDeckSlot();  //새 덱 슬롯이 없으면 하나 추가
        }
    }

    private void OnApplyDeck(Deck deck)
    {
        if (deck?.GetDeckPack() != null) {
            currentDeck = deck;
            var popup = UIManager.Instance.ShowPopup<UIEditorDeckPhase3>("UIEditorDeckPhase3");
            popup.ApplyDeckPack(deck.GetDeckPack());
        }
    }

    public DeckPack GetCurrentDeckPack() => currentDeck?.GetDeckPack();

    protected override void ResetUI() { }
}

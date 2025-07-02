using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class Deck : MonoBehaviour
{
    [SerializeField] Image icon;
    [SerializeField] TextMeshProUGUI txt_deckName;
    [SerializeField] Button btn_deck;

    private DeckPack deckPack = null;  //덱 정보 저장
    private Action OnCreateNewDeck;
    public event Action<Deck> OnApplyDeck;  //event: 캡슐화

    private void Awake()
    {
        btn_deck.onClick.AddListener(() =>
        {
            if (deckPack == null) OnCreateNewDeck?.Invoke();  //새 덱 슬롯 클릭
            else OnApplyDeck?.Invoke(this);                 //저장된 덱 클릭
        });
    }

    /// <summary>
    /// 덱 저장 전
    /// </summary>
    public void InitNewDeck()
    {
        deckPack = null;
        icon.gameObject.SetActive(true);
        txt_deckName.gameObject.SetActive(false);
    }

    /// <summary>
    /// 덱 저장 후
    /// </summary>
    /// <param name="pack"></param>
    public void InitSavedDeck(DeckPack pack)
    {
        deckPack = pack;
        icon.gameObject.SetActive(false);
        txt_deckName.gameObject.SetActive(true);
        txt_deckName.text = deckPack.deckName;
    }

    public void SetOnCreateNewDeck(Action callback) => OnCreateNewDeck = callback;

    public DeckPack GetDeckPack() => deckPack;
}

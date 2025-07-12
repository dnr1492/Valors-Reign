using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UILobbyPopup : UIPopupBase
{
    [SerializeField] Button btn_battle, btn_editorDeck;

    private void Awake()
    {
        btn_battle.onClick.AddListener(OnClickBattle);
        btn_editorDeck.onClick.AddListener(OnClickEditorDeck);
    }

    private void OnClickBattle()
    {
        Debug.Log("===== Battle ¹Ì±¸Çö =====");
    }

    private void OnClickEditorDeck()
    {
        UIManager.Instance.ShowPopup<UIEditorDeckPhase1>("UIEditorDeckPhase1");
    }

    protected override void ResetUI() { }
}

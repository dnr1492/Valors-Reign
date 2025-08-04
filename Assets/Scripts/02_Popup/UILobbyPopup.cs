using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UILobbyPopup : UIPopupBase
{
    [SerializeField] Button btn_battle, btn_editorDeck;
    [SerializeField] Button btn_logout;
    [SerializeField] TextMeshProUGUI txt_userName;

    private void Awake()
    {
        btn_battle.onClick.AddListener(OnClickBattle);
        btn_editorDeck.onClick.AddListener(OnClickEditorDeck);
        btn_logout.onClick.AddListener(UIManager.Instance.GetPopup<UILoginPopup>("UILoginPopup").OnClickLogout);
    }

    public void Init()
    {
        txt_userName.text = BackendManager.Instance.GetNickname();

        BackendManager.Instance.LoadAllDecks();
    }

    public void OnClickBattle()
    {
        UIManager.Instance.ShowPopup<UIBattleReady>("UIBattleReady");
    }

    private void OnClickEditorDeck()
    {
        UIEditorDeckPhase1 popup = UIManager.Instance.ShowPopup<UIEditorDeckPhase1>("UIEditorDeckPhase1");
        popup.SetEditMode(true);
    }

    protected override void ResetUI() { }
}

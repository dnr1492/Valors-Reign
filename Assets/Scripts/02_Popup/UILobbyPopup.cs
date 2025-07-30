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

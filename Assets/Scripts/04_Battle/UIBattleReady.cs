using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIBattleReady : UIPopupBase
{
    [SerializeField] Button btn_back;
    [SerializeField] Button btn_startMatching;
    [SerializeField] Button btn_battleDeck;

    private void Awake()
    {
        btn_back.onClick.AddListener(OnClickBack);
        btn_startMatching.onClick.AddListener(OnClickStartMatching);
        btn_battleDeck.onClick.AddListener(OnClickSelectBattleDeck);
    }

    private void OnClickBack()
    {
        UIManager.Instance.ShowPopup<UILobbyPopup>("UILobbyPopup");
    }

    private void OnClickStartMatching()
    {
        LoadingManager.Instance.Show("상대방 입장 대기 중...", OnClickCancelMatching);

        var photon = ControllerRegister.Get<PhotonController>();
        if (!PhotonController.IsConnected) photon.RequestJoinRoomAfterConnection();
        else photon.JoinOrCreateRoom();
    }

    private void OnClickCancelMatching()
    {
        LoadingManager.Instance.Hide();

        var photon = ControllerRegister.Get<PhotonController>();
        photon.LeaveRoom();
    }

    private void OnClickSelectBattleDeck()
    {
        UIEditorDeckPhase1 popup = UIManager.Instance.ShowPopup<UIEditorDeckPhase1>("UIEditorDeckPhase1");
        popup.SetEditMode(false);
    }

    protected override void ResetUI() { }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static EnumClass;
using Cysharp.Threading.Tasks;

public class UILobbyPopup : UIPopupBase
{
    [SerializeField] Button btn_battle, btn_editorDeck;
    [SerializeField] Button btn_logout;
    [SerializeField] Button btn_toGoogle;
    [SerializeField] TextMeshProUGUI txt_userName;

    public void Init()
    {
        btn_battle.onClick.RemoveAllListeners();
        btn_editorDeck.onClick.RemoveAllListeners();
        btn_logout.onClick.RemoveAllListeners();
        btn_toGoogle.onClick.RemoveAllListeners();

        btn_battle.onClick.AddListener(OnClickBattle);
        btn_editorDeck.onClick.AddListener(OnClickEditorDeck);
        btn_logout.onClick.AddListener(UIManager.Instance.GetPopup<UILoginPopup>("UILoginPopup").OnClickLogout);

        if (BackendManager.Instance.GetLoginType() == "guest")
        {
            btn_toGoogle.gameObject.SetActive(true);
            btn_toGoogle.onClick.AddListener(OnClickUpgradeToGoogle);
        }
        else btn_toGoogle.gameObject.SetActive(false);

        txt_userName.text = BackendManager.Instance.GetNickname();

        BackendManager.Instance.LoadAllDecks(_ =>
        {
            var phase1 = UIManager.Instance.GetPopup<UIEditorDeckPhase1>("UIEditorDeckPhase1");
            if (phase1 != null) phase1.LoadSavedDecks();
            Debug.Log("[덱 로드 완료] 현재 계정의 덱으로 UI 리프레시");
        });
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

//    private void OnClickUpgradeToGoogle()
//    {
//#if UNITY_ANDROID
//        LoadingManager.Instance.Show("구글 계정 연동 중...");
//        TheBackend.ToolKit.GoogleLogin.Android.GoogleLogin((ok, msg, token) =>
//        {
//            if (!ok)
//            {
//                LoadingManager.Instance.Hide();
//                ToastManager.Instance.Show("구글 로그인 실패", ToastAnchor.Top);
//                return;
//            }

//            bool done = BackendManager.Instance.TryUpgradeGuestToGoogle(token);
//            LoadingManager.Instance.Hide();
//            if (done) Init();
//        });
//#else
//    ToastManager.Instance.Show("Android에서만 지원됩니다.", ToastAnchor.Top);
//#endif
//    }

    protected override void ResetUI() { }

    // ====================================== 구현 중 =========================================== //
    // ====================================== 구현 중 =========================================== //
    // ====================================== 구현 중 =========================================== //

    private void OnClickUpgradeToGoogle()
    {
//#if UNITY_ANDROID
//        LoadingManager.Instance.Show("구글 계정 연동 중...");
//        var config = new PlayGamesClientConfiguration.Builder()
//            .RequestEmail()
//            .RequestIdToken(BackendManager.WEB_CLIENT_ID)
//            .Build();
//        PlayGamesPlatform.InitializeInstance(config);
//        PlayGamesPlatform.Activate();

//        Social.localUser.Authenticate(success =>
//        {
//            if (!success)
//            {
//                LoadingManager.Instance.Hide();
//                ToastManager.Instance.Show("구글 로그인 실패", ToastAnchor.Top);
//                return;
//            }

//            string idToken = PlayGamesPlatform.Instance.GetIdToken();
//            if (string.IsNullOrEmpty(idToken))
//            {
//                LoadingManager.Instance.Hide();
//                ToastManager.Instance.Show("구글 토큰 획득 실패", ToastAnchor.Top);
//                return;
//            }

//            bool done = BackendManager.Instance.TryUpgradeGuestToGoogle(idToken);
//            LoadingManager.Instance.Hide();
//            if (done) Init();
//        });
//#else
//    ToastManager.Instance.Show("Android에서만 지원됩니다.", ToastAnchor.Top);
//#endif
    }
}

using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UILoginPopup : UIPopupBase
{
    [SerializeField] Button btn_retry, btn_loginGuest, btn_loginGoogle;

    private void Start()
    {
        btn_retry.gameObject.SetActive(false);
        btn_loginGuest.gameObject.SetActive(false);
        btn_loginGoogle.gameObject.SetActive(false);

        LodingManager.Instance.Show("서버에 연결 중입니다...");

        ConnectNetwork();
    }

    private async void ConnectNetwork()
    {
        const int maxRetry = 3;
        bool success = false;

        for (int i = 0; i < maxRetry; i++)
        {
            success = await BackendManager.Instance.InitBackendAsync();
            if (success) break;
            await UniTask.Delay(2000);
        }

        LodingManager.Instance.Hide();

        if (success)
        {
            btn_loginGuest.gameObject.SetActive(true);
            btn_loginGuest.onClick.RemoveAllListeners();
            btn_loginGuest.onClick.AddListener(OnClickLoginGuest);

            btn_loginGoogle.gameObject.SetActive(true);
            btn_loginGoogle.onClick.RemoveAllListeners();
            btn_loginGoogle.onClick.AddListener(OnClickLoginGoogle);
        }
        else
        {
            btn_retry.gameObject.SetActive(true);
            btn_retry.onClick.RemoveAllListeners();
            btn_retry.onClick.AddListener(() =>
            {
                LodingManager.Instance.Show("서버에 연결 중입니다...");
                btn_retry.gameObject.SetActive(false);
                ConnectNetwork();
            });
        }
    }

    private void OnClickLoginGuest()
    {
        BackendManager.Instance.OnLoginCompleteCallback = () =>
        {
            UIManager.Instance.ShowPopup<UILobbyPopup>("UILobbyPopup").Init();
        };

        BackendManager.Instance.LoginGuest(err =>
        {
            Debug.Log($"게스트 로그인 실패: {err}");
        });
    }

    private void OnClickLoginGoogle()
    {
        BackendManager.Instance.OnLoginCompleteCallback = () =>
        {
            UIManager.Instance.ShowPopup<UILobbyPopup>("UILobbyPopup").Init();
        };

        TheBackend.ToolKit.GoogleLogin.Android.GoogleLogin(BackendManager.Instance.LoginGoogle);
    }

    private void OnLoginComplete()
    {
        UIManager.Instance.ShowPopup<UILobbyPopup>("UILobbyPopup").Init();
    }

    protected override void ResetUI() { }
}
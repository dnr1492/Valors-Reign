using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class UILoginPopup : UIPopupBase
{
    [SerializeField] Button btn_loginGuest, btn_retry;

    private void Start()
    {
        btn_loginGuest.gameObject.SetActive(false);
        btn_retry.gameObject.SetActive(false);

        LodingManager.Instance.Show("서버에 연결 중입니다...");

        ConnectNetwork();
    }

    private void OnClickLoginGuest()
    {
        BackendManager.Instance.LoginGuest(
            onSuccess: () =>
            {
                string nickname = BackendManager.Instance.GetNickname();
                if (string.IsNullOrEmpty(nickname))
                {
                    //자동 닉네임 생성
                    string guestNickname = $"게스트{Random.Range(1000, 9999)}";
                    BackendManager.Instance.SetNickname(guestNickname,
                        onSuccess: () => {
                            Debug.Log($"닉네임 자동 설정: {guestNickname}");
                            OnLoginComplete();
                        },
                        onFail: err => {
                            Debug.LogError($"닉네임 설정 실패: {err}");
                        });
                }
                else OnLoginComplete();
            },
            onFail: err => Debug.LogError($"게스트 로그인 실패: {err}")
        );
    }

    private void OnLoginComplete()
    {
        // ===== 임시로 덱 편집으로 진입 중. 추후 로비 화면으로 진입으로 변경하기 ===== //
        UIManager.Instance.ShowPopup<UIEditorDeckPhase1>("UIEditorDeckPhase1");
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

    protected override void ResetUI() { }
}
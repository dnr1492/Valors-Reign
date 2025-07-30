using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class UILoginPopup : UIPopupBase
{
    [SerializeField] Button btn_retry, btn_loginGuest, btn_loginGoogle;

    private bool isLoggingOut = false;  //로그아웃 클릭 연타 방지

    private void Start()
    {
        btn_retry.gameObject.SetActive(false);
        btn_loginGuest.gameObject.SetActive(false);
        btn_loginGoogle.gameObject.SetActive(false);

        LodingManager.Instance.Show("서버에 연결 중입니다...");
        ConnectNetwork().Forget();
    }

    private async UniTaskVoid ConnectNetwork()
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

        if (!success)
        {
            btn_retry.gameObject.SetActive(true);
            btn_retry.onClick.RemoveAllListeners();
            btn_retry.onClick.AddListener(() =>
            {
                LodingManager.Instance.Show("서버에 연결 중입니다...");
                btn_retry.gameObject.SetActive(false);
                ConnectNetwork().Forget();
            });
            return;
        }

        string loginType = BackendManager.Instance.GetLoginType();
        bool googleAuto = BackendManager.Instance.IsGoogleAutoLogin();

        if (loginType == "guest") TryAutoGuestLogin();
        else if (loginType == "google" && googleAuto) TryAutoGoogleLogin();
        else ShowLoginButtons();
    }

    private void TryAutoGuestLogin()
    {
        HideLoginButtons();
        LodingManager.Instance.Show("게스트 자동 로그인 중...");

        BackendManager.Instance.OnLoginCompleteCallback = HandleLoginSuccess;

        BackendManager.Instance.LoginGuest(err =>
        {
            LodingManager.Instance.Hide();
            Debug.Log($"게스트 자동 로그인 실패: {err}");
            ShowLoginButtons();
        });
    }

    private void TryAutoGoogleLogin()
    {
        HideLoginButtons();
        LodingManager.Instance.Show("구글 자동 로그인 중...");

        BackendManager.Instance.OnLoginCompleteCallback = HandleLoginSuccess;

        TheBackend.ToolKit.GoogleLogin.Android.GoogleLogin((isSuccess, errMsg, token) =>
        {
            BackendManager.Instance.LoginGoogle(isSuccess, errMsg, token);
        });
    }

    private void OnClickGuestLogin()
    {
        HideLoginButtons();
        LodingManager.Instance.Show("게스트 로그인 중...");

        BackendManager.Instance.OnLoginCompleteCallback = () =>
        {
            //게스트 로그인 시 구글 자동 로그인 비활성화
            BackendManager.Instance.SetLoginType("guest");
            BackendManager.Instance.SetAutoGoogleLogin(false);
            HandleLoginSuccess();
        };

        BackendManager.Instance.LoginGuest(err =>
        {
            LodingManager.Instance.Hide();
            Debug.Log($"게스트 로그인 실패: {err}");
            ShowLoginButtons();
        });
    }

    private void OnClickGoogleLogin()
    {
        HideLoginButtons();
        LodingManager.Instance.Show("구글 로그인 중...");

        BackendManager.Instance.OnLoginCompleteCallback = () =>
        {
            BackendManager.Instance.SetLoginType("google");
            BackendManager.Instance.SetAutoGoogleLogin(true);
            HandleLoginSuccess();
        };

        TheBackend.ToolKit.GoogleLogin.Android.GoogleLogin((isSuccess, errMsg, token) =>
        {
            BackendManager.Instance.LoginGoogle(isSuccess, errMsg, token);
        });
    }

    public void OnClickLogout()
    {
        if (isLoggingOut) return;
        isLoggingOut = true;

        HideLoginButtons();

        UIManager.Instance.ShowPopup<UIModalPopup>("UIModalPopup", false)
            .Set("로그아웃 확인", "정말 로그아웃하시겠습니까?", () =>
            {
                LodingManager.Instance.Show("로그아웃 중입니다...");

                BackendManager.Instance.Logout(() =>
                {
                    isLoggingOut = false;

                    LodingManager.Instance.Hide();
                    UIManager.Instance.ShowPopup<UILoginPopup>("UILoginPopup");
                    ShowLoginButtons();
                });
            });
    }

    private void HandleLoginSuccess()
    {
        LodingManager.Instance.Hide();
        UIManager.Instance.ShowPopup<UILobbyPopup>("UILobbyPopup").Init();
    }

    public void ShowLoginButtons()
    {
        btn_loginGuest.gameObject.SetActive(true);
        btn_loginGoogle.gameObject.SetActive(true);

        btn_loginGuest.onClick.RemoveAllListeners();
        btn_loginGoogle.onClick.RemoveAllListeners();

        btn_loginGuest.onClick.AddListener(OnClickGuestLogin);
        btn_loginGoogle.onClick.AddListener(OnClickGoogleLogin);
    }

    private void HideLoginButtons()
    {
        btn_loginGuest.gameObject.SetActive(false);
        btn_loginGoogle.gameObject.SetActive(false);

        btn_loginGuest.onClick.RemoveAllListeners();
        btn_loginGoogle.onClick.RemoveAllListeners();
    }

    protected override void ResetUI() { }
}
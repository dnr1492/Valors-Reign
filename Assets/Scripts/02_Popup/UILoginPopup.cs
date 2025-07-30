using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class UILoginPopup : UIPopupBase
{
    [SerializeField] Button btn_retry, btn_loginGuest, btn_loginGoogle;

    private void Awake()
    {
        btn_loginGuest.onClick.AddListener(() => TryLogin("guest", false));
        btn_loginGoogle.onClick.AddListener(() => TryLogin("google", false));
    }

    private void Start()
    {
        btn_retry.gameObject.SetActive(false);
        btn_loginGuest.gameObject.SetActive(false);
        btn_loginGoogle.gameObject.SetActive(false);
    }

    public async UniTask ConnectNetwork()
    {
        LoadingManager.Instance.Show("서버에 연결 중입니다...");

        const int maxRetry = 3;
        bool success = false;

        for (int i = 0; i < maxRetry; i++)
        {
            success = await BackendManager.Instance.InitBackendAsync();
            if (success) break;
            await UniTask.Delay(2000);
        }

        LoadingManager.Instance.Hide();

        if (!success)
        {
            btn_retry.gameObject.SetActive(true);
            btn_retry.onClick.RemoveAllListeners();
            btn_retry.onClick.AddListener(() =>
            {
                LoadingManager.Instance.Show("서버에 연결 중입니다...");
                btn_retry.gameObject.SetActive(false);
                ConnectNetwork().Forget();
            });
            return;
        }
    }

    public async UniTask<bool> AttemptAutoLogin()
    {
        //내부 세션 기반 자동 로그인
        if (await BackendManager.Instance.TryAutoLoginWithBackendTokenAsync())
        {
            HandleLoginSuccess();
            return true;
        }

        //세션 기반 자동 로그인 실패 시
        //저장된 로그인 타입에 따라 게스트 또는 구글 로그인 재시도
        //fallback
        string loginType = BackendManager.Instance.GetLoginType();
        bool googleAuto = BackendManager.Instance.IsGoogleAutoLogin();
        if (loginType == "guest") TryLogin("guest", true);
        else if (loginType == "google" && googleAuto) TryLogin("google", true);
        else ShowLoginButtons();
        return false;
    }

    private void TryLogin(string type, bool isAuto)
    {
        HideLoginButtons();
        string loginText = (isAuto ? "자동 " : "") + (type == "guest" ? "게스트" : "구글") + " 로그인 중...";
        LoadingManager.Instance.Show(loginText);

        BackendManager.Instance.OnLoginCompleteCallback = () =>
        {
            BackendManager.Instance.SetLoginType(type);
            BackendManager.Instance.SetAutoGoogleLogin(type == "google" && !isAuto);
            HandleLoginSuccess();
        };

        if (type == "guest")
        {
            BackendManager.Instance.LoginGuest(err =>
            {
                LoadingManager.Instance.Hide();
                Debug.Log($"{loginText} 실패: {err}");
                ShowLoginButtons();
            });
        }
        else if (type == "google")
        {
            TheBackend.ToolKit.GoogleLogin.Android.GoogleLogin((isSuccess, errMsg, token) =>
            {
                if (!isSuccess)
                {
                    LoadingManager.Instance.Hide();
                    Debug.Log($"구글 로그인 실패: {errMsg}");
                    ShowLoginButtons();
                    return;
                }

                BackendManager.Instance.LoginGoogle(true, errMsg, token);
            });
        }
    }

    public void OnClickLogout()
    {
        UIManager.Instance.ShowPopup<UIModalPopup>("UIModalPopup", false)
            .Set("로그아웃 확인", "정말 로그아웃하시겠습니까?", () =>
            {
                LoadingManager.Instance.Show("로그아웃 중입니다...");

                BackendManager.Instance.Logout(() =>
                {
                    LoadingManager.Instance.Hide();
                    UIManager.Instance.ShowPopup<UILoginPopup>("UILoginPopup");
                    ShowLoginButtons();
                });
            });
    }

    private void HandleLoginSuccess()
    {
        LoadingManager.Instance.Hide();
        UIManager.Instance.ShowPopup<UILobbyPopup>("UILobbyPopup").Init();
    }

    public void ShowLoginButtons()
    {
        btn_loginGuest.gameObject.SetActive(true);
        btn_loginGoogle.gameObject.SetActive(true);
    }

    private void HideLoginButtons()
    {
        btn_loginGuest.gameObject.SetActive(false);
        btn_loginGoogle.gameObject.SetActive(false);
    }

    protected override void ResetUI() { }
}
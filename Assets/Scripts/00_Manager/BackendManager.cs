using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BackEnd;
using Cysharp.Threading.Tasks;
using static EnumClass;
using LitJson;

public class BackendManager : Singleton<BackendManager>
{
    private const string TABLE_NAME = "Deck";
    private const string LOGIN_TYPE_KEY = "login_type";
    private const string GOOGLE_AUTOLOGIN_KEY = "google_autologin";
    private const string SUPPRESS_AUTOLOGIN_KEY = "suppress_autologin";

    private bool errorHandlerBound = false;  //전역 에러 핸들러 1회 바인딩

    public Action OnLoginCompleteCallback;

    private List<(string guid, DeckPack)> sortedDecks = new();

    #region 뒤끝 초기화
    public async UniTask<bool> InitBackendAsync()
    {
        try
        {
            var bro = Backend.Initialize();
            await UniTask.DelayFrame(1);
            bool ok = bro.IsSuccess();
            if (ok) BindGlobalBackendErrorHandlers();
            return ok;
        }
        catch
        {
            return false;
        }
    }
    #endregion

    #region 전역 에러 핸들러
    //전역 에러 핸들러 바인딩 (1회)
    private void BindGlobalBackendErrorHandlers()
    {
        if (errorHandlerBound) return;

        Backend.ErrorHandler.OnMaintenanceError = () => {
            Debug.Log("[유지보수] 점검 에러 발생");
            try { ToastManager.Instance.Show("점검 중입니다.", ToastAnchor.Top); } catch { }
        };

        Backend.ErrorHandler.OnTooManyRequestError = () => {
            Debug.Log("[요청 과다] 서버 제한(403) 발생");
            try { ToastManager.Instance.Show("과도한 요청이 감지되었습니다.", ToastAnchor.Top); } catch { }
        };

        Backend.ErrorHandler.OnTooManyRequestByLocalError = () => {
            Debug.Log("[요청 과다 - 로컬] 연속 요청 제한 발생");
            try { ToastManager.Instance.Show("과도한 요청중입니다.", ToastAnchor.Top); } catch { }
        };

        Backend.ErrorHandler.OnOtherDeviceLoginDetectedError = () => {
            Debug.Log("[중복 로그인 감지] 다른 기기에서 로그인됨 → 현재 세션 만료 처리");
            ForceSessionExpired("403", "already logged in on other device");
        };

        errorHandlerBound = true;
    }

    //구글 로그인 세션 만료 일괄 처리 (로딩 Hide → 토스트 → 플래그 → 로그인 팝업)
    private void ForceSessionExpired(string statusCode = null, string message = null)
    {
        Debug.Log($"[세션 만료 처리] code={statusCode}, msg={message}");

        //1) 로딩 숨기기
        try { LoadingManager.Instance.Hide(); }
        catch { }

        //2) 자동 로그아웃 토스트 (사유에 따라 문구 분기)
        try
        {
            string lower = string.IsNullOrEmpty(message) ? "" : message.ToLowerInvariant();
            string toastMsg =
                (!string.IsNullOrEmpty(statusCode) && (statusCode == "403" || statusCode == "401") && lower.Contains("already"))
                    ? "다른 기기에서 같은 계정으로 로그인되어 자동 로그아웃되었습니다."
                    : "세션이 만료되어 자동 로그아웃되었습니다.";

            ToastManager.Instance.Show(toastMsg, ToastAnchor.Top);
        }
        catch { }

        //3) 자동 로그인 상태값 정리
        ClearLoginType();
        SetSuppressAutoLogin();

        //4) 로그인 팝업 오픈
        var login = UIManager.Instance.ShowPopup<UILoginPopup>("UILoginPopup");
        login.ShowLoginButtons();
    }
    #endregion

    #region 게스트 로그인
    public void LoginGuest(Action<string> onFail = null, bool retried = false)
    {
        var bro = Backend.BMember.GuestLogin();
        if (bro.IsSuccess())
        {
            Debug.Log("[게스트 로그인 성공]");
            HandleLoginComplete();
            return;
        }

        string code = bro.GetStatusCode();
        string msg = bro.GetMessage();
        string lower = string.IsNullOrEmpty(msg) ? "" : msg.ToLowerInvariant();

        bool badCustomId =
            (code == "400" || code == "401") &&
            lower.Contains("customid") && (lower.Contains("bad") || lower.Contains("잘못된"));

        if (badCustomId && !retried)
        {
            Debug.Log($"[게스트 로그인 복구] bad customId 감지 → 로컬 게스트 정보 삭제 후 재시도. code={code}, msg={msg}");
            //게스트 정보 삭제
            Backend.BMember.DeleteGuestInfo();

            //1회만 재시도
            LoginGuest(onFail, true);
            return;
        }

        Debug.Log($"[게스트 로그인 실패] code={code}, msg={msg}");
        onFail?.Invoke(msg);
    }
    #endregion

    #region 구글 로그인
    public void LoginGoogle(bool isSuccess, string errorMessage, string token)
    {
        if (!isSuccess)
        {
            Debug.Log("[구글 로그인 실패] 토큰 획득 실패: " + errorMessage);
            OnLoginCompleteCallback = null;
            return;
        }

        var bro = Backend.BMember.AuthorizeFederation(token, FederationType.Google);
        Debug.Log("[페더레이션 로그인 결과]: " + bro);

        if (bro.IsSuccess())
        {
            HandleLoginComplete();
            return;
        }

        //다른 기기에서 이미 로그인된 상태일 경우
        if (bro.GetStatusCode() == "403" && bro.GetMessage().Contains("already logged in"))
        {
            Debug.Log("[구글 중복 로그인 감지] 기존 세션 로그아웃 후 재시도");
            //현재(다른 기기) 세션 강제 로그아웃
            Backend.BMember.Logout();

            //다시 로그인 재시도
            var retry = Backend.BMember.AuthorizeFederation(token, FederationType.Google);
            if (retry.IsSuccess())
            {
                Debug.Log("[구글 로그인 재시도 성공] 기존 세션 끊고 새 세션 확보");
                HandleLoginComplete();
                return;
            }
            else
            {
                Debug.Log("[구글 로그인 재시도 실패] " + retry.GetMessage());
            }
        }

        Debug.Log("[구글 로그인 실패] " + bro.GetMessage());
        OnLoginCompleteCallback = null;
    }
    #endregion

    #region 자동 로그인 처리
    public async UniTask<bool> TryAutoLoginWithBackendTokenAsync()
    {
        if (IsSuppressAutoLogin())
        {
            Debug.Log("[자동 로그인] 방지");
            return false;
        }

        var bro = Backend.BMember.LoginWithTheBackendToken();
        if (bro.IsSuccess())
        {
            Debug.Log("[자동 로그인 성공] 내부 세션 토큰으로 로그인 완료");
            return true;
        }

        Debug.Log("[자동 로그인 실패] 내부 토큰 만료 또는 무효");
        return false;
    }
    #endregion

    private void HandleLoginComplete()
    {
        try
        {
            //로그인 성공 시 자동 로그인 방지 플래그 해제
            ClearSuppressAutoLogin();

            //계정 전환 직후 이전 계정의 덱 캐시 제거
            ClearDeckCache();

            string nickname = GetNickname();
            if (string.IsNullOrEmpty(nickname))
            {
                string generatedName = $"유저 {UnityEngine.Random.Range(1000, 9999)}";
                SetNickname(generatedName,
                    onSuccess: () =>
                    {
                        Debug.Log($"닉네임 자동 설정: {generatedName}");
                        OnLoginCompleteCallback?.Invoke();
                    },
                    onFail: err =>
                    {
                        Debug.Log($"닉네임 설정 실패: {err}");
                    });
            }
            else OnLoginCompleteCallback?.Invoke();
        }
        catch (Exception e)
        {
            Debug.Log($"[LoginComplete 처리 중 오류] {e}");
            //최소한 방지 플래그는 해제해야 자동 로그인 정상 동작 가능
            ClearSuppressAutoLogin();
        }
    }

    #region 자동 로그인 방지
    public void SetSuppressAutoLogin()
    {
        PlayerPrefs.SetInt(SUPPRESS_AUTOLOGIN_KEY, 1);
        PlayerPrefs.Save();
    }

    private void ClearSuppressAutoLogin()
    {
        PlayerPrefs.DeleteKey(SUPPRESS_AUTOLOGIN_KEY);
        PlayerPrefs.Save();
    }

    public bool IsSuppressAutoLogin() => PlayerPrefs.GetInt(SUPPRESS_AUTOLOGIN_KEY, 0) == 1;
    #endregion

    #region 로그인 타입 관리
    public void SetLoginType(string type)
    {
        PlayerPrefs.SetString(LOGIN_TYPE_KEY, type);
        PlayerPrefs.Save();
    }

    public string GetLoginType() => PlayerPrefs.GetString(LOGIN_TYPE_KEY, "");

    public void ClearLoginType()
    {
        PlayerPrefs.DeleteKey(LOGIN_TYPE_KEY);
        PlayerPrefs.DeleteKey(GOOGLE_AUTOLOGIN_KEY);
        PlayerPrefs.Save();
    }

    public void SetAutoGoogleLogin(bool enabled)
    {
        PlayerPrefs.SetInt(GOOGLE_AUTOLOGIN_KEY, enabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    public bool IsGoogleAutoLogin() => PlayerPrefs.GetInt(GOOGLE_AUTOLOGIN_KEY, 0) == 1;
    #endregion

    #region 로그아웃
    public void Logout(Action onComplete = null)
    {
        //로그아웃 시 자동 로그인 방지
        SetSuppressAutoLogin();

        //이 계정의 덱 캐시 제거 (이전 계정 데이터 잔존 방지)
        ClearDeckCache();

        PlayerPrefs.DeleteKey(LOGIN_TYPE_KEY);
        PlayerPrefs.DeleteKey(GOOGLE_AUTOLOGIN_KEY);
        PlayerPrefs.Save();

        try
        {
            Backend.BMember.Logout();
        }
        catch { }

#if UNITY_ANDROID
        TheBackend.ToolKit.GoogleLogin.Android.GoogleSignOut((isSuccess, msg) =>
        {
            Debug.Log($"[구글 로그아웃] 성공 여부: {isSuccess}, 메시지: {msg}");
            onComplete?.Invoke();
        });
#else
    onComplete?.Invoke();
#endif
    }

    //덱 캐시 비우기 (로그아웃/로그인 전환 시 호출)
    private void ClearDeckCache()
    {
        if (sortedDecks != null) sortedDecks.Clear();
    }
    #endregion

    #region 닉네임 처리
    private void SetNickname(string nickname, Action onSuccess = null, Action<string> onFail = null)
    {
        var bro = Backend.BMember.UpdateNickname(nickname);
        if (bro.IsSuccess())
        {
            Debug.Log("닉네임 설정 성공");
            onSuccess?.Invoke();
        }
        else onFail?.Invoke(bro.GetMessage());
    }

    public string GetNickname() => Backend.UserNickName;
    #endregion

    #region 덱 저장
    public void SaveDeck(DeckPack pack, bool isNewSave)
    {
        string json = JsonUtility.ToJson(pack, true);
        Param param = new Param {
            { "guid", pack.guid },
            { "deckName", pack.deckName },
            { "jsonData", json }
        };

        if (isNewSave)
        {
            Backend.GameData.Insert(TABLE_NAME, param, callback => {
                if (!callback.IsSuccess())
                {
                    var code = callback.GetStatusCode();
                    var msg = callback.GetMessage();
                    Debug.Log($"[서버 저장 실패] {code} / {msg}");
                    return;
                }
                Debug.Log($"[서버 저장 성공] {pack.deckName}");
            });
        }
        else
        {
            Where where = new Where();
            where.Equal("guid", pack.guid);

            Backend.GameData.Update(TABLE_NAME, where, param, callback => {
                if (!callback.IsSuccess())
                {
                    var code = callback.GetStatusCode();
                    var msg = callback.GetMessage();
                    Debug.Log($"[서버 업데이트 실패] {code} / {msg}");
                    return;
                }
                Debug.Log($"[서버 업데이트 성공] {pack.deckName}");
            });
        }
    }
    #endregion

    #region 특정 덱 불러오기
    public void LoadDeckByGuid(string guid, Action<DeckPack> onLoaded)
    {
        Where where = new Where();
        where.Equal("guid", guid);

        Backend.GameData.Get(TABLE_NAME, where, callback => 
        {
            if (callback.IsSuccess() && callback.Rows().Count > 0)
            {
                string json = (string)callback.Rows()[0]["jsonData"]["S"];
                DeckPack pack = JsonUtility.FromJson<DeckPack>(json);
                onLoaded?.Invoke(pack);
            }
            else
            {
                var msg = callback.GetMessage();
                Debug.Log($"[서버 불러오기 실패] guid: {guid} / {msg}");
                onLoaded?.Invoke(null);
            }
        });
    }
    #endregion

    #region 모든 덱 불러오기
    public void LoadAllDecks(Action<List<(string guid, DeckPack)>> onLoaded = null)
    {
        //이전 덱 데이터 초기화
        sortedDecks.Clear();

        Backend.GameData.GetMyData(TABLE_NAME, new Where(), callback => {
            if (callback.IsSuccess())
            {
                var temp = new List<(string guid, DeckPack, DateTime inDate)>();
                foreach (JsonData row in callback.Rows())
                {
                    string guid = row["guid"]["S"].ToString();
                    string json = row["jsonData"]["S"].ToString();
                    string inDateStr = row["inDate"]["S"].ToString();
                    var inDate = DateTimeOffset.Parse(inDateStr).UtcDateTime;

                    DeckPack pack = JsonUtility.FromJson<DeckPack>(json);
                    if (pack != null && !string.IsNullOrEmpty(guid)) temp.Add((guid, pack, inDate));
                }

                sortedDecks = temp.OrderBy(x => x.inDate).Select(x => (x.guid, x.Item2)).ToList();

                //UI가 새 덱 데이터로 갱신할 수 있도록 콜백 제공
                onLoaded?.Invoke(sortedDecks);
            }
            else
            {
                var msg = callback.GetMessage();
                Debug.Log($"[서버 LoadAll 실패] {msg}");

                //실패 시에도 빈 목록 전달해 UI가 이전 덱 데이터 고정 노출을 피하도록
                onLoaded?.Invoke(sortedDecks);
            }
        });
    }
    #endregion

    #region 오름차순으로 정렬해서 캐싱해 둔 덱 목록 반환
    public List<(string guid, DeckPack)> GetSortedDecks() => sortedDecks;
    #endregion

    #region Photon 서버 접근 전 구글 로그인 세션 유효성 검증
    public bool EnsureSessionValidForPhoton()
    {
        //자동 로그인이 불가능한 상태 → 바로 로그아웃 처리
        try { LoadingManager.Instance.Hide(); } catch { }
        if (IsSuppressAutoLogin() || !IsGoogleAutoLogin())
        {
            //사용자 설정/상태로 자동로그인이 불가한 경우
            //즉시 로그아웃 처리 후 로그인 팝업 이동
            ForceSessionExpired("local_flag", "autologin suppressed or disabled");
            return false;
        }

        //구글 로그인 세션 토큰이 유효한 지
        //토큰이 유효: 세션 유효
        //토큰이 만료: 세션 만료
        var bro = Backend.BMember.LoginWithTheBackendToken();
        if (!bro.IsSuccess())
        {
            try { LoadingManager.Instance.Hide(); } catch { }

            string code = bro.GetStatusCode();
            string msg = bro.GetMessage() ?? "";
            string lower = msg.ToLowerInvariant();

            //즉시 로그아웃 처리
            bool otherDevice = code == "403" && (lower.Contains("already") || lower.Contains("other device"));  //다른 기기에서 새로 로그인한 경우
            bool authFail = code == "401" || lower.Contains("unauthorized") || (lower.Contains("invalid") && lower.Contains("token"));  //권한 없음 또는 토큰 무효
            bool goneExpired = code == "410" || lower.Contains("session");  //세션 만료
            bool tooManyReq = code == "429" || lower.Contains("too many");  //요청 과다로 인증 흐름 막힘
            bool anyAuthish = (code.StartsWith("4") && (lower.Contains("auth") || lower.Contains("token") || lower.Contains("session")));  //기타

            if (otherDevice || authFail || goneExpired || tooManyReq || anyAuthish)
            {
                //즉시 로그아웃 처리 후 로그인 팝업 이동
                ForceSessionExpired(code, msg);
                return false;
            }

            //그 외 케이스도 안전하게 로그아웃 처리
            ForceSessionExpired(code, msg);
            return false;
        }

        //성공→세션 유효
        return true;
    }
    #endregion

    #region 게스트 → 구글 승격: 성공 시 게스트 정보 삭제
    public bool TryUpgradeGuestToGoogle(string googleIdToken)
    {
        if (GetLoginType() != "guest")
        {
            Debug.Log("[구글 승격 실패] 현재 게스트가 아님");
            return false;
        }

        //게스트 (커스텀) → 구글 (페더레이션) 전환
        var bro = Backend.BMember.ChangeCustomToFederation(googleIdToken, FederationType.Google);

        if (!bro.IsSuccess())
        {
            string code = bro.GetStatusCode();
            string msg = bro.GetMessage();
            Debug.Log($"[구글 승격 실패] code={code}, msg={msg}");

            //이미 다른 계정에 연동된 구글 계정 등 중복 케이스는 그대로 안내
            if (!string.IsNullOrEmpty(msg) && msg.ToLowerInvariant().Contains("duplicated"))
                ToastManager.Instance.Show("이미 다른 계정에 연동된 구글 계정입니다.", ToastAnchor.Top);
            else if (code == "401")
                ToastManager.Instance.Show("구글 토큰이 유효하지 않습니다.", ToastAnchor.Top);

            return false;
        }

        //전환 성공: 이 기기의 '게스트 정보'를 공식 API로 삭제
        //이제 이 기기에서 다시 게스트 로그인하면 '새' 게스트 생성
        Backend.BMember.DeleteGuestInfo();

        SetLoginType("google");
        SetAutoGoogleLogin(true);
        Debug.Log("[구글 승격 성공] 커스텀(게스트) → 구글 전환 완료");
        return true;
    }
    #endregion

    // ====================================== 구현 중 =========================================== //
    // ====================================== 구현 중 =========================================== //
    // ====================================== 구현 중 =========================================== //


}

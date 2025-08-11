using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BackEnd;
using Cysharp.Threading.Tasks;

public class BackendManager : Singleton<BackendManager>
{
    private const string GUEST_UUID_KEY = "guest_uuid";
    private const string TABLE_NAME = "Deck";
    private const string LOGIN_TYPE_KEY = "login_type";
    private const string GOOGLE_AUTOLOGIN_KEY = "google_autologin";

    public Action OnLoginCompleteCallback;

    private List<(string guid, DeckPack)> sortedDecks = new();

    #region 뒤끝 초기화
    public async UniTask<bool> InitBackendAsync()
    {
        try
        {
            var bro = Backend.Initialize();
            await UniTask.DelayFrame(1);
            return bro.IsSuccess();
        }
        catch
        {
            return false;
        }
    }
    #endregion

    #region 게스트 로그인
    public void LoginGuest(Action<string> onFail = null)
    {
        string guestUuid = PlayerPrefs.GetString(GUEST_UUID_KEY, "");

        if (string.IsNullOrEmpty(guestUuid))
        {
            guestUuid = Guid.NewGuid().ToString();
            PlayerPrefs.SetString(GUEST_UUID_KEY, guestUuid);
            PlayerPrefs.Save();
        }

        var bro = Backend.BMember.GuestLogin(guestUuid);

        if (bro.IsSuccess())
        {
            Debug.Log($"게스트 로그인 성공: {bro}");
            HandleLoginComplete();
        }
        else if (bro.GetStatusCode() == "403" && bro.GetMessage().Contains("customId is invalid"))
        {
            PlayerPrefs.DeleteKey(GUEST_UUID_KEY);
            LoginGuest(onFail);  //1회 재시도
        }
        else
        {
            Debug.Log($"게스트 로그인 실패: {bro.GetMessage()}");
            onFail?.Invoke(bro.GetMessage());
        }
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

        if (bro.IsSuccess()) HandleLoginComplete();
        else
        {
            Debug.Log("[구글 로그인 실패] " + bro.GetMessage());
            OnLoginCompleteCallback = null;
        }
    }
    #endregion

    #region 자동 로그인 처리
    public async UniTask<bool> TryAutoLoginWithBackendTokenAsync()
    {
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
        string nickname = GetNickname();

        if (string.IsNullOrEmpty(nickname))
        {
            string generatedName = $"유저 {UnityEngine.Random.Range(1000, 9999)}";
            SetNickname(generatedName,
                onSuccess: () => {
                    Debug.Log($"닉네임 자동 설정: {generatedName}");
                    OnLoginCompleteCallback?.Invoke();
                },
                onFail: err => {
                    Debug.Log($"닉네임 설정 실패: {err}");
                });
        }
        else OnLoginCompleteCallback?.Invoke();
    }

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
        PlayerPrefs.DeleteKey(LOGIN_TYPE_KEY);
        PlayerPrefs.DeleteKey(GOOGLE_AUTOLOGIN_KEY);
        PlayerPrefs.Save();

#if UNITY_ANDROID
        TheBackend.ToolKit.GoogleLogin.Android.GoogleSignOut((isSuccess, msg) => {
            Debug.Log($"[구글 로그아웃] 성공 여부: {isSuccess}, 메시지: {msg}");
            onComplete?.Invoke();
        });
#else
        onComplete?.Invoke();
#endif
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
            Backend.GameData.Insert(TABLE_NAME, param, callback =>
            {
                Debug.Log(callback.IsSuccess()
                    ? $"[서버 저장] 신규 저장 성공: {pack.deckName}"
                    : $"[서버 저장] 신규 저장 실패: {callback.GetStatusCode()} / {callback.GetMessage()}");
            });
        }
        else
        {
            Where where = new Where();
            where.Equal("guid", pack.guid);

            Backend.GameData.Update(TABLE_NAME, where, param, callback =>
            {
                Debug.Log(callback.IsSuccess()
                    ? $"[서버 업데이트] 덮어쓰기 성공: {pack.deckName}"
                    : $"[서버 업데이트] 덮어쓰기 실패: {callback.GetStatusCode()} / {callback.GetMessage()}");
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
                Debug.Log($"[서버 불러오기 실패] guid: {guid}");
                onLoaded?.Invoke(null);
            }
        });
    }
    #endregion

    #region 모든 덱 불러오기
    public void LoadAllDecks()
    {
        sortedDecks.Clear();

        Backend.GameData.GetMyData(TABLE_NAME, new Where(), callback =>
        {
            List<(string guid, DeckPack, DateTime inDate)> temp = new();

            if (callback.IsSuccess())
            {
                foreach (LitJson.JsonData row in callback.Rows())
                {
                    string guid = row["guid"]["S"].ToString();
                    string json = row["jsonData"]["S"].ToString();
                    string inDateStr = row["inDate"]["S"].ToString();
                    DateTime inDate = DateTimeOffset.Parse(inDateStr).UtcDateTime;

                    DeckPack pack = JsonUtility.FromJson<DeckPack>(json);
                    if (pack != null && !string.IsNullOrEmpty(guid))
                        temp.Add((guid, pack, inDate));
                }

                //inDate 오름차순 정렬 (ex) 1,2,3,4 순)
                sortedDecks = temp.OrderBy(x => x.inDate)
                                   .Select(x => (x.guid, x.Item2))
                                   .ToList();
            }
            else
            {
                Debug.Log($"[서버 LoadAll 실패]: {callback.GetMessage()}");
            }
        });
    }
    #endregion

    #region 오름차순으로 정렬해서 캐싱해 둔 덱 목록 반환
    public List<(string guid, DeckPack)> GetSortedDecks() => sortedDecks;
    #endregion
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BackEnd;
using System;
using Cysharp.Threading.Tasks;
using System.Linq;

public class BackendManager : Singleton<BackendManager>
{
    private const string GUEST_UUID_KEY = "guest_uuid";
    private const string TableName = "Deck";

    public Action OnLoginCompleteCallback;

    protected override void Awake()
    {
        base.Awake();
    }

    public async UniTask<bool> InitBackendAsync()
    {
        try
        {
            var bro = Backend.Initialize();
            await UniTask.DelayFrame(1);  //안정화 대기 (프레임 기준)

            if (bro.IsSuccess()) {
                Debug.Log("뒤끝 초기화 성공");
                return true;
            }
            else return false;
        }
        catch (Exception e)
        {
            return false;
        }
    }

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
        else
        {
            if (bro.GetStatusCode() == "403" && bro.GetMessage().Contains("customId is invalid")) {
                PlayerPrefs.DeleteKey(GUEST_UUID_KEY);
                LoginGuest(onFail);  //재시도
            }
            else {
                Debug.Log($"게스트 로그인 실패: {bro.GetMessage()}");
                onFail?.Invoke(bro.GetMessage());
            }
        }
    }

    public void LoginGoogle(bool isSuccess, string errorMessage, string token)
    {
        if (!isSuccess) {
            Debug.Log(errorMessage);
            return;
        }

        Debug.Log("구글 토큰 : " + token);
        var bro = Backend.BMember.AuthorizeFederation(token, FederationType.Google);
        Debug.Log("페데레이션 로그인 결과 : " + bro);

        if (bro.IsSuccess()) HandleLoginComplete();
        else Debug.Log($"구글 로그인 실패: {bro.GetMessage()}");
    }

    private void HandleLoginComplete()
    {
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

    private void SetNickname(string nickname, Action onSuccess = null, Action<string> onFail = null)
    {
        var bro = Backend.BMember.UpdateNickname(nickname);
        if (bro.IsSuccess()) {
            Debug.Log("닉네임 설정 성공");
            onSuccess?.Invoke();
        }
        else onFail?.Invoke(bro.GetMessage());
    }

    public string GetNickname() => Backend.UserNickName;

    #region 덱 저장
    public void SaveDeck(DeckPack pack, bool isNewSave)
    {
        string json = JsonUtility.ToJson(pack, true);
        Param param = new Param
        {
            { "guid", pack.guid },
            { "deckName", pack.deckName },
            { "jsonData", json }
        };

        if (isNewSave) {
            //신규 저장
            Backend.GameData.Insert(TableName, param, callback =>
            {
                if (callback.IsSuccess()) Debug.Log($"[서버 저장] 신규 저장 성공: {pack.deckName}");
                else Debug.Log($"[서버 저장] 신규 저장 실패: {callback.GetStatusCode()} / {callback.GetMessage()}");
            });
        }
        else {
            //수정 (덮어쓰기)
            Where where = new Where();
            where.Equal("guid", pack.guid);

            Backend.GameData.Update(TableName, where, param, callback =>
            {
                if (callback.IsSuccess()) Debug.Log($"[서버 업데이트] 덮어쓰기 성공: {pack.deckName}");
                else Debug.Log($"[서버 업데이트] 덮어쓰기 실패: {callback.GetStatusCode()} / {callback.GetMessage()}");
            });
        }
    }
    #endregion

    #region 특정 덱 불러오기
    public void LoadDeckByGuid(string guid, Action<DeckPack> onLoaded)
    {
        Where where = new Where();
        where.Equal("guid", guid);

        Backend.GameData.Get(TableName, where, callback =>
        {
            if (callback.IsSuccess() && callback.Rows().Count > 0)
            {
                string json = (string)callback.Rows()[0]["jsonData"]["S"];
                DeckPack pack = JsonUtility.FromJson<DeckPack>(json);
                onLoaded?.Invoke(pack);
            }
            else
            {
                Debug.LogWarning($"[서버 불러오기 실패] guid: {guid}");
                onLoaded?.Invoke(null);
            }
        });
    }
    #endregion

    #region 모든 덱 불러오기
    public void LoadAllDecks(Action<List<(string guid, DeckPack)>> onLoaded)
    {
        Backend.GameData.GetMyData(TableName, new Where(), callback =>
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
                var sorted = temp.OrderBy(x => x.inDate)
                                 .Select(x => (x.guid, x.Item2))
                                 .ToList();

                onLoaded?.Invoke(sorted);
            }
            else
            {
                Debug.Log($"[서버 LoadAll 실패]: {callback.GetMessage()}");
                onLoaded?.Invoke(new List<(string, DeckPack)>());
            }
        });
    }

    public async UniTask<List<(string guid, DeckPack)>> LoadAllDecksAsync()
    {
        var tcs = new UniTaskCompletionSource<List<(string, DeckPack)>>();

        Backend.GameData.GetMyData(TableName, new Where(), callback =>
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
                var sorted = temp.OrderBy(x => x.inDate)
                                 .Select(x => (x.guid, x.Item2))
                                 .ToList();

                tcs.TrySetResult(sorted);
            }
            else
            {
                Debug.Log($"[서버 LoadAll 실패]: {callback.GetMessage()}");
                tcs.TrySetResult(new List<(string, DeckPack)>());
            }
        });

        return await tcs.Task;
    }
    #endregion
}
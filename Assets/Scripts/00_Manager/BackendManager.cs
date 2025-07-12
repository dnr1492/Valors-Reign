using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BackEnd;
using System;
using Cysharp.Threading.Tasks;
using UnityEditor;

public class BackendManager : Singleton<BackendManager>
{
    private const string TableName = "Deck";

    protected override void Awake()
    {
        base.Awake();
    }

    public async UniTask<bool> InitBackendAsync()
    {
        try
        {
            var bro = Backend.Initialize(true);
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

    public void LoginGuest(Action onSuccess = null, Action<string> onFail = null)
    {
        BackendReturnObject bro = Backend.BMember.LoginWithTheBackendToken();
        if (bro.IsSuccess()) {
            Debug.Log($"게스트 로그인 성공: {bro}");
            onSuccess?.Invoke();
        }
        else {
            //새로 가입 시도
            bro = Backend.BMember.GuestLogin();
            if (bro.IsSuccess()) {
                Debug.Log("게스트 계정 생성 및 로그인 성공");
                onSuccess?.Invoke();
            }
            else {
                onFail?.Invoke(bro.GetMessage());
            }
        }
    }

    public void SetNickname(string nickname, Action onSuccess = null, Action<string> onFail = null)
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
    public void SaveDeck(DeckPack pack)
    {
        if (string.IsNullOrEmpty(pack.guid))
            pack.guid = Guid.NewGuid().ToString();

        string json = JsonUtility.ToJson(pack, true);
        Param param = new Param
        {
            { "guid", pack.guid },
            { "deckName", pack.deckName },
            { "jsonData", json }
        };

        Backend.GameData.Insert(TableName, param, callback =>
        {
            if (callback.IsSuccess()) Debug.Log($"[서버 저장] 성공: {pack.deckName}");
            else if (callback.GetStatusCode() == "409")
            {
                //중복 시 Update
                Where where = new Where();
                where.Equal("guid", pack.guid);
                Backend.GameData.Update(TableName, where, param, updateCallback =>
                {
                    if (updateCallback.IsSuccess()) Debug.Log($"[서버 업데이트] 성공: {pack.deckName}");
                    else Debug.LogError($"[서버 업데이트] 실패: {updateCallback.GetMessage()}");
                });
            }
            else Debug.LogError($"[서버 저장] 실패: {callback.GetMessage()}");
        });
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
            List<(string guid, DeckPack)> result = new();

            if (callback.IsSuccess())
            {
                foreach (LitJson.JsonData row in callback.Rows())
                {
                    string guid = row["guid"].ToString();
                    string json = row["jsonData"].ToString();

                    DeckPack pack = JsonUtility.FromJson<DeckPack>(json);
                    if (pack != null && !string.IsNullOrEmpty(guid))
                        result.Add((guid, pack));
                }
            }
            else Debug.LogError($"[서버 LoadAll 실패]: {callback.GetMessage()}");

            onLoaded?.Invoke(result);
        });
    }

    public async UniTask<List<(string guid, DeckPack)>> LoadAllDecksAsync()
    {
        var tcs = new UniTaskCompletionSource<List<(string, DeckPack)>>();

        Backend.GameData.GetMyData(TableName, new Where(), callback =>
        {
            List<(string guid, DeckPack)> result = new();

            if (callback.IsSuccess())
            {
                foreach (LitJson.JsonData row in callback.Rows())
                {
                    string guid = row["guid"].ToString();
                    string json = row["jsonData"].ToString();

                    DeckPack pack = JsonUtility.FromJson<DeckPack>(json);
                    if (pack != null && !string.IsNullOrEmpty(guid))
                        result.Add((guid, pack));
                }
            }
            else Debug.LogError($"[서버 LoadAll 실패]: {callback.GetMessage()}");

            tcs.TrySetResult(result);
        });

        return await tcs.Task;
    }
    #endregion
}
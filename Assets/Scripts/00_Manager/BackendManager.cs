using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BackEnd;
using System;
using Cysharp.Threading.Tasks;

public class BackendManager : Singleton<BackendManager>
{
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
            else {
                Debug.LogError($"뒤끝 초기화 실패: {bro.GetMessage()}");
                return false;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[BackendManager] 예외 발생: {e.Message}");
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
                Debug.LogError($"게스트 로그인 실패: {bro.GetMessage()}");
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
        else {
            Debug.LogError($"닉네임 설정 실패: {bro.GetMessage()}");
            onFail?.Invoke(bro.GetMessage());
        }
    }

    public string GetNickname() => Backend.UserNickName;
}
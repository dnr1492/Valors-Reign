using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BackEnd;
using System;

public class BackendManager : Singleton<BackendManager>
{
    protected override void Awake()
    {
        base.Awake();

        InitBackend();
    }

    private void InitBackend()
    {
        //뒤끝 서버에 연결
        var bro = Backend.Initialize();
        if (bro.IsSuccess()) Debug.Log($"뒤끝 서버 초기화 성공 : {bro}");  //204
        else Debug.Log($"뒤끝 서버 초기화 실패 : {bro}");  //400
    }
}
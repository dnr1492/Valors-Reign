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
        //�ڳ� ������ ����
        var bro = Backend.Initialize();
        if (bro.IsSuccess()) Debug.Log($"�ڳ� ���� �ʱ�ȭ ���� : {bro}");  //204
        else Debug.Log($"�ڳ� ���� �ʱ�ȭ ���� : {bro}");  //400
    }
}
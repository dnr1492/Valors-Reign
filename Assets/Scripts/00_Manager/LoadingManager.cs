using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Threading;
using Cysharp.Threading.Tasks;

public class LoadingManager : Singleton<LoadingManager>
{
    [SerializeField] GameObject root;  //전체 로딩 패널
    [SerializeField] TextMeshProUGUI loadingText;  //메시지 출력용

    [Header("로딩 이미지 애니메이션")]
    [SerializeField] GameObject[] loadingRotateImages;  //Loading_rotate_00 ~ 11
    private CancellationTokenSource cts;
    private int currentIndex = 0;

    [Header("로딩 점(.) 텍스트 애니메이션")]
    private string baseMessage;
    private int maxDotCount;

    protected override void Awake()
    {
        base.Awake();

        if (root != null)
            root.SetActive(false);
    }

    /// <summary>
    /// 로딩 UI 표시
    /// </summary>
    public void Show(string message)
    {
        if (root != null)
            root.SetActive(true);

        //점 개수 감지
        int dotStart = message.LastIndexOf('.');
        maxDotCount = 0;
        if (dotStart != -1)
        {
            //문자열 끝에서부터 점 개수 세기
            int i = message.Length - 1;
            while (i >= 0 && message[i] == '.') {
                maxDotCount++;
                i--;
            }
            baseMessage = message.Substring(0, message.Length - maxDotCount);
        }
        else {
            baseMessage = message;
            maxDotCount = 3;
        }

        loadingText.text = baseMessage;

        StartLoadingAnimation();
    }

    /// <summary>
    /// 로딩 UI 숨기기
    /// </summary>
    public void Hide()
    {
        if (root != null)
            root.SetActive(false);

        StopLoadingAnimation();
    }

    /// <summary>
    /// 로딩 애니메이션 시작
    /// </summary>
    private void StartLoadingAnimation()
    {
        StopLoadingAnimation();

        cts = new CancellationTokenSource();
        var token = cts.Token;

        //이미지 애니메이션
        UniTask.Void(async () =>
        {
            while (!token.IsCancellationRequested)
            {
                for (int i = 0; i < loadingRotateImages.Length; i++)
                    loadingRotateImages[i].SetActive(i == currentIndex);
                currentIndex = (currentIndex + 1) % loadingRotateImages.Length;

                await UniTask.Delay(100, cancellationToken: token);
            }
        });

        //점(.) 텍스트 애니메이션
        UniTask.Void(async () =>
        {
            int dotCount = 1;
            while (!token.IsCancellationRequested)
            {
                if (loadingText != null)
                    loadingText.text = baseMessage + new string('.', dotCount);

                dotCount++;
                if (dotCount > maxDotCount) dotCount = 1;  //항상 1 ~ maxDotCount 순환

                await UniTask.Delay(500, cancellationToken: token);
            }
        });
    }

    /// <summary>
    /// 로딩 애니메이션 중단
    /// </summary>
    private void StopLoadingAnimation()
    {
        if (cts != null)
        {
            cts.Cancel();
            cts.Dispose();
            cts = null;
        }

        //모두 끄기
        foreach (var img in loadingRotateImages)
            img.SetActive(false);

        currentIndex = 0;
    }
}

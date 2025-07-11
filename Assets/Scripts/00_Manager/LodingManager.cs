using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LodingManager : Singleton<LodingManager>
{
    [SerializeField] GameObject root;  //전체 로딩 패널
    [SerializeField] TextMeshProUGUI loadingText;  //메시지 출력용

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

        if (loadingText != null)
            loadingText.text = message;
    }

    /// <summary>
    /// 로딩 UI 숨기기
    /// </summary>
    public void Hide()
    {
        if (root != null)
            root.SetActive(false);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UICreateDeckPhase1 : UIPopupBase
{
    // ===== 임시 버튼 1개 ===== //
    [SerializeField] Button btn;

    private void Awake()
    {
        btn.onClick.AddListener(OnClick_Primordial);
    }

    private void OnClick_Primordial()
    {
        UIManager.Instance.ShowPopup<UICreateDeckPhase2>("UICreateDeckPhase2");
    }

    protected override void ResetUI()
    {
        // ===== 초기화 로직 구현 요망 ===== //
    }
}

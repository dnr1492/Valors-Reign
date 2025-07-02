using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIEditorDeckPhase2 : UIPopupBase
{
    [SerializeField] Button btn_back;

    private void Awake()
    {
        btn_back.onClick.AddListener(OnClickBack);
    }

    private void OnClickBack()
    {
        UIManager.Instance.ShowPopup<UIEditorDeckPhase1>("UIEditorDeckPhase1");
    }

    protected override void ResetUI() { }
}

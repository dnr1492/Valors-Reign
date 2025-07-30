using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

public class UIModalPopup : UIPopupBase
{
    [SerializeField] TextMeshProUGUI txtTitle;
    [SerializeField] TextMeshProUGUI txtBody;
    [SerializeField] Button btnClose, btnConfirm;

    private Action onConfirm = null;

    public UIModalPopup Set(string title, string body, Action confirmCallback = null)
    {
        txtTitle.text = title;
        txtBody.text = body;
        onConfirm = confirmCallback;

        btnClose.onClick.RemoveAllListeners();
        btnClose.onClick.AddListener(Close);

        btnConfirm.onClick.RemoveAllListeners();
        btnConfirm.onClick.AddListener(() =>
        {
            onConfirm?.Invoke();
            Close();
        });

        transform.SetAsLastSibling();
        return this;
    }

    protected override void ResetUI() { }
}

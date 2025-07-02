using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIModalPopup : UIPopupBase
{
    [SerializeField] TextMeshProUGUI txtTitle;
    [SerializeField] TextMeshProUGUI txtBody;
    [SerializeField] Button btnClose, btnCheck;

    public UIModalPopup Set(string title, string body)
    {
        txtTitle.text = title;
        txtBody.text = body;

        btnClose.onClick.RemoveAllListeners();
        btnClose.onClick.AddListener(Close);

        btnCheck.onClick.RemoveAllListeners();
        btnCheck.onClick.AddListener(Close);

        return this;
    }

    protected override void ResetUI() { }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIMain : MonoBehaviour
{
    private void Start()
    {
        // ===== 임시로 먼저 Phase1 팝업 열기 - 추후 로비 화면부터... ===== //
        UIManager.Instance.ShowPopup<UIEditorDeckPhase1>("UIEditorDeckPhase1");
    }
}

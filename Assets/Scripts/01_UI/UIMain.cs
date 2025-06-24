using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIMain : MonoBehaviour
{
    private void Start()
    {
        // ===== 임시로 가장 먼저 시작 ===== //
        UIManager.Instance.ShowPopup<UICreateDeckPhase1>("UICreateDeckPhase1");
    }
}

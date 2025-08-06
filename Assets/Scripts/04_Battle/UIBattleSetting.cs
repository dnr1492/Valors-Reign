using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIBattleSetting : UIPopupBase
{
    [SerializeField] GameObject uiCoinFlipPrefab;
    [SerializeField] Button btn_back;

    [Header("Hex Grid")]
    [SerializeField] RectTransform hexParantRt /*map*/, battleFieldRt;
    [SerializeField] GameObject hexPrefab;  //육각형 모양의 이미지가 있는 UI 프리팹

    private UICoinFlip uiCoinFlip;
    private bool isMyTurnFirst;

    public void Init()
    {
        GridManager.Instance.CreateHexGrid(battleFieldRt, hexPrefab, hexParantRt, false, true);

        UIEditorDeckPhase1 popup = UIManager.Instance.GetPopup<UIEditorDeckPhase1>("UIEditorDeckPhase1");
        var pack = popup.GetSelectedDeckPack();
        if (pack != null) GridManager.Instance.ShowDecksOnField(pack, ControllerRegister.Get<PhotonController>().OpponentDeckPack);

        var go = Instantiate(uiCoinFlipPrefab, transform);
        uiCoinFlip = go.GetComponent<UICoinFlip>();
        uiCoinFlip.Init(OnCoinSelected);
    }

    private void OnCoinSelected(int myChoice)
    {
        ControllerRegister.Get<PhotonController>().RequestCoinFlip(myChoice);
    }

    public void ShowCoinFlipResult(int result, bool isMineFirst)
    {
        uiCoinFlip.PlayFlipAnimation(result, () => {
            SetTurnOrder(isMineFirst);
        });
    }

    public void SetTurnOrder(bool isMineFirst)
    {
        isMyTurnFirst = isMineFirst;
        Debug.Log("내 턴인가? " + isMyTurnFirst);

        //if (isMyTurnFirst)
        //{
        //    DrawInitialCards();         //카드 드로우
        //    ShowCardSettingUI();        //4장 세팅 UI 활성화
        //}
        //else
        //{
        //    ShowOpponentWaitingUI();    //상대 설정 기다리는 표시
        //}
    }

    protected override void ResetUI() { }
}
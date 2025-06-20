using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UICreateDeckPhase2 : UIPopupBase
{
    [SerializeField] Button btn_back, btn_save, btn_load, btn_reset;

    [Header("Hex Grid")]
    [SerializeField] RectTransform hexParantRt /*map*/, battleFieldRt;
    [SerializeField] GameObject hexPrefab;  //사각형 안에 육각형 이미지 있는 UI 프리팹

    [Header("Cost")]
    [SerializeField] Slider sliCost;
    private int maxCost, sumCost;

    private void Awake()
    {
        btn_back.onClick.AddListener(OnClickBack);
        btn_save.onClick.AddListener(OnClickSave);
        btn_load.onClick.AddListener(OnClickLoad);
        btn_reset.onClick.AddListener(OnClickReset);
    }

    private void Start()
    {
        GridManager.Instance.CreateHexGrid(battleFieldRt, hexPrefab, hexParantRt);

        maxCost = DataManager.Instance.gamePlayData.maxCost;
        sliCost.value = 0;
        sliCost.maxValue = maxCost;
    }

    public void SetMaxCost(int cost)
    {
        int newCost = sumCost + cost;
        if (newCost < 0 || newCost > maxCost) return;

        sumCost = newCost;
        sliCost.value = sumCost;
    }

    public void OnClickBack()
    {
        UIManager.Instance.ShowPopup<UICreateDeckPhase1>("UICreateDeckPhase1");
    }

    public void OnClickSave()
    {
        GridManager.Instance.SaveTokenPack(GridManager.Instance.GetTokenPack());

        //deckGenerator.CreateDeck();
    }

    public void OnClickLoad()
    {
        GridManager.Instance.LoadTokenPack();
    }

    public void OnClickReset()
    {
        GridManager.Instance.ResetDeckPhase2();
    }

    protected override void ResetUI()
    {
        GridManager.Instance.ResetDeckPhase2();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIMain : MonoBehaviour
{
    [Header("Hex Grid")]
    [SerializeField] RectTransform hexParantRt /*map*/, battleFieldRt;
    [SerializeField] GameObject hexPrefab;  //사각형 안에 육각형 이미지 있는 UI 프리팹

    [Header("Cost")]
    [SerializeField] Slider sliCost;
    private int maxCost, sumCost;

    private void Start()
    {
        GridManager.Instance.CreateHexGrid(battleFieldRt, hexPrefab, hexParantRt);

        maxCost = DataManager.Instance.gamePlayData.maxCost;
        sliCost.value = 0;
        sliCost.maxValue = maxCost;
    }

    public void OnClickAddDeck()
    {
         //deckGenerator.CreateDeck();
    }

    public void SetMaxCost(int cost)
    {
        if (sumCost + cost > maxCost) return;
        sumCost += cost;

        sliCost.value = sumCost;
    }
}
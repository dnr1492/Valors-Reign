using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIMain : MonoBehaviour
{
    [SerializeField] DeckGenerator deckGenerator;
    [SerializeField] GameObject createDeckPhase1, createDeckPhase2;

    [SerializeField] RectTransform hexParantRt /*map*/, battleFieldRt;
    [SerializeField] GameObject hexPrefab;  //사각형 안에 육각형 이미지 있는 UI 프리팹

    private void Start()
    {
        GridManager.Instance.CreateHexGrid(battleFieldRt, hexPrefab, hexParantRt);
    }

    public void OnClickAddDeck()
    {
         //deckGenerator.CreateDeck();
    }
}
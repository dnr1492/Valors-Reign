using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager
{
    private static GridManager instance = null;

    private GridManager() { }

    public static GridManager GetInstance()
    {
        if (instance == null) instance = new GridManager();
        return instance;
    }

    private int rows = 8;  //행 (가로)
    private int columns = 9;  //열 (세로)
    private float hexWidth = 102f;  //육각형의 가로 크기, 해당 프리팹의 기본 크기 100
    private float hexHeight = 88.5f;  //육각형의 세로 크기 (정육각형 기준으로 √3/2 * 너비), 해당 프리팹의 기본 크기 88

    private readonly Dictionary<(int col, int row), string> txtMap = new()
    {
        //행, 열
        { (8, 7), "H" }, { (8, 6), "L" },
        { (7, 6), "M" },
        { (6, 7), "H" }, { (6, 6), "L" },
        { (5, 6), "M" },
        { (4, 7), "R" }, { (4, 6), "L" },
        { (3, 6), "M" },
        { (2, 7), "H" }, { (2, 6), "L" },
        { (1, 6), "M" },
        { (0, 7), "H" }, { (0, 6), "L" },
    };

    public void CreateHexGrid(RectTransform mainRt, GameObject hexPrefab, RectTransform parant)
    {
        Resize(mainRt);

        for (int col = 0; col < columns; col++)
        {
            //홀수 열은 하나 줄인다
            int maxRow = rows - ((col % 2 == 1) ? 1 : 0);

            for (int row = 0; row < maxRow; row++)
            {
                GameObject hex = Object.Instantiate(hexPrefab, parant);
                RectTransform rt = hex.GetComponent<RectTransform>();
                var widthOffset = 2 * mainRt.localScale.x;
                var heightOffset = 0.25f * mainRt.localScale.y;
                rt.sizeDelta = new Vector2(hexWidth - widthOffset, hexHeight - heightOffset);

                float x = col * hexWidth * 0.75f + 4;
                float y = row * hexHeight + ((col % 2 == 1) ? hexHeight / 2f : 0f) + 1;

                //UI y축은 아래로 내려야 하므로 부호 반대
                rt.anchoredPosition = new Vector2(x, -y);

                //텍스트 입력
                if (txtMap.TryGetValue((col, row), out string textValue))
                {
                    var textComponent = hex.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                    if (textComponent != null) textComponent.text = textValue;
                    Debug.Log($"열 {col}, 행 {row} : {textValue}");
                }
            }
        }
    }

    private void Resize(RectTransform mainRt)
    {
        Vector3 scale = mainRt.localScale;
        hexWidth = hexWidth * scale.x;
        hexHeight = hexHeight * scale.y;
        Debug.Log($"Scale: {scale}, Hex Size: {hexWidth} x {hexHeight}");
    }
}
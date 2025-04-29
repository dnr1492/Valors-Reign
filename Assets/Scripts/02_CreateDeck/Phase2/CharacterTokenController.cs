using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class CharacterTokenController : MonoBehaviour
{
    [SerializeField] GridLayoutGroup gridLayoutGroup;
    [SerializeField] RectTransform contentRt;

    private readonly int columnCount = 4;
    private readonly float aspectRatio = 1.136f;  //100(넓이)/88(높이)
    private readonly float safeMarginRatio = 0.97f;  //여유를 위해 97%만 사용

    private void Awake()
    {
        ControllerRegister.Register(this);
    }

    private void Start()
    {
        ResizeCharacterTokenCellSize();
    }

    private void ResizeCharacterTokenCellSize()
    {
        float totalWidth = contentRt.rect.width;
        float paddingHorizontal = gridLayoutGroup.padding.left + gridLayoutGroup.padding.right;
        float spacingHorizontal = gridLayoutGroup.spacing.x * (columnCount - 1);

        float availableWidth = (totalWidth - paddingHorizontal - spacingHorizontal) * safeMarginRatio;

        float cellWidth = availableWidth / columnCount;
        float cellHeight = cellWidth / aspectRatio;

        gridLayoutGroup.cellSize = new Vector2(cellWidth, cellHeight);
    }

    public CharacterToken[] GetAllCharacterToken()
    {
        var characterTokens = gridLayoutGroup.GetComponentsInChildren<CharacterToken>(true);
        return characterTokens;
    }
}

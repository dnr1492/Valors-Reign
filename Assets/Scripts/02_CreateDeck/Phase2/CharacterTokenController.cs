using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static EnumClass;

public class CharacterTokenController : MonoBehaviour
{
    [SerializeField] RectTransform contentRt;

    private GridLayoutGroup gridLayoutGroup;
    private CharacterToken[] arrAllCharacterToken;

    private readonly int columnCount = 4;
    private readonly float aspectRatio = 1.136f;  //100(넓이)/88(높이)
    private readonly float safeMarginRatio = 0.97f;  //여유를 위해 97%만 사용

    private void Awake()
    {
        gridLayoutGroup = contentRt.GetComponent<GridLayoutGroup>();

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
        if (arrAllCharacterToken == null) arrAllCharacterToken = gridLayoutGroup.GetComponentsInChildren<CharacterToken>(true);
        return arrAllCharacterToken;
    }

    public void OnClickToken(CharacterToken clickedToken, CharacterCard characterCard)
    {
        //확정된(Confirm) 토큰을 선택한 경우
        if (clickedToken.State == CharacterTokenState.Confirm)
        {
            foreach (var token in GetAllCharacterToken())
            {
                if (token != clickedToken && token.State == CharacterTokenState.Select)
                    token.SetTokenState(CharacterTokenState.Cancel);
            }
            return;
        }

        //선택한(Select) 토큰을 다시 선택한 경우
        if (clickedToken.State == CharacterTokenState.Select)
        {
            clickedToken.SetTokenState(CharacterTokenState.Cancel);

            //캐릭터 카드 뒷면으로 변경
            characterCard.SetCardState(CardState.Back);
            return;
        }

        //선택한(Select) 토큰 외에 다른 토큰을 선택한(Select) 경우
        foreach (var token in GetAllCharacterToken())
        {
            if (token.State == CharacterTokenState.Select)
                token.SetTokenState(CharacterTokenState.Cancel);
        }

        clickedToken.SetTokenState(CharacterTokenState.Select);
    }

    public void OnClickConfirm(CharacterToken clickedToken)
    {
        //Confirm 상태에서 다시 누르면 Cancel
        if (clickedToken.State == CharacterTokenState.Confirm)
        {
            clickedToken.SetTokenState(CharacterTokenState.Select);

            //선택한 캐릭터 토큰을 배틀필드에 표시 제거
            GridManager.Instance.RemoveTokenFromBattlefield(clickedToken);
            return;
        }

        if (clickedToken.State != CharacterTokenState.Select) return;

        //Captain은 오직 하나만 Confirm 가능
        if (clickedToken.Tier == CharacterTierAndCost.Captain)
        {
            foreach (var t in GetAllCharacterToken())
            {
                if (t.Tier == CharacterTierAndCost.Captain && t.State == CharacterTokenState.Confirm)
                    t.SetTokenState(CharacterTokenState.Cancel);
            }
        }

        clickedToken.SetTokenState(CharacterTokenState.Confirm);

        //선택한 캐릭터 토큰을 배틀필드에 표시
        GridManager.Instance.DisplayTokenOnBattlefield(clickedToken);
    }
}

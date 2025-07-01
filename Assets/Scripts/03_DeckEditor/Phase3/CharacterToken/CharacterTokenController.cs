using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static EnumClass;

public class CharacterTokenController : MonoBehaviour
{
    [SerializeField] SkillSlotCollection skillSlotCollection;
    [SerializeField] RectTransform contentRt;

    private GridLayoutGroup gridLayoutGroup;
    private CharacterToken[] arrAllCharacterToken;

    private readonly int columnCount = 4;
    private readonly float aspectRatio = 1.136f;  //100(너비)/88(높이)
    private readonly float safeMarginRatio = 0.97f;  //안전한 여백 97%로 설정

    private void Awake()
    {
        gridLayoutGroup = contentRt.GetComponent<GridLayoutGroup>();
        ControllerRegister.Register(this);
    }

    private void Start()
    {
        ResizeCharacterTokenCellSize();
    }

    //캐릭터 토큰 셀 크기 조정
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

    //모든 캐릭터 토큰 가져오기
    public CharacterToken[] GetAllCharacterToken()
    {
        if (arrAllCharacterToken == null) arrAllCharacterToken = gridLayoutGroup.GetComponentsInChildren<CharacterToken>(true);
        return arrAllCharacterToken;
    }

    //캐릭터 토큰 클릭 처리
    public void OnClickToken(CharacterToken clickedToken, CharacterCard characterCard)
    {
        //확정된(Confirm) 토큰을 다시 클릭한 경우
        if (clickedToken.State == CharacterTokenState.Confirm)
        {
            CancelOtherSelectedTokens(clickedToken);
            return;
        }

        //선택된(Select) 토큰을 다시 클릭한 경우
        if (clickedToken.State == CharacterTokenState.Select)
        {
            clickedToken.SetTokenState(CharacterTokenState.Cancel);
            characterCard.SetCardState(CardState.Back);
            return;
        }

        //새로운 토큰 선택 시 다른 토큰들 취소
        CancelAllSelectedTokens();
        clickedToken.SetTokenState(CharacterTokenState.Select);
    }

    //확정 버튼 클릭 처리
    public void OnClickConfirm(CharacterToken clickedToken)
    {
        //Confirm 상태에서 다시 클릭하면 Select (스킬 매수 정보는 유지)
        if (clickedToken.State == CharacterTokenState.Confirm)
        {
            clickedToken.SetTokenState(CharacterTokenState.Select);
            RemoveTokenFromBattlefield(clickedToken);
            return;
        }

        if (clickedToken.State != CharacterTokenState.Select) return;

        //Captain은 하나만 Confirm 가능
        if (clickedToken.Tier == CharacterTierAndCost.Captain) CancelOtherCaptainTokens();

        clickedToken.SetTokenState(CharacterTokenState.Confirm);
        DisplayTokenOnBattlefield(clickedToken);
    }

    //다른 선택된 토큰들 취소 (확정 토큰 제외)
    private void CancelOtherSelectedTokens(CharacterToken excludeToken)
    {
        foreach (var token in GetAllCharacterToken())
        {
            if (token != excludeToken && token.State == CharacterTokenState.Select) 
                token.SetTokenState(CharacterTokenState.Cancel);
        }
    }

    //모든 선택된 토큰들 취소
    private void CancelAllSelectedTokens()
    {
        foreach (var token in GetAllCharacterToken())
        {
            if (token.State == CharacterTokenState.Select) 
                token.SetTokenState(CharacterTokenState.Cancel);
        }
    }

    //다른 Captain 토큰들 취소
    private void CancelOtherCaptainTokens()
    {
        foreach (var token in GetAllCharacterToken())
        {
            if (token.Tier == CharacterTierAndCost.Captain && token.State == CharacterTokenState.Confirm) 
                token.SetTokenState(CharacterTokenState.Cancel);
        }
    }

    //전장에서 토큰 제거
    private void RemoveTokenFromBattlefield(CharacterToken token)
    {
        GridManager.Instance.RemoveTokenFromBattlefield(token);
        skillSlotCollection.Refresh();
    }

    //전장에 토큰 표시
    private void DisplayTokenOnBattlefield(CharacterToken token)
    {
        GridManager.Instance.DisplayTokenOnBattlefield(token);
        skillSlotCollection.Refresh();
    }

    //특정 상태의 토큰들 가져오기 (현재 사용하지 않음 - 추후 필터링 기능 구현 시 사용 예정)
    //public CharacterToken[] GetTokensByState(CharacterTokenState state)
    //{
    //    return GetAllCharacterToken().Where(token => token.State == state).ToArray();
    //}

    //선택된 토큰들 가져오기 (현재 사용하지 않음 - 추후 덱 저장 기능 구현 시 사용 예정)
    //public CharacterToken[] GetSelectedTokens()
    //{
    //    return GetTokensByState(CharacterTokenState.Confirm);
    //}
}

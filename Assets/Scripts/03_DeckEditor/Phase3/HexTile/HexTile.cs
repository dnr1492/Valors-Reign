using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static EnumClass;

public class HexTile : MonoBehaviour
{
    [SerializeField] Image imgCharacter, outline, direction;

    public (int col, int row) GridPosition { get; private set; }
    public Nullable<int> AssignedTokenKey { get; private set; } = null;
    public CharacterToken AssignedToken { get; private set; }
    public CharacterTokenDirection CharacterTokenDirection { get; private set; }  //캐릭터 토큰 방향 저장

    //헥스 타일 초기화
    public void Init((int, int) pos)
    {
        ShowDecorations(false);
        GridPosition = pos;
    }

    //토큰 할당
    public void AssignToken(int tokenKey, CharacterToken token)
    {
        ShowDecorations(true);
        AssignedTokenKey = tokenKey;
        AssignedToken = token;
    }

    //토큰 제거
    public void ClearToken()
    {
        ShowDecorations(false);
        AssignedTokenKey = null;
        AssignedToken = null;
    }

    //장식 요소 표시/숨김 (아웃라인, 방향 표시)
    public void ShowDecorations(bool isShow)
    {
        outline.gameObject.SetActive(isShow);
        direction.gameObject.SetActive(isShow);
    }

    //헥스 타일 색상 설정
    public void SetColor(Color color)
    {
        imgCharacter.color = color;
    }

    //캐릭터 토큰 방향 설정 (현재 사용하지 않음 - 추후 검증 기능 구현 시 사용 예정)
    //사용 예시: hexTile.SetCharacterTokenDirection(CharacterTokenDirection.UpLeft);
    public void SetCharacterTokenDirection(CharacterTokenDirection direction)
    {
        float[] angles = { 0f, 60f, 120f, 180f, 240f, 300f };
        int index = Mathf.Clamp((int)direction, 0, 5);
        imgCharacter.rectTransform.rotation = Quaternion.Euler(0, 0, -angles[index]);  //시계방향 회전
    }
}

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
    public CharacterTokenDirection CharacterTokenDirection { get; private set; }  // ===== 추후 방향 저장용 ===== //

    public void Init((int, int) pos)
    {
        ShowDecorations(false);

        GridPosition = pos;
    }

    public void AssignToken(int tokenKey)
    {
        ShowDecorations(true);

        AssignedTokenKey = tokenKey;
    }

    public void ClearToken()
    {
        ShowDecorations(false);

        AssignedTokenKey = null;
    }

    // ===== 추후 방향 셋팅용 ===== //
    // ===== 다른 곳에서 사용 예시: hexTile.SetCharacterTokenDirection(CharacterTokenDirection.UpLeft); ===== //
    public void SetCharacterTokenDirection(CharacterTokenDirection direction)
    {
        float[] angles = { 0f, 60f, 120f, 180f, 240f, 300f };
        int index = Mathf.Clamp((int)direction, 0, 5);
        imgCharacter.rectTransform.rotation = Quaternion.Euler(0, 0, -angles[index]);  //시계방향 회전
    }

    private void ShowDecorations(bool isShow)
    {
        outline.gameObject.SetActive(isShow);
        direction.gameObject.SetActive(isShow);
    }
}

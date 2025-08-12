using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static EnumClass;
using Cysharp.Threading.Tasks;

public class HexTile : MonoBehaviour
{
    [SerializeField] Image imgCharacter, outline, direction;
    [SerializeField] GameObject cost;
    [SerializeField] TextMeshProUGUI txtTier, txtCost;

    public (int col, int row) GridPosition { get; private set; }
    public Nullable<int> AssignedTokenKey { get; private set; } = null;
    public CharacterToken AssignedToken { get; private set; }
    public bool IsMyToken { get; private set; }  //캐릭터 토큰이 내 것인지

    private const float DIR_OFFSET_DEG = -120f;  //회전 보정값
    private const float DIR_STEP = 60f;          //육각 6방향

    //헥스 타일 초기화
    public void Init((int, int) pos)
    {
        ShowDecorations(false);
        GridPosition = pos;
        SetCost(false);
    }

    //토큰 할당
    public void AssignToken(int tokenKey, CharacterToken token, bool isEditorMode, bool isMyToken)
    {
        ShowDecorations(true);
        AssignedTokenKey = tokenKey;
        AssignedToken = token;
        SetCost(isEditorMode, token.Cost);
        IsMyToken = isMyToken;
    }

    //토큰 제거
    public void ClearToken()
    {
        ShowDecorations(false);
        AssignedTokenKey = null;
        AssignedToken = null;
        SetCost(false);
        IsMyToken = false;
    }

    //장식 요소 표시/숨김 (아웃라인, 방향 표시)
    private void ShowDecorations(bool isShow)
    {
        outline.gameObject.SetActive(isShow);
        direction.gameObject.SetActive(isShow);
    }

    //헥스 타일 색상 설정
    public void SetColor(Color color)
    {
        imgCharacter.color = color;
    }

    //티어별 텍스트를 표시
    public void DisplayTierText(string txt)
    {
        txtTier.text = txt;
    }

    //캐릭터 토큰의 Cost 설정
    private void SetCost(bool isActive, int cost = 0)
    {
        this.cost.SetActive(isActive);
        txtCost.text = cost.ToString();
    }

    //텍스트의 폰트 크기를 설정
    public void SetFontSize(float tierFontSize, float costFontSize)
    {
        txtTier.fontSize = tierFontSize;
        txtCost.fontSize = costFontSize;
    }

    //이동 방향으로 부드러운 선회
    public async UniTask RotateAnimationAsync(CharacterTokenDirection direction, float duration)
    {
        int index = Mathf.Clamp((int)direction, 0, 5);
        float targetZ = IndexToAngle(index);
        var rt = imgCharacter.rectTransform;

        float startZ = rt.localEulerAngles.z;
        float t = 0f;
        float dur = Mathf.Max(0.0001f, duration);
        while (t < 1f)
        {
            t += Time.deltaTime / dur;
            float z = Mathf.LerpAngle(startZ, targetZ, Mathf.Clamp01(t));
            ApplyRotationZ(z);
            await UniTask.Yield(PlayerLoopTiming.Update);
        }
        ApplyRotationZ(targetZ);
    }

    //즉시 방향 고정 (도착 후 되돌림 방지용)
    public void SetCharacterTokenDirection(CharacterTokenDirection direction)
    {
        int index = Mathf.Clamp((int)direction, 0, 5);
        ApplyRotationZ(IndexToAngle(index));
    }

    //인덱스 → 각도로 변환
    private float IndexToAngle(int index) => DIR_OFFSET_DEG + index * DIR_STEP;

    //회전 적용
    private void ApplyRotationZ(float z) => imgCharacter.rectTransform.localRotation = Quaternion.Euler(0, 0, z);  
}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GridManager : Singleton<GridManager>
{
    private float hexWidth;  //육각형의 가로 크기, 해당 프리팹의 기본 크기 70
    private float hexHeight;  //육각형의 세로 크기 (정육각형 기준으로 √3/2 * 너비), 해당 프리팹의 기본 크기 60

    private readonly int rows = 8;  //행 (가로)
    private readonly int columns = 9;  //열 (세로)

    private readonly Dictionary<(int col, int row), string> txtMap = new()
    {
        //행, 열
        { (8, 7), "H" }, { (8, 6), "L" },
        { (7, 6), "M" },
        { (6, 7), "H" }, { (6, 6), "L" },
        { (5, 6), "M" },
        { (4, 7), "C" }, { (4, 6), "L" },
        { (3, 6), "M" },
        { (2, 7), "H" }, { (2, 6), "L" },
        { (1, 6), "M" },
        { (0, 7), "H" }, { (0, 6), "L" },
    };

    private readonly Dictionary<(int col, int row), Image> imgCharacterMap = new();
    private readonly Dictionary<char, List<(int col, int row)>> tierSlots = new();
    private readonly Dictionary<int, (int col, int row)> tokenPosMap = new();  //토큰의 고유 Key(id)를 좌표와 매핑

    public void CreateHexGrid(RectTransform battleFieldRt, GameObject hexPrefab, RectTransform parant)
    {
        ResizeHexGrid(battleFieldRt);

        float gridWidth = (columns - 1) * hexWidth * 0.75f + hexWidth;
        float gridHeight = rows * hexHeight + (columns > 1 ? hexHeight / 2f : 0);

        Vector2 startOffset = new Vector2(
            -gridWidth / 2f + hexWidth / 2f,
            gridHeight / 2f - hexHeight / 2f
        );

        List<RectTransform> hexTransforms = new List<RectTransform>();

        //그리드를 생성하면서 RectTransform을 리스트에 저장
        for (int col = 0; col < columns; col++)
        {
            int maxRow = rows - ((col % 2 == 1) ? 1 : 0);

            for (int row = 0; row < maxRow; row++)
            {
                GameObject hex = Object.Instantiate(hexPrefab, parant);
                RectTransform rt = hex.GetComponent<RectTransform>();
                hexTransforms.Add(rt);

                var widthScaleOffset = 2 * battleFieldRt.localScale.x;
                var heightScaleOffset = 0.25f * battleFieldRt.localScale.y;
                rt.sizeDelta = new Vector2(hexWidth - widthScaleOffset, hexHeight - heightScaleOffset);

                float x = col * hexWidth * 0.75f;
                float y = row * hexHeight + ((col % 2 == 1) ? hexHeight / 2f : 0f);

                rt.anchoredPosition = new Vector2(x + startOffset.x, -(y - startOffset.y));

                if (txtMap.TryGetValue((col, row), out string textValue))
                {
                    var textComponent = hex.GetComponentInChildren<TextMeshProUGUI>();
                    if (textComponent != null) textComponent.text = textValue;
                    Debug.Log($"열 {col}, 행 {row} : {textValue}");
                }

                //1. Hex에 Mask 추가
                //2. Hex 하위에 캐릭터용 이미지를 생성
                //3. 좌표로 이미지 매핑 등록
                hex.AddComponent<Mask>();
                GameObject imgCharacter = new GameObject("imgCharacter", typeof(RectTransform), typeof(Image));
                imgCharacter.transform.SetParent(hex.transform, false);
                imgCharacter.GetComponent<RectTransform>().sizeDelta = rt.sizeDelta;
                var image = imgCharacter.GetComponent<Image>();
                image.enabled = false;
                imgCharacterMap[(col, row)] = image;
            }
        }

        FitHexGrid(hexTransforms, parant);
        BuildTierSlotTable();  //마지막에 호출
    }

    private void ResizeHexGrid(RectTransform battleFieldRt)
    {
        float availableWidth = battleFieldRt.rect.width;
        float availableHeight = battleFieldRt.rect.height;

        //기준 hex 크기 (임의 초기값)
        float baseHexWidth = 72f;
        float baseHexHeight = baseHexWidth * Mathf.Sqrt(3) / 2f;  //정육각형 기준 비율

        //Grid 전체 너비/높이를 초기 크기로 계산
        float rawTotalWidth = (columns - 1) * baseHexWidth * 0.75f + baseHexWidth;
        float rawTotalHeight = rows * baseHexHeight + (columns > 1 ? baseHexHeight / 2f : 0);

        //실제 크기에 맞게 스케일 조정
        float scaleX = availableWidth / rawTotalWidth;
        float scaleY = availableHeight / rawTotalHeight;
        float scaleFactor = Mathf.Min(scaleX, scaleY);  //너무 커지지 않도록 작은 값 선택

        //최종 적용 hex 크기
        hexWidth = baseHexWidth * scaleFactor;
        hexHeight = baseHexHeight * scaleFactor;
    }

    private void FitHexGrid(List<RectTransform> hexTransforms, RectTransform parant)
    {
        //모든 Hex 오브젝트의 중앙값을 구하기
        Vector2 totalCenter = Vector2.zero;
        foreach (var rt in hexTransforms) totalCenter += rt.anchoredPosition;
        totalCenter /= hexTransforms.Count;

        //부모 RectTransform의 중앙에 맞추기
        Vector2 parentCenter = parant.rect.center;
        Vector2 offset = parentCenter - totalCenter;

        //그리드의 각 위치를 중앙에 맞게 조정
        foreach (var rt in hexTransforms) rt.anchoredPosition += offset;
    }

    // ================================================================================================================ //

    public void DisplayCharacterTokenOnBattlefield(CharacterToken token)
    {
        //토큰 Tier 글자 추출 (H, L, C ...)
        char tier = token.Tier.ToString()[0];

        //이미 배치되어 있는 Token이면 토큰 해제
        if (tokenPosMap.TryGetValue(token.Key, out var curPos))
        {
            ClearToken(curPos);
            tokenPosMap.Remove(token.Key);
            Debug.Log($"토큰 해제 좌표: {curPos} Tier: {tier}");
            return;
        }

        //배치될 슬롯 리스트
        var slots = tierSlots[tier];

        //리더면 무조건 첫 칸
        if (tier == 'C')
        {
            PlaceToken(slots[0], token);
            return;
        }

        //빈 칸 찾기
        foreach (var pos in slots)
        {
            if (imgCharacterMap[pos].sprite == null)
            {
                PlaceToken(pos, token);
                return;
            }
        }

        //다 찬 경우 → 첫 칸 덮어쓰기
        PlaceToken(slots[0], token);
    }

    private void BuildTierSlotTable()
    {
        tierSlots.Clear();

        //Tier 글자에 해당하는 좌표 찾기
        foreach (var kvp in txtMap)
        {
            //토큰 Tier 글자 추출 (H, L, C ...)
            char tier = kvp.Value[0];
            if (!tierSlots.TryGetValue(tier, out var list))
            {
                list = new List<(int, int)>();
                tierSlots[tier] = list;
            }

            list.Add(kvp.Key);
        }

        //왼쪽 → 오른쪽, 같은 열이면 아래쪽(행 큰 값) 우선
        foreach (var list in tierSlots.Values)
            list.Sort((a, b) =>
                a.col != b.col ? a.col.CompareTo(b.col)
                               : b.row.CompareTo(a.row));
    }

    private void PlaceToken((int col, int row) pos, CharacterToken token)
    {
        var img = imgCharacterMap[pos];

        //다른 토큰이 이미 이 칸을 쓰고 있었다면 기록 제거
        foreach (var kvp in tokenPosMap)
            if (kvp.Value == pos)
            {
                tokenPosMap.Remove(kvp.Key);
                break;
            }

        //해당 좌표의 이미지에 캐릭터 이미지 할당
        img.sprite = token.GetCharacterSprite();
        img.enabled = true;
        tokenPosMap[token.Key] = pos;
        Debug.Log($"토큰 배치 좌표: {tokenPosMap[token.Key]} Tier: {token.Tier}");
    }

    private void ClearToken((int col, int row) pos)
    {
        var img = imgCharacterMap[pos];
        img.sprite = null;
        img.enabled = false;
    }
}
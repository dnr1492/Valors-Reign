using Coffee.UISoftMask;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    private readonly Dictionary<(int col, int row), Image> imgCharacterMap = new();  //좌표와 해당 좌표의 Image 매핑
    private readonly Dictionary<char, List<(int col, int row)>> tierSlots = new();  //티어와 해당 좌표 매핑
    private readonly Dictionary<int, (int col, int row)> tokenPosMap = new();  //토큰의 고유 Key(id)를 좌표와 매핑
    private readonly Dictionary<(int col, int row), HexTile> hexTileMap = new();  //좌표와 해당 좌표의 HexTile 매핑

    #region 필드 생성
    public void CreateHexGrid(RectTransform battleFieldRt, GameObject hexPrefab, RectTransform parant)
    {
        ResizeHexGrid(battleFieldRt);

        float gridWidth = (columns - 1) * hexWidth * 0.75f + hexWidth;
        float gridHeight = rows * hexHeight + (columns > 1 ? hexHeight / 2f : 0);

        Vector2 startOffset = new(
            -gridWidth / 2f + hexWidth / 2f,
            gridHeight / 2f - hexHeight / 2f
        );

        List<RectTransform> hexTransforms = new();

        //그리드를 생성하면서 RectTransform을 리스트에 저장
        for (int col = 0; col < columns; col++)
        {
            int maxRow = rows - ((col % 2 == 1) ? 1 : 0);

            for (int row = 0; row < maxRow; row++)
            {
                GameObject hex = Instantiate(hexPrefab, parant);
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
                }

                var hexTile = hex.GetComponent<HexTile>();
                hexTile.Init((col, row));
                hexTileMap[(col, row)] = hexTile;

                //좌표로 이미지 매핑 등록
                var image = hexTile.transform.Find("mask").transform.Find("imgCharacter").GetComponent<Image>();
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
    #endregion

    #region 필드 위에 선택한 캐릭터 토큰 표시 및 표시 제거
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
            PlaceToken(slots[0], token.Key, token.GetCharacterSprite());
            return;
        }

        //빈 칸 찾기
        foreach (var pos in slots)
        {
            if (imgCharacterMap[pos].sprite == null)
            {
                PlaceToken(pos, token.Key, token.GetCharacterSprite());
                return;
            }
        }

        //다 찬 경우 → 첫 칸 덮어쓰기
        PlaceToken(slots[0], token.Key, token.GetCharacterSprite());
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

    private void PlaceToken((int col, int row) pos, int tokenKey, Sprite sprite)
    {
        if (!hexTileMap.TryGetValue(pos, out var hexTile)) return;

        //기존 토큰 제거
        foreach (var kvp in tokenPosMap.ToList())
        {
            if (kvp.Value == pos)
            {
                tokenPosMap.Remove(kvp.Key);
                hexTile.ClearToken();
                break;
            }
        }

        //이미지 설정
        imgCharacterMap[pos].sprite = sprite;
        imgCharacterMap[pos].enabled = true;

        //HexTile에 해당 토큰 Key값 설정
        hexTile.AssignToken(tokenKey);

        //위치 기록
        tokenPosMap[tokenKey] = pos;
    }

    private void ClearToken((int col, int row) pos)
    {
        if (!hexTileMap.TryGetValue(pos, out var hexTile)) return;

        var image = imgCharacterMap[pos];
        image.sprite = null;
        image.enabled = false;

        hexTile.ClearToken();
    }
    #endregion

    /// <summary>
    /// (슬롯(Hex)과 마우스 간의) 거리 기반 탐색
    /// </summary>
    /// <param name="screenPos"></param>
    /// <param name="nearestSlot"></param>
    /// <returns></returns>
    public bool TryGetNearestSlot(Vector2 screenPos, out (int col, int row) nearestSlot)
    {
        nearestSlot = default;
        float minDistance = float.MaxValue;
        bool found = false;

        foreach (var kvp in imgCharacterMap)
        {
            var rt = kvp.Value.rectTransform;
            Vector2 screenWorldPos = rt.position;
            float distance = Vector2.Distance(screenWorldPos, screenPos);

            if (distance < minDistance) {
                minDistance = distance;
                nearestSlot = kvp.Key;
                found = true;
            }
        }

        return found && minDistance <= 100f;
    }

    /// <summary>
    /// 토큰을 이동
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="tile"></param>
    public void MoveToken((int col, int row) from, (int col, int row) to, HexTile tile)
    {
        if (from == to || !tile.AssignedTokenKey.HasValue) return;

        int fromKey = tile.AssignedTokenKey.Value;
        if (!txtMap.TryGetValue(to, out string dropSlotType)) return;

        char dropSlotChar = dropSlotType[0];
        var fromToken = ControllerRegister.Get<CharacterTokenController>().GetAllCharacterToken()
            .FirstOrDefault(t => t.Key == fromKey);

        if (fromToken == null || fromToken.Tier.ToString()[0] != dropSlotChar)
        {
            //드롭 위치가 동일한 티어의 슬롯이 아니면 복귀 (복사본만 삭제)
            return;
        }

        var fromImg = imgCharacterMap[from];
        var toImg = imgCharacterMap[to];
        var fromTile = hexTileMap[from];
        var toTile = hexTileMap[to];

        //빈 칸으로 이동
        if (!toTile.AssignedTokenKey.HasValue)
        {
            tokenPosMap[fromKey] = to;
            fromTile.ClearToken();
            toTile.AssignToken(fromKey);

            toImg.sprite = fromImg.sprite;
            toImg.enabled = true;
            fromImg.sprite = null;
            fromImg.enabled = false;
            return;
        }

        int toKey = toTile.AssignedTokenKey.Value;
        var toToken = ControllerRegister.Get<CharacterTokenController>().GetAllCharacterToken()
            .FirstOrDefault(t => t.Key == toKey);

        if (fromToken.Tier == toToken?.Tier)
        {
            tokenPosMap[fromKey] = to;
            tokenPosMap[toKey] = from;

            fromTile.AssignToken(toKey);
            toTile.AssignToken(fromKey);

            (fromImg.sprite, toImg.sprite) = (toImg.sprite, fromImg.sprite);
            fromImg.enabled = fromImg.sprite != null;
            toImg.enabled = toImg.sprite != null;
        }
        else
        {
            tile.transform.SetParent(fromTile.transform, false);
            tile.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        }
    }

    /// <summary>
    /// TokenPack으로 묶기
    /// </summary>
    /// <returns></returns>
    public TokenPack GetTokenPack()
    {
        var tokenPack = new TokenPack();

        foreach (var kvp in tokenPosMap)
        {
            var key = kvp.Key;
            var (col, row) = kvp.Value;
            tokenPack.tokenSlots.Add(new TokenSlotData { tokenKey = key, col = col, row = row });
        }

        return tokenPack;
    }

    #region (임시로 로컬에서) TokenPack 로드
    public void LoadTokenPack()
    {
        TokenPack tokenPack = Load();

        var allTokens = ControllerRegister.Get<CharacterTokenController>().GetAllCharacterToken();

        foreach (var slot in tokenPack.tokenSlots)
        {
            var token = allTokens.FirstOrDefault(t => t.Key == slot.tokenKey);
            if (token == null) continue;

            PlaceToken((slot.col, slot.row), token.Key, token.GetCharacterSprite());  //위치 지정하여 강제 배치
            token.Select();  //선택 상태도 복원
        }
    }

    private TokenPack Load()
    {
        string savePath = Application.persistentDataPath + "/token_pack.json";

        if (!System.IO.File.Exists(savePath))
        {
            Debug.LogWarning("저장된 TokenPack 없음");
            return null;
        }

        string json = System.IO.File.ReadAllText(savePath);
        TokenPack tokenPack = JsonUtility.FromJson<TokenPack>(json);
        return tokenPack;
    }
    #endregion

    /// <summary>
    /// (임시로 로컬에) TokenPack 저장
    /// </summary>
    /// <param name="tokenPack"></param>
    public void SaveTokenPack(TokenPack tokenPack)
    {
        string savePath = Application.persistentDataPath + "/token_pack.json";
        string json = JsonUtility.ToJson(tokenPack, true);
        System.IO.File.WriteAllText(savePath, json);
        Debug.Log($"TokenPack 저장 완료: {savePath}");
    }

    /// <summary>
    /// UIDeckPhase2 Popup 초기화
    /// </summary>
    public void ResetUIDeckPhase2()
    {
        //모든 토큰 이미지 제거
        foreach (var img in imgCharacterMap.Values) {
            img.sprite = null;
            img.enabled = false;
        }

        //모든 HexTile 초기화
        foreach (var tile in hexTileMap.Values)
            tile.ClearToken();

        //위치 기록 제거
        tokenPosMap.Clear();

        //선택 해제
        var tokens = ControllerRegister.Get<CharacterTokenController>().GetAllCharacterToken();
        foreach (var token in tokens)
            if (token.IsSelect) token.Unselect();

        //필터 초기화
        ControllerRegister.Get<FilterController>().ResetFilter();
    }
}
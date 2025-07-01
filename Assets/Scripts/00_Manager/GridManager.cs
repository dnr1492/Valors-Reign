using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static EnumClass;

public class GridManager : Singleton<GridManager>
{
    private float hexWidth;  //육각형의 가로 크기, 해당 육각형의 기본 크기 70
    private float hexHeight;  //육각형의 세로 크기 (육각형의 높이는 가로*√3/2), 해당 육각형의 기본 크기 60

    private readonly int rows = 8;  //행 (세로)
    private readonly int columns = 9;  //열 (가로)

    private readonly Dictionary<(int col, int row), string> txtMap = new()
    {
        //열, 행
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

    private readonly Dictionary<(int col, int row), Image> imgCharacterMap = new();  //좌표에 해당하는 좌표의 Image 컴포넌트
    private readonly Dictionary<char, List<(int col, int row)>> tierSlots = new();  //티어에 해당하는 좌표 리스트
    private readonly Dictionary<int, (int col, int row)> tokenPosMap = new();  //토큰의 고유 Key(id)와 좌표를 매핑
    private readonly Dictionary<(int col, int row), HexTile> hexTileMap = new();  //좌표에 해당하는 좌표의 HexTile 컴포넌트

    #region 그리드 생성
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

        //육각형을 생성하면서 RectTransform을 리스트에 추가
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

                //좌표의 이미지 컴포넌트 참조
                var image = hexTile.transform.Find("mask").transform.Find("imgCharacter").GetComponent<Image>();
                image.enabled = false;
                imgCharacterMap[(col, row)] = image;
            }
        }

        FitHexGrid(hexTransforms, parant);
        BuildTierSlotTable();  //티어 슬롯 테이블 호출
    }

    private void ResizeHexGrid(RectTransform battleFieldRt)
    {
        float availableWidth = battleFieldRt.rect.width;
        float availableHeight = battleFieldRt.rect.height;

        //기본 hex 크기 (기본 초기값)
        float baseHexWidth = 72f;
        float baseHexHeight = baseHexWidth * Mathf.Sqrt(3) / 2f;  //육각형의 높이 공식

        //Grid 전체 가로/세로를 기본 크기로 계산
        float rawTotalWidth = (columns - 1) * baseHexWidth * 0.75f + baseHexWidth;
        float rawTotalHeight = rows * baseHexHeight + (columns > 1 ? baseHexHeight / 2f : 0);

        //사용 가능한 크기에 맞춰 스케일 계산
        float scaleX = availableWidth / rawTotalWidth;
        float scaleY = availableHeight / rawTotalHeight;
        float scaleFactor = Mathf.Min(scaleX, scaleY);  //더 작은 스케일을 사용하여 비율 유지

        //최종 계산된 hex 크기
        hexWidth = baseHexWidth * scaleFactor;
        hexHeight = baseHexHeight * scaleFactor;
    }

    private void FitHexGrid(List<RectTransform> hexTransforms, RectTransform parant)
    {
        //모든 Hex 트랜스폼의 중심점 계산
        Vector2 totalCenter = Vector2.zero;
        foreach (var rt in hexTransforms) totalCenter += rt.anchoredPosition;
        totalCenter /= hexTransforms.Count;

        //부모 RectTransform의 중심점 계산
        Vector2 parentCenter = parant.rect.center;
        Vector2 offset = parentCenter - totalCenter;

        //육각형들을 부모의 중심에 맞춰 이동
        foreach (var rt in hexTransforms) rt.anchoredPosition += offset;
    }
    #endregion

    #region 캐릭터 토큰을 전장에 표시
    public void DisplayTokenOnBattlefield(CharacterToken token)
    {
        //토큰 Tier 문자 추출 (H, L, C ...)
        char tier = token.Tier.ToString()[0];

        //해당하는 좌표 리스트
        var slots = tierSlots[tier];

        //캡틴은 중앙 첫 칸
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

        //빈 칸이 없으면 첫 칸에 배치
        PlaceToken(slots[0], token.Key, token.GetCharacterSprite());
    }

    private void BuildTierSlotTable()
    {
        tierSlots.Clear();

        //Tier 문자와 해당하는 좌표 찾기
        foreach (var kvp in txtMap)
        {
            //토큰 Tier 문자 추출 (H, L, C ...)
            char tier = kvp.Value[0];
            if (!tierSlots.TryGetValue(tier, out var list))
            {
                list = new List<(int, int)>();
                tierSlots[tier] = list;
            }

            list.Add(kvp.Key);
        }

        //각 리스트를 정렬, 같은 열에서는 아래쪽(행이 큰 쪽) 우선
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

        //HexTile에 해당 토큰 Key를 할당
        hexTile.AssignToken(tokenKey);

        //위치 매핑
        tokenPosMap[tokenKey] = pos;
    }
    #endregion

    #region 캐릭터 토큰을 전장에서 제거
    public void RemoveTokenFromBattlefield(CharacterToken token)
    {
        //이미 배치되어 있는 Token이면 토큰 제거
        if (tokenPosMap.TryGetValue(token.Key, out var curPos))
        {
            ClearToken(curPos);
            tokenPosMap.Remove(token.Key);
            Debug.Log($"토큰 제거 좌표: {curPos} Tier: {token.Tier}");
            return;
        }
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
    /// (마우스(Hex)의 마우스 위치) 가장 가까운 슬롯 검색
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
            //드롭 위치의 티어와 토큰의 티어가 다르면 이동 (이동 불가)
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
    /// DeckPack 생성
    /// </summary>
    /// <param name="deckName">덱 이름</param>
    /// <returns></returns>
    public DeckPack CreateDeckPack(string deckName)
    {
        var DeckPack = new DeckPack();
        DeckPack.deckName = deckName;
        DeckPack.race = ControllerRegister.Get<FilterController>().GetSelectedRace();

        var allTokens = ControllerRegister.Get<CharacterTokenController>().GetAllCharacterToken();
        foreach (var kvp in tokenPosMap)
        {
            var key = kvp.Key;
            var (col, row) = kvp.Value;

            var token = allTokens.FirstOrDefault(t => t.Key == key);
            if (token == null) continue;

            var slotData = new TokenSlotData
            {
                tokenKey = key,
                col = col,
                row = row,
                skillCounts = new()
            };

            foreach (var kv in token.GetAllSkillCounts())
                slotData.skillCounts.Add(new SkillCountData { skillId = kv.Key, count = kv.Value });

            DeckPack.tokenSlots.Add(slotData);
        }

        return DeckPack;
    }

    /// <summary>
    /// DeckPack 적용
    /// </summary>
    /// <param name="deckPack"></param>
    /// <param name="characterCard"></param>
    public void ApplyDeckPack(DeckPack deckPack, CharacterCard characterCard)
    {
        ControllerRegister.Get<FilterController>().InitFilter(deckPack.race);

        //모든 토큰 상태 초기화 (Select/Confirm -> Cancel)
        var allTokens = ControllerRegister.Get<CharacterTokenController>().GetAllCharacterToken();
        foreach (var token in allTokens)
        {
            if (token.State != CharacterTokenState.Cancel)
                token.SetTokenState(CharacterTokenState.Cancel);
        }

        //저장된 위치에 Confirm 상태로 배치
        foreach (var slot in deckPack.tokenSlots)
        {
            var token = allTokens.FirstOrDefault(t => t.Key == slot.tokenKey);
            if (token == null) continue;

            PlaceToken((slot.col, slot.row), token.Key, token.GetCharacterSprite());  //위치 계산해서 배치
            token.SetTokenState(CharacterTokenState.Confirm);  //상태를 Confirm으로 변경

            //스킬 개수 복원
            if (slot.skillCounts != null)
            {
                foreach (var skill in slot.skillCounts)
                    token.SetSkillCount(skill.skillId, skill.count);
            }
        }

        //캐릭터 카드 숨김상태로 변경
        characterCard.SetCardState(CardState.Back);
    }

    /// <summary>
    /// UIDeckPhase3 Popup 초기화
    /// </summary>
    public void ResetUIDeckPhase3(CharacterCard characterCard)
    {
        //모든 토큰 이미지 제거
        foreach (var img in imgCharacterMap.Values)
        {
            img.sprite = null;
            img.enabled = false;
        }

        //모든 HexTile 초기화
        foreach (var tile in hexTileMap.Values)
            tile.ClearToken();

        //위치 매핑 초기화
        tokenPosMap.Clear();

        //모든 토큰 상태 초기화
        var tokens = ControllerRegister.Get<CharacterTokenController>().GetAllCharacterToken();
        foreach (var token in tokens)
        {
            if (token.State != CharacterTokenState.Cancel)
                token.SetTokenState(CharacterTokenState.Cancel);

            //스킬 개수 초기화
            foreach (var skillId in DataManager.Instance.dicCharacterCardData[token.Key].skills)
                token.SetSkillCount(skillId, 0);
        }

        ////필터 초기화
        //ControllerRegister.Get<FilterController>().ResetFilter();

        //캐릭터 카드 숨김상태로 변경
        characterCard.SetCardState(CardState.Back);
    }
}
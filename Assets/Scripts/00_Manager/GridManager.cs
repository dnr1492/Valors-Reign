using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static EnumClass;

public class GridManager : Singleton<GridManager>
{
    private RectTransform battleFieldRt;
    private bool isEditorMode;

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
        { (4, 7), "B" }, { (4, 6), "L" },
        { (3, 6), "M" },
        { (2, 7), "H" }, { (2, 6), "L" },
        { (1, 6), "M" },
        { (0, 7), "H" }, { (0, 6), "L" },
    };

    private readonly Dictionary<(int col, int row), Image> imgCharacterMap = new();  //좌표에 해당하는 좌표의 Image 컴포넌트
    private readonly Dictionary<char, List<(int col, int row)>> tierSlots = new();  //티어에 해당하는 좌표 리스트
    private readonly Dictionary<int, (int col, int row)> tokenPosMap = new();  //토큰의 고유 Key(id)와 좌표를 매핑
    private readonly Dictionary<(int col, int row), HexTile> hexTileMap = new();  //좌표에 해당하는 좌표의 HexTile 컴포넌트

    [Header("필드의 Width에 맞게 Tier/Cost FontSize 동적 설정")]
    private const float BASE_WIDTH = 550f;
    private const float BASE_TIER_FONT = 45f;
    private const float BASE_COST_FONT = 13f;
    private float tierFontSize;
    private float costFontSize;

    [Header("Highlight")]
    private readonly Color HL_SELECT = new Color(0.2f, 1f, 0.2f, 1f);     //하이라이트 색상 초록
    private readonly Color HL_RESERVED = new Color(1f, 0.95f, 0.6f, 1f);  //하이라이트 색상 연노랑
    private readonly Color HL_CANDID = new Color(1f, 0.2f, 0.2f, 1f);     //하이라이트 색상 빨강
    private readonly List<OutlineSnapshot> selectedSnaps = new();  //현재 프레임에 적용된 하이라이트 기록
    private readonly List<OutlineSnapshot> reservedSnaps = new();  //현재 프레임에 적용된 하이라이트 기록
    private readonly List<OutlineSnapshot> candidSnaps = new();    //현재 프레임에 적용된 하이라이트 기록

    private struct OutlineSnapshot  //원복을 위한 스냅샷
    {
        public Image img;
        public Color origColor;
        public bool origActive;  //GameObject 활성 상태를 저장
    }

    [Header("이동 애니메이션")]
    private RectTransform tokenLayer;
    private float moveAnimDuration = 0.25f;  //한 칸 이동 시간(초)
    private AnimationCurve moveEase = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);  //가감속 곡선

    protected override void Awake()
    {
        base.Awake();
    }

    #region 그리드 생성
    public void CreateHexGrid(RectTransform battleFieldRt, GameObject hexPrefab, RectTransform parant, bool isEditorMode, bool enableTierTextForOpponent)
    {
        this.battleFieldRt = battleFieldRt;
        this.isEditorMode = isEditorMode;

        //초기화
        imgCharacterMap.Clear();
        tokenPosMap.Clear();
        hexTileMap.Clear();
        foreach (Transform child in parant) Destroy(child.gameObject);

        if (enableTierTextForOpponent) DuplicateTierTextForOpponent();

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
                var hexEditorEvent = hex.GetComponent<HexTileEditorEvent>();
                var hexBattleEvent = hex.GetComponent<HexTileBattleEvent>();
                if (!isEditorMode) {
                    hexEditorEvent.enabled = false;
                    hexBattleEvent.enabled = true;
                }
                else {
                    hexEditorEvent.enabled = true;
                    hexBattleEvent.enabled = false;
                }
                RectTransform rt = hex.GetComponent<RectTransform>();
                hexTransforms.Add(rt);

                var widthScaleOffset = 2 * battleFieldRt.localScale.x;
                var heightScaleOffset = 0.25f * battleFieldRt.localScale.y;
                rt.sizeDelta = new Vector2(hexWidth - widthScaleOffset, hexHeight - heightScaleOffset);

                float x = col * hexWidth * 0.75f;
                float y = row * hexHeight + ((col % 2 == 1) ? hexHeight / 2f : 0f);

                rt.anchoredPosition = new Vector2(x + startOffset.x, -(y - startOffset.y));

                var hexTile = hex.GetComponent<HexTile>();
                hexTile.Init((col, row));
                hexTileMap[(col, row)] = hexTile;

                //TierText 표시
                if (txtMap.TryGetValue((col, row), out string textValue)) {
                    hexTile.DisplayTierText(textValue);
                    hexTile.SetFontSize(tierFontSize, costFontSize);
                }

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

        //폰트 크기 자동 계산
        float widthRatio = availableWidth / BASE_WIDTH;
        tierFontSize = BASE_TIER_FONT * widthRatio;
        costFontSize = BASE_COST_FONT * widthRatio;
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
        //토큰 Tier 문자 추출 (H, L, B ...)
        char tier = token.Tier.ToString()[0];

        //해당하는 좌표 리스트
        var slots = tierSlots[tier];

        //캡틴은 중앙 첫 칸
        if (tier == 'B')
        {
            PlaceToken(slots[0], token.Key, token.GetCharacterSprite(), true);
            return;
        }

        //빈 칸 찾기
        foreach (var pos in slots)
        {
            if (imgCharacterMap[pos].sprite == null)
            {
                PlaceToken(pos, token.Key, token.GetCharacterSprite(), true);
                return;
            }
        }

        //빈 칸이 없으면 첫 칸에 배치
        PlaceToken(slots[0], token.Key, token.GetCharacterSprite(), true);
    }

    private void BuildTierSlotTable()
    {
        tierSlots.Clear();

        //Tier 문자와 해당하는 좌표 찾기
        foreach (var kvp in txtMap)
        {
            //토큰 Tier 문자 추출 (H, L, B ...)
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

    private void PlaceToken((int col, int row) pos, int tokenKey, Sprite sprite, bool isMyToken)
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

        //HexTile에 할당
        var token = ControllerRegister.Get<CharacterTokenController>().GetAllCharacterToken()
            .FirstOrDefault(t => t.Key == tokenKey);
        hexTile.AssignToken(tokenKey, token, isEditorMode, isMyToken);

        //내 토큰만 위치 매핑 (적은 기록하지 않음 → key 충돌 제거)
        if (isMyToken) tokenPosMap[tokenKey] = pos;
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
    public bool MoveTokenEditor((int col, int row) from, (int col, int row) to, HexTile tile)
    {
        if (from == to || !tile.AssignedTokenKey.HasValue) return false;

        int fromKey = tile.AssignedTokenKey.Value;

        if (!txtMap.TryGetValue(to, out string dropSlotType)) return false;
        char dropSlotChar = dropSlotType[0];

        var allTokens = ControllerRegister.Get<CharacterTokenController>().GetAllCharacterToken();
        var fromToken = allTokens.FirstOrDefault(t => t.Key == fromKey);
        if (fromToken == null || fromToken.Tier.ToString()[0] != dropSlotChar) return false;

        var fromImg = imgCharacterMap[from];
        var toImg = imgCharacterMap[to];
        var fromTile = hexTileMap[from];
        var toTile = hexTileMap[to];

        //빈 칸으로 이동
        if (!toTile.AssignedTokenKey.HasValue)
        {
            tokenPosMap[fromKey] = to;
            fromTile.ClearToken();
            toTile.AssignToken(fromKey, fromToken, isEditorMode, true);

            toImg.sprite = fromImg.sprite;
            toImg.enabled = true;
            fromImg.sprite = null;
            fromImg.enabled = false;
            return true;
        }

        //자리 교환
        int toKey = toTile.AssignedTokenKey.Value;
        var toToken = allTokens.FirstOrDefault(t => t.Key == toKey);
        if (toToken == null) return false;

        if (fromToken.Tier == toToken.Tier)
        {
            tokenPosMap[fromKey] = to;
            tokenPosMap[toKey] = from;

            fromTile.AssignToken(toKey, toToken, isEditorMode, true);
            toTile.AssignToken(fromKey, fromToken, isEditorMode, true);

            (fromImg.sprite, toImg.sprite) = (toImg.sprite, fromImg.sprite);
            fromImg.enabled = fromImg.sprite != null;
            toImg.enabled = toImg.sprite != null;
            return true;
        }

        //티어 불일치 → 되돌리기
        tile.transform.SetParent(fromTile.transform, false);
        tile.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        return false;
    }

    /// <summary>
    /// DeckPack 생성
    /// </summary>
    /// <param name="deckName">덱 이름</param>
    /// <returns></returns>
    public DeckPack CreateDeckPack(string deckName)
    {
        var phase1Popup = UIManager.Instance.GetPopup<UIEditorDeckPhase1>("UIEditorDeckPhase1");
        var currentDeckPack = phase1Popup?.GetSelectedDeckPack();
        var DeckPack = new DeckPack();
        DeckPack.guid = currentDeckPack?.guid ?? Guid.NewGuid().ToString();  //덱이 이미 존재하는 경우 기존 guid 재사용
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
    /// 덱 구성 화면에 DeckPack을 적용
    /// </summary>
    /// <param name="deckPack"></param>
    public async UniTask ApplyDeckPack(DeckPack deckPack)
    {
        try
        {
            await ControllerRegister.Get<FilterController>().InitFilterAsync(deckPack.race);
        }
        finally
        {
            //모든 토큰 상태 초기화 (Select/Confirm → Cancel)
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
                token.SetCardToBack();  //캐릭터 카드를 숨김 상태로 변경

                PlaceToken((slot.col, slot.row), token.Key, token.GetCharacterSprite(), true);  //위치 계산해서 배치
                token.SetTokenState(CharacterTokenState.Confirm);  //상태를 Confirm으로 변경

                //스킬 개수 복원
                if (slot.skillCounts != null) {
                    foreach (var skill in slot.skillCounts)
                        token.SetSkillCount(skill.skillId, skill.count);
                }
            }
        }
    }

    #region 전장에 내 덱(필수)과 상대 덱(선택)을 표시
    /// <summary>
    /// 필드에 내 덱(필수)과 상대 덱(선택)을 표시
    /// </summary>
    /// <param name="myDeck"></param>
    /// <param name="opponentDeck"></param>
    /// <returns></returns>
    public async void ShowDecksOnField(DeckPack myDeck, DeckPack opponentDeck = null)
    {
        try
        {
            await ControllerRegister.Get<FilterController>().InitFilterAsync(myDeck.race);
        }
        finally
        {
            var allTokens = ControllerRegister.Get<CharacterTokenController>().GetAllCharacterToken();

            //내 덱 표시
            foreach (var slot in myDeck.tokenSlots)
            {
                var token = allTokens.FirstOrDefault(t => t.Key == slot.tokenKey);
                if (token == null) continue;

                PlaceToken((slot.col, slot.row), token.Key, token.GetCharacterSprite(), true);

                //스킬 수량 반영
                if (slot.skillCounts != null) {
                    foreach (var skill in slot.skillCounts) {
                        token.SetSkillCount(skill.skillId, skill.count);
                    }
                }
            }

            //상대 덱 표시
            if (opponentDeck != null)
            {
                foreach (var slot in opponentDeck.tokenSlots)
                {
                    int col = MirrorCol(slot.col);
                    int row = MirrorRow(col, slot.row);  //보정된 col을 넘겨야 시각 offset 일치

                    var token = allTokens.FirstOrDefault(t => t.Key == slot.tokenKey);
                    if (token == null) continue;

                    PlaceToken((col, row), token.Key, token.GetCharacterSprite(), false);

                    //스킬 수량 반영
                    if (slot.skillCounts != null) {
                        foreach (var skill in slot.skillCounts) {
                            token.SetSkillCount(skill.skillId, skill.count);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 상하 대칭
    /// </summary>
    /// <param name="col"></param>
    /// <param name="row"></param>
    /// <returns></returns>
    private int MirrorRow(int col, int row)
    {
        int totalRows = rows;
        int mirroredRow = totalRows - 1 - row;

        //홀수 열(col)은 offset이 반 칸 아래로 내려가 있으므로 보정
        if ((col & 1) == 1) mirroredRow = Mathf.Max(0, mirroredRow - 1);

        return mirroredRow;
    }

    /// <summary>
    /// 좌우 대칭
    /// </summary>
    /// <param name="col"></param>
    /// <returns></returns>
    private int MirrorCol(int col)
    {
        int totalCols = columns;
        return totalCols - 1 - col;
    }

    /// <summary>
    /// 상대 진영에도 동일한 티어 텍스트가 표시되도록 txtMap을 상하좌우 대칭 좌표로 복사 등록함
    /// </summary>
    public void DuplicateTierTextForOpponent()
    {
        var existing = txtMap.ToList();  //복사

        foreach (var ((col, row), label) in existing)
        {
            int mirroredCol = MirrorCol(col);
            int mirroredRow = MirrorRow(col, row);
            txtMap[(mirroredCol, mirroredRow)] = label;
        }
    }
    #endregion

    #region UIDeckPhase3 Popup 초기화
    public void ResetUIDeckPhase3()
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
            if (DataManager.Instance.dicCharacterCardData.TryGetValue(token.Key, out var cardData) 
                && cardData.skills != null 
                && cardData.skills.Count > 0)
            {
                foreach (var skillId in cardData.skills) {
                    token.SetSkillCount(skillId, 0);
                }
            }

            //캐릭터 카드 숨김상태로 변경
            token.SetCardToBack();
        }

        //필터 초기화
        ControllerRegister.Get<FilterController>().ResetFilter();
    }
    #endregion

    #region 필드 정보 조회/판정 ===== TODO: IsPassable는 추가적으로 구현 필요 =====
    //해당 좌표가 필드 범위 안인지
    public bool CellExists(Vector2Int hex) => hexTileMap.ContainsKey((hex.x, hex.y));

    //해당 좌표가 점유되었는지
    public bool IsOccupied(Vector2Int hex) => hexTileMap.TryGetValue((hex.x, hex.y), out var t) && t.AssignedTokenKey.HasValue;

    //통행 가능 여부 ===== TODO: 지형 규칙 생기면 확장 =====
    public bool IsPassable(Vector2Int hex) => true;

    //토큰 ID로 현재 좌표 조회
    public bool TryGetTokenPosition(int tokenKey, out Vector2Int pos)
    {
        if (tokenPosMap.TryGetValue(tokenKey, out var p))
        {
            pos = new Vector2Int(p.col, p.row);
            return true;
        }
        pos = default;
        return false;
    }

    //지정한 좌표에 상대방 토큰이 아닌 '나의 토큰'이 존재하는지 여부 반환
    public bool IsMyTokenAt(Vector2Int hex) => hexTileMap.TryGetValue((hex.x, hex.y), out var tile) && tile.AssignedTokenKey.HasValue && tile.IsMyToken;
    #endregion

    #region 이동 처리 (feat.애니메이션) (feat.이동 방향으로 회전)
    //선회 인덱스 계산 → 선회 → 이동 → 도착 후 동일 방향 고정
    public async UniTask MoveFromToByHexAsync(Vector2Int from, Vector2Int to)
    {
        if (!CanMove(from, to, out var fromTile, out var toTile)) return;

        int dirIdx = ResolveDirectionIndex(from, to);
        if (dirIdx >= 0 && fromTile.AssignedTokenKey.HasValue)
            await fromTile.RotateAnimationAsync((CharacterTokenDirection)dirIdx, 0.15f);

        var fromTileRt = fromTile.GetComponent<RectTransform>();
        var toTileRt = toTile.GetComponent<RectTransform>();
        var fromMaskTf = fromTile.transform.Find("mask");
        var fromOutlineTf = fromTile.transform.Find("outline");
        if (fromMaskTf == null || fromOutlineTf == null) return;

        //도착칸 이미지 잠시 비활성 (중복 노출 방지)
        var toImg = toTile.transform.Find("mask/imgCharacter").GetComponent<Image>();
        if (toImg == null) return;
        toImg.enabled = false;

        EnsureTokenLayer();

        //이동 컨테이너 생성
        var moverGO = new GameObject("HexMover", typeof(RectTransform));
        var moverRT = (RectTransform)moverGO.transform;
        moverRT.SetParent(tokenLayer, false);
        moverRT.position = fromTileRt.position;
        moverRT.SetAsLastSibling();

        var maskOrigParent = fromMaskTf.parent;
        int maskOrigIndex = fromMaskTf.GetSiblingIndex();
        var outlineOrigParent = fromOutlineTf.parent;
        int outlineOrigIndex = fromOutlineTf.GetSiblingIndex();

        fromMaskTf.SetParent(moverRT, true);
        fromOutlineTf.SetParent(moverRT, true);

        //보간 이동
        Vector3 start = fromTileRt.position;
        Vector3 end = toTileRt.position;
        float t = 0f, dur = Mathf.Max(0.0001f, moveAnimDuration);
        while (t < 1f)
        {
            t += Time.deltaTime / dur;
            float e = moveEase != null ? moveEase.Evaluate(t) : t;
            moverRT.position = Vector3.Lerp(start, end, Mathf.Clamp01(e));
            await UniTask.Yield(PlayerLoopTiming.Update);
        }

        MoveTokenBattle((from.x, from.y), (to.x, to.y));

        fromMaskTf.SetParent(maskOrigParent, false);
        fromMaskTf.SetSiblingIndex(maskOrigIndex);
        fromOutlineTf.SetParent(outlineOrigParent, false);
        fromOutlineTf.SetSiblingIndex(outlineOrigIndex);

        var mRt = (RectTransform)fromMaskTf;
        var oRt = (RectTransform)fromOutlineTf;
        mRt.offsetMin = Vector2.zero; mRt.offsetMax = Vector2.zero;
        oRt.offsetMin = Vector2.zero; oRt.offsetMax = Vector2.zero;
        mRt.anchoredPosition = Vector2.zero; oRt.anchoredPosition = Vector2.zero;
        mRt.localScale = Vector3.one; oRt.localScale = Vector3.one;
        mRt.localRotation = Quaternion.identity; oRt.localRotation = Quaternion.identity;

        //이동 후 같은 방향으로 고정 (원위치 회전 방지)
        if (dirIdx >= 0) toTile.SetCharacterTokenDirection((CharacterTokenDirection)dirIdx);

        //정리
        Destroy(moverGO);
        toImg.enabled = true;
    }

    private void MoveTokenBattle((int col, int row) from, (int col, int row) to)
    {
        var fromTile = hexTileMap[(from.col, from.row)];
        var toTile = hexTileMap[(to.col, to.row)];
        int fromKey = fromTile.AssignedTokenKey.Value;

        var tokenCtrl = ControllerRegister.Get<CharacterTokenController>();
        var token = tokenCtrl != null ? tokenCtrl.GetAllCharacterToken().FirstOrDefault(t => t.Key == fromKey) : null;

        tokenPosMap[fromKey] = to;

        fromTile.ClearToken();
        toTile.AssignToken(fromKey, token, false, true);

        var fromImg = imgCharacterMap[from];
        var toImg = imgCharacterMap[to];
        toImg.sprite = fromImg.sprite;
        toImg.enabled = true;
        fromImg.sprite = null;
        fromImg.enabled = false;
    }

    private bool CanMove(Vector2Int from, Vector2Int to, out HexTile fromTile, out HexTile toTile)
    {
        fromTile = null; 
        toTile = null;

        if (from == to) return false;
        if (!hexTileMap.TryGetValue((from.x, from.y), out fromTile)) return false;
        if (!hexTileMap.TryGetValue((to.x, to.y), out toTile)) return false;
        if (!fromTile.AssignedTokenKey.HasValue) return false;  //출발칸 토큰 없음
        if (toTile.AssignedTokenKey.HasValue) return false;     //도착칸 점유(스왑 금지)
        return true;
    }

    //안전한 이동을 위한 토큰 레이어 확보
    private void EnsureTokenLayer()
    {
        if (tokenLayer != null) return;
        var go = new GameObject("TokenLayer", typeof(RectTransform));
        tokenLayer = go.GetComponent<RectTransform>();
        tokenLayer.SetParent(battleFieldRt, false);
        tokenLayer.anchorMin = Vector2.zero;
        tokenLayer.anchorMax = Vector2.one;
        tokenLayer.offsetMin = Vector2.zero;
        tokenLayer.offsetMax = Vector2.zero;
    }

    //from → to가 인접칸일 때 정확히 방향 인덱스를 반환
    private int GetDirectionIndexByOffset(Vector2Int from, Vector2Int to)
    {
        var moveCtrl = ControllerRegister.Get<MovementOrderController>();
        Vector2Int d = to - from;
        var dirs = ((from.x & 1) == 0) ? moveCtrl.EvenQDirs : moveCtrl.OddQDirs;
        for (int i = 0; i < 6; i++) if (dirs[i] == d) return i;
        return -1;  //인접이 아니면 -1
    }

    //비인접 이동 폴백 (각도 기반). bin 기준을 index 0 = RightDown의 월드각으로 교정
    private int GetDirectionIndexByAngle(Vector2Int from, Vector2Int to)
    {
        if (!hexTileMap.TryGetValue((from.x, from.y), out var ft)) return -1;
        if (!hexTileMap.TryGetValue((to.x, to.y), out var tt)) return -1;

        Vector2 a = ft.GetComponent<RectTransform>().position;
        Vector2 b = tt.GetComponent<RectTransform>().position;
        Vector2 v = b - a;
        if (v.sqrMagnitude < 0.000001f) return -1;

        float ang = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;  //동 = 0도, 북 = +90도
        if (ang < 0f) ang += 360f;

        const float index0 = 330f;  //0 = RightDown 기준축을 330도로 고정 (실제 배치 각도에 맞춤)
        float rel = ang - index0;
        rel = (rel % 360f + 360f) % 360f;  //0...360 정규화

        //경계 튐 방지: 30도 쉬프트 후 60도 단위로 Floor
        int idx = Mathf.FloorToInt((rel + 30f) / 60f) % 6;
        return idx;
    }

    //최종 방향 인덱스 선택: 1순위 오프셋, 2순위 각도 (멀리 이동하는 연출 대비)
    private int ResolveDirectionIndex(Vector2Int from, Vector2Int to)
    {
        int idx = GetDirectionIndexByOffset(from, to);
        if (idx >= 0) return idx;
        return GetDirectionIndexByAngle(from, to);
    }
    #endregion

    #region 기본 이동카드 관련 캐릭터/이동 후보 좌표 하이라이트
    //선택 하이라이트 (내 캐릭터)
    public void OnHighlightMyCharacters(IEnumerable<int> ids)
    {
        RestoreSnapshots(selectedSnaps);
        selectedSnaps.Clear();

        foreach (var id in ids)
        {
            if (!TryGetTokenHex(id, out var pos)) continue;
            if (!TryGetOutline(pos, out var img)) continue;

            selectedSnaps.Add(new OutlineSnapshot
            {
                img = img,
                origColor = img.color,
                origActive = img.gameObject.activeSelf  //원래 활성 상태 저장
            });

            img.color = HL_SELECT;
            if (!img.gameObject.activeSelf) img.gameObject.SetActive(true);  //잠시 활성
            img.transform.SetAsLastSibling();
        }

        Debug.Log("[Highlight] 내 캐릭터 선택 단계");
    }

    //이동 후보만 하이라이트
    public void OnHighlightCandidateCells(IEnumerable<Vector2Int> hexes)
    {
        RestoreSnapshots(candidSnaps);
        candidSnaps.Clear();

        foreach (var h in hexes)
        {
            if (!TryGetOutline(h, out var img)) continue;

            bool alreadySelected = selectedSnaps.Any(s => s.img == img);
            if (!alreadySelected)
            {
                candidSnaps.Add(new OutlineSnapshot
                {
                    img = img,
                    origColor = img.color,
                    origActive = img.gameObject.activeSelf  //원래 활성 상태 저장
                });

                img.color = HL_CANDID;
            }

            if (!img.gameObject.activeSelf) img.gameObject.SetActive(true);  //잠시 활성
            img.transform.SetAsLastSibling();
        }

        Debug.Log($"[Highlight] 이동 가능한 좌표: {string.Join(",", hexes)}");
    }

    //이동 후보(빨강) + 예약(연노랑) 하이라이트 동시 처리
    public void OnHighlightCandidateCells(IEnumerable<Vector2Int> candidates, IEnumerable<Vector2Int> reserved)
    {
        //기존 스냅샷 원복
        RestoreSnapshots(candidSnaps);
        RestoreSnapshots(reservedSnaps);
        candidSnaps.Clear();
        reservedSnaps.Clear();

        var reservedSet = new HashSet<Vector2Int>(reserved ?? Array.Empty<Vector2Int>());

        //1) 후보 칠하기 (빨강)
        foreach (var h in candidates)
        {
            if (!TryGetOutline(h, out var img)) continue;

            //선택(초록)으로 이미 칠한 칸은 건너뜀
            bool alreadySelected = selectedSnaps.Any(s => s.img == img);
            if (!alreadySelected)
            {
                candidSnaps.Add(new OutlineSnapshot { img = img, origColor = img.color, origActive = img.gameObject.activeSelf });
                img.color = HL_CANDID;
            }
            if (!img.gameObject.activeSelf) img.gameObject.SetActive(true);
            img.transform.SetAsLastSibling();
        }

        //2) 예약 칠하기
        //초록(선택) → 연노랑(예약) → 빨강(이동 후보) 순서 보장
        foreach (var h in reservedSet)
        {
            if (!TryGetOutline(h, out var img)) continue;

            bool alreadySelected = selectedSnaps.Any(s => s.img == img);
            if (alreadySelected) continue;  //초록이 최우선

            //연노랑 스냅샷 기록하고 색 덮어씀 (빨강 위에 우선)
            reservedSnaps.Add(new OutlineSnapshot { img = img, origColor = img.color, origActive = img.gameObject.activeSelf });
            img.color = HL_RESERVED;
            if (!img.gameObject.activeSelf) img.gameObject.SetActive(true);
            img.transform.SetAsLastSibling();
        }

        Debug.Log($"[Highlight] 후보:{string.Join(",", candidates)} | 예약:{string.Join(",", reservedSet)}");
    }

    //outline 찾기
    private bool TryGetOutline(Vector2Int hex, out Image img)
    {
        img = null;
        if (!hexTileMap.TryGetValue((hex.x, hex.y), out var tile)) return false;
        var tf = tile.transform.Find("outline");
        if (tf == null) return false;
        img = tf.GetComponent<Image>();
        return img != null;
    }

    //id → 현재 좌표
    private bool TryGetTokenHex(int tokenKey, out Vector2Int hex) => TryGetTokenPosition(tokenKey, out hex);

    //모든 하이라이트 원복
    public void OnClearHighlights()
    {
        RestoreSnapshots(candidSnaps);
        RestoreSnapshots(selectedSnaps);
        RestoreSnapshots(reservedSnaps);
        candidSnaps.Clear();
        selectedSnaps.Clear();
        reservedSnaps.Clear();
        Debug.Log("[Highlight] Clear");
    }

    //원복 공통 함수
    private void RestoreSnapshots(List<OutlineSnapshot> list)
    {
        foreach (var s in list)
        {
            if (s.img == null) continue;
            s.img.color = s.origColor;
            s.img.gameObject.SetActive(s.origActive);  //GameObject 활성 상태 복원
        }
    }
    #endregion
}
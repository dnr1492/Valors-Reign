using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class MovementOrderController : MonoBehaviour
{
    private SkillCardRoundSlot currentRoundSlot;
    private int selectingTokenKey = -1;
    private Vector2Int selectingCharHexPos;
    private bool isExecuting = false;  //이동 중인지
    private List<SkillCardRoundSlot> boundRoundSlots = new();  //라운드 슬롯 바인딩 (라운드 슬롯에 셋팅한 기본 이동카드 수 계산용)
    private bool isEditing = false;
    private MoveOrder? editingBackup = null;
    private SkillCardRoundSlot resumeAfterEditSlot = null;  //인라인 편집 중, 편집 종료 후 재개해야 할 타겟팅 슬롯 저장

    //라운드 슬롯에 설정된 한 번의 이동
    public struct MoveOrder
    {
        public int tokenKey;
        public Vector2Int fromHexPos;  //출발 좌표 (선택 당시)
        public Vector2Int toHexPos;    //도착 좌표
        public int roundOrder;         //현재 Round
    }

    private readonly Dictionary<SkillCardRoundSlot, MoveOrder> moveOrders = new();
    private readonly Dictionary<Vector2Int, SkillCardRoundSlot> reservedByHexPos = new();

    private void Awake()
    {
        ControllerRegister.Register(this);
    }

    #region 필드 정보 조회용 콜백
    public Func<Vector2Int, bool> Map_CellExists;   //인자로 전달된 좌표(hex)가 필드 범위 안에 존재하는지 여부 반환
    public Func<Vector2Int, bool> Map_IsPassable;   //해당 좌표가 통행 가능한 지형인지 여부 반환 (벽/물 등 판정)
    public Func<Vector2Int, bool> Map_IsOccupied;   //해당 좌표에 다른 유닛(아군/적군)이 존재하는지 여부 반환
    public Func<Vector2Int, bool> Map_IsMyTokenAt;  //나의 캐릭터 Token 인지 여부 반환
    public Func<int, bool> Char_IsMineAlive;        //해당 캐릭터 ID가 '나의 Token'이면서 현재 생존 상태인지 여부 반환
    #endregion

    #region 캐릭터 정보 및 이동 요청용 콜백
    public Func<int, Vector2Int, UniTask> Char_MoveToHexAsync;  //지정한 캐릭터 ID를 주어진 좌표(hex)로 실제 이동시키는 비동기 요청
    #endregion

    #region [UI] 하이라이트 표시
    public Action<IEnumerable<int>> UI_HighlightMyCharacters;                                      //[UI] 내 캐릭터들의 ID 목록을 하이라이트 표시
    public Action<IEnumerable<Vector2Int>> UI_HighlightCandidateCells;                             //[UI] 이동 후보 셀 좌표 목록을 하이라이트 표시
    public Action<IEnumerable<Vector2Int>, IEnumerable<Vector2Int>> UI_HighlightCandidateCellsEx;  //[UI] 이동 후보 + 예약 동시 하이라이트 표시
    public Action UI_ClearHighlights;                                                              //[UI] 모든 하이라이트 제거
    #endregion

    #region [UI] 토스트 메시지 표시
    public Action<string> UI_Toast;                                     //[UI] 안내/경고 등의 토스트 메시지 표시
    #endregion

    #region Odd-Q 이웃 방향 테이블
    //짝수 열(even column)에서의 6방향 상대 좌표
    public readonly Vector2Int[] EvenQDirs = {
        new(+1, 0),  //오른쪽 아래
        new(+1,-1),  //오른쪽 위
        new( 0,-1),  //위
        new(-1,-1),  //왼쪽 위
        new(-1, 0),  //왼쪽 아래
        new( 0,+1)   //아래
    };

    //홀수 열(odd column)에서의 6방향 상대 좌표
    public readonly Vector2Int[] OddQDirs = {
        new(+1,+1),  //오른쪽 아래
        new(+1, 0),  //오른쪽 위
        new( 0,-1),  //위
        new(-1, 0),  //왼쪽 위
        new(-1,+1),  //왼쪽 아래
        new( 0,+1)   //아래
    };
    #endregion

    #region Odd-Q 이웃 계산
    //주어진 좌표 a의 6방향 이웃 좌표들을 반환
    private IEnumerable<Vector2Int> GetOddQNeighbors(Vector2Int a)
    {
        var dirs = ((a.x & 1) == 0) ? EvenQDirs : OddQDirs;
        for (int i = 0; i < 6; i++) yield return a + dirs[i];
    }

    //두 좌표 a, b가 odd-q 기준에서 인접한 육각 타일인지 여부 반환
    private bool IsNeighborOffsetOddQ(Vector2Int a, Vector2Int b)
    {
        var dirs = ((a.x & 1) == 0) ? EvenQDirs : OddQDirs;
        for (int i = 0; i < 6; i++) if (a + dirs[i] == b) return true;
        return false;
    }
    #endregion

    #region 유효 후보 좌표 계산
    //origin 기준 odd-q 방식 6방향 중, 필드 안/통행 가능/미점유/미예약 칸 목록 반환
    private List<Vector2Int> ComputeValidNeighbors(Vector2Int origin)
    {
        var res = new List<Vector2Int>(6);
        foreach (var hex in GetOddQNeighbors(origin))
        {
            if (Map_CellExists != null && !Map_CellExists(hex)) continue;  //필드 존재 여부
            if (Map_IsPassable != null && !Map_IsPassable(hex)) continue;  //통행 가능 여부
            if (Map_IsOccupied != null && Map_IsOccupied(hex)) continue;   //점유 여부
            if (reservedByHexPos.ContainsKey(hex)) continue;               //다른 슬롯 예약 여부
            res.Add(hex);  //조건 통과 칸 추가
        }
        return res;
    }
    #endregion

    #region 최종 이동 가능 판정
    //캐릭터 ID 기준 fromHexPos → toHexPos 이동 가능 여부 최종 검증
    private bool ValidateFinalDestination(int characterId, Vector2Int fromHex, Vector2Int toHex, SkillCardRoundSlot ownerSlot)
    {
        if (Char_IsMineAlive != null && !Char_IsMineAlive(characterId)) return false;  //생존 여부
        if (!IsNeighborOffsetOddQ(fromHex, toHex)) return false;                       //인접 여부
        if (Map_CellExists != null && !Map_CellExists(toHex)) return false;            //맵 존재 여부
        if (Map_IsPassable != null && !Map_IsPassable(toHex)) return false;            //통행 가능 여부
        if (Map_IsOccupied != null && Map_IsOccupied(toHex)) return false;             //점유 여부
        if (IsReservedByOther(toHex, ownerSlot)) return false;                         //다른 슬롯 예약 여부
        return true;
    }
    #endregion

    #region 타겟팅 플로우
    public void BeginForSlot(SkillCardRoundSlot slot)
    {
        if (isExecuting) {
            UI_Toast?.Invoke("이동 실행 중입니다.");
            return;
        }

        CancelCurrent();               //진행 중인 타겟팅 초기화
        currentRoundSlot = slot;       //현재 조작 슬롯 지정
        selectingTokenKey = -1;        //선택 해제
        resumeAfterEditSlot = null;    //새 타겟팅 시작 시 편집 복귀 슬롯 초기화
        UI_ClearHighlights?.Invoke();  //기존 하이라이트 제거

        var used = GetUsedTokenKeys();
        var candidates = GetMyAliveCharacterIds().Where(id => !used.Contains(id));
        if (!candidates.Any()) {
            UI_Toast?.Invoke("이동 가능한 캐릭터가 없습니다. (이미 다른 이동카드로 설정됨)");
            CancelCurrent();
            return;
        }

        UI_HighlightMyCharacters?.Invoke(candidates);  //내 유닛 하이라이트
    }

    //현재 타겟팅/편집 모드 취소 (타겟팅이면 초기화, 편집 중이면 예약 원복)
    public void CancelCurrent()
    {
        if (isEditing && currentRoundSlot != null && editingBackup.HasValue)
        {
            var mo = editingBackup.Value;
            moveOrders[currentRoundSlot] = mo;
            reservedByHexPos[mo.toHexPos] = currentRoundSlot;
            isEditing = false;
            editingBackup = null;
        }

        currentRoundSlot = null;
        selectingTokenKey = -1;
        UI_ClearHighlights?.Invoke();
    }

    public void OnCharacterClicked(int tokenKey, Vector2Int charHexPos)
    {
        //편집 중에 미사용 캐릭터를 클릭하면, 편집을 끝내고 복귀 슬롯으로 타겟팅을 넘김
        if (isEditing && resumeAfterEditSlot != null && currentRoundSlot != resumeAfterEditSlot)
        {
            bool usedAnywhere = moveOrders.Any(kv => kv.Value.tokenKey == tokenKey);
            if (!usedAnywhere) {
                //편집 슬롯 예약/하이라이트 복구 + 복귀 슬롯으로 BeginForSlot됨
                //fall-through: 아래 일반 타겟팅 로직이 이 클릭을 복귀 슬롯에 대해 처리
                CancelEdit();
            }
        }

        //1) 이미 배정된 슬롯이면 그 슬롯 인라인 편집으로 스위치
        if (TryGetSlotByToken(tokenKey, out var ownerSlot))
        {
            if (currentRoundSlot != ownerSlot)
            {
                EnterInlineEdit(ownerSlot, tokenKey, charHexPos);
                return;
            }
        }

        //2) 일반 타겟팅 흐름
        if (currentRoundSlot == null)
        {
            var next = PendingMoveSlots().FirstOrDefault();
            if (next != null) BeginForSlot(next);
            else
            {
                UI_Toast?.Invoke("먼저 이동카드를 라운드 슬롯에 배치해 주세요.");
                return;
            }
        }

        //3) 다른 슬롯에서 이미 사용 중이면 차단
        bool usedByOther = moveOrders.Any(kv => kv.Value.tokenKey == tokenKey && kv.Key != currentRoundSlot);
        if (usedByOther)
        {
            UI_Toast?.Invoke("이 캐릭터는 이번 턴에 이미 이동이 설정되었습니다.");
            return;
        }

        //4) 소유/생존 검증 → 선택 가능한 후보 표시
        bool isMineHere = Map_IsMyTokenAt != null && Map_IsMyTokenAt(charHexPos);
        if (!isMineHere || (Char_IsMineAlive != null && !Char_IsMineAlive(tokenKey)))
        {
            UI_Toast?.Invoke("선택할 수 없는 캐릭터입니다.");
            return;
        }

        //5) 인접 후보 계산 → 이동 후보 표시 + 예약해 둔 경우 같이 표시
        selectingTokenKey = tokenKey;
        selectingCharHexPos = charHexPos;
        var candidates = ComputeValidNeighbors(selectingCharHexPos);
        if (candidates.Count == 0)
        {
            UI_Toast?.Invoke("이동 가능한 인접칸이 없습니다.");
            var used = GetUsedTokenKeys();
            UI_HighlightMyCharacters?.Invoke(GetMyAliveCharacterIds().Where(id => !used.Contains(id)));
            return;
        }
        var reserved = reservedByHexPos.Keys;  //예약해 둔 목적지들
        if (UI_HighlightCandidateCellsEx != null) UI_HighlightCandidateCellsEx.Invoke(candidates, reserved);
        else UI_HighlightCandidateCells?.Invoke(candidates);
    }

    public bool OnHexClicked(Vector2Int destinationHexPos)
    {
        if (currentRoundSlot == null || selectingTokenKey < 0) return false;

        if (!ValidateFinalDestination(selectingTokenKey, selectingCharHexPos, destinationHexPos, currentRoundSlot))
        {
            UI_Toast?.Invoke("이동할 수 없는 위치입니다.");
            return false;
        }

        var newOrder = new MoveOrder
        {
            tokenKey = selectingTokenKey,
            fromHexPos = selectingCharHexPos,
            toHexPos = destinationHexPos,
            roundOrder = currentRoundSlot != null ? currentRoundSlot.GetRoundOrder() : 999
        };

        moveOrders[currentRoundSlot] = newOrder;
        reservedByHexPos[destinationHexPos] = currentRoundSlot;

        UpdateMoveCardImageByOrder(currentRoundSlot, selectingTokenKey);

        UI_Toast?.Invoke("이동 위치가 설정되었습니다.");

        if (isEditing)
        {
            isEditing = false;
            editingBackup = null;

            //편집 완료 후 복귀 슬롯이 존재하면 그 슬롯의 타겟팅을 즉시 재개
            var resume = resumeAfterEditSlot;
            resumeAfterEditSlot = null;

            //편집 후에는 '복귀 슬롯'이 유효하고 미설정이면 그 슬롯으로,
            //그 외에는 '항상 다음 미설정 슬롯(1 → 2 → 3 → 4)'로 진행
            if (resume != null && !moveOrders.ContainsKey(resume)) BeginForSlot(resume);
            else if (!BeginNextPending()) CancelCurrent();
        }
        else
        {
            if (!BeginNextPending()) CancelCurrent();
        }
        return true;
    }

    public void ReleaseReservation(SkillCardRoundSlot slot)
    {
        if (slot == null) return;

        bool hadOrder = false;
        if (moveOrders.TryGetValue(slot, out var order))
        {
            reservedByHexPos.Remove(order.toHexPos);
            moveOrders.Remove(slot);
            hadOrder = true;
        }

        //현재 타겟팅 중인 슬롯이어도 '실제 오더 제거가 있었을 때'만 취소
        if (currentRoundSlot == slot) {
            if (isEditing || hadOrder) CancelCurrent();
        }

        ResetMoveCardImageIfNoOrder(slot);
    }

    public bool ValidateAllBeforeRound(int? onlyRound = null)
    {
        var invalid = new List<SkillCardRoundSlot>();

        foreach (var kv in moveOrders)
        {
            var slot = kv.Key;
            var mo = kv.Value;

            //특정 라운드만 검사
            if (onlyRound.HasValue && mo.roundOrder != onlyRound.Value) continue;

            Vector2Int from = mo.fromHexPos;
            if (!ValidateFinalDestination(mo.tokenKey, from, mo.toHexPos, slot))
                invalid.Add(slot);
        }

        foreach (var s in invalid)
        {
            var hex = moveOrders[s].toHexPos;
            reservedByHexPos.Remove(hex);
            moveOrders.Remove(s);

            ResetMoveCardImageIfNoOrder(s);
        }

        if (invalid.Count > 0)
            UI_Toast?.Invoke("일부 이동 위치가 유효하지 않아 해제되었습니다.");

        return invalid.Count == 0;
    }

    //현재까지 예약에 사용된 캐릭터 집합
    private HashSet<int> GetUsedTokenKeys() => new HashSet<int>(moveOrders.Values.Select(v => v.tokenKey));

    //이미 오더에 쓰인 캐릭터의 슬롯 찾기
    private bool TryGetSlotByToken(int tokenKey, out SkillCardRoundSlot slot)
    {
        foreach (var kv in moveOrders)
        {
            if (kv.Value.tokenKey == tokenKey)
            {
                slot = kv.Key;
                return true;
            }
        }
        slot = null;
        return false;
    }

    //해당 라운드에 오더가 있는지 체크
    public bool HasOrderForRound(int roundOrder) => moveOrders.Any(kv => kv.Value.roundOrder == roundOrder);
    #endregion

    #region 순서대로 이동을 수행 (지정된 라운드의 이동 오더만 순차적으로 수행)
    public void ExecuteForRound(int roundOrder, Action onComplete = null) => UniTask.Void(async () => await ExecuteForRoundAsync(roundOrder, onComplete));

    private async UniTask ExecuteForRoundAsync(int roundOrder, Action onComplete)
    {
        if (isExecuting) { onComplete?.Invoke(); return; }
        isExecuting = true;

        //해당 라운드의 오더만 추출 (안전하게 여러 개도 처리)
        var batch = moveOrders
            .Where(kv => kv.Value.roundOrder == roundOrder)
            .Select(kv => (slot: kv.Key, order: kv.Value))
            .ToList();

        foreach (var x in batch)
        {
            var slot = x.slot;
            var mo = x.order;

            Vector2Int from = mo.fromHexPos;
            if (!ValidateFinalDestination(mo.tokenKey, from, mo.toHexPos, slot))
            {
                reservedByHexPos.Remove(mo.toHexPos);
                moveOrders.Remove(slot);
                UI_Toast?.Invoke($"이동 불가 지역입니다.(round {roundOrder})");
                continue;
            }

            if (Char_MoveToHexAsync != null) await Char_MoveToHexAsync(mo.tokenKey, mo.toHexPos);
            else await GridManager.Instance.MoveFromToByHexAsync(from, mo.toHexPos);

            reservedByHexPos.Remove(mo.toHexPos);
            moveOrders.Remove(slot);
        }

        isExecuting = false;
        onComplete?.Invoke();
    }
    #endregion

    #region 라운드 시작 게이트
    public void BindRoundSlots(IEnumerable<SkillCardRoundSlot> slots) => boundRoundSlots = slots != null
            ? slots.Where(s => s != null).OrderBy(s => s.GetRoundOrder()).ToList()
            : new List<SkillCardRoundSlot>();

    public bool AreAllMoveCardsConfigured(bool toast = true)
    {
        if (boundRoundSlots == null || boundRoundSlots.Count == 0) return true;

        int need = boundRoundSlots.Count(s => s.AssignedSkillCardData != null && s.AssignedSkillCardData.id == 1000);

        var ordersForMoveSlots = moveOrders
            .Where(kv => kv.Key != null && kv.Key.AssignedSkillCardData != null && kv.Key.AssignedSkillCardData.id == 1000)
            .Select(kv => kv.Value)
            .ToList();

        int have = ordersForMoveSlots.Count;
        if (have < need)
        {
            if (toast) UI_Toast?.Invoke("이동카드의 캐릭터/목적지를 모두 설정해 주세요.");
            return false;
        }

        int distinct = ordersForMoveSlots.Select(o => o.tokenKey).Distinct().Count();
        if (distinct < have)
        {
            if (toast) UI_Toast?.Invoke("한 캐릭터는 턴당 1회만 이동할 수 있습니다.");
            return false;
        }
        return true;
    }
    #endregion

    #region 캐릭터/목적지를 설정하지 않은 라운드 슬롯들을 찾기
    public bool BeginNextPending()
    {
        if (isExecuting) return false;

        //무조건 1 → 2 → 3 → 4 순서의 첫 번째 미설정 슬롯을 사용
        SkillCardRoundSlot next = PendingMoveSlots().FirstOrDefault();

        if (next == null) {
            CancelCurrent();
            return false;
        }

        BeginForSlot(next);
        return true;
    }

    private IEnumerable<SkillCardRoundSlot> PendingMoveSlots()
    {
        if (boundRoundSlots == null) yield break;
        foreach (var s in boundRoundSlots)
        {
            if (s == null) continue;
            var d = s.AssignedSkillCardData;
            if (d == null || d.id != 1000) continue;
            if (moveOrders.ContainsKey(s)) continue;
            yield return s;
        }
    }
    #endregion

    #region Inline 편집 모드
    private void EnterInlineEdit(SkillCardRoundSlot slot, int tokenKey, Vector2Int charHexPos)
    {
        if (isExecuting)
        {
            UI_Toast?.Invoke("이동 실행 중입니다.");
            return;
        }

        //기존에 다른 슬롯을 편집 중이었다면 그 슬롯의 예약을 먼저 원복
        if (isEditing && currentRoundSlot != null && currentRoundSlot != slot && editingBackup.HasValue)
        {
            var prev = editingBackup.Value;
            moveOrders[currentRoundSlot] = prev;
            reservedByHexPos[prev.toHexPos] = currentRoundSlot;
            editingBackup = null;
        }

        //편집 전, '미설정' 슬롯에서 들어온 경우에만 복귀 대상 설정하되, 기존 값이 있으면 유지
        if (currentRoundSlot != null && currentRoundSlot != slot && !moveOrders.ContainsKey(currentRoundSlot))
            resumeAfterEditSlot = resumeAfterEditSlot != null ? resumeAfterEditSlot : currentRoundSlot;

        isEditing = true;
        currentRoundSlot = slot;
        selectingTokenKey = tokenKey;
        selectingCharHexPos = charHexPos;

        if (moveOrders.TryGetValue(slot, out var mo)) {
            editingBackup = mo;
            reservedByHexPos.Remove(mo.toHexPos);
        }

        UI_ClearHighlights?.Invoke();

        var candidates = ComputeValidNeighbors(selectingCharHexPos);
        if (candidates.Count == 0)
        {
            UI_Toast?.Invoke("이동 가능한 인접칸이 없습니다.");
            CancelEdit();
            return;
        }

        var reserved = reservedByHexPos.Keys;
        if (UI_HighlightCandidateCellsEx != null) UI_HighlightCandidateCellsEx.Invoke(candidates, reserved);
        else UI_HighlightCandidateCells?.Invoke(candidates);
    }

    public void CancelEdit()
    {
        if (!isEditing || currentRoundSlot == null || editingBackup == null)
        {
            CancelCurrent();
            return;
        }

        var mo = editingBackup.Value;
        moveOrders[currentRoundSlot] = mo;
        reservedByHexPos[mo.toHexPos] = currentRoundSlot;

        isEditing = false;
        editingBackup = null;

        //편집 취소 후 복귀 슬롯 재개
        var resume = resumeAfterEditSlot;
        resumeAfterEditSlot = null;
        if (resume != null && !moveOrders.ContainsKey(resume)) BeginForSlot(resume);
        else CancelCurrent();
    }
    #endregion

    #region 오더 이동/스왑 및 카드 이미지 처리
    //슬롯 간 오더 이동 (카드가 다른 슬롯으로 '이동'한 경우)
    //from 슬롯의 오더를 제거하고 to 슬롯으로 그대로 옮기며,
    //슬롯의 roundOrder를 to 슬롯 기준으로 동기화
    public bool TransferOrder(SkillCardRoundSlot from, SkillCardRoundSlot to)
    {
        if (from == null || to == null) return false;
        if (!moveOrders.TryGetValue(from, out var mo)) return false;

        moveOrders.Remove(from);

        //실행 순서 동기화
        mo.roundOrder = to.GetRoundOrder();

        //to 슬롯에 오더 등록 + 목적지 예약
        moveOrders[to] = mo;
        reservedByHexPos[mo.toHexPos] = to;

        //UI 반영
        UpdateMoveCardImageByOrder(to, mo.tokenKey);  //이동카드 이미지를 캐릭터로 변경
        ResetMoveCardImageIfNoOrder(from);            //떠난 슬롯은 기본 이미지로 복원
        return true;
    }

    //슬롯 간 오더 스왑 (카드가 서로 '스왑'된 경우)
    //각 슬롯에 있던 오더를 반대 슬롯으로 옮기면서 roundOrder도 목적 슬롯 기준으로 갱신
    public void SwapOrders(SkillCardRoundSlot a, SkillCardRoundSlot b)
    {
        if (a == null || b == null) return;

        bool hasA = moveOrders.TryGetValue(a, out var oa);
        bool hasB = moveOrders.TryGetValue(b, out var ob);

        //기존 슬롯에서 오더 제거
        if (hasA) moveOrders.Remove(a);
        if (hasB) moveOrders.Remove(b);

        //a → b
        if (hasA)
        {
            oa.roundOrder = b.GetRoundOrder();
            moveOrders[b] = oa;
            reservedByHexPos[oa.toHexPos] = b;
            UpdateMoveCardImageByOrder(b, oa.tokenKey);
        }
        else ResetMoveCardImageIfNoOrder(b);

        //b → a
        if (hasB)
        {
            ob.roundOrder = a.GetRoundOrder();
            moveOrders[a] = ob;
            reservedByHexPos[ob.toHexPos] = a;
            UpdateMoveCardImageByOrder(a, ob.tokenKey);
        }
        else ResetMoveCardImageIfNoOrder(a);
    }

    //슬롯 오더 제거 시 카드 이미지 원복
    private void ResetMoveCardImageIfNoOrder(SkillCardRoundSlot slot)
    {
        if (slot == null) return;
        if (!moveOrders.ContainsKey(slot))
        {
            var card = slot.GetComponentInChildren<SkillCard>();
            if (card != null) card.ResetImageIfMoveCard();
        }
    }

    //해당 슬롯의 이동카드 이미지를 캐릭터 스프라이트로 교체
    private void UpdateMoveCardImageByOrder(SkillCardRoundSlot slot, int tokenKey)
    {
        if (slot == null) return;
        var card = slot.GetComponentInChildren<SkillCard>();
        if (card == null || card.SkillCardData == null || card.SkillCardData.id != 1000) return;

        var tokenCtrl = ControllerRegister.Get<CharacterTokenController>();
        var token = tokenCtrl != null ? tokenCtrl.GetAllCharacterToken().FirstOrDefault(t => t.Key == tokenKey) : null;
        if (token == null) return;

        card.SetCharacterImageIfMoveCard(token.GetCharacterSprite());
    }
    #endregion

    #region 기타 유틸 ===== TODO: 추후 다시 확인 필요 =====
    public void ClearAll()
    {
        CancelCurrent();
        moveOrders.Clear();
        reservedByHexPos.Clear();
        isExecuting = false;
    }

    private IEnumerable<int> GetMyAliveCharacterIds()
    {
        return CombatManager.Instance != null
            ? CombatManager.Instance.GetMyAliveTokenIds()
            : Array.Empty<int>();
    }

    private bool IsReservedByOther(Vector2Int hex, SkillCardRoundSlot owner) => reservedByHexPos.TryGetValue(hex, out var s) && s != owner;
    #endregion
}
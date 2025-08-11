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

    public bool IsTargeting => currentRoundSlot != null;

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
    public Func<int, Vector2Int> Char_GetCurrentHex;            //캐릭터 ID를 이용해 현재 위치 좌표(hex) 반환 (재검증/이동 시작 전 위치 확인)
    public Func<int, Vector2Int, UniTask> Char_MoveToHexAsync;  //지정한 캐릭터 ID를 주어진 좌표(hex)로 실제 이동시키는 비동기 요청
    #endregion

    #region [UI] 하이라이트 표시
    public Action<IEnumerable<int>> UI_HighlightMyCharacters;           //[UI] 내 캐릭터들의 ID 목록을 하이라이트 표시
    public Action<IEnumerable<Vector2Int>> UI_HighlightCandidateCells;  //[UI] 이동 후보 셀 좌표 목록을 하이라이트 표시
    public Action UI_ClearHighlights;                                   //[UI] 모든 하이라이트 제거
    #endregion

    #region [UI] 토스트 메시지 표시
    public Action<string> UI_Toast;                                     //[UI] 안내/경고 등의 토스트 메시지 표시
    #endregion

    #region Odd-Q 이웃 방향 테이블
    //짝수 열(even column)에서의 6방향 상대 좌표
    private static readonly Vector2Int[] EvenQDirs = {
        new(+1, 0),  //오른쪽
        new(+1,-1),  //오른쪽 위
        new( 0,-1),  //위
        new(-1,-1),  //왼쪽 위
        new(-1, 0),  //왼쪽
        new( 0,+1)   //아래
    };

    //홀수 열(odd column)에서의 6방향 상대 좌표
    private static readonly Vector2Int[] OddQDirs = {
        new(+1,+1),  //오른쪽 아래
        new(+1, 0),  //오른쪽
        new( 0,-1),  //위
        new(-1, 0),  //왼쪽
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
    //캐릭터 ID 기준 fromHex → toHex 이동 가능 여부 최종 검증
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
    //슬롯 대상으로 타겟팅 시작: 실행 중이면 안내, 현재 상태 초기화 후 내 유닛 하이라이트
    public void BeginForSlot(SkillCardRoundSlot slot)
    {
        if (isExecuting) {
            //이동 중 보호
            UI_Toast?.Invoke("이동 실행 중입니다.");
            return; 
        }
        CancelCurrent();                                             //진행 중인 타겟팅 초기화
        currentRoundSlot = slot;                                     //현재 조작 슬롯 지정
        selectingTokenKey = -1;                                      //선택 해제
        UI_ClearHighlights?.Invoke();                                //기존 하이라이트 제거
        UI_HighlightMyCharacters?.Invoke(GetMyAliveCharacterIds());  //내 유닛 하이라이트
    }

    //현재 타겟팅 상태 전부 초기화
    public void CancelCurrent()
    {
        currentRoundSlot = null;
        selectingTokenKey = -1;
        UI_ClearHighlights?.Invoke();
    }

    //캐릭터 클릭 처리: 유효성 검사 후 이동 후보 칸 하이라이트
    public void OnCharacterClicked(int tokenKey, Vector2Int charHexPos)
    {
        //슬롯 미지정
        if (currentRoundSlot == null) return;                             

        //소유 + 생존을 동시에 확인 (동일 tokenKey라도 적이면 걸러짐)
        bool isMineHere = Map_IsMyTokenAt != null && Map_IsMyTokenAt(charHexPos);
        if (!isMineHere || (Char_IsMineAlive != null && !Char_IsMineAlive(tokenKey))) {
            UI_Toast?.Invoke("선택할 수 없는 캐릭터입니다.");
            return;
        }

        selectingTokenKey = tokenKey;      //선택 캐릭터 고정
        selectingCharHexPos = charHexPos;  //선택 시점 좌표 저장

        var candidates = ComputeValidNeighbors(selectingCharHexPos);  //인접 유효 칸 계산
        if (candidates.Count == 0) {
            UI_Toast?.Invoke("이동 가능한 인접칸이 없습니다.");
            UI_HighlightMyCharacters?.Invoke(GetMyAliveCharacterIds());  //재선택 유도
            return;
        }
        UI_HighlightCandidateCells?.Invoke(candidates);  //이동 후보 하이라이트
    }

    //목적지 클릭 처리: 현재 슬롯을 포함해 최종 검증 후 예약/저장
    public bool OnHexClicked(Vector2Int destinationHexPos)
    {
        if (currentRoundSlot == null || selectingTokenKey < 0) return false;  //선택 상태 확인

        //소유 슬롯(ownerSlot) 전달로 자기 슬롯 예약은 허용
        if (!ValidateFinalDestination(selectingTokenKey, selectingCharHexPos, destinationHexPos, currentRoundSlot)) {
            UI_Toast?.Invoke("이동할 수 없는 위치입니다.");
            return false;
        }

        //예약 + 오더 저장 (fromHexPos를 함께 저장해 라운드 시작 전 재검증 가능)
        moveOrders[currentRoundSlot] = new MoveOrder {
            tokenKey = selectingTokenKey,
            fromHexPos = selectingCharHexPos,
            toHexPos = destinationHexPos,
            roundOrder = currentRoundSlot != null ? currentRoundSlot.GetRoundOrder() : 999
        };
        reservedByHexPos[destinationHexPos] = currentRoundSlot;  //도착지 예약

        UI_Toast?.Invoke("이동 위치가 설정되었습니다.");
        CancelCurrent();  //선택 흐름 종료
        return true;
    }

    //해당 슬롯의 예약 해제 (오더와 예약 모두 제거, 현재 슬롯이면 타겟팅도 취소)
    public void ReleaseReservation(SkillCardRoundSlot slot)
    {
        if (slot == null) return;
        if (moveOrders.TryGetValue(slot, out var order)) {
            reservedByHexPos.Remove(order.toHexPos);
            moveOrders.Remove(slot);
        }
        if (currentRoundSlot == slot) CancelCurrent();
    }

    //라운드 시작 전 일괄 재검증: fromHexPos 기준으로 최종 판정, 무효한 예약 제거
    public bool ValidateAllBeforeRound()
    {
        var invalid = new List<SkillCardRoundSlot>();

        foreach (var kv in moveOrders)
        {
            var slot = kv.Key;
            var mo = kv.Value;

            Vector2Int from = mo.fromHexPos;  //항상 fromHexPos 사용 (키 덮어쓰기 영향 제거)
            if (!ValidateFinalDestination(mo.tokenKey, from, mo.toHexPos, slot))
                invalid.Add(slot);
        }

        //무효 오더 정리 (예약/오더 동시 제거)
        foreach (var s in invalid)
        {
            var hex = moveOrders[s].toHexPos;
            reservedByHexPos.Remove(hex);
            moveOrders.Remove(s);
        }

        if (invalid.Count > 0)
            UI_Toast?.Invoke("일부 이동 위치가 유효하지 않아 해제되었습니다.");

        return invalid.Count == 0;  //모두 유효하면 true
    }
    #endregion

    #region 순서대로 실행
    //외부에서 순차 실행 요청 진입점
    public void ExecuteInOrder(Action onComplete = null)
    {
        if (!isActiveAndEnabled) {
            onComplete?.Invoke();
            return;
        }
        UniTask.Void(async () => await ExecuteInOrderAsync(onComplete));
    }

    //실행 직전 재검증(fromHexPos 기준) 후 좌표 기반 이동을 순서대로 수행
    private async UniTask ExecuteInOrderAsync(Action onComplete)
    {
        if (isExecuting) return;  //중복 실행 방지
        isExecuting = true;

        var sequence = moveOrders
            .Select(kv => (slot: kv.Key, order: kv.Value))
            .OrderBy(x => x.order.roundOrder).ToList();  //라운드 순서 기준 정렬

        foreach (var x in sequence)
        {
            var slot = x.slot;
            var mo = x.order;

            Vector2Int from = mo.fromHexPos;

            if (!ValidateFinalDestination(mo.tokenKey, from, mo.toHexPos, slot))
            {
                reservedByHexPos.Remove(mo.toHexPos);  //예약 해제
                moveOrders.Remove(slot);               //오더 제거
                UI_Toast?.Invoke($"이동 불가로 무시됨 (slot {mo.roundOrder}).");
                continue;
            }

            if (Char_MoveToHexAsync != null) await Char_MoveToHexAsync(mo.tokenKey, mo.toHexPos);
            else await GridManager.Instance.MoveFromToByHexAsync(from, mo.toHexPos);

            reservedByHexPos.Remove(mo.toHexPos);  //정상 완료 시 예약 해제
            moveOrders.Remove(slot);               //오더 소진
        }

        isExecuting = false;
        onComplete?.Invoke();
    }
    #endregion

    #region 기타 유틸 ===== TODO: 추후 다시 확인 필요 =====
    //전체 상태 초기화(타깃팅/오더/예약/실행)
    public void ClearAll()
    {
        CancelCurrent();
        moveOrders.Clear();
        reservedByHexPos.Clear();
        isExecuting = false;
    }

    //슬롯에 대응하는 이동 오더 조회 시도
    public bool TryGetOrder(SkillCardRoundSlot slot, out MoveOrder order)
        => moveOrders.TryGetValue(slot, out order);

    //지정 좌표가 예약 상태인지 여부
    public bool IsReserved(Vector2Int hex) => reservedByHexPos.ContainsKey(hex);

    //내 유닛 하이라이트용 ID 목록 제공
    private IEnumerable<int> GetMyAliveCharacterIds()
    {
        return CombatManager.Instance != null
            ? CombatManager.Instance.GetMyAliveTokenIds()
            : Array.Empty<int>();
    }

    //해당 좌표 예약자가 나인지 여부 확인 (다른 슬롯이 예약했는지 판정)
    private bool IsReservedByOther(Vector2Int hex, SkillCardRoundSlot owner) => reservedByHexPos.TryGetValue(hex, out var s) && s != owner;
    #endregion
}
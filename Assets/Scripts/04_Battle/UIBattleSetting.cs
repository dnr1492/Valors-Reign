using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading;
using Cysharp.Threading.Tasks;
using System;
using static EnumClass;

public class UIBattleSetting : UIPopupBase
{
    [SerializeField] GameObject uiCoinFlipPrefab;

    private Canvas rootCanvas;
    private UICoinFlip uiCoinFlip;
    private MovementOrderController movementOrderCtrl;

    [Header("Hex Grid")]
    [SerializeField] RectTransform hexParentRt /*map*/, battleFieldRt;
    [SerializeField] GameObject hexPrefab;  //육각형 모양의 이미지가 있는 UI 프리팹

    [Header("Setting SkillCardZone")]
    [SerializeField] Transform skillCardZone;  //SkillCard의 Parant
    [SerializeField] GameObject skillCardPrefab;
    private readonly List<SkillCard> settingSkillCards = new();
    private readonly float skillCardWidth = 130f;
    private readonly float horizontalPadding = 10f;
    private readonly float minVisiblePixelsWhenOverlapping = 10f;

    [Header("Setting SkillCardRoundZone")]
    [SerializeField] SkillCardRoundSlot[] roundSlots = new SkillCardRoundSlot[4];  //RoundSlot 4칸 연결
    public SkillCardRoundSlot[] GetRoundSlots() => roundSlots;

    [Header("SkillCard Detail UI")]
    [SerializeField] Transform skillCardCopyZone;                  //클론을 붙일 영역
    [SerializeField] TextMeshProUGUI txtSkillCardDescriptionZone;  //설명 표시

    [Header("Top UI")]
    [SerializeField] Button btn_ready;
    [SerializeField] TextMeshProUGUI txt_timer, txt_curTrun, txt_settingState;
    private readonly float settingTimeLimitSec = 20f;
    private CancellationTokenSource settingTimerCts;

    private void OnToast(string msg) => ToastManager.Instance.Show(msg, ToastAnchor.Top, 1.6f);

    public void Init()
    {
        rootCanvas = GetComponentInParent<Canvas>();

        //필드 생성
        GridManager.Instance.CreateHexGrid(battleFieldRt, hexPrefab, hexParentRt, false, true);

        //덱 불러와서 필드에 표시
        UIEditorDeckPhase1 popup = UIManager.Instance.GetPopup<UIEditorDeckPhase1>("UIEditorDeckPhase1");
        var pack = popup.GetSelectedDeckPack();
        if (pack != null)
            GridManager.Instance.ShowDecksOnField(pack, ControllerRegister.Get<PhotonController>().OpponentDeckPack);

        //코인 플립 UI 초기화
        var go = Instantiate(uiCoinFlipPrefab, transform);
        uiCoinFlip = go.GetComponent<UICoinFlip>();
        uiCoinFlip.Init(OnCoinDirectionSelected);

        //이동 오더 컨트롤러 참조 및 초기화
        movementOrderCtrl = ControllerRegister.Get<MovementOrderController>();
        if (movementOrderCtrl != null)
        {
            #region (1) UI 이벤트 구독
            movementOrderCtrl.UI_HighlightMyCharacters -= GridManager.Instance.OnHighlightMyCharacters;
            movementOrderCtrl.UI_HighlightMyCharacters += GridManager.Instance.OnHighlightMyCharacters;

            movementOrderCtrl.UI_HighlightCandidateCells -= GridManager.Instance.OnHighlightCandidateCells;
            movementOrderCtrl.UI_HighlightCandidateCells += GridManager.Instance.OnHighlightCandidateCells;

            movementOrderCtrl.UI_HighlightCandidateCellsEx -= GridManager.Instance.OnHighlightCandidateCells;
            movementOrderCtrl.UI_HighlightCandidateCellsEx += GridManager.Instance.OnHighlightCandidateCells;

            movementOrderCtrl.UI_ClearHighlights -= GridManager.Instance.OnClearHighlights;
            movementOrderCtrl.UI_ClearHighlights += GridManager.Instance.OnClearHighlights;

            movementOrderCtrl.UI_Toast -= OnToast;
            movementOrderCtrl.UI_Toast += OnToast;
            #endregion

            #region (2) 규칙 및 이동 실행 델리게이트 주입
            movementOrderCtrl.Map_CellExists = GridManager.Instance.CellExists;
            movementOrderCtrl.Map_IsPassable = GridManager.Instance.IsPassable;
            movementOrderCtrl.Map_IsOccupied = GridManager.Instance.IsOccupied;
            movementOrderCtrl.Map_IsMyTokenAt = hex => GridManager.Instance.IsMyTokenAt(hex);

            movementOrderCtrl.Char_MoveToHexAsync = async (id, toHex) => {
                if (!GridManager.Instance.TryGetTokenPosition(id, out var from)) return;
                await GridManager.Instance.MoveFromToByHexAsync(from, toHex);
            };
            #endregion
        }

        //라운드 슬롯 순서 지정 (1 ~ 4)
        for (int i = 0; i < roundSlots.Length; i++)
            roundSlots[i].SetRoundOrder(i + 1);

        //라운드 슬롯 바인딩
        movementOrderCtrl.BindRoundSlots(roundSlots);

        //준비완료 버튼
        btn_ready.onClick.RemoveAllListeners();
        btn_ready.onClick.AddListener(() => {
            //수동 준비완료 전 게이트
            if (movementOrderCtrl != null && !movementOrderCtrl.AreAllMoveCardsConfigured())
            {
                movementOrderCtrl.BeginNextPending();
                return;
            }
            TurnManager.Instance.ReadyLocal(false);  //false = Manual
        });

        //타이머 시작
        StopSettingTimerIfAny();
        settingTimerCts = new CancellationTokenSource();
        SetTimeoutAsync(settingTimeLimitSec).Forget();
    }

    private void OnCoinDirectionSelected(int myCoinDriection)
    {
        ControllerRegister.Get<PhotonController>().RequestCoinFlip(myCoinDriection);
    }

    public void ShowCoinFlipResult(int result, bool hasFirstTurnChoice)
    {
        uiCoinFlip.PlayFlipAnimation(result, () => {
            if (hasFirstTurnChoice) {
                uiCoinFlip.ActiveTurnChoiceButton(true);  //선공 or 후공 선택 버튼 표시
                uiCoinFlip.Invoke(nameof(UICoinFlip.AutoSelectTurnOrder), 3f);
            }
            else {
                LoadingManager.Instance.Show("상대가 선공 또는 후공을 선택하는 중입니다...");
                AIBattleHelper.AutoSelectTurnOrderAI(uiCoinFlip);
            }
        });
    }

    public void DestroyUICoinFlip()
    {
        if (uiCoinFlip != null) {
            Destroy(uiCoinFlip.gameObject);
            uiCoinFlip = null;
        }
    }

    #region 드로우한 스킬카드를 CardZone에 셋팅
    public void SetDrawnSkillCard(List<SkillCardData> drawnSkillCards)
    {
        //이전 카드 정리
        foreach (var card in settingSkillCards) Destroy(card.gameObject);
        settingSkillCards.Clear();

        //새로 생성
        foreach (var skillCardData in drawnSkillCards)
        {
            //SkillCardZone에 생성
            var go = Instantiate(skillCardPrefab, skillCardZone);
            var sprite = SpriteManager.Instance.dicSkillSprite.TryGetValue(skillCardData.name, out var sp) ? sp : null;
            var skillCard = go.GetComponent<SkillCard>();
            skillCard.Set(sprite, skillCardData);
            settingSkillCards.Add(skillCard);

            //드래그 세팅 + 드롭 콜백
            var drag = go.GetComponent<SkillCardEvent>();
            drag.Set(skillCardData, rootCanvas, roundSlots, skillCardZone, RefreshSkillCardZoneLayout);
            drag.onDropToRoundSlot = OnDropToRoundSlot;  //슬롯에 제대로 떨어졌을 때 처리
            drag.onSkillCardClick = OnSkillCardClicked;  //스킬카드를 클릭할 경우에 대한 이벤트
        }

        EnsureBasicMoveCardInCardZone();
        RefreshSkillCardZoneLayout();
        AdjustSkillCardZoonHeight(10, 10);

        Debug.Log($"[UIBattleSetting] 드로우 카드 {settingSkillCards.Count}장 UI에 세팅 완료");
    }

    /// <summary>
    /// CardZone 새로고침
    /// </summary>
    public void RefreshSkillCardZoneLayout()
    {
        EnsureBasicMoveCardInCardZone();

        SetSkillCardZoneLayout(settingSkillCards
            .Where(c => c != null && c.transform.parent == skillCardZone)
            .ToList());
    }

    /// <summary>
    /// 스킬카드를 skillCardZone 안에서 가운데 정렬되도록 배치
    /// 기본 이동카드(1000)를 항상 맨 왼쪽/맨 위로 오도록 재정렬
    /// 카드 개수가 많을수록 겹쳐지며, Zone 너비를 벗어나지 않도록 spacing을 자동 계산
    /// 카드 인덱스가 작을수록 화면에서 위로 보이도록 SiblingIndex()로 순서 지정
    /// </summary>
    /// <param name="cards"></param>
    private void SetSkillCardZoneLayout(List<SkillCard> cards)
    {
        var cardsInZone = cards
            .Where(c => c != null && c.transform.parent == skillCardZone)
            .ToList();
        int count = cardsInZone.Count;
        if (count == 0) return;

        //기본 이동카드(1000)를 항상 맨 왼쪽/맨 위로 오도록 재정렬
        //즉, 1000번 카드를 먼저, 나머지는 뒤로 (상대적 순서는 유지)
        var moveCards = cardsInZone.Where(c => c.SkillCardData != null && c.SkillCardData.id == 1000).ToList();
        var otherCards = cardsInZone.Where(c => c.SkillCardData == null || c.SkillCardData.id != 1000).ToList();
        cardsInZone = moveCards.Concat(otherCards).ToList();

        RectTransform zoneRt = skillCardZone.GetComponent<RectTransform>();
        float availableWidth = zoneRt.rect.width - horizontalPadding * 2;

        //spacing을 조절해서 카드들이 영역 안에 딱 맞도록 하기
        float spacing;

        if (count == 1) spacing = 0f;
        else
        {
            //전체 카드 + 간격 합이 availableWidth 안에 들어오게 spacing 계산
            spacing = (availableWidth - (skillCardWidth * count)) / (count - 1);
            float minSpacing = -(skillCardWidth - minVisiblePixelsWhenOverlapping);
            spacing = Mathf.Clamp(spacing, minSpacing, skillCardWidth);  //과도한 겹침 방지
        }

        float layoutWidth = (skillCardWidth * count) + (spacing * (count - 1));
        float startX = -layoutWidth / 2f + skillCardWidth / 2f;  //첫 카드 중심이 기준

        for (int i = 0; i < count; i++)
        {
            var card = cardsInZone[i];
            var rt = card.GetComponent<RectTransform>();

            float x = startX + i * (skillCardWidth + spacing);
            rt.anchoredPosition = new Vector2(x, 0);

            //카드 순서 보정 (작은 인덱스가 앞(화면 위)으로 오도록 역순 배치)
            rt.SetSiblingIndex(count - 1 - i);
        }
    }

    /// <summary>
    /// 모든 스킬카드 중 가장 높은 카드 기준으로 skillCardZone의 height를 자동 조정
    /// 위, 아래 여백(paddingTop, paddingBottom)은 추가적으로 설정
    /// </summary>
    /// <param name="paddingTop">상단 여백</param>
    /// <param name="paddingBottom">하단 여백</param>
    private void AdjustSkillCardZoonHeight(float paddingTop, float paddingBottom)
    {
        float maxHeight = settingSkillCards.Max(card =>
            card.GetComponent<RectTransform>().rect.height);

        float finalHeight = maxHeight + paddingTop + paddingBottom;

        var rt = skillCardZone.GetComponent<RectTransform>();
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, finalHeight);
    }
    #endregion

    #region 드로우한 스킬카드를 Drag & Drop으로 RoundZone에 셋팅
    private void OnDropToRoundSlot(SkillCardEvent drag, SkillCardRoundSlot slot)
    {
        //슬롯의 자식으로 이동 + 배정
        slot.Assign(drag.SkillCardData, drag);

        //드롭 직후, 빈 라운드 슬롯을 먼저 정리
        foreach (var s in roundSlots)
        {
            if (s == null) continue;
            if (s.GetComponentInChildren<SkillCardEvent>() == null && !s.IsEmpty)
                s.Clear();  //고스트 데이터 제거 (예약/이미지 원복 포함)
        }

        //CardZone 재정렬 (이 시점엔 RoundZone 카운트가 정확)
        RefreshSkillCardZoneLayout();

        //드래그 드롭 이후 기본 이동카드면 타겟팅 모드 ON (항상 1 → 2 → 3 → 4 첫 미설정부터)
        if (drag.SkillCardData != null && drag.SkillCardData.id == 1000) {
            movementOrderCtrl.BeginNextPending();
            Debug.Log("[MoveOrder] Begin targeting for pending basic-move slot");
        }
    }
    #endregion

    #region CardZone의 기본 이동카드(1000)를 '1장 또는 0장'으로 유지
    private void EnsureBasicMoveCardInCardZone()
    {
        int alive = GetAliveCharacterCount();
        int basicMoveInRound = GetBasicMoveCardCountInRoundZone();
        int desiredInCardZone = (alive > 0 && basicMoveInRound < alive) ? 1 : 0;  //CardZone에 1장은 계속 유지. 다만 생존 0이면 0장.

        //현재 CardZone에 있는 기본 이동카드 목록
        var basicMoveCardsInZone = settingSkillCards
            .Where(c => c != null
                && c.SkillCardData != null
                && c.SkillCardData.id == 1000
                && c.transform.parent == skillCardZone)
            .ToList();

        //남는 복제된 기본 이동카드는 유지해야 하는 장수를 제외하고 제거/삭제
        //많으면 제거
        if (basicMoveCardsInZone.Count > desiredInCardZone)
        {
            for (int i = desiredInCardZone; i < basicMoveCardsInZone.Count; i++)
            {
                var extra = basicMoveCardsInZone[i];
                settingSkillCards.Remove(extra);
                Destroy(extra.gameObject);
            }
        }
        //부족하면 생성
        else if (basicMoveCardsInZone.Count < desiredInCardZone)
        {
            if (DataManager.Instance.dicSkillCardData.TryGetValue(1000, out var basicMoveCardData))
            {
                var go = Instantiate(skillCardPrefab, skillCardZone);
                var sprite = SpriteManager.Instance.dicSkillSprite.TryGetValue(basicMoveCardData.name, out var sp) ? sp : null;

                var skillCard = go.GetComponent<SkillCard>();
                skillCard.Set(sprite, basicMoveCardData);
                settingSkillCards.Add(skillCard);

                //드래그로 RoundZone에 세팅 (CardZone에서 계속 드래그 가능해야 하므로 동일하게 구성)
                var drag = go.GetComponent<SkillCardEvent>();
                drag.Set(basicMoveCardData, rootCanvas, roundSlots, skillCardZone, RefreshSkillCardZoneLayout);
                drag.onDropToRoundSlot = OnDropToRoundSlot;
                drag.onSkillCardClick = OnSkillCardClicked;  //스킬카드를 클릭할 경우에 대한 이벤트
            }
            else Debug.Log("[UIBattleSetting] 기본 이동카드(1000) 데이터를 찾지 못했습니다.");
        }
    }

    private int GetAliveCharacterCount() => CombatManager.Instance.GetAliveCharacterCount();

    private int GetBasicMoveCardCountInRoundZone() => roundSlots.Count(
        s => s.AssignedSkillCardData != null
          && s.AssignedSkillCardData.id == 1000
          && s.GetComponentInChildren<SkillCardEvent>() != null  //실제 카드가 붙어 있을 때만 카운트
    );
    #endregion

    #region DetailZone에 스킬카드 Detail 표시 및 필드에 스킬 범위 표시
    private void OnSkillCardClicked(SkillCardEvent evt)
    {
        if (evt == null || evt.SkillCardData == null) return;

        int ownerTokenKey = ResolveOwnerTokenKeyForCard(evt.SkillCardData.id);

        //설명 텍스트 갱신
        if (txtSkillCardDescriptionZone != null)
            txtSkillCardDescriptionZone.text = evt.SkillCardData.effect ?? string.Empty;

        //카드 정보 보기 (항상 하나만 유지)
        if (skillCardCopyZone != null)
        {
            for (int i = skillCardCopyZone.childCount - 1; i >= 0; i--)
                Destroy(skillCardCopyZone.GetChild(i).gameObject);

            //원본을 그대로 복제해서 현재 상태로 보여준다.
            //단, 기본 이동카드인 경우 기본 이미지로 보여준다.
            var clone = Instantiate(evt.gameObject, skillCardCopyZone);
            var skillCard = clone.GetComponent<SkillCard>();
            var sprite = SpriteManager.Instance.dicSkillSprite.TryGetValue(evt.SkillCardData.name, out var sp) ? sp : null;
            skillCard.Set(sprite, evt.SkillCardData);
            skillCard.ResetImageIfMoveCard();
            var rt = clone.GetComponent<RectTransform>();
            rt.anchoredPosition = Vector2.zero;

            //클론은 클릭/드래그 불가능
            var cloneEvt = clone.GetComponent<SkillCardEvent>();
            if (cloneEvt != null) cloneEvt.enabled = false;

            //복제 카드 클릭 시 확대 토글
            var zoom = clone.AddComponent<SkillCardDetailZoom>();
            zoom.previewOwnerTokenKey = ownerTokenKey;
        }

        //필드 스킬 범위 프리뷰 (보기 전용)
        if (ownerTokenKey >= 0) GridManager.Instance.ShowSkillRangePreview(evt.SkillCardData, ownerTokenKey);
        else GridManager.Instance.ClearSkillRangePreview();
    }

    /// <summary>
    /// 클릭한 스킬카드를 가지고 있는 캐릭터 토큰의 키를 추정
    /// 1) 라운드 슬롯에 스킬카드가 꽂혀 있으면 그 슬롯의 토큰 키 우선
    /// 2) 아니면 내 생존 캐릭터 토큰들 중 이 스킬카드를 보유한 첫 번째 캐릭터 토큰
    /// 찾지 못하면 -1
    /// </summary>
    private int ResolveOwnerTokenKeyForCard(int skillId)
    {
        //1) RoundSlot에 카드가 배치되어 있다면, 슬롯에서 추적
        foreach (var slot in roundSlots)
        {
            if (slot == null || slot.IsEmpty) continue;
            var cardEvt = slot.GetComponentInChildren<SkillCardEvent>();
            if (cardEvt == null || cardEvt.SkillCardData == null) continue;

            if (cardEvt.SkillCardData.id == skillId) break;
        }

        //2) 내 생존 토큰 중 '이 스킬을 가진 캐릭터' 찾기
        var myAliveTokenKeys = CombatManager.Instance.GetMyAliveTokenIds();
        var tokenCtrl = ControllerRegister.Get<CharacterTokenController>();
        if (tokenCtrl == null) return -1;

        foreach (var tk in tokenCtrl.GetAllCharacterToken())
        {
            if (!myAliveTokenKeys.Contains(tk.Key)) continue;
            int characterId = tk.Key;
            if (DataManager.Instance.dicCharacterCardData.TryGetValue(characterId, out var cdata)
                && cdata.skills != null
                && cdata.skills.Contains(skillId))
            {
                return tk.Key;  //첫 번째로 매칭된 토큰 반환
            }
        }

        return -1;
    }
    #endregion

    #region 대전 셋팅 Timer ===== TODO: SkillCard 드래그 비활성, 타겟팅 종료 등 추가 =====
    //타이머 만료 시 자동 준비 (게이트 무시, UI에 타이머 표시)
    private async UniTaskVoid SetTimeoutAsync(float sec)
    {
        int prevSec = -1;  //이전 초 값 저장
        float t = sec;

        try
        {
            while (t > 0f)
            {
                int currentSec = Mathf.CeilToInt(t);  //올림해서 표시 (예: 9.2초 → 10)
                if (currentSec != prevSec)
                {
                    DisplayTimer(currentSec);
                    prevSec = currentSec;
                }

                t -= Time.deltaTime;
                await UniTask.Yield(PlayerLoopTiming.Update, settingTimerCts.Token);
            }

            OnToast("제한시간 초과로 준비완료 처리됩니다.");

            //이미 준비되어 있으면 중복 호출 안 함
            var tm = TurnManager.Instance;
            if (tm != null && !tm.IsLocalReady)
                tm.ReadyLocal(true);  //true = Timeout
        }
        catch (OperationCanceledException)
        {
            //타이머 취소 시 예외 무시
        }
    }

    //준비완료 상태인 지
    public void SetReadyState(bool ready)
    {
        if (ready) movementOrderCtrl.UI_ClearHighlights?.Invoke();

        // ===== TODO: SkillCard 드래그 비활성, 타겟팅 종료 등 추가 =====
    }

    //양쪽 모두 준비가 완료된 경우
    public void OnBothReady()
    {
        //타이머 종료
        StopSettingTimerIfAny();
        DisplayTimer(0);

        // ===== TODO: SkillCard 드래그 비활성, 타겟팅 종료 등 추가 =====
    }

    //새 턴 셋업 단계 시작: 타이머 리셋 + 시작, UI 표시 복구
    public void BeginSetupPhase()
    {
        //준비 상태 표시 초기화
        SetReadyState(false);

        //기존 타이머 중지 후 재시작
        StopSettingTimerIfAny();
        settingTimerCts = new CancellationTokenSource();
        SetTimeoutAsync(settingTimeLimitSec).Forget();
    }

    private void StopSettingTimerIfAny()
    {
        if (settingTimerCts != null)
        {
            settingTimerCts.Cancel();
            settingTimerCts.Dispose();
            settingTimerCts = null;
        }
    }
    #endregion

    #region [대전 진행] 내 라운드 슬롯 상태를 OppRoundPlan으로 직렬화
    public OppRoundPlan BuildMyRoundPlan()
    {
        var list = new List<RoundCardInfo>(4);
        var slots = GetRoundSlots();
        if (slots != null)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                var slot = slots[i];
                if (slot == null) continue;

                var evt = slot.GetComponentInChildren<SkillCardEvent>();
                if (evt == null || evt.SkillCardData == null) continue;

                int cardId = evt.SkillCardData.id;
                int round = i + 1;
                int tokenKey = -1;

                if (cardId == 1000 && movementOrderCtrl != null) {
                    movementOrderCtrl.TryGetAssignedTokenKey(slot, out tokenKey);
                }

                list.Add(new RoundCardInfo { round = round, cardId = cardId, moveTokenKey = tokenKey });
            }
        }
        return new OppRoundPlan { cards = list.ToArray() };
    }
    #endregion

    //[UI] 현재 제한시간 표시
    public void DisplayTimer(float timer)
    {
        txt_timer.text = timer.ToString() + " 초";
    }

    //[UI] 현재 턴 표시
    public void DisplayTurn(int turnIndex)
    {
        txt_curTrun.text = $"{turnIndex} 턴";
    }

    //[UI] 현재 셋팅 상태 표시 ===== TODO: 구현 요망 =====
    public void DisplaySettingState(string str)
    {
        //txt_settingState.text = str
    }

    protected override void ResetUI() { }
}
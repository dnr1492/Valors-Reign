using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UIBattleSetting : UIPopupBase
{
    [SerializeField] GameObject uiCoinFlipPrefab;
    [SerializeField] Button btn_back;

    private Canvas rootCanvas;
    private UICoinFlip uiCoinFlip;

    [Header("Hex Grid")]
    [SerializeField] RectTransform hexParantRt /*map*/, battleFieldRt;
    [SerializeField] GameObject hexPrefab;  //육각형 모양의 이미지가 있는 UI 프리팹

    [Header("Setting SkillCardZone")]
    [SerializeField] Transform skillCardZone;  //SkillCard의 Parant
    [SerializeField] GameObject skillCardPrefab;
    private readonly List<SkillCard> settingSkillCards = new();
    private readonly float skillCardWidth = 130f;
    private readonly float horizontalPadding = 10f;
    private readonly float minVisiblePixelsWhenOverlapping = 10f;

    [Header("Setting SkillCardRoundZone")]
    [SerializeField] SkillCardRoundSlot[] roundSlots = new SkillCardRoundSlot[4];  //SkillCardRoundSlot 4칸 연결

    private int GetAliveCharacterCount() => CombatManager.Instance.GetAliveCharacterCount();
    private int GetBasicMoveCardCountInRoundZone() => roundSlots.Count(s => s.AssignedSkillCardData != null && s.AssignedSkillCardData.id == 1000);

    public void Init()
    {
        rootCanvas = GetComponentInParent<Canvas>();

        GridManager.Instance.CreateHexGrid(battleFieldRt, hexPrefab, hexParantRt, false, true);

        UIEditorDeckPhase1 popup = UIManager.Instance.GetPopup<UIEditorDeckPhase1>("UIEditorDeckPhase1");
        var pack = popup.GetSelectedDeckPack();
        if (pack != null) GridManager.Instance.ShowDecksOnField(pack, ControllerRegister.Get<PhotonController>().OpponentDeckPack);

        var go = Instantiate(uiCoinFlipPrefab, transform);
        uiCoinFlip = go.GetComponent<UICoinFlip>();
        uiCoinFlip.Init(OnCoinDirectionSelected);
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

        //CardZone만 재정렬
        RefreshSkillCardZoneLayout();  
    }
    #endregion

    #region CardZone의 기본 이동카드(1000)를 '1장 또는 0장'으로 유지
    private void EnsureBasicMoveCardInCardZone()
    {
        int alive = GetAliveCharacterCount();
        int basicMoveInRound = GetBasicMoveCardCountInRoundZone();
        int desiredInCardZone = (basicMoveInRound < alive) ? 1 : 0;  //생존 캐릭터 수를 초과하지 않으면 CardZone에 기본 이동카드(1000)을 계속 표시

        //현재 CardZone에 있는 기본 이동카드 목록
        var basicMoveCardsInZone = settingSkillCards
            .Where(c => c != null
                && c.SkillCardData != null
                && c.SkillCardData.id == 1000
                && c.transform.parent == skillCardZone)
            .ToList();

        //남는 복제된 기본 이동카드는 1장 뺴고 제거/삭제
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
            }
            else Debug.Log("[UIBattleSetting] 기본 이동카드(1000) 데이터를 찾지 못했습니다.");
        }
    }
    #endregion

    protected override void ResetUI() { }

    // ================================ 구현 중 ================================ //
    // ================================ 구현 중 ================================ //
    // ================================ 구현 중 ================================ //
}
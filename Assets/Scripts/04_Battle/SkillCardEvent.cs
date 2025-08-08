using System;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;

public class SkillCardEvent : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    private Canvas rootCanvas;
    private SkillCardRoundSlot[] allRoundSlots;
    private Transform skillCardZoneParent;
    private Action refreshSkillCardZoneLayout;

    private RectTransform skillCardRt;
    private CanvasGroup canvasGroup;

    private Transform originalParent;
    private Vector2 originalAnchoredPos;
    private SkillCardRoundSlot dragSlot;  //드래그 시작 시 카드가 있던 슬롯 (카드의 출발지)

    public SkillCardData SkillCardData { get; private set; }

    public Action<SkillCardEvent, SkillCardRoundSlot> onDropToRoundSlot;

    public void Set(SkillCardData skillCardData, 
        Canvas rootCanvas, 
        SkillCardRoundSlot[] allRoundSlots,
        Transform skillCardZoneParent,
        Action refreshSkillCardZoneLayout)
    {
        this.rootCanvas = rootCanvas;
        this.allRoundSlots = allRoundSlots;
        this.skillCardZoneParent = skillCardZoneParent;
        this.refreshSkillCardZoneLayout = refreshSkillCardZoneLayout;

        skillCardRt = (RectTransform)transform;
        canvasGroup = GetComponent<CanvasGroup>();
        if (!canvasGroup) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        SkillCardData = skillCardData;
        
        //하이라이트 끄기
        foreach (var slot in this.allRoundSlots) slot.HideHighlight();
    }

    public void OnBeginDrag(PointerEventData e)
    {
        originalParent = transform.parent;
        originalAnchoredPos = skillCardRt.anchoredPosition;
        canvasGroup.blocksRaycasts = false;  //슬롯이 드롭을 받도록 슬롯의 이벤트를 막기
        transform.SetParent(rootCanvas.transform, true);  //최상위로 올려서 마우스 따라다니게
        dragSlot = originalParent ? originalParent.GetComponent<SkillCardRoundSlot>() : null;

        //하이라이트 켜기
        foreach (var slot in allRoundSlots) {
            if (slot == null) continue;

            if (!slot.CanAccept()) { 
                slot.HideHighlight(); 
                continue; 
            }

            if (slot.IsEmpty) slot.ShowEmptyHighlight();
            else slot.ShowSwapHighlight();
        }
    }

    public void OnDrag(PointerEventData e)
    {
        //로컬 좌표로 변환
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)rootCanvas.transform, e.position, e.pressEventCamera, out var localPos);
        //스킬카드가 마우스 커서를 따라다니게
        skillCardRt.localPosition = localPos;
    }

    public void OnEndDrag(PointerEventData e)
    {
        canvasGroup.blocksRaycasts = true;

        //레이캐스트로 슬롯 찾기 (UI에 레이캐스트 쏘기)
        //Raycast Target이 true인 모든 UI를 찾음
        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(e, results);

        SkillCardRoundSlot roundSlot = null;
        foreach (var r in results)
        {
            roundSlot = r.gameObject.GetComponentInParent<SkillCardRoundSlot>();
            if (roundSlot != null) break;
        }

        //배치 또는 스왑
        if (roundSlot != null && roundSlot.CanAccept()) {
            SwapSkillCard(roundSlot);
            onDropToRoundSlot?.Invoke(this, roundSlot);
        }
        else {
            //RoundZone에서 CardZone으로 이동
            bool droppedOnSkillZone = results.Any(r => r.gameObject.GetComponentInParent<SkillCardZone>() != null);
            if (droppedOnSkillZone && skillCardZoneParent != null) {
                //슬롯에서 빼고 CardZone으로 귀환
                transform.SetParent(skillCardZoneParent, false);
                skillCardRt.anchoredPosition = Vector2.zero;

                //출발 슬롯 데이터 초기화
                if (dragSlot != null) dragSlot.Clear();

                //레이아웃 다시 깔기
                refreshSkillCardZoneLayout?.Invoke();
            }
            else {
                //실패 → 원위치
                transform.SetParent(originalParent, false);
                skillCardRt.anchoredPosition = originalAnchoredPos;
            }
        }

        foreach (var slot in allRoundSlots)
        {
            //하이라이트 끄기
            slot.HideHighlight();
            //빈 슬롯 데이터 초기화
            if (slot.GetComponentInChildren<SkillCardEvent>() == null) slot.Clear();
        }
    }

    private void SwapSkillCard(SkillCardRoundSlot dropSlot)
    {
        //드롭하려는 슬롯(roundSlot)에 이미 카드가 있으면 스왑 처리
        if (!dropSlot.IsEmpty)
        {
            var existingCard = dropSlot.GetComponentInChildren<SkillCardEvent>();
            //슬롯에 카드가 존재한다면
            if (existingCard != null)
            {
                if (dragSlot == null) return;

                //드래그를 시작한 슬롯에 스왑한 다른 카드를 배치하고,
                //그 슬롯의 데이터도 스왑한 다른 카드 데이터로 동기화
                dragSlot.Assign(existingCard.SkillCardData, existingCard);
            }
        }
    }

    // ================================ 구현 중 ================================ //
    // ================================ 구현 중 ================================ //
    // ================================ 구현 중 ================================ //

    public Action<SkillCardEvent> onSkillCardClick;

    public void OnPointerClick(PointerEventData e)
    {
        Debug.Log("추후 스킬카드를 클릭할 경우 이벤트");
        onSkillCardClick?.Invoke(this);
    }
}
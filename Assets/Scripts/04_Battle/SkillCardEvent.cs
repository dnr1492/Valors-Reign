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
    public Action<SkillCardEvent> onSkillCardClick;

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
        foreach (var slot in allRoundSlots)
        {
            if (slot == null) continue;

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

        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(e, results);

        SkillCardRoundSlot roundSlot = null;
        foreach (var r in results)
        {
            roundSlot = r.gameObject.GetComponentInParent<SkillCardRoundSlot>();
            if (roundSlot != null) break;
        }

        if (roundSlot != null)
        {
            SwapSkillCard(roundSlot);
            onDropToRoundSlot?.Invoke(this, roundSlot);
        }
        else
        {
            bool droppedOnSkillZone = results.Any(r => r.gameObject.GetComponentInParent<SkillCardZone>() != null);
            if (droppedOnSkillZone && skillCardZoneParent != null)
            {
                transform.SetParent(skillCardZoneParent, false);
                skillCardRt.anchoredPosition = Vector2.zero;

                //RoundZone → CardZone 이동 시, 출발 슬롯 데이터/오더/이미지 리셋
                if (dragSlot != null)
                {
                    var moveCtrl = ControllerRegister.Get<MovementOrderController>();
                    moveCtrl.ReleaseReservation(dragSlot);
                    var selfCard = GetComponent<SkillCard>();
                    if (selfCard != null) selfCard.ResetImageIfMoveCard();
                }

                if (dragSlot != null) dragSlot.Clear();

                refreshSkillCardZoneLayout?.Invoke();
            }
            else
            {
                transform.SetParent(originalParent, false);
                skillCardRt.anchoredPosition = originalAnchoredPos;

                if (originalParent == skillCardZoneParent)
                    refreshSkillCardZoneLayout?.Invoke();
            }
        }

        foreach (var slot in allRoundSlots)
        {
            slot.HideHighlight();
            if (slot.AssignedSkillCardData != null && slot.GetComponentInChildren<SkillCardEvent>() == null)
                slot.Clear();
        }
    }

    private void SwapSkillCard(SkillCardRoundSlot dropSlot)
    {
        var moveCtrl = ControllerRegister.Get<MovementOrderController>();

        if (!dropSlot.IsEmpty)
        {
            var existingCard = dropSlot.GetComponentInChildren<SkillCardEvent>();
            if (existingCard != null)
            {
                if (dragSlot != null)
                {
                    dragSlot.Assign(existingCard.SkillCardData, existingCard);
                    moveCtrl.SwapOrders(dragSlot, dropSlot);
                }
                else
                {
                    //CardZone → RoundSlot (점유) 드롭
                    existingCard.transform.SetParent(skillCardZoneParent, false);
                    var exRt = (RectTransform)existingCard.transform;
                    exRt.anchoredPosition = Vector2.zero;

                    if (moveCtrl != null) moveCtrl.ReleaseReservation(dropSlot);
                    var existingSkillCard = existingCard.GetComponent<SkillCard>();
                    if (existingSkillCard != null) existingSkillCard.ResetImageIfMoveCard();

                    refreshSkillCardZoneLayout?.Invoke();
                }
            }
        }
        else
        {
            //빈 슬롯으로 이동
            if (dragSlot != null)
            {
                moveCtrl.TransferOrder(dragSlot, dropSlot);
            }
        }
    }

    public void OnPointerClick(PointerEventData e)
    {
        onSkillCardClick?.Invoke(this);
    }
}

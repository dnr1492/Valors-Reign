using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SkillCardDetailZoom : MonoBehaviour, IPointerClickHandler
{
    private static SkillCardDetailZoom instance;

    private Vector2 zoomSize = new Vector2(350f, 550f);
    private RectTransform rtOverlay;
    private RectTransform rtCard;
    private GameObject zoomClone;

    public int previewOwnerTokenKey = -1;

    //현재 열려있는지
    private static bool IsOpen() => instance != null;

    private void Show(GameObject sourceCard)
    {
        if (!sourceCard) return;

        //이미 열려있으면 먼저 닫기
        if (instance != null) instance.Close();

        int srcOwnerKey = previewOwnerTokenKey;

        //루트 캔버스 탐색
        Canvas root = sourceCard.GetComponentInParent<Canvas>();
        if (!root) return;

        //오버레이 생성
        GameObject go = new GameObject("SkillCardDetailZoom", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(SkillCardDetailZoom));
        go.transform.SetParent(root.transform, false);

        //반투명 백드롭
        var img = go.GetComponent<Image>();
        img.raycastTarget = true;
        img.color = new Color(0f, 0f, 0f, 0.55f);  //반투명 블랙

        instance = go.GetComponent<SkillCardDetailZoom>();
        instance.previewOwnerTokenKey = srcOwnerKey; 
        instance.rtOverlay = (RectTransform)go.transform;

        //풀스크린 세팅
        instance.rtOverlay.anchorMin = Vector2.zero;
        instance.rtOverlay.anchorMax = Vector2.one;
        instance.rtOverlay.offsetMin = Vector2.zero;
        instance.rtOverlay.offsetMax = Vector2.zero;
        instance.rtOverlay.pivot = new Vector2(0.5f, 0.5f);

        //카드 클론 생성
        instance.zoomClone = Instantiate(sourceCard, instance.rtOverlay);
        instance.zoomClone.name = "ZoomClone";

        //불필요 컴포넌트/이벤트 비활성
        var cloneEvt = instance.zoomClone.GetComponent<SkillCardEvent>();
        if (cloneEvt) cloneEvt.enabled = false;
        var cloneZoom = instance.zoomClone.GetComponent<SkillCardDetailZoom>();
        if (cloneZoom) Destroy(cloneZoom);  //재귀 방지

        //레이아웃 간섭 차단
        var le = instance.zoomClone.GetComponent<LayoutElement>();
        if (!le) le = instance.zoomClone.AddComponent<LayoutElement>();
        le.ignoreLayout = true;

        //클릭 닫기용 가드
        var cg = instance.zoomClone.GetComponent<CanvasGroup>();
        if (!cg) cg = instance.zoomClone.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = true;  //카드 자체는 클릭 통과 안되게

        //중앙 배치 및 사이즈
        instance.rtCard = (RectTransform)instance.zoomClone.transform;
        instance.rtCard.anchorMin = new Vector2(0.5f, 0.5f);
        instance.rtCard.anchorMax = new Vector2(0.5f, 0.5f);
        instance.rtCard.pivot = new Vector2(0.5f, 0.5f);
        instance.rtCard.anchoredPosition = Vector2.zero;
        instance.rtCard.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, zoomSize.x);
        instance.rtCard.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, zoomSize.y);

        //카드 이미지 레이캐스트 보장
        var imgCard = instance.zoomClone.GetComponent<Image>();
        if (imgCard) imgCard.raycastTarget = true;

        //텍스트 자동사이즈 + 스킬 범위 재구성
        var sc = instance.zoomClone.GetComponent<SkillCard>();
        if (sc != null)
        {
            sc.EnableEffectTextAutoSizeForEnlarge();  //텍스트 자동사이즈
            sc.RebuildHexForEnlarge();                //스킬 범위 재구성
        }
    }

    //클릭 토글 로직
    public void OnPointerClick(PointerEventData e)
    {
        //오버레이 배경 클릭 처리
        if (this == instance)
        {
            if (rtCard == null)
            {
                Close();
                return;
            }

            bool onCard = RectTransformUtility.RectangleContainsScreenPoint(rtCard, e.position, e.pressEventCamera);
            if (!onCard) Close();

            return;
        }

        //이 객체가 '스킬카드'라면 토글 (열려있으면 닫기, 아니면 열기)
        if (IsOpen()) instance.Close();
        else Show(gameObject);
    }

    //닫기
    private void Close()
    {
        if (zoomClone) Destroy(zoomClone);
        Destroy(gameObject);
        instance = null;
    }
}
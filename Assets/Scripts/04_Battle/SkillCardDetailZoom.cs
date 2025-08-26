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

    //���� �����ִ���
    private static bool IsOpen() => instance != null;

    private void Show(GameObject sourceCard)
    {
        if (!sourceCard) return;

        //�̹� ���������� ���� �ݱ�
        if (instance != null) instance.Close();

        int srcOwnerKey = previewOwnerTokenKey;

        //��Ʈ ĵ���� Ž��
        Canvas root = sourceCard.GetComponentInParent<Canvas>();
        if (!root) return;

        //�������� ����
        GameObject go = new GameObject("SkillCardDetailZoom", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(SkillCardDetailZoom));
        go.transform.SetParent(root.transform, false);

        //������ ����
        var img = go.GetComponent<Image>();
        img.raycastTarget = true;
        img.color = new Color(0f, 0f, 0f, 0.55f);  //������ ��

        instance = go.GetComponent<SkillCardDetailZoom>();
        instance.previewOwnerTokenKey = srcOwnerKey; 
        instance.rtOverlay = (RectTransform)go.transform;

        //Ǯ��ũ�� ����
        instance.rtOverlay.anchorMin = Vector2.zero;
        instance.rtOverlay.anchorMax = Vector2.one;
        instance.rtOverlay.offsetMin = Vector2.zero;
        instance.rtOverlay.offsetMax = Vector2.zero;
        instance.rtOverlay.pivot = new Vector2(0.5f, 0.5f);

        //ī�� Ŭ�� ����
        instance.zoomClone = Instantiate(sourceCard, instance.rtOverlay);
        instance.zoomClone.name = "ZoomClone";

        //���ʿ� ������Ʈ/�̺�Ʈ ��Ȱ��
        var cloneEvt = instance.zoomClone.GetComponent<SkillCardEvent>();
        if (cloneEvt) cloneEvt.enabled = false;
        var cloneZoom = instance.zoomClone.GetComponent<SkillCardDetailZoom>();
        if (cloneZoom) Destroy(cloneZoom);  //��� ����

        //���̾ƿ� ���� ����
        var le = instance.zoomClone.GetComponent<LayoutElement>();
        if (!le) le = instance.zoomClone.AddComponent<LayoutElement>();
        le.ignoreLayout = true;

        //Ŭ�� �ݱ�� ����
        var cg = instance.zoomClone.GetComponent<CanvasGroup>();
        if (!cg) cg = instance.zoomClone.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = true;  //ī�� ��ü�� Ŭ�� ��� �ȵǰ�

        //�߾� ��ġ �� ������
        instance.rtCard = (RectTransform)instance.zoomClone.transform;
        instance.rtCard.anchorMin = new Vector2(0.5f, 0.5f);
        instance.rtCard.anchorMax = new Vector2(0.5f, 0.5f);
        instance.rtCard.pivot = new Vector2(0.5f, 0.5f);
        instance.rtCard.anchoredPosition = Vector2.zero;
        instance.rtCard.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, zoomSize.x);
        instance.rtCard.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, zoomSize.y);

        //ī�� �̹��� ����ĳ��Ʈ ����
        var imgCard = instance.zoomClone.GetComponent<Image>();
        if (imgCard) imgCard.raycastTarget = true;

        //�ؽ�Ʈ �ڵ������� + ��ų ���� �籸��
        var sc = instance.zoomClone.GetComponent<SkillCard>();
        if (sc != null)
        {
            sc.EnableEffectTextAutoSizeForEnlarge();  //�ؽ�Ʈ �ڵ�������
            sc.RebuildHexForEnlarge();                //��ų ���� �籸��
        }
    }

    //Ŭ�� ��� ����
    public void OnPointerClick(PointerEventData e)
    {
        //�������� ��� Ŭ�� ó��
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

        //�� ��ü�� '��ųī��'��� ��� (���������� �ݱ�, �ƴϸ� ����)
        if (IsOpen()) instance.Close();
        else Show(gameObject);
    }

    //�ݱ�
    private void Close()
    {
        if (zoomClone) Destroy(zoomClone);
        Destroy(gameObject);
        instance = null;
    }
}
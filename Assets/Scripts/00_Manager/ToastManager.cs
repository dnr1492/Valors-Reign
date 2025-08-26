using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using static EnumClass;

public class ToastManager : Singleton<ToastManager>
{
    private GameObject toastPrefab;
    private Canvas rootCanvas;

    private readonly string resourcesPath = "Prefabs/Toast";
    private readonly float yOffset = 60f;  //��/�� �⺻ ����

    //�ִϸ��̼� �Ķ����
    private readonly float fadeIn = 0.18f;    //���̵� �� �ð�
    private readonly float fadeOut = 0.25f;   //���̵� �ƿ� �ð�
    private readonly float moveOffset = 18f;  //��¦ Ƣ����� �̵��� (px)
    private readonly bool scaleIn = true;     //������ �� ȿ�� ��� ����

    protected override void Awake()
    {
        base.Awake();

        if (!toastPrefab) toastPrefab = Resources.Load<GameObject>(resourcesPath);
        if (!rootCanvas) rootCanvas = FindAnyObjectByType<Canvas>();
    }

    public void Show(string message, ToastAnchor anchor, float duration = 1.6f)
    {
        var go = Instantiate(toastPrefab, rootCanvas.transform);
        var rt = go.GetComponent<RectTransform>();

        //anchor�� ���� ��ġ ����
        switch (anchor)
        {
            case ToastAnchor.Top:
                rt.anchorMin = new Vector2(0.5f, 1f);
                rt.anchorMax = new Vector2(0.5f, 1f);
                rt.pivot = new Vector2(0.5f, 1f);
                rt.anchoredPosition = new Vector2(0f, -yOffset);
                break;
            case ToastAnchor.Center:
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = Vector2.zero;
                break;
            case ToastAnchor.Bottom:
                rt.anchorMin = new Vector2(0.5f, 0f);
                rt.anchorMax = new Vector2(0.5f, 0f);
                rt.pivot = new Vector2(0.5f, 0f);
                rt.anchoredPosition = new Vector2(0f, yOffset);
                break;
        }

        var txt = go.GetComponentInChildren<TextMeshProUGUI>(true);
        if (txt) txt.text = message;

        //�佺Ʈ�� Ŭ�� �̺�Ʈ�� ���� �ʰ� ����ĳ��Ʈ ��Ȱ��ȭ
        foreach (var g in go.GetComponentsInChildren<Graphic>(true)) g.raycastTarget = false;

        //���� ����
        var cg = go.GetComponent<CanvasGroup>(); 
        if (!cg) cg = go.AddComponent<CanvasGroup>();

        _ = PlayToastAsync(go, rt, cg, duration);
    }

    //�ִϸ��̼� (���̵� �� �� ��� �� ���̵� �ƿ�)
    private async UniTaskVoid PlayToastAsync(GameObject go, RectTransform rt, CanvasGroup cg, float keepDuration)
    {
        var token = go.GetCancellationTokenOnDestroy();

        //�ʱ� ����
        var basePos = rt.anchoredPosition;
        var startPos = basePos + new Vector2(0f, moveOffset);
        rt.anchoredPosition = startPos;
        cg.alpha = 0f;

        var baseScale = go.transform.localScale;
        if (scaleIn) go.transform.localScale = baseScale * 0.95f;

        //Fade In
        float t = 0f;
        while (t < fadeIn && go)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / fadeIn);
            cg.alpha = Mathf.SmoothStep(0f, 1f, k);
            rt.anchoredPosition = Vector2.Lerp(startPos, basePos, k);
            if (scaleIn) go.transform.localScale = Vector3.Lerp(baseScale * 0.95f, baseScale, k);
            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }
        if (!go) return;
        cg.alpha = 1f;
        rt.anchoredPosition = basePos;
        if (scaleIn) go.transform.localScale = baseScale;

        //��� (ǥ�� ����)
        if (keepDuration > 0f) await UniTask.Delay(System.TimeSpan.FromSeconds(keepDuration), DelayType.UnscaledDeltaTime, PlayerLoopTiming.Update, token);
        if (!go) return;

        //Fade Out (��¦ �Ʒ���)
        t = 0f;
        var endPos = basePos + new Vector2(0f, -moveOffset * 0.6f);
        while (t < fadeOut && go)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / fadeOut);
            cg.alpha = Mathf.SmoothStep(1f, 0f, k);
            rt.anchoredPosition = Vector2.Lerp(basePos, endPos, k);
            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }
        if (go) Destroy(go);
    }
}
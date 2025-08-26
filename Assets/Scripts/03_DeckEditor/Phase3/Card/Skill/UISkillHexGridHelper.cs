using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using static EnumClass;

public static class UISkillHexGridHelper
{
    private readonly static float spacingX = 1f;
    private readonly static float spacingY = 0.5f;
    private readonly static int verticalExtra = 3;  //상/하 여유
    private readonly static int extraMargin = 1;  //스킬 범위 주변 여유 (행/열)
    private readonly static float minHexHeightPx = 32f;
    private static bool isBuilding = false;  //동시 재진입 방지

    #region 스킬 범위 기반 HexGrid 생성
    /// <summary>
    /// 스킬 범위를 모두 담도록 필수 열/행을 계산하고 Hex를 생성
    /// - hex 크기는 minHexHeightPx를 기준으로 '크게' 시도하고, 세로가 넘칠 때만 세로를 줄여 맞춘다 (가로 여유 유지)
    /// - 좌/우는 항상 extraMargin만큼 여유를 둔다
    /// - 생성 시 Outline, cost, 각종 스크립트는 UI 프리뷰 용도로 비활성화
    /// </summary>
    public static void CreateSkillHexGrid(
        RectTransform container, GameObject prefab,
        List<GameObject> hexList, Dictionary<(int, int), GameObject> hexMap,
        SkillCardData skill, int stepIndex = 0
    )
    {
        if (isBuilding) return;
        isBuilding = true;

        try
        {
            EnsureClean(container, hexList, hexMap);

            float parentWidth = container.rect.width;
            float parentHeight = container.rect.height;

            //0) 범위 오프셋 (중앙 포함) — axial(dq,dr) 상대 좌표로 수집
            var rel = BuildRelativeOffsetsForUI(skill, stepIndex);
            if (!rel.Contains((0, 0))) rel.Add((0, 0));

            //1) 필수 최소 반폭/반높이 산출
            int maxAbsQ = 0, maxAbsR = 0;
            foreach (var (dq, dr) in rel)
            {
                int aq = Mathf.Abs(dq), ar = Mathf.Abs(dr);
                if (aq > maxAbsQ) maxAbsQ = aq;
                if (ar > maxAbsR) maxAbsR = ar;
            }
            int requiredHalfCols = maxAbsQ + extraMargin;
            int requiredHalfRows = maxAbsR + extraMargin;

            //2) 기본 열/행
            int cols = requiredHalfCols * 2 + 1;
            int rows = requiredHalfRows * 2 + 1;

            //3) 헥스 크기 1차: 크게 시도 (세로 기준)
            float hexHeight = Mathf.Max(minHexHeightPx, 4f);
            float hexWidth = hexHeight * 2f / Mathf.Sqrt(3f);

            //컨테이너 사용폭/높이 계산식
            float UsedWidth(int c) => (0.75f * c + 0.25f) * hexWidth + spacingX * (c - 1);
            float UsedHeight(int r) => r * (hexHeight + spacingY) + spacingY * (verticalExtra * 2);

            //4) 세로가 넘치면 '세로만' 줄여 맞춤 (가로는 그대로 유지 → 좌우 여유 보전)
            if (UsedHeight(rows) > parentHeight)
            {
                float maxH = ((parentHeight - spacingY * (verticalExtra * 2)) / rows) - spacingY;
                hexHeight = Mathf.Max(4f, maxH);
                hexWidth = hexHeight * 2f / Mathf.Sqrt(3f);
            }

            //5) 가로가 남으면 열 수 확장 (Hex 크기는 유지)
            while (UsedWidth(cols) < parentWidth - 0.5f) cols += 2;

            int halfCols = cols / 2;
            int halfRows = rows / 2;

            //6) 생성 (odd-q 배치 공식은 '그대로' 유지)
            for (int dq = -halfCols; dq <= halfCols; dq++)
            {
                for (int dr = -(halfRows + verticalExtra); dr <= (halfRows + verticalExtra); dr++)
                {
                    //중복 가드 (있으면 파괴 후 갱신)
                    if (hexMap.TryGetValue((dq, dr), out var oldHex) && oldHex)
                    {
#if UNITY_EDITOR
                        if (!Application.isPlaying) Object.DestroyImmediate(oldHex);
                        else Object.Destroy(oldHex);
#else
                        Object.Destroy(oldHex);
#endif
                        hexMap.Remove((dq, dr));
                        hexList.Remove(oldHex);
                    }

                    var hex = Object.Instantiate(prefab, container);
                    hexList.Add(hex);
                    hexMap[(dq, dr)] = hex;

                    var rt = hex.GetComponent<RectTransform>();
                    rt.sizeDelta = new Vector2(hexWidth, hexHeight);

                    //odd-q 배치
                    float x = dq * (hexWidth * 0.75f + spacingX);
                    float y = dr * (hexHeight + spacingY)
                              + (dq % 2 != 0 ? (hexHeight + spacingY) / 2f : 0f);
                    rt.anchoredPosition = new Vector2(x, -y);

                    //UI 프리뷰용 초기화 (Outline/Cost/스크립트 비활성화 등)
                    InitHexForUI(hex);

                    //좌표 기록 (HexTile에 상대 좌표 전달)
                    var tile = hex.GetComponent<HexTile>();
                    if (tile != null) tile.Init((dq, dr));
                }
            }
        }
        finally { isBuilding = false; }
    }

    /// <summary>
    /// 스킬 범위 패턴의 상대 오프셋(dq,dr) 목록을 생성
    /// - SkillRangeType에 따라 Custom 또는 기본 패턴 사용
    /// - 색상(Color)은 무시하고 좌표만 반환
    /// - includeSelf = false면 (0,0) 오프셋 제거
    /// </summary>
    private static List<(int dq, int dr)> BuildRelativeOffsetsForUI(SkillCardData data, int stepIndex)
    {
        var list = new List<(int, int)>();
        if (data == null || data.steps == null || data.steps.Count == 0) return list;
        if (stepIndex < 0 || stepIndex >= data.steps.Count) stepIndex = 0;

        var step = data.steps[stepIndex];
        if (step == null || step.target == null) return list;

        int p = Mathf.Max(0, step.target.rangeParam);

        //1) 스킬 패턴 계산 (AXIAL 전용)
        List<(int dq, int dr, Color color)> range =
            (step.target.rangeType == SkillRangeType.Custom)
                ? SkillRangeHelper.GetCustom(step.target.customOffsets, Color.green)
                : SkillRangeHelper.GetPattern(step.target.rangeType, p, Color.green, Color.green, Color.green, Color.green);

        //2) dq/dr만 추출
        foreach (var (dq, dr, _) in range)
            list.Add((dq, dr));

        //3) 자기 자신 제외 옵션 처리
        if (step.target.includeSelf == false)
            list.RemoveAll(v => v.Item1 == 0 && v.Item2 == 0);

        return list;
    }

    /// <summary>
    /// 기존 자식 중 HexTile이 붙은 오브젝트 정리 (누수 방지)
    /// </summary>
    private static void EnsureClean(RectTransform container, List<GameObject> hexList, Dictionary<(int, int), GameObject> hexMap)
    {
        for (int i = container.childCount - 1; i >= 0; i--)
        {
            var child = container.GetChild(i);
            if (child.GetComponent<HexTile>() != null)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying) Object.DestroyImmediate(child.gameObject);
                else Object.Destroy(child.gameObject);
#else
                Object.Destroy(child.gameObject);
#endif
            }
        }
        hexList.Clear();
        hexMap.Clear();
    }

    /// <summary>
    /// Hex를 UI 프리뷰 용도로 초기화 (Outline/Cost/스크립트 비활성화)
    /// </summary>
    private static void InitHexForUI(GameObject hex)
    {
        //1) 스크립트 비활성화 (에디터/배틀 이벤트 등)
        var editorEvt = hex.GetComponent<HexTileEditorEvent>(); if (editorEvt) editorEvt.enabled = false;
        var battleEvt = hex.GetComponent<HexTileBattleEvent>(); if (battleEvt) battleEvt.enabled = false;

        //2) 레이캐스트/상호작용 비활성화
        var btn = hex.GetComponent<Button>(); if (btn) btn.enabled = false;
        var et = hex.GetComponent<EventTrigger>(); if (et) et.enabled = false;
        foreach (var g in hex.GetComponentsInChildren<Graphic>(true)) g.raycastTarget = false;

        //3) Outline/Shadow류 비활성화
        foreach (var o in hex.GetComponentsInChildren<Outline>(true)) o.enabled = false;
        foreach (var s in hex.GetComponentsInChildren<Shadow>(true)) s.enabled = false;

        //4) Cost 비활성화
        foreach (var t in hex.GetComponentsInChildren<Transform>(true))
        {
            var n = t.name.ToLower();
            if (n.Contains("cost")) t.gameObject.SetActive(false);
        }

        //5) 색 매우 옅게
        TrySetHexColor(hex, new Color(1f, 1f, 1f, 0.05f));
    }

    private static void TrySetHexColor(GameObject go, Color c)
    {
        if (!go) return;

        var tile = go.GetComponent<HexTile>();
        if (tile != null) { tile.SetColor(c); return; }

        var img = go.GetComponent<Image>();
        if (img != null) { img.color = c; return; }

        var sr = go.GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = c;
    }
    #endregion

    #region 생성된 HexGrid 전부 제거
    public static void ClearSkillHexGrid(List<GameObject> hexList, Dictionary<(int, int), GameObject> hexMap)
    {
        foreach (var hex in hexList)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) Object.DestroyImmediate(hex);
            else Object.Destroy(hex);
#else
            Object.Destroy(hex);
#endif
        }
        hexList.Clear();
        hexMap.Clear();
    }
    #endregion

    #region 스킬 범위를 필드에 표시
    /// <summary>
    /// UI 미리보기: 스킬 범위를 색으로 표시
    /// 중앙(0,0)은 includeSelf == true → 노랑, 아니면 회색
    /// axial(q,r)로 생성된 패턴을 odd-q(offset)로 변환해 hexMap을 조회
    /// </summary>
    public static void ShowSkillHexRange(SkillCardData data, Dictionary<(int, int), GameObject> hexMap)
    {
        if (data == null || data.steps == null || data.steps.Count == 0 || hexMap == null || hexMap.Count == 0) return;

        var step = data.steps[0];
        if (step == null || step.target == null) return;

        //팔레트
        Color colLine = new Color(1f, 0.35f, 0.35f, 1f);
        Color colRing = new Color(0.15f, 0.9f, 0.4f, 1f);
        Color colPlus = new Color(1f, 0.6f, 0.15f, 1f);
        Color colBox = new Color(0.35f, 0.6f, 1f, 1f);

        int p = Mathf.Max(0, step.target.rangeParam);
        bool includeSelf = step.target.includeSelf;

        //전체 색 매우 옅게
        foreach (var kv in hexMap)
            TrySetHexColor(kv.Value, new Color(1f, 1f, 1f, 0.05f));

        //스킬 범위 패턴을 계산
        List<(int dq, int dr, Color color)> range;
        if (step.target.rangeType == SkillRangeType.Custom)
            range = SkillRangeHelper.GetCustom(step.target.customOffsets, colLine);
        else
            range = SkillRangeHelper.GetPattern(step.target.rangeType, p, colLine, colPlus, colBox, colRing);

        //includeSelf = false면 (0,0) 제외
        if (!includeSelf)
            range.RemoveAll(v => v.dq == 0 && v.dr == 0);

        //axial(q,r) → odd-q(offset) 변환하는 로컬 함수
        int AxialToRow_OddQ(int q, int r) => r + ((q - (q & 1)) / 2);

        //칠하기
        foreach (var (dq, dr, color) in range)
        {
            int col = dq;
            int row = AxialToRow_OddQ(dq, dr);

            if (hexMap.TryGetValue((col, row), out var hex))
                TrySetHexColor(hex, color);
        }

        //중앙 기준점 (0,0)도 동일 변환 사용
        {
            int col0 = 0;
            int row0 = AxialToRow_OddQ(0, 0);
            if (hexMap.TryGetValue((col0, row0), out var centerHex))
            {
                var centerColor = includeSelf
                    ? (Color)new Color32(255, 221, 87, 255)  //노랑(자신 포함)
                    : new Color(0.75f, 0.75f, 0.75f, 1f);  //회색(기준점)
                TrySetHexColor(centerHex, centerColor);
            }
        }
    }
    #endregion
}

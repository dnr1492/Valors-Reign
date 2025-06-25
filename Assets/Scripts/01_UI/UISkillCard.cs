using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static EnumClass;

/* 
[방향 offset 참고]
위	    (0, -1)
아래	(0, 1)
오른쪽	(1, 0)
왼쪽	(-1, 0)
좌상	(-1, -1)
우하	(1, 1)
*/

public class UISkillCard : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI txtName, txtSkillEffect;
    [SerializeField] Image imgSkill;
    [SerializeField] RectTransform hexContainer;
    [SerializeField] GameObject hexPrefab;

    private readonly List<GameObject> skillHexes = new();
    private readonly Dictionary<(int dq, int dr), GameObject> hexMap = new();

    private readonly float spacingX = 1f;
    private readonly float spacingY = 0.5f;
    private readonly float hexScale = 1.1f;
    private readonly int visualOffset = 2;  //Hex 생성을 위, 아래 2칸씩 더 추가 

    public void Set(Sprite sprite, SkillCardData skillCardData)
    {
        CreateSkillHexGrid();
        ShowSkillHexRange(new TempSkillCardData
        {
            name = "Heal Zone",
            effect = "Restores HP",
            rangeType = SkillRangeType.LineForward2,
            customOffsetRange = new List<(int, int, Color)> {
                (0, 0, Color.gray),
                (1, 0, Color.red),
                (-1, 1, Color.green)
            }
        });

        txtName.text = skillCardData.name;
        imgSkill.sprite = sprite;
        txtSkillEffect.text = skillCardData.effect;
    }

    private void CreateSkillHexGrid()
    {
        ClearSkillHexGrid();

        float parentWidth = hexContainer.rect.width;
        float parentHeight = hexContainer.rect.height;

        float baseHexWidth = 24f;
        float baseHexHeight = baseHexWidth * Mathf.Sqrt(3f) / 2f;

        float unitWidth = baseHexWidth * 0.75f + spacingX;
        float unitHeight = baseHexHeight + spacingY;

        int cols = Mathf.FloorToInt((parentWidth + spacingX) / unitWidth);
        int rows = Mathf.FloorToInt((parentHeight + spacingY) / unitHeight);

        int halfCols = cols / 2;
        int halfRows = rows / 2;

        float hexWidth = (parentWidth - spacingX * (cols - 1)) / (cols * 0.75f + 0.25f);
        float hexHeight = hexWidth * Mathf.Sqrt(3f) / 2f;

        hexWidth *= hexScale;
        hexHeight *= hexScale;

        for (int dq = -halfCols; dq <= halfCols; dq++)
        {
            for (int dr = -(halfRows + visualOffset); dr <= (halfRows + visualOffset); dr++)
            {
                var hex = Instantiate(hexPrefab, hexContainer);
                hex.GetComponent<HexTile>().ShowDecorations(false);
                skillHexes.Add(hex);
                hexMap[(dq, dr)] = hex;

                var rt = hex.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(hexWidth, hexHeight);

                float x = dq * (hexWidth * 0.75f + spacingX);
                float y = dr * (hexHeight + spacingY) + (dq % 2 != 0 ? (hexHeight + spacingY) / 2f : 0f);

                rt.anchoredPosition = new Vector2(x, -y);
            }
        }
    }

    private void ClearSkillHexGrid()
    {
        foreach (var hex in skillHexes) Destroy(hex);
        skillHexes.Clear();
        hexMap.Clear();
    }

    private void ShowSkillHexRange(TempSkillCardData data)
    {
        List<(int dq, int dr, Color color)> range = new();

        switch (data.rangeType)
        {
            case SkillRangeType.LineForward1:
                range.AddRange(GetLine((0, -1), 1, Color.red));
                break;

            case SkillRangeType.LineForward2:
                range.AddRange(GetLine((0, -1), 2, Color.red));
                break;

            case SkillRangeType.LineForward3:
                range.AddRange(GetLine((0, -1), 3, Color.red));
                break;

            case SkillRangeType.Custom:
                range = data.customOffsetRange;
                break;
        }

        //먼저 전체 범위 표시 (중심 제외)
        foreach (var (dq, dr, color) in range)
        {
            if (dq == 0 && dr == 0) continue;

            if (hexMap.TryGetValue((dq, dr), out var hex))
                hex.GetComponent<HexTile>().SetColor(color);
        }

        //중심은 항상 회색
        if (hexMap.TryGetValue((0, 0), out var centerHex))
            centerHex.GetComponent<HexTile>().SetColor(Color.gray);
    }

    // ============== 1. 범위 표시하는 스크립트 따로 만들어두는게 어때?? Controller도 괜찮고.. ================= //
    // ============== 2. Craete Hex의 Container가 0x0인 상태로 호출돼서 첫 생성이 이상하게 되는 버그... ================= //
    private List<(int dq, int dr, Color color)> GetLine((int dq, int dr) direction, int length, Color color)
    {
        var result = new List<(int, int, Color)>();
        int q = 0, r = 0;
        for (int i = 1; i <= length; i++)
        {
            q += direction.dq;
            r += direction.dr;
            result.Add((q, r, color));
        }
        return result;
    }

    public class TempSkillCardData
    {
        public string name;
        public string effect;
        public SkillRangeType rangeType;
        public List<(int dq, int dr, Color color)> customOffsetRange;
    }
}

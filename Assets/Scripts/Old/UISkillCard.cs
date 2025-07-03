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
    [SerializeField] TextMeshProUGUI txtTotalSkillCount, txtSelectedSkillCount;
    [SerializeField] TextMeshProUGUI txtSkillRank, txtSkillType, txtSkillName, txtSkillEffect;
    [SerializeField] Image imgSkill;
    [SerializeField] Button btnDecrease, btnIncrease;
    [SerializeField] Button btnEnlargeImage, btnEnlargeRange;
    [SerializeField] RectTransform hexContainer;
    [SerializeField] GameObject hexPrefab;

    private readonly List<GameObject> skillHexes = new();
    private readonly Dictionary<(int dq, int dr), GameObject> hexMap = new();

    private readonly float spacingX = 1f;
    private readonly float spacingY = 0.5f;
    private readonly float hexScale = 1.1f;
    private readonly int visualOffset = 2;  //Hex 생성을 위, 아래 2칸씩 더 추가

    private int count = 0;
    private int skillId = -1;
    public int GetSkillId() => skillId;
    public int GetCount() => count;

    public void Set(Sprite sprite, SkillCardData skillCardData, int initialCount)
    {
        CreateSkillHexGrid();

        ShowSkillHexRange(new TempSkillCardData
        {
            name = "Heal Zone",
            effect = "Restores HP",
            rangeType = SkillRangeType.Ring1,
            customOffsetRange = new List<(int, int, Color)> {
                (0, 0, Color.gray),
                (1, 0, Color.red),
                (-1, 1, Color.green)
            }
        });

        txtSkillRank.text = skillCardData.rank.ToString();
        txtSkillType.text = skillCardData.type.ToString();
        txtSkillName.text = skillCardData.name;
        txtSkillEffect.text = skillCardData.effect;
        imgSkill.sprite = sprite;

        btnDecrease.onClick.RemoveAllListeners();
        btnIncrease.onClick.RemoveAllListeners();
        btnEnlargeImage.onClick.RemoveAllListeners();
        btnEnlargeRange.onClick.RemoveAllListeners();

        btnDecrease.onClick.AddListener(OnClickDecrease);
        btnIncrease.onClick.AddListener(OnClickIncrease);
        btnEnlargeImage.onClick.AddListener(OnClickEnlargeImage);
        btnEnlargeRange.onClick.AddListener(OnClickEnlargeRange);

        skillId = skillCardData.id;
        count = Mathf.Clamp(initialCount, 0, 4);
        txtSelectedSkillCount.text = count.ToString();
    }

    #region Skill HexGrid 생성
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
    #endregion

    #region Skill HexGrid 초기화
    private void ClearSkillHexGrid()
    {
        foreach (var hex in skillHexes) Destroy(hex);
        skillHexes.Clear();
        hexMap.Clear();
    }
    #endregion

    #region HexGrid에 스킬 범위를 표시
    private void ShowSkillHexRange(TempSkillCardData data)
    {
        List<(int dq, int dr, Color color)> range = new();

        switch (data.rangeType)
        {
            case SkillRangeType.LineForward1:
                range.AddRange(SkillRangeHelper.GetLine((0, -1), 1, Color.red));
                break;

            case SkillRangeType.LineForward2:
                range.AddRange(SkillRangeHelper.GetLine((0, -1), 2, Color.red));
                break;

            case SkillRangeType.LineForward3:
                range.AddRange(SkillRangeHelper.GetLine((0, -1), 3, Color.red));
                break;

            case SkillRangeType.Ring1:
                //range.Add((0, 0, Color.gray));  //중심은 회색
                range.AddRange(SkillRangeHelper.GetRing(1, Color.green));  //1칸, ring은 초록
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
    #endregion

    private void OnClickDecrease()
    {
        if (count > 0) {
            count--;
            txtSelectedSkillCount.text = count.ToString();
        }
    }

    private void OnClickIncrease()
    {
        if (count < 4) {
            count++;
            txtSelectedSkillCount.text = count.ToString();
        }
    }

    private void OnClickEnlargeImage() { }

    private void OnClickEnlargeRange() { }

    // ============== 1. Craete Hex의 Container가 0x0인 상태로 호출돼서 첫 생성이 이상하게 되는 버그... ================= //
    // ============== 2. 원형의 경우 제대로 적용이 안되는 버그... ================= //
    // ============== 3. TempSkillCardData를 Tool의 SkillData에 반영 필요... =============================== //

    public class TempSkillCardData
    {
        public string name;
        public string effect;
        public SkillRangeType rangeType;
        public List<(int dq, int dr, Color color)> customOffsetRange;
    }
}

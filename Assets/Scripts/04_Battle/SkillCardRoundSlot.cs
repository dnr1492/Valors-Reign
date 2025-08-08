using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SkillCardRoundSlot : MonoBehaviour
{
    [SerializeField] Image slotHighlightsBg;  //Drop이 가능한 RoundZone의 하이라이트용

    private Color emptyHighlight = new Color(0f, 1f, 0f, 0.35f);    //빈 슬롯: 초록
    private Color swapHighlight = new Color(1f, 0.6f, 0f, 0.35f);   //스왑: 주황
    private Color normalColor = new Color(1f, 1f, 1f, 0.5f);        //평상시: 반투명

    public SkillCardData AssignedSkillCardData { get; private set; }
    public bool IsEmpty => AssignedSkillCardData == null;
    public void ShowEmptyHighlight() => ShowHighlight(emptyHighlight);
    public void ShowSwapHighlight() => ShowHighlight(swapHighlight);

    #region CardZone에 스킬카드 셋팅
    public void Assign(SkillCardData data, SkillCardEvent drag = null)
    {
        AssignedSkillCardData = data;

        if (drag != null) {
            //드래그된 카드를 이 슬롯 아래로 붙인다 (원본 카드를 그대로 옮기는 방식)
            drag.transform.SetParent(transform, false);
            var rt = (RectTransform)drag.transform;
            rt.anchoredPosition = Vector2.zero;
        }
    }
    #endregion

    #region CardZone Highlight Show/Hide
    private void ShowHighlight(Color c)
    {
        if (slotHighlightsBg == null) return;
        slotHighlightsBg.color = c;

        //카드보다 항상 위에 보이게
        slotHighlightsBg.transform.SetAsLastSibling();
    }

    public void HideHighlight()
    {
        if (slotHighlightsBg == null) return;
        slotHighlightsBg.color = normalColor;

        //카드보다 항상 뒤에 보이게
        slotHighlightsBg.transform.SetSiblingIndex(0);
    }
    #endregion

    public void Clear()
    {
        AssignedSkillCardData = null;
    }

    // ================================ 구현 중 ================================ //
    // ================================ 구현 중 ================================ //
    // ================================ 구현 중 ================================ //

    public bool CanAccept()
    {
        // ===== 필요하면 제약(예: 이동카드 금지 등) 체크 ===== //
        return true;
    }
}

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SkillCardRoundSlot : MonoBehaviour
{
    [SerializeField] Image slotHighlightsBg;  //Drop�� ������ RoundZone�� ���̶���Ʈ��

    private Color emptyHighlight = new Color(0f, 1f, 0f, 0.35f);   //�� ����: �ʷ�
    private Color swapHighlight = new Color(1f, 0.6f, 0f, 0.35f);  //����: ��Ȳ
    private Color normalColor = new Color(1f, 1f, 1f, 0.5f);       //����: ������

    public SkillCardData AssignedSkillCardData { get; private set; }
    public bool IsEmpty => AssignedSkillCardData == null;
    public void ShowEmptyHighlight() => ShowHighlight(emptyHighlight);
    public void ShowSwapHighlight() => ShowHighlight(swapHighlight);

    #region RoundSlot�� ��ġ
    public void Assign(SkillCardData data, SkillCardEvent drag = null)
    {
        AssignedSkillCardData = data;

        if (drag != null)
        {
            //�巡�׵� ī�带 �� ���� �Ʒ��� ���δ� (���� ī�带 �״�� �ű�� ���)
            drag.transform.SetParent(transform, false);
            var rt = (RectTransform)drag.transform;
            rt.anchoredPosition = Vector2.zero;  //ī�� ���߾� ��ġ
        }

        //�⺻ �̵�ī�尡 �ƴ� ������ �ٲ�ų�, ������ ����� ��� �� ������ �̵� ����/���� ����
        if (AssignedSkillCardData == null || AssignedSkillCardData.id != 1000)
            ControllerRegister.Get<MovementOrderController>().ReleaseReservation(this);
    }
    #endregion

    #region RoundSlot���� ���� (������ ���� + ���� ����)
    public void Clear()
    {
        AssignedSkillCardData = null;

        //���� ����� �� �̵� ����/���� ����
        ControllerRegister.Get<MovementOrderController>().ReleaseReservation(this);
    }
    #endregion

    #region RoundSlot Highlight Show/Hide
    private void ShowHighlight(Color c)
    {
        if (slotHighlightsBg == null) return;
        slotHighlightsBg.color = c;

        //ī�庸�� �׻� ���� ���̰�
        slotHighlightsBg.transform.SetAsLastSibling();
    }

    public void HideHighlight()
    {
        if (slotHighlightsBg == null) return;
        slotHighlightsBg.color = normalColor;

        //ī�庸�� �׻� �ڿ� ���̰�
        slotHighlightsBg.transform.SetSiblingIndex(0);
    }
    #endregion

    #region ���� ���� ����
    private int roundOrder = 0;
    public int GetRoundOrder() => roundOrder;
    public void SetRoundOrder(int order) => roundOrder = order;
    #endregion
}
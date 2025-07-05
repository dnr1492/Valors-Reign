using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static EnumClass;

public class UISkillInfoPopup : UIPopupBase
{
    [SerializeField] GameObject uiSkillCardPrefab;
    [SerializeField] Transform cardContainer;
    [SerializeField] Button btn_close;

    private readonly List<UISkillCard> skillCardPool = new();
    private CharacterCard targetCharacterCard;

    [Header("Overlay Enlarge")]
    [SerializeField] GameObject overlayPreventClick;
    [SerializeField] GameObject overlayEnlargedImage, overlayEnlargedRange;
    [SerializeField] Button btn_close_image, btn_close_range;
    [SerializeField] RectTransform hexContainer;
    [SerializeField] GameObject hexPrefab;

    private readonly List<GameObject> enlargedSkillHexes = new();
    private readonly Dictionary<(int dq, int dr), GameObject> enlargedSkillHexMap = new();

    private void Awake()
    {
        overlayEnlargedImage.SetActive(false);
        overlayEnlargedRange.SetActive(false);
        overlayPreventClick.SetActive(false);

        btn_close_image.onClick.AddListener(() => CloseOverlay(overlayEnlargedImage));
        btn_close_range.onClick.AddListener(() => CloseOverlay(overlayEnlargedRange));
    }

    public void Init(List<SkillCardData> skillDataList, CharacterCard characterCard)
    {
        targetCharacterCard = characterCard;

        //필요한 만큼 활성화, 없으면 새로 생성
        for (int i = 0; i < skillDataList.Count; i++)
        {
            UISkillCard card;
            if (i < skillCardPool.Count)
            {
                card = skillCardPool[i];
                card.gameObject.SetActive(true);
            }
            else
            {
                var go = Instantiate(uiSkillCardPrefab, cardContainer);
                card = go.GetComponent<UISkillCard>();
                skillCardPool.Add(card);
            }

            var skillData = skillDataList[i];
            var sprite = SpriteManager.Instance.dicSkillSprite.TryGetValue(skillData.name, out var sp) ? sp : null;
            card.Set(sprite, skillData, targetCharacterCard.GetSkillCount(skillData.id));
        }

        //사용하지 않는 나머지는 비활성화
        for (int i = skillDataList.Count; i < skillCardPool.Count; i++)
        {
            skillCardPool[i].gameObject.SetActive(false);
        }

        //SkillCard Resize
        ResizeSkillCards(skillCardPool.GetRange(0, skillDataList.Count));

        //닫기
        btn_close.onClick.RemoveAllListeners();
        btn_close.onClick.AddListener(OnClickClose);
    }

    private void ResizeSkillCards(List<UISkillCard> cards)
    {
        int maxCardCount = 4;
        float spacing = 30f;
        float padding = 80f;
        float availableWidth = Screen.width - spacing * (maxCardCount - 1) - padding;
        float cardWidth = availableWidth / maxCardCount;
        float cardHeight = cardWidth * 1.4f;

        foreach (var card in cards)
        {
            if (card.TryGetComponent<LayoutElement>(out var layout))
            {
                layout.preferredWidth = cardWidth;
                layout.preferredHeight = cardHeight;
            }
        }
    }
    
    private void OnClickClose()
    {
        foreach (var card in skillCardPool)
        {
            if (!card.gameObject.activeSelf) continue;

            int skillId = card.GetSkillId();
            int count = card.GetCount();
            targetCharacterCard.SetSkillCountManually(skillId, count);
        }

        Close();
    }

    public void ShowImageOverlay(Sprite sprite)
    {
        overlayEnlargedImage.SetActive(true);
        overlayPreventClick.SetActive(true);

        var image = overlayEnlargedImage.GetComponentInChildren<Image>();
        if (image != null) image.sprite = sprite;
    }

    public void ShowRangeOverlay(SkillCardData skillData)
    {
        overlayPreventClick.SetActive(true);
        overlayEnlargedRange.SetActive(true);

        SkillHexGridHelper.ClearSkillHexGrid(enlargedSkillHexes, enlargedSkillHexMap);
        SkillHexGridHelper.CreateSkillHexGrid(hexContainer, hexPrefab, enlargedSkillHexes, enlargedSkillHexMap, 1.3f);
        SkillHexGridHelper.ShowSkillHexRange(skillData, enlargedSkillHexMap);
    }

    private void CloseOverlay(GameObject target)
    {
        target.SetActive(false);
        overlayPreventClick.SetActive(false);
    }

    protected override void ResetUI() { }

    // ============== 1. Craete Hex의 Container가 0x0인 상태로 호출돼서 첫 생성이 이상하게 되는 버그... ================= //
    // ============== 2. 원형의 경우 제대로 적용이 안되는 버그... ================= //
}

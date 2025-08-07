using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UIBattleSetting : UIPopupBase
{
    [SerializeField] GameObject uiCoinFlipPrefab;
    [SerializeField] Button btn_back;

    private UICoinFlip uiCoinFlip;

    [Header("Hex Grid")]
    [SerializeField] RectTransform hexParantRt /*map*/, battleFieldRt;
    [SerializeField] GameObject hexPrefab;  //육각형 모양의 이미지가 있는 UI 프리팹

    [Header("Setting SkillCard")]
    [SerializeField] Transform skillCardZone;  //SkillCard의 Parant
    [SerializeField] GameObject skillCardPrefab;
    private readonly List<SkillCard> settingSkillCards = new();

    public void Init()
    {
        GridManager.Instance.CreateHexGrid(battleFieldRt, hexPrefab, hexParantRt, false, true);

        UIEditorDeckPhase1 popup = UIManager.Instance.GetPopup<UIEditorDeckPhase1>("UIEditorDeckPhase1");
        var pack = popup.GetSelectedDeckPack();
        if (pack != null) GridManager.Instance.ShowDecksOnField(pack, ControllerRegister.Get<PhotonController>().OpponentDeckPack);

        var go = Instantiate(uiCoinFlipPrefab, transform);
        uiCoinFlip = go.GetComponent<UICoinFlip>();
        uiCoinFlip.Init(OnCoinDirectionSelected);
    }

    private void OnCoinDirectionSelected(int myCoinDriection)
    {
        ControllerRegister.Get<PhotonController>().RequestCoinFlip(myCoinDriection);
    }

    public void ShowCoinFlipResult(int result, bool hasFirstTurnChoice)
    {
        uiCoinFlip.PlayFlipAnimation(result, () => {
            if (hasFirstTurnChoice) uiCoinFlip.ActiveTurnChoiceButton(true);  //선공 or 후공 선택 버튼 표시
            else LoadingManager.Instance.Show("상대가 선공 또는 후공을 선택하는 중입니다...");
        });
    }

    public void DestroyUICoinFlip()
    {
        if (uiCoinFlip != null) {
            Destroy(uiCoinFlip.gameObject);
            uiCoinFlip = null;
        }
    }

    #region 드로우한 스킬카드를 표시
    public void DisplayDrawnSkillCard(List<SkillCardData> drawnSkillCards)
    {
        //이전 카드 정리
        foreach (var card in settingSkillCards) Destroy(card.gameObject);
        settingSkillCards.Clear();

        //새로 생성
        foreach (var skillCardData in drawnSkillCards)
        {
            var go = Instantiate(skillCardPrefab, skillCardZone);
            var sprite = SpriteManager.Instance.dicSkillSprite.TryGetValue(skillCardData.name, out var sp) ? sp : null;
            var skillCard = go.GetComponent<SkillCard>();
            skillCard.Set(sprite, skillCardData);
            settingSkillCards.Add(skillCard);
        }

        SetLayoutSkillCards(settingSkillCards);
        AdjustSkillCardZoonHeight(10, 10);

        Debug.Log($"[UIBattleSetting] 드로우 카드 {settingSkillCards.Count}장 UI에 세팅 완료");
    }

    /// <summary>
    /// 스킬카드를 skillCardZone 안에서 가운데 정렬되도록 배치
    /// 카드 개수가 많을수록 겹쳐지며, Zone 너비를 벗어나지 않도록 spacing을 자동 계산
    /// 카드 인덱스가 작을수록 화면에서 위로 보이도록 SiblingIndex()로 순서 지정
    /// </summary>
    /// <param name="cards"></param>
    private void SetLayoutSkillCards(List<SkillCard> cards)
    {
        float skillCardWidth = 130f;
        float padding = 10f;
        float paddingOverlap = 10f;

        int count = cards.Count;
        if (count == 0) return;

        RectTransform zoneRt = skillCardZone.GetComponent<RectTransform>();
        float availableWidth = zoneRt.rect.width - padding * 2;

        //spacing을 조절해서 카드들이 영역 안에 딱 맞도록 하기
        float spacing;

        if (count == 1) spacing = 0f;
        else
        {
            //전체 카드 + 간격 합이 availableWidth 안에 들어오게 spacing 계산
            spacing = (availableWidth - (skillCardWidth * count)) / (count - 1);
            spacing = Mathf.Clamp(spacing, -skillCardWidth + paddingOverlap, skillCardWidth);  //과도한 겹침 방지
        }

        float layoutWidth = (skillCardWidth * count) + (spacing * (count - 1));
        float startX = -layoutWidth / 2f + skillCardWidth / 2f;  //첫 카드 중심이 기준

        for (int i = 0; i < count; i++)
        {
            var card = cards[i];
            var rt = card.GetComponent<RectTransform>();

            float x = startX + i * (skillCardWidth + spacing);
            rt.anchoredPosition = new Vector2(x, 0);

            //카드 순서 보정 (인덱스가 작을 수록 가장 위)
            rt.SetSiblingIndex(count - 1 - i);
        }
    }

    /// <summary>
    /// 모든 스킬카드 중 가장 높은 카드 기준으로 skillCardZone의 height를 자동 조정
    /// 위, 아래 여백(paddingTop, paddingBottom)은 추가적으로 설정
    /// </summary>
    /// <param name="paddingTop">상단 여백</param>
    /// <param name="paddingBottom">하단 여백</param>
    private void AdjustSkillCardZoonHeight(float paddingTop, float paddingBottom)
    {
        float maxHeight = settingSkillCards.Max(card =>
            card.GetComponent<RectTransform>().rect.height);

        float finalHeight = maxHeight + paddingTop + paddingBottom;

        var rt = skillCardZone.GetComponent<RectTransform>();
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, finalHeight);
    }
    #endregion

    protected override void ResetUI() { }

    // ================================ 구현 중 ================================ //
    // ================================ 구현 중 ================================ //
    // ================================ 구현 중 ================================ //
}
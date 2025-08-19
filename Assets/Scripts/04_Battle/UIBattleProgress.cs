using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;

public class UIBattleProgress : UIPopupBase
{
    [SerializeField] RectTransform[] progressMyRoundSlots = new RectTransform[4];
    [SerializeField] RectTransform[] progressOpponentRoundSlots = new RectTransform[4];
    [SerializeField] GameObject skillCardPrefab;
    [SerializeField] TextMeshProUGUI txt_roundIndex;
    [SerializeField] RectTransform[] myGuideAnchors = new RectTransform[4];
    [SerializeField] RectTransform[] oppGuideAnchors = new RectTransform[4];
    private readonly float guideMoveDuration = 0.35f;
    private readonly AnimationCurve guideMoveEase = null;

    //라운드 슬롯 초기화
    private void ClearSlots(RectTransform[] slots)
    {
        if (slots == null) return;
        foreach (var rt in slots)
        {
            if (rt == null) continue;
            for (int i = rt.childCount - 1; i >= 0; i--) Destroy(rt.GetChild(i).gameObject);
        }
    }

    //UIBattleSetting의 라운드 슬롯에서 스킬카드를 복제해와서 표시
    private void DisplyCloneFromSettingSlots(UIBattleSetting setting, RectTransform[] dstSlots)
    {
        if (setting == null || dstSlots == null) return;
        var srcSlots = setting.GetRoundSlots();
        if (srcSlots == null) return;

        for (int i = 0; i < dstSlots.Length && i < srcSlots.Length; i++)
        {
            var dst = dstSlots[i];
            var srcSlot = srcSlots[i];
            if (dst == null || srcSlot == null) continue;

            var srcEvt = srcSlot.GetComponentInChildren<SkillCardEvent>();
            if (srcEvt == null || srcEvt.SkillCardData == null) continue;

            //현재 슬롯에 보이는 모양(캐릭터 이미지로 바뀐 기본 이동카드 포함)을 그대로 복제
            var clone = Instantiate(srcEvt.gameObject, dst);
            var rt = clone.GetComponent<RectTransform>();
            rt.anchoredPosition = Vector2.zero;

            var evt = clone.GetComponent<SkillCardEvent>();
            if (evt != null) evt.enabled = false;
        }
    }

    //[AI 대전] 상대 덱에서 4장 랜덤 뽑아 표시
    //기본 이동카드(1000)일 경우 해당 카드 이미지를 '상대 캐릭터' 이미지로 교체
    private void BuildOpponentRandomCardsForAI(RectTransform[] dstSlots)
    {
        if (dstSlots == null) return;

        var photon = ControllerRegister.Get<PhotonController>();
        var oppDeck = photon != null ? photon.OpponentDeckPack : null;
        if (oppDeck == null || skillCardPrefab == null) return;

        //1) 카드 풀 구성 (상대 덱의 스킬카드 기반)
        var pool = new List<SkillCardData>();
        foreach (var slot in oppDeck.tokenSlots)
        {
            foreach (var sc in slot.skillCounts)
            {
                if (DataManager.Instance.dicSkillCardData.TryGetValue(sc.skillId, out var data))
                {
                    for (int c = 0; c < sc.count; c++) pool.Add(data);
                }
            }
        }

        //2) 상대 캐릭터 후보 (이미 전장에 배치된 상대 덱의 tokenKey들)
        var oppTokenKeys = oppDeck.tokenSlots.Select(t => t.tokenKey).ToList();

        //3) 4칸 채우기
        for (int i = 0; i < dstSlots.Length; i++)
        {
            var dst = dstSlots[i];
            if (dst == null) continue;

            //랜덤 스킬카드 1장
            var pick = pool[Random.Range(0, pool.Count)];
            var go = Instantiate(skillCardPrefab, dst);
            var sc = go.GetComponent<SkillCard>();
            var sprite = SpriteManager.Instance.dicSkillSprite.TryGetValue(pick.name, out var sp) ? sp : null;
            sc.Set(sprite, pick);

            //기본 이동카드면 '상대 캐릭터 이미지'로 교체
            if (pick.id == 1000 && oppTokenKeys.Count > 0)
            {
                int tokenKey = oppTokenKeys[Random.Range(0, oppTokenKeys.Count)];
                var token = ControllerRegister.Get<CharacterTokenController>()
                    .GetAllCharacterToken()
                    .FirstOrDefault(t => t.Key == tokenKey);
                if (token != null)
                    sc.SetCharacterImageIfMoveCard(token.GetCharacterSprite());
            }

            //상호작용 비활성
            var evt = go.GetComponent<SkillCardEvent>();
            if (evt != null) evt.enabled = false;

            var rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = Vector2.zero;
        }
    }

    //양쪽 라운드 슬롯에 스킬카드 표시
    public void DisplayBoth(UIBattleSetting mySetting, UIBattleSetting oppSetting, bool isPvP)
    {
        ClearSlots(progressMyRoundSlots);
        ClearSlots(progressOpponentRoundSlots);

        //내 쪽
        DisplyCloneFromSettingSlots(mySetting, progressMyRoundSlots);

        //상대 쪽
        //유저 대전
        if (isPvP) DisplyCloneFromSettingSlots(oppSetting, progressOpponentRoundSlots);
        //AI 대전
        else if (!isPvP) BuildOpponentRandomCardsForAI(progressOpponentRoundSlots);
    }

    //내 셋팅을 진행 화면의 나의 라운드 슬롯에 복제 표시
    public void DisplayMyRoundCards(UIBattleSetting mySetting)
    {
        ClearSlots(progressMyRoundSlots);
        DisplyCloneFromSettingSlots(mySetting, progressMyRoundSlots);
    }

    //상대 셋팅을 진행 팝업의 상대 라운드 슬롯에 복제 표시
    public void DisplayOpponentRoundCards(OppRoundPlan plan)
    {
        ClearSlots(progressOpponentRoundSlots);
        if (plan.cards == null) return;

        foreach (var info in plan.cards)
        {
            int idx = Mathf.Clamp(info.round - 1, 0, progressOpponentRoundSlots.Length - 1);
            var dst = progressOpponentRoundSlots[idx];
            if (dst == null) continue;

            //카드 프리팹 생성
            var go = Instantiate(skillCardPrefab, dst);
            var sc = go.GetComponent<SkillCard>();

            //기본 스킬 스프라이트
            Sprite baseSp = null;
            if (DataManager.Instance.dicSkillCardData.TryGetValue(info.cardId, out var cardData))
                SpriteManager.Instance.dicSkillSprite.TryGetValue(cardData.name, out baseSp);

            sc.Set(baseSp, DataManager.Instance.dicSkillCardData[info.cardId]);

            //기본 이동카드(1000)면 moveTokenKey의 캐릭터 스프라이트로 교체
            if (info.cardId == 1000 && info.moveTokenKey >= 0)
            {
                var token = ControllerRegister.Get<CharacterTokenController>()
                            .GetAllCharacterToken()
                            .FirstOrDefault(t => t.Key == info.moveTokenKey);
                if (token != null)
                    sc.SetCharacterImageIfMoveCard(token.GetCharacterSprite());
            }

            //진행 화면은 상호작용 비활성
            var evt = go.GetComponent<SkillCardEvent>();
            if (evt) evt.enabled = false;

            var rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = Vector2.zero;
        }
    }

    #region 나/상대 진행 슬롯에 있는 스킬카드를 가이드까지 이동 (feat.애니메이션)
    //선공/후공에 따라 둘 다 순차 실행
    public async UniTask AnimateRoundIntroAsync(int roundIndex, bool iAmFirst)
    {
        //나/상대 진행 슬롯 루트들을 넘겨서 레이아웃 먼저 고정
        //꼬임 방지
        await EnsureLayoutStabilized(progressMyRoundSlots);
        await EnsureLayoutStabilized(progressOpponentRoundSlots);

        if (iAmFirst)
        {
            await AnimateRoundStartToGuideAsync(roundIndex, true);   //내 카드 먼저
            await AnimateRoundStartToGuideAsync(roundIndex, false);  //그 다음 상대
        }
        else
        {
            await AnimateRoundStartToGuideAsync(roundIndex, false);  //상대 카드 먼저
            await AnimateRoundStartToGuideAsync(roundIndex, true);   //그 다음 내 카드
        }
    }

    //라운드 n의 카드 하나(나/상대)를 해당 n의 가이드까지 이동
    private async UniTask AnimateRoundStartToGuideAsync(int roundIndex, bool isMine)
    {
        int idx = Mathf.Clamp(roundIndex - 1, 0, 3);

        RectTransform slot = isMine ? progressMyRoundSlots?[idx] : progressOpponentRoundSlots?[idx];
        RectTransform guide = isMine ? myGuideAnchors?[idx] : oppGuideAnchors?[idx];
        if (slot == null || guide == null) return;

        var card = FindCardRectInSlot(slot);
        if (card == null) return;

        await AnimateCardToGuideAsync(card, guide);
    }

    //가이드까지 실제 이동
    private async UniTask AnimateCardToGuideAsync(RectTransform card, RectTransform guide)
    {
        if (!card || !guide) return;

        Vector3 start = card.position;
        Vector3 end = guide.position;
        Vector3 startScale = card.localScale;

        float dur = Mathf.Max(0.0001f, guideMoveDuration);
        var ease = guideMoveEase ?? AnimationCurve.EaseInOut(0, 0, 1, 1);

        float t = 0f;
        while (t < 1f)
        {
            if (!card || !guide) return;

            t += Time.deltaTime / dur;
            float e = ease.Evaluate(Mathf.Clamp01(t));

            card.position = Vector3.Lerp(start, end, e);

            await UniTask.Yield(PlayerLoopTiming.Update);
        }

        if (!card || !guide) return;
        card.position = end;
        card.localScale = startScale;
    }

    //슬롯에서 스킬카드 RectTransform을 찾아온다.
    private RectTransform FindCardRectInSlot(RectTransform slot)
    {
        if (!slot) return null;

        for (int i = 0; i < slot.childCount; i++)
        {
            var child = slot.GetChild(i) as RectTransform;
            if (!child) continue;

            if (child.GetComponent<SkillCard>() != null)
                return child;
        }
        return null;
    }

    private async UniTask EnsureLayoutStabilized(params RectTransform[] roots)
    {
        await UniTask.NextFrame();  //1프레임 밀기
        Canvas.ForceUpdateCanvases();
        if (roots != null)
        {
            for (int i = 0; i < roots.Length; i++)
                if (roots[i]) LayoutRebuilder.ForceRebuildLayoutImmediate(roots[i]);
        }

        //레이아웃 최종 반영 프레임까지 밀기
        await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
        Canvas.ForceUpdateCanvases();
    }
    #endregion

    //[UI] 현재 라운드 표시
    public void DisplayRound(int roundIndex)
    {
        txt_roundIndex.text = $"현재 {roundIndex} 라운드가 진행 중입니다.";
    }

    protected override void ResetUI() { }
}
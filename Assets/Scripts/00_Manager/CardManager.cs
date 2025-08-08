using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CardManager : Singleton<CardManager>
{
    private readonly List<SkillCardData> totalSkillCards = new();  //전체 스킬카드
    private readonly List<SkillCardData> drawnSkillCards = new();  //드로우된 스킬카드 (+기본 이동카드)

    public List<SkillCardData> GetDrawnSkillCards() => new(drawnSkillCards);

    protected override void Awake()
    {
        base.Awake();
    }

    public void InitDeckFromDeckPack(DeckPack pack)
    {
        totalSkillCards.Clear();

        foreach (var slot in pack.tokenSlots)
        {
            foreach (var skill in slot.skillCounts)
            {
                int skillId = skill.skillId;
                int count = skill.count;

                for (int i = 0; i < count; i++) {
                    if (DataManager.Instance.dicSkillCardData.TryGetValue(skillId, out var skillCard)) totalSkillCards.Add(skillCard);
                    else Debug.Log($"[CardManager] 존재하지 않는 스킬카드 ID: {skillId}");
                }
            }
        }

        Debug.Log($"[CardManager] 스킬카드로 덱 편성 완료: 총 {totalSkillCards.Count}장");
    }

    public void DrawSkillCards(int count)
    {
        totalSkillCards.Shuffle();
        drawnSkillCards.Clear();

        //0번 인덱스는 이동카드(1000) 고정
        if (DataManager.Instance.dicSkillCardData.TryGetValue(1000, out var moveCard))
            drawnSkillCards.Add(moveCard);

        int drawn = 0;
        foreach (var card in totalSkillCards.ToList())
        {
            if (card.id == 1000) continue;  //이동카드는 제외 (중복 방지)

            drawnSkillCards.Add(card);
            totalSkillCards.Remove(card);
            drawn++;

            if (drawn >= count) break;
        }

        Debug.Log($"[CardManager] 스킬카드 {drawn}장 + 이동카드 1장 드로우");
    }

    // ================================ 구현 중 ================================ //
    // ================================ 구현 중 ================================ //
    // ================================ 구현 중 ================================ //

    public void RecollectUnusedSkillCards(List<SkillCardData> unused)
    {
        totalSkillCards.AddRange(unused);
        totalSkillCards.Shuffle();
    }

    public void ResetAll()
    {
        totalSkillCards.Clear();
        drawnSkillCards.Clear();
    }
}
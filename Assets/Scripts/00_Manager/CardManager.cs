using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CardManager : Singleton<CardManager>
{
    private readonly List<SkillCardData> totalSkillCards = new();  //전체 스킬카드
    private readonly List<SkillCardData> drawnSkillCards = new();  //드로우된 스킬카드 (+기본 이동카드)
    private readonly Dictionary<int, List<SkillCardData>> poolByAlive = new();  //턴 전환 시 생존 캐릭터의 스킬카드만으로 덱 편성에 사용

    public List<SkillCardData> GetDrawnSkillCards() => new(drawnSkillCards);

    protected override void Awake()
    {
        base.Awake();
    }

    public void InitDeckFromDeckPack(DeckPack pack)
    {
        totalSkillCards.Clear();
        poolByAlive.Clear();

        foreach (var slot in pack.tokenSlots)
        {
            int tokenKey = slot.tokenKey;

            foreach (var skill in slot.skillCounts)
            {
                int skillId = skill.skillId;
                int count = skill.count;

                for (int i = 0; i < count; i++)
                {
                    if (DataManager.Instance.dicSkillCardData.TryGetValue(skillId, out var skillCard))
                    {
                        totalSkillCards.Add(skillCard);
                        if (!poolByAlive.TryGetValue(tokenKey, out var list)) {
                            list = new List<SkillCardData>();
                            poolByAlive[tokenKey] = list;
                        }
                        list.Add(skillCard);
                    }
                    else Debug.Log($"[CardManager] 존재하지 않는 스킬카드 ID: {skillId}");
                }
            }
        }

        Debug.Log($"[CardManager] 스킬카드로 덱 편성 완료: 총 {totalSkillCards.Count}장");
    }

    public void DrawSkillCardsForAliveOwners(IEnumerable<int> aliveKeys, int count)
    {
        RebuildDeckForAliveOwners(aliveKeys);
        DrawSkillCards(count);
    }

    //생존 캐릭터의 스킬카드만 덱으로 편성
    private void RebuildDeckForAliveOwners(IEnumerable<int> aliveKeys)
    {
        totalSkillCards.Clear();
        foreach (var key in aliveKeys)
        {
            if (poolByAlive.TryGetValue(key, out var list))
                totalSkillCards.AddRange(list);
        }
    }

    private void DrawSkillCards(int count)
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
}
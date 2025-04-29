using System.Collections.Generic;
using UnityEngine;
using static EnumClass;

public class CardController : MonoBehaviour
{
    // 1) 총 코스트 제한
    // 2) 티어별 선택 수 제한
    // 3) Low 티어는 동일 카드 최대 3장까지, Middle 티어는 동일 카드 최대 2장까지 중복 허용 및 그 외에는 중복 제한
    [Header("Character Card")]
    private int maxCost;  //최대 Cost
    private int curTotalCost;  //현재 Cost
    private List<int> selectedCharacterCardIDs;  //선택한 캐릭터 카드 ID
    private Dictionary<int, int> selectedCharacterCardCounts;  //선택한 캐릭터 카드 개수 - (ID, 개수)
    private Dictionary<string, int> characterCardTierMaxCounts;  //캐릭터 카드 Tier별 최대 개수 - (Tier, 개수)
    private Dictionary<string, int> characterCardTierCurrentCounts;  //캐릭터 카드 Tier별 현재 개수 - (Tier, 개수)
    
    [Header("Skill Card")]
    [SerializeField] GameObject skillCardPrefab;
    [SerializeField] RectTransform skillCardParant;
    private List<SkillCardData> deck = new List<SkillCardData>();  //필드에 있는 스킬 카드 덱
    private List<GameObject> handCards = new List<GameObject>();  //손에 있는 스킬 카드들

    private void Start()
    {
        var gamePlayData = DataManager.Instance.gamePlayData;
        maxCost = gamePlayData.maxCost;
        curTotalCost = 0;

        selectedCharacterCardIDs = new List<int>();
        selectedCharacterCardCounts = new Dictionary<int, int>();
        characterCardTierMaxCounts = new Dictionary<string, int>() {
            { CharacterTierAndCost.Leader.ToString(), gamePlayData.maxLeaderCount },
            { CharacterTierAndCost.High.ToString(), gamePlayData.maxHighTierCount },
            { CharacterTierAndCost.Middle.ToString(), gamePlayData.maxMiddleTierCount },
            { CharacterTierAndCost.Low.ToString(), gamePlayData.maxLowTierCount },
        };
        characterCardTierCurrentCounts = new Dictionary<string, int>() {
            { CharacterTierAndCost.Leader.ToString(), 0 },
            { CharacterTierAndCost.High.ToString(), 0 },
            { CharacterTierAndCost.Middle.ToString(), 0 },
            { CharacterTierAndCost.Low.ToString(), 0 },
        };
    }

    //// ===== 최초 캐릭터 선택을 위해서 모든 캐릭터 카드 데이터를 가지고 표출할 지 어떻게 할 지는 UI 정해지면 구현하기 ===== //
    //// ===== *** Leader는 무조건 1개 *** ===== //
    //// ===== *** 캐릭터 카드는 토큰 형식의 5각형으로서 필드에 셋팅 *** ===== //
    //private void CreateCharacterCard()
    //{
    //}

    #region (Toggle) 캐릭터 카드 선택
    public bool SelectCharacterCard(int cardID)
    {
        var dataManager = DataManager.Instance;
        var cardData = dataManager.dicCharacterCardData[cardID];
        var tier = cardData.tier;
        int cost = (int)tier;

        //현재 선택 수 확인
        int currentCardCount = selectedCharacterCardCounts.TryGetValue(cardID, out var cardCount) ? cardCount : 0;
        int currentTierCount = characterCardTierCurrentCounts[tier.ToString()];
        int maxTierCount = characterCardTierMaxCounts[tier.ToString()];

        //중복 선택 제한 체크
        switch (System.Enum.Parse<CharacterTierAndCost>(tier.ToString()))
        {
            case CharacterTierAndCost.Low:
                if (currentCardCount >= dataManager.gamePlayData.limitLow) return false;
                break;
            case CharacterTierAndCost.Middle:
                if (currentCardCount >= dataManager.gamePlayData.limitMiddle) return false;
                break;
            default:
                if (currentCardCount > 0) return false;
                break;
        }

        //티어별 수 제한 체크
        if (currentTierCount >= maxTierCount) return false;

        //코스트 초과 체크
        if (curTotalCost + cost > maxCost) return false;

        //캐릭터 선택 처리
        if (currentCardCount == 0) selectedCharacterCardIDs.Add(cardID);
        selectedCharacterCardCounts[cardID] = currentCardCount + 1;
        characterCardTierCurrentCounts[tier.ToString()]++;
        curTotalCost += cost;

        return true;
    }
    #endregion

    #region (Toggle) 캐릭터 카드 선택 해제
    public bool UnselectCharacterCard(int cardID)
    {
        if (!selectedCharacterCardCounts.TryGetValue(cardID, out var count) || count <= 0)
            return false;

        var cardData = DataManager.Instance.dicCharacterCardData[cardID];
        var tier = cardData.tier;
        int cost = (int)tier;

        //개수 감소
        if (--count == 0) {
            selectedCharacterCardCounts.Remove(cardID);
            selectedCharacterCardIDs.Remove(cardID);
        }
        else {
            selectedCharacterCardCounts[cardID] = count;
        }

        characterCardTierCurrentCounts[tier.ToString()]--;
        curTotalCost -= cost;

        return true;
    }
    #endregion

    #region 선택한 캐릭터 카드에 대한 스킬 카드를 가져와서 섞어서 셋팅
    public void SettingDeck()
    {
        var selectedCharacterCards = GetSelectedCharacterCard();

        var allSkillCardData = DataManager.Instance.dicSkillCardData;
        List<SkillCardData> skillCardDeck = new List<SkillCardData>();

        foreach (var characterCard in selectedCharacterCards) {
            foreach (var skill in characterCard.skills) {
                skillCardDeck.Add(allSkillCardData[skill]);
            }
        }

        foreach (var data in skillCardDeck) {
            Debug.Log("덱에 추가할 스킬 카드 : " + data.name + " / " + data.rank);
        }

        //카드들을 셔플하여 덱을 랜덤화
        ShuffleDeck(skillCardDeck);
    }

    private List<CharacterCardData> GetSelectedCharacterCard()
    {
        var allCards = DataManager.Instance.dicCharacterCardData;
        List<CharacterCardData> selectedCharacterCards = new List<CharacterCardData>();

        foreach (var cardID in selectedCharacterCardCounts.Keys) {
            if (allCards.TryGetValue(cardID, out var cardData)) {
                selectedCharacterCards.Add(cardData);
            }
        }

        return selectedCharacterCards;
    }

    private void ShuffleDeck(List<SkillCardData> cards)
    {
        deck.Clear();

        //카드 셔플 (Fisher-Yates 알고리즘)
        for (int i = 0; i < cards.Count; i++)
        {
            int random = Random.Range(i, cards.Count);
            SkillCardData temp = cards[i];
            cards[i] = cards[random];
            cards[random] = temp;
        }

        //셔플된 카드를 덱에 추가
        deck.AddRange(cards);
    }
    #endregion

    #region 덱에서 스킬 카드를 뽑기
    private void DrawCardsFromDeck(int drawCount)
    {
        //덱에서 특정 개수만큼 카드를 드로우
        for (int i = 0; i < drawCount && deck.Count > 0; i++)
        {
            //덱에서 맨 첫 번째 카드를 드로우
            SkillCardData drawnCard = deck[0];
            //드로우한 카드는 덱에서 제거
            deck.RemoveAt(0);  
            //드로우한 카드를 패에 표시
            CreateHandCard(drawnCard);
        }
    }

    private void CreateHandCard(SkillCardData card)
    {
        GameObject cardGo = Instantiate(skillCardPrefab, skillCardParant);
        Card cardScript = cardGo.GetComponent<Card>();
        cardScript.InitCardData(card, State.Front, CardType.SkillCard);

        //패에 카드 배치 (UI 처리)
        handCards.Add(cardGo);
    }
    #endregion
}
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CardController : MonoBehaviour
{
    private List<SkillCardData> deck = new List<SkillCardData>();  //스킬 덱

    //[SerializeField] GameObject cardPrefab;
    //private List<GameObject> handCards = new List<GameObject>();  //패에 있는 스킬 카드들

    private void Start()
    {
        SettingDeck();
    }

    private void SettingDeck()
    {
        var selectedCharacterCards = TempSelectCard();

        var allSkillCardData = DataManager.GetInstance().dicSkillCardData;
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

    //private void DrawCardsFromDeck(int numberOfCards)
    //{
    //    // 덱에서 특정 개수만큼 카드를 드로우
    //    for (int i = 0; i < numberOfCards && deck.Count > 0; i++)
    //    {
    //        SkillCardData drawnCard = deck[0];  // 덱에서 첫 번째 카드 드로우
    //        deck.RemoveAt(0);  // 드로우한 카드는 덱에서 제거

    //        // 드로우한 카드를 패에 표시
    //        CreateCard(drawnCard);
    //    }
    //}

    //private void CreateCard(SkillCardData cardData)
    //{
    //    // 카드 프리팹 생성
    //    GameObject cardGo = Instantiate(cardPrefab);

    //    // 카드 스크립트 가져오기
    //    Card cardScript = cardGo.GetComponent<Card>();
    //    cardScript.SetCardData(cardData);

    //    // 패에 카드 배치 (UI 처리)
    //    handCards.Add(cardGo);
    //    cardGo.SetActive(true);  // 패에 있는 카드는 활성화
    //}

    // ===== 추후 캐릭터 카드 선택 로직 구현 필요 - 현재는 임시 ===== //
    private List<CharacterCardData> TempSelectCard()
    {
        List<CharacterCardData> selectedCharacterCards = new List<CharacterCardData>();
        var allCards = DataManager.GetInstance().dicCharacterCardData;
        foreach (var card in allCards)
        {
            if (card.Key == 2 || card.Key == 3) continue;
            selectedCharacterCards.Add(card.Value);
        }
        return selectedCharacterCards;
    }
}
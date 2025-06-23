using UnityEngine;
using static EnumClass;

public class Card : MonoBehaviour
{
    private CardState curState;
    private CardType curCardType;

    [SerializeField] GameObject front, back;

    public void InitCardData(CharacterToken clickedToken, Sprite sprite, object cardData, CardState state, CardType cardType)
    {
        curState = state;
        curCardType = cardType;

        switch (cardType)
        {
            case CardType.SkillCard:
                //var skillCardData = cardData as SkillCardData;
                //SetSkillCard(sprite, skillCardData);
                break;
            case CardType.CharacterCard:
                var characterCardData = cardData as CharacterCardData;
                SetCharacterCard(clickedToken,  sprite, characterCardData);
                break;
            default:
                Debug.Log("Not Card Type");
                break;
        }

        SetCardState(state);
    }

    public void SetCardState(CardState newState)
    {
        curState = newState;

        front.SetActive(newState == CardState.Front);
        back.SetActive(newState == CardState.Back);
    }

    protected virtual void SetCharacterCard(CharacterToken clickedToken, Sprite sprite, CharacterCardData cardData) { }

    //protected virtual void SetSkillCard(Sprite sprite, SkillCardData cardData) { }
}
using UnityEngine;
using static EnumClass;

public class Card : MonoBehaviour
{
    private State curState;
    private CardType curCardType;

    [SerializeField] GameObject front, back;

    public void InitCardData(Sprite sprite, object cardData, State state, CardType cardType)
    {
        //curState = state;
        //curCardType = cardType;

        //switch (cardType)
        //{
        //    case CardType.SkillCard:
        //        var skillCardData = cardData as SkillCardData;
        //        SetSkillCard(sprite, skillCardData);
        //        break;
        //    case CardType.CharacterCard:
        //        var characterCardData = cardData as CharacterCardData;
        //        SetCharacterCard(sprite, characterCardData);
        //        break;
        //    default:
        //        Debug.Log("Not Card Type");
        //        break;
        //}

        //SetState(state);

        var characterCardData = cardData as CharacterCardData;
        SetCharacterCard(sprite, characterCardData);
    }

    public void SetState(State newState)
    {
        curState = newState;

        front.SetActive(newState == State.Front);
        back.SetActive(newState == State.Back);
    }

    protected virtual void SetCharacterCard(Sprite sprite, CharacterCardData cardData) { }

    protected virtual void SetSkillCard(Sprite sprite, SkillCardData cardData) { }
}
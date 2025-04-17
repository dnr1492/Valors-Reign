using UnityEngine;
using static EnumClass;

public class Card : MonoBehaviour
{
    private SkillCardData _skillCardData;
    private CharacterCardData _characterCardData;
    private State curState;
    private CardType curCardType;

    [SerializeField] GameObject front, back;

    public void InitCardData(object cardData, State state, CardType cardType)
    {
        curState = state;
        curCardType = cardType;

        switch (cardType)
        {
            case CardType.SkillCard:
                _skillCardData = cardData as SkillCardData;
                break;
            case CardType.CharacterCard:
                _characterCardData = cardData as CharacterCardData;
                break;
            default:
                Debug.Log("Not Card Type");
                break;
        }

        SetState(state);
    }

    public void SetState(State newState)
    {
        curState = newState;

        front.SetActive(newState == State.Front);
        back.SetActive(newState == State.Back);
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterToken : MonoBehaviour
{
    [SerializeField] Image imgCharacter;
    [SerializeField] Button btn;
    [SerializeField] Card characterCard;
    [SerializeField] CustomBackground cb;

    private bool isSelect = false;

    private void Start()
    {
        gameObject.SetActive(false);
    }

    public void Init(Sprite sprite, CharacterCardData cardData)
    {
        gameObject.SetActive(true);

        imgCharacter.sprite = sprite;

        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => {
            Debug.Log($"OnClick Event: {cardData.name}의 정보 표출");
            characterCard.InitCardData(sprite, cardData, EnumClass.State.Front, EnumClass.CardType.CharacterCard);
            cb.SetSelect(isSelect = !isSelect);
        });
    }
}
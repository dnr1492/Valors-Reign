using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static EnumClass;

public class CharacterToken : MonoBehaviour
{
    [SerializeField] Image imgCharacter;
    [SerializeField] Button btn;
    [SerializeField] CustomBackground cb;

    private UICreateDeckPhase2 uiCreateDeckPhase2;
    private CharacterCard characterCard;
    
    public bool IsSelect { get; private set; }
    public int Key { get; private set; }
    public CharacterTierAndCost Tier { get; private set; }
    public int Cost { get; private set; }

    private void Start()
    {
        uiCreateDeckPhase2 = FindObjectOfType<UICreateDeckPhase2>();
        characterCard = FindObjectOfType<CharacterCard>();
        gameObject.SetActive(false);
    }

    public void Init(Sprite sprite, CharacterCardData characterCardData)
    {
        Key = characterCardData.id;
        Tier = characterCardData.tier;
        Cost = characterCardData.tier == CharacterTierAndCost.Captain ? 0 : (int)characterCardData.tier;
        imgCharacter.sprite = sprite;

        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => {
            Debug.Log($"OnClick Event: {characterCardData.name}의 정보 표출");

            //캐릭터 카드 정보 표시
            characterCard.InitCardData(sprite, characterCardData, State.Front, CardType.CharacterCard);

            //캐릭터 토큰 선택 및 선택 해제
            ControllerRegister.Get<CharacterTokenController>().OnClickToken(this);
        });
    }

    public void Select()
    {
        cb.SetSelect(IsSelect = !IsSelect);
        if (Cost != 0) uiCreateDeckPhase2.SetMaxCost(Cost);
    }

    public void Unselect()
    {
        cb.SetSelect(IsSelect = false);
        if (Cost != 0) uiCreateDeckPhase2.SetMaxCost(-Cost);
    }

    public Sprite GetCharacterSprite() => imgCharacter.sprite;
}
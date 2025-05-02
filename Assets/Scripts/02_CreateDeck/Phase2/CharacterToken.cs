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

    private UIMain uiMain;
    private CharacterCard characterCard;
    private bool isSelect = false;

    public int Key { get; private set; }

    private void Start()
    {
        uiMain = FindObjectOfType<UIMain>();
        characterCard = FindObjectOfType<CharacterCard>();
        gameObject.SetActive(false);
    }

    public void Init(Sprite sprite, CharacterCardData characterCardData)
    {
        Key = characterCardData.id;

        gameObject.SetActive(true);

        imgCharacter.sprite = sprite;

        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => {
            Debug.Log($"OnClick Event: {characterCardData.name}의 정보 표출");

            //캐릭터 카드 정보 표시
            characterCard.InitCardData(sprite, characterCardData, State.Front, CardType.CharacterCard);

            //코스트 설정
            var cost = characterCardData.tier == CharacterTierAndCost.Leader ? 0 : (int)characterCardData.tier;
            uiMain.SetMaxCost(cost);

            //캐릭터 토큰 선택 및 선택 해제
            ControllerRegister.Get<CharacterTokenController>().OnTokenClicked(this, characterCardData.tier);
        });
    }

    public void Select() => cb.SetSelect(isSelect = !isSelect);

    public void Deselect() => cb.SetSelect(isSelect = false);
}
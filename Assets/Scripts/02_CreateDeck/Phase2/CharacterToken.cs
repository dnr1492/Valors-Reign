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

    private readonly Dictionary<int, int> selectedSkillCounts = new();  //예: { 1000: 2, 1001: 1 } (skillId, count)

    public CharacterTokenState State { get; private set; } = CharacterTokenState.Cancel;
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
            characterCard.InitCardData(this, sprite, characterCardData, CardState.Front, CardType.CharacterCard);

            ControllerRegister.Get<CharacterTokenController>().OnClickToken(this, characterCard);
        });
    }

    public void SetTokenState(CharacterTokenState newState)
    {
        if (State == newState) return;

        //상태 전환에 따른 처리
        switch (newState)
        {
            case CharacterTokenState.Cancel:
                if (State == CharacterTokenState.Confirm && Cost != 0)
                    uiCreateDeckPhase2.SetMaxCost(-Cost);
                    selectedSkillCounts.Clear();  //스킬 개수 초기화
                break;

            case CharacterTokenState.Confirm:
                if (Cost != 0)
                    uiCreateDeckPhase2.SetMaxCost(Cost);
                break;
        }

        State = newState;
        cb.SetSelect(newState);
    }

    public Sprite GetCharacterSprite() => imgCharacter.sprite;

    /// <summary>
    /// 스킬 개수 설정
    /// </summary>
    /// <param name="skillId"></param>
    /// <param name="count"></param>
    public void SetSkillCount(int skillId, int count)
    {
        if (count <= 0) selectedSkillCounts.Remove(skillId);
        else selectedSkillCounts[skillId] = count;
    }

    /// <summary>
    /// 스킬 개수 조회
    /// </summary>
    /// <param name="skillId"></param>
    /// <returns></returns>
    public int GetSkillCount(int skillId)
    {
        return selectedSkillCounts.TryGetValue(skillId, out var count) ? count : 0;
    }

    /// <summary>
    /// 스킬 전체 조회
    /// </summary>
    /// <returns></returns>
    public Dictionary<int, int> GetAllSkillCounts()
    {
        return new Dictionary<int, int>(selectedSkillCounts);  //복사본
    }
}
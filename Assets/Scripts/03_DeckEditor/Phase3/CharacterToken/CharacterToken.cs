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

    private UIEditorDeckPhase3 uiCreateDeckPhase2;
    private CharacterCard characterCard;

    //선택된 스킬 개수 저장: { 1000: 2, 1001: 1 } (skillId, count)
    private readonly Dictionary<int, int> selectedSkillCounts = new();

    public CharacterTokenState State { get; private set; } = CharacterTokenState.Cancel;
    public int Key { get; private set; }
    public CharacterTier Tier { get; private set; } = CharacterTier.None;
    public int Cost { get; private set; }

    private void Start()
    {
        uiCreateDeckPhase2 = FindObjectOfType<UIEditorDeckPhase3>();
        characterCard = FindObjectOfType<CharacterCard>();
        gameObject.SetActive(false);
    }

    //캐릭터 토큰 초기화
    public void Init(Sprite sprite, CharacterCardData characterCardData)
    {
        Key = characterCardData.id;
        Tier = characterCardData.tier;
        Cost = characterCardData.cost;
        imgCharacter.sprite = sprite;

        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => {
            Debug.Log($"OnClick Event: {characterCardData.name}의 카드 표시");

            //캐릭터 카드 정보 표시
            characterCard.InitCardData(this, sprite, characterCardData, CardState.Front, CardType.CharacterCard);

            ControllerRegister.Get<CharacterTokenController>().OnClickToken(this, characterCard);
        });
    }

    //캐릭터 토큰 정보 표시
    public void ShowTokenInfo()
    {
        if (DataManager.Instance.dicCharacterCardData.TryGetValue(Key, out var characterCardData)) {
            characterCard.InitCardData(this, imgCharacter.sprite, characterCardData, CardState.Front, CardType.CharacterCard);
        }
    }

    //토큰 상태 설정
    public void SetTokenState(CharacterTokenState newState)
    {
        if (State == newState) return;

        //Confirm 상태에서 Cancel 또는 Select로 바뀔 때 비용 차감
        if (State == CharacterTokenState.Confirm && newState != CharacterTokenState.Confirm && Cost != 0)
            uiCreateDeckPhase2.SetCost(-Cost);

        //Confirm이 아닌 상태에서 Confirm으로 바뀔 때 비용 추가
        if (newState == CharacterTokenState.Confirm && State != CharacterTokenState.Confirm && Cost != 0)
            uiCreateDeckPhase2.SetCost(Cost);

        //Select로 바뀔 때 스킬 개수 초기화
        if (newState == CharacterTokenState.Select)
            selectedSkillCounts.Clear();

        State = newState;
        cb.SetSelect(newState);
    }

    //캐릭터 스프라이트 가져오기
    public Sprite GetCharacterSprite() => imgCharacter.sprite;

    //스킬 개수 설정
    public void SetSkillCount(int skillId, int count)
    {
        if (count <= 0) selectedSkillCounts.Remove(skillId);
        else selectedSkillCounts[skillId] = count;
    }

    //스킬 개수 반환
    public int GetSkillCount(int skillId)
    {
        return selectedSkillCounts.TryGetValue(skillId, out var count) ? count : 0;
    }

    //스킬 전체 반환 (복사본)
    public Dictionary<int, int> GetAllSkillCounts()
    {
        //깊은 복사
        return new Dictionary<int, int>(selectedSkillCounts);  
    }

    //캐릭터 카드의 상태를 Back으로 설정
    public void SetCardToBack()
    {
        characterCard.SetCardState(CardState.Back);
    }
}
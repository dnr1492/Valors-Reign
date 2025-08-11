using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static EnumClass;
using Cysharp.Threading.Tasks;

public class FilterController : MonoBehaviour
{
    private CharacterRace selectedRace = CharacterRace.None;

    private void Awake()
    {
        ControllerRegister.Register(this);
    }

    //해당 Race에 해당하는 캐릭터 토큰 전부를 보여주는 메서드
    public async UniTask InitFilterAsync(CharacterRace race)
    {
        selectedRace = race;
        Debug.Log($"FilterController: {race} 종족의 캐릭터들을 표시합니다.");

        await UniTask.Yield();
        SetCharacterTokenByRace();
    }

    //Race에 해당하는 캐릭터 토큰만 표시
    private void SetCharacterTokenByRace()
    {
        var allCharacterCardDatas = DataManager.Instance.dicCharacterCardData;
        var arrCharacterToken = ControllerRegister.Get<CharacterTokenController>().GetAllCharacterToken();
        var dicCharacterEnlargeSprite = SpriteManager.Instance.dicCharacterEnlargeSprite;

        //모든 캐릭터 토큰 비활성화
        foreach (var token in arrCharacterToken) 
        {
            token.gameObject.SetActive(false);
        }

        //선택된 Race에 해당하는 캐릭터들만 필터링
        var raceFilteredCharacters = allCharacterCardDatas
            .Where(ch => ch.Value.race == selectedRace)
            .ToList();

        Debug.Log($"{selectedRace} 종족 캐릭터 수: {raceFilteredCharacters.Count}");

        //필터링된 캐릭터들의 토큰만 활성화하고 초기화
        int tokenIndex = 0;
        foreach (var characterCardData in raceFilteredCharacters)
        {
            if (tokenIndex >= arrCharacterToken.Length)
            {
                Debug.Log("캐릭터 토큰이 부족합니다!");
                break;
            }

            var spriteKey = $"{characterCardData.Value.race}_{characterCardData.Value.job}_{characterCardData.Value.tier}_{characterCardData.Value.name}";
            
            if (dicCharacterEnlargeSprite.ContainsKey(spriteKey))
            {
                arrCharacterToken[tokenIndex].Init(dicCharacterEnlargeSprite[spriteKey], characterCardData.Value);
                arrCharacterToken[tokenIndex].gameObject.SetActive(true);
                tokenIndex++;
            }
            else Debug.Log($"스프라이트를 찾을 수 없습니다: {spriteKey}");
        }
    }

    public CharacterRace GetSelectedRace() => selectedRace;

    //필터 초기화
    public void ResetFilter()
    {
        selectedRace = CharacterRace.None;
        Debug.Log("FilterController: 필터가 초기화되었습니다.");

        //모든 캐릭터 토큰 표시
        var arrCharacterToken = ControllerRegister.Get<CharacterTokenController>().GetAllCharacterToken();
        foreach (var token in arrCharacterToken)
            token.gameObject.SetActive(true);

        //선택 버튼 초기화
        var filterPopup = UIManager.Instance.GetPopup<UIFilterPopup>("UIFilterPopup");
        filterPopup.OnResetFilter();
    }
}
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillCard : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI txtSkillRank, txtSkillType, txtSkillName, txtSkillEffect;
    [SerializeField] Image imgSkill;
    [SerializeField] RectTransform hexContainer;
    [SerializeField] GameObject hexPrefab;

    private readonly List<GameObject> skillHexes = new();
    private readonly Dictionary<(int dq, int dr), GameObject> skillHexMap = new();

    private Sprite originalSkillSprite;  //원본 스킬 스프라이트 보관

    public SkillCardData SkillCardData { get; private set; }

    public void Set(Sprite sprite, SkillCardData skillCardData)
    {
        txtSkillRank.text = skillCardData.rank.ToString();
        txtSkillType.text = skillCardData.cardType.ToString();
        txtSkillName.text = skillCardData.name;
        txtSkillEffect.text = skillCardData.effect;
        imgSkill.sprite = sprite;
        originalSkillSprite = sprite;

        UISkillHexGridHelper.ClearSkillHexGrid(skillHexes, skillHexMap);
        UISkillHexGridHelper.CreateSkillHexGrid(hexContainer, hexPrefab, skillHexes, skillHexMap, 1.1f);
        UISkillHexGridHelper.ShowSkillHexRange(skillCardData, skillHexMap);

        SkillCardData = skillCardData;
    }

    //기본 이동카드인 경우 캐릭터 이미지로 교체
    public void SetCharacterImageIfMoveCard(Sprite characterSprite)
    {
        if (SkillCardData != null && SkillCardData.id == 1000 && characterSprite != null)
            imgSkill.sprite = characterSprite;
    }

    //CardZone으로 돌아가거나 설정 해제 시 원복
    public void ResetImageIfMoveCard()
    {
        if (SkillCardData != null && SkillCardData.id == 1000)
            imgSkill.sprite = originalSkillSprite;
    }
}

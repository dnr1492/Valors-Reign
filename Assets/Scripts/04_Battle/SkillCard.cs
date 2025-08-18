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
    [SerializeField] Toggle toggle_effect, toggle_skillRange;

    private readonly List<GameObject> skillHexes = new();
    private readonly Dictionary<(int dq, int dr), GameObject> skillHexMap = new();

    private Sprite originalBasicMoveSkillSprite;  //���� �⺻ �̵�ī���� ��������Ʈ ����

    public SkillCardData SkillCardData { get; private set; }

    private void Start()
    {
        InitToggleEvent();
    }

    public void Set(Sprite sprite, SkillCardData skillCardData)
    {
        txtSkillRank.text = skillCardData.rank.ToString();
        txtSkillType.text = skillCardData.cardType.ToString();
        txtSkillName.text = skillCardData.name;
        txtSkillEffect.text = skillCardData.effect;
        imgSkill.sprite = sprite;
        originalBasicMoveSkillSprite = sprite;

        UISkillHexGridHelper.ClearSkillHexGrid(skillHexes, skillHexMap);
        UISkillHexGridHelper.CreateSkillHexGrid(hexContainer, hexPrefab, skillHexes, skillHexMap, skillCardData);
        UISkillHexGridHelper.ShowSkillHexRange(skillCardData, skillHexMap);

        SkillCardData = skillCardData;
    }

    //�⺻ �̵�ī���� ��� ĳ���� �̹����� ��ü
    public void SetCharacterImageIfMoveCard(Sprite characterSprite)
    {
        if (SkillCardData != null && SkillCardData.id == 1000 && characterSprite != null)
            imgSkill.sprite = characterSprite;
    }

    //CardZone���� ���ư��ų� ���� ���� �� ����
    public void ResetImageIfMoveCard()
    {
        if (SkillCardData != null && SkillCardData.id == 1000)
            imgSkill.sprite = originalBasicMoveSkillSprite;
    }

    #region Detail �� �޴�
    private void InitToggleEvent()
    {
        Active(true);

        toggle_effect.onValueChanged.AddListener((isOn) => {
            if (isOn) Active(true);
        });

        toggle_skillRange.onValueChanged.AddListener((isOn) => {
            if (isOn) Active(false);
        });
    }

    private void Active(bool isActive)
    {
        txtSkillEffect.gameObject.SetActive(isActive);
        hexContainer.gameObject.SetActive(!isActive);
    }
    #endregion
}

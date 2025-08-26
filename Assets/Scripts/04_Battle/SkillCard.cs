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

    private Sprite originalBasicMoveSkillSprite;  //원본 기본 이동카드의 스프라이트 보관

    //필드 직렬화로 Instantiate 시에도 값 보존
    [field: SerializeField] public SkillCardData SkillCardData { get; private set; }

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
            imgSkill.sprite = originalBasicMoveSkillSprite;
    }

    #region Detail 탭 메뉴
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

    //확대 시 Effect 텍스트 자동사이즈 (클론만 파괴되므로 복원 불필요)
    public void EnableEffectTextAutoSizeForEnlarge(float baseW = 130f, float baseH = 190f)
    {
        if (!txtSkillEffect) return;
        var cardRT = (RectTransform)transform;
        float w = Mathf.Max(1f, cardRT.rect.width);
        float h = Mathf.Max(1f, cardRT.rect.height);
        float s = Mathf.Min(w / baseW, h / baseH);  //보수적으로 작은쪽 스케일 채택

        float baseSize = txtSkillEffect.fontSize;  //현재 폰트를 베이스로 사용
        txtSkillEffect.enableAutoSizing = false;  //오토사이즈 끄고 직접 세팅
        txtSkillEffect.fontSize = Mathf.Round(baseSize * s);
        txtSkillEffect.ForceMeshUpdate();
    }

    //확대 사이즈로 바뀐 뒤 스킬범위 표시 새로고침
    public void RebuildHexForEnlarge()
    {
        UISkillHexGridHelper.ClearSkillHexGrid(skillHexes, skillHexMap);
        UISkillHexGridHelper.CreateSkillHexGrid(hexContainer, hexPrefab, skillHexes, skillHexMap, SkillCardData);
        UISkillHexGridHelper.ShowSkillHexRange(SkillCardData, skillHexMap);

        //레이아웃 강제 갱신
        Canvas.ForceUpdateCanvases();
        var rt = (RectTransform)transform;
        LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
        if (hexContainer) LayoutRebuilder.ForceRebuildLayoutImmediate(hexContainer);
    }
}

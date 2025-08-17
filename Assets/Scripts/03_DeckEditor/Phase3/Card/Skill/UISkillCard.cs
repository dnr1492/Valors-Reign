using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static EnumClass;

/* 
[방향 offset 참고]
위	    (0, -1)
아래	(0, 1)
오른쪽	(1, 0)
왼쪽	(-1, 0)
좌상	(-1, -1)
우하	(1, 1)
*/

public class UISkillCard : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI txtTotalSkillCount, txtSelectedSkillCount;
    [SerializeField] TextMeshProUGUI txtSkillRank, txtSkillType, txtSkillName, txtSkillEffect;
    [SerializeField] Image imgSkill;
    [SerializeField] Button btnDecrease, btnIncrease;
    [SerializeField] Button btnEnlargeImage, btnEnlargeRange;
    [SerializeField] RectTransform hexContainer;
    [SerializeField] GameObject hexPrefab;

    private readonly List<GameObject> skillHexes = new();
    private readonly Dictionary<(int dq, int dr), GameObject> skillHexMap = new();

    private SkillCardData curSkillCardData;
    private int count = 0;
    private int skillId = -1;
    public int GetSkillId() => skillId;
    public int GetCount() => count;

    public void Set(Sprite sprite, SkillCardData skillCardData, int initialCount)
    {
        txtSkillRank.text = skillCardData.rank.ToString();
        txtSkillType.text = skillCardData.cardType.ToString();
        txtSkillName.text = skillCardData.name;
        txtSkillEffect.text = skillCardData.effect;
        imgSkill.sprite = sprite;

        btnDecrease.onClick.RemoveAllListeners();
        btnIncrease.onClick.RemoveAllListeners();
        btnEnlargeImage.onClick.RemoveAllListeners();
        btnEnlargeRange.onClick.RemoveAllListeners();

        btnDecrease.onClick.AddListener(OnClickDecrease);
        btnIncrease.onClick.AddListener(OnClickIncrease);
        btnEnlargeImage.onClick.AddListener(OnClickEnlargeImage);
        btnEnlargeRange.onClick.AddListener(OnClickEnlargeRange);

        curSkillCardData = skillCardData;
        skillId = skillCardData.id;
        count = Mathf.Clamp(initialCount, 0, 4);
        txtSelectedSkillCount.text = count.ToString();

        UISkillHexGridHelper.ClearSkillHexGrid(skillHexes, skillHexMap);
        UISkillHexGridHelper.CreateSkillHexGrid(hexContainer, hexPrefab, skillHexes, skillHexMap, skillCardData);
        UISkillHexGridHelper.ShowSkillHexRange(skillCardData, skillHexMap);
    }

    private void OnClickDecrease()
    {
        if (count > 0) {
            count--;
            txtSelectedSkillCount.text = count.ToString();
        }
    }

    private void OnClickIncrease()
    {
        if (count < 4) {
            count++;
            txtSelectedSkillCount.text = count.ToString();
        }
    }

    private void OnClickEnlargeImage()
    {
        UIManager.Instance.GetPopup<UISkillInfoPopup>("UISkillInfoPopup").ShowImageOverlay(imgSkill.sprite);
    }

    private void OnClickEnlargeRange()
    {
        UIManager.Instance.GetPopup<UISkillInfoPopup>("UISkillInfoPopup").ShowRangeOverlay(curSkillCardData);
    }
}

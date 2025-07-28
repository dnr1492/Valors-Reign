using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static EnumClass;

public class UIFilterPopup : UIPopupBase
{
    [SerializeField] Button btn_apply, btn_reset;
    [SerializeField] Transform filterRoot;  //Toggle들이 모여있는 부모 오브젝트

    private List<Toggle> filterToggles = new();

    private void Awake()
    {
        filterToggles = filterRoot.GetComponentsInChildren<Toggle>(true).ToList();

        btn_apply.onClick.AddListener(OnFilterChanged);
        btn_apply.onClick.AddListener(Close);

        btn_reset.onClick.AddListener(OnResetFilter);
    }

    public void OnFilterChanged()
    {
        var tokens = ControllerRegister.Get<CharacterTokenController>().GetAllCharacterToken();
        var characterDataDict = DataManager.Instance.dicCharacterCardData;

        //활성화된 토글이 있는 필터 그룹만 추출
        var groupedActiveFilters = GroupFiltersByType()
            .Where(kv => kv.Value.Any(t => t.isOn))
            .ToDictionary(kv => kv.Key, kv => kv.Value.Where(t => t.isOn).ToList());

        bool noFilterSelected = groupedActiveFilters.Count == 0;

        foreach (var token in tokens)
        {
            if (token.Tier == CharacterTier.None)
            {
                token.gameObject.SetActive(false);
                continue;
            }

            //CharacterCardData 가져오기
            if (!characterDataDict.TryGetValue(token.Key, out var data))
            {
                token.gameObject.SetActive(false);
                continue;
            }

            //필터가 하나도 선택되지 않은 경우 → 전체 표시
            if (noFilterSelected)
            {
                token.gameObject.SetActive(true);
                continue;
            }

            bool isVisible = true;

            //필터 그룹별로 처리 (AND 관계)
            foreach (var (filterType, activeToggles) in groupedActiveFilters)
            {
                //같은 그룹 내에서는 OR 조건
                bool match = activeToggles.Any(t =>
                {
                    var f = t.GetComponent<FilterData>();
                    if (f == null) return false;

                    return f.filterType switch
                    {
                        FilterType.Hp => data.hp == f.intValue,
                        FilterType.Mp => data.mp == f.intValue,
                        FilterType.Tier => data.tier == f.tier,
                        FilterType.Job => data.job == f.job,
                        FilterType.SkillCardRank => data.skills.Any(skillId =>
                            DataManager.Instance.dicSkillCardData.TryGetValue(skillId, out var skillData) &&
                            skillData.rank == (int)f.skillCardRank),
                        FilterType.SkillCardType => data.skills.Any(skillId =>
                            DataManager.Instance.dicSkillCardData.TryGetValue(skillId, out var skillData) &&
                            skillData.cardType == f.skillCardType),
                        _ => false
                    };
                });

                if (!match) {
                    isVisible = false;
                    break;
                }
            }

            token.gameObject.SetActive(isVisible);
        }
    }

    private Dictionary<FilterType, List<Toggle>> GroupFiltersByType()
    {
        return filterToggles
            .Where(t => t != null && t.TryGetComponent<FilterData>(out var _))
            .GroupBy(t => t.GetComponent<FilterData>().filterType)
            .ToDictionary(g => g.Key, g => g.ToList());
    }

    private void OnResetFilter()
    {
        foreach (var toggle in filterToggles)
            toggle.isOn = false;
    }

    protected override void ResetUI() { }
}
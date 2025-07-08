using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static EnumClass;

public class FilterData : MonoBehaviour
{
    public FilterType filterType;

    //Enum 기반 필터용
    public CharacterJob job;
    public CharacterTierAndCost tier;
    public SkillCardRankAndMpConsum skillCardRank;
    public SkillCardType skillCardType;

    //정수 기반 필터용
    public int intValue;  //Hp, Mp에 사용
}

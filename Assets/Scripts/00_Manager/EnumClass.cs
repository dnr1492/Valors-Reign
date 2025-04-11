using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnumClass
{
    public enum CharacterTierAndCost { None = -1, Leader = 100, High = 4, Middle = 3, Low = 2 }
    public enum State { None = -1, Front, Back }
    public enum CardType { None = -1, CharacterCard, SkillCard }
}

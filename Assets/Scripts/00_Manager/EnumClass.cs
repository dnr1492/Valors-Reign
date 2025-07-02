public class EnumClass
{
    public enum CharacterRace { None = -1, Primordial, Earthbound, Seaborne, Verdant, Skyborne, Mythkin, Divinite, Morphid, Undying, Automaton }  //영장, 지주, 수생, 식물, 천익, 환수, 성휘, 이형, 불사, 기계
    public enum CharacterTokenState { None = -1, Cancel, Select, Confirm }
    public enum CharacterTokenDirection { Right = 0, UpRight, UpLeft, Left, DownLeft, DownRight }
    public enum CharacterTierAndCost { None = -1, Boss = 100, High = 4, Middle = 3, Low = 2 }
    public enum CharacterJob { Dealer, Tanker, Supporter }
    public enum CardState { None = -1, Front, Back }
    public enum CardType { None = -1, CharacterCard, SkillCard }
    public enum SkillCardType { None = -1, Move, Attack, Buff, Debuff, Trap }  // ===== 추후 수정 요망 ===== //
    public enum SkillCardRankAndMpConsum { None = -1, Rank1 = 1, Rank2 = 2, Rank3 = 3 }
    public enum SkillRangeType
    {
        None,
        LineForward1,  //직선 1칸
        LineForward2,  //직선 2칸
        LineForward3,  //직선 3칸
        Ring1,         //원형 1칸
        Custom
    }
}

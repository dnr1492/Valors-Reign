public class EnumClass
{
    public enum CharacterRace { None = -1, Primordial, Earthbound, Seaborne, Verdant, Skyborne, Mythkin, Divinite, Morphid, Undying, Automaton }  //영장, 지주, 수생, 식물, 천익, 환수, 성휘, 이형, 불사, 기계
    public enum CharacterTokenState { None = -1, Cancel, Select, Confirm }
    public enum CharacterTokenDirection { None = -1, Right = 0, UpRight, UpLeft, Left, DownLeft, DownRight }
    public enum CharacterTier { None = -1, Boss, High, Middle, Low }
    public enum CharacterJob { None = -1, Dealer, Tanker, Supporter }
    public enum FilterType
    {
        Job,
        Tier,
        Hp,
        Mp,
        SkillCardRank,
        SkillCardType
    }
    public enum CardState { None = -1, Front, Back }
    public enum CardType { None = -1, CharacterCard, SkillCard }
    public enum SkillCardType { None = -1, Attack, Defense, Move, Buff, Debuff }
    public enum SkillCardRankAndMpConsum { None = -1, Zero = 0, Rank1 = 1, Rank2 = 2, Rank3 = 3, Rank4 =4, Rank5 = 5 }
    public enum SkillRangeType
    {
        None,
        LineForward1,  //직선 1칸
        LineForward2,  //직선 2칸
        LineForward3,  //직선 3칸
        Ring1,         //원형 1칸
        Custom
    }
    public enum PhotonEventCode : byte { SendDeck = 1 }
}

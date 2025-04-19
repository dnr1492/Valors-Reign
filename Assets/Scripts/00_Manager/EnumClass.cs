public class EnumClass
{
    public enum CharacterRace { None = -1, Primordial, Earthbound, Seaborne, Verdant, Skyborne, Mythkin, Divinite, Morphid, Undying, Automaton }  //영장, 지주, 수생, 식물, 천익, 환수, 성휘, 이형, 불사, 기계
    public enum CharacterTierAndCost { None = -1, Leader = 100, High = 4, Middle = 3, Low = 2 }
    public enum CharacterJob { Dealer, Tanker, Supporter }
    public enum State { None = -1, Front, Back }
    public enum CardType { None = -1, CharacterCard, SkillCard }
    public enum SkillCardType { None = -1, Move, Attack, Buff }
    public enum SkillCardRankAndMpConsum { None = -1, Rank1 = 1, Rank2 = 2, Rank3 = 3 }
}

public class EnumClass
{
    public enum ToastAnchor { Top, Center, Bottom }
    public enum PhotonEventCode : byte { SendDeck = 1, CoinFlip = 2, SendTurnOrderChoice = 3, RoundFinished = 4, PlayerReady = 5, SendRoundPlan = 6, }
    public enum CharacterRace { None = -1, Primordial, Earthbound, Seaborne, Verdant, Skyborne, Mythkin, Divinite, Morphid, Undying, Automaton }  //����, ����, ����, �Ĺ�, õ��, ȯ��, ����, ����, �һ�, ���
    public enum CharacterTokenState { None = -1, Cancel, Select, Confirm }
    public enum CharacterTokenDirection { None = -1, RightDown = 0, RightUp = 1, Up = 2, LeftUp = 3, LeftDown = 4, Down = 5 }
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
        None = 0,
        LineForward1, LineForward2, LineForward3, LineForward4,  //����(����) Nĭ
        Cone2, Cone3,  //��ä��/��/���
        Ring1, Ring2, Ring3,  //��/����
        Plus1, Box3x3,  //����/�÷���/�簢
        Custom  //Ŀ����(������ ���� ����)
    }
    public enum SkillTargetKind { None, Self, TargetTileEnemy, TargetTileAlly, AllEnemiesInRange, AllAlliesInRange, Tile }
    public enum SkillConditionType { None, IfHPBelowX, IfHPAboveX, IfStackCount, 
        IfUsedHpInsteadOfMp, IfTargetHasStatus, IfTargetTileEnemy, IfTileHasNoEnemy, 
        IfTargetTileHascharacter, IfCounterExists, IfTilesFull, IfXOfXTilesOccupied, 
        IfOnEffectTrigger, IfDamageTrigger, IfDamageDealtTrigger, IfSkillActivated, IfMoveTrigger 
    }
    public enum SkillEffectType { AddHp, HealFlat, ShieldFlat, DealDamageFlat, ApplyStatus, RemoveStatus, Custom }
    public enum SkillTriggerType { Instant, OnEffect, OnMove, OnDamage, OnDamageDealt, OnSkillActive }
    public enum SkillLogicOp { And, Or, But }
    public enum SkillDuration
    {
        None = 0,  //���� ���� (�����)
        ThisRound = 1,  //���� ���������
        UpToNextRound = 2,
        UpToSecondRound = 3,
        UntilThisTurn = 4
    }
}

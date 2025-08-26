public class EnumClass
{
    public enum ToastAnchor { Top, Center, Bottom }
    public enum PhotonEventCode : byte { SendDeck = 1, CoinFlip = 2, SendTurnOrderChoice = 3, RoundFinished = 4, PlayerReady = 5, SendRoundPlan = 6, }
    public enum CharacterRace { None = -1, Primordial, Earthbound, Seaborne, Verdant, Skyborne, Mythkin, Divinite, Morphid, Undying, Automaton }  //영장, 지주, 수생, 식물, 천익, 환수, 성휘, 이형, 불사, 기계
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
        LineForward1, LineForward2, LineForward3, LineForward4,  //선형(전방) N칸
        Cone2, Cone3,  //부채꼴/콘/방사
        Ring1, Ring2, Ring3,  //링/원형
        Plus1, Box3x3,  //십자/플러스/사각
        Custom  //커스텀(오프셋 직접 지정)
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
        None = 0,  //지속 없음 (즉시형)
        ThisRound = 1,  //현재 라운드까지만
        UpToNextRound = 2,
        UpToSecondRound = 3,
        UntilThisTurn = 4
    }
}

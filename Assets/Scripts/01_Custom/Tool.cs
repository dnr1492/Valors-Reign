using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using static EnumClass;

public class Tool
{
#if UNITY_EDITOR
    private static void LoadDataFromJSON<T>(T data, string fileName)
    {
        //Resources/Datas 경로
        string path = Path.Combine(Application.dataPath, "Resources/Datas");
        string jsonPath = Path.Combine(path, fileName);

        //JSON 파일 저장
        string jsonData = JsonUtility.ToJson(data, true);
        File.WriteAllText(jsonPath, jsonData);

        Debug.Log($"JSON data saved at: {jsonPath}");
        AssetDatabase.Refresh();
    }

    #region 게임 플레이 데이터
    [MenuItem("정재욱/Generate gamePlay_data")]
    private static void GenerateGamePlayData()
    {
        GamePlayData gamePlayData = new GamePlayData {
            maxCost = 10,
            maxLeaderCount = 1
        };

        LoadDataFromJSON(gamePlayData, "gamePlay_data.json");
    }
    #endregion

    #region 캐릭터 카드 데이터
    [MenuItem("정재욱/Generate characterCard_data")]
    private static void GenerateCharacterCardData()
    {
        //JSON 데이터 생성
        CharacterCardDataList list = new CharacterCardDataList {
            characterCardDatas = new List<CharacterCardData>()
        };

        list.characterCardDatas.Add(new CharacterCardData { id = 101, name = "어설픈 방패병", tier = CharacterTier.Low, cost = 2, hp = 4, mp = 1, race = CharacterRace.Primordial, job = CharacterJob.Tanker, skills = new List<int> { 1000, 3001 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 102, name = "최전선의 창병", tier = CharacterTier.Low, cost = 2, hp = 3, mp = 1, race = CharacterRace.Primordial, job = CharacterJob.Tanker, skills = new List<int> { 1000, 2001, 3002 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 103, name = "그림자속 수호자", tier = CharacterTier.Low, cost = 2, hp = 2, mp = 2, race = CharacterRace.Primordial, job = CharacterJob.Tanker, skills = new List<int> { 1000, 3003, 3004 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 104, name = "침착한 수비수", tier = CharacterTier.Middle, cost = 3, hp = 4, mp = 2, race = CharacterRace.Primordial, job = CharacterJob.Tanker, skills = new List<int> { 1000, 3005, 2002 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 105, name = "나태한 문지기", tier = CharacterTier.Middle, cost = 3, hp = 3, mp = 3, race = CharacterRace.Primordial, job = CharacterJob.Tanker, skills = new List<int> { 1000, 3006 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 106, name = "마법방패를 두른 수호자", tier = CharacterTier.Middle, cost = 3, hp = 2, mp = 4, race = CharacterRace.Primordial, job = CharacterJob.Tanker, skills = new List<int> { 1000, 4001, 3007 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 107, name = "산군 -강수-", tier = CharacterTier.High, cost = 4, hp = 6, mp = 3, race = CharacterRace.Primordial, job = CharacterJob.Tanker, skills = new List<int> { 1000, 3008, 2003, 3009 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 108, name = "은결의 마녀 -루안-", tier = CharacterTier.High, cost = 4, hp = 2, mp = 6, race = CharacterRace.Primordial, job = CharacterJob.Tanker, skills = new List<int> { 1000, 5001, 5002, 3010, 3011 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 109, name = "블러드가드 -로간-", tier = CharacterTier.High, cost = 4, hp = 6, mp = 3, race = CharacterRace.Primordial, job = CharacterJob.Tanker, skills = new List<int> { 1000, 4002, 2004, 3012 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 110, name = "검은 장미 -엘-", tier = CharacterTier.Boss, cost = 0, hp = 2, mp = 1, race = CharacterRace.Primordial, job = CharacterJob.Tanker, skills = new List<int> { 1000, 3013 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 111, name = "빛의 방패 -세노티-", tier = CharacterTier.Boss, cost = 0, hp = 2, mp = 1, race = CharacterRace.Primordial, job = CharacterJob.Tanker, skills = new List<int> { 1000, 4003 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 112, name = "철의 성벽 -우르-", tier = CharacterTier.Boss, cost = 1, hp = 3, mp = 1, race = CharacterRace.Primordial, job = CharacterJob.Tanker, skills = new List<int> { 1000, 3014 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 113, name = "하얀검사", tier = CharacterTier.Low, cost = 2, hp = 3, mp = 2, race = CharacterRace.Primordial, job = CharacterJob.Dealer, skills = new List<int> { 1000, 2005 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 114, name = "그림자 속 암살자", tier = CharacterTier.Low, cost = 2, hp = 2, mp = 2, race = CharacterRace.Primordial, job = CharacterJob.Dealer, skills = new List<int> { 1000, 2006, 2007 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 115, name = "죽음과 함께 걷는 암살자", tier = CharacterTier.Low, cost = 2, hp = 2, mp = 3, race = CharacterRace.Primordial, job = CharacterJob.Dealer, skills = new List<int> { 1000, 2008 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 116, name = "섬광의 명궁", tier = CharacterTier.Middle, cost = 3, hp = 4, mp = 2, race = CharacterRace.Primordial, job = CharacterJob.Dealer, skills = new List<int> { 1000, 2009, 2010 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 117, name = "푸른눈의 저격수", tier = CharacterTier.Middle, cost = 3, hp = 2, mp = 3, race = CharacterRace.Primordial, job = CharacterJob.Dealer, skills = new List<int> { 1000, 4004, 1002, 2011 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 118, name = "석양의 보안관", tier = CharacterTier.Middle, cost = 3, hp = 3, mp = 2, race = CharacterRace.Primordial, job = CharacterJob.Dealer, skills = new List<int> { 1000, 2012, 2013, 2014 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 119, name = "카이른 블레이즈", tier = CharacterTier.High, cost = 4, hp = 6, mp = 3, race = CharacterRace.Primordial, job = CharacterJob.Dealer, skills = new List<int> { 1000, 2015, 2016, 2017 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 120, name = "광전사 -가르칸-", tier = CharacterTier.High, cost = 4, hp = 9, mp = -1, race = CharacterRace.Primordial, job = CharacterJob.Dealer, skills = new List<int> { 1000, 2018, 2019, 2020 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 121, name = "광무 -잔영-", tier = CharacterTier.High, cost = 4, hp = 5, mp = 4, race = CharacterRace.Primordial, job = CharacterJob.Dealer, skills = new List<int> { 1000, 2021, 2022, 2023 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 122, name = "검은불꽃 -청현-", tier = CharacterTier.Boss, cost = 1, hp = 2, mp = 1, race = CharacterRace.Primordial, job = CharacterJob.Dealer, skills = new List<int> { 1000, 2024, 2025 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 123, name = "이계의 창 -라이트-", tier = CharacterTier.Boss, cost = 0, hp = 1, mp = 2, race = CharacterRace.Primordial, job = CharacterJob.Dealer, skills = new List<int> { 1000, 2026 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 124, name = "기사단장 -자스안-", tier = CharacterTier.Boss, cost = 0, hp = 2, mp = 1, race = CharacterRace.Primordial, job = CharacterJob.Dealer, skills = new List<int> { 1000, 2027 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 125, name = "자유의 치유사", tier = CharacterTier.Low, cost = 2, hp = 1, mp = 4, race = CharacterRace.Primordial, job = CharacterJob.Supporter, skills = new List<int> { 1000, 4005 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 126, name = "행운의 치료사", tier = CharacterTier.Low, cost = 2, hp = 2, mp = 3, race = CharacterRace.Primordial, job = CharacterJob.Supporter, skills = new List<int> { 1000, 4006 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 127, name = "검은 마력의 저주사", tier = CharacterTier.Low, cost = 2, hp = 2, mp = 2, race = CharacterRace.Primordial, job = CharacterJob.Supporter, skills = new List<int> { 1000, 5003, 5004 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 128, name = "노련한 전략가", tier = CharacterTier.Middle, cost = 3, hp = 2, mp = 3, race = CharacterRace.Primordial, job = CharacterJob.Supporter, skills = new List<int> { 1000, 1003, 4007, 3015 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 129, name = "교활한 정보원", tier = CharacterTier.Middle, cost = 3, hp = 3, mp = 3, race = CharacterRace.Primordial, job = CharacterJob.Supporter, skills = new List<int> { 1000, 4008, 1004 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 130, name = "현자의 돌을 지닌 연금술사", tier = CharacterTier.Middle, cost = 3, hp = 2, mp = 4, race = CharacterRace.Primordial, job = CharacterJob.Supporter, skills = new List<int> { 1000, 4009, 3016 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 131, name = "로열 에이드", tier = CharacterTier.High, cost = 4, hp = 3, mp = 5, race = CharacterRace.Primordial, job = CharacterJob.Supporter, skills = new List<int> { 1000, 4010, 5005, 4011, 4012 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 132, name = "북방의 마법현자 -와이즈-", tier = CharacterTier.High, cost = 4, hp = 5, mp = 4, race = CharacterRace.Primordial, job = CharacterJob.Supporter, skills = new List<int> { 1000, 4013, 4014, 5006 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 133, name = "순풍의 가인 -라에나-", tier = CharacterTier.High, cost = 4, hp = 3, mp = 6, race = CharacterRace.Primordial, job = CharacterJob.Supporter, skills = new List<int> { 1000, 4015, 4016, 4017 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 134, name = "시간을 걷는 자 -노아-", tier = CharacterTier.Boss, cost = 1, hp = 1, mp = 2, race = CharacterRace.Primordial, job = CharacterJob.Supporter, skills = new List<int> { 1000, 4018, 5007 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 135, name = "천상의 울림 -벨-", tier = CharacterTier.Boss, cost = 1, hp = 2, mp = 2, race = CharacterRace.Primordial, job = CharacterJob.Supporter, skills = new List<int> { 1000, 4019 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 136, name = "망각 -루누스-", tier = CharacterTier.Boss, cost = 0, hp = 2, mp = 1, race = CharacterRace.Primordial, job = CharacterJob.Supporter, skills = new List<int> { 1000, 5008 } });

        LoadDataFromJSON(list, "characterCard_data.json");
    }
    #endregion

    #region 스킬 카드 데이터
    [MenuItem("정재욱/Generate skillCard_data")]
    private static void GenerateSkillCardData()
    {
        //JSON 데이터 생성
        SkillCardDataList list = new SkillCardDataList {
            skillCardDatas = new List<SkillCardData>()
        };

        #region Move
        list.skillCardDatas.Add(new SkillCardData { id = 1000, name = "기본 이동카드", effect = "해당 대상지 중 한 곳으로 이동한다. 이동하려는 칸에 다른 캐릭터가 존재하는 경우 데미지 1 을 주고 이동하지 않는다.", cardType = SkillCardType.Move, rank = null, round = null });
        list.skillCardDatas.Add(new SkillCardData { id = 1001, name = "신속", effect = "해당 이동 칸으로 이동한다. \"정조준 카운터\"는 전부 사라진다.", cardType = SkillCardType.Move, rank = 2, round = null });
        list.skillCardDatas.Add(new SkillCardData { id = 1002, name = "전선 재배치", effect = "해당 칸에 캐릭터가 전부 있을 경우, 칸에 있는 캐릭터의 위치를 변경한다.", cardType = SkillCardType.Move, rank = 2, round = null });
        list.skillCardDatas.Add(new SkillCardData { id = 1003, name = "거짓 정보", effect = "해당 칸에 캐릭터가 1 칸에만 있을 경우, 해당 칸 중 다른 칸으로 이동한다.", cardType = SkillCardType.Move, rank = 3, round = null });
        #endregion

        #region Attack
        list.skillCardDatas.Add(new SkillCardData { id = 2001, name = "스피어러쉬", effect = "해당 칸에 2 데미지를 준다.", cardType = SkillCardType.Attack, rank = 1, round = 1 });
        list.skillCardDatas.Add(new SkillCardData { id = 2002, name = "순응의 반격", effect = "해당 캐릭터는 데미지를 2 감소한다. 그 후 해당 칸의 캐릭터는 2 데미지를 준다.", cardType = SkillCardType.Attack, rank = 2, round = 4 });
        list.skillCardDatas.Add(new SkillCardData { id = 2003, name = "호쇄광", effect = "해당 칸에 3 데미지를 준다.", cardType = SkillCardType.Attack, rank = 1, round = 1 });
        list.skillCardDatas.Add(new SkillCardData { id = 2004, name = "상처 전환", effect = "해당 칸에 4 데미지를 준다. \"피의 맹세 카운터\" 2 개를 올린다. (최대 3 개)", cardType = SkillCardType.Attack, rank = 2, round = 2 });
        list.skillCardDatas.Add(new SkillCardData { id = 2005, name = "하얀검무", effect = "해당 칸에 2 데미지를 준다.", cardType = SkillCardType.Attack, rank = 1, round = 1 });
        list.skillCardDatas.Add(new SkillCardData { id = 2006, name = "그림자베기", effect = "해당 칸에 2 데미지를 준다.", cardType = SkillCardType.Attack, rank = 1, round = 2 });
        list.skillCardDatas.Add(new SkillCardData { id = 2007, name = "그림자 수리검", effect = "해당 칸에 2 데미지를 준다.", cardType = SkillCardType.Attack, rank = 1, round = 2 });
        list.skillCardDatas.Add(new SkillCardData { id = 2008, name = "죽음의 발자국", effect = "해당 칸에 2 데미지를 준다.", cardType = SkillCardType.Attack, rank = 3, round = 4 });
        list.skillCardDatas.Add(new SkillCardData { id = 2009, name = "연속사격", effect = "해당 칸에 2 데미지를 준다.", cardType = SkillCardType.Attack, rank = 2, round = 3 });
        list.skillCardDatas.Add(new SkillCardData { id = 2010, name = "바람가르기", effect = "해당 공격 칸에 2 데미지를 준다. 해당 이동 칸으로 이동한다.", cardType = SkillCardType.Attack, rank = 2, round = 3 });
        list.skillCardDatas.Add(new SkillCardData { id = 2011, name = "크리티컬샷", effect = "해당 칸에 2 데미지를 준다. \"정조준 카운터\"를 전부 소모하고, 소모한 카운터 수만큼 추가 데미지를 준다.", cardType = SkillCardType.Attack, rank = 1, round = 1 });
        list.skillCardDatas.Add(new SkillCardData { id = 2012, name = "연사", effect = "해당 칸에 2 데미지를 준다.", cardType = SkillCardType.Attack, rank = 1, round = 3 });
        list.skillCardDatas.Add(new SkillCardData { id = 2013, name = "속사", effect = "해당 칸에 2 데미지를 준다.", cardType = SkillCardType.Attack, rank = 1, round = 3 });
        list.skillCardDatas.Add(new SkillCardData { id = 2014, name = "석양의 신판", effect = "해당 칸에 2 데미지를 준다.", cardType = SkillCardType.Attack, rank = 1, round = 2 });
        list.skillCardDatas.Add(new SkillCardData { id = 2015, name = "화염의 심판", effect = "해당 칸에 2 데미지를 준다.", cardType = SkillCardType.Attack, rank = 2, round = 3 });
        list.skillCardDatas.Add(new SkillCardData { id = 2016, name = "불타는 맹세", effect = "해당 칸에 2 데미지를 준다. 그 후 출혈 상태가 된다.", cardType = SkillCardType.Attack, rank = 2, round = 1 });
        list.skillCardDatas.Add(new SkillCardData { id = 2017, name = "지옥의 참격", effect = "해당 칸에 2 데미지를 준다.", cardType = SkillCardType.Attack, rank = 3, round = 9 });
        list.skillCardDatas.Add(new SkillCardData { id = 2018, name = "블러드 프렌지", effect = "- 스킬 발동 시 MP가 아닌 HP를 소모한다. (스킬 발동 시 이번 라운드 HP 회복 불가)\n- 해당 칸에 3 데미지를 준다. 해당 칸에 캐릭터가 없다면 본 캐릭터의 HP 1 감소.\n- \"프렌지 카운터\"를 1 개 올린다. (최대 10 개)", cardType = SkillCardType.Attack, rank = 1, round = 1 });
        list.skillCardDatas.Add(new SkillCardData { id = 2019, name = "크림슨 로어", effect = "- 스킬 발동 시 MP가 아닌 HP를 소모한다. (스킬 발동 시 이번 라운드 HP 회복 불가)\n- 해당 칸에 4 데미지를 준다. 그 후 \"프렌지 카운터\"를 소모하고, 소모한 카운터 수만큼 추가 데미지를 준다.\n- \"프렌지 카운터\"를 2 개 올린다. (최대 10 개)", cardType = SkillCardType.Attack, rank = 2, round = 2 });
        list.skillCardDatas.Add(new SkillCardData { id = 2020, name = "헬팽 스트라이크", effect = "- 스킬 발동 시 MP가 아닌 HP를 소모한다. (스킬 발동 시 이번 라운드 HP 회복 불가)\n- 해당 공격 칸에 3 데미지를 준다. 그 후 \"프렌지 카운터\"를 소모하고, 소모한 카운터 수만큼 추가 데미지를 준다.\n- 해당 이동 칸으로 이동한다.\n- \"프렌지 카운터\"를 3 개 올린다. (최대 10 개)", cardType = SkillCardType.Attack, rank = 3, round = 3 });
        list.skillCardDatas.Add(new SkillCardData { id = 2022, name = "광무-일섬", effect = "해당 칸에 4 데미지를 준다.", cardType = SkillCardType.Attack, rank = 1, round = 1 });
        list.skillCardDatas.Add(new SkillCardData { id = 2023, name = "광무-연정", effect = "해당 칸에 4 데미지를 준다.", cardType = SkillCardType.Attack, rank = 2, round = 4 });
        list.skillCardDatas.Add(new SkillCardData { id = 2024, name = "광무-난무", effect = "해당 칸에 6 데미지를 준다.", cardType = SkillCardType.Attack, rank = 3, round = 6 });
        list.skillCardDatas.Add(new SkillCardData { id = 2025, name = "흑염참", effect = "해당 칸에 데미지를 준다.", cardType = SkillCardType.Attack, rank = 1, round = 2 });
        list.skillCardDatas.Add(new SkillCardData { id = 2026, name = "암인비영", effect = "해당 칸에 데미지를 준다.", cardType = SkillCardType.Attack, rank = 1, round = 1 });
        list.skillCardDatas.Add(new SkillCardData { id = 2027, name = "차원의 가르는 일격", effect = "해당 대상에게 데미지를 준다.", cardType = SkillCardType.Attack, rank = 2, round = 3 });
        list.skillCardDatas.Add(new SkillCardData { id = 2028, name = "소드 오브 오더", effect = "해당 대상에게 데미지를 준다.", cardType = SkillCardType.Attack, rank = 1, round = 1 });
        #endregion

        #region Defense
        list.skillCardDatas.Add(new SkillCardData { id = 3001, name = "어전트 가드", effect = "해당 캐릭터는 방어한다.", cardType = SkillCardType.Defense, rank = 1, round = 1 });
        list.skillCardDatas.Add(new SkillCardData { id = 3002, name = "스피어가드", effect = "해당 캐릭터는 데미지를 2 감소한다.", cardType = SkillCardType.Defense, rank = 2, round = 1 });
        list.skillCardDatas.Add(new SkillCardData { id = 3003, name = "그림자 벽", effect = "다음 라운드까지 해당 칸에 락존을 형성한다.\n(해당 칸에 캐릭터가 있을 경우, 스킬은 효과 처리되지 않는다.)", cardType = SkillCardType.Defense, rank = 1, round = 1 });
        list.skillCardDatas.Add(new SkillCardData { id = 3004, name = "어둠에서의 반격", effect = "다음 라운드까지 해당 캐릭터가 받는 데미지를 반사한다.", cardType = SkillCardType.Defense, rank = 2, round = 1 });
        list.skillCardDatas.Add(new SkillCardData { id = 3005, name = "철벽 태세", effect = "해당 캐릭터는 데미지를 2 감소한다. 그 후 해당 칸의 캐릭터는 추가 HP + 2 를 올린다.", cardType = SkillCardType.Defense, rank = 2, round = 4 });
        list.skillCardDatas.Add(new SkillCardData { id = 3006, name = "그래도 일이니까", effect = "이번 턴까지 해당 칸에 락존을 형성한다.\n락존에 있던 캐릭터는 한 칸 이동한다.", cardType = SkillCardType.Defense, rank = 3, round = 3 });
        list.skillCardDatas.Add(new SkillCardData { id = 3007, name = "마법반사진", effect = "이번 라운드까지 해당 칸에 데미지를 반사한다.", cardType = SkillCardType.Defense, rank = 2, round = 1 });
        list.skillCardDatas.Add(new SkillCardData { id = 3008, name = "웅인장", effect = "해당 칸에 1 데미지를 준다.", cardType = SkillCardType.Defense, rank = 1, round = 1 });
        list.skillCardDatas.Add(new SkillCardData { id = 3009, name = "룡윤강벽", effect = "해당 칸에 1 데미지를 준다.\n다음 라운드까지 해당 칸에 락존을 형성한다.", cardType = SkillCardType.Defense, rank = 2, round = 1 });
        list.skillCardDatas.Add(new SkillCardData { id = 3010, name = "미러바인드", effect = "두 번째 라운드까지 해당 칸에 데미지를 크리티컬로 반사한다.", cardType = SkillCardType.Defense, rank = 3, round = 2 });
        list.skillCardDatas.Add(new SkillCardData { id = 3011, name = "문브레이커", effect = "이번 턴까지 해당 칸을 방어한다.", cardType = SkillCardType.Defense, rank = 3, round = 3 });
        list.skillCardDatas.Add(new SkillCardData { id = 3012, name = "진홍의 벽", effect = "이번 턴까지 해당 칸에 락존을 형성한다.\n해당 칸에 있는 캐릭터는 속박, 출혈 상태가 된다.\n\"피의 맹세 카운터\" 1 개를 제거하고 칸 수를 늘릴 수 있다.", cardType = SkillCardType.Defense, rank = 3, round = 1 });
        list.skillCardDatas.Add(new SkillCardData { id = 3013, name = "가시덩굴", effect = "이번 라운드까지 해당 칸에 락존을 형성한다.", cardType = SkillCardType.Defense, rank = 1, round = 2 });
        list.skillCardDatas.Add(new SkillCardData { id = 3014, name = "철벽의 자세", effect = "해당 캐릭터는 방어한다.", cardType = SkillCardType.Defense, rank = 1, round = 1 });
        list.skillCardDatas.Add(new SkillCardData { id = 3015, name = "정보 우위", effect = "해당 캐릭터는 방어한다. 그 후 이동한다.", cardType = SkillCardType.Defense, rank = 2, round = 1 });
        list.skillCardDatas.Add(new SkillCardData { id = 3016, name = "변화: 물질", effect = "다음 라운드까지 해당 칸에 락존을 형성한다.\n(해당 칸에 캐릭터가 있을 경우, 효과 처리되지 않는다.)", cardType = SkillCardType.Defense, rank = 1, round = 2 });
        #endregion

        #region Buff
        list.skillCardDatas.Add(new SkillCardData { id = 4001, name = "오색방진", effect = "해당 칸에 있는 캐릭터는 추가 HP + 2 를 올린다.", cardType = SkillCardType.Buff, rank = 1, round = 3 });
        list.skillCardDatas.Add(new SkillCardData { id = 4002, name = "피의 맹세", effect = "캐릭터의 HP - 1 감소. \"피의 맹세 카운터\" 2 개를 올린다.\n(최대 3 개)", cardType = SkillCardType.Buff, rank = 1, round = 1 });
        list.skillCardDatas.Add(new SkillCardData { id = 4004, name = "샤이닝", effect = "해당 칸에 \"샤이닝 카운터\" 1 개를 준다.\n\"샤이닝 카운터\" 1 개당 데미지 1 감소한다. (최대 5 개)", cardType = SkillCardType.Buff, rank = 1, round = 1 });
        list.skillCardDatas.Add(new SkillCardData { id = 4005, name = "정조존", effect = "\"정조준 카운터\" 1 개를 올린다. (최대 6 개)", cardType = SkillCardType.Buff, rank = 1, round = 1 });
        list.skillCardDatas.Add(new SkillCardData { id = 4007, name = "해방의 빛", effect = "해당 칸에 HP + 1 을 부여한다.", cardType = SkillCardType.Buff, rank = 1, round = 2 });
        list.skillCardDatas.Add(new SkillCardData { id = 4008, name = "행운의 손길", effect = "동전을 굴린다. 앞면이 나온다면 해당 칸에 HP + 3 을 부여한다.", cardType = SkillCardType.Buff, rank = 2, round = 3 });
        list.skillCardDatas.Add(new SkillCardData { id = 4009, name = "지휘 강화령", effect = "해당 칸에 있는 캐릭터에게 \"지휘 강화령 카운터\"를 올린다. (최대 1 개)\n캐릭터가 스킬 사용 시 \"지휘 강화령 카운터\"를 소모하고, 추가 데미지 + 2 를 준다.", cardType = SkillCardType.Buff, rank = 1, round = 3 });
        list.skillCardDatas.Add(new SkillCardData { id = 4011, name = "더러운 거래", effect = "해당 칸에 있는 캐릭터의 HP 를 1 소모하고, MP 를 2 올린다.", cardType = SkillCardType.Buff, rank = 1, round = 3 });
        list.skillCardDatas.Add(new SkillCardData { id = 4012, name = "변환: 생명", effect = "해당 칸에 있는 캐릭터는 MP 를 가능한 만큼 소모하고, HP 를 소모한 만큼 올린다.", cardType = SkillCardType.Buff, rank = 1, round = 1 });
        list.skillCardDatas.Add(new SkillCardData { id = 4013, name = "커맨딩 그레이스", effect = "해당 칸에서 공격 시 데미지 + 4 를 부여한다.", cardType = SkillCardType.Buff, rank = 2, round = 1 });
        list.skillCardDatas.Add(new SkillCardData { id = 4014, name = "배너 오브 바우", effect = "해당 칸의 대상자에게 HP + 2, MP + 1 을 부여한다.", cardType = SkillCardType.Buff, rank = 2, round = 3 });
        list.skillCardDatas.Add(new SkillCardData { id = 4015, name = "로열 펄스", effect = "해당 칸에 추가 MP + 2, 추가 데미지 + 2 를 1 회 부여한다.\n(중첩 최대 1 회)", cardType = SkillCardType.Buff, rank = 3, round = 6 });
        list.skillCardDatas.Add(new SkillCardData { id = 4016, name = "글레이셜 리듬", effect = "해당 칸에 있는 캐릭터에게 \"글레이셜 카운터\"를 1 개 부여한다. (최대 1 회)\n글레이셜 카운터: 일정 이동 추가 + 1", cardType = SkillCardType.Buff, rank = 1, round = 1 });
        list.skillCardDatas.Add(new SkillCardData { id = 4017, name = "프로즌 팩트", effect = "해당 대상자에 있는 캐릭터에게 \"프로즌 카운터\"를 1 개 부여한다. (최대 1 회)\n프로즌 카운터: 해당 캐릭터를 방어한다.", cardType = SkillCardType.Buff, rank = 2, round = 1 });
        list.skillCardDatas.Add(new SkillCardData { id = 4020, name = "페더 왈츠", effect = "해당 칸에 HP + 4 를 부여한다.", cardType = SkillCardType.Buff, rank = 2, round = 3 });
        list.skillCardDatas.Add(new SkillCardData { id = 4021, name = "바람의 노래", effect = "해당 칸에 MP + 4 를 부여한다.", cardType = SkillCardType.Buff, rank = 2, round = 3 });
        list.skillCardDatas.Add(new SkillCardData { id = 4022, name = "천상의 아리아", effect = "해당 칸에 HP + 2, MP + 2 를 부여한다.", cardType = SkillCardType.Buff, rank = 3, round = 6 });
        list.skillCardDatas.Add(new SkillCardData { id = 4023, name = "하늘의 밀물", effect = "해당 칸에 있는 캐릭터를 회복한다.", cardType = SkillCardType.Buff, rank = 2, round = 3 });
        list.skillCardDatas.Add(new SkillCardData { id = 4024, name = "천상의 선율", effect = "해당 대상자에게 버프를 부여한다.", cardType = SkillCardType.Buff, rank = 2, round = 3 });
        #endregion

        #region Debuff
        list.skillCardDatas.Add(new SkillCardData { id = 5001, name = "사일런트 그리모어", effect = "해당 칸에 있는 캐릭터의 버프를 제거한다.", cardType = SkillCardType.Debuff, rank = 2, round = 6 });
        list.skillCardDatas.Add(new SkillCardData { id = 5002, name = "문릿 케이지", effect = "해당 칸에 있는 캐릭터를 속박한다.", cardType = SkillCardType.Debuff, rank = 1, round = 3 });
        list.skillCardDatas.Add(new SkillCardData { id = 5003, name = "저주의 낙인", effect = "해당 칸에서 데미지를 받는 경우, 추가 데미지 + 1 을 받는다.", cardType = SkillCardType.Debuff, rank = 1, round = 1 });
        list.skillCardDatas.Add(new SkillCardData { id = 5004, name = "어둠의 속삭임", effect = "다음 라운드까지 해당 칸에 있는 캐릭터는 속박한다.", cardType = SkillCardType.Debuff, rank = 1, round = 1 });
        list.skillCardDatas.Add(new SkillCardData { id = 5005, name = "사일런트 오더", effect = "이번 턴까지 해당 칸에 있는 캐릭터는 침묵한다.", cardType = SkillCardType.Debuff, rank = 3, round = 3 });
        list.skillCardDatas.Add(new SkillCardData { id = 5006, name = "앱솔루트", effect = "이번 턴까지 해당 칸에 있는 캐릭터는 기절한다.", cardType = SkillCardType.Debuff, rank = 4, round = 3 });
        list.skillCardDatas.Add(new SkillCardData { id = 5007, name = "크로노 실", effect = "두 번째 라운드까지 해당 칸에 있는 캐릭터는 침묵한다.", cardType = SkillCardType.Debuff, rank = 2, round = 2 });
        list.skillCardDatas.Add(new SkillCardData { id = 5008, name = "정신 부식", effect = "해당 대상에 있는 캐릭터는 침묵한다.", cardType = SkillCardType.Debuff, rank = 1, round = 2 });
        #endregion

        LoadDataFromJSON(list, "skillCard_data.json");
    }
    #endregion
#endif
}

#region 유저 데이터

#endregion

#region 게임 플레이 데이터
[Serializable]
public class GamePlayData
{
    public int maxCost;
    public int maxLeaderCount;
}
#endregion

#region 캐릭터 카드 데이터
[Serializable]
public class CharacterCardDataList
{
    public List<CharacterCardData> characterCardDatas;
}

[Serializable]
public class CharacterCardData
{
    public int id;
    public string name;
    public CharacterTier tier;
    public int cost;
    public int hp;
    public int mp;
    public CharacterRace race;
    public CharacterJob job;
    public List<int> skills;  //ex) 1001, 1002, 1003)
}
#endregion

#region 스킬 카드 데이터
[Serializable]
public class SkillCardDataList
{
    public List<SkillCardData> skillCardDatas;
}

[Serializable]
public class SkillCardData
{
    public int id;  //CharacterCardData의 skills가 키값 ex) 1001
    public string name;
    public string effect;
    public SkillCardType cardType;
    public int? rank;  //MP소모량은 Rank랑 동일
    /* 추후 다시 정리
    스킬 사거리
    스킬 범위
    public SkillRangeType rangeType;
    public List<(int dq, int dr, Color color)> customOffsetRange; */
    public int? round;  //지속시간
}
#endregion
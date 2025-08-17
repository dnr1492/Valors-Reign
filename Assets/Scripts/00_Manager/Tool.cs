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
        //Resources/Datas ���
        string path = Path.Combine(Application.dataPath, "Resources/Datas");
        string jsonPath = Path.Combine(path, fileName);

        //JSON ���� ����
        string jsonData = JsonUtility.ToJson(data, true);
        File.WriteAllText(jsonPath, jsonData);

        Debug.Log($"JSON data saved at: {jsonPath}");
        AssetDatabase.Refresh();
    }

    #region ���� �÷��� ������
    [MenuItem("�����/Generate gamePlay_data")]
    private static void GenerateGamePlayData()
    {
        GamePlayData gamePlayData = new GamePlayData {
            maxCost = 10,
            maxLeaderCount = 1
        };

        LoadDataFromJSON(gamePlayData, "gamePlay_data.json");
    }
    #endregion

    #region ĳ���� ī�� ������
    [MenuItem("�����/Generate characterCard_data")]
    private static void GenerateCharacterCardData()
    {
        //JSON ������ ����
        CharacterCardDataList list = new CharacterCardDataList {
            characterCardDatas = new List<CharacterCardData>()
        };

        list.characterCardDatas.Add(new CharacterCardData { id = 101, name = "��� ���к�", tier = CharacterTier.Low, cost = 2, hp = 4, mp = 1, race = CharacterRace.Primordial, job = CharacterJob.Tanker, skills = new List<int> { 1000, 3001 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 102, name = "�������� â��", tier = CharacterTier.Low, cost = 2, hp = 3, mp = 1, race = CharacterRace.Primordial, job = CharacterJob.Tanker, skills = new List<int> { 1000, 2001, 3002 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 103, name = "�׸��� �� ��ȣ��", tier = CharacterTier.Low, cost = 2, hp = 2, mp = 2, race = CharacterRace.Primordial, job = CharacterJob.Tanker, skills = new List<int> { 1000, 3003, 3004 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 104, name = "ħ���� �����", tier = CharacterTier.Middle, cost = 3, hp = 4, mp = 2, race = CharacterRace.Primordial, job = CharacterJob.Tanker, skills = new List<int> { 1000, 3005, 2002 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 105, name = "������ ������", tier = CharacterTier.Middle, cost = 3, hp = 2, mp = 3, race = CharacterRace.Primordial, job = CharacterJob.Tanker, skills = new List<int> { 1000, 3006 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 106, name = "�������и� �θ� ��ȣ��", tier = CharacterTier.Middle, cost = 3, hp = 2, mp = 4, race = CharacterRace.Primordial, job = CharacterJob.Tanker, skills = new List<int> { 1000, 4001, 3007 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 107, name = "�걺 -����-", tier = CharacterTier.High, cost = 4, hp = 6, mp = 3, race = CharacterRace.Primordial, job = CharacterJob.Tanker, skills = new List<int> { 1000, 3008, 2003, 3009 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 108, name = "������ ���� -���-", tier = CharacterTier.High, cost = 4, hp = 2, mp = 6, race = CharacterRace.Primordial, job = CharacterJob.Tanker, skills = new List<int> { 1000, 5001, 5002, 3010, 3011 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 109, name = "���尡�� -�ΰ�-", tier = CharacterTier.High, cost = 4, hp = 6, mp = 3, race = CharacterRace.Primordial, job = CharacterJob.Tanker, skills = new List<int> { 1000, 4002, 2004, 3012, 4003 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 110, name = "���� ��� -��-", tier = CharacterTier.Boss, cost = 0, hp = 2, mp = 1, race = CharacterRace.Primordial, job = CharacterJob.Tanker, skills = new List<int> { 1000, 3013 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 111, name = "���� ���� -����Ƽ-", tier = CharacterTier.Boss, cost = 0, hp = 2, mp = 1, race = CharacterRace.Primordial, job = CharacterJob.Tanker, skills = new List<int> { 1000, 4004, 4005 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 112, name = "ö�� ���� -�츣-", tier = CharacterTier.Boss, cost = 1, hp = 3, mp = 1, race = CharacterRace.Primordial, job = CharacterJob.Tanker, skills = new List<int> { 1000, 3014 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 113, name = "�Ͼ�˻�", tier = CharacterTier.Low, cost = 2, hp = 3, mp = 2, race = CharacterRace.Primordial, job = CharacterJob.Dealer, skills = new List<int> { 1000, 2005 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 114, name = "�׸��� �� �ϻ���", tier = CharacterTier.Low, cost = 2, hp = 2, mp = 2, race = CharacterRace.Primordial, job = CharacterJob.Dealer, skills = new List<int> { 1000, 2006, 2007 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 115, name = "������ �Բ� �ȴ� �ϻ���", tier = CharacterTier.Low, cost = 2, hp = 2, mp = 3, race = CharacterRace.Primordial, job = CharacterJob.Dealer, skills = new List<int> { 1000, 2008 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 116, name = "������ ���", tier = CharacterTier.Middle, cost = 3, hp = 4, mp = 2, race = CharacterRace.Primordial, job = CharacterJob.Dealer, skills = new List<int> { 1000, 2009, 2010 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 117, name = "Ǫ������ ���ݼ�", tier = CharacterTier.Middle, cost = 3, hp = 2, mp = 3, race = CharacterRace.Primordial, job = CharacterJob.Dealer, skills = new List<int> { 1000, 4005, 1001, 2011, 4006 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 118, name = "������ ���Ȱ�", tier = CharacterTier.Middle, cost = 3, hp = 3, mp = 2, race = CharacterRace.Primordial, job = CharacterJob.Dealer, skills = new List<int> { 1000, 2012, 2013, 2014 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 119, name = "ī�̸� ������", tier = CharacterTier.High, cost = 4, hp = 6, mp = 3, race = CharacterRace.Primordial, job = CharacterJob.Dealer, skills = new List<int> { 1000, 2015, 2016, 2017 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 120, name = "������ -����ĭ-", tier = CharacterTier.High, cost = 4, hp = 9, mp = 0, race = CharacterRace.Primordial, job = CharacterJob.Dealer, skills = new List<int> { 1000, 2018, 2019, 2020 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 121, name = "���� -�ܿ�-", tier = CharacterTier.High, cost = 4, hp = 5, mp = 4, race = CharacterRace.Primordial, job = CharacterJob.Dealer, skills = new List<int> { 1000, 2022, 2023, 2024 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 122, name = "�����Ҳ� -û��-", tier = CharacterTier.Boss, cost = 1, hp = 2, mp = 1, race = CharacterRace.Primordial, job = CharacterJob.Dealer, skills = new List<int> { 1000, 2025, 2026 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 123, name = "�̰��� â -����Ʈ-", tier = CharacterTier.Boss, cost = 0, hp = 2, mp = 1, race = CharacterRace.Primordial, job = CharacterJob.Dealer, skills = new List<int> { 1000, 2027 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 124, name = "������ -�ڽ���-", tier = CharacterTier.Boss, cost = 0, hp = 2, mp = 1, race = CharacterRace.Primordial, job = CharacterJob.Dealer, skills = new List<int> { 1000, 2028 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 125, name = "������ ġ����", tier = CharacterTier.Low, cost = 2, hp = 1, mp = 4, race = CharacterRace.Primordial, job = CharacterJob.Supporter, skills = new List<int> { 1000, 4007 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 126, name = "����� ġ���", tier = CharacterTier.Low, cost = 2, hp = 2, mp = 3, race = CharacterRace.Primordial, job = CharacterJob.Supporter, skills = new List<int> { 1000, 4008 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 127, name = "���� ������ ���ֻ�", tier = CharacterTier.Low, cost = 2, hp = 2, mp = 2, race = CharacterRace.Primordial, job = CharacterJob.Supporter, skills = new List<int> { 1000, 5003, 5004 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 128, name = "����� ������", tier = CharacterTier.Middle, cost = 3, hp = 2, mp = 3, race = CharacterRace.Primordial, job = CharacterJob.Supporter, skills = new List<int> { 1000, 1002, 4009, 3015, 4010 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 129, name = "��Ȱ�� ������", tier = CharacterTier.Middle, cost = 3, hp = 1, mp = 3, race = CharacterRace.Primordial, job = CharacterJob.Supporter, skills = new List<int> { 1000, 4011, 1003 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 130, name = "������ ���� ���� ���ݼ���", tier = CharacterTier.Middle, cost = 3, hp = 2, mp = 4, race = CharacterRace.Primordial, job = CharacterJob.Supporter, skills = new List<int> { 1000, 4012, 3016 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 131, name = "�ο� ���̵�", tier = CharacterTier.High, cost = 4, hp = 3, mp = 5, race = CharacterRace.Primordial, job = CharacterJob.Supporter, skills = new List<int> { 1000, 4013, 5005, 4014, 4015 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 132, name = "�Ϲ��� �������� -������-", tier = CharacterTier.High, cost = 4, hp = 5, mp = 4, race = CharacterRace.Primordial, job = CharacterJob.Supporter, skills = new List<int> { 1000, 4016, 4017, 5006, 4018, 4019 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 133, name = "��ǳ�� ���� -�󿡳�-", tier = CharacterTier.High, cost = 4, hp = 3, mp = 6, race = CharacterRace.Primordial, job = CharacterJob.Supporter, skills = new List<int> { 1000, 4020, 4021, 4022 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 134, name = "�ð��� �ȴ� �� -���-", tier = CharacterTier.Boss, cost = 1, hp = 1, mp = 2, race = CharacterRace.Primordial, job = CharacterJob.Supporter, skills = new List<int> { 1000, 4023, 5007 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 135, name = "õ���� �︲ -��-", tier = CharacterTier.Boss, cost = 1, hp = 2, mp = 2, race = CharacterRace.Primordial, job = CharacterJob.Supporter, skills = new List<int> { 1000, 4024 } });
        list.characterCardDatas.Add(new CharacterCardData { id = 136, name = "���� -�紩��-", tier = CharacterTier.Boss, cost = 0, hp = 2, mp = 1, race = CharacterRace.Primordial, job = CharacterJob.Supporter, skills = new List<int> { 1000, 5008 } });

        LoadDataFromJSON(list, "characterCard_data.json");
    }
    #endregion

    #region ��ų ī�� ������
    [MenuItem("�����/Generate skillCard_data")]
    private static void GenerateSkillCardData()
    {
        //JSON ������ ����
        SkillCardDataList list = new SkillCardDataList {
            skillCardDatas = new List<SkillCardData>()
        };

        #region Move
        list.skillCardDatas.Add(new SkillCardData { id = 1000, name = "�⺻ �̵�ī��", effect = "�ش� ����� �� �� ������ �̵��Ѵ�. �̵��Ϸ��� ĭ�� �ٸ� ĳ���Ͱ� �����ϴ� ��� ������ 1 �� �ְ� �̵����� �ʴ´�.", cardType = SkillCardType.Move, rank = 0, round = 0 });
        list.skillCardDatas.Add(new SkillCardData { id = 1001, name = "�ż�", effect = "�ش� �̵� ĭ���� �̵��Ѵ�. \"������ ī����\"�� ���� �������.", cardType = SkillCardType.Move, rank = 2, round = 0 });
        list.skillCardDatas.Add(new SkillCardData { id = 1002, name = "���� ���ġ", effect = "�ش� ĭ�� ĳ���Ͱ� ���� ���� ���, ĭ�� �ִ� ĳ������ ��ġ�� �����Ѵ�.", cardType = SkillCardType.Move, rank = 2, round = 0 });
        list.skillCardDatas.Add(new SkillCardData { id = 1003, name = "���� ����", effect = "�ش� ĭ�� ĳ���Ͱ� 1 ĭ���� ���� ���, �ش� ĭ �� �ٸ� ĭ���� �̵��Ѵ�.", cardType = SkillCardType.Move, rank = 3, round = 0 });
        #endregion

        #region Attack
        list.skillCardDatas.Add(new SkillCardData { id = 2001, name = "���Ǿ��", effect = "�ش� ĭ�� 2 �������� �ش�.", cardType = SkillCardType.Attack, rank = 1, round = 1 });
        list.skillCardDatas.Add(new SkillCardData { id = 2002, name = "������ �ݰ�", effect = "�ش� ĳ���ʹ� �������� 2 �����Ѵ�. �� �� �ش� ĭ�� ĳ���ʹ� 2 �������� �ش�.", cardType = SkillCardType.Attack, rank = 2, round = 4 });
        list.skillCardDatas.Add(new SkillCardData { id = 2003, name = "ȣ�Ɽ", effect = "�ش� ĭ�� 3 �������� �ش�.", cardType = SkillCardType.Attack, rank = 1, round = 1 });
        list.skillCardDatas.Add(new SkillCardData { id = 2004, name = "��ó ��ȯ", effect = "�ش� ĭ�� 4 �������� �ش�. \"���� �ͼ� ī����\" 2 ���� �ø���. (�ִ� 3 ��)", cardType = SkillCardType.Attack, rank = 2, round = 2 });
        list.skillCardDatas.Add(new SkillCardData { id = 2005, name = "�Ͼ�˹�", effect = "�ش� ĭ�� 2 �������� �ش�.", cardType = SkillCardType.Attack, rank = 1, round = 1 });
        list.skillCardDatas.Add(new SkillCardData { id = 2006, name = "�׸��� ����", effect = "�ش� ĭ�� 2 �������� �ش�.", cardType = SkillCardType.Attack, rank = 1, round = 2 });
        list.skillCardDatas.Add(new SkillCardData { id = 2007, name = "�׸��� ������", effect = "�ش� ĭ�� 2 �������� �ش�.", cardType = SkillCardType.Attack, rank = 1, round = 2 });
        list.skillCardDatas.Add(new SkillCardData { id = 2008, name = "������ ���ڱ�", effect = "�ش� ĭ�� 2 �������� �ش�.", cardType = SkillCardType.Attack, rank = 3, round = 4 });
        list.skillCardDatas.Add(new SkillCardData { id = 2009, name = "���ӻ��", effect = "�ش� ĭ�� 2 �������� �ش�.", cardType = SkillCardType.Attack, rank = 2, round = 3 });
        list.skillCardDatas.Add(new SkillCardData { id = 2010, name = "�ٶ�������", effect = "�ش� ���� ĭ�� 2 �������� �ش�. �ش� �̵� ĭ���� �̵��Ѵ�.", cardType = SkillCardType.Attack, rank = 2, round = 3 });
        list.skillCardDatas.Add(new SkillCardData { id = 2011, name = "ũ��Ƽ�ü�", effect = "�ش� ĭ�� 2 �������� �ش�. \"������ ī����\"�� ���� �Ҹ��ϰ�, �Ҹ��� ī���� ����ŭ �߰� �������� �ش�.", cardType = SkillCardType.Attack, rank = 1, round = 1 });
        list.skillCardDatas.Add(new SkillCardData { id = 2012, name = "����", effect = "�ش� ĭ�� 2 �������� �ش�.", cardType = SkillCardType.Attack, rank = 1, round = 3 });
        list.skillCardDatas.Add(new SkillCardData { id = 2013, name = "�ӻ�", effect = "�ش� ĭ�� 2 �������� �ش�.", cardType = SkillCardType.Attack, rank = 1, round = 3 });
        list.skillCardDatas.Add(new SkillCardData { id = 2014, name = "������ ����", effect = "�ش� ĭ�� 2 �������� �ش�.", cardType = SkillCardType.Attack, rank = 1, round = 2 });
        list.skillCardDatas.Add(new SkillCardData { id = 2015, name = "ȭ���� ����", effect = "�ش� ĭ�� 2 �������� �ش�.", cardType = SkillCardType.Attack, rank = 2, round = 3 });
        list.skillCardDatas.Add(new SkillCardData { id = 2016, name = "��Ÿ�� �ͼ�", effect = "�ش� ĭ�� 2 �������� �ش�. �� �� ���� ���°� �ȴ�.", cardType = SkillCardType.Attack, rank = 2, round = 1 });
        list.skillCardDatas.Add(new SkillCardData { id = 2017, name = "������ ����", effect = "�ش� ĭ�� 2 �������� �ش�.", cardType = SkillCardType.Attack, rank = 3, round = 9 });
        list.skillCardDatas.Add(new SkillCardData { id = 2018, name = "���� ������", effect = "- ��ų �ߵ� �� MP�� �ƴ� HP�� �Ҹ��Ѵ�. (��ų �ߵ� �� �̹� ���� HP ȸ�� �Ұ�)\n- �ش� ĭ�� 3 �������� �ش�. �ش� ĭ�� ĳ���Ͱ� ���ٸ� �� ĳ������ HP 1 ����.\n- \"������ ī����\"�� 1 �� �ø���. (�ִ� 10 ��)", cardType = SkillCardType.Attack, rank = 1, round = 1 });
        list.skillCardDatas.Add(new SkillCardData { id = 2019, name = "ũ���� �ξ�", effect = "- ��ų �ߵ� �� MP�� �ƴ� HP�� �Ҹ��Ѵ�. (��ų �ߵ� �� �̹� ���� HP ȸ�� �Ұ�)\n- �ش� ĭ�� 4 �������� �ش�. �� �� \"������ ī����\"�� �Ҹ��ϰ�, �Ҹ��� ī���� ����ŭ �߰� �������� �ش�.\n- \"������ ī����\"�� 2 �� �ø���. (�ִ� 10 ��)", cardType = SkillCardType.Attack, rank = 2, round = 2 });
        list.skillCardDatas.Add(new SkillCardData { id = 2020, name = "���� ��Ʈ����ũ", effect = "- ��ų �ߵ� �� MP�� �ƴ� HP�� �Ҹ��Ѵ�. (��ų �ߵ� �� �̹� ���� HP ȸ�� �Ұ�)\n- �ش� ���� ĭ�� 3 �������� �ش�. �� �� \"������ ī����\"�� �Ҹ��ϰ�, �Ҹ��� ī���� ����ŭ �߰� �������� �ش�.\n- �ش� �̵� ĭ���� �̵��Ѵ�.\n- \"������ ī����\"�� 3 �� �ø���. (�ִ� 10 ��)", cardType = SkillCardType.Attack, rank = 3, round = 3 });
        list.skillCardDatas.Add(new SkillCardData { id = 2022, name = "����-�ϼ�", effect = "�ش� ĭ�� 4 �������� �ش�.", cardType = SkillCardType.Attack, rank = 1, round = 1 });
        list.skillCardDatas.Add(new SkillCardData { id = 2023, name = "����-����", effect = "�ش� ĭ�� 4 �������� �ش�.", cardType = SkillCardType.Attack, rank = 2, round = 4 });
        list.skillCardDatas.Add(new SkillCardData { id = 2024, name = "����-����", effect = "�ش� ĭ�� 6 �������� �ش�.", cardType = SkillCardType.Attack, rank = 3, round = 6 });
        list.skillCardDatas.Add(new SkillCardData { id = 2025, name = "�濰��", effect = "�ش� ĭ�� �������� �ش�.", cardType = SkillCardType.Attack, rank = 1, round = 2 });
        list.skillCardDatas.Add(new SkillCardData { id = 2026, name = "���κ�", effect = "�ش� ĭ�� �������� �ش�.", cardType = SkillCardType.Attack, rank = 1, round = 1 });
        list.skillCardDatas.Add(new SkillCardData { id = 2027, name = "������ ������ �ϰ�", effect = "�ش� ��󿡰� �������� �ش�.", cardType = SkillCardType.Attack, rank = 2, round = 3 });
        list.skillCardDatas.Add(new SkillCardData { id = 2028, name = "�ҵ� ���� ����", effect = "�ش� ��󿡰� �������� �ش�.", cardType = SkillCardType.Attack, rank = 1, round = 1 });
        #endregion

        #region Defense
        list.skillCardDatas.Add(new SkillCardData { id = 3001, name = "����Ʈ ����", effect = "�ش� ĳ���ʹ� ����Ѵ�.", cardType = SkillCardType.Defense, rank = 1, round = 1 });
        list.skillCardDatas.Add(new SkillCardData { id = 3002, name = "���Ǿ��", effect = "�ش� ĳ���ʹ� �������� 2 �����Ѵ�.", cardType = SkillCardType.Defense, rank = 2, round = 1 });
        list.skillCardDatas.Add(new SkillCardData { id = 3003, name = "�׸��� ��", effect = "���� ������� �ش� ĭ�� ������ �����Ѵ�.\n(�ش� ĭ�� ĳ���Ͱ� ���� ���, ��ų�� ȿ�� ó������ �ʴ´�.)", cardType = SkillCardType.Defense, rank = 1, round = 1 });
        list.skillCardDatas.Add(new SkillCardData { id = 3004, name = "��ҿ����� �ݰ�", effect = "���� ������� �ش� ĳ���Ͱ� �޴� �������� �ݻ��Ѵ�.", cardType = SkillCardType.Defense, rank = 2, round = 1 });
        list.skillCardDatas.Add(new SkillCardData { id = 3005, name = "ö�� �¼�", effect = "�ش� ĳ���ʹ� �������� 2 �����Ѵ�. �� �� �ش� ĭ�� ĳ���ʹ� �߰� HP + 2 �� �ø���.", cardType = SkillCardType.Defense, rank = 2, round = 4 });
        list.skillCardDatas.Add(new SkillCardData { id = 3006, name = "�׷��� ���̴ϱ�", effect = "�̹� �ϱ��� �ش� ĭ�� ������ �����Ѵ�.\n������ �ִ� ĳ���ʹ� �� ĭ �̵��Ѵ�.", cardType = SkillCardType.Defense, rank = 3, round = 3 });
        list.skillCardDatas.Add(new SkillCardData { id = 3007, name = "�����ݻ���", effect = "�̹� ������� �ش� ĭ�� �������� �ݻ��Ѵ�.", cardType = SkillCardType.Defense, rank = 2, round = 1 });
        list.skillCardDatas.Add(new SkillCardData { id = 3008, name = "������", effect = "�ش� ĭ�� 1 �������� �ش�.", cardType = SkillCardType.Defense, rank = 1, round = 1 });
        list.skillCardDatas.Add(new SkillCardData { id = 3009, name = "��������", effect = "�ش� ĭ�� 1 �������� �ش�.\n���� ������� �ش� ĭ�� ������ �����Ѵ�.", cardType = SkillCardType.Defense, rank = 2, round = 1 });
        list.skillCardDatas.Add(new SkillCardData { id = 3010, name = "�̷����ε�", effect = "�� ��° ������� �ش� ĭ�� �������� ũ��Ƽ�÷� �ݻ��Ѵ�.", cardType = SkillCardType.Defense, rank = 3, round = 2 });
        list.skillCardDatas.Add(new SkillCardData { id = 3011, name = "���극��Ŀ", effect = "�̹� �ϱ��� �ش� ĭ�� ����Ѵ�.", cardType = SkillCardType.Defense, rank = 3, round = 3 });
        list.skillCardDatas.Add(new SkillCardData { id = 3012, name = "��ȫ�� ��", effect = "�̹� �ϱ��� �ش� ĭ�� ������ �����Ѵ�.\n�ش� ĭ�� �ִ� ĳ���ʹ� �ӹ�, ���� ���°� �ȴ�.\n\"���� �ͼ� ī����\" 1 ���� �����ϰ� ĭ ���� �ø� �� �ִ�.", cardType = SkillCardType.Defense, rank = 3, round = 1 });
        list.skillCardDatas.Add(new SkillCardData { id = 3013, name = "���õ���", effect = "�̹� ������� �ش� ĭ�� ������ �����Ѵ�.", cardType = SkillCardType.Defense, rank = 1, round = 2 });
        list.skillCardDatas.Add(new SkillCardData { id = 3014, name = "ö���� �ڼ�", effect = "�ش� ĳ���ʹ� ����Ѵ�.", cardType = SkillCardType.Defense, rank = 1, round = 1 });
        list.skillCardDatas.Add(new SkillCardData { id = 3015, name = "���� ����", effect = "�ش� ĳ���ʹ� ����Ѵ�. �� �� �̵��Ѵ�.", cardType = SkillCardType.Defense, rank = 2, round = 1 });
        list.skillCardDatas.Add(new SkillCardData { id = 3016, name = "��ȭ-����", effect = "���� ������� �ش� ĭ�� ������ �����Ѵ�.\n(�ش� ĭ�� ĳ���Ͱ� ���� ���, ȿ�� ó������ �ʴ´�.)", cardType = SkillCardType.Defense, rank = 1, round = 2 });
        #endregion

        #region Buff
        list.skillCardDatas.Add(new SkillCardData { id = 4001, name = "��������", effect = "�ش� ĭ�� �ִ� ĳ���ʹ� �߰� HP + 2 �� �ø���.", cardType = SkillCardType.Buff, rank = 1, round = 3 });
        list.skillCardDatas.Add(new SkillCardData { id = 4002, name = "���� �ͼ�", effect = "ĳ������ HP - 1 ����. \"���� �ͼ� ī����\" 2 ���� �ø���.\n(�ִ� 3 ��)", cardType = SkillCardType.Buff, rank = 1, round = 1 });
        list.skillCardDatas.Add(new SkillCardData { id = 4004, name = "���̴�", effect = "�ش� ĭ�� \"���̴� ī����\" 1 ���� �ش�.\n\"���̴� ī����\" 1 ���� ������ 1 �����Ѵ�. (�ִ� 5 ��)", cardType = SkillCardType.Buff, rank = 1, round = 1 });
        list.skillCardDatas.Add(new SkillCardData { id = 4005, name = "������", effect = "\"������ ī����\" 1 ���� �ø���. (�ִ� 6 ��)", cardType = SkillCardType.Buff, rank = 1, round = 1 });
        list.skillCardDatas.Add(new SkillCardData { id = 4007, name = "�ع��� ��", effect = "�ش� ĭ�� HP + 1 �� �ο��Ѵ�.", cardType = SkillCardType.Buff, rank = 1, round = 2 });
        list.skillCardDatas.Add(new SkillCardData { id = 4008, name = "����� �ձ�", effect = "������ ������. �ո��� ���´ٸ� �ش� ĭ�� HP + 3 �� �ο��Ѵ�.", cardType = SkillCardType.Buff, rank = 2, round = 3 });
        list.skillCardDatas.Add(new SkillCardData { id = 4009, name = "���� ��ȭ��", effect = "�ش� ĭ�� �ִ� ĳ���Ϳ��� \"���� ��ȭ�� ī����\"�� �ø���. (�ִ� 1 ��)\nĳ���Ͱ� ��ų ��� �� \"���� ��ȭ�� ī����\"�� �Ҹ��ϰ�, �߰� ������ + 2 �� �ش�.", cardType = SkillCardType.Buff, rank = 1, round = 3 });
        list.skillCardDatas.Add(new SkillCardData { id = 4011, name = "������ �ŷ�", effect = "�ش� ĭ�� �ִ� ĳ������ HP �� 1 �Ҹ��ϰ�, MP �� 2 �ø���.", cardType = SkillCardType.Buff, rank = 1, round = 3 });
        list.skillCardDatas.Add(new SkillCardData { id = 4012, name = "��ȯ-����", effect = "�ش� ĭ�� �ִ� ĳ���ʹ� MP �� ������ ��ŭ �Ҹ��ϰ�, HP �� �Ҹ��� ��ŭ �ø���.", cardType = SkillCardType.Buff, rank = 1, round = 1 });
        list.skillCardDatas.Add(new SkillCardData { id = 4013, name = "Ŀ�ǵ� �׷��̽�", effect = "�ش� ĭ���� ���� �� ������ + 4 �� �ο��Ѵ�.", cardType = SkillCardType.Buff, rank = 2, round = 1 });
        list.skillCardDatas.Add(new SkillCardData { id = 4014, name = "��� ���� �ٿ�", effect = "�ش� ĭ�� ����ڿ��� HP + 2, MP + 1 �� �ο��Ѵ�.", cardType = SkillCardType.Buff, rank = 2, round = 3 });
        list.skillCardDatas.Add(new SkillCardData { id = 4015, name = "�ο� �޽�", effect = "�ش� ĭ�� �߰� MP + 2, �߰� ������ + 2 �� 1 ȸ �ο��Ѵ�.\n(��ø �ִ� 1 ȸ)", cardType = SkillCardType.Buff, rank = 3, round = 6 });
        list.skillCardDatas.Add(new SkillCardData { id = 4016, name = "�۷��̼� ����", effect = "�ش� ĭ�� �ִ� ĳ���Ϳ��� \"�۷��̼� ī����\"�� 1 �� �ο��Ѵ�. (�ִ� 1 ȸ)\n�۷��̼� ī����: ���� �̵� �߰� + 1", cardType = SkillCardType.Buff, rank = 1, round = 1 });
        list.skillCardDatas.Add(new SkillCardData { id = 4017, name = "������ ��Ʈ", effect = "�ش� ����ڿ� �ִ� ĳ���Ϳ��� \"������ ī����\"�� 1 �� �ο��Ѵ�. (�ִ� 1 ȸ)\n������ ī����: �ش� ĳ���͸� ����Ѵ�.", cardType = SkillCardType.Buff, rank = 2, round = 1 });
        list.skillCardDatas.Add(new SkillCardData { id = 4020, name = "��� ����", effect = "�ش� ĭ�� HP + 4 �� �ο��Ѵ�.", cardType = SkillCardType.Buff, rank = 2, round = 3 });
        list.skillCardDatas.Add(new SkillCardData { id = 4021, name = "�ٶ��� �뷡", effect = "�ش� ĭ�� MP + 4 �� �ο��Ѵ�.", cardType = SkillCardType.Buff, rank = 2, round = 3 });
        list.skillCardDatas.Add(new SkillCardData { id = 4022, name = "õ���� �Ƹ���", effect = "�ش� ĭ�� HP + 2, MP + 2 �� �ο��Ѵ�.", cardType = SkillCardType.Buff, rank = 3, round = 6 });
        list.skillCardDatas.Add(new SkillCardData { id = 4023, name = "Ÿ�� ����", effect = "�ش� ĭ�� �ִ� ĳ���ʹ� �̵��Ѵ�.", cardType = SkillCardType.Buff, rank = 2, round = 3 });
        list.skillCardDatas.Add(new SkillCardData { id = 4024, name = "õ���� ����", effect = "�ش� ����ڿ��� ������ �ο��Ѵ�.", cardType = SkillCardType.Buff, rank = 2, round = 3 });
        #endregion

        #region Debuff
        list.skillCardDatas.Add(new SkillCardData { id = 5001, name = "���Ϸ�Ʈ �׸����", effect = "�ش� ĭ�� �ִ� ĳ������ ������ �����Ѵ�.", cardType = SkillCardType.Debuff, rank = 2, round = 6 });
        list.skillCardDatas.Add(new SkillCardData { id = 5002, name = "���� ������", effect = "�ش� ĭ�� �ִ� ĳ���͸� �ӹ��Ѵ�.", cardType = SkillCardType.Debuff, rank = 1, round = 3 });
        list.skillCardDatas.Add(new SkillCardData { id = 5003, name = "������ ����", effect = "�ش� ĭ���� �������� �޴� ���, �߰� ������ + 1 �� �޴´�.", cardType = SkillCardType.Debuff, rank = 1, round = 1 });
        list.skillCardDatas.Add(new SkillCardData { id = 5004, name = "����� �ӻ���", effect = "���� ������� �ش� ĭ�� �ִ� ĳ���ʹ� �ӹ��Ѵ�.", cardType = SkillCardType.Debuff, rank = 1, round = 1 });
        list.skillCardDatas.Add(new SkillCardData { id = 5005, name = "���Ϸ�Ʈ ����", effect = "�̹� �ϱ��� �ش� ĭ�� �ִ� ĳ���ʹ� ħ���Ѵ�.", cardType = SkillCardType.Debuff, rank = 3, round = 3 });
        list.skillCardDatas.Add(new SkillCardData { id = 5006, name = "�ۼַ�Ʈ", effect = "�̹� �ϱ��� �ش� ĭ�� �ִ� ĳ���ʹ� �����Ѵ�.", cardType = SkillCardType.Debuff, rank = 4, round = 3 });
        list.skillCardDatas.Add(new SkillCardData { id = 5007, name = "ũ�γ� ��", effect = "�� ��° ������� �ش� ĭ�� �ִ� ĳ���ʹ� ħ���Ѵ�.", cardType = SkillCardType.Debuff, rank = 2, round = 2 });
        list.skillCardDatas.Add(new SkillCardData { id = 5008, name = "���� �ν�", effect = "�ش� ��� �ִ� ĳ���ʹ� ħ���Ѵ�.", cardType = SkillCardType.Debuff, rank = 1, round = 2 });
        #endregion

        LoadDataFromJSON(list, "skillCard_data.json");
    }
    #endregion
#endif
}

#region ���� ������

#endregion

#region ���� �÷��� ������
[Serializable]
public class GamePlayData
{
    public int maxCost;
    public int maxLeaderCount;
}
#endregion

#region ĳ���� ī�� ������
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

#region ��ų ī�� ������
[Serializable]
public class SkillCardDataList
{
    public List<SkillCardData> skillCardDatas;
}

[Serializable]
public class SkillCardData
{
    public int id;  //CharacterCardData�� skills�� Ű�� ex) 1001
    public string name;
    public string effect;
    public SkillCardType cardType;
    public int rank;  //MP�Ҹ��� Rank�� ����
    public int round;  //���ӽð�

    //������ ���� (��� �� ���� �� ȿ�� �� Ʈ����)
    //1. skillCard_steps_pamphlet�� ��ųī�� ������ ����ó�� ����
    //2. CVS�� Ȯ���� ����
    //3. Tool�� Ȱ���ؼ� CVS �� steps�� ��ȯ
    //4. �ڵ����� skillCard_data�� ����
    public List<EffectStep> steps = new();
}
#endregion
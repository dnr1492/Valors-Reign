using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class CharacterInfo
{
    public int tokenKey;
    public int currentHp;
    public int currentMp;

    public bool IsDead => currentHp <= 0;
}

//������ ���� ������
[System.Serializable]
public struct OutlineSnapshot
{
    public Image img;
    public Color origColor;
    public bool origActive;  //GameObject Ȱ�� ���¸� ����
}

//���� ���Կ� ������ �� ���� �̵�
[System.Serializable]
public struct MoveOrder
{
    public int tokenKey;
    public Vector2Int fromHexPos;  //��� ��ǥ (���� ���)
    public Vector2Int toHexPos;    //���� ��ǥ
    public int roundOrder;         //���� Round
}

//���� ���Կ� ��ϵ� ��ųī���� ����
[System.Serializable]
public struct RoundCardInfo
{
    public int round;         //1 ~ 4
    public int cardId;        //��ųī�� ID (�⺻ �̵�ī�� = 1000)
    public int moveTokenKey;  //�⺻ �̵�ī�带 ������ ĳ���� ��ū Ű (������ -1)
}

//��� ���� ��ȹ
[System.Serializable]
public struct OppRoundPlan
{
    public RoundCardInfo[] cards;  //�ִ� 4��
}

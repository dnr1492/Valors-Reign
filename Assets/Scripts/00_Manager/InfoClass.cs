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

//원복을 위한 스냅샷
[System.Serializable]
public struct OutlineSnapshot
{
    public Image img;
    public Color origColor;
    public bool origActive;  //GameObject 활성 상태를 저장
}

//라운드 슬롯에 설정된 한 번의 이동
[System.Serializable]
public struct MoveOrder
{
    public int tokenKey;
    public Vector2Int fromHexPos;  //출발 좌표 (선택 당시)
    public Vector2Int toHexPos;    //도착 좌표
    public int roundOrder;         //현재 Round
}

//라운드 슬롯에 등록된 스킬카드의 정보
[System.Serializable]
public struct RoundCardInfo
{
    public int round;         //1 ~ 4
    public int cardId;        //스킬카드 ID (기본 이동카드 = 1000)
    public int moveTokenKey;  //기본 이동카드를 설정한 캐릭터 토큰 키 (없으면 -1)
}

//상대 라운드 계획
[System.Serializable]
public struct OppRoundPlan
{
    public RoundCardInfo[] cards;  //최대 4개
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HexTileBattleEvent : MonoBehaviour, IPointerClickHandler
{
    private HexTile hexTile;

    private void Awake()
    {
        hexTile = GetComponent<HexTile>();
    }

    public void OnPointerClick(PointerEventData e)
    {
        var ctrl = ControllerRegister.Get<MovementOrderController>();
        if (ctrl == null) return;

        //(1) 클릭한 Hex 좌표 계산
        var (col, row) = hexTile.GridPosition;
        var hex = new Vector2Int(col, row);

        //(2) 해당 타일에 '내 토큰'이 있으면 캐릭터 클릭 이벤트 전달 (타겟팅 상태 무관하게 항상 전달)
        if (hexTile.AssignedTokenKey.HasValue && hexTile.IsMyToken) {
            int tokenKey = hexTile.AssignedTokenKey.Value;
            ctrl.OnCharacterClicked(tokenKey, hex);
            return;
        }

        //(3) 그 외에는 목적지 클릭 시도
        ctrl.OnHexClicked(hex);
    }
}

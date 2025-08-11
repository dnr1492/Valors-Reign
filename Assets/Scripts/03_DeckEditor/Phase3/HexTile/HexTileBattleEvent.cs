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

    public void OnPointerClick(PointerEventData eventData)
    {
        var movementOrderCtrl = ControllerRegister.Get<MovementOrderController>();
        if (movementOrderCtrl == null || !movementOrderCtrl.IsTargeting || hexTile == null) return;

        //클릭한 Hex의 좌표
        var (col, row) = hexTile.GridPosition;
        var hex = new Vector2Int(col, row);

        //캐릭터가 있으면 캐릭터 선택
        if (hexTile.AssignedTokenKey.HasValue) movementOrderCtrl.OnCharacterClicked(hexTile.AssignedTokenKey.Value, hex);
        //캐릭터가 없으면 Hex 선택 (목적지)
        else movementOrderCtrl.OnHexClicked(hex);
    }
}

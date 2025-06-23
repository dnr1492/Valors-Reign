using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HexTileDraggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Canvas canvas;
    private HexTile hexTile;
    private RectTransform ghostRect;
    private GameObject dragGhost;
   
    private void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        hexTile = GetComponent<HexTile>();
    }

    private bool IsCaptainTier()
    {
        var key = hexTile.AssignedTokenKey;
        if (!key.HasValue) return false;

        var token = ControllerRegister.Get<CharacterTokenController>()
            .GetAllCharacterToken()
            .FirstOrDefault(t => t.Key == key.Value);

        return token?.Tier == EnumClass.CharacterTierAndCost.Captain;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (hexTile.AssignedTokenKey == null) return;
        if (IsCaptainTier()) return;

        //드래그용 복제 생성 (자기 자신 복제)
        dragGhost = Instantiate(gameObject, canvas.transform);
        dragGhost.name = "DragGhost";

        ghostRect = dragGhost.GetComponent<RectTransform>();
        ghostRect.sizeDelta = GetComponent<RectTransform>().sizeDelta;

        //복제된 오브젝트에서 필요 없는 컴포넌트 제거
        Destroy(dragGhost.GetComponent<HexTileDraggable>());

        //원래 위치랑 겹치지 않게 UI 제일 위로 보정
        ghostRect.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragGhost == null) return;
        ghostRect.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (dragGhost != null) {
            Destroy(dragGhost);
            dragGhost = null;
        }

        if (!hexTile.AssignedTokenKey.HasValue) return;

        if (GridManager.Instance.TryGetNearestSlot(Input.mousePosition, out var dropPos)) {
            var fromPos = hexTile.GridPosition;
            GridManager.Instance.MoveToken(fromPos, dropPos, hexTile);
        }
    }
}
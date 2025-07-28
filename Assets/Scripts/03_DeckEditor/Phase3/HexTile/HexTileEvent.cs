using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static EnumClass;

public class HexTileEvent : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    private Canvas canvas;
    private HexTile hexTile;
    private RectTransform ghostRect;
    private GameObject dragGhost;

    private GameObject cancelZone;
    private RectTransform cancelZoneRect;
    private Image cancelZoneImage;
    private readonly Color cancelDefaultColor = Color.gray;
    private readonly Color cancelHoverColor = Color.red;

    private void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        hexTile = GetComponent<HexTile>();

        cancelZone = UIManager.Instance.GetPopup<UIEditorDeckPhase3>("UIEditorDeckPhase3").GetCancelZone();
        cancelZone.SetActive(false);
        cancelZoneRect = cancelZone.GetComponent<RectTransform>();
        cancelZoneImage = cancelZone.GetComponentInChildren<Image>();
    }

    private bool IsBossTier()
    {
        var key = hexTile.AssignedTokenKey;
        if (!key.HasValue) return false;

        var token = ControllerRegister.Get<CharacterTokenController>()
            .GetAllCharacterToken()
            .FirstOrDefault(t => t.Key == key.Value);

        return token != null && token.Tier == CharacterTierAndCost.Boss;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (hexTile.AssignedTokenKey == null) return;
        if (IsBossTier()) return;

        //캐릭터 카드의 상태를 Back으로 설정
        var token = hexTile.AssignedToken;
        token.SetCardToBack();

        //드래그용 복제 생성 (자기 자신 복제)
        dragGhost = Instantiate(gameObject, canvas.transform);
        dragGhost.name = "DragGhost";
        ghostRect = dragGhost.GetComponent<RectTransform>();
        ghostRect.sizeDelta = GetComponent<RectTransform>().sizeDelta;

        //복제된 오브젝트에서 필요 없는 컴포넌트 제거
        Destroy(dragGhost.GetComponent<HexTileEvent>());

        //원래 위치랑 겹치지 않게 UI 제일 위로 보정
        ghostRect.SetAsLastSibling();

        //취소 영역 표시
        cancelZone.SetActive(true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragGhost == null) return;
        ghostRect.position = Input.mousePosition;

        //CancelZone 위에 있는지 체크해서 색상 강조
        if (RectTransformUtility.RectangleContainsScreenPoint(
            cancelZoneRect, Input.mousePosition, null)) {
            cancelZoneImage.color = cancelHoverColor;
        }
        else cancelZoneImage.color = cancelDefaultColor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (dragGhost != null) {
            Destroy(dragGhost);
            dragGhost = null;
        }

        cancelZone.SetActive(false);

        //CancelZone에 드롭된 경우 확정 취소
        bool isOverCancelZone = cancelZone != null &&
            RectTransformUtility.RectangleContainsScreenPoint(
                cancelZoneRect,
                Input.mousePosition,
                null
            );

        if (isOverCancelZone)
        {
            var token = hexTile.AssignedToken;
            token.SetTokenState(CharacterTokenState.Cancel);
            GridManager.Instance.RemoveTokenFromBattlefield(token);
            return;
        }

        //다시 일반 필드로 이동
        if (GridManager.Instance.TryGetNearestSlot(Input.mousePosition, out var dropPos))
        {
            var fromPos = hexTile.GridPosition;
            GridManager.Instance.MoveToken(fromPos, dropPos, hexTile);
        }
    }

    //캐릭터 토큰 클릭 시 정보 표시
    public void OnPointerClick(PointerEventData eventData)
    {
        if (hexTile.AssignedToken != null)
            hexTile.AssignedToken.ShowTokenInfo();
    }
}
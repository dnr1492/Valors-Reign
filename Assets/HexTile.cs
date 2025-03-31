using UnityEngine;
using UnityEngine.EventSystems;

public class HexTile : MonoBehaviour, IPointerClickHandler
{
    public Vector2Int gridPosition;  //타일의 좌표
    private HexGridManager gridManager;
    private RectTransform rt;

    public void Setup(Vector2Int pos, HexGridManager manager)
    {
        rt = GetComponent<RectTransform>();
        gridPosition = pos;
        gridManager = manager;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        gridManager.SelectTile(rt);  //타일 클릭 시 그리드 매니저로 전달
    }
}
using UnityEngine;
using UnityEngine.UI;

public class HexGridManager : MonoBehaviour
{
    public GameObject hexPrefab;        // 육각형 타일 프리팹
    public GameObject playerUnit;       // 이동할 유닛 (UI 이미지로 표시)
    public RectTransform gridContainer; // 그리드를 배치할 UI 컨테이너 (Canvas 내)

    public int gridWidth;           // 그리드 가로 크기
    public int gridHeight;          // 그리드 세로 크기

    private float hexWidth;      // 육각형 타일의 가로 크기 (UI 단위)
    private float hexHeight;    // 육각형 타일의 세로 크기 (UI 단위)

    private void Awake()
    {
        RectTransform hexRect = hexPrefab.GetComponent<RectTransform>();
        hexWidth = hexRect.rect.width;
        hexHeight = hexRect.rect.height;
    }

    private void Start()
    {
        GenerateHexGrid();
    }

    /// <summary>
    /// UI에서 육각형 그리드 생성
    /// </summary>
    private void GenerateHexGrid()
    {
        // 그리드 타일 배치
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                // 홀수 행에 오프셋 적용 (육각형 그리드에 맞게)
                float xOffset = (y % 2 == 0) ? 0 : hexWidth * 0.5f;

                // RectTransform 위치 계산
                Vector3 position = new Vector3(x * hexWidth + xOffset, -y * hexHeight, 0);

                // 프리팹을 UI 공간에 생성
                GameObject hex = Instantiate(hexPrefab, position, Quaternion.identity, gridContainer);
                hex.GetComponent<RectTransform>().localPosition = position;

                // 필요에 따라 타일을 설정할 수 있습니다.
                // 예: HexTile tile = hex.GetComponent<HexTile>();
                // tile.Setup(new Vector2Int(x, y), this);
            }
        }
    }

    /// <summary>
    /// 유닛이 타일로 이동하도록 구현
    /// </summary>
    public void SelectTile(RectTransform tile)
    {
        if (playerUnit == null) return;  // 유닛이 없다면 리턴

        // 유닛 이동 (RectTransform의 위치를 사용)
        playerUnit.transform.position = tile.position;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class UIPopupBase : MonoBehaviour
{
    public virtual void Open() => gameObject.SetActive(true);
    public virtual void Close() => gameObject.SetActive(false);
}

public class UIManager : Singleton<UIManager>
{
    [Header("필수 레퍼런스")]
    [SerializeField] private Canvas mainCanvas;  //UI용 최상위 캔버스

    readonly Dictionary<string, GameObject> prefabCache = new();  //로드한 프리팹
    readonly Dictionary<string, UIPopupBase> activePopups = new();  //활성화된 팝업

    protected override void Awake()
    {
        base.Awake();

        if (mainCanvas == null) mainCanvas = FindObjectOfType<Canvas>();
    }

    /// <summary>
    /// 팝업 열기
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="popupName"></param>
    /// <returns></returns>
    public T ShowPopup<T>(string popupName) where T : UIPopupBase
    {
        CloseAll();

        //이미 활성화되어 있으면 재활성화
        if (activePopups.TryGetValue(popupName, out UIPopupBase cached)) {
            cached.Open();
            return cached as T;
        }

        //없으면 새로 생성
        T instance = InstantiatePopup<T>(popupName);
        if (instance == null) return null;

        activePopups.Add(popupName, instance);
        instance.Open();
        return instance;
    }

    /// <summary>
    /// 특정 팝업 닫기
    /// </summary>
    /// <param name="popupName"></param>
    public void ClosePopup(string popupName)
    {
        if (activePopups.TryGetValue(popupName, out UIPopupBase popup))
            popup.Close();
    }

    /// <summary>
    /// 모든 팝업 닫기
    /// </summary>
    public void CloseAll()
    {
        foreach (var kv in activePopups) kv.Value.Close();
    }

    /// <summary>
    /// 팝업이 현재 열려 있는지 확인
    /// </summary>
    /// <param name="popupName"></param>
    /// <returns></returns>
    public bool IsOpen(string popupName) => activePopups.TryGetValue(popupName, out var p) && p.gameObject.activeSelf;

    #region 내부 로드 / 인스턴스화
    private T InstantiatePopup<T>(string popupName) where T : UIPopupBase
    {
        if (!prefabCache.TryGetValue(popupName, out GameObject prefab))
        {
            prefab = Resources.Load<GameObject>($"Prefabs/Popup/{popupName}");
            if (prefab == null)
            {
                Debug.LogError($"UIManager '{popupName}'프리팹 로드 실패");
                return null;
            }
            prefabCache.Add(popupName, prefab);
        }

        GameObject go = Instantiate(prefab, mainCanvas.transform);
        return go.GetComponent<T>();
    }
    #endregion
}
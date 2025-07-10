using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class UIPopupBase : MonoBehaviour
{
    private bool isFirstOpen = true;

    protected abstract void ResetUI();

    public virtual void Open()
    {
        //두 번째 이후 오픈부터만 리셋
        if (!isFirstOpen) ResetUI();
        //최초 오픈만 건너뜀
        else isFirstOpen = false;  
        gameObject.SetActive(true);
    }

    public virtual void Close() => gameObject.SetActive(false);
}

public class UIManager : Singleton<UIManager>
{
    private Canvas mainCanvas;  //UI용 최상위 캔버스
    private readonly Dictionary<string, GameObject> prefabCache = new();  //로드한 프리팹
    private readonly Dictionary<string, UIPopupBase> activePopups = new();  //활성화된 팝업

    protected override void Awake()
    {
        base.Awake();

        if (mainCanvas == null) mainCanvas = FindObjectOfType<Canvas>();

        LoadAllPopups();
    }

    /// <summary>
    /// 모든 팝업 로드
    /// </summary>
    private void LoadAllPopups()
    {
        //여기에 필요한 팝업 이름 추가
        string[] popupNames = {
            "UIEditorDeckPhase1",
            "UIEditorDeckPhase2",
            "UIEditorDeckPhase3",
            "UIModalPopup",
            "UISkillInfoPopup",
            "UIFilterPopup",
            "UILoginPopup"
        };

        foreach (string name in popupNames)
        {
            if (!prefabCache.TryGetValue(name, out GameObject prefab))
            {
                prefab = Resources.Load<GameObject>($"Prefabs/Popup/{name}");
                if (prefab == null)
                {
                    Debug.LogError($"UIManager '{name}' 프리팹 로드 실패");
                    continue;
                }
                prefabCache.Add(name, prefab);
            }

            GameObject go = Instantiate(prefab, mainCanvas.transform);
            go.SetActive(false);

            if (go.TryGetComponent<UIPopupBase>(out var popup)) activePopups[name] = popup;
        }
    }

    /// <summary>
    /// 팝업 열기
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="popupName"></param>
    /// <param name="isCloseAll"></param>
    /// <returns></returns>
    public T ShowPopup<T>(string popupName, bool isCloseAll = true) where T : UIPopupBase
    {
        if (isCloseAll) CloseAll();

        //활성화
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
        if (activePopups.TryGetValue(popupName, out UIPopupBase popup)) popup.Close();
    }

    /// <summary>
    /// 모든 팝업 닫기
    /// </summary>
    public void CloseAll()
    {
        foreach (var kv in activePopups) kv.Value.Close();
    }

    /// <summary>
    /// 팝업 가져오기
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="popupName"></param>
    /// <returns></returns>
    public T GetPopup<T>(string popupName) where T : UIPopupBase
    {
        if (activePopups.TryGetValue(popupName, out UIPopupBase popup)) return popup as T;
        return null;
    }

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
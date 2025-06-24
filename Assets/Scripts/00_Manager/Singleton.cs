using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;
    private static object lockObj = new object();

    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                lock (lockObj)
                {
                    instance = FindObjectOfType<T>();

                    if (instance == null)
                    {
                        GameObject singletonObject = new GameObject(typeof(T).Name);
                        instance = singletonObject.AddComponent<T>();
                        DontDestroyOnLoad(singletonObject);
                    }
                }
            }
            return instance;
        }
    }

    protected virtual void Awake()
    {
        //가로 모드로 변경 및 고정
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;
        Screen.autorotateToLandscapeRight = false;  //고정

        if (instance == null) {
            instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this) {
            //중복 인스턴스 제거
            Destroy(gameObject); 
        }
    }
}

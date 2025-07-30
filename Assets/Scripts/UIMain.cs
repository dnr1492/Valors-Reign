using Cysharp.Threading.Tasks;
using System.Collections;
using UnityEngine;

public class UIMain : MonoBehaviour
{
    private void Start()
    {
        InitApp().Forget();
    }

    private async UniTaskVoid InitApp()
    {
        //1. UILoginPopup 프리팹이 반드시 Instantiate되도록 ShowPopup 호출
        UIManager.Instance.ShowPopup<UILoginPopup>("UILoginPopup", false);
        UILoginPopup loginPopup = UIManager.Instance.GetPopup<UILoginPopup>("UILoginPopup");

        //2. 서버 연결 시도
        await loginPopup.ConnectNetwork();

        //3. 자동 로그인 시도
        if (await loginPopup.AttemptAutoLogin()) UIManager.Instance.ShowPopup<UILobbyPopup>("UILobbyPopup");
        else UIManager.Instance.ShowPopup<UILoginPopup>("UILoginPopup");
    }
}
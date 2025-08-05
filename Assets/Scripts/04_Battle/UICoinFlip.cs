using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UICoinFlip : MonoBehaviour
{
    [SerializeField] Button btn_front, btn_back;
    [SerializeField] Image icon_coin;
    [SerializeField] Sprite spriteFront, spriteBack;

    private Action<int> onCoinSelect;

    public void Init(Action<int> onSelectCoinCallback)
    {
        gameObject.SetActive(true);

        SetCoinButtonsActive(true);

        onCoinSelect = onSelectCoinCallback;

        btn_front.onClick.AddListener(() => OnSelect(0));
        btn_back.onClick.AddListener(() => OnSelect(1));
    }

    private void OnSelect(int selected)
    {
        SetCoinButtonsActive(false);

        onCoinSelect?.Invoke(selected); 
    }

    private void SetCoinButtonsActive(bool isActive)
    {
        btn_front.gameObject.SetActive(isActive);
        btn_back.gameObject.SetActive(isActive);
    }

    public void PlayFlipAnimation(int result, Action onComplete)
    {
        float duration = 2f;
        float jumpHeight = 200f;  //던지는 높이

        icon_coin.transform.localPosition = Vector3.zero;
        icon_coin.transform.localRotation = Quaternion.identity;

        Sequence seq = DOTween.Sequence();
        seq.Append(icon_coin.transform.DOLocalMoveY(jumpHeight, duration / 2f).SetEase(Ease.OutQuad));
        seq.Append(icon_coin.transform.DOLocalMoveY(0, duration / 2f).SetEase(Ease.InQuad));

        UniTask.Void(async () => {
            await RotateAndFlipSprite(duration, result, onComplete);
        });
    }

    private async UniTask RotateAndFlipSprite(float totalDuration, int result, Action onComplete)
    {
        float elapsed = 0f;
        float startSpeed = 1000f;  //초기 회전 속도 (deg/sec)
        float endSpeed = 360f;  //최종 회전 속도 (deg/sec)
        float currentRotation = 0f;

        bool showFront = true;
        float lastFlipped = 0f;

        while (elapsed < totalDuration)
        {
            float t = elapsed / totalDuration;

            //감속: 처음엔 빠르고 점점 느려짐
            float rotationSpeed = Mathf.Lerp(startSpeed, endSpeed, t);
            float deltaRotation = rotationSpeed * Time.deltaTime;
            currentRotation += deltaRotation;

            //실제 회전 적용
            icon_coin.transform.localRotation = Quaternion.Euler(currentRotation, 0f, 0f);

            //180도마다 앞/뒤 전환
            if (Mathf.Floor((currentRotation - lastFlipped) / 180f) >= 1f) {
                showFront = !showFront;
                icon_coin.sprite = showFront ? spriteFront : spriteBack;
                lastFlipped = Mathf.Floor(currentRotation / 180f) * 180f;
            }

            await UniTask.Yield();
            elapsed += Time.deltaTime;
        }

        //결과 고정
        SetCoinFlipResultImage(result);

        //회전도 스프라이트에 맞게 정면 값으로 고정
        float finalX = (result == 0) ? 0f : 180f;
        icon_coin.transform.localRotation = Quaternion.Euler(finalX, 0f, 0f);

        onComplete?.Invoke();
        Destroy(gameObject, 2f);
    }

    private void SetCoinFlipResultImage(int result)
    {
        icon_coin.sprite = (result == 0) ? spriteFront : spriteBack;
    }
}

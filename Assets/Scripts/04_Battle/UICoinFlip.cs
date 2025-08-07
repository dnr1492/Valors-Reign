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
    [SerializeField] Button btn_first, btn_second;
    [SerializeField] Image icon_coin;
    [SerializeField] Sprite spriteFront, spriteBack;

    private Action<int> onCoinDirectionSelect;

    public void Init(Action<int> onSelectCoinDirectionCallback)
    {
        gameObject.SetActive(true);

        ActiveCoinDirectionButtons(true);
        ActiveTurnChoiceButton(false);

        onCoinDirectionSelect = onSelectCoinDirectionCallback;

        btn_front.onClick.AddListener(() => OnSelectCoinDirection(0));
        btn_back.onClick.AddListener(() => OnSelectCoinDirection(1));

        btn_first.onClick.AddListener(() => OnSelectTurnOrder(true));
        btn_second.onClick.AddListener(() => OnSelectTurnOrder(false));
    }

    private void OnSelectCoinDirection(int selected)
    {
        ActiveCoinDirectionButtons(false);

        onCoinDirectionSelect?.Invoke(selected); 
    }

    private void OnSelectTurnOrder(bool wantFirst)
    {
        ActiveTurnChoiceButton(false);
        CancelInvoke(nameof(AutoSelectTurnOrder));

        ControllerRegister.Get<PhotonController>().SendTurnOrderChoice(wantFirst);
    }

    private void ActiveCoinDirectionButtons(bool isActive)
    {
        btn_front.gameObject.SetActive(isActive);
        btn_back.gameObject.SetActive(isActive);
    }

    public void ActiveTurnChoiceButton(bool isActive)
    {
        btn_first.gameObject.SetActive(isActive);
        btn_second.gameObject.SetActive(isActive);
    }

    #region 동전 던지기 (feat.애니메이션)
    public void PlayFlipAnimation(int result, Action onComplete)
    {
        float duration = 2f;
        float jumpHeight = 200f;

        icon_coin.transform.localPosition = Vector3.zero;
        icon_coin.transform.localRotation = Quaternion.identity;
        icon_coin.transform.localScale = Vector3.one;

        //점프 애니메이션
        Sequence seq = DOTween.Sequence();
        seq.Append(icon_coin.transform.DOLocalMoveY(jumpHeight, duration / 2f).SetEase(Ease.OutQuad));
        seq.Append(icon_coin.transform.DOLocalMoveY(0, duration / 2f).SetEase(Ease.InQuad));

        //회전 애니메이션 실행
        SimulateCoinSpin(duration, result, onComplete).Forget();
    }

    private async UniTaskVoid SimulateCoinSpin(float duration, int result, Action onComplete)
    {
        float elapsed = 0f;
        float currentRotation = 0f;

        //회전 설정
        int fullFlips = 6;
        bool resultIsFront = result == 0;
        if (!resultIsFront) fullFlips += 1;

        float totalRotation = fullFlips * 180f;
        float startRotation = 0f;
        bool currentIsFront = true;

        //초기 상태 설정
        icon_coin.transform.localRotation = Quaternion.Euler(startRotation, 0f, 0f);
        icon_coin.sprite = spriteFront;

        //회전 루프
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float interpolatedRot = Mathf.Lerp(0, totalRotation, t);
            currentRotation = startRotation + interpolatedRot;

            icon_coin.transform.localRotation = Quaternion.Euler(currentRotation, 0f, 0f);

            int flipIndex = Mathf.FloorToInt(currentRotation / 180f);
            bool isFront = flipIndex % 2 == 0;

            if (isFront != currentIsFront)
            {
                currentIsFront = isFront;
                icon_coin.sprite = currentIsFront ? spriteFront : spriteBack;
            }

            await UniTask.Yield();
            elapsed += Time.deltaTime;
        }

        //최종 회전 및 결과 정리
        icon_coin.transform.localRotation = Quaternion.Euler(startRotation + totalRotation, 0f, 0f);
        icon_coin.sprite = resultIsFront ? spriteFront : spriteBack;
        icon_coin.transform.localScale = new Vector3(1f, resultIsFront ? 1f : -1f, 1f);

        await UniTask.Yield();

        onComplete?.Invoke();
    }
    #endregion

    #region 선공 또는 후공 자동 선택
    public void AutoSelectTurnOrder()
    {
        //이미 턴이 시작됐으면 패스 (상대가 선택 완료한 상태)
        if (TurnManager.Instance.TurnIndex > 0) {
            return;
        }

        bool randomChoice = UnityEngine.Random.value < 0.5f;
        ControllerRegister.Get<PhotonController>().SendTurnOrderChoice(randomChoice, true);
    }
    #endregion
}

using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TurnManager : Singleton<TurnManager>
{
    private int trnIndex = 0;
    private int roundIndex = 0;
    private bool isMyRound;

    public int TurnIndex => trnIndex;

    protected override void Awake()
    {
        base.Awake();
    }

    #region 매치 시작
    public void StartMatch(bool iAmFirst)
    {
        trnIndex = 1;
        roundIndex = 0;
        this.isMyRound = iAmFirst;

        var photon = ControllerRegister.Get<PhotonController>();
        CardManager.Instance.InitDeckFromDeckPack(photon.MyDeckPack);             //덱 초기화
        CombatManager.Instance.InitCharacterInfoFromDeckPack(photon.MyDeckPack);  //캐릭터 정보 초기화

        var movementOrderCtrl = ControllerRegister.Get<MovementOrderController>();
        if (movementOrderCtrl == null)
        {
            var go = new GameObject("MovementOrderController");
            go.AddComponent<MovementOrderController>();
            DontDestroyOnLoad(go);
        }

        ProceedToNextTurn();
    }
    #endregion

    // ================================ 구현 중 ================================ //
    // ================================ 구현 중 ================================ //
    // ================================ 구현 중 ================================ //

    private void ProceedToNextTurn()
    {
        Debug.Log($"[턴 {trnIndex} 시작]");

        //1. 생존 캐릭터 수 계산
        int aliveCharacterCount = CombatManager.Instance.GetAliveCharacterCount();

        //2. 카드 드로우
        CardManager.Instance.DrawSkillCards(aliveCharacterCount * 2);   //생존 캐릭터 수 × 2 장 드로우 + 기본 이동카드 1 장

        //3. 드로우한 스킬카드를 표시
        UIManager.Instance.GetPopup<UIBattleSetting>("UIBattleSetting")
            .SetDrawnSkillCard(CardManager.Instance.GetDrawnSkillCards());
    }

    #region 라운드 진행 (라운드 증가 → 종료 검사 → 라운드 소유권 설정 → 이동/스킬카드 실행)
    public async void StartRound()
    {
        roundIndex++;

        //최대 라운드 초과 시 다음 턴으로 전환
        if (roundIndex > 4) {
            EndRound();
            return;
        }

        //현재 라운드가 내 라운드인지 판정
        isMyRound = IsMyRound(roundIndex);

        // ==================== 여기서 스킬카드 실행 로직 들어감 ==================== //

        //이동 오더 재검증 + 순차 실행
        var movementOrderCtrl = ControllerRegister.Get<MovementOrderController>();
        if (movementOrderCtrl != null)
        {
            //라운드 시작 직전 전체 재검증 (fromHexPos 기준)
            bool ok = movementOrderCtrl.ValidateAllBeforeRound(); 

            //순서대로 실행(1 → 4). 완료 후 후속 처리(다음 스킬 등)는 콜백에서 이어가기.
            await UniTask.Create(async () => {
                bool done = false;
                movementOrderCtrl.ExecuteInOrder(() => {
                    Debug.Log("[Turn] 이동 실행 완료");
                    // ===== TODO: 이동 외 다른 스킬 실행/라운드 진행을 여기에 이어 붙이세요. ===== //
                    done = true;
                });

                //완료까지 프레임 대기
                while (!done) await UniTask.Yield(PlayerLoopTiming.Update);  
            });
        }
    }
    #endregion

    private bool IsMyRound(int round) => (round % 2 == 1) ? isMyRound : !isMyRound;

    private void EndRound()
    {
        Debug.Log($"[턴 {trnIndex} 종료]");

        trnIndex++;
        roundIndex = 0;

        ProceedToNextTurn();
    }
}

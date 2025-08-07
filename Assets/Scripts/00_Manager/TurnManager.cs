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

    public void StartTurn(bool isMyRound)
    {
        trnIndex = 1;
        roundIndex = 0;
        this.isMyRound = isMyRound;

        var photon = ControllerRegister.Get<PhotonController>();
        CardManager.Instance.InitDeckFromDeckPack(photon.MyDeckPack);
        CombatManager.Instance.InitCharacterInfoFromDeckPack(photon.MyDeckPack);

        ProceedToNextTurn();
    }

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
            .DisplayDrawnSkillCard(CardManager.Instance.GetDrawnSkillCards());

        ////4. OnSkillCardSettingComplete

        ////5. StartNextRound()로 진입
    }

    public void OnSkillCardSettingComplete(List<UISkillCard> selected)
    {
        Debug.Log($"[세팅 완료] → 라운드 실행 시작");
        StartNextRound();
    }

    private void StartNextRound()
    {
        roundIndex++;

        if (roundIndex > 4) {
            EndTurn();  //다음 턴으로 전환
            return;
        }

        isMyRound = IsMyRound(roundIndex);

        // =====  여기서 스킬카드 실행 로직 들어감 ===== //
    }

    private bool IsMyRound(int round) => (round % 2 == 1) ? isMyRound : !isMyRound;

    private void EndTurn()
    {
        Debug.Log($"[턴 {trnIndex} 종료]");

        trnIndex++;
        roundIndex = 0;

        ProceedToNextTurn();
    }
}

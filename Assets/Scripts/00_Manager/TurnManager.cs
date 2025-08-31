using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TurnManager : Singleton<TurnManager>
{
    private int trnIndex = 0;
    private int roundIndex = 0;
    private bool iAmFirst;  //선공여부
    private bool myExecutedThisRound = false;  //이번 라운드에서 내가 실행했는지
    private bool matchEnded = false;  //매치 종료 플래그
    private bool myReady = false;  //나의 준비 상태
    private bool oppReady = false;  //상대 준비 상태
    private bool roundsStarted = false;  //중복 트리거 방지
    private UIBattleProgress progressUI;
    
    public int TurnIndex => trnIndex;
    public bool IsPvP => ControllerRegister.Get<PhotonController>().IsPvPMatch == true;  //PvP 여부
    public bool IsLocalReady => myReady;  //외부(UI)에서 내 준비 상태 조회용

    protected override void Awake()
    {
        base.Awake();
    }

    #region 매치 시작
    public void StartMatch(bool iAmFirst)
    {
        trnIndex = 1;
        roundIndex = 0;
        this.iAmFirst = iAmFirst;

        ResetReadyBarrier();

        var photon = ControllerRegister.Get<PhotonController>();
        CardManager.Instance.InitDeckFromDeckPack(photon.MyDeckPack);  //덱 초기화
        CombatManager.Instance.InitCharacterInfoFromDeckPack(photon.MyDeckPack);   //캐릭터 정보 초기화

        var movementOrderCtrl = ControllerRegister.Get<MovementOrderController>();
        if (movementOrderCtrl == null)
        {
            var go = new GameObject("MovementOrderController");
            go.AddComponent<MovementOrderController>();
            DontDestroyOnLoad(go);
        }

        ProceedToNextTurn();

        //타이머 시작
        UIManager.Instance.GetPopup<UIBattleSetting>("UIBattleSetting").BeginSetupPhase();
    }
    #endregion

    private void ProceedToNextTurn()
    {
        Debug.Log($"[턴 {trnIndex} 시작]");

        //1) 생존 캐릭터 수/키 수집
        int aliveCharacterCount = CombatManager.Instance.GetAliveCharacterCount();
        var aliveKeys = CombatManager.Instance.GetMyAliveTokenIds();

        //2) 생존 캐릭터의 스킬카드로만 덱 재구성 + 드로우 (내부에서 이동카드 1000 포함)
        //생존 캐릭터 수 × 2 장 드로우 + 기본 이동카드 1 장
        CardManager.Instance.DrawSkillCardsForAliveOwners(aliveKeys, aliveCharacterCount * 2);

        //3) UI 반영 - 드로우한 스킬카드를 표시, 턴 표시
        UIManager.Instance.GetPopup<UIBattleSetting>("UIBattleSetting").SetDrawnSkillCard(CardManager.Instance.GetDrawnSkillCards());
        UIManager.Instance.GetPopup<UIBattleSetting>("UIBattleSetting").DisplayTurn(trnIndex);
    }

    #region 라운드 진행 (동일한 라운드에서 선공 → 후공 순으로 실행)
    public async void StartRound()
    {
        if (matchEnded) return;

        roundIndex++;

        //최대 라운드 초과 시 다음 턴으로 전환
        if (roundIndex > 4) {
            EndRound();
            return;
        }

        //진행 라운드 갱신
        progressUI.DisplayRound(roundIndex);

        myExecutedThisRound = false;

        //누가 선공인지(iAmFirst) 기준으로 먼저 움직일 쪽을 결정해서 양쪽 카드 순차 이동
        await progressUI.AnimateRoundIntroAsync(roundIndex, iAmFirst);

        if (IsPvP)
        {
            if (iAmFirst)
            {
                await ExecuteMyRoundAsync();

                //상대에 N 라운드 끝남 알림
                //여기서 대기 → 상대가 해당 roundIndex를 마치고 알림 보내면 다음 라운드로 이동
                var photon = ControllerRegister.Get<PhotonController>();
                photon.NotifyRoundFinished(trnIndex, roundIndex);
            }
            else
            {
                //상대 알림 수신 시 나의 실행 → 알림 → 다음 라운드로 진행됨
                Debug.Log($"[Round] 상대가 선공이므로 round {roundIndex} 실행 대기");
            }
        }
        else
        {
            if (iAmFirst)
            {
                //내 쪽 먼저 연출 → 내 라운드 실행
                await ExecuteMyRoundAsync();

                //그 다음 상대 쪽 연출 → AI 라운드 실행
                await ExecuteOpponentRoundAIAsync();
            }
            else
            {
                //상대 쪽 먼저 연출 → AI 라운드 실행
                await ExecuteOpponentRoundAIAsync();

                //그 다음 내 쪽 연출 → 내 라운드 실행
                await ExecuteMyRoundAsync();
            }

            StartRound();
        }
    }

    //상대 라운드 종료 수신: 선공/후공에 따라 분기
    public void OnOpponentRoundFinished(int trn, int round)
    {
        trnIndex = trn;
        roundIndex = round;

        if (!IsPvP || matchEnded) return;

        if (iAmFirst)
        {
            //나는 이미 이 라운드 실행함 → 상대도 방금 끝냄 → 다음 라운드로
            if (myExecutedThisRound) StartRound();
            else Debug.Log("[Round] 선공인데 내 실행 플래그가 비정상 상태");
        }
        else
        {
            //후공: 이제 내 차례 (같은 라운드에서 나의 실행) → 알림 → 다음 라운드
            if (!myExecutedThisRound)
            {
                UniTask.Void(async () =>
                {
                    await ExecuteMyRoundAsync();
                    myExecutedThisRound = true;
                    ControllerRegister.Get<PhotonController>().NotifyRoundFinished(trnIndex, roundIndex);
                    StartRound();
                });
            }
        }
    }

    //나의 라운드 진행
    private async UniTask ExecuteMyRoundAsync()
    {
        Debug.Log($"[Round] 내 {roundIndex} 라운드");

        var move = ControllerRegister.Get<MovementOrderController>();
        if (move == null) return;

        //현재 라운드만 재검증
        move.ValidateAllBeforeRound(roundIndex);

        //오더 없으면 스킵
        if (!move.HasOrderForRound(roundIndex))
        {
            Debug.Log($"[Round] {roundIndex} 라운드: 실행할 이동 없음");
            return;
        }

        bool done = false;
        //현재 라운드 1개만 실행
        move.ExecuteForRound(roundIndex, () => done = true);

        while (!done) await UniTask.Yield(PlayerLoopTiming.Update);
    }

    //상대 라운드 처리 - AI
    private async UniTask ExecuteOpponentRoundAIAsync()
    {
        // ===== TODO: AI 행동 로직 구현 필요 ===== //
        Debug.Log($"[Round] 상대 {roundIndex} 라운드 자동 처리");
        await UniTask.Delay(1000);
    }

    private void EndRound()
    {
        Debug.Log($"[턴 {trnIndex} 종료]");

        trnIndex++;
        roundIndex = 0;

        ResetReadyBarrier();

        ProceedToNextTurn();

        //타이머 재시작
        UIManager.Instance.ShowPopup<UIBattleSetting>("UIBattleSetting").BeginSetupPhase();
    }
    #endregion

    #region 대전 셋팅 준비
    //내가 준비완료
    public void ReadyLocal(bool byTimeout)
    {
        if (myReady) return;
        myReady = true;

        UIManager.Instance.GetPopup<UIBattleSetting>("UIBattleSetting").SetReadyState(true);

        var photon = ControllerRegister.Get<PhotonController>();
        if (IsPvP) photon.NotifyPlayerReady(trnIndex, byTimeout ? 1 : 0);
        //AI 매치: 내가 준비되면 즉시 상대도 준비된 것으로 간주
        else oppReady = true;

        TryStartRoundsIfBothReady();
    }

    //상대 준비완료 수신
    public void OnOpponentReady(int trn, int readyType)
    {
        //턴 동기화
        trnIndex = trn;
        oppReady = true;

        TryStartRoundsIfBothReady();
    }

    //둘 다 준비완료했을 때 선공/후공 기준으로 라운드 시작
    private void TryStartRoundsIfBothReady()
    {
        if (roundsStarted) return;

        if (IsPvP) {
            if (!myReady || !oppReady) return;
        }
        else {
            if (!myReady) return;
        }

        roundsStarted = true;

        var setting = UIManager.Instance.GetPopup<UIBattleSetting>("UIBattleSetting");
        setting.OnBothReady();

        progressUI = UIManager.Instance.ShowPopup<UIBattleProgress>("UIBattleProgress", false);

        //유저
        if (IsPvP)
        {
            progressUI.DisplayMyRoundCards(setting);

            if (iAmFirst) StartRound();
            else Debug.Log("[Turn] 상대가 선공이므로 대기");
        }
        //AI
        else
        {
            progressUI.DisplayBoth(setting, null, IsPvP);

            StartRound();
        }
    }

    //준비 배리어 초기화
    private void ResetReadyBarrier()
    {
        myReady = false;
        oppReady = false;
        roundsStarted = false;
    }
    #endregion

    //상대가 나가서 승리한 경우
    public void OnOpponentLeftAndWin()
    {
        if (matchEnded) return;
        matchEnded = true;

        Debug.Log("[Match] 상대 이탈로 승리");
        // ===== TODO: 원하는 연출/결과창 표시, 보상/통계 처리 등 추가 ===== //
    }
}
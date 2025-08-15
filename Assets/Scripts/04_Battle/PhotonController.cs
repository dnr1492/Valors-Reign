using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using Cysharp.Threading.Tasks;
using static EnumClass;

public class PhotonController : MonoBehaviourPunCallbacks
{
    public static bool IsConnected => PhotonNetwork.IsConnectedAndReady;
    private const string ROOM_NAME_PREFIX = "Room_";

    private bool pendingJoinRequest = false;
    private bool isMyDeckSent = false;
    private bool isOpponentDeckReceived = false;
    private bool hasFirstTurnChoice = false;  //내가 선공 선택권을 가졌는지

    private DeckPack myDeckPack;
    public DeckPack MyDeckPack { get => myDeckPack; set => myDeckPack = value; }

    private DeckPack opponentDeckPack;
    public DeckPack OpponentDeckPack { get => opponentDeckPack; }

    public bool IsPvPMatch { get; private set; }  //매치 시작 시 확정되는 PvP 여부 플래그

    private void Awake()
    {
        ControllerRegister.Register(this);
    }

    public override void OnEnable() => PhotonNetwork.AddCallbackTarget(this);

    public override void OnDisable() => PhotonNetwork.RemoveCallbackTarget(this);

    /// <summary>
    /// Photon 서버 연결 여부를 판단해서 즉시 매칭하거나, 연결 후 자동 매칭 예약
    /// </summary>
    public void RequestJoinRoomAfterConnection()
    {
        if (PhotonNetwork.IsConnectedAndReady) {
            Debug.Log("Photon 연결 완료 → 즉시 룸 입장");
            JoinOrCreateRoom();
        }
        else {
            Debug.Log("Photon 아직 연결 안 됨 → 매칭 예약 후 연결 시도");
            pendingJoinRequest = true;
            ConnectToPhoton();
        }
    }

    #region Photon 서버에 연결
    private void ConnectToPhoton()
    {
        if (!PhotonNetwork.IsConnected)
        {
            Debug.Log("Photon 서버 연결 시도...");
            PhotonNetwork.ConnectUsingSettings();
        }
        else Debug.Log("이미 Photon에 연결됨");
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Photon 서버 연결 성공");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("로비 입장 완료");

        if (pendingJoinRequest) {
            Debug.Log("예약된 매칭 요청 → JoinOrCreateRoom 호출");
            pendingJoinRequest = false;
            JoinOrCreateRoom();
        }
    }
    #endregion

    #region 룸 생성 또는 참가
    public void JoinOrCreateRoom()
    {
        string roomName = ROOM_NAME_PREFIX + Random.Range(1000, 9999);
        RoomOptions options = new RoomOptions
        {
            MaxPlayers = 2,
            IsVisible = true,
            IsOpen = true
        };

        PhotonNetwork.JoinOrCreateRoom(roomName, options, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"룸 입장 성공! 현재 인원: {PhotonNetwork.CurrentRoom.PlayerCount}");
        IsPvPMatch = PhotonNetwork.CurrentRoom.PlayerCount == 2;

        if (PhotonNetwork.CurrentRoom.PlayerCount == 2) {
            Debug.Log("실제 상대 입장 → 나의 덱 전송");
            StartDeckExchange(UIManager.Instance.GetPopup<UIEditorDeckPhase1>("UIEditorDeckPhase1").GetSelectedDeckPack());
        }
        else {
            Debug.Log("상대 대기 중... 3초 후 AI 대전 전환");
            WaitForOpponentThenStartAI().Forget();
        }
    }
    #endregion

    #region [AI 대전] AI 덱으로 변경
    private async UniTaskVoid WaitForOpponentThenStartAI()
    {
        await UniTask.Delay(3000);

        if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            Debug.Log("상대 없음 → AI 대전 시작");
            isMyDeckSent = true;

            opponentDeckPack = AIBattleHelper.GetRandomDeckAI();
            OnOpponentDeckReceived();
        }
    }
    #endregion

    #region [유저 대전] 전투 준비 단계
    public void StartDeckExchange(DeckPack deckPack)
    {
        string json = JsonUtility.ToJson(deckPack);
        object[] content = { json };

        RaiseEventOptions options = new RaiseEventOptions {
            Receivers = ReceiverGroup.Others
        };

        PhotonNetwork.RaiseEvent((byte)PhotonEventCode.SendDeck, content, options, SendOptions.SendReliable);

        isMyDeckSent = true;
        TryStartBattleIfReady();
    }

    public void OnEvent(EventData photonEvent)
    {
        HandlePhotonEvent(photonEvent);
    }

    private void HandlePhotonEvent(EventData photonEvent)
    {
        //덱 동기화
        if (photonEvent.Code == (byte)PhotonEventCode.SendDeck)
        {
            object[] data = (object[])photonEvent.CustomData;
            string json = (string)data[0];

            opponentDeckPack = JsonUtility.FromJson<DeckPack>(json);
            OnOpponentDeckReceived();
        }
        //동전 던지기 - 선공 선택권
        else if (photonEvent.Code == (byte)PhotonEventCode.CoinFlip)
        {
            object[] data = (object[])photonEvent.CustomData;
            int result = (int)data[0];
            int selected = (int)data[1];

            HandleCoinFlipResult(result, selected);
        }
        //선공 or 후공 결정
        else if (photonEvent.Code == (byte)PhotonEventCode.SendTurnOrderChoice)
        {
            object[] data = (object[])photonEvent.CustomData;
            bool selectedFirst = (bool)data[0];

            //내가 선택권이 없을때 동기화
            TryFinalizeTurnOrder(selectedFirst);
        }
        //상대 라운드 종료 수신
        if (photonEvent.Code == (byte)PhotonEventCode.RoundFinished)
        {
            object[] data = (object[])photonEvent.CustomData;
            int trn = (int)data[0];
            int round = (int)data[1];

            //상대가 round 라운드를 마쳤다고 알림 → 내 클라이언트에서 즉시 다음 라운드 시작
            TurnManager.Instance.OnOpponentRoundFinished(trn, round);
            return;
        }
        //상대 준비완료/타임아웃 수신
        if (photonEvent.Code == (byte)PhotonEventCode.PlayerReady)
        {
            object[] data = (object[])photonEvent.CustomData;
            int trn = (int)data[0];
            int readyType = (int)data[1];  //0 = Manual, 1 = Timeout
            TurnManager.Instance.OnOpponentReady(trn, readyType);
            return;
        }
    }
    #endregion

    #region 대전 시작 조건 확인
    private void OnOpponentDeckReceived()
    {
        isOpponentDeckReceived = true;
        TryStartBattleIfReady();
    }

    private void TryStartBattleIfReady()
    {
        if (isMyDeckSent && isOpponentDeckReceived)
        {
            LoadingManager.Instance.Hide();

            Debug.Log("양쪽 덱 적용 완료 → 동전 던지기 진입");
            var popup = UIManager.Instance.ShowPopup<UIBattleSetting>("UIBattleSetting");
            popup.Init();
        }
    }
    #endregion

    #region (동전 던지기) 선공 선택권 결정
    public void RequestCoinFlip(int myCoinDriection)
    {
#if UNITY_EDITOR
        int coinDirectionResult = 0;
#else
        int coinDirectionResult = Random.Range(0, 2);  //0: 앞면, 1: 뒷면
#endif
        if (PhotonNetwork.IsMasterClient)
        {
            object[] content = { coinDirectionResult, myCoinDriection };
            PhotonNetwork.RaiseEvent((byte)PhotonEventCode.CoinFlip, content,
                new RaiseEventOptions { Receivers = ReceiverGroup.All },
                SendOptions.SendReliable);

            HandleCoinFlipResult(coinDirectionResult, myCoinDriection);
        }
    }

    private void HandleCoinFlipResult(int result, int myCoinDriection)
    {
        bool hasFirstTurnChoice = (result == myCoinDriection && PhotonNetwork.IsMasterClient) 
                        || (result != myCoinDriection && !PhotonNetwork.IsMasterClient);

        Debug.Log($"{(hasFirstTurnChoice ? "선공 선택권 획득" : "선택권 없음")}");

        this.hasFirstTurnChoice = hasFirstTurnChoice;

        var popup = UIManager.Instance.GetPopup<UIBattleSetting>("UIBattleSetting");
        popup.ShowCoinFlipResult(result, hasFirstTurnChoice);
    }
#endregion

    #region (동전 던지기) 선공 or 후공 선택
    public void SendTurnOrderChoice(bool wantFirst, bool force = false)
    {
        //선공 선택권 없는 유저가 보내는 경우 막기 (AI는 force = true로 통과시킴)
        if (!hasFirstTurnChoice && !force) return;

        //내가 선택권 있을 때
        object[] content = { wantFirst };
        PhotonNetwork.RaiseEvent((byte)PhotonEventCode.SendTurnOrderChoice, content,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            SendOptions.SendReliable);

        TryFinalizeTurnOrder(wantFirst);
    }

    private void TryFinalizeTurnOrder(bool selectedFirst)
    {
        bool iAmFirst = hasFirstTurnChoice
            ? selectedFirst
            : !selectedFirst;

        TurnManager.Instance.StartMatch(iAmFirst);

        LoadingManager.Instance.Hide();

        UIManager.Instance.GetPopup<UIBattleSetting>("UIBattleSetting")
            .DestroyUICoinFlip();
    }
    #endregion

    #region 룸 취소
    public void LeaveRoom()
    {
        if (PhotonNetwork.InRoom) {
            Debug.Log("룸에서 나갑니다...");
            PhotonNetwork.LeaveRoom();
        }
        else {
            Debug.Log("현재 룸에 있지 않음 → 매칭 예약만 취소");
            pendingJoinRequest = false;
        }
    }
    #endregion

    #region 룸 나가기
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        //부전승 처리 트리거
        Debug.Log($"상대 플레이어 퇴장: {otherPlayer.NickName} → 부전승 처리");
        TurnManager.Instance.OnOpponentLeftAndWin();
    }
    #endregion

    #region 네트워크 연결 끊김 ===== TODO: 필요시 재연결 UI, 일시정지 처리 등 추가 =====
    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log($"Photon 연결 끊김: {cause}");
        // ===== TODO: 필요시 재연결 UI, 일시정지 처리 등 추가 =====
    }
    #endregion

    #region 라운드 진행
    //해당 라운드 종료 알림
    public void NotifyRoundFinished(int trnIndex, int roundIndex)
    {
        if (!PhotonNetwork.InRoom) return;

        object[] content = { trnIndex, roundIndex };
        PhotonNetwork.RaiseEvent(
            (byte)PhotonEventCode.RoundFinished,
            content,
            new RaiseEventOptions { Receivers = ReceiverGroup.Others },
            SendOptions.SendReliable
        );
    }

    //대전 셋팅 준비완료 알림 (Manual = 0, Timeout = 1)
    public void NotifyPlayerReady(int trnIndex, int readyType)
    {
        if (!PhotonNetwork.InRoom) return;
        object[] content = { trnIndex, readyType };
        PhotonNetwork.RaiseEvent(
            (byte)PhotonEventCode.PlayerReady,
            content,
            new RaiseEventOptions { Receivers = ReceiverGroup.Others },
            SendOptions.SendReliable
        );
    }
    #endregion

    // ==================================================== 구현 중 =========================================================== //
    // ==================================================== 구현 중 =========================================================== //
    // ==================================================== 구현 중 =========================================================== //

    //public override void OnJoinRoomFailed(short returnCode, string message)
    //{
    //    Debug.Log($"룸 입장 실패: {message}");
    //}
}

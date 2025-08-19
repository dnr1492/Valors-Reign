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
    private bool hasFirstTurnChoice = false;  //���� ���� ���ñ��� ��������

    private DeckPack myDeckPack;
    public DeckPack MyDeckPack { get => myDeckPack; set => myDeckPack = value; }

    private DeckPack opponentDeckPack;
    public DeckPack OpponentDeckPack { get => opponentDeckPack; }

    public bool IsPvPMatch { get; private set; }  //��ġ ���� �� Ȯ���Ǵ� PvP ���� �÷���

    private void Awake()
    {
        ControllerRegister.Register(this);
    }

    public override void OnEnable() => PhotonNetwork.AddCallbackTarget(this);

    public override void OnDisable() => PhotonNetwork.RemoveCallbackTarget(this);

    /// <summary>
    /// Photon ���� ���� ���θ� �Ǵ��ؼ� ��� ��Ī�ϰų�, ���� �� �ڵ� ��Ī ����
    /// </summary>
    public void RequestJoinRoomAfterConnection()
    {
        if (PhotonNetwork.IsConnectedAndReady) {
            Debug.Log("Photon ���� �Ϸ� �� ��� �� ����");
            JoinOrCreateRoom();
        }
        else {
            Debug.Log("Photon ���� ���� �� �� �� ��Ī ���� �� ���� �õ�");
            pendingJoinRequest = true;
            ConnectToPhoton();
        }
    }

    #region Photon ������ ����
    private void ConnectToPhoton()
    {
        if (!PhotonNetwork.IsConnected)
        {
            Debug.Log("Photon ���� ���� �õ�...");
            PhotonNetwork.ConnectUsingSettings();
        }
        else Debug.Log("�̹� Photon�� �����");
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Photon ���� ���� ����");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("�κ� ���� �Ϸ�");

        if (pendingJoinRequest) {
            Debug.Log("����� ��Ī ��û �� JoinOrCreateRoom ȣ��");
            pendingJoinRequest = false;
            JoinOrCreateRoom();
        }
    }
    #endregion

    #region �� ���� �Ǵ� ����
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
        Debug.Log($"�� ���� ����! ���� �ο�: {PhotonNetwork.CurrentRoom.PlayerCount}");
        IsPvPMatch = PhotonNetwork.CurrentRoom.PlayerCount == 2;

        if (PhotonNetwork.CurrentRoom.PlayerCount == 2) {
            Debug.Log("���� ��� ���� �� ���� �� ����");
            StartDeckExchange(UIManager.Instance.GetPopup<UIEditorDeckPhase1>("UIEditorDeckPhase1").GetSelectedDeckPack());
        }
        else {
            Debug.Log("��� ��� ��... 3�� �� AI ���� ��ȯ");
            WaitForOpponentThenStartAI().Forget();
        }
    }
    #endregion

    #region [AI ����] AI ������ ����
    private async UniTaskVoid WaitForOpponentThenStartAI()
    {
        await UniTask.Delay(3000);

        if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            Debug.Log("��� ���� �� AI ���� ����");
            isMyDeckSent = true;

            opponentDeckPack = AIBattleHelper.GetRandomDeckAI();
            OnOpponentDeckReceived();
        }
    }
    #endregion

    #region [���� ����] ���� �غ� �ܰ�
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
        //�� ����ȭ
        if (photonEvent.Code == (byte)PhotonEventCode.SendDeck)
        {
            object[] data = (object[])photonEvent.CustomData;
            string json = (string)data[0];

            opponentDeckPack = JsonUtility.FromJson<DeckPack>(json);
            OnOpponentDeckReceived();
        }
        //���� ������ - ���� ���ñ�
        else if (photonEvent.Code == (byte)PhotonEventCode.CoinFlip)
        {
            object[] data = (object[])photonEvent.CustomData;
            int result = (int)data[0];
            int selected = (int)data[1];

            HandleCoinFlipResult(result, selected);
        }
        //���� or �İ� ����
        else if (photonEvent.Code == (byte)PhotonEventCode.SendTurnOrderChoice)
        {
            object[] data = (object[])photonEvent.CustomData;
            bool selectedFirst = (bool)data[0];

            //���� ���ñ��� ������ ����ȭ
            TryFinalizeTurnOrder(selectedFirst);
        }
        //��� ���� ���� ����
        else if (photonEvent.Code == (byte)PhotonEventCode.RoundFinished)
        {
            object[] data = (object[])photonEvent.CustomData;
            int trn = (int)data[0];
            int round = (int)data[1];

            //��밡 round ���带 ���ƴٰ� �˸� �� �� Ŭ���̾�Ʈ���� ��� ���� ���� ����
            TurnManager.Instance.OnOpponentRoundFinished(trn, round);
            return;
        }
        //��� �غ�Ϸ�/Ÿ�Ӿƿ� ����
        else if (photonEvent.Code == (byte)PhotonEventCode.PlayerReady)
        {
            object[] data = (object[])photonEvent.CustomData;
            int trn = (int)data[0];
            int readyType = (int)data[1];  //0 = Manual, 1 = Timeout
            TurnManager.Instance.OnOpponentReady(trn, readyType);
            return;
        }
        //��� ���� ��ȹ ����
        else if (photonEvent.Code == (byte)PhotonEventCode.SendRoundPlan)
        {
            object[] data = (object[])photonEvent.CustomData;
            string json = (string)data[0];
            var plan = JsonUtility.FromJson<OppRoundPlan>(json);
            TurnManager.Instance.SetOpponentRoundPlan(plan);
            return;
        }
    }
    #endregion

    #region ���� ���� ���� Ȯ��
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

            Debug.Log("���� �� ���� �Ϸ� �� ���� ������ ����");
            var popup = UIManager.Instance.ShowPopup<UIBattleSetting>("UIBattleSetting");
            popup.Init();
        }
    }
    #endregion

    #region (���� ������) ���� ���ñ� ����
    public void RequestCoinFlip(int myCoinDriection)
    {
#if UNITY_EDITOR
        int coinDirectionResult = 0;
#else
        int coinDirectionResult = Random.Range(0, 2);  //0: �ո�, 1: �޸�
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

        Debug.Log($"{(hasFirstTurnChoice ? "���� ���ñ� ȹ��" : "���ñ� ����")}");

        this.hasFirstTurnChoice = hasFirstTurnChoice;

        var popup = UIManager.Instance.GetPopup<UIBattleSetting>("UIBattleSetting");
        popup.ShowCoinFlipResult(result, hasFirstTurnChoice);
    }
#endregion

    #region (���� ������) ���� or �İ� ����
    public void SendTurnOrderChoice(bool wantFirst, bool force = false)
    {
        //���� ���ñ� ���� ������ ������ ��� ���� (AI�� force = true�� �����Ŵ)
        if (!hasFirstTurnChoice && !force) return;

        //���� ���ñ� ���� ��
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

    #region �� ���
    public void LeaveRoom()
    {
        if (PhotonNetwork.InRoom) {
            Debug.Log("�뿡�� �����ϴ�...");
            PhotonNetwork.LeaveRoom();
        }
        else {
            Debug.Log("���� �뿡 ���� ���� �� ��Ī ���ุ ���");
            pendingJoinRequest = false;
        }
    }
    #endregion

    #region �� ������
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        //������ ó�� Ʈ����
        Debug.Log($"��� �÷��̾� ����: {otherPlayer.NickName} �� ������ ó��");
        TurnManager.Instance.OnOpponentLeftAndWin();
    }
    #endregion

    #region ��Ʈ��ũ ���� ���� ===== TODO: �ʿ�� �翬�� UI, �Ͻ����� ó�� �� �߰� =====
    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log($"Photon ���� ����: {cause}");
        // ===== TODO: �ʿ�� �翬�� UI, �Ͻ����� ó�� �� �߰� =====
    }
    #endregion

    #region ���� ����
    //�� ���� ��ȹ �۽�
    public void SendMyRoundPlan(OppRoundPlan plan)
    {
        if (!PhotonNetwork.InRoom) return;
        string json = JsonUtility.ToJson(plan);
        object[] content = { json };
        PhotonNetwork.RaiseEvent((byte)PhotonEventCode.SendRoundPlan, content,
            new RaiseEventOptions { Receivers = ReceiverGroup.Others },
            SendOptions.SendReliable);
    }

    //���� ���� �غ�Ϸ� �˸� (Manual = 0, Timeout = 1)
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

    //�ش� ���� ���� �˸�
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
    #endregion

    // ==================================================== ���� �� =========================================================== //
    // ==================================================== ���� �� =========================================================== //
    // ==================================================== ���� �� =========================================================== //

    //public override void OnJoinRoomFailed(short returnCode, string message)
    //{
    //    Debug.Log($"�� ���� ����: {message}");
    //}
}

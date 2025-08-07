using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class AIBattleHelper
{
    /// <summary>
    /// 유저(나)의 저장된 덱들 중에서 랜덤으로 선택
    /// </summary>
    /// <returns></returns>
    public static DeckPack GetRandomDeckAI()
    {
        var decks = BackendManager.Instance.GetSortedDecks();
        if (decks == null || decks.Count == 0) {
            Debug.Log("AI용 덱이 없습니다");
            return null;
        }

        var values = decks;
        int index = Random.Range(0, values.Count);
        Debug.Log($"선택된 AI용 덱 이름: {values[index].Item2.deckName}");
        return values[index].Item2;
    }

    /// <summary>
    /// 선공 또는 후공 자동 선택
    /// </summary>
    /// <param name="uiCoinFlip"></param>
    public static void AutoSelectTurnOrderAI(UICoinFlip uiCoinFlip)
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount == 1) {
            //AI는 자동으로 선택하도록 유도 (1초 딜레이)
            uiCoinFlip.Invoke(nameof(UICoinFlip.AutoSelectTurnOrder), 1f);
        }
    }
}
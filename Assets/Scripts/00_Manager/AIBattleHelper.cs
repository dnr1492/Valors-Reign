using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class AIBattleHelper
{
    public static DeckPack GetRandomAIDeck()
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
}
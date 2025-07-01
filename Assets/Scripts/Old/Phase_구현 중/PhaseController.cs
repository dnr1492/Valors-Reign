using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PhaseController : MonoBehaviour
{
    [SerializeField] RectTransform safeAreaRt;

    private int turnCount = 0;
    private GameObject battleField = null;

    protected IEnumerator Timer(TextMeshProUGUI txtTimer, float timer)
    {
        float tempTimer;
        while (timer > 0) {
            tempTimer = timer -= Time.deltaTime;
            txtTimer.text = tempTimer.ToString("F2");
            yield return new WaitForSeconds(Time.deltaTime);
        }
        Debug.Log("Timer Complete");
    }

    protected void IncreaseTurnCount(TextMeshProUGUI txtTurnCount)
    {
        turnCount++;
        txtTurnCount.text = turnCount.ToString();
        Debug.Log($"{turnCount}��");
    }

    protected void CreateBattleField()
    {
        if (battleField == null) {
            var battleFieldGo = Resources.Load<GameObject>("BattleField");
            battleField = Instantiate(battleFieldGo, safeAreaRt);
            battleField.SetActive(false);
        }
    }

    protected void ActiveBattleField()
    {
        if (battleField == null) {
            Debug.Log("Not Found BattleField");
            return;
        }
        battleField.SetActive(true);
    }

    protected void InactiveBattleField()
    {
        if (battleField == null) {
            Debug.Log("Not Found BattleField");
            return;
        }
        battleField.SetActive(false);
    }
}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Phase1 : PhaseController
{
    [SerializeField] TextMeshProUGUI txtTimer, txtTurnCount;
    private float turnTimer = 30;

    private void Start()
    {
        CreateBattleField();
    }

    private void OnEnable()
    {
        StartCoroutine(Timer(txtTimer, turnTimer));
        IncreaseTurnCount(txtTurnCount);
    }
}

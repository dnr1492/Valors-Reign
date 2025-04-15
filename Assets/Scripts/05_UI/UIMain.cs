using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIMain : MonoBehaviour
{
    [SerializeField] DeckGenerator deckGenerator;
    [SerializeField] GameObject createDeckPhase1, createDeckPhase2;

    public void OnClickAddDeck()
    {
         //deckGenerator.CreateDeck();
    }
}

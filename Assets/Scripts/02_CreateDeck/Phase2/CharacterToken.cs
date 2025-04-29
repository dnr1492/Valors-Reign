using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterToken : MonoBehaviour
{
    [SerializeField] Image imgCharacter;

    private void Start()
    {
        gameObject.SetActive(false);
    }

    public void Init(Sprite sprite)
    {
        gameObject.SetActive(true);

        imgCharacter.sprite = sprite;
    }
}

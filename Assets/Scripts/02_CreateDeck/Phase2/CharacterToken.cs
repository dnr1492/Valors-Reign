using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterToken : MonoBehaviour
{
    [SerializeField] Image imgCharacter;
    [SerializeField] Button btn;
    [SerializeField] GameObject characterCardPrefab;

    private void Start()
    {
        gameObject.SetActive(false);
    }

    public void Init(Sprite sprite, CharacterCardData cardData)
    {
        gameObject.SetActive(true);

        imgCharacter.sprite = sprite;

        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => {
            //var characterCard = Instantiate(characterCardPrefab, transform);
            Debug.Log($"OnClick Event: {cardData.name}의 정보 표출");
        });
    }
}

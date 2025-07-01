using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillSlot : MonoBehaviour
{
    [SerializeField] Image img;
    [SerializeField] TextMeshProUGUI txt;

    public void Set(Sprite sprite, int skillCount)
    {
        img.sprite = sprite;
        txt.text = $"{skillCount}";
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISkillInfoPopup : MonoBehaviour
{
    [SerializeField] Button btn_close;

    private void Close()
    {
        Destroy(gameObject);
    }

    public void Init(SkillCardData skillCardData)
    {
        var sprite = SpriteManager.Instance.dicSkillSprite[skillCardData.name];
        GetComponentInChildren<UISkillCard>().Set(sprite, skillCardData);

        btn_close.onClick.AddListener(Close);
    }
}

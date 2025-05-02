using UnityEngine;
using UnityEngine.UI;

public class CustomBackground : MonoBehaviour
{
    [SerializeField] Image background;
    [SerializeField] Sprite[] sps;
    private int currentSelectID = 0;

    public void SetSelect(bool isSelect)
    {
        currentSelectID = isSelect ? 1 : 0;  //Select 1, Unselect 0
        if (sps != null && sps.Length > 0) background.sprite = sps[currentSelectID];
    }
}

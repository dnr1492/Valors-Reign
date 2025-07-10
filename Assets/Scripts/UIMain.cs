using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIMain : MonoBehaviour
{
    private void Start()
    {
        UIManager.Instance.ShowPopup<UILoginPopup>("UILoginPopup");
    }
}

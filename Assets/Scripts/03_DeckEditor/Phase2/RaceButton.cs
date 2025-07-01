using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static EnumClass;

public class RaceButton : MonoBehaviour
{
    [SerializeField] CharacterRace race;
    private Button btn;

    private void Awake()
    {
        btn = GetComponent<Button>();
        btn.onClick.AddListener(() => {
            UIManager.Instance.ShowPopup<UIEditorDeckPhase3>("UIEditorDeckPhase3");
            ControllerRegister.Get<FilterController>().InitFilter(race);
            Debug.Log($"Selected Race: {race}");
        });
    }
}

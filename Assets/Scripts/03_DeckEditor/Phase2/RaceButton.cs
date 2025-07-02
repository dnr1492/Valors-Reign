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
        btn.onClick.AddListener(async () => {
            UIManager.Instance.ShowPopup<UIEditorDeckPhase3>("UIEditorDeckPhase3");
            await ControllerRegister.Get<FilterController>().InitFilterAsync(race);
            Debug.Log($"Selected Race: {race}");
        });
    }
}

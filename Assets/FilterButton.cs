using UnityEngine;
using UnityEngine.UI;

public class FilterButton : MonoBehaviour
{
    public string FilterKey { get { return filterKey; } }
    private string filterKey;

    private bool isSelected;
    public bool IsSelected() => isSelected;

    private Image img;

    private void Awake()
    {
        filterKey = transform.name.Split("_")[1];
        Debug.Log($"filterKey: {filterKey}");

        img = GetComponent<Image>();
        SetSelected(false);
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        img.color = selected ? Color.cyan : Color.white;
    }
}
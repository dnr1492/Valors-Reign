using Unity.Jobs.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.UI;

public class FilterButton : MonoBehaviour
{
    public string FilterKey { get { return filterKey; } }
    private string filterKey;

    private bool isSelected;
    public bool IsSelected() => isSelected;

    private Image img;

    private void Start()
    {
        filterKey = transform.name.Split("_")[1];
        Debug.Log($"filterKey: {filterKey}");

        img = GetComponent<Image>();
        SetSelected(false);
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;

        //선택
        if (isSelected) img.color = Color.cyan;
        //미선택
        else {
            if (filterKey == "Type") 
                img.color = new Color(0.5283019f, 0.4660021f, 0.4660021f);
            else if (filterKey == "D" || filterKey == "T" || filterKey == "S" || filterKey == "C" || filterKey == "H" || filterKey == "M" || filterKey == "L") 
                img.color = new Color(0.7830189f, 0.7830189f, 0.7830189f);
            else 
                img.color = Color.white;
        }
    }
}
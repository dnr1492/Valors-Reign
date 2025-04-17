using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class FilterController : MonoBehaviour
{
    [SerializeField] FilterButton[] arrFilterButton;

    private HashSet<string> selectedJob = new();  //D, T, S
    private HashSet<string> selectedTier = new();  //C, H, M, L

    private void Awake()
    {
        for (int i = 0; i < arrFilterButton.Length; i++)
        {
            var btn = arrFilterButton[i].GetComponent<Button>();
            var filterBtn = arrFilterButton[i];
            if (btn != null) btn.onClick.AddListener(() => OnClickFilter(filterBtn));
        }
    }

    private void OnClickFilter(FilterButton filterButton)
    {
        string key = filterButton.FilterKey;

        bool isJob = key is "D" or "T" or "S";
        bool isTier = key is "C" or "H" or "M" or "L";

        if (isJob)
        {
            if (selectedJob.Contains(key)) selectedJob.Clear();
            else {
                selectedJob.Clear();
                selectedJob.Add(key);
            }
        }
        else if (isTier)
        {
            if (selectedTier.Contains(key)) selectedTier.Clear();
            else {
                selectedTier.Clear();
                selectedTier.Add(key);
            }
        }

        //직업/티어 버튼 상태만 업데이트
        foreach (var btn in arrFilterButton)
        {
            key = btn.FilterKey;
            if (key.Length == 1)
            {
                //직업 필터
                if (key is "D" or "T" or "S") btn.SetSelected(selectedJob.Contains(key));
                //티어 필터
                else if (key is "C" or "H" or "M" or "L") btn.SetSelected(selectedTier.Contains(key));
            }
        }

        //모든 필터 버튼만 따로 처리
        foreach (var btn in arrFilterButton)
        {
            if (!btn.FilterKey.Contains("/")) continue;

            string[] splitKey = btn.FilterKey.Split('/');
            if (splitKey.Length != 2) {
                btn.SetSelected(false);
                continue;
            }

            string tier = splitKey[0];
            string job = splitKey[1];

            if (selectedJob.Count > 0 && selectedTier.Count > 0) btn.SetSelected(selectedJob.Contains(job) && selectedTier.Contains(tier));
            else if (selectedJob.Count > 0) btn.SetSelected(selectedJob.Contains(job));
            else if (selectedTier.Count > 0) btn.SetSelected(selectedTier.Contains(tier));
            else btn.SetSelected(false);
        }

        ApplyFilter();
    }

    private void ApplyFilter()
    {
        //var allCharacterCards = DataManager.GetInstance().dicCharacterCardData;

        //var result = allCharacterCards.Where(ch =>
        //    (selectedJob.Count == 0 || selectedJob.Contains(ch.Value.job.Substring(0, 1).ToUpper())) &&
        //    (selectedTier.Count == 0 || selectedTier.Contains(ch.Value.tier.Substring(0, 1).ToUpper()))
        //).ToList();

        //foreach (var filterButton in arrFilterButton)
        //{
        //    string key = filterButton.FilterKey;
        //    bool matched = result.Any(ch => ch.Key.ToString() == key);
        //    filterButton.SetSelected(matched);
        //}
    }
}
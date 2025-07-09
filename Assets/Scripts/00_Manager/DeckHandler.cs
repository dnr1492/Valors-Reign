using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class DeckHandler
{
    private static readonly string basePath = Path.Combine(Application.persistentDataPath, "deck");

    public static void Save(DeckPack pack)
    {
        if (!Directory.Exists(basePath)) Directory.CreateDirectory(basePath);

        string name = SanitizeFileName(pack.deckName);
        string path = Path.Combine(basePath, $"deck_{pack.guid}.json");

        string json = JsonUtility.ToJson(pack, true);
        File.WriteAllText(path, json);
    }

    public static DeckPack LoadByGuid(string guid)
    {
        string path = Path.Combine(basePath, $"deck_{guid}.json");
        if (!File.Exists(path)) return null;

        string json = File.ReadAllText(path);
        return JsonUtility.FromJson<DeckPack>(json);
    }

    public static IEnumerable<(string guid, DeckPack pack)> LoadAll()
    {
        if (!Directory.Exists(basePath)) yield break;

        string[] files = Directory.GetFiles(basePath, "deck_*.json");
        foreach (string file in files)
        {
            string json = File.ReadAllText(file);
            DeckPack pack = JsonUtility.FromJson<DeckPack>(json);
            if (pack != null && !string.IsNullOrEmpty(pack.guid)) yield return (pack.guid, pack);
        }
    }

    private static string SanitizeFileName(string name)
    {
        foreach (char c in Path.GetInvalidFileNameChars())
            name = name.Replace(c, '_');
        return name;
    }
}


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
        string path = Path.Combine(basePath, $"deck_{name}.json");

        string json = JsonUtility.ToJson(pack, true);
        File.WriteAllText(path, json);
    }

    public static DeckPack Load(string deckName)
    {
        string name = SanitizeFileName(deckName);
        string path = Path.Combine(basePath, $"deck_{name}.json");

        if (!File.Exists(path)) return null;

        string json = File.ReadAllText(path);
        return JsonUtility.FromJson<DeckPack>(json);
    }

    public static IEnumerable<(string deckName, DeckPack pack)> LoadAll()
    {
        if (!Directory.Exists(basePath)) yield break;

        string[] files = Directory.GetFiles(basePath, "deck_*.json");
        foreach (string file in files)
        {
            string deckName = Path.GetFileNameWithoutExtension(file).Replace("deck_", "");
            DeckPack pack = Load(deckName);
            if (pack != null) yield return (deckName, pack);
        }
    }

    private static string SanitizeFileName(string name)
    {
        foreach (char c in Path.GetInvalidFileNameChars())
            name = name.Replace(c, '_');
        return name;
    }
}


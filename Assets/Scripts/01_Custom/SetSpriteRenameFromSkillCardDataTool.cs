using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class SetSpriteRenameFromSkillCardDataTool : EditorWindow
{
    public TextAsset toolScriptFile;  //Tool.cs 파일 참조
    public string prefix = "";  //파일명 앞에 붙일 문자열
    public string suffix = "";  //파일명 뒤에 붙일 문자열

    private List<string> extractedNames = new List<string>();  //추출된 스킬 이름 목록
    private List<string> previewOld = new List<string>();  //기존 파일명 목록
    private List<string> previewNew = new List<string>();  //변경될 파일명 목록
    private Vector2 scroll;  //미리보기 스크롤 위치
    private List<(string path, string tempName, string finalName)> renameQueue = new List<(string, string, string)>();  //임시 → 최종 이름 매핑 저장용

    [MenuItem("정재욱/Sprite/Rename From SkillCardData")]
    public static void Open()
    {
        SetSpriteRenameFromSkillCardDataTool window = GetWindow<SetSpriteRenameFromSkillCardDataTool>("Rename From SkillCardData");
        window.minSize = new Vector2(520, 420);
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("스프라이트 이름 일괄 변경 (SkillCardData 기반)", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "1) Tool.cs 파일 드래그 (SkillCardData가 들어있는)\n" +
            "2) Project 창에서 변경할 스프라이트 선택\n" +
            "3) Load Names → Preview → Apply Rename", MessageType.Info);

        toolScriptFile = (TextAsset)EditorGUILayout.ObjectField(new GUIContent("Tool.cs 파일", "SkillCardData 데이터가 들어있는 스크립트"),
                                                                 toolScriptFile, typeof(TextAsset), false);
        prefix = EditorGUILayout.TextField(new GUIContent("Prefix", "파일명 앞에 붙일 문자열"), prefix);
        suffix = EditorGUILayout.TextField(new GUIContent("Suffix", "파일명 뒤에 붙일 문자열"), suffix);

        EditorGUILayout.Space(6);

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Load Names"))
                LoadNames();
            if (GUILayout.Button("Clear"))
            {
                extractedNames.Clear();
                previewOld.Clear();
                previewNew.Clear();
            }
        }

        EditorGUILayout.LabelField($"추출된 스킬 이름 수: {extractedNames.Count}");

        if (GUILayout.Button("Preview"))
            BuildPreview();

        if (previewOld.Count > 0)
        {
            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("미리보기", EditorStyles.boldLabel);
            scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Height(180));
            for (int i = 0; i < previewOld.Count; i++)
                EditorGUILayout.LabelField($"{i + 1}. {previewOld[i]}  →  {previewNew[i]}");
            EditorGUILayout.EndScrollView();
        }

        GUI.enabled = previewOld.Count > 0;
        if (GUILayout.Button("Apply Rename", GUILayout.Height(32)))
            ApplyRename();
        GUI.enabled = true;
    }

    private void LoadNames()
    {
        extractedNames.Clear();
        if (toolScriptFile == null)
        {
            Debug.Log("Tool.cs 파일을 지정하세요.");
            return;
        }

        string path = AssetDatabase.GetAssetPath(toolScriptFile);  //선택한 Tool.cs 경로
        string text = File.ReadAllText(path, Encoding.GetEncoding("euc-kr"));  //EUC-KR로 파일 읽기

        MatchCollection matches = Regex.Matches(text, @"SkillCardData\s*\{[^}]*name\s*=\s*""([^""]+)""");
        foreach (Match m in matches)
        {
            if (m.Groups.Count > 1)
            {
                string skillName = m.Groups[1].Value.Trim();
                if (!string.IsNullOrEmpty(skillName))
                    extractedNames.Add(skillName);
            }
        }

        Debug.Log($"스킬 이름 추출 완료: {extractedNames.Count}개");
    }

    private void BuildPreview()
    {
        previewOld.Clear();
        previewNew.Clear();

        List<string> spritePaths = GetSelectedSpriteAssetPaths();
        if (spritePaths.Count == 0)
        {
            Debug.Log("Project 창에서 스프라이트(텍스처)를 선택하세요.");
            return;
        }

        if (spritePaths.Count != extractedNames.Count)
            Debug.Log($"스프라이트 수({spritePaths.Count})와 스킬 이름 수({extractedNames.Count})가 다릅니다. 순서가 어긋날 수 있음.");

        spritePaths.Sort();
        int count = Mathf.Min(spritePaths.Count, extractedNames.Count);

        HashSet<string> used = new HashSet<string>();
        for (int i = 0; i < count; i++)
        {
            string oldPath = spritePaths[i];
            string oldName = Path.GetFileNameWithoutExtension(oldPath);
            string ext = Path.GetExtension(oldPath);

            string target = SanitizeFileName(prefix + extractedNames[i] + suffix);
            string unique = MakeUnique(target, used);
            used.Add(unique);

            previewOld.Add(oldName + ext);
            previewNew.Add(unique + ext);
        }
    }

    private void ApplyRename()
    {
        List<string> spritePaths = GetSelectedSpriteAssetPaths();
        if (spritePaths.Count == 0 || extractedNames.Count == 0 || previewNew.Count == 0)
        {
            Debug.Log("적용할 데이터가 없습니다.");
            return;
        }

        spritePaths.Sort();
        int count = Mathf.Min(spritePaths.Count, previewNew.Count);

        //임시 → 최종 이름 매핑 저장
        renameQueue.Clear();
        for (int i = 0; i < count; i++)
        {
            string path = spritePaths[i];
            string tempName = $"__TMP_RENAME_{i}__";
            string finalName = previewNew[i];
            renameQueue.Add((path, tempName, finalName));
        }

        //1단계: 임시 이름 적용
        AssetDatabase.StartAssetEditing();
        try
        {
            foreach (var item in renameQueue)
            {
                AssetDatabase.RenameAsset(item.path, item.tempName);
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        //다음 프레임에 최종 이름 적용 예약
        EditorApplication.delayCall += ApplyFinalRename;
    }

    private void ApplyFinalRename()
    {
        //2단계: 최종 이름 적용
        AssetDatabase.StartAssetEditing();
        try
        {
            foreach (var item in renameQueue)
            {
                string dir = Path.GetDirectoryName(item.path);
                string ext = Path.GetExtension(item.path);
                string tempPath = Path.Combine(dir, item.tempName + ext).Replace("\\", "/");
                AssetDatabase.RenameAsset(tempPath, item.finalName);
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        Debug.Log("임시명 → 최종명 변경 완료");
        renameQueue.Clear();
    }

    private static List<string> GetSelectedSpriteAssetPaths()
    {
        List<string> res = new List<string>();
        foreach (Object obj in Selection.objects)
        {
            string path = AssetDatabase.GetAssetPath(obj);
            if (string.IsNullOrEmpty(path))
                continue;

            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null && importer.textureType == TextureImporterType.Sprite)
                res.Add(path);
        }
        return res;
    }

    private static string SanitizeFileName(string s)
    {
        foreach (char c in Path.GetInvalidFileNameChars())
            s = s.Replace(c.ToString(), "");
        return s.Trim();
    }

    private static string MakeUnique(string name, HashSet<string> used)
    {
        if (!used.Contains(name)) return name;
        int idx = 1;
        string test;
        do { test = $"{name}_{idx++}"; } while (used.Contains(test));
        return test;
    }
}
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

/// <summary>
/// Сканирует C# скрипты на пути к .prefab ассетам и копирует их в Assets/Resources/GamePrefabs/
/// чтобы они были доступны через Resources.Load() в билде.
/// Запускается автоматически перед билдом и вручную через меню.
/// </summary>
public class PrefabResourceSync : IPreprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        SyncPrefabs();
        ShaderIncludeTool.FixBuildShaders();
    }

    [MenuItem("Tools/Sync Prefabs to Resources")]
    public static void SyncPrefabs()
    {
        string scriptsDir = Path.Combine(Application.dataPath, "Scripts");
        string resourcesDir = Path.Combine(Application.dataPath, "Resources", "GamePrefabs");

        if (!Directory.Exists(resourcesDir))
            Directory.CreateDirectory(resourcesDir);

        var prefabPaths = new HashSet<string>();
        var regex = new Regex(@"""(Assets/[^""]+\.prefab)""", RegexOptions.Compiled);

        foreach (string csFile in Directory.GetFiles(scriptsDir, "*.cs", SearchOption.AllDirectories))
        {
            string content = File.ReadAllText(csFile);
            foreach (Match match in regex.Matches(content))
            {
                string assetPath = match.Groups[1].Value;
                prefabPaths.Add(assetPath);
            }
        }

        int copied = 0;
        int skipped = 0;
        var seenNames = new Dictionary<string, string>();

        foreach (string assetPath in prefabPaths)
        {
            string fullSource = Path.Combine(Application.dataPath, "..", assetPath);
            fullSource = Path.GetFullPath(fullSource);

            if (!File.Exists(fullSource))
            {
                Debug.LogWarning($"[PrefabResourceSync] Prefab not found: {assetPath}");
                continue;
            }

            string fileName = Path.GetFileNameWithoutExtension(assetPath);
            string destPath = Path.Combine(resourcesDir, fileName + ".prefab");

            if (seenNames.ContainsKey(fileName))
            {
                if (seenNames[fileName] != assetPath)
                    Debug.LogWarning($"[PrefabResourceSync] Filename collision: '{fileName}' from '{assetPath}' and '{seenNames[fileName]}'");
                continue;
            }
            seenNames[fileName] = assetPath;

            bool needsCopy = !File.Exists(destPath)
                || File.GetLastWriteTimeUtc(fullSource) > File.GetLastWriteTimeUtc(destPath);

            if (!needsCopy) { skipped++; continue; }

            File.Copy(fullSource, destPath, true);
            copied++;
        }

        if (copied > 0)
            AssetDatabase.Refresh();

        Debug.Log($"[PrefabResourceSync] Done: {copied} copied, {skipped} up-to-date, {prefabPaths.Count} total paths found.");
    }
}

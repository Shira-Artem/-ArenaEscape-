#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public static class ShaderIncludeTool
{
    [MenuItem("Tools/Fix Build Shaders (Add All to Always Included)")]
    public static void FixBuildShadersMenu() { FixBuildShaders(true); }

    public static void FixBuildShaders(bool showDialog = false)
    {
        string[] shaderNames = new[]
        {
            "Universal Render Pipeline/Lit",
            "Standard",
            "Legacy Shaders/Diffuse",
            "Sprites/Default",
            "Universal Render Pipeline/Particles/Unlit",
            "Particles/Standard Unlit",
            "Legacy Shaders/Particles/Additive",
            "Legacy Shaders/Particles/Alpha Blended",
            "Unlit/Color",
            "UI/Default",
        };

        var graphicsSettings = AssetDatabase.LoadAssetAtPath<GraphicsSettings>("ProjectSettings/GraphicsSettings.asset");
        var so = new SerializedObject(graphicsSettings);
        var prop = so.FindProperty("m_AlwaysIncludedShaders");

        var existing = new HashSet<string>();
        for (int i = 0; i < prop.arraySize; i++)
        {
            var shader = prop.GetArrayElementAtIndex(i).objectReferenceValue as Shader;
            if (shader != null) existing.Add(shader.name);
        }

        int added = 0;
        foreach (var name in shaderNames)
        {
            if (existing.Contains(name)) continue;
            var shader = Shader.Find(name);
            if (shader == null) { Debug.LogWarning($"[ShaderInclude] Shader '{name}' not found, skipping"); continue; }

            int idx = prop.arraySize;
            prop.InsertArrayElementAtIndex(idx);
            prop.GetArrayElementAtIndex(idx).objectReferenceValue = shader;
            existing.Add(name);
            added++;
        }

        so.ApplyModifiedProperties();
        Debug.Log($"[ShaderInclude] Done! Added {added} shaders. Total: {prop.arraySize}");
        if (showDialog)
            EditorUtility.DisplayDialog("Fix Build Shaders",
                $"Added {added} new shaders to Always Included Shaders.\nTotal: {prop.arraySize}\n\nNow rebuild the game.",
                "OK");
    }
}
#endif

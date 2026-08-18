using UnityEngine;

/// <summary>
/// Общие утилиты для Level0 Dressing-билдеров (ForestBuilder, CastleDressingBuilder).
/// Содержит: FixMaterials, RemoveAllColliders, LoadPrefab, CreateSimpleMat, fallback-деревья.
/// </summary>
public static class L0Util
{
    public static void FixMaterials(GameObject obj)
    {
        Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
        Shader standard = Shader.Find("Standard");
        Shader fallback = urpLit != null ? urpLit : standard;
        if (fallback == null) fallback = Shader.Find("Diffuse");
        if (fallback == null) return;

        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            Material[] mats = renderers[i].materials;
            bool changed = false;
            for (int m = 0; m < mats.Length; m++)
            {
                bool isBroken = mats[m] == null || mats[m].shader == null
                    || mats[m].shader.name.Contains("Error")
                    || mats[m].shader.name.Contains("InternalErrorShader");
                if (!isBroken) continue;

                Color origColor = new Color(0.5f, 0.5f, 0.5f);
                Texture origTex = null;
                if (mats[m] != null)
                {
                    if (mats[m].HasProperty("_BaseColor")) origColor = mats[m].GetColor("_BaseColor");
                    else if (mats[m].HasProperty("_Color")) origColor = mats[m].color;
                    if (mats[m].HasProperty("_BaseMap")) origTex = mats[m].GetTexture("_BaseMap");
                    else if (mats[m].HasProperty("_MainTex")) origTex = mats[m].GetTexture("_MainTex");
                }
                mats[m] = new Material(fallback);
                if (mats[m].HasProperty("_BaseColor")) mats[m].SetColor("_BaseColor", origColor);
                if (mats[m].HasProperty("_Color")) mats[m].color = origColor;
                if (origTex != null)
                {
                    if (mats[m].HasProperty("_BaseMap")) mats[m].SetTexture("_BaseMap", origTex);
                    if (mats[m].HasProperty("_MainTex")) mats[m].mainTexture = origTex;
                }
                changed = true;
            }
            if (changed) renderers[i].materials = mats;
        }
    }

    /// <summary>
    /// Покраска дерева: если есть текстура (PP палитра) — мягкий тинт для насыщенности.
    /// Если текстуры нет (серый/сломанный материал) — ставим зелёный напрямую.
    /// PP деревья = 1 меш + 1 материал + UV-палитра (ствол и листва в одной текстуре).
    /// </summary>
    public static void TintTree(GameObject obj)
    {
        // R7: приглушённая зелень — не пересвеченная, темнее, но НЕ серая.
        Color greenBoost = new Color(0.50f, 0.72f, 0.40f);
        Color fallbackColor = new Color(0.26f, 0.42f, 0.20f);

        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            Material[] mats = renderers[i].materials;
            bool changed = false;
            for (int m = 0; m < mats.Length; m++)
            {
                if (mats[m] == null) continue;
                bool hasTexture = HasMainTexture(mats[m]);
                if (hasTexture)
                {
                    Color baseCol = GetBaseColor(mats[m]);
                    Color result = baseCol * greenBoost;
                    // Если зелёный не доминирует (жёлтые/бежевые берёзы) — форсируем
                    if (result.g < result.r + 0.08f)
                        result = Color.Lerp(result, new Color(0.30f, 0.46f, 0.22f), 0.50f);
                    SetBaseColor(mats[m], result);
                }
                else
                {
                    SetBaseColor(mats[m], fallbackColor);
                }
                changed = true;
            }
            if (changed) renderers[i].materials = mats;
        }
    }

    private static bool HasMainTexture(Material mat)
    {
        if (mat.HasProperty("_BaseMap") && mat.GetTexture("_BaseMap") != null) return true;
        if (mat.HasProperty("_MainTex") && mat.GetTexture("_MainTex") != null) return true;
        return false;
    }

    private static Color GetBaseColor(Material mat)
    {
        if (mat.HasProperty("_BaseColor")) return mat.GetColor("_BaseColor");
        if (mat.HasProperty("_Color")) return mat.color;
        return Color.white;
    }

    private static void SetBaseColor(Material mat, Color color)
    {
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
        if (mat.HasProperty("_Color")) mat.color = color;
    }

    /// <summary>
    /// Умножает _BaseColor всех материалов на tint (для не-деревьев).
    /// </summary>
    public static void TintMaterials(GameObject obj, Color tint)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            Material[] mats = renderers[i].materials;
            bool changed = false;
            for (int m = 0; m < mats.Length; m++)
            {
                if (mats[m] == null) continue;
                if (mats[m].HasProperty("_BaseColor"))
                {
                    mats[m].SetColor("_BaseColor", mats[m].GetColor("_BaseColor") * tint);
                    changed = true;
                }
                else if (mats[m].HasProperty("_Color"))
                {
                    mats[m].color = mats[m].color * tint;
                    changed = true;
                }
            }
            if (changed) renderers[i].materials = mats;
        }
    }

    public static void RemoveAllColliders(GameObject obj)
    {
        Collider[] cols = obj.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < cols.Length; i++)
        {
            if (Application.isPlaying)
                Object.Destroy(cols[i]);
            else
                Object.DestroyImmediate(cols[i]);
        }
    }

    public static void RemoveColliderSingle(GameObject obj)
    {
        Collider col = obj.GetComponent<Collider>();
        if (col != null)
        {
            if (Application.isPlaying) Object.Destroy(col);
            else Object.DestroyImmediate(col);
        }
    }

    public static GameObject LoadPrefab(string path)
    {
#if UNITY_EDITOR
        return UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
#else
        string resName = PrefabPathToResourceName(path);
        return Resources.Load<GameObject>(resName);
#endif
    }

    static string PrefabPathToResourceName(string assetPath)
    {
        string name = System.IO.Path.GetFileNameWithoutExtension(assetPath);
        return "GamePrefabs/" + name;
    }

    public static Material CreateSimpleMat(Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        if (shader == null) shader = Shader.Find("Diffuse");
        Material mat = new Material(shader);
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
        if (mat.HasProperty("_Color")) mat.color = color;
        return mat;
    }

    public static void CreateFallbackTree(Vector3 pos, float scale, Transform parent, string name)
    {
        GameObject tree = new GameObject(name + "_fallback");
        tree.transform.SetParent(parent);
        tree.transform.position = pos;

        Material trunkMat = CreateSimpleMat(new Color(0.35f, 0.22f, 0.1f));
        Material leafMat = CreateSimpleMat(new Color(0.2f, 0.5f, 0.18f));

        GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        trunk.transform.SetParent(tree.transform);
        trunk.transform.localPosition = new Vector3(0, 1.5f * scale, 0);
        trunk.transform.localScale = new Vector3(0.3f * scale, 1.5f * scale, 0.3f * scale);
        trunk.GetComponent<Renderer>().material = trunkMat;
        RemoveColliderSingle(trunk);

        GameObject crown = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        crown.transform.SetParent(tree.transform);
        crown.transform.localPosition = new Vector3(0, 3.5f * scale, 0);
        crown.transform.localScale = new Vector3(2f * scale, 2.5f * scale, 2f * scale);
        crown.GetComponent<Renderer>().material = leafMat;
        RemoveColliderSingle(crown);
    }

    public static void SafeDestroy(GameObject obj)
    {
        if (obj == null) return;
        if (Application.isPlaying) Object.Destroy(obj);
        else Object.DestroyImmediate(obj);
    }

    /// <summary>
    /// Загружает prefab, ставит, чинит материалы, убирает коллайдеры. Возвращает null если prefab не найден.
    /// </summary>
    public static GameObject Place(string prefabPath, Vector3 pos, Quaternion rot, float scale, Transform parent, string name = null)
    {
        GameObject prefab = LoadPrefab(prefabPath);
        if (prefab == null) return null;
        GameObject obj = Object.Instantiate(prefab, pos, rot, parent);
        if (name != null) obj.name = name;
        obj.transform.localScale = Vector3.one * scale;
        FixMaterials(obj);
        RemoveAllColliders(obj);
        return obj;
    }

    /// <summary>
    /// Place + загрузка msVFX дыма (ParticleSystem). Не убирает коллайдеры — их там нет.
    /// </summary>
    public static GameObject PlaceSmoke(string prefabPath, Vector3 pos, float scale, Transform parent, string name = null)
    {
        GameObject prefab = LoadPrefab(prefabPath);
        if (prefab == null) return null;
        GameObject obj = Object.Instantiate(prefab, pos, Quaternion.identity, parent);
        if (name != null) obj.name = name;
        obj.transform.localScale = Vector3.one * scale;
        return obj;
    }
}

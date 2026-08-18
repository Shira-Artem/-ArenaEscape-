using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Mountain Wall v5 — естественные low-poly горы вокруг карты.
/// Горы строятся из простых мешей-пирамид, образующих непрерывные хребты,
/// с несколькими слоями: дальний силуэт, предгорья, ближние валуны и туман.
/// Без хаотичных наклонённых цилиндров и визуального мусора.
/// </summary>
public static class CastleMountainBuilder
{
    // -------------------- Public entry point --------------------
    public static void BuildDistantMountains(CastleGenerator.CastleContext c, GameObject parent)
    {
        BuildMountainWall(c, parent);
    }

    // -------------------- Main builder --------------------
    private static void BuildMountainWall(CastleGenerator.CastleContext c, GameObject parent)
    {
        GameObject wall = c.Child(parent, "Mountain_Wall");

        // --- Материалы ---
        // Дальние горы – синевато-серые
        Material farMountainMat = c.NewMaterial(new Color(0.32f, 0.35f, 0.42f));
        // Средние предгорья – каменный
        Material midMountainMat = c.NewMaterial(new Color(0.46f, 0.43f, 0.39f));
        // Валуны
        Material boulderMat = c.NewMaterial(new Color(0.40f, 0.37f, 0.33f));
        // Снег
        Material snowMat = c.NewMaterial(new Color(0.96f, 0.98f, 0.94f));
        // Туман (прозрачный будет создан отдельно)

        // ================ Layer A : дальний силуэт ================
        // Северный хребет (самый высокий, z ≈ 110..125)
        BuildRidge(c, wall, "Far_North", new Vector3(0, 0, 110), 12, 14f,
            28f, 38f, new Vector3(1, 0, 0), farMountainMat, snowMat, true);
        // Западный
        BuildRidge(c, wall, "Far_West", new Vector3(-115, 0, 0), 12, 14f,
            26f, 34f, new Vector3(0, 0, 1), farMountainMat, snowMat, true);
        // Восточный
        BuildRidge(c, wall, "Far_East", new Vector3(115, 0, 0), 12, 14f,
            26f, 34f, new Vector3(0, 0, 1), farMountainMat, snowMat, true);
        // Южный с проходом для дороги
        BuildRidgeWithGap(c, wall, "Far_South", new Vector3(0, 0, -120), 14, 14f,
            24f, 30f, new Vector3(1, 0, 0), farMountainMat, snowMat, true, 22f);
        // Диагональные углы
        BuildRidge(c, wall, "Far_NW", new Vector3(-110, 0, 90), 6, 14f,
            24f, 30f, new Vector3(1, 0, 1).normalized, farMountainMat, snowMat, true);
        BuildRidge(c, wall, "Far_NE", new Vector3(110, 0, 90), 6, 14f,
            24f, 30f, new Vector3(-1, 0, 1).normalized, farMountainMat, snowMat, true);
        BuildRidge(c, wall, "Far_SW", new Vector3(-110, 0, -90), 6, 14f,
            22f, 28f, new Vector3(1, 0, -1).normalized, farMountainMat, snowMat, true);
        BuildRidge(c, wall, "Far_SE", new Vector3(110, 0, -90), 6, 14f,
            22f, 28f, new Vector3(-1, 0, -1).normalized, farMountainMat, snowMat, true);

        // ================ Layer B : средние предгорья ================
        BuildRidge(c, wall, "Mid_North", new Vector3(0, 0, 90), 10, 12f,
            12f, 18f, new Vector3(1, 0, 0), midMountainMat, snowMat, false);
        BuildRidge(c, wall, "Mid_West", new Vector3(-95, 0, 0), 10, 12f,
            12f, 18f, new Vector3(0, 0, 1), midMountainMat, snowMat, false);
        BuildRidge(c, wall, "Mid_East", new Vector3(95, 0, 0), 10, 12f,
            12f, 18f, new Vector3(0, 0, 1), midMountainMat, snowMat, false);
        BuildRidgeWithGap(c, wall, "Mid_South", new Vector3(0, 0, -100), 12, 12f,
            10f, 16f, new Vector3(1, 0, 0), midMountainMat, snowMat, false, 22f);
        // Углы средних
        BuildRidge(c, wall, "Mid_NW", new Vector3(-85, 0, 75), 5, 12f,
            12f, 16f, new Vector3(1, 0, 1).normalized, midMountainMat, snowMat, false);
        BuildRidge(c, wall, "Mid_NE", new Vector3(85, 0, 75), 5, 12f,
            12f, 16f, new Vector3(-1, 0, 1).normalized, midMountainMat, snowMat, false);
        BuildRidge(c, wall, "Mid_SW", new Vector3(-85, 0, -75), 5, 12f,
            12f, 16f, new Vector3(1, 0, -1).normalized, midMountainMat, snowMat, false);
        BuildRidge(c, wall, "Mid_SE", new Vector3(85, 0, -75), 5, 12f,
            12f, 16f, new Vector3(-1, 0, -1).normalized, midMountainMat, snowMat, false);

        // ================ Layer C : ближние валуны ================
        BuildScatteredBoulders(c, wall, boulderMat);

        // ================ Туман ================
        BuildFogBands(c, wall);
    }

    // -------------------- Генерация одного пика через Mesh --------------------
    /// <summary>
    /// Создаёт GameObject с low-poly горой в виде пирамиды с многоугольным основанием.
    /// Вершина немного смещена для естественности.
    /// </summary>
    private static GameObject MountainPeakMesh(CastleGenerator.CastleContext c, GameObject parent,
        string name, Vector3 position, float baseWidth, float height, Material material, int seed)
    {
        Random.State oldState = Random.state;
        Random.InitState(seed);

        int sides = Random.Range(5, 7); // 5-6 вершин основания
        float radius = baseWidth * 0.5f;
        float peakJitter = radius * 0.3f;
        Vector3 localPeak = new Vector3(Random.Range(-peakJitter, peakJitter), height, Random.Range(-peakJitter, peakJitter));

        // Вершины основания по кругу с лёгким разбросом
        List<Vector3> verts = new List<Vector3>();
        for (int i = 0; i < sides; i++)
        {
            float angle = (i / (float)sides) * Mathf.PI * 2f;
            float r = radius * Random.Range(0.9f, 1.1f);
            verts.Add(new Vector3(Mathf.Cos(angle) * r, 0f, Mathf.Sin(angle) * r));
        }
        int peakIdx = verts.Count;
        verts.Add(localPeak);

        // Треугольники (боковые грани + дно)
        List<int> tris = new List<int>();
        for (int i = 0; i < sides; i++)
        {
            int next = (i + 1) % sides;
            tris.Add(i);
            tris.Add(next);
            tris.Add(peakIdx);
        }
        for (int i = 0; i < sides - 2; i++)
        {
            tris.Add(0);
            tris.Add(i + 2);
            tris.Add(i + 1);
        }

        Mesh mesh = new Mesh();
        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.RecalculateNormals();

        GameObject go = new GameObject(name);
        go.transform.position = position;
        go.transform.parent = parent.transform;

        MeshFilter mf = go.AddComponent<MeshFilter>();
        mf.mesh = mesh;
        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        mr.material = material;
        // Без коллайдера

        Random.state = oldState;
        return go;
    }

    // -------------------- Хребет --------------------
    /// <summary>
    /// Строит линию пиков вдоль направления.
    /// </summary>
    private static void BuildRidge(CastleGenerator.CastleContext c, GameObject parent,
        string ridgeName, Vector3 center, int count, float spacing,
        float heightMin, float heightMax,
        Vector3 direction, Material rockMat, Material snowMat, bool snowEnabled)
    {
        GameObject ridge = c.Child(parent, ridgeName);
        direction.Normalize();
        Vector3 perp = new Vector3(direction.z, 0, -direction.x); // перпендикуляр для разнообразия

        Random.State oldState = Random.state;
        Random.InitState(center.GetHashCode() + ridgeName.GetHashCode());

        // Ширина пика теперь считается автоматически, потому что в вызовах BuildRidge
        // передаются только высоты. Так мы убираем ошибку CS7036.
        float baseWidth = Mathf.Lerp(heightMin, heightMax, 0.72f);

        for (int i = 0; i < count; i++)
        {
            float t = i - (count - 1) * 0.5f;
            Vector3 pos = center + direction * (t * spacing);
            pos += perp * Random.Range(-spacing * 0.2f, spacing * 0.2f);
            pos.y = 0f;

            float h = Random.Range(heightMin, heightMax);
            int seed = Mathf.RoundToInt(center.x * 100 + center.z * 100 + i * 1000);

            GameObject peak = MountainPeakMesh(c, ridge, "Peak_" + i, pos, baseWidth, h, rockMat, seed);

            if (snowEnabled && h > 18f)
            {
                AddSnowCap(c, ridge, pos, baseWidth, h, snowMat, seed);
            }
        }

        Random.state = oldState;
    }

    /// <summary>
    /// Хребет с пропуском в центре (для дороги).
    /// </summary>
    private static void BuildRidgeWithGap(CastleGenerator.CastleContext c, GameObject parent,
        string ridgeName, Vector3 center, int count, float spacing,
        float heightMin, float heightMax,
        Vector3 direction, Material rockMat, Material snowMat, bool snowEnabled, float gapWidth)
    {
        GameObject ridge = c.Child(parent, ridgeName);
        direction.Normalize();
        Vector3 perp = new Vector3(direction.z, 0, -direction.x);
        Random.State oldState = Random.state;
        Random.InitState(center.GetHashCode() + ridgeName.GetHashCode());

        // Ширина пика теперь считается автоматически, потому что в вызовах BuildRidgeWithGap
        // передаются только высоты. Так мы убираем ошибку CS7036.
        float baseWidth = Mathf.Lerp(heightMin, heightMax, 0.72f);

        for (int i = 0; i < count; i++)
        {
            float t = i - (count - 1) * 0.5f;
            Vector3 pos = center + direction * (t * spacing);
            // Проверка на попадание в коридор
            if (Mathf.Abs(t * spacing) < gapWidth * 0.5f)
                continue;

            pos += perp * Random.Range(-spacing * 0.2f, spacing * 0.2f);
            pos.y = 0f;

            float h = Random.Range(heightMin, heightMax);
            int seed = Mathf.RoundToInt(center.x * 100 + center.z * 100 + i * 1000);
            MountainPeakMesh(c, ridge, "Peak_" + i, pos, baseWidth, h, rockMat, seed);

            if (snowEnabled && h > 18f)
                AddSnowCap(c, ridge, pos, baseWidth, h, snowMat, seed);
        }

        Random.state = oldState;
    }

    // -------------------- Снежные шапки --------------------
    private static void AddSnowCap(CastleGenerator.CastleContext c, GameObject parent,
        Vector3 basePos, float baseWidth, float height, Material snowMat, int seed)
    {
        Random.State oldState = Random.state;
        Random.InitState(seed + 10000);
        int patches = Random.Range(3, 6);
        for (int i = 0; i < patches; i++)
        {
            float angle = Random.Range(0f, 360f);
            float dist = baseWidth * 0.15f * Random.Range(0.5f, 1f);
            Vector3 offset = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad) * dist, 0, Mathf.Sin(angle * Mathf.Deg2Rad) * dist);
            Vector3 snowPos = basePos + offset + new Vector3(0, height * 0.88f, 0);
            float size = 0.3f + Random.Range(0.1f, 0.4f);
            c.Sphere(parent, "SnowPatch_" + i, snowPos, new Vector3(size, size * 0.5f, size), snowMat, false);
        }
        Random.state = oldState;
    }

    // -------------------- Ближние валуны --------------------
    private static void BuildScatteredBoulders(CastleGenerator.CastleContext c, GameObject parent, Material boulderMat)
    {
        GameObject boulders = c.Child(parent, "Near_Boulders");
        Random.State oldState = Random.state;
        Random.InitState(999);
        int count = 30;
        for (int i = 0; i < count; i++)
        {
            float angle = Random.Range(0f, 360f);
            float dist = Random.Range(45f, 65f);
            float x = Mathf.Cos(angle * Mathf.Deg2Rad) * dist;
            float z = Mathf.Sin(angle * Mathf.Deg2Rad) * dist;

            // Не загораживаем ворота
            if (Mathf.Abs(x) < 15f && z < -35f) continue;

            float scale = Random.Range(0.8f, 2.2f);
            Vector3 pos = new Vector3(x, scale * 0.2f, z);

            int shape = Random.Range(0, 3);
            GameObject rock;
            if (shape == 0)
                rock = c.Sphere(boulders, "Boulder_" + i, pos, new Vector3(scale, scale * 0.6f, scale), boulderMat, false);
            else if (shape == 1)
                rock = c.Cube(boulders, "Boulder_" + i, pos, new Vector3(scale, scale * 0.5f, scale * 0.8f), boulderMat, false);
            else
                rock = c.Cylinder(boulders, "Boulder_" + i, pos, new Vector3(scale * 0.7f, scale * 0.6f, scale * 0.7f), boulderMat, false);

            if (rock != null)
                rock.transform.rotation = Quaternion.Euler(Random.Range(0f, 30f), Random.Range(0f, 360f), Random.Range(0f, 20f));
        }
        Random.state = oldState;
    }

    // -------------------- Туманные полосы --------------------
    private static void BuildFogBands(CastleGenerator.CastleContext c, GameObject parent)
    {
        GameObject fogGroup = c.Child(parent, "Fog_Bands");

        // Прозрачный материал (Legacy Shaders/Transparent/Diffuse надёжен в любом RP)
        Shader fogShader = Shader.Find("Legacy Shaders/Transparent/Diffuse");
        if (fogShader == null) fogShader = Shader.Find("Standard");
        if (fogShader == null) fogShader = Shader.Find("Universal Render Pipeline/Lit");
        if (fogShader == null) fogShader = Shader.Find("Diffuse");

        Material fogMaterial = new Material(fogShader);
        fogMaterial.color = new Color(0.65f, 0.72f, 0.8f, 0.28f);

        Random.State oldState = Random.state;
        Random.InitState(777);
        int count = 12;
        for (int i = 0; i < count; i++)
        {
            float angle = Random.Range(0f, 360f);
            float dist = Random.Range(70f, 95f);
            float x = Mathf.Cos(angle * Mathf.Deg2Rad) * dist;
            float z = Mathf.Sin(angle * Mathf.Deg2Rad) * dist;
            float y = Random.Range(2f, 7f);

            float width = Random.Range(8f, 15f);
            float depth = Random.Range(3f, 6f);

            GameObject band = GameObject.CreatePrimitive(PrimitiveType.Cube);
            band.name = "FogBand_" + i;
            band.transform.position = new Vector3(x, y, z);
            band.transform.localScale = new Vector3(width, 0.2f, depth);
            band.transform.parent = fogGroup.transform;

            // Убираем коллайдер
            Collider col = band.GetComponent<Collider>();
            if (col != null) Object.Destroy(col);

            // Прозрачный материал
            band.GetComponent<Renderer>().material = fogMaterial;

            // Доворачиваем к центру карты для мягкости
            band.transform.LookAt(new Vector3(0, y, 0));
            band.transform.Rotate(0, Random.Range(-30f, 30f), 0);
        }
        Random.state = oldState;
    }
}
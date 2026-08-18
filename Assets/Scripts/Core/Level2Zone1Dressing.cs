using UnityEngine;

/// Живая растительность Зоны 1 (Холмистая равнина, Z 0..40.5).
/// Вызывается из SceneBuilder.BuildRoutine() после BuildZone1_Hills().
public static class Level2Zone1Dressing
{
    static readonly string[] TreePrefabs = {
        "Assets/Pure Poly/Free Low Poly Nature Pack/Prefabs/PP_Tree_02.prefab",
        "Assets/Pure Poly/Free Low Poly Nature Pack/Prefabs/PP_Tree_10.prefab",
        "Assets/Pure Poly/Free Low Poly Nature Pack/Prefabs/PP_Birch_Tree_05.prefab",
        "Assets/Pure Poly/Free Low Poly Nature Pack/Prefabs/PP_Birch_Tree_06.prefab",
    };
    static readonly string[] MeadowPrefabs = {
        "Assets/Pure Poly/Free Low Poly Nature Pack/Prefabs/PP_Meadow_07.prefab",
        "Assets/Pure Poly/Free Low Poly Nature Pack/Prefabs/PP_Meadow_08.prefab",
    };
    static readonly string[] RockPrefabs = {
        "Assets/Pure Poly/Free Low Poly Nature Pack/Prefabs/PP_Rock_Moss_Grown_09.prefab",
        "Assets/Pure Poly/Free Low Poly Nature Pack/Prefabs/PP_Rock_Moss_Grown_11.prefab",
        "Assets/Pure Poly/Free Low Poly Nature Pack/Prefabs/PP_Rock_Pile_Forest_Moss_05.prefab",
    };

    static readonly Color GrassTint   = new Color(0.70f, 1.00f, 0.55f);
    static readonly Color RockTint    = new Color(0.65f, 0.80f, 0.55f);

    public static void Build(Transform decorRoot)
    {
        var root = new GameObject("Z1_Dressing").transform;
        root.SetParent(decorRoot);

        // ── ДЕРЕВЬЯ — левый фланг (путь не перекрываем, X < -7) ──────────────
        // Стараемся обойти Low Hill: center X=-8 Z=13, footprint X[-12..-4] Z[8..18]
        PlaceTree(-9f,   8f,  root, "Z1T_L01", 1.1f);
        PlaceTree(-13f,  6f,  root, "Z1T_L02", 1.3f);
        PlaceTree(-15f,  10f, root, "Z1T_L03", 1.2f);
        PlaceTree(-10f,  20f, root, "Z1T_L04", 1.0f);
        PlaceTree(-14f,  24f, root, "Z1T_L05", 1.4f);
        PlaceTree(-12f,  30f, root, "Z1T_L06", 1.1f);
        PlaceTree(-9f,   36f, root, "Z1T_L07", 1.2f);
        PlaceTree(-15f,  38f, root, "Z1T_L08", 1.0f);
        // фоновый ряд
        PlaceTree(-19f,  5f,  root, "Z1T_LF01", 1.5f);
        PlaceTree(-20f,  15f, root, "Z1T_LF02", 1.6f);
        PlaceTree(-19f,  27f, root, "Z1T_LF03", 1.5f);
        PlaceTree(-21f,  36f, root, "Z1T_LF04", 1.4f);

        // ── ДЕРЕВЬЯ — правый фланг (обходим High Hill: X[3.5..12.5] Z[21..31]) ─
        PlaceTree(10f,   8f,  root, "Z1T_R01", 1.2f);
        PlaceTree(13f,   7f,  root, "Z1T_R02", 1.1f);
        PlaceTree(14f,   14f, root, "Z1T_R03", 1.3f);
        PlaceTree(9f,    19f, root, "Z1T_R04", 1.0f);
        PlaceTree(14f,   33f, root, "Z1T_R05", 1.2f);
        PlaceTree(12f,   37f, root, "Z1T_R06", 1.1f);
        PlaceTree(10f,   40f, root, "Z1T_R07", 1.0f);
        // фоновый ряд
        PlaceTree(19f,   4f,  root, "Z1T_RF01", 1.5f);
        PlaceTree(21f,   13f, root, "Z1T_RF02", 1.6f);
        PlaceTree(20f,   24f, root, "Z1T_RF03", 1.4f);
        PlaceTree(19f,   35f, root, "Z1T_RF04", 1.5f);

        // ── ЛУГА (PP_Meadow) — декор между деревьями ─────────────────────────
        PlaceMeadow(-8f,   6f,  root, "Z1M_L01", 0.9f);
        PlaceMeadow(-11f,  22f, root, "Z1M_L02", 1.0f);
        PlaceMeadow(-10f,  32f, root, "Z1M_L03", 0.8f);
        PlaceMeadow(9f,    9f,  root, "Z1M_R01", 0.9f);
        PlaceMeadow(11f,   16f, root, "Z1M_R02", 1.0f);
        PlaceMeadow(10f,   35f, root, "Z1M_R03", 0.85f);
        // Z1M_C01 и Z1M_C02 убраны — перекрывали вход игроку

        // ── КАМНИ — фактура рельефа ───────────────────────────────────────────
        PlaceRock(-7f,  10f, root, "Z1R_01", 0.50f);
        PlaceRock(7f,   14f, root, "Z1R_02", 0.52f);
        PlaceRock(-9f,  28f, root, "Z1R_03", 0.48f);
        PlaceRock(8f,   36f, root, "Z1R_04", 0.50f);
        PlaceRock(-14f, 18f, root, "Z1R_05", 0.55f);
        PlaceRock(13f,  22f, root, "Z1R_06", 0.52f);

        // ── КУСТИКИ вдоль пути — мелкие PP_Meadow как трава/кусты ───────────
        PlaceMeadow(-3f, 12f, root, "Z1M_Bush01", 0.5f);
        PlaceMeadow( 4f, 18f, root, "Z1M_Bush02", 0.45f);
        PlaceMeadow(-6f, 25f, root, "Z1M_Bush03", 0.55f);
        PlaceMeadow( 7f, 30f, root, "Z1M_Bush04", 0.5f);
        PlaceMeadow(-2f, 35f, root, "Z1M_Bush05", 0.4f);
        PlaceMeadow( 3f,  6f, root, "Z1M_Bush06", 0.45f);
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    static void PlaceTree(float x, float z, Transform root, string id, float scale)
    {
        int idx = Mathf.Abs(id.GetHashCode()) % TreePrefabs.Length;
        var prefab = L0Util.LoadPrefab(TreePrefabs[idx]);
        if (prefab == null) return;
        float yRot = (id.GetHashCode() * 137) % 360f;
        var go = Object.Instantiate(prefab, new Vector3(x, 0f, z), Quaternion.Euler(0, yRot, 0), root);
        go.name = id;
        go.transform.localScale = Vector3.one * scale;
        L0Util.FixMaterials(go);
        L0Util.TintTree(go);
    }

    static void PlaceMeadow(float x, float z, Transform root, string id, float scale)
    {
        int idx = Mathf.Abs(id.GetHashCode()) % MeadowPrefabs.Length;
        var prefab = L0Util.LoadPrefab(MeadowPrefabs[idx]);
        if (prefab == null) return;
        float yRot = (id.GetHashCode() * 97) % 360f;
        var go = Object.Instantiate(prefab, new Vector3(x, 0f, z), Quaternion.Euler(0, yRot, 0), root);
        go.name = id;
        go.transform.localScale = Vector3.one * (scale * 0.16f);
        L0Util.FixMaterials(go);
        L0Util.TintMaterials(go, GrassTint);
        L0Util.RemoveAllColliders(go);
    }

    static void PlaceRock(float x, float z, Transform root, string id, float scale)
    {
        int idx = Mathf.Abs(id.GetHashCode()) % RockPrefabs.Length;
        var prefab = L0Util.LoadPrefab(RockPrefabs[idx]);
        if (prefab == null) return;
        float yRot = (id.GetHashCode() * 73) % 360f;
        var go = Object.Instantiate(prefab, new Vector3(x, 0.05f, z), Quaternion.Euler(0, yRot, 0), root);
        go.name = id;
        go.transform.localScale = Vector3.one * scale;
        L0Util.FixMaterials(go);
        L0Util.TintMaterials(go, RockTint);
    }
}

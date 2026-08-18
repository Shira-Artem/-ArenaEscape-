using UnityEngine;

/// <summary>
/// STEP 7.5 — Dark-fortress fortification clusters for the orc arena.
/// Builds a "П"-shaped cover nest (opening toward the battle center) out of ready-made
/// Dungeon (Low Poly Toon Battle Arena) prefabs: ruined stone walls, corner posts,
/// jail cells / barricades / rubble accents, a fire-lit cauldron and scattered detail.
///
/// Все куски — готовые URP-префабы, поставленные через L0Util.Place (чинит материалы,
/// СНИМАЕТ коллайдеры). Барьер-укрытие даёт ОДИН общий BoxCollider на корне кластера,
/// а не честные коллайдеры на каждый кусок.
///
/// Coordinate convention (from LEVEL0_VISUAL_PLAN cheatsheet):
///   f = unit vector anchor->BaseCenter (proём смотрит к центру)
///   s = Cross(up, f)  (right of f)
/// Walls: mesh ~2(X) x 2(Y) x 0.25(Z), pivot in the middle → bottom sits at anchor.y when
/// raised by halfH = 1*scale. LookRotation(f) puts the thin Z toward center (barrier), wide X along s.
/// </summary>
public static class L0DungeonFort
{
    const string Dungeon = "Assets/DungeonAssetPack/Prefabs/";
    static readonly string FlatWall = Dungeon + "StructureParts/Flat_Wall.prefab";
    static readonly string TyledWall = Dungeon + "StructureParts/Tyled_Wall.prefab";
    static readonly string CornerPost = Dungeon + "StructureParts/Stone_Wall_Corner.prefab";
    static readonly string JailWall = Dungeon + "StructureParts/Jail_Wall.prefab";
    static readonly string JailColumn = Dungeon + "StructureParts/Jail_Column.prefab";
    static readonly string Torch = Dungeon + "Props/Torch.prefab";
    static readonly string Cauldron = Dungeon + "Props/Big_Cauldron.prefab";
    static readonly string Shield = Dungeon + "Props/Decorative_Shield.prefab";
    static readonly string PlankPile = Dungeon + "Props/Plank_Pile.prefab";
    static readonly string BrickPile = Dungeon + "Props/Brick_Pile.prefab";
    static readonly string Barrel = Dungeon + "Props/Barrel.prefab";
    static readonly string BrokenColumn = Dungeon + "Props/Broken_Column.prefab";
    static readonly string Column = Dungeon + "Props/Column.prefab";
    static readonly string Stone = Dungeon + "Props/Stone.prefab";

    static readonly string FxFire = "Assets/Synty/PolygonGeneric/Prefabs/FX/FX_Fire_01.prefab";
    static readonly string Smoke = "Assets/msVFX_Free Smoke Effects Pack/Prefabs/msVFX_Stylized Smoke 2.prefab";
    static readonly string WeaponSpear = "Assets/New Folder/Low Poly Modular Medieval Weapons/Sample Weapons/spear-1.prefab";

    static readonly Color GroundDark = new Color(0.16f, 0.13f, 0.11f);
    static readonly Color FireWarm = new Color(1f, 0.55f, 0.18f);

    // Mesh half-extents at scale 1 (pivot centred): walls are ~2m tall.
    const float WallMeshHalfH = 1.0f;
    const float WallMeshW = 2.0f;

    /// <summary>
    /// One isolated test nest near the arena entrance (front-right sector, CampRing).
    /// Reserves through the guard just like the real ring so footprint behaves identically.
    /// </summary>
    public static void BuildTestCluster(Transform parent, bool createAtmosphere, L0OrcArenaPlacementGuard placementGuard)
    {
        Vector3 c = L0OrcArenaConfig.BaseCenter;
        Vector3 s = L0OrcArenaConfig.SideDir;
        Vector3 g = L0OrcArenaConfig.GateDir;
        Vector3 anchor = c + s * 38f + g * 42f; // ~(46,-133), radius ~57 → CampRing, clear of lanes

        const float footprint = 7.5f;
        if (placementGuard != null &&
            !placementGuard.TryReserve(L0OrcArenaPlacementCategory.Camp, "FORT_TEST", anchor, footprint, L0OrcArenaZone.CampRing))
        {
            Debug.LogWarning("[L0DungeonFort] Test cluster reservation rejected by guard.");
            return;
        }

        BuildCluster(parent, "FORT_TEST", anchor, 0, 1.2f, createAtmosphere);
    }

    /// <summary>
    /// Builds one fortification nest. variant%3 selects the accent: 0=jail cell, 1=barricade, 2=rubble.
    /// </summary>
    public static void BuildCluster(Transform parent, string id, Vector3 anchor, int variant, float scale, bool createAtmosphere)
    {
        GameObject root = new GameObject(id);
        root.transform.SetParent(parent, true);

        Vector3 f = L0OrcArenaConfig.BaseCenter - anchor;
        f.y = 0f;
        f = f.sqrMagnitude < 0.01f ? L0OrcArenaConfig.GateDir : f.normalized;
        Vector3 s = Vector3.Cross(Vector3.up, f).normalized; // right of f
        Quaternion faceCenter = Quaternion.LookRotation(f, Vector3.up);

        float halfH = WallMeshHalfH * scale;
        float wallW = WallMeshW * scale;
        Vector3 up = Vector3.up * halfH;
        float depth = wallW; // П depth ~ one wall width

        // --- Trampled dark ground patch under the nest (scaled to the cluster footprint) ---
        L0OrcArenaPrimitiveKit.CreateGroundPatchDecorative(
            root.transform, id + "_Ground_NoCollider", anchor + Vector3.up * 0.06f, faceCenter,
            new Vector3(wallW * 2.6f, 0.03f, wallW * 2.6f), GroundDark);

        // --- П frame: back wall + two side walls, opening toward center (+f) ---
        Vector3 backCenter = anchor - f * (depth * 0.5f);
        Place(TyledWall, backCenter + up, Quaternion.LookRotation(f, Vector3.up), scale, root.transform, id + "_BackWall");

        Vector3 leftBase = anchor - s * (wallW * 0.5f);
        Vector3 rightBase = anchor + s * (wallW * 0.5f);
        Place(FlatWall, leftBase + up, Quaternion.LookRotation(s, Vector3.up), scale, root.transform, id + "_SideWallL");
        // Right side wall is the jail cell on variant 0, so plain wall only when not jail.
        if (variant % 3 != 0)
            Place(FlatWall, rightBase + up, Quaternion.LookRotation(s, Vector3.up), scale, root.transform, id + "_SideWallR");

        // --- Corner posts at the two back corners (ruined silhouette) ---
        Place(CornerPost, backCenter - s * (wallW * 0.5f) + up, faceCenter, scale, root.transform, id + "_PostL");
        Place(CornerPost, backCenter + s * (wallW * 0.5f) + up, faceCenter, scale, root.transform, id + "_PostR");

        // --- Accent by variant ---
        switch (variant % 3)
        {
            case 0: BuildJailCell(root.transform, id, rightBase, f, s, scale, up); break;
            case 1: BuildBarricade(root.transform, id, anchor, f, s, scale); break;
            default: BuildRubble(root.transform, id, anchor, f, s, scale); break;
        }

        // --- Fire: cauldron + FX fire + warm light + smoke in a back corner (all scaled to the nest) ---
        if (createAtmosphere)
        {
            Vector3 firePos = backCenter + s * (wallW * 0.32f) + f * (depth * 0.18f);
            Place(Cauldron, firePos, faceCenter, scale * 0.9f, root.transform, id + "_Cauldron");
            Place(FxFire, firePos + Vector3.up * (0.9f * scale), Quaternion.identity, scale * 0.7f, root.transform, id + "_Fire");
            L0Util.PlaceSmoke(Smoke, firePos + Vector3.up * (1.4f * scale), scale * 0.35f, root.transform, id + "_Smoke");
            CreateWarmLight(root.transform, id + "_FireLight", firePos + Vector3.up * (1.2f * scale), scale * 4f, 2.6f);
        }

        // --- Detail: torch on a post, shield on the back wall, leaning spear, scattered stones ---
        Vector3 torchPostTop = backCenter - s * (wallW * 0.5f) + Vector3.up * (halfH * 2f - 0.1f * scale);
        Place(Torch, torchPostTop, Quaternion.LookRotation(-s, Vector3.up), scale, root.transform, id + "_Torch");
        CreateWarmLight(root.transform, id + "_TorchLight", torchPostTop + Vector3.up * (0.2f * scale), scale * 2.5f, 1.6f);

        Place(Shield, backCenter + f * (0.18f * scale) + Vector3.up * (halfH * 0.85f), faceCenter, scale * 0.9f,
            root.transform, id + "_Shield");
        Place(WeaponSpear, anchor + s * (wallW * 0.55f) + f * (depth * 0.35f),
            Quaternion.Euler(-68f, 40f + variant * 17f, 0f), scale * 0.7f, root.transform, id + "_Spear");
        Place(Stone, anchor - s * (wallW * 0.4f) + f * (depth * 0.3f), Quaternion.Euler(0f, variant * 33f, 0f),
            scale * 0.8f, root.transform, id + "_Stone1");
        Place(Stone, anchor + s * (wallW * 0.25f) + f * (depth * 0.45f), Quaternion.Euler(0f, variant * 71f, 12f),
            scale * 0.6f, root.transform, id + "_Stone2");

        // --- One shared barrier collider on the root (cover obstacle: orcs/player go around) ---
        float barrierH = 2.4f * scale;
        BoxCollider box = root.AddComponent<BoxCollider>();
        box.center = anchor + Vector3.up * (barrierH * 0.5f); // root at world origin → local==world
        box.size = new Vector3(wallW * 2.1f, barrierH, wallW * 2.1f);
    }

    static void BuildJailCell(Transform parent, string id, Vector3 rightBase, Vector3 f, Vector3 s, float scale, Vector3 up)
    {
        // Replace the right side wall with a barred jail cell (prisoner pocket).
        Place(JailColumn, rightBase - f * (1.0f * scale) + up, Quaternion.LookRotation(s, Vector3.up), scale, parent, id + "_JailColB");
        Place(JailColumn, rightBase + f * (1.0f * scale) + up, Quaternion.LookRotation(s, Vector3.up), scale, parent, id + "_JailColF");
        Place(JailWall, rightBase + up, Quaternion.LookRotation(s, Vector3.up), scale, parent, id + "_JailWall");
    }

    static void BuildBarricade(Transform parent, string id, Vector3 anchor, Vector3 f, Vector3 s, float scale)
    {
        // Low plank barricade across the opening + a couple of barrels.
        Place(PlankPile, anchor + f * (1.0f * scale), Quaternion.LookRotation(s, Vector3.up), scale * 0.85f, parent, id + "_Planks");
        Place(Barrel, anchor + f * (1.3f * scale) + s * (0.9f * scale), Quaternion.identity, scale, parent, id + "_Barrel1");
        Place(Barrel, anchor + f * (0.6f * scale) + s * (1.2f * scale), Quaternion.identity, scale * 0.9f, parent, id + "_Barrel2");
    }

    static void BuildRubble(Transform parent, string id, Vector3 anchor, Vector3 f, Vector3 s, float scale)
    {
        Place(BrokenColumn, anchor + f * (0.8f * scale) - s * (1.0f * scale), Quaternion.Euler(0f, 25f, 0f), scale, parent, id + "_BrokenCol");
        Place(BrickPile, anchor + f * (1.1f * scale) + s * (0.6f * scale), Quaternion.Euler(0f, 60f, 0f), scale * 0.7f, parent, id + "_Bricks");
        Place(Column, anchor + s * (1.3f * scale), Quaternion.Euler(74f, 30f, 0f), scale * 0.8f, parent, id + "_FallenCol");
    }

    static GameObject Place(string path, Vector3 pos, Quaternion rot, float scale, Transform parent, string name)
    {
        return L0Util.Place(path, pos, rot, scale, parent, name);
    }

    static void CreateWarmLight(Transform parent, string name, Vector3 pos, float range = 9f, float intensity = 2.2f)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, true);
        go.transform.position = pos;
        Light l = go.AddComponent<Light>();
        l.type = LightType.Point;
        l.color = FireWarm;
        l.range = range;
        l.intensity = intensity;
        l.shadows = LightShadows.None;
    }
}

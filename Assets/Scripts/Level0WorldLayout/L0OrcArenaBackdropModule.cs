using UnityEngine;

/// <summary>
/// Safe zero backdrop for the Level0 orc arena.
/// A6 pass: removes all foreground / tall mountain geometry from the arena view.
/// This module deliberately creates only far, low, no-collider horizon blockers, mist and small far silhouettes.
/// No large cliff, mountain, ridge or spire is allowed near the gate, arena, portal, palisade or set dressing.
/// </summary>
public static class L0OrcArenaBackdropModule
{
    private const float RearWallDistanceFromPortal = 310f;
    private const float SideWallDistanceFromCenter = 390f;

    // Горы перенесены в Level0ForestBuilder.BuildMountainRing (далеко от арены, scale 3-4)
    private static readonly string DeadTree = "Assets/Polytope Studio/Lowpoly_Environments/Prefabs/Trees/PT_Pine_Tree_03_dead.prefab";

    public static void Build(Transform parent, L0OrcArenaPlacementGuard placementGuard)
    {
        if (parent == null)
            return;

        Transform safeLowHorizon = L0OrcArenaPrimitiveKit.CreateGroup("BACKDROP_A6_SAFE_LowHorizonOnly", parent).transform;
        Transform horizonFog = L0OrcArenaPrimitiveKit.CreateGroup("BACKDROP_A6_SAFE_FogMistOnly", parent).transform;
        Transform farFence = L0OrcArenaPrimitiveKit.CreateGroup("BACKDROP_A6_SAFE_FarFenceOnly", parent).transform;
        Transform realAssets = L0OrcArenaPrimitiveKit.CreateGroup("BACKDROP_RealAssets", parent).transform;

        BuildLowHorizonWalls(safeLowHorizon);
        BuildHorizonFog(horizonFog);
        BuildFarFenceSilhouette(farFence);
        BuildRealAssetBackdrop(realAssets);

        if (L0OrcArenaConfig.showOrcArenaDebugMarkers)
            CreateDebugMarkers(parent);
    }

    private static void BuildLowHorizonWalls(Transform parent)
    {
        Vector3 center = L0OrcArenaConfig.BaseCenter;
        Vector3 portal = L0OrcArenaConfig.PortalShrine;
        Vector3 side = L0OrcArenaConfig.SideDir;
        Vector3 gate = L0OrcArenaConfig.GateDir;
        Vector3 back = L0OrcArenaConfig.BackDir;

        // Rear horizon: flat and very far. It blocks empty horizon without entering the playable arena silhouette.
        for (int i = 0; i < 9; i++)
        {
            float t = i / 8f;
            float x = Mathf.Lerp(-340f, 340f, t);
            float height = 4.2f + (i % 3) * 0.7f;
            Vector3 pos = portal + back * RearWallDistanceFromPortal + side * x + Vector3.up * (height * 0.5f);
            CreateLowWallSegment(parent, "BACKDROP_A6_SAFE_RearLowWall_" + i, pos, Quaternion.LookRotation(gate, Vector3.up), new Vector3(92f, height, 10f), i);
        }

        // Side horizons are far outside the arena. They are low slabs, not mountains.
        for (int i = 0; i < 7; i++)
        {
            float t = i / 6f;
            float z = Mathf.Lerp(60f, -205f, t);
            float height = 3.6f + (i % 2) * 0.8f;

            Vector3 leftPos = center + side * -SideWallDistanceFromCenter + gate * z + Vector3.up * (height * 0.5f);
            Vector3 rightPos = center + side * SideWallDistanceFromCenter + gate * z + Vector3.up * (height * 0.5f);
            CreateLowWallSegment(parent, "BACKDROP_A6_SAFE_LeftLowWall_" + i, leftPos, Quaternion.LookRotation(side, Vector3.up), new Vector3(82f, height, 10f), i + 20);
            CreateLowWallSegment(parent, "BACKDROP_A6_SAFE_RightLowWall_" + i, rightPos, Quaternion.LookRotation(-side, Vector3.up), new Vector3(82f, height, 10f), i + 40);
        }
    }

    private static void CreateLowWallSegment(Transform parent, string name, Vector3 position, Quaternion rotation, Vector3 size, int seed)
    {
        // Extra hard guard: if someone moves these numbers later and a segment enters the forbidden vista, skip it.
        float footprint = Mathf.Max(size.x, size.z) * 0.55f + 10f;
        if (!L0OrcArenaConfig.IsFootprintInsideZone(position, footprint, L0OrcArenaZone.DistantBackdrop))
            return;

        if (!L0OrcArenaConfig.IsBackdropFootprintOutsideVisualForbiddenZones(position, footprint))
            return;

        Transform root = L0OrcArenaPrimitiveKit.CreateGroup(name, parent).transform;
        root.position = position;
        root.rotation = rotation * Quaternion.Euler(0f, seed % 2 == 0 ? 1.5f : -1.5f, 0f);

        Color baseColor = seed % 2 == 0 ? L0OrcArenaMaterials.MountainFar : L0OrcArenaMaterials.Mountain;
        Color capColor = L0OrcArenaMaterials.MountainDark;

        L0OrcArenaPrimitiveKit.CreatePrimitive(root, "FlatFarMass_NoCollider", PrimitiveType.Cube, Vector3.zero, Quaternion.identity, size, baseColor, false);
        L0OrcArenaPrimitiveKit.CreatePrimitive(root, "SoftTop_NoCollider", PrimitiveType.Cube, new Vector3(0f, size.y * 0.52f, -0.35f), Quaternion.Euler(0f, 0f, seed % 2 == 0 ? 0.7f : -0.7f), new Vector3(size.x * 0.86f, size.y * 0.24f, size.z * 0.82f), capColor, false);
    }

    private static void BuildHorizonFog(Transform parent)
    {
        Vector3 center = L0OrcArenaConfig.BaseCenter;
        Vector3 portal = L0OrcArenaConfig.PortalShrine;
        Vector3 side = L0OrcArenaConfig.SideDir;
        Vector3 gate = L0OrcArenaConfig.GateDir;
        Vector3 back = L0OrcArenaConfig.BackDir;

        CreateMistBand(parent, "BACKDROP_A6_SAFE_RearMistBand", portal + back * 250f + Vector3.up * 4.4f, Quaternion.LookRotation(gate, Vector3.up), 620f, 6.8f, 0.45f, 0.20f);
        CreateMistBand(parent, "BACKDROP_A6_SAFE_LeftMistBand", center + side * -310f + gate * -65f + Vector3.up * 4.2f, Quaternion.LookRotation(side, Vector3.up), 310f, 5.6f, 0.45f, 0.15f);
        CreateMistBand(parent, "BACKDROP_A6_SAFE_RightMistBand", center + side * 310f + gate * -65f + Vector3.up * 4.2f, Quaternion.LookRotation(-side, Vector3.up), 310f, 5.6f, 0.45f, 0.15f);

        for (int i = 0; i < 10; i++)
        {
            float x = Mathf.Lerp(-260f, 260f, i / 9f);
            L0Props.CreateSmokePuff(portal + back * 205f + side * x + Vector3.up * 1.4f, 1.8f + (i % 3) * 0.25f, parent);
        }
    }

    private static void CreateMistBand(Transform parent, string name, Vector3 position, Quaternion rotation, float width, float height, float depth, float alpha)
    {
        Color c = L0OrcArenaMaterials.AshMist;
        c.a = Mathf.Clamp01(alpha);
        L0OrcArenaPrimitiveKit.CreatePrimitive(parent, name + "_NoCollider", PrimitiveType.Cube, position, rotation, new Vector3(width, height, depth), c, false, false, true);
    }

    private static void BuildFarFenceSilhouette(Transform parent)
    {
        Vector3 center = L0OrcArenaConfig.BaseCenter;
        Vector3 side = L0OrcArenaConfig.SideDir;
        Vector3 gate = L0OrcArenaConfig.GateDir;
        Vector3 back = L0OrcArenaConfig.BackDir;

        for (int i = 0; i < 6; i++)
        {
            float z = Mathf.Lerp(10f, -125f, i / 5f);
            CreateLowNoColliderFence(parent, "BACKDROP_A6_SAFE_LeftFarFence_" + i, center + side * -185f + gate * z, Quaternion.LookRotation(side, Vector3.up), 0.92f);
            CreateLowNoColliderFence(parent, "BACKDROP_A6_SAFE_RightFarFence_" + i, center + side * 185f + gate * z, Quaternion.LookRotation(-side, Vector3.up), 0.92f);
        }

        for (int i = 0; i < 6; i++)
        {
            float x = Mathf.Lerp(-110f, 110f, i / 5f);
            CreateLowNoColliderFence(parent, "BACKDROP_A6_SAFE_RearFarFence_" + i, center + back * 182f + side * x, Quaternion.LookRotation(gate, Vector3.up), 0.88f);
        }
    }

    private static void CreateLowNoColliderFence(Transform parent, string name, Vector3 position, Quaternion rotation, float scale)
    {
        float footprint = 7f * scale;
        if (!L0OrcArenaConfig.IsBackdropFootprintOutsideVisualForbiddenZones(position, footprint))
            return;

        Transform root = L0OrcArenaPrimitiveKit.CreateGroup(name, parent).transform;
        root.position = position;
        root.rotation = rotation;

        for (int i = 0; i < 4; i++)
        {
            float x = (i - 1.5f) * 1.18f * scale;
            float h = 2.15f * scale + (i % 2) * 0.28f * scale;
            L0OrcArenaPrimitiveKit.CreatePrimitive(root, "ShortStake_" + i + "_NoCollider", PrimitiveType.Cylinder, new Vector3(x, h * 0.5f, 0f), Quaternion.Euler(0f, 0f, i % 2 == 0 ? -3.5f : 3.5f), new Vector3(0.09f * scale, h * 0.5f, 0.09f * scale), L0OrcArenaMaterials.DarkWood, false);
        }

        L0OrcArenaPrimitiveKit.CreatePrimitive(root, "LowFenceRail_NoCollider", PrimitiveType.Cube, new Vector3(0f, 1.05f * scale, -0.04f), Quaternion.identity, new Vector3(5.2f * scale, 0.12f * scale, 0.15f * scale), L0OrcArenaMaterials.Wood, false);
        L0OrcArenaPrimitiveKit.CreatePrimitive(root, "SmallRedRag_NoCollider", PrimitiveType.Cube, new Vector3(0.65f * scale, 1.56f * scale, -0.09f), Quaternion.Euler(0f, 0f, -5f), new Vector3(1.25f * scale, 0.22f * scale, 0.05f * scale), L0OrcArenaMaterials.OrcRed, false);
    }

    private static void BuildRealAssetBackdrop(Transform parent)
    {
        Vector3 center = L0OrcArenaConfig.BaseCenter;
        Vector3 portal = L0OrcArenaConfig.PortalShrine;

        // Горы УБРАНЫ — теперь в Level0ForestBuilder.BuildMountainRing (далеко, scale 3-4)
        // Здесь только мёртвые деревья-силуэты ближе к арене
        PlaceDT(center.x - 70f, center.z + 40f, 2.5f, parent, "ArenaDT_01");
        PlaceDT(center.x - 75f, center.z - 10f, 2.8f, parent, "ArenaDT_02");
        PlaceDT(center.x - 68f, center.z - 55f, 2.3f, parent, "ArenaDT_03");
        PlaceDT(center.x + 70f, center.z + 35f, 2.5f, parent, "ArenaDT_04");
        PlaceDT(center.x + 78f, center.z - 15f, 2.7f, parent, "ArenaDT_05");
        PlaceDT(center.x + 72f, center.z - 50f, 2.4f, parent, "ArenaDT_06");
        PlaceDT(portal.x - 40f, portal.z - 20f, 2.2f, parent, "ArenaDT_07");
        PlaceDT(portal.x + 40f, portal.z - 25f, 2.3f, parent, "ArenaDT_08");
    }

    private static void PlaceDT(float x, float z, float scale, Transform parent, string name)
    {
        L0Util.Place(DeadTree, new Vector3(x, 0f, z), Quaternion.Euler(0, Random.Range(0f, 360f), 0), scale, parent, name);
    }

    private static void CreateDebugMarkers(Transform parent)
    {
        L0OrcArenaPrimitiveKit.CreateDebugMarker(parent, "RoadStart", L0OrcArenaConfig.RoadStart, Color.green);
        L0OrcArenaPrimitiveKit.CreateDebugMarker(parent, "BaseEntrance", L0OrcArenaConfig.BaseEntrance, Color.yellow);
        L0OrcArenaPrimitiveKit.CreateDebugMarker(parent, "BaseCenter", L0OrcArenaConfig.BaseCenter, Color.cyan);
        L0OrcArenaPrimitiveKit.CreateDebugMarker(parent, "PortalShrine", L0OrcArenaConfig.PortalShrine, Color.magenta);
    }
}

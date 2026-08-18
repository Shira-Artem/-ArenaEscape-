using UnityEngine;

/// <summary>
/// Reusable, arena-specific combat silhouettes. This module owns construction language;
/// placement and encounter composition remain the responsibility of CombatLayoutModule.
/// </summary>
public static class L0OrcArenaCombatPropsModule
{
    public static GameObject CreateClimbableWatchPlatform(Transform parent, string name, Vector3 position, Quaternion rotation)
    {
        GameObject root = CreateRoot(parent, name, position, rotation);
        const float deckHeight = 2.8f;

        CreateDeck(root.transform, new Vector2(5.8f, 5.2f), deckHeight);
        CreateWalkableRamp(root.transform, "WatchAccessRamp", new Vector3(0f, 0f, -10.1f), 3.0f, deckHeight, 7.5f);
        Primitive(root.transform, "BackShieldCover", PrimitiveType.Cube, new Vector3(0f, deckHeight + 0.85f, 2.30f), Quaternion.identity, new Vector3(5.25f, 1.7f, 0.48f), L0OrcArenaMaterials.DarkWood, true);
        Primitive(root.transform, "LeftWingCover", PrimitiveType.Cube, new Vector3(-2.55f, deckHeight + 0.58f, 0.85f), Quaternion.Euler(0f, 14f, 0f), new Vector3(0.42f, 1.16f, 2.6f), L0OrcArenaMaterials.Wood, true);
        Primitive(root.transform, "RightWingCover", PrimitiveType.Cube, new Vector3(2.55f, deckHeight + 0.58f, 0.85f), Quaternion.Euler(0f, -14f, 0f), new Vector3(0.42f, 1.16f, 2.6f), L0OrcArenaMaterials.Wood, true);
        Primitive(root.transform, "WatchRedBand_NoCollider", PrimitiveType.Cube, new Vector3(0f, deckHeight + 1.15f, 2.02f), Quaternion.identity, new Vector3(3.3f, 0.32f, 0.07f), L0OrcArenaMaterials.OrcRed, false);
        CreateTuskPair(root.transform, new Vector3(0f, deckHeight, 2.55f), 0.9f);
        return root;
    }

    public static GameObject CreateRampBarricadePost(Transform parent, string name, Vector3 position, Quaternion rotation)
    {
        GameObject root = CreateRoot(parent, name, position, rotation);
        const float deckHeight = 1.55f;

        CreateDeck(root.transform, new Vector2(6.4f, 4.2f), deckHeight);
        CreateWalkableRamp(root.transform, "BarricadeAccessRamp", new Vector3(0f, 0f, -7.8f), 3.4f, deckHeight, 5.8f);
        Primitive(root.transform, "LeftBarricade", PrimitiveType.Cube, new Vector3(-2.05f, deckHeight + 0.76f, 1.75f), Quaternion.Euler(0f, 10f, -3f), new Vector3(2.5f, 1.52f, 0.58f), L0OrcArenaMaterials.DarkWood, true);
        Primitive(root.transform, "RightBarricade", PrimitiveType.Cube, new Vector3(2.05f, deckHeight + 0.76f, 1.75f), Quaternion.Euler(0f, -10f, 3f), new Vector3(2.5f, 1.52f, 0.58f), L0OrcArenaMaterials.Wood, true);
        Primitive(root.transform, "CenterSightGapBeam_NoCollider", PrimitiveType.Cube, new Vector3(0f, deckHeight + 1.55f, 1.85f), Quaternion.identity, new Vector3(1.45f, 0.18f, 0.20f), L0OrcArenaMaterials.OrcRed, false);
        CreateSpikeFan(root.transform, new Vector3(-3.0f, 0f, 1.8f), 3, 0.78f);
        CreateSpikeFan(root.transform, new Vector3(3.0f, 0f, 1.8f), 3, 0.78f);
        return root;
    }

    public static GameObject CreateOrcCageStrongpoint(Transform parent, string name, Vector3 position, Quaternion rotation)
    {
        GameObject root = CreateRoot(parent, name, position, rotation);
        Primitive(root.transform, "CageBackCover", PrimitiveType.Cube, new Vector3(0f, 1.35f, 1.70f), Quaternion.identity, new Vector3(5.4f, 2.7f, 0.58f), L0OrcArenaMaterials.DarkWood, true);
        Primitive(root.transform, "CageLeftCover", PrimitiveType.Cube, new Vector3(-2.45f, 1.10f, 0f), Quaternion.identity, new Vector3(0.52f, 2.2f, 3.7f), L0OrcArenaMaterials.Wood, true);
        Primitive(root.transform, "CageRightCover", PrimitiveType.Cube, new Vector3(2.45f, 1.10f, 0f), Quaternion.identity, new Vector3(0.52f, 2.2f, 3.7f), L0OrcArenaMaterials.Wood, true);
        Primitive(root.transform, "CageLowFrontCover", PrimitiveType.Cube, new Vector3(0f, 0.55f, -1.70f), Quaternion.identity, new Vector3(5.4f, 1.10f, 0.52f), L0OrcArenaMaterials.DarkWood, true);

        for (int i = -2; i <= 2; i++)
            Primitive(root.transform, "CageBar_" + (i + 2) + "_NoCollider", PrimitiveType.Cylinder, new Vector3(i * 1.05f, 2.0f, -1.76f), Quaternion.identity, new Vector3(0.09f, 1.45f, 0.09f), L0OrcArenaMaterials.Bone, false);

        Primitive(root.transform, "CageTop_NoCollider", PrimitiveType.Cube, new Vector3(0f, 3.45f, 0f), Quaternion.identity, new Vector3(5.6f, 0.22f, 3.8f), L0OrcArenaMaterials.Wood, false);
        L0OrcArenaPrimitiveKit.CreateSideFlag(root.transform, "CageClaimFlag", root.transform.TransformPoint(new Vector3(-2.2f, 0f, 1.85f)), rotation, 0.85f);
        return root;
    }

    public static GameObject CreateSpikeBarricadeNest(Transform parent, string name, Vector3 position, Quaternion rotation)
    {
        GameObject root = CreateRoot(parent, name, position, rotation);
        Primitive(root.transform, "NestCenterCover", PrimitiveType.Cube, new Vector3(0f, 1.05f, 1.05f), Quaternion.identity, new Vector3(4.8f, 2.1f, 0.72f), L0OrcArenaMaterials.DarkWood, true);
        Primitive(root.transform, "NestLeftCover", PrimitiveType.Cube, new Vector3(-3.0f, 0.82f, 0f), Quaternion.Euler(0f, 38f, 0f), new Vector3(3.5f, 1.64f, 0.62f), L0OrcArenaMaterials.Wood, true);
        Primitive(root.transform, "NestRightCover", PrimitiveType.Cube, new Vector3(3.0f, 0.82f, 0f), Quaternion.Euler(0f, -38f, 0f), new Vector3(3.5f, 1.64f, 0.62f), L0OrcArenaMaterials.Wood, true);
        CreateSpikeFan(root.transform, new Vector3(-3.9f, 0f, 1.0f), 4, 0.92f);
        CreateSpikeFan(root.transform, new Vector3(0f, 0f, 1.75f), 5, 1.0f);
        CreateSpikeFan(root.transform, new Vector3(3.9f, 0f, 1.0f), 4, 0.92f);
        return root;
    }

    public static GameObject CreateBoneShieldWall(Transform parent, string name, Vector3 position, Quaternion rotation)
    {
        GameObject root = CreateRoot(parent, name, position, rotation);
        for (int i = -2; i <= 2; i++)
        {
            float x = i * 1.45f;
            float height = i == 0 ? 1.65f : 1.35f;
            Primitive(root.transform, "ShieldBody_" + (i + 2), PrimitiveType.Cylinder, new Vector3(x, height * 0.56f, 0f), Quaternion.Euler(90f, 0f, 0f), new Vector3(0.78f, 0.24f, height * 0.56f), i % 2 == 0 ? L0OrcArenaMaterials.DarkWood : L0OrcArenaMaterials.Wood, true);
            Primitive(root.transform, "ShieldBoneRib_" + (i + 2) + "_NoCollider", PrimitiveType.Cube, new Vector3(x, height * 0.58f, -0.28f), Quaternion.Euler(0f, 0f, i * 2f), new Vector3(0.16f, height * 0.88f, 0.10f), L0OrcArenaMaterials.Bone, false);
        }

        Primitive(root.transform, "WallTopBeam", PrimitiveType.Cube, new Vector3(0f, 1.52f, 0.12f), Quaternion.Euler(0f, 0f, -2f), new Vector3(7.8f, 0.24f, 0.38f), L0OrcArenaMaterials.DarkWood, true);
        return root;
    }

    public static GameObject CreateCrateCoverCluster(Transform parent, string name, Vector3 position, Quaternion rotation)
    {
        GameObject root = CreateRoot(parent, name, position, rotation);
        Primitive(root.transform, "LongSupplyChest", PrimitiveType.Cube, new Vector3(-0.8f, 0.65f, 0.35f), Quaternion.Euler(0f, -8f, 0f), new Vector3(4.2f, 1.30f, 1.75f), L0OrcArenaMaterials.Wood, true);
        Primitive(root.transform, "TallCrate", PrimitiveType.Cube, new Vector3(-1.65f, 1.80f, 0.15f), Quaternion.Euler(0f, 7f, 0f), new Vector3(1.55f, 2.20f, 1.55f), L0OrcArenaMaterials.DarkWood, true);
        Primitive(root.transform, "LowCrate", PrimitiveType.Cube, new Vector3(1.65f, 0.72f, -0.45f), Quaternion.Euler(0f, -12f, 0f), new Vector3(1.85f, 1.44f, 1.65f), L0OrcArenaMaterials.Wood, true);
        Primitive(root.transform, "SupplyTusk_NoCollider", PrimitiveType.Cylinder, new Vector3(0.45f, 1.55f, 0.55f), Quaternion.Euler(0f, 0f, 70f), new Vector3(0.14f, 1.15f, 0.14f), L0OrcArenaMaterials.Bone, false);
        return root;
    }

    public static GameObject CreateRitualPitCenterpiece(Transform parent, string name, Vector3 position, Quaternion rotation)
    {
        GameObject root = CreateRoot(parent, name, position, rotation);
        Primitive(root.transform, "RitualAsh_NoCollider", PrimitiveType.Cylinder, new Vector3(0f, 0.08f, 0f), Quaternion.identity, new Vector3(15f, 0.04f, 15f), L0OrcArenaMaterials.ArenaInner, false);
        Primitive(root.transform, "RitualEmberRing_NoCollider", PrimitiveType.Cylinder, new Vector3(0f, 0.13f, 0f), Quaternion.identity, new Vector3(9.5f, 0.025f, 9.5f), L0OrcArenaMaterials.OrcRed, false, true);
        Primitive(root.transform, "RitualPit_NoCollider", PrimitiveType.Cylinder, new Vector3(0f, 0.18f, 0f), Quaternion.identity, new Vector3(5.2f, 0.05f, 5.2f), L0OrcArenaMaterials.DarkStone, false);

        // Low diagonal stones break sightlines but keep the gate-to-portal axis open.
        CreatePitStone(root.transform, "PitStone_FL", new Vector3(-6.2f, 0f, -4.8f), 24f);
        CreatePitStone(root.transform, "PitStone_FR", new Vector3(6.2f, 0f, -4.8f), -24f);
        CreatePitStone(root.transform, "PitStone_BL", new Vector3(-6.2f, 0f, 4.8f), -24f);
        CreatePitStone(root.transform, "PitStone_BR", new Vector3(6.2f, 0f, 4.8f), 24f);
        CreateTuskPair(root.transform, new Vector3(0f, 0f, 4.2f), 1.15f);
        return root;
    }

    public static GameObject CreatePortalApproachBlockade(Transform parent, string name, Vector3 position, Quaternion rotation, float clearWidth)
    {
        clearWidth = Mathf.Max(12f, clearWidth);
        GameObject root = CreateRoot(parent, name, position, rotation);
        float sideOffset = clearWidth * 0.5f + 4.2f;

        CreateBoneShieldWall(root.transform, "PortalBlockade_Left", root.transform.TransformPoint(new Vector3(-sideOffset, 0f, 0f)), rotation * Quaternion.Euler(0f, 18f, 0f));
        CreateBoneShieldWall(root.transform, "PortalBlockade_Right", root.transform.TransformPoint(new Vector3(sideOffset, 0f, 0f)), rotation * Quaternion.Euler(0f, -18f, 0f));
        CreateSpikeFan(root.transform, new Vector3(-sideOffset - 4.0f, 0f, 0.6f), 4, 1f);
        CreateSpikeFan(root.transform, new Vector3(sideOffset + 4.0f, 0f, 0.6f), 4, 1f);
        L0OrcArenaPrimitiveKit.CreateSideFlag(root.transform, "PortalBlockadeFlag_L", root.transform.TransformPoint(new Vector3(-sideOffset, 0f, 1.2f)), rotation, 0.95f);
        L0OrcArenaPrimitiveKit.CreateSideFlag(root.transform, "PortalBlockadeFlag_R", root.transform.TransformPoint(new Vector3(sideOffset, 0f, 1.2f)), rotation, 0.95f);
        return root;
    }

    private static GameObject CreateRoot(Transform parent, string name, Vector3 position, Quaternion rotation)
    {
        GameObject root = L0OrcArenaPrimitiveKit.CreateGroup(name, parent);
        root.transform.position = position;
        root.transform.rotation = rotation;
        return root;
    }

    private static void CreateDeck(Transform parent, Vector2 size, float height)
    {
        Primitive(parent, "WalkableDeck", PrimitiveType.Cube, new Vector3(0f, height, 0f), Quaternion.identity, new Vector3(size.x, 0.34f, size.y), L0OrcArenaMaterials.Wood, true);
        float x = size.x * 0.40f;
        float z = size.y * 0.38f;
        Primitive(parent, "DeckSupport_FL", PrimitiveType.Cylinder, new Vector3(-x, height * 0.5f, z), Quaternion.identity, new Vector3(0.18f, height * 0.5f, 0.18f), L0OrcArenaMaterials.DarkWood, true);
        Primitive(parent, "DeckSupport_FR", PrimitiveType.Cylinder, new Vector3(x, height * 0.5f, z), Quaternion.identity, new Vector3(0.18f, height * 0.5f, 0.18f), L0OrcArenaMaterials.DarkWood, true);
        Primitive(parent, "DeckSupport_BL", PrimitiveType.Cylinder, new Vector3(-x, height * 0.5f, -z), Quaternion.identity, new Vector3(0.18f, height * 0.5f, 0.18f), L0OrcArenaMaterials.DarkWood, true);
        Primitive(parent, "DeckSupport_BR", PrimitiveType.Cylinder, new Vector3(x, height * 0.5f, -z), Quaternion.identity, new Vector3(0.18f, height * 0.5f, 0.18f), L0OrcArenaMaterials.DarkWood, true);
    }

    private static void CreateWalkableRamp(Transform parent, string name, Vector3 bottom, float width, float rise, float run)
    {
        GameObject ramp = L0OrcArenaPrimitiveKit.CreateGroup(name, parent);
        ramp.transform.localPosition = bottom;
        ramp.transform.localRotation = Quaternion.identity;
        ramp.transform.localScale = Vector3.one;
        float angle = Mathf.Atan2(rise, run) * Mathf.Rad2Deg;
        float length = Mathf.Sqrt(rise * rise + run * run);
        Primitive(ramp.transform, "WalkableRamp", PrimitiveType.Cube, new Vector3(0f, rise * 0.5f, run * 0.5f), Quaternion.Euler(-angle, 0f, 0f), new Vector3(width, 0.30f, length), L0OrcArenaMaterials.Wood, true);
        Primitive(ramp.transform, "RampLeftRail_NoCollider", PrimitiveType.Cube, new Vector3(-width * 0.48f, rise * 0.5f + 0.18f, run * 0.5f), Quaternion.Euler(-angle, 0f, 0f), new Vector3(0.12f, 0.20f, length), L0OrcArenaMaterials.DarkWood, false);
        Primitive(ramp.transform, "RampRightRail_NoCollider", PrimitiveType.Cube, new Vector3(width * 0.48f, rise * 0.5f + 0.18f, run * 0.5f), Quaternion.Euler(-angle, 0f, 0f), new Vector3(0.12f, 0.20f, length), L0OrcArenaMaterials.DarkWood, false);
    }

    private static void CreatePitStone(Transform parent, string name, Vector3 localPosition, float yaw)
    {
        Primitive(parent, name, PrimitiveType.Cube, localPosition + Vector3.up * 0.62f, Quaternion.Euler(0f, yaw, -4f), new Vector3(3.6f, 1.24f, 1.05f), L0OrcArenaMaterials.DarkStone, true);
    }

    private static void CreateSpikeFan(Transform parent, Vector3 localPosition, int count, float scale)
    {
        for (int i = 0; i < count; i++)
        {
            float centered = i - (count - 1) * 0.5f;
            Primitive(parent, "Spike_NoCollider_" + localPosition.x + "_" + i, PrimitiveType.Cylinder, localPosition + new Vector3(centered * 0.48f * scale, 1.05f * scale, centered * 0.12f), Quaternion.Euler(58f, 0f, centered * 7f), new Vector3(0.11f * scale, 1.35f * scale, 0.11f * scale), L0OrcArenaMaterials.Bone, false);
        }
    }

    private static void CreateTuskPair(Transform parent, Vector3 localPosition, float scale)
    {
        Primitive(parent, "Tusk_L_NoCollider", PrimitiveType.Cylinder, localPosition + new Vector3(-0.65f * scale, 1.25f * scale, 0f), Quaternion.Euler(0f, 0f, -18f), new Vector3(0.13f * scale, 1.25f * scale, 0.13f * scale), L0OrcArenaMaterials.Bone, false);
        Primitive(parent, "Tusk_R_NoCollider", PrimitiveType.Cylinder, localPosition + new Vector3(0.65f * scale, 1.25f * scale, 0f), Quaternion.Euler(0f, 0f, 18f), new Vector3(0.13f * scale, 1.25f * scale, 0.13f * scale), L0OrcArenaMaterials.Bone, false);
    }

    private static GameObject Primitive(Transform parent, string name, PrimitiveType type, Vector3 position, Quaternion rotation, Vector3 scale, Color color, bool collider, bool emissive = false)
    {
        return L0OrcArenaPrimitiveKit.CreatePrimitive(parent, name, type, position, rotation, scale, color, collider, emissive);
    }
}

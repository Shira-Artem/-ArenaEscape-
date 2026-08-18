using UnityEngine;

/// <summary>
/// Authored combat composition for the Level0 orc arena. It owns all primary
/// strongpoints, climbable positions, combat cover and the central ritual landmark.
/// </summary>
public static class L0OrcArenaCombatLayoutModule
{
    private const string EntranceLeftId = "Combat_EntranceLeftRampPost";
    private const string EntranceRightId = "Combat_EntranceRightRampPost";
    private const string WatchLeftId = "Combat_CentralLeftWatchPost";
    private const string WatchRightId = "Combat_CentralRightWatchPost";
    private const string CageLeftId = "Combat_CentralLeftCagePost";
    private const string SpikeRightId = "Combat_CentralRightSpikePost";
    private const string RearLeftId = "Combat_RearLeftAssaultPost";
    private const string RearRightId = "Combat_RearRightAssaultPost";

    /// <summary>
    /// Planning happens before visual modules build so Camp and SetDressing can never
    /// claim combat footprints even though their geometry is instantiated earlier/later.
    /// </summary>
    public static void ReserveLayout(L0OrcArenaPlacementGuard guard)
    {
        if (guard == null)
            return;

        Reserve(guard, EntranceLeftId, L0OrcArenaConfig.EntranceLeftRampPost, L0OrcArenaConfig.RampStrongpointRadius);
        Reserve(guard, EntranceRightId, L0OrcArenaConfig.EntranceRightRampPost, L0OrcArenaConfig.RampStrongpointRadius);
        Reserve(guard, WatchLeftId, L0OrcArenaConfig.CentralLeftWatchPost, L0OrcArenaConfig.WatchStrongpointRadius);
        Reserve(guard, WatchRightId, L0OrcArenaConfig.CentralRightWatchPost, L0OrcArenaConfig.WatchStrongpointRadius);
        Reserve(guard, CageLeftId, L0OrcArenaConfig.CentralLeftCagePost, L0OrcArenaConfig.MidStrongpointRadius);
        Reserve(guard, SpikeRightId, L0OrcArenaConfig.CentralRightSpikePost, L0OrcArenaConfig.MidStrongpointRadius);
        Reserve(guard, RearLeftId, L0OrcArenaConfig.RearLeftAssaultPost, L0OrcArenaConfig.RearStrongpointRadius);
        Reserve(guard, RearRightId, L0OrcArenaConfig.RearRightAssaultPost, L0OrcArenaConfig.RearStrongpointRadius);
    }

    public static void Build(Transform parent, L0OrcArenaPlacementGuard guard)
    {
        if (parent == null || guard == null)
            return;

        BuildSectorLanguage(parent);
        L0OrcArenaCombatPropsModule.CreateRitualPitCenterpiece(
            parent,
            "COMBAT_CENTER_RitualPit",
            L0OrcArenaConfig.CentralRitualPitAnchor,
            L0OrcArenaConfig.FaceBack);

        BuildEntranceLeft(parent, guard);
        BuildEntranceRight(parent, guard);
        BuildWatchLeft(parent, guard);
        BuildWatchRight(parent, guard);
        BuildCageLeft(parent, guard);
        BuildSpikeRight(parent, guard);
        BuildRearLeft(parent, guard);
        BuildRearRight(parent, guard);
    }

    private static void BuildEntranceLeft(Transform parent, L0OrcArenaPlacementGuard guard)
    {
        if (!guard.HasReservation(EntranceLeftId))
            return;

        Vector3 anchor = L0OrcArenaConfig.EntranceLeftRampPost;
        Transform root = StrongpointRoot(parent, "STRONGPOINT_01_EntranceLeftRamp", anchor);
        Quaternion faceCenter = FaceArena(anchor);
        L0OrcArenaCombatPropsModule.CreateRampBarricadePost(root, "Primary_ClimbableRampBarricade", anchor, faceCenter);
        L0OrcArenaCombatPropsModule.CreateCrateCoverCluster(root, "Support_CrateCover", anchor + L0OrcArenaConfig.SideDir * -6.8f + L0OrcArenaConfig.GateDir * 1.5f, faceCenter * Quaternion.Euler(0f, 22f, 0f));
    }

    private static void BuildEntranceRight(Transform parent, L0OrcArenaPlacementGuard guard)
    {
        if (!guard.HasReservation(EntranceRightId))
            return;

        Vector3 anchor = L0OrcArenaConfig.EntranceRightRampPost;
        Transform root = StrongpointRoot(parent, "STRONGPOINT_02_EntranceRightRamp", anchor);
        Quaternion faceCenter = FaceArena(anchor);
        L0OrcArenaCombatPropsModule.CreateRampBarricadePost(root, "Primary_ClimbableRampBarricade", anchor, faceCenter);
        L0OrcArenaCombatPropsModule.CreateBoneShieldWall(root, "Support_BoneShieldCover", anchor + L0OrcArenaConfig.SideDir * 6.4f + L0OrcArenaConfig.GateDir * 1.2f, faceCenter * Quaternion.Euler(0f, -24f, 0f));
    }

    private static void BuildWatchLeft(Transform parent, L0OrcArenaPlacementGuard guard)
    {
        if (!guard.HasReservation(WatchLeftId))
            return;

        Vector3 anchor = L0OrcArenaConfig.CentralLeftWatchPost;
        Transform root = StrongpointRoot(parent, "STRONGPOINT_03_CentralLeftWatch", anchor);
        Quaternion faceCenter = FaceArena(anchor);
        L0OrcArenaCombatPropsModule.CreateClimbableWatchPlatform(root, "Primary_ClimbableWatchPlatform", anchor, faceCenter);
        L0OrcArenaCombatPropsModule.CreateBoneShieldWall(root, "Support_LowShieldCover", anchor + L0OrcArenaConfig.GateDir * 7.1f, faceCenter * Quaternion.Euler(0f, 18f, 0f));
    }

    private static void BuildWatchRight(Transform parent, L0OrcArenaPlacementGuard guard)
    {
        if (!guard.HasReservation(WatchRightId))
            return;

        Vector3 anchor = L0OrcArenaConfig.CentralRightWatchPost;
        Transform root = StrongpointRoot(parent, "STRONGPOINT_04_CentralRightWatch", anchor);
        Quaternion faceCenter = FaceArena(anchor);
        L0OrcArenaCombatPropsModule.CreateClimbableWatchPlatform(root, "Primary_ClimbableWatchPlatform", anchor, faceCenter);
        L0OrcArenaCombatPropsModule.CreateCrateCoverCluster(root, "Support_SupplyCover", anchor + L0OrcArenaConfig.GateDir * 7.0f, faceCenter * Quaternion.Euler(0f, -18f, 0f));
    }

    private static void BuildCageLeft(Transform parent, L0OrcArenaPlacementGuard guard)
    {
        if (!guard.HasReservation(CageLeftId))
            return;

        Vector3 anchor = L0OrcArenaConfig.CentralLeftCagePost;
        Transform root = StrongpointRoot(parent, "STRONGPOINT_05_CentralLeftCage", anchor);
        Quaternion faceCenter = FaceArena(anchor);
        L0OrcArenaCombatPropsModule.CreateOrcCageStrongpoint(root, "Primary_OrcCageStrongpoint", anchor, faceCenter);
        L0OrcArenaCombatPropsModule.CreateBoneShieldWall(root, "Support_CageShieldWall", anchor + L0OrcArenaConfig.SideDir * -6.7f, faceCenter * Quaternion.Euler(0f, 28f, 0f));
    }

    private static void BuildSpikeRight(Transform parent, L0OrcArenaPlacementGuard guard)
    {
        if (!guard.HasReservation(SpikeRightId))
            return;

        Vector3 anchor = L0OrcArenaConfig.CentralRightSpikePost;
        Transform root = StrongpointRoot(parent, "STRONGPOINT_06_CentralRightSpikeNest", anchor);
        Quaternion faceCenter = FaceArena(anchor);
        L0OrcArenaCombatPropsModule.CreateSpikeBarricadeNest(root, "Primary_SpikeBarricadeNest", anchor, faceCenter);
        L0OrcArenaCombatPropsModule.CreateCrateCoverCluster(root, "Support_NestCrates", anchor + L0OrcArenaConfig.SideDir * 6.7f, faceCenter * Quaternion.Euler(0f, -26f, 0f));
    }

    private static void BuildRearLeft(Transform parent, L0OrcArenaPlacementGuard guard)
    {
        if (!guard.HasReservation(RearLeftId))
            return;

        Vector3 anchor = L0OrcArenaConfig.RearLeftAssaultPost;
        Transform root = StrongpointRoot(parent, "STRONGPOINT_07_RearLeftAssault", anchor);
        Quaternion faceCenter = FaceArena(anchor);
        L0OrcArenaCombatPropsModule.CreateBoneShieldWall(root, "Primary_BoneShieldWall", anchor, faceCenter);
        L0OrcArenaCombatPropsModule.CreateCrateCoverCluster(root, "Support_AssaultSupplies", anchor + L0OrcArenaConfig.SideDir * -5.6f + L0OrcArenaConfig.GateDir * 4.2f, faceCenter * Quaternion.Euler(0f, 32f, 0f));
        L0OrcArenaPrimitiveKit.CreateSideFlag(root, "RearAssaultFlag", anchor + L0OrcArenaConfig.BackDir * 3.4f, faceCenter, 1.15f);
    }

    private static void BuildRearRight(Transform parent, L0OrcArenaPlacementGuard guard)
    {
        if (!guard.HasReservation(RearRightId))
            return;

        Vector3 anchor = L0OrcArenaConfig.RearRightAssaultPost;
        Transform root = StrongpointRoot(parent, "STRONGPOINT_08_RearRightAssault", anchor);
        Quaternion faceCenter = FaceArena(anchor);
        L0OrcArenaCombatPropsModule.CreateSpikeBarricadeNest(root, "Primary_AssaultSpikeNest", anchor, faceCenter);
        L0OrcArenaCombatPropsModule.CreateBoneShieldWall(root, "Support_AssaultShieldWall", anchor + L0OrcArenaConfig.SideDir * 5.8f + L0OrcArenaConfig.GateDir * 4.0f, faceCenter * Quaternion.Euler(0f, -32f, 0f));
        L0OrcArenaPrimitiveKit.CreateSideFlag(root, "RearAssaultFlag", anchor + L0OrcArenaConfig.BackDir * 3.4f, faceCenter, 1.15f);
    }

    private static void BuildSectorLanguage(Transform parent)
    {
        L0OrcArenaPrimitiveKit.CreatePrimitive(parent, "SECTOR_EntranceCombatThreshold_NoCollider", PrimitiveType.Cylinder, L0OrcArenaConfig.EntranceSectorCenter + Vector3.up * 0.08f, Quaternion.identity, new Vector3(40f, 0.018f, 28f), L0OrcArenaMaterials.GateGround, false);
        L0OrcArenaPrimitiveKit.CreatePrimitive(parent, "SECTOR_CentralCombatField_NoCollider", PrimitiveType.Cylinder, L0OrcArenaConfig.CentralCombatSectorCenter + Vector3.up * 0.075f, Quaternion.identity, new Vector3(72f, 0.015f, 58f), L0OrcArenaMaterials.ArenaInner, false);
        L0OrcArenaPrimitiveKit.CreatePrimitive(parent, "SECTOR_RearShrineThreshold_NoCollider", PrimitiveType.Cylinder, L0OrcArenaConfig.RearPortalSectorCenter + Vector3.up * 0.08f, Quaternion.identity, new Vector3(44f, 0.018f, 30f), L0OrcArenaMaterials.GateGround, false);
    }

    private static void Reserve(L0OrcArenaPlacementGuard guard, string id, Vector3 center, float radius)
    {
        guard.TryReserve(L0OrcArenaPlacementCategory.Combat, id, center, radius, L0OrcArenaZone.CombatStrongpointRing);
    }

    private static Transform StrongpointRoot(Transform parent, string name, Vector3 anchor)
    {
        GameObject root = L0OrcArenaPrimitiveKit.CreateGroup(name, parent);
        root.transform.position = Vector3.zero;

        if (L0OrcArenaConfig.showOrcArenaDebugMarkers)
            L0OrcArenaPrimitiveKit.CreateDebugMarker(root.transform, name, anchor, new Color(0.95f, 0.18f, 0.05f), 0.72f);

        return root.transform;
    }

    private static Quaternion FaceArena(Vector3 position)
    {
        Vector3 direction = L0OrcArenaConfig.BaseCenter - position;
        direction.y = 0f;
        return direction.sqrMagnitude > 0.01f ? Quaternion.LookRotation(direction, Vector3.up) : L0OrcArenaConfig.FaceGate;
    }
}

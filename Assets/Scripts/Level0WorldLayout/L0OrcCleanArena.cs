using UnityEngine;

/// <summary>
/// Orchestrates the modular clean orc arena for Level0.
/// Modules own gate, camp shell, primary combat layout, secondary dressing,
/// portal shrine and backdrop details.
/// </summary>
public static class L0OrcCleanArena
{
    public static void Build(GameObject container, bool createAtmosphere, bool createAudio)
    {
        if (container == null)
            return;

        GameObject root = L0OrcArenaPrimitiveKit.CreateGroup(L0OrcArenaConfig.RootName, container.transform);
        Transform gateZone = L0OrcArenaPrimitiveKit.CreateGroup("GateZone", root.transform).transform;
        Transform battleZone = L0OrcArenaPrimitiveKit.CreateGroup("BattleZone", root.transform).transform;
        Transform outerCampRing = L0OrcArenaPrimitiveKit.CreateGroup("OuterCampRing", root.transform).transform;
        Transform combatLayout = L0OrcArenaPrimitiveKit.CreateGroup("CombatLayout", root.transform).transform;
        Transform secondarySetDressing = L0OrcArenaPrimitiveKit.CreateGroup("SecondarySetDressing", root.transform).transform;
        Transform portalShrineZone = L0OrcArenaPrimitiveKit.CreateGroup("PortalShrineZone", root.transform).transform;
        Transform backgroundFrame = L0OrcArenaPrimitiveKit.CreateGroup("BackgroundFrame", root.transform).transform;

        L0OrcArenaPlacementGuard placementGuard = CreatePlacementGuard();
        L0OrcArenaCombatLayoutModule.ReserveLayout(placementGuard);

        // Visual build order: Gate -> Camp -> CombatLayout -> SetDressing -> Portal -> Backdrop.
        // Combat footprints are planned above so the earlier Camp pass still respects them.
        L0OrcArenaGateModule.Build(gateZone, outerCampRing, createAtmosphere);
        L0OrcArenaCampModule.Build(battleZone, outerCampRing, createAtmosphere, placementGuard);
        L0OrcArenaCombatLayoutModule.Build(combatLayout, placementGuard);
        L0OrcArenaSetDressingModule.Build(secondarySetDressing, createAtmosphere, placementGuard);
        L0OrcArenaPortalModule.Build(portalShrineZone, createAtmosphere);
        L0OrcArenaBackdropModule.Build(backgroundFrame, placementGuard);

        ClearPlayableCorridors(root.transform);

        if (createAtmosphere)
            L0Props.CreatePointLight("CleanArena_CenterFire", L0OrcArenaConfig.BaseCenter + Vector3.up * 2.5f, L0OrcArenaMaterials.Fire, 1.2f, 14f, battleZone);

        if (createAudio)
            L0Audio.CreateOrcVillageSoundscape(L0OrcArenaConfig.BaseCenter, L0OrcArenaCampModule.GetFireAudioPoints(), root.transform);

        L0OrcArenaEnhancement.Build(root.transform);
    }

    private static L0OrcArenaPlacementGuard CreatePlacementGuard()
    {
        L0OrcArenaPlacementGuard guard = new L0OrcArenaPlacementGuard();

        guard.ReserveCriticalCircle(
            "Critical_BattleClearArea",
            L0OrcArenaConfig.BaseCenter,
            L0OrcArenaConfig.BattleCenterMaxRadius);

        guard.ReserveCriticalLane(
            "Critical_RoadLane_StartToMid",
            L0OrcArenaConfig.RoadStart,
            L0OrcArenaConfig.RoadMid,
            L0OrcArenaConfig.RoadWidth * 0.5f + L0OrcArenaConfig.RoadCorridorMargin);

        guard.ReserveCriticalLane(
            "Critical_RoadLane_MidToEntrance",
            L0OrcArenaConfig.RoadMid,
            L0OrcArenaConfig.BaseEntrance,
            L0OrcArenaConfig.RoadWidth * 0.5f + L0OrcArenaConfig.RoadCorridorMargin);

        guard.ReserveCriticalLane(
            "Critical_EntranceLane",
            L0OrcArenaConfig.GetEntranceLaneStart(),
            L0OrcArenaConfig.GetEntranceLaneEnd(),
            L0OrcArenaConfig.EntranceClearWidth * 0.5f + L0OrcArenaConfig.EntranceCorridorMargin);

        guard.ReserveCriticalLane(
            "Critical_PortalApproachLane",
            L0OrcArenaConfig.GetPortalLaneStart(),
            L0OrcArenaConfig.GetPortalLaneEnd(),
            L0OrcArenaConfig.RoadWidth * 0.5f + L0OrcArenaConfig.PortalCorridorMargin);

        guard.ReserveCriticalCircle(
            "Critical_PortalShrine",
            L0OrcArenaConfig.PortalShrine,
            L0OrcArenaConfig.PortalShrineRadius);

        return guard;
    }

    private static void ClearPlayableCorridors(Transform root)
    {
        L0OrcArenaPrimitiveKit.ClearTallCollidersInCorridor(
            root,
            L0OrcArenaConfig.BaseEntrance + L0OrcArenaConfig.GateDir * L0OrcArenaConfig.EntranceForwardClear,
            L0OrcArenaConfig.BaseCenter + L0OrcArenaConfig.GateDir * 4f,
            L0OrcArenaConfig.EntranceClearWidth * 0.5f,
            0.75f);

        L0OrcArenaPrimitiveKit.ClearTallCollidersInCorridor(
            root,
            L0OrcArenaConfig.BaseCenter + L0OrcArenaConfig.BackDir * 8f,
            L0OrcArenaConfig.PortalShrine + L0OrcArenaConfig.GateDir * 3f,
            L0OrcArenaConfig.RoadWidth * 0.5f + 2f,
            0.75f);
    }
}

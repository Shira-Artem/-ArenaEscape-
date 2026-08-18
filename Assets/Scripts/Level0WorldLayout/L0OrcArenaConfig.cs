using UnityEngine;

/// <summary>
/// Named radial bands in local Level0 orc-arena space.
/// </summary>
public enum L0OrcArenaZone
{
    BattleCenter,
    TransitionAisle,
    CampRing,
    CombatStrongpointRing,
    SetDressingRing,
    PerimeterRing,
    EmptyBuffer,
    CliffRing,
    DistantBackdrop,
}

/// <summary>
/// Single spatial contract for the playable Level0 orc arena and its visual backdrop.
/// </summary>
public static class L0OrcArenaConfig
{
    public static readonly Vector3 RoadStart = new Vector3(0f, 0.25f, -35f);
    public static readonly Vector3 RoadMid = new Vector3(4f, 0.25f, -92f);
    public static readonly Vector3 BaseEntrance = new Vector3(8f, 0.25f, -120f);
    public static readonly Vector3 BaseCenter = new Vector3(8f, 0.25f, -175f);
    public static readonly Vector3 PortalShrine = new Vector3(10f, 0.25f, -275f);

    public const float RoadWidth = 20f;
    public const float EntranceClearWidth = 22f;
    public const float EntranceForwardClear = 48f;
    public const float ArenaRadius = 112f;
    public const float ArenaDepthRadius = 130f;
    public const float InnerBattleRadius = 42f;
    public const float PortalShrineRadius = 26f;

    public const float BattleCenterMinRadius = 0f;
    public const float BattleCenterMaxRadius = 42f;
    public const float TransitionAisleMinRadius = 42f;
    public const float TransitionAisleMaxRadius = 50f;
    public const float CampRingMinRadius = 50f;
    public const float CampRingMaxRadius = 74f;
    public const float CombatStrongpointRingMinRadius = 58f;
    public const float CombatStrongpointRingMaxRadius = 112f;
    public const float SetDressingRingMinRadius = 78f;
    public const float SetDressingRingMaxRadius = 104f;
    public const float PerimeterRingMinRadius = 104f;
    public const float PerimeterRingMaxRadius = 140f;
    public const float EmptyBufferMinRadius = 108f;
    public const float EmptyBufferMaxRadius = 142f;
    public const float CliffRingMinRadius = 150f;
    public const float CliffRingMaxRadius = 230f;
    public const float CliffRingPlacementRadius = 190f;
    public const float DistantBackdropMinRadius = 240f;

    public const float RoadCorridorMargin = 4f;
    public const float EntranceCorridorMargin = 4f;
    public const float PortalCorridorMargin = 4f;

    // Backdrop hard-ban geometry. All checks expand these zones by the full footprint radius.
    public const float EntranceVistaClearZoneBackOffset = 20f;
    public const float EntranceVistaClearZoneForwardLength = 280f;
    public const float EntranceVistaClearZoneBaseHalfWidth = 75f;
    public const float EntranceVistaClearZoneWidthGrowth = 0.45f;
    public const float GateForegroundNoBackdropZoneRadius = 90f;
    public const float RoadToGateNoBackdropZoneHalfWidth = 60f;
    public const float BackdropFootprintScale = 0.65f;
    public const float BackdropFootprintMargin = 8f;

    public const string RootName = "LEVEL0_ORC_BASE_CLEAN_ARENA";
    public static bool showOrcArenaDebugMarkers = false;
    public static bool showOrcArenaBackdropDebugLabels = false;

    public static Vector3 GateDir
    {
        get
        {
            Vector3 dir = BaseEntrance - BaseCenter;
            dir.y = 0f;
            return dir.sqrMagnitude < 0.01f ? Vector3.forward : dir.normalized;
        }
    }

    public static Vector3 BackDir
    {
        get { return -GateDir; }
    }

    public static Vector3 SideDir
    {
        get { return Vector3.Cross(Vector3.up, GateDir).normalized; }
    }

    public static Quaternion FaceGate
    {
        get { return Quaternion.LookRotation(GateDir, Vector3.up); }
    }

    public static Quaternion FaceBack
    {
        get { return Quaternion.LookRotation(BackDir, Vector3.up); }
    }

    // Three authored sectors form the village's main read from gate to shrine.
    public static Vector3 EntranceSectorCenter { get { return BaseEntrance + BackDir * 22f; } }
    public static Vector3 CentralCombatSectorCenter { get { return BaseCenter; } }
    public static Vector3 RearPortalSectorCenter { get { return PortalShrine + GateDir * 28f; } }
    public static Vector3 CentralRitualPitAnchor { get { return BaseCenter; } }
    public static Vector3 PortalApproachAnchor { get { return PortalShrine + GateDir * 36f; } }

    // Eight deterministic strongpoints frame the open center without occupying the route axis.
    public static Vector3 EntranceLeftRampPost { get { return BaseCenter + SideDir * -62f + GateDir * 45f; } }
    public static Vector3 EntranceRightRampPost { get { return BaseCenter + SideDir * 62f + GateDir * 45f; } }
    public static Vector3 CentralLeftWatchPost { get { return BaseCenter + SideDir * -88f + GateDir * 5f; } }
    public static Vector3 CentralRightWatchPost { get { return BaseCenter + SideDir * 88f + GateDir * 5f; } }
    public static Vector3 CentralLeftCagePost { get { return BaseCenter + SideDir * -70f + BackDir * 32f; } }
    public static Vector3 CentralRightSpikePost { get { return BaseCenter + SideDir * 70f + BackDir * 32f; } }
    public static Vector3 RearLeftAssaultPost { get { return BaseCenter + SideDir * -60f + BackDir * 78f; } }
    public static Vector3 RearRightAssaultPost { get { return BaseCenter + SideDir * 60f + BackDir * 78f; } }

    // Camp anchors are identity/perimeter dressing only; combat ownership lives elsewhere.
    public static Vector3 LeftFrontCampIdentity { get { return BaseCenter + SideDir * -58f + GateDir * 5f; } }
    public static Vector3 RightFrontCampIdentity { get { return BaseCenter + SideDir * 58f + GateDir * 5f; } }
    public static Vector3 LeftRearCampIdentity { get { return BaseCenter + SideDir * -48f + BackDir * 40f; } }
    public static Vector3 RightRearCampIdentity { get { return BaseCenter + SideDir * 48f + BackDir * 40f; } }

    public const float RampStrongpointRadius = 11f;
    public const float WatchStrongpointRadius = 12f;
    public const float MidStrongpointRadius = 11f;
    public const float RearStrongpointRadius = 12f;
    public const float CampIdentityRadius = 7.5f;

    public static Vector3 GetRoadPoint(float t)
    {
        t = Mathf.Clamp01(t);
        if (t < 0.55f)
            return Vector3.Lerp(RoadStart, RoadMid, t / 0.55f);

        return Vector3.Lerp(RoadMid, BaseEntrance, (t - 0.55f) / 0.45f);
    }

    public static Vector2 ToArenaLocalXZ(Vector3 worldPosition)
    {
        Vector3 offset = worldPosition - BaseCenter;
        offset.y = 0f;
        return new Vector2(Vector3.Dot(offset, SideDir), Vector3.Dot(offset, GateDir));
    }

    public static float GetArenaLocalRadius(Vector3 worldPosition)
    {
        return ToArenaLocalXZ(worldPosition).magnitude;
    }

    public static void GetZoneRadii(L0OrcArenaZone zone, out float minRadius, out float maxRadius)
    {
        switch (zone)
        {
            case L0OrcArenaZone.BattleCenter:
                minRadius = BattleCenterMinRadius;
                maxRadius = BattleCenterMaxRadius;
                return;
            case L0OrcArenaZone.TransitionAisle:
                minRadius = TransitionAisleMinRadius;
                maxRadius = TransitionAisleMaxRadius;
                return;
            case L0OrcArenaZone.CampRing:
                minRadius = CampRingMinRadius;
                maxRadius = CampRingMaxRadius;
                return;
            case L0OrcArenaZone.CombatStrongpointRing:
                minRadius = CombatStrongpointRingMinRadius;
                maxRadius = CombatStrongpointRingMaxRadius;
                return;
            case L0OrcArenaZone.SetDressingRing:
                minRadius = SetDressingRingMinRadius;
                maxRadius = SetDressingRingMaxRadius;
                return;
            case L0OrcArenaZone.PerimeterRing:
                minRadius = PerimeterRingMinRadius;
                maxRadius = PerimeterRingMaxRadius;
                return;
            case L0OrcArenaZone.EmptyBuffer:
                minRadius = EmptyBufferMinRadius;
                maxRadius = EmptyBufferMaxRadius;
                return;
            case L0OrcArenaZone.CliffRing:
                minRadius = CliffRingMinRadius;
                maxRadius = CliffRingMaxRadius;
                return;
            case L0OrcArenaZone.DistantBackdrop:
                minRadius = DistantBackdropMinRadius;
                maxRadius = float.PositiveInfinity;
                return;
            default:
                minRadius = 0f;
                maxRadius = -1f;
                return;
        }
    }

    public static bool IsRadiusInsideZone(float localRadius, float footprintRadius, L0OrcArenaZone zone)
    {
        footprintRadius = Mathf.Max(0f, footprintRadius);
        float minRadius;
        float maxRadius;
        GetZoneRadii(zone, out minRadius, out maxRadius);

        float nearEdge = localRadius - footprintRadius;
        float farEdge = localRadius + footprintRadius;
        return nearEdge >= minRadius && farEdge <= maxRadius;
    }

    public static bool IsFootprintInsideZone(Vector3 center, float footprintRadius, L0OrcArenaZone zone)
    {
        return IsRadiusInsideZone(GetArenaLocalRadius(center), footprintRadius, zone);
    }

    public static bool IsInsideBattleCenter(Vector3 pos)
    {
        return IsFootprintInsideZone(pos, 0f, L0OrcArenaZone.BattleCenter);
    }

    public static bool IsInsideRoadCorridor(Vector3 pos)
    {
        float halfWidth = RoadWidth * 0.5f;
        return DistanceToSegmentXZ(pos, RoadStart, RoadMid) <= halfWidth
            || DistanceToSegmentXZ(pos, RoadMid, BaseEntrance) <= halfWidth;
    }

    public static bool IsInsideEntranceClearZone(Vector3 pos)
    {
        return DistanceToSegmentXZ(pos, GetEntranceLaneStart(), GetEntranceLaneEnd()) <= EntranceClearWidth * 0.5f;
    }

    public static bool IsInsidePortalPath(Vector3 pos)
    {
        return DistanceToSegmentXZ(pos, GetPortalLaneStart(), GetPortalLaneEnd()) <= RoadWidth * 0.5f + 3f;
    }

    public static bool IsInsidePortalShrine(Vector3 pos)
    {
        return FlatDistance(pos, PortalShrine) <= PortalShrineRadius;
    }

    public static Vector3 GetEntranceLaneStart()
    {
        return BaseEntrance + GateDir * EntranceForwardClear;
    }

    public static Vector3 GetEntranceLaneEnd()
    {
        return BaseCenter + GateDir * 4f;
    }

    public static Vector3 GetPortalLaneStart()
    {
        return BaseCenter + BackDir * 8f;
    }

    public static Vector3 GetPortalLaneEnd()
    {
        return PortalShrine + GateDir * 3f;
    }

    public static bool IsFootprintOutsideConfiguredCriticalLanes(Vector3 center, float footprintRadius)
    {
        footprintRadius = Mathf.Max(0f, footprintRadius);

        if (DistanceToSegmentXZ(center, RoadStart, RoadMid) < RoadWidth * 0.5f + RoadCorridorMargin + footprintRadius)
            return false;

        if (DistanceToSegmentXZ(center, RoadMid, BaseEntrance) < RoadWidth * 0.5f + RoadCorridorMargin + footprintRadius)
            return false;

        if (DistanceToSegmentXZ(center, GetEntranceLaneStart(), GetEntranceLaneEnd()) < EntranceClearWidth * 0.5f + EntranceCorridorMargin + footprintRadius)
            return false;

        if (DistanceToSegmentXZ(center, GetPortalLaneStart(), GetPortalLaneEnd()) < RoadWidth * 0.5f + PortalCorridorMargin + footprintRadius)
            return false;

        return true;
    }

    public static bool IntersectsEntranceVistaClearZone(Vector3 center, float footprintRadius)
    {
        footprintRadius = Mathf.Max(0f, footprintRadius);
        Vector2 local = ToArenaLocalXZ(center);
        float side = local.x;
        float forward = local.y;

        if (forward + footprintRadius < -EntranceVistaClearZoneBackOffset)
            return false;

        if (forward - footprintRadius > EntranceVistaClearZoneForwardLength)
            return false;

        float widthSample = Mathf.Clamp(forward, 0f, EntranceVistaClearZoneForwardLength);
        float halfWidth = EntranceVistaClearZoneBaseHalfWidth + widthSample * EntranceVistaClearZoneWidthGrowth;
        return Mathf.Abs(side) - footprintRadius <= halfWidth;
    }

    public static bool IntersectsGateForegroundNoBackdropZone(Vector3 center, float footprintRadius)
    {
        footprintRadius = Mathf.Max(0f, footprintRadius);
        return FlatDistance(center, BaseEntrance) <= GateForegroundNoBackdropZoneRadius + footprintRadius;
    }

    public static bool IntersectsRoadToGateNoBackdropZone(Vector3 center, float footprintRadius)
    {
        footprintRadius = Mathf.Max(0f, footprintRadius);
        float expandedHalfWidth = RoadToGateNoBackdropZoneHalfWidth + footprintRadius;
        return DistanceToSegmentXZ(center, RoadStart, RoadMid) <= expandedHalfWidth
            || DistanceToSegmentXZ(center, RoadMid, BaseEntrance) <= expandedHalfWidth;
    }

    public static bool IsBackdropFootprintOutsideVisualForbiddenZones(Vector3 center, float footprintRadius)
    {
        return !IntersectsEntranceVistaClearZone(center, footprintRadius)
            && !IntersectsGateForegroundNoBackdropZone(center, footprintRadius)
            && !IntersectsRoadToGateNoBackdropZone(center, footprintRadius);
    }

    public static bool IsForbiddenForMountains(Vector3 pos)
    {
        return IsForbiddenForMountains(pos, 0f);
    }

    public static bool IsForbiddenForMountains(Vector3 pos, float footprintRadius)
    {
        footprintRadius = Mathf.Max(0f, footprintRadius);
        bool inBackdropZone = IsFootprintInsideZone(pos, footprintRadius, L0OrcArenaZone.CliffRing)
            || IsFootprintInsideZone(pos, footprintRadius, L0OrcArenaZone.DistantBackdrop);
        return !inBackdropZone
            || !IsFootprintOutsideConfiguredCriticalLanes(pos, footprintRadius)
            || !IsBackdropFootprintOutsideVisualForbiddenZones(pos, footprintRadius);
    }

    public static bool IsForbiddenForLargeProps(Vector3 pos)
    {
        bool inPropZone = IsFootprintInsideZone(pos, 0f, L0OrcArenaZone.CampRing)
            || IsFootprintInsideZone(pos, 0f, L0OrcArenaZone.SetDressingRing);

        return !inPropZone
            || !IsFootprintOutsideConfiguredCriticalLanes(pos, 0f)
            || IsInsidePortalShrine(pos);
    }

    public static float FlatDistance(Vector3 a, Vector3 b)
    {
        a.y = 0f;
        b.y = 0f;
        return Vector3.Distance(a, b);
    }

    public static float DistanceToSegmentXZ(Vector3 point, Vector3 start, Vector3 end)
    {
        Vector2 p = new Vector2(point.x, point.z);
        Vector2 a = new Vector2(start.x, start.z);
        Vector2 b = new Vector2(end.x, end.z);
        Vector2 ab = b - a;
        float denom = Vector2.Dot(ab, ab);
        if (denom <= 0.0001f)
            return Vector2.Distance(p, a);

        float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / denom);
        return Vector2.Distance(p, a + ab * t);
    }
}

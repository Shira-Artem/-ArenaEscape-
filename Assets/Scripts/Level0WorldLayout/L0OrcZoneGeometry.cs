using UnityEngine;

/// <summary>
/// Compatibility facade for older Level0 helpers.
/// All coordinates and geometry decisions delegate to L0OrcArenaConfig.
/// </summary>
public static class L0OrcZoneGeometry
{
    public static Vector3 RoadStart { get { return L0OrcArenaConfig.RoadStart; } }
    public static Vector3 RoadMid { get { return L0OrcArenaConfig.RoadMid; } }
    public static Vector3 BaseEntrance { get { return L0OrcArenaConfig.BaseEntrance; } }
    public static Vector3 BaseCenter { get { return L0OrcArenaConfig.BaseCenter; } }
    public static Vector3 ExitPlatform { get { return L0OrcArenaConfig.PortalShrine; } }

    public const float RoadHalfWidth = L0OrcArenaConfig.RoadWidth * 0.5f;
    public const float BaseInnerRadius = L0OrcArenaConfig.BattleCenterMaxRadius;
    public const float BaseOuterRadius = L0OrcArenaConfig.CampRingMaxRadius;
    public const float MountainKeepOutRadius = L0OrcArenaConfig.EmptyBufferMaxRadius;

    public static bool showOrcZoneDebugMarkers
    {
        get { return L0OrcArenaConfig.showOrcArenaDebugMarkers; }
        set { L0OrcArenaConfig.showOrcArenaDebugMarkers = value; }
    }

    public static Vector3 GetRoadPoint(float t)
    {
        return L0OrcArenaConfig.GetRoadPoint(t);
    }

    public static bool IsMountainAllowed(Vector3 position)
    {
        return !L0OrcArenaConfig.IsForbiddenForMountains(position);
    }

    public static bool IsInRoadCorridor(Vector3 position, float halfWidth)
    {
        return L0OrcArenaConfig.DistanceToSegmentXZ(position, RoadStart, RoadMid) <= halfWidth
            || L0OrcArenaConfig.DistanceToSegmentXZ(position, RoadMid, BaseEntrance) <= halfWidth;
    }

    public static void CreateDebugMarkers(Transform parent)
    {
        if (!showOrcZoneDebugMarkers || parent == null)
            return;

        L0OrcArenaPrimitiveKit.CreateDebugMarker(parent, "RoadStart", RoadStart, Color.green);
        L0OrcArenaPrimitiveKit.CreateDebugMarker(parent, "RoadMid", RoadMid, Color.yellow);
        L0OrcArenaPrimitiveKit.CreateDebugMarker(parent, "BaseEntrance", BaseEntrance, Color.red);
        L0OrcArenaPrimitiveKit.CreateDebugMarker(parent, "BaseCenter", BaseCenter, Color.cyan);
        L0OrcArenaPrimitiveKit.CreateDebugMarker(parent, "ExitPlatform", ExitPlatform, Color.magenta);
    }

    public static float FlatDistance(Vector3 a, Vector3 b)
    {
        return L0OrcArenaConfig.FlatDistance(a, b);
    }
}

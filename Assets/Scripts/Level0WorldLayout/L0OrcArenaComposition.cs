using UnityEngine;

/// <summary>
/// Shared deterministic coordinates for the clean Level0 orc arena composition.
/// Visual builders use these values so the road, gate, battle floor and future portal stay aligned.
/// </summary>
public static class L0OrcArenaComposition
{
    public static readonly Vector3 RoadStart = L0OrcArenaConfig.RoadStart;
    public static readonly Vector3 RoadMid = L0OrcArenaConfig.RoadMid;
    public static readonly Vector3 BaseEntrance = L0OrcArenaConfig.BaseEntrance;
    public static readonly Vector3 BaseCenter = L0OrcArenaConfig.BaseCenter;
    public static readonly Vector3 PortalShrine = L0OrcArenaConfig.PortalShrine;

    public const float RoadWidth = L0OrcArenaConfig.RoadWidth;
    public const float EntranceClearWidth = L0OrcArenaConfig.EntranceClearWidth;
    public const float ArenaRadius = L0OrcArenaConfig.ArenaRadius;
    public const float InnerBattleRadius = L0OrcArenaConfig.InnerBattleRadius;
    public const float PortalShrineRadius = L0OrcArenaConfig.PortalShrineRadius;

    public static Vector3 GetRoadPoint(float t)
    {
        return L0OrcArenaConfig.GetRoadPoint(t);
    }
}

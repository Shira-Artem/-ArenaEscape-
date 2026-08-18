using UnityEngine;

/// <summary>
/// Thin compatibility entry point for the Level0 orc base layout.
/// The actual visual composition lives in L0OrcCleanArena to keep the base simple and readable.
/// </summary>
public static class L0OrcBase
{
    public static void Build(GameObject container, Vector3 approachPoint, Vector3 center, int tentCount, int towerCount, float baseRadius, bool createLargeGate, bool createArenaCore, bool createCentralBossArea, bool createAtmosphere, bool createExtraAtmosphere, bool createAudio)
    {
        if (container == null)
            return;

        L0OrcCleanArena.Build(container, createAtmosphere, createAudio);
    }
}

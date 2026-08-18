using UnityEngine;

/// <summary>
/// Minimal visual placeholder for the future Level Two exit.
/// Scene transition logic is owned by Level0ExitPortal/L0QuickExitToOne and is not changed here.
/// A6 pass: this connector no longer creates background mountains or RockSpire objects.
/// </summary>
public static class L0Exit
{
    public static void Build(GameObject container, Vector3 orcStrongholdCenter, Vector3 exitPassPoint, bool createAtmosphere, bool createExtraAtmosphere)
    {
        if (container == null)
            return;

        Transform parent = container.transform;
        Vector3 baseCenter = L0OrcArenaConfig.BaseCenter;
        Vector3 exitPlatform = L0OrcArenaConfig.PortalShrine;
        Vector3 fromBase = exitPlatform - baseCenter;
        fromBase.y = 0f;
        if (fromBase.sqrMagnitude < 0.01f) fromBase = Vector3.back;
        fromBase.Normalize();

        Quaternion facePath = Quaternion.LookRotation(fromBase, Vector3.up);

        // Keep only the connector ground and the small navigation light.
        // Backdrop ownership belongs exclusively to L0OrcArenaBackdropModule.
        L0Props.CreateGroundPatchWalkable(
            Vector3.Lerp(baseCenter, exitPlatform, 0.58f),
            new Vector3(L0OrcArenaConfig.RoadWidth + 2f, 0.065f, 54f),
            new Color(0.36f, 0.27f, 0.17f),
            parent,
            "CleanExitConnectorPath",
            facePath);

        L0Props.CreateGroundPatchWalkable(
            exitPlatform + fromBase * 2.0f,
            new Vector3(L0OrcArenaConfig.PortalShrineRadius * 2f, 0.07f, 28f),
            new Color(0.20f, 0.18f, 0.17f),
            parent,
            "CleanExitPassGround",
            facePath);

        if (createAtmosphere)
            L0Props.CreateLantern(exitPlatform + fromBase * 5.0f, parent);
    }
}

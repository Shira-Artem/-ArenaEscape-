using UnityEngine;

/// <summary>
/// Builds the visual future level-two shrine without changing portal logic.
/// </summary>
public static class L0OrcArenaPortalModule
{
    public static void Build(Transform parent, bool createAtmosphere)
    {
        if (parent == null)
            return;

        Vector3 center = L0OrcArenaConfig.BaseCenter;
        Vector3 portal = L0OrcArenaConfig.PortalShrine;
        Vector3 side = L0OrcArenaConfig.SideDir;
        Vector3 gate = L0OrcArenaConfig.GateDir;
        Vector3 back = L0OrcArenaConfig.BackDir;
        Quaternion faceBack = L0OrcArenaConfig.FaceBack;

        L0OrcArenaPrimitiveKit.CreateGroundPatchDecorative(parent, "PortalShrineDarkStoneGround_NoCollider", portal + Vector3.up * 0.06f, faceBack, new Vector3(L0OrcArenaConfig.PortalShrineRadius * 2.20f, 0.05f, L0OrcArenaConfig.PortalShrineRadius * 2.20f), L0OrcArenaMaterials.PortalStone);
        L0OrcArenaPrimitiveKit.CreatePrimitive(parent, "PortalOuterStoneCircle_NoCollider", PrimitiveType.Cylinder, portal + Vector3.up * 0.095f, Quaternion.identity, new Vector3(L0OrcArenaConfig.PortalShrineRadius * 2.72f, 0.025f, L0OrcArenaConfig.PortalShrineRadius * 2.72f), L0OrcArenaMaterials.DarkStone, false);
        L0OrcArenaPrimitiveKit.CreatePrimitive(parent, "PortalBurntRuneCircle_NoCollider", PrimitiveType.Cylinder, portal + Vector3.up * 0.128f, Quaternion.identity, new Vector3(L0OrcArenaConfig.PortalShrineRadius * 1.62f, 0.018f, L0OrcArenaConfig.PortalShrineRadius * 1.62f), L0OrcArenaMaterials.ArenaInner, false);
        L0OrcArenaPrimitiveKit.CreatePrimitive(parent, "PortalInnerRuneCircle_Emissive_NoCollider", PrimitiveType.Cylinder, portal + Vector3.up * 0.155f, Quaternion.identity, new Vector3(L0OrcArenaConfig.PortalShrineRadius * 0.92f, 0.012f, L0OrcArenaConfig.PortalShrineRadius * 0.92f), new Color(0.38f, 0.035f, 0.028f), false, true);
        L0OrcArenaCombatPropsModule.CreatePortalApproachBlockade(parent, "REAR_SECTOR_PortalApproachBlockade", L0OrcArenaConfig.PortalApproachAnchor, faceBack, 28f);

        Vector3 leftTower = portal + side * -28f + gate * 5f;
        Vector3 rightTower = portal + side * 28f + gate * 5f;
        L0OrcArenaPrimitiveKit.CreateGroundPatchDecorative(parent, "PortalGuardPad_L_NoCollider", leftTower + Vector3.up * 0.06f, faceBack, new Vector3(14f, 0.05f, 14f), L0OrcArenaMaterials.GateGround);
        L0OrcArenaPrimitiveKit.CreateGroundPatchDecorative(parent, "PortalGuardPad_R_NoCollider", rightTower + Vector3.up * 0.06f, faceBack, new Vector3(14f, 0.05f, 14f), L0OrcArenaMaterials.GateGround);
        L0Props.CreateOrcTower(leftTower, Quaternion.LookRotation(center - leftTower, Vector3.up), 2.08f, parent);
        L0Props.CreateOrcTower(rightTower, Quaternion.LookRotation(center - rightTower, Vector3.up), 2.08f, parent);

        L0Props.CreateFutureLevelTwoExitPlatform(portal, faceBack, 2.35f, parent);
        L0Props.CreateRouteSign("ПУТЬ В ЦАРСТВО ОРКОВ", portal + side * -12f + back * 8.5f, faceBack * Quaternion.Euler(0f, 12f, 0f), parent);
        L0Props.CreateRouteSign("ПОРТАЛ ЗАПЕЧАТАН", portal + side * 12f + back * 8.5f, faceBack * Quaternion.Euler(0f, -12f, 0f), parent);

        Vector3[] torches =
        {
            portal + side * -12f + gate * 6f,
            portal + side * 12f + gate * 6f,
            portal + side * -12f + back * 6.5f,
            portal + side * 12f + back * 6.5f,
            portal + side * -20f + back * 2.5f,
            portal + side * 20f + back * 2.5f,
            portal + side * -25f + gate * 1.5f,
            portal + side * 25f + gate * 1.5f,
        };

        for (int i = 0; i < torches.Length; i++)
            L0Props.CreateTorchPost(torches[i], faceBack, i < 4 ? 1.25f : 1.12f, parent);

        CreateObelisk(parent, portal + side * -17.5f + back * 5.2f, faceBack, 1.88f);
        CreateObelisk(parent, portal + side * 17.5f + back * 5.2f, faceBack, 1.88f);
        CreateObelisk(parent, portal + side * -10.5f + gate * 8.0f, faceBack, 1.35f);
        CreateObelisk(parent, portal + side * 10.5f + gate * 8.0f, faceBack, 1.35f);
        L0Props.CreateOrcTuskPillar(portal + side * -23.5f + gate * 3f, faceBack, 1.55f, parent);
        L0Props.CreateOrcTuskPillar(portal + side * 23.5f + gate * 3f, faceBack, 1.55f, parent);
        L0OrcArenaPrimitiveKit.CreateBannerBar(parent, "PortalShrineBanner", portal + Vector3.up * 6.8f + back * 8.2f, faceBack, 17f);
        L0OrcArenaPrimitiveKit.CreateSideFlag(parent, "PortalFlag_L", portal + side * -19f + gate * 9f, faceBack, 1.38f);
        L0OrcArenaPrimitiveKit.CreateSideFlag(parent, "PortalFlag_R", portal + side * 19f + gate * 9f, faceBack, 1.38f);

        CreateRuneTickMarks(parent, portal, 14.5f, faceBack);
        CreateRuneTickMarks(parent, portal, 22.5f, faceBack);

        if (createAtmosphere)
        {
            L0Props.CreatePointLight("CleanArena_PortalFire", portal + Vector3.up * 2.9f, L0OrcArenaMaterials.Fire, 1.45f, 18f, parent);
            L0Props.CreatePointLight("CleanArena_PortalRedPulse", portal + Vector3.up * 1.2f, L0OrcArenaMaterials.OrcRed, 0.85f, 16f, parent);
            CreatePortalSmokeColumn(parent, portal + back * 4.2f + Vector3.up * 2.2f, 1.2f);
            CreatePortalSmokeColumn(parent, portal + side * -8f + back * 1.0f + Vector3.up * 1.6f, 0.82f);
            CreatePortalSmokeColumn(parent, portal + side * 8f + back * 1.0f + Vector3.up * 1.6f, 0.82f);
        }
    }

    private static void CreateObelisk(Transform parent, Vector3 position, Quaternion rotation, float scale)
    {
        GameObject root = L0OrcArenaPrimitiveKit.CreateGroup("PortalShrineObelisk", parent);
        root.transform.position = position;
        root.transform.rotation = rotation;
        L0OrcArenaPrimitiveKit.CreatePrimitive(root.transform, "DarkStoneBase", PrimitiveType.Cube, new Vector3(0f, 0.35f * scale, 0f), Quaternion.identity, new Vector3(1.5f * scale, 0.7f * scale, 1.5f * scale), L0OrcArenaMaterials.PortalStone, true);
        L0OrcArenaPrimitiveKit.CreatePrimitive(root.transform, "DarkStoneSpire", PrimitiveType.Cube, new Vector3(0f, 2.25f * scale, 0f), Quaternion.Euler(0f, 45f, 0f), new Vector3(0.88f * scale, 3.05f * scale, 0.88f * scale), L0OrcArenaMaterials.DarkStone, true);
        L0OrcArenaPrimitiveKit.CreatePrimitive(root.transform, "RedRune", PrimitiveType.Cube, new Vector3(0f, 2.72f * scale, -0.46f * scale), Quaternion.identity, new Vector3(0.48f * scale, 0.92f * scale, 0.055f * scale), L0OrcArenaMaterials.OrcRed, false, true);
    }

    private static void CreateRuneTickMarks(Transform parent, Vector3 center, float radius, Quaternion baseRotation)
    {
        for (int i = 0; i < 12; i++)
        {
            float a = i * 30f;
            Quaternion rot = Quaternion.Euler(0f, a, 0f) * baseRotation;
            Vector3 pos = center + Quaternion.Euler(0f, a, 0f) * Vector3.forward * radius + Vector3.up * 0.19f;
            L0OrcArenaPrimitiveKit.CreatePrimitive(parent, "PortalRuneTick_" + Mathf.RoundToInt(radius) + "_" + i + "_NoCollider", PrimitiveType.Cube, pos, rot, new Vector3(0.35f, 0.055f, 1.35f), L0OrcArenaMaterials.OrcRed, false, true);
        }
    }

    private static void CreatePortalSmokeColumn(Transform parent, Vector3 basePosition, float scale)
    {
        L0Props.CreateSmokePuff(basePosition, scale, parent);
        L0Props.CreateSmokePuff(basePosition + Vector3.up * (1.6f * scale) + L0OrcArenaConfig.SideDir * (0.5f * scale), scale * 0.72f, parent);
        L0Props.CreateSmokePuff(basePosition + Vector3.up * (3.1f * scale) + L0OrcArenaConfig.SideDir * (-0.3f * scale), scale * 0.52f, parent);
    }
}

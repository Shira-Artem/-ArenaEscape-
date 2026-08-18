using UnityEngine;

/// <summary>
/// Secondary visual punctuation only. Primary cover, height and strongpoints belong
/// to L0OrcArenaCombatLayoutModule.
/// </summary>
public static class L0OrcArenaSetDressingModule
{
    public static void Build(Transform parent, bool createAtmosphere, L0OrcArenaPlacementGuard placementGuard)
    {
        if (parent == null || placementGuard == null)
            return;

        CreateFiller(parent, "DRESSING_LeftEntranceRags", -92f, 32f, -1f, createAtmosphere, placementGuard);
        CreateFiller(parent, "DRESSING_RightEntranceRags", 92f, 32f, 1f, createAtmosphere, placementGuard);
        CreateFiller(parent, "DRESSING_LeftMidBones", -99f, -10f, -1f, createAtmosphere, placementGuard);
        CreateFiller(parent, "DRESSING_RightMidBones", 99f, -10f, 1f, createAtmosphere, placementGuard);
        CreateFiller(parent, "DRESSING_LeftRearWarning", -80f, -55f, -1f, createAtmosphere, placementGuard);
        CreateFiller(parent, "DRESSING_RightRearWarning", 80f, -55f, 1f, createAtmosphere, placementGuard);
    }

    private static void CreateFiller(Transform parent, string name, float sideOffset, float gateOffset, float sideSign, bool createAtmosphere, L0OrcArenaPlacementGuard placementGuard)
    {
        Vector3 anchor = L0OrcArenaConfig.BaseCenter
            + L0OrcArenaConfig.SideDir * sideOffset
            + L0OrcArenaConfig.GateDir * gateOffset;
        const float footprintRadius = 3.2f;

        if (!placementGuard.TryReserve(L0OrcArenaPlacementCategory.SetDressing, name, anchor, footprintRadius, L0OrcArenaZone.SetDressingRing))
            return;

        Quaternion faceCenter = FaceCenter(anchor);
        GameObject root = L0OrcArenaPrimitiveKit.CreateGroup(name, parent);
        L0OrcArenaPrimitiveKit.CreateSideFlag(root.transform, name + "_TornFlag", anchor, faceCenter * Quaternion.Euler(0f, sideSign * 12f, 0f), 0.82f);
        L0Props.CreateBonePile(anchor + L0OrcArenaConfig.SideDir * (sideSign * 1.5f) + L0OrcArenaConfig.GateDir * 1.1f, 0.62f, root.transform);
        CreateWarningStakes(root.transform, anchor + L0OrcArenaConfig.SideDir * (sideSign * -1.25f), faceCenter, sideSign);

        if (createAtmosphere)
            L0Props.CreateTorchPost(anchor + L0OrcArenaConfig.GateDir * -1.8f, faceCenter, 0.68f, root.transform);
    }

    private static void CreateWarningStakes(Transform parent, Vector3 position, Quaternion rotation, float sideSign)
    {
        GameObject root = L0OrcArenaPrimitiveKit.CreateGroup("SecondaryWarningStakes", parent);
        root.transform.position = position;
        root.transform.rotation = rotation;
        L0OrcArenaPrimitiveKit.CreatePrimitive(root.transform, "StakeA_NoCollider", PrimitiveType.Cylinder, new Vector3(-0.36f, 0.92f, 0f), Quaternion.Euler(0f, 0f, sideSign * -9f), new Vector3(0.08f, 0.92f, 0.08f), L0OrcArenaMaterials.DarkWood, false);
        L0OrcArenaPrimitiveKit.CreatePrimitive(root.transform, "StakeB_NoCollider", PrimitiveType.Cylinder, new Vector3(0.36f, 0.78f, 0.12f), Quaternion.Euler(0f, 0f, sideSign * 11f), new Vector3(0.07f, 0.78f, 0.07f), L0OrcArenaMaterials.Bone, false);
        L0OrcArenaPrimitiveKit.CreatePrimitive(root.transform, "RedRag_NoCollider", PrimitiveType.Cube, new Vector3(0f, 1.30f, -0.04f), Quaternion.Euler(0f, 0f, sideSign * 5f), new Vector3(0.92f, 0.32f, 0.05f), L0OrcArenaMaterials.OrcRed, false);
    }

    private static Quaternion FaceCenter(Vector3 position)
    {
        Vector3 direction = L0OrcArenaConfig.BaseCenter - position;
        direction.y = 0f;
        return direction.sqrMagnitude > 0.01f ? Quaternion.LookRotation(direction, Vector3.up) : L0OrcArenaConfig.FaceGate;
    }
}

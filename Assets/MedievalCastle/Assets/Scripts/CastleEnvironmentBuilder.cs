using UnityEngine;

/// <summary>
/// Environment Refactor v1
/// Главный файл окружения.
/// Здесь только общий сценарий сборки: земля, дорога, ров, лес, горы и мелкий декор.
/// Детали вынесены в подфайлы для более чистой архитектуры.
/// </summary>
public static class CastleEnvironmentBuilder
{
    public static void Build(CastleGenerator.CastleContext c, bool addForest)
    {
        GameObject parent = c.Child(c.Root, "00_ENVIRONMENT_REFACTOR_V1");

        BuildGround(c, parent);
        CastleRoadDecorBuilder.BuildRoadAndBridge(c, parent);
        CastleRoadDecorBuilder.BuildMoat(c, parent);

        if (addForest)
        {
            CastleMountainBuilder.BuildDistantMountains(c, parent);
            CastleForestBuilder.BuildDenseForest(c, parent);
            CastleForestBuilder.BuildRoadsideForestCorridor(c, parent);
            CastleRoadDecorBuilder.BuildNatureDecor(c, parent);
            CastleRoadDecorBuilder.BuildRoadDetails(c, parent);
        }
    }

    /// <summary>
    /// Большая земля и локальная площадка вокруг замка.
    /// Вынесено отдельно, чтобы главный Build оставался коротким и читаемым.
    /// </summary>
    private static void BuildGround(CastleGenerator.CastleContext c, GameObject parent)
    {
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Large_Forest_Ground_Plane";
        ground.transform.SetParent(parent.transform);
        ground.transform.position = new Vector3(0f, -0.045f, 0f);
        ground.transform.localScale = new Vector3(24f, 1f, 24f);

        Renderer renderer = ground.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = c.MatGrass;
        }

        c.Cube(parent, "Castle_Foundation_Stone_Slab",
            new Vector3(0f, 0.012f, 0f),
            new Vector3(39.5f, 0.045f, 35.5f),
            c.MatFloor);

        c.Cube(parent, "Castle_Clearing_Around_Walls",
            new Vector3(0f, 0.026f, 0f),
            new Vector3(53f, 0.035f, 48f),
            c.Mode == CastleGenerator.LabMode.Lab3_Blockout
                ? c.MatGrass
                : c.NewMaterial(new Color(0.28f, 0.52f, 0.25f)),
            false);

        Material darkGrass = c.Mode == CastleGenerator.LabMode.Lab3_Blockout
            ? c.MatGrass
            : c.NewMaterial(new Color(0.17f, 0.37f, 0.16f));

        Material lightGrass = c.Mode == CastleGenerator.LabMode.Lab3_Blockout
            ? c.MatGrass
            : c.NewMaterial(new Color(0.34f, 0.58f, 0.30f));

        for (int i = 0; i < 34; i++)
        {
            float x = -66f + CastleNatureAssets.Hash01(i + 10) * 132f;
            float z = -70f + CastleNatureAssets.Hash01(i + 30) * 132f;

            if (Mathf.Abs(x) < 8f && z < -18f) continue;
            if (Mathf.Abs(x) < 26f && Mathf.Abs(z) < 20f) continue;

            GameObject patch = c.Sphere(parent, "Natural_Grass_Color_Patch",
                new Vector3(x, 0.025f, z),
                new Vector3(2.5f + CastleNatureAssets.Hash01(i + 80) * 4.5f, 0.035f, 1.5f + CastleNatureAssets.Hash01(i + 90) * 3.5f),
                i % 2 == 0 ? darkGrass : lightGrass,
                false);

            patch.transform.rotation = Quaternion.Euler(0f, CastleNatureAssets.Hash01(i + 140) * 180f, 0f);
        }
    }
}

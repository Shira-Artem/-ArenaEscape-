using UnityEngine;

/// <summary>
/// Общая фабрика пропсов для Level0 World Layout v4.
/// Все пропсы создаются из Unity-примитивов, без зависимостей от игровой логики.
/// Коллайдеры у декоративных примитивов удаляются.
/// </summary>
public static class L0Props
{
    private static readonly Color Wood = new Color(0.28f, 0.17f, 0.08f);
    private static readonly Color DarkWood = new Color(0.16f, 0.09f, 0.04f);
    private static readonly Color ClothRefugee = new Color(0.42f, 0.36f, 0.27f);
    private static readonly Color OrcCloth = new Color(0.13f, 0.08f, 0.06f);
    private static readonly Color OrcRed = new Color(0.62f, 0.04f, 0.03f);
    private static readonly Color Bone = new Color(0.72f, 0.62f, 0.48f);
    private static readonly Color Fire = new Color(1f, 0.43f, 0.08f);
    private static readonly Color Stone = new Color(0.28f, 0.27f, 0.25f);

    // ---------- Required API ----------

    public static GameObject CreateSimpleHouse(Vector3 position, Quaternion rotation, float scale, bool damaged, Transform parent)
    {
        GameObject root = CreateRoot(damaged ? "DamagedRefugeeHouse" : "RefugeeHouse", position, rotation, parent);

        float w = 3.0f * scale;
        float h = 2.35f * scale;
        float d = 3.2f * scale;

        Color wall = damaged ? new Color(0.34f, 0.29f, 0.23f) : new Color(0.52f, 0.43f, 0.32f);
        Color roof = damaged ? new Color(0.22f, 0.10f, 0.06f) : new Color(0.42f, 0.18f, 0.10f);

        CreatePrimitiveProp(root.transform, "Walls", PrimitiveType.Cube, new Vector3(0f, h * 0.5f, 0f), Quaternion.identity, new Vector3(w, h, d), wall);
        CreatePrimitiveProp(root.transform, "RoofLeft", PrimitiveType.Cube, new Vector3(0f, h + 0.33f * scale, -0.36f * scale), Quaternion.Euler(24f, 0f, 0f), new Vector3(w + 0.45f * scale, 0.42f * scale, d + 0.45f * scale), roof);
        CreatePrimitiveProp(root.transform, "RoofRight", PrimitiveType.Cube, new Vector3(0f, h + 0.33f * scale, 0.36f * scale), Quaternion.Euler(-24f, 0f, 0f), new Vector3(w + 0.45f * scale, 0.42f * scale, d + 0.45f * scale), roof);
        CreatePrimitiveProp(root.transform, "Door", PrimitiveType.Cube, new Vector3(0f, 0.68f * scale, d * 0.5f + 0.04f), Quaternion.identity, new Vector3(0.65f * scale, 1.25f * scale, 0.08f * scale), DarkWood);

        Color window = damaged ? new Color(0.05f, 0.05f, 0.06f) : new Color(1f, 0.68f, 0.28f);
        CreatePrimitiveProp(root.transform, "WindowLeft", PrimitiveType.Cube, new Vector3(-w * 0.32f, 1.35f * scale, d * 0.5f + 0.045f), Quaternion.identity, new Vector3(0.42f * scale, 0.42f * scale, 0.07f * scale), window, false, !damaged);
        CreatePrimitiveProp(root.transform, "WindowRight", PrimitiveType.Cube, new Vector3(w * 0.32f, 1.35f * scale, d * 0.5f + 0.045f), Quaternion.identity, new Vector3(0.42f * scale, 0.42f * scale, 0.07f * scale), window, false, !damaged);

        CreatePrimitiveProp(root.transform, "Chimney", PrimitiveType.Cube, new Vector3(w * 0.28f, h + 0.8f * scale, -d * 0.15f), Quaternion.identity, new Vector3(0.38f * scale, 0.95f * scale, 0.38f * scale), new Color(0.25f, 0.22f, 0.19f));

        if (damaged)
        {
            CreatePrimitiveProp(root.transform, "BrokenDarkHole", PrimitiveType.Cube, new Vector3(-w * 0.18f, 1.45f * scale, d * 0.5f + 0.055f), Quaternion.Euler(0f, 0f, 8f), new Vector3(0.85f * scale, 0.7f * scale, 0.09f * scale), new Color(0.04f, 0.035f, 0.03f));
            CreatePrimitiveProp(root.transform, "CollapsedBeam", PrimitiveType.Cube, new Vector3(w * 0.1f, h + 0.55f * scale, d * 0.55f), Quaternion.Euler(0f, 0f, -25f), new Vector3(1.8f * scale, 0.18f * scale, 0.18f * scale), DarkWood);
        }

        return root;
    }

    public static GameObject CreateRefugeeTent(Vector3 position, Quaternion rotation, float scale, Transform parent)
    {
        GameObject root = CreateRoot("RefugeeTent", position, rotation, parent);
        CreatePrimitiveProp(root.transform, "Canvas", PrimitiveType.Cube, new Vector3(0f, 0.85f * scale, 0f), Quaternion.Euler(0f, 0f, 45f), new Vector3(1.8f * scale, 1.8f * scale, 2.6f * scale), ClothRefugee);
        CreatePrimitiveProp(root.transform, "Entrance", PrimitiveType.Cube, new Vector3(0f, 0.5f * scale, 1.32f * scale), Quaternion.identity, new Vector3(0.65f * scale, 0.8f * scale, 0.06f * scale), new Color(0.08f, 0.06f, 0.045f));
        CreatePrimitiveProp(root.transform, "CenterPole", PrimitiveType.Cylinder, new Vector3(0f, 0.95f * scale, 0f), Quaternion.identity, new Vector3(0.08f * scale, 0.95f * scale, 0.08f * scale), Wood);
        return root;
    }

    public static GameObject CreateOrcTent(Vector3 position, Quaternion rotation, float scale, Transform parent)
    {
        GameObject root = CreateRoot("OrcHutTent", position, rotation, parent);
        CreatePrimitiveProp(root.transform, "HeavyBase", PrimitiveType.Cylinder, new Vector3(0f, 0.72f * scale, 0f), Quaternion.identity, new Vector3(1.55f * scale, 0.72f * scale, 1.45f * scale), OrcCloth);
        CreatePrimitiveProp(root.transform, "RaggedTop", PrimitiveType.Sphere, new Vector3(0f, 1.45f * scale, 0f), Quaternion.identity, new Vector3(1.55f * scale, 0.72f * scale, 1.45f * scale), new Color(0.10f, 0.06f, 0.045f));
        CreatePrimitiveProp(root.transform, "BlackEntrance", PrimitiveType.Cube, new Vector3(0f, 0.62f * scale, 1.48f * scale), Quaternion.identity, new Vector3(0.75f * scale, 0.9f * scale, 0.08f * scale), new Color(0.025f, 0.02f, 0.018f));
        CreatePrimitiveProp(root.transform, "BoneSpikeLeft", PrimitiveType.Cylinder, new Vector3(-1.2f * scale, 1.45f * scale, 0.6f * scale), Quaternion.Euler(0f, 0f, -28f), new Vector3(0.055f * scale, 0.75f * scale, 0.055f * scale), Bone);
        CreatePrimitiveProp(root.transform, "BoneSpikeRight", PrimitiveType.Cylinder, new Vector3(1.2f * scale, 1.45f * scale, 0.6f * scale), Quaternion.Euler(0f, 0f, 28f), new Vector3(0.055f * scale, 0.75f * scale, 0.055f * scale), Bone);
        CreatePrimitiveProp(root.transform, "RedClothMark", PrimitiveType.Cube, new Vector3(0f, 1.25f * scale, 1.5f * scale), Quaternion.identity, new Vector3(0.55f * scale, 0.35f * scale, 0.07f * scale), OrcRed);
        return root;
    }

    public static GameObject CreateOrcTower(Vector3 position, Quaternion rotation, float scale, Transform parent)
    {
        GameObject root = CreateRoot("OrcWatchTower", position, rotation, parent);
        float h = 5.2f * scale;
        float spread = 1.35f * scale;

        CreatePrimitiveProp(root.transform, "LegFL", PrimitiveType.Cylinder, new Vector3(-spread, h * 0.43f, spread), Quaternion.Euler(6f, 0f, -8f), new Vector3(0.12f * scale, h * 0.46f, 0.12f * scale), DarkWood);
        CreatePrimitiveProp(root.transform, "LegFR", PrimitiveType.Cylinder, new Vector3(spread, h * 0.43f, spread), Quaternion.Euler(6f, 0f, 8f), new Vector3(0.12f * scale, h * 0.46f, 0.12f * scale), DarkWood);
        CreatePrimitiveProp(root.transform, "LegBL", PrimitiveType.Cylinder, new Vector3(-spread, h * 0.43f, -spread), Quaternion.Euler(-6f, 0f, -8f), new Vector3(0.12f * scale, h * 0.46f, 0.12f * scale), DarkWood);
        CreatePrimitiveProp(root.transform, "LegBR", PrimitiveType.Cylinder, new Vector3(spread, h * 0.43f, -spread), Quaternion.Euler(-6f, 0f, 8f), new Vector3(0.12f * scale, h * 0.46f, 0.12f * scale), DarkWood);

        CreatePrimitiveProp(root.transform, "Platform", PrimitiveType.Cube, new Vector3(0f, h, 0f), Quaternion.identity, new Vector3(3.3f * scale, 0.28f * scale, 3.3f * scale), Wood);
        CreatePrimitiveProp(root.transform, "Roof", PrimitiveType.Cube, new Vector3(0f, h + 1.0f * scale, 0f), Quaternion.Euler(0f, 45f, 0f), new Vector3(2.65f * scale, 0.42f * scale, 2.65f * scale), OrcCloth);

        for (int i = 0; i < 4; i++)
        {
            float a = i * 90f;
            Vector3 pos = Quaternion.Euler(0f, a, 0f) * new Vector3(0f, h + 0.45f * scale, 1.65f * scale);
            CreatePrimitiveProp(root.transform, "TowerSpike_" + i, PrimitiveType.Cylinder, pos, Quaternion.Euler(25f, a, 0f), new Vector3(0.07f * scale, 0.6f * scale, 0.07f * scale), Bone);
        }

        CreateOrcBanner(position + rotation * new Vector3(0.9f * scale, 0f, 0.5f * scale), rotation, root.transform);
        return root;
    }

    public static GameObject CreatePalisadeSegment(Vector3 position, Quaternion rotation, float scale, Transform parent)
    {
        GameObject root = CreateRoot("PalisadeSegment", position, rotation, parent);

        for (int i = 0; i < 3; i++)
        {
            float x = (i - 1) * 0.55f * scale;
            float height = Random.Range(2.2f, 3.1f) * scale;
            CreatePrimitiveProp(root.transform, "Stake_" + i, PrimitiveType.Cylinder, new Vector3(x, height * 0.48f, 0f), Quaternion.Euler(Random.Range(-5f, 5f), 0f, Random.Range(-5f, 5f)), new Vector3(0.13f * scale, height * 0.5f, 0.13f * scale), DarkWood);
            CreatePrimitiveProp(root.transform, "Tip_" + i, PrimitiveType.Cube, new Vector3(x, height, 0f), Quaternion.Euler(45f, 0f, 45f), new Vector3(0.24f * scale, 0.32f * scale, 0.24f * scale), DarkWood);
        }

        CreatePrimitiveProp(root.transform, "BackRail", PrimitiveType.Cube, new Vector3(0f, 1.2f * scale, -0.12f * scale), Quaternion.identity, new Vector3(1.75f * scale, 0.16f * scale, 0.16f * scale), Wood);
        return root;
    }

    public static GameObject CreateOrcBanner(Vector3 position, Quaternion rotation, Transform parent)
    {
        GameObject root = CreateRoot("OrcBanner", position, rotation, parent);
        CreatePrimitiveProp(root.transform, "Pole", PrimitiveType.Cylinder, new Vector3(0f, 1.65f, 0f), Quaternion.identity, new Vector3(0.07f, 1.65f, 0.07f), DarkWood);
        CreatePrimitiveProp(root.transform, "ClothBig", PrimitiveType.Cube, new Vector3(0.45f, 2.6f, 0f), Quaternion.Euler(0f, 0f, -8f), new Vector3(0.95f, 0.72f, 0.06f), OrcRed);
        CreatePrimitiveProp(root.transform, "ClothTorn", PrimitiveType.Cube, new Vector3(0.75f, 2.05f, 0f), Quaternion.Euler(0f, 0f, -24f), new Vector3(0.55f, 0.52f, 0.06f), new Color(0.42f, 0.02f, 0.02f));
        CreatePrimitiveProp(root.transform, "SkullMark", PrimitiveType.Sphere, new Vector3(0.45f, 2.62f, 0.055f), Quaternion.identity, new Vector3(0.16f, 0.13f, 0.055f), Bone);
        return root;
    }

    public static GameObject CreateCampfire(Vector3 position, float scale, Transform parent)
    {
        GameObject root = CreateRoot("Campfire", position, Quaternion.identity, parent);
        for (int i = 0; i < 5; i++)
        {
            float a = i * 72f;
            Vector3 pos = Quaternion.Euler(0f, a, 0f) * new Vector3(0f, 0.18f * scale, 0.72f * scale);
            CreatePrimitiveProp(root.transform, "Log_" + i, PrimitiveType.Cube, pos, Quaternion.Euler(0f, a + 90f, 18f), new Vector3(1.25f * scale, 0.18f * scale, 0.18f * scale), Wood);
        }

        CreatePrimitiveProp(root.transform, "FlameCore", PrimitiveType.Sphere, new Vector3(0f, 0.65f * scale, 0f), Quaternion.identity, new Vector3(0.75f * scale, 1.05f * scale, 0.75f * scale), Fire, false, true);
        CreatePrimitiveProp(root.transform, "FlameTip", PrimitiveType.Sphere, new Vector3(0.08f * scale, 1.2f * scale, 0.02f * scale), Quaternion.identity, new Vector3(0.42f * scale, 0.75f * scale, 0.42f * scale), Color.yellow, false, true);
        return root;
    }

    public static GameObject CreateBrokenCart(Vector3 position, Quaternion rotation, Transform parent)
    {
        GameObject root = CreateRoot("BrokenCart", position, rotation, parent);
        CreatePrimitiveProp(root.transform, "CartBody", PrimitiveType.Cube, new Vector3(0f, 0.38f, 0f), Quaternion.Euler(0f, 0f, Random.Range(-5f, 5f)), new Vector3(2.35f, 0.48f, 1.25f), Wood);
        CreatePrimitiveProp(root.transform, "BrokenSide", PrimitiveType.Cube, new Vector3(0.2f, 0.72f, 0.72f), Quaternion.Euler(0f, 0f, -12f), new Vector3(2.0f, 0.18f, 0.16f), DarkWood);
        CreatePrimitiveProp(root.transform, "WheelLeft", PrimitiveType.Cylinder, new Vector3(-1.15f, 0.28f, -0.72f), Quaternion.Euler(90f, 0f, 0f), new Vector3(0.43f, 0.1f, 0.43f), DarkWood);
        CreatePrimitiveProp(root.transform, "WheelRightBroken", PrimitiveType.Cylinder, new Vector3(1.1f, 0.12f, -0.55f), Quaternion.Euler(80f, 0f, 28f), new Vector3(0.34f, 0.08f, 0.34f), DarkWood);
        CreatePrimitiveProp(root.transform, "Shaft", PrimitiveType.Cylinder, new Vector3(0f, 0.28f, 1.3f), Quaternion.Euler(75f, 0f, 0f), new Vector3(0.08f, 1.15f, 0.08f), Wood);
        return root;
    }

    public static GameObject CreateBrokenShield(Vector3 position, Quaternion rotation, Transform parent)
    {
        GameObject root = CreateRoot("BrokenShield", position, rotation, parent);
        CreatePrimitiveProp(root.transform, "ShieldPlate", PrimitiveType.Cube, new Vector3(0f, 0.08f, 0f), Quaternion.Euler(8f, 0f, 12f), new Vector3(0.8f, 0.08f, 0.95f), new Color(0.33f, 0.22f, 0.13f));
        CreatePrimitiveProp(root.transform, "ShieldCrack", PrimitiveType.Cube, new Vector3(0.08f, 0.14f, 0f), Quaternion.Euler(8f, 0f, 35f), new Vector3(0.08f, 0.09f, 0.95f), new Color(0.06f, 0.04f, 0.03f));
        CreatePrimitiveProp(root.transform, "IronBoss", PrimitiveType.Sphere, new Vector3(0f, 0.16f, 0f), Quaternion.identity, new Vector3(0.22f, 0.09f, 0.22f), new Color(0.38f, 0.36f, 0.34f));
        return root;
    }

    public static GameObject CreateSpearInGround(Vector3 position, Quaternion rotation, Transform parent)
    {
        GameObject root = CreateRoot("SpearInGround", position, rotation, parent);
        CreatePrimitiveProp(root.transform, "Shaft", PrimitiveType.Cylinder, new Vector3(0f, 0.85f, 0f), Quaternion.Euler(0f, 0f, 8f), new Vector3(0.045f, 0.85f, 0.045f), Wood);
        CreatePrimitiveProp(root.transform, "Head", PrimitiveType.Cube, new Vector3(0.12f, 1.72f, 0f), Quaternion.Euler(0f, 0f, 45f), new Vector3(0.18f, 0.28f, 0.06f), new Color(0.55f, 0.55f, 0.52f));
        return root;
    }

    public static GameObject CreateCratesAndBags(Vector3 position, Quaternion rotation, Transform parent)
    {
        GameObject root = CreateRoot("CratesAndBags", position, rotation, parent);
        CreatePrimitiveProp(root.transform, "CrateA", PrimitiveType.Cube, new Vector3(-0.42f, 0.28f, 0f), Quaternion.Euler(0f, 8f, 0f), new Vector3(0.75f, 0.55f, 0.65f), Wood);
        CreatePrimitiveProp(root.transform, "CrateB", PrimitiveType.Cube, new Vector3(0.35f, 0.22f, 0.22f), Quaternion.Euler(0f, -12f, 0f), new Vector3(0.55f, 0.45f, 0.5f), DarkWood);
        CreatePrimitiveProp(root.transform, "BagA", PrimitiveType.Sphere, new Vector3(0.15f, 0.2f, -0.45f), Quaternion.identity, new Vector3(0.45f, 0.35f, 0.36f), new Color(0.46f, 0.38f, 0.24f));
        CreatePrimitiveProp(root.transform, "BagB", PrimitiveType.Sphere, new Vector3(0.62f, 0.15f, -0.25f), Quaternion.identity, new Vector3(0.33f, 0.25f, 0.3f), new Color(0.40f, 0.32f, 0.20f));
        return root;
    }

    public static GameObject CreateRouteSign(string label, Vector3 position, Quaternion rotation, Transform parent)
    {
        GameObject root = CreateRoot("RouteSign_" + SafeName(label), position, rotation, parent);
        CreatePrimitiveProp(root.transform, "Post", PrimitiveType.Cylinder, new Vector3(0f, 0.9f, 0f), Quaternion.identity, new Vector3(0.08f, 0.9f, 0.08f), Wood);
        CreatePrimitiveProp(root.transform, "Board", PrimitiveType.Cube, new Vector3(0f, 1.55f, 0.03f), Quaternion.identity, new Vector3(1.55f, 0.42f, 0.08f), new Color(0.46f, 0.28f, 0.10f));

        GameObject textObj = new GameObject("Label");
        textObj.transform.SetParent(root.transform, false);
        textObj.transform.localPosition = new Vector3(0f, 1.57f, 0.13f);
        textObj.transform.localRotation = Quaternion.identity;

        TextMesh tm = textObj.AddComponent<TextMesh>();
        tm.text = label;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.characterSize = 0.09f;
        tm.fontSize = 30;
        tm.color = Color.white;

        L0Label billboard = textObj.AddComponent<L0Label>();
        billboard.visibleDistance = 10.5f;
        billboard.fadeByDistance = true;

        return root;
    }

    public static GameObject CreateSmokePuff(Vector3 position, float scale, Transform parent)
    {
        GameObject root = CreateRoot("SmokePuff", position, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f), parent);
        Color smoke = new Color(0.22f, 0.22f, 0.22f, 0.45f);
        CreatePrimitiveProp(root.transform, "PuffA", PrimitiveType.Sphere, Vector3.zero, Quaternion.identity, new Vector3(0.8f * scale, 0.55f * scale, 0.8f * scale), smoke, false, false, true);
        CreatePrimitiveProp(root.transform, "PuffB", PrimitiveType.Sphere, new Vector3(0.45f * scale, 0.25f * scale, 0f), Quaternion.identity, new Vector3(0.65f * scale, 0.45f * scale, 0.65f * scale), smoke, false, false, true);
        CreatePrimitiveProp(root.transform, "PuffC", PrimitiveType.Sphere, new Vector3(-0.38f * scale, 0.18f * scale, 0.22f * scale), Quaternion.identity, new Vector3(0.55f * scale, 0.4f * scale, 0.55f * scale), smoke, false, false, true);
        return root;
    }

    public static GameObject CreateFenceSegment(Vector3 position, Quaternion rotation, Transform parent)
    {
        GameObject root = CreateRoot("RefugeeFenceSegment", position, rotation, parent);
        for (int i = 0; i < 4; i++)
        {
            float x = (i - 1.5f) * 0.85f;
            CreatePrimitiveProp(root.transform, "Post_" + i, PrimitiveType.Cylinder, new Vector3(x, 0.55f, 0f), Quaternion.identity, new Vector3(0.06f, 0.55f, 0.06f), Wood);
        }
        CreatePrimitiveProp(root.transform, "RailTop", PrimitiveType.Cube, new Vector3(0f, 0.82f, 0f), Quaternion.identity, new Vector3(3.1f, 0.08f, 0.08f), Wood);
        CreatePrimitiveProp(root.transform, "RailLow", PrimitiveType.Cube, new Vector3(0f, 0.45f, 0f), Quaternion.identity, new Vector3(3.1f, 0.08f, 0.08f), Wood);
        return root;
    }

    public static GameObject CreateWell(Vector3 position, Transform parent)
    {
        GameObject root = CreateRoot("VillageWell", position, Quaternion.identity, parent);
        CreatePrimitiveProp(root.transform, "StoneRing", PrimitiveType.Cylinder, new Vector3(0f, 0.35f, 0f), Quaternion.identity, new Vector3(0.95f, 0.35f, 0.95f), new Color(0.40f, 0.37f, 0.32f));
        CreatePrimitiveProp(root.transform, "Water", PrimitiveType.Cylinder, new Vector3(0f, 0.38f, 0f), Quaternion.identity, new Vector3(0.72f, 0.03f, 0.72f), new Color(0.05f, 0.09f, 0.15f));
        CreatePrimitiveProp(root.transform, "PostL", PrimitiveType.Cylinder, new Vector3(-0.75f, 1.1f, 0f), Quaternion.identity, new Vector3(0.08f, 1.1f, 0.08f), Wood);
        CreatePrimitiveProp(root.transform, "PostR", PrimitiveType.Cylinder, new Vector3(0.75f, 1.1f, 0f), Quaternion.identity, new Vector3(0.08f, 1.1f, 0.08f), Wood);
        CreatePrimitiveProp(root.transform, "Beam", PrimitiveType.Cube, new Vector3(0f, 2.05f, 0f), Quaternion.identity, new Vector3(1.75f, 0.12f, 0.12f), Wood);
        CreatePrimitiveProp(root.transform, "Bucket", PrimitiveType.Cube, new Vector3(0f, 0.8f, 0.18f), Quaternion.identity, new Vector3(0.32f, 0.32f, 0.32f), DarkWood);
        return root;
    }

    // ---------- Additional visual helpers ----------

    public static GameObject CreateGroundPatch(Vector3 position, Vector3 scale, Color color, Transform parent, string name)
    {
        return CreateGroundPatch(position, scale, color, parent, name, Quaternion.identity);
    }

    public static GameObject CreateGroundPatch(Vector3 position, Vector3 scale, Color color, Transform parent, string name, Quaternion rotation)
    {
        GameObject root = CreateRoot(name, position, rotation, parent);
        CreatePrimitiveProp(root.transform, "Patch", PrimitiveType.Cube, Vector3.zero, Quaternion.identity, scale, color);
        return root;
    }

    public static GameObject CreateGroundPatchWalkable(Vector3 position, Vector3 scale, Color color, Transform parent, string name, Quaternion rotation)
    {
        GameObject root = CreateRoot(name, position, rotation, parent);
        CreatePrimitiveProp(root.transform, "Patch", PrimitiveType.Cube, Vector3.zero, Quaternion.identity, scale, color, true, false, false);
        return root;
    }

    public static GameObject CreateSupportFloor(Vector3 position, Vector3 scale, Transform parent, string name)
    {
        return CreateSupportFloor(position, scale, parent, name, default, false);
    }

    public static GameObject CreateSupportFloor(Vector3 position, Vector3 scale, Transform parent, string name, Color color, bool visible)
    {
        GameObject root = CreateRoot(name, position, Quaternion.identity, parent);
        GameObject floor = CreatePrimitiveProp(root.transform, "Floor", PrimitiveType.Cube, Vector3.zero, Quaternion.identity, scale, visible ? color : new Color(0f, 0f, 0f, 0f), true, false, false);
        Renderer r = floor.GetComponent<Renderer>();
        if (r != null && !visible) r.enabled = false;
        return root;
    }

    public static GameObject CreateHaystack(Vector3 position, float scale, Transform parent)
    {
        GameObject root = CreateRoot("Haystack", position, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f), parent);
        Color hay = new Color(0.74f, 0.62f, 0.22f);
        CreatePrimitiveProp(root.transform, "Base", PrimitiveType.Cube, new Vector3(0f, 0.32f * scale, 0f), Quaternion.Euler(0f, 0f, 4f), new Vector3(1.7f * scale, 0.65f * scale, 1.2f * scale), hay);
        CreatePrimitiveProp(root.transform, "Top", PrimitiveType.Cube, new Vector3(0f, 0.82f * scale, 0f), Quaternion.Euler(0f, 0f, -5f), new Vector3(1.28f * scale, 0.52f * scale, 0.92f * scale), hay);
        return root;
    }

    public static GameObject CreateBench(Vector3 position, Quaternion rotation, Transform parent)
    {
        GameObject root = CreateRoot("VillageBench", position, rotation, parent);
        CreatePrimitiveProp(root.transform, "Seat", PrimitiveType.Cube, new Vector3(0f, 0.45f, 0f), Quaternion.identity, new Vector3(1.8f, 0.16f, 0.42f), Wood);
        CreatePrimitiveProp(root.transform, "LegL", PrimitiveType.Cube, new Vector3(-0.65f, 0.23f, 0f), Quaternion.identity, new Vector3(0.16f, 0.45f, 0.34f), DarkWood);
        CreatePrimitiveProp(root.transform, "LegR", PrimitiveType.Cube, new Vector3(0.65f, 0.23f, 0f), Quaternion.identity, new Vector3(0.16f, 0.45f, 0.34f), DarkWood);
        return root;
    }

    public static GameObject CreateLantern(Vector3 position, Transform parent)
    {
        GameObject root = CreateRoot("RoadLanternTorch", position, Quaternion.identity, parent);
        CreatePrimitiveProp(root.transform, "Post", PrimitiveType.Cylinder, new Vector3(0f, 1f, 0f), Quaternion.identity, new Vector3(0.06f, 1f, 0.06f), Wood);
        CreatePrimitiveProp(root.transform, "Head", PrimitiveType.Cube, new Vector3(0f, 2.03f, 0f), Quaternion.identity, new Vector3(0.28f, 0.32f, 0.28f), DarkWood);
        CreatePrimitiveProp(root.transform, "Glow", PrimitiveType.Sphere, new Vector3(0f, 2.05f, 0f), Quaternion.identity, new Vector3(0.22f, 0.22f, 0.22f), Fire, false, true);
        return root;
    }

    public static GameObject CreateBattleArrowCluster(Vector3 position, Transform parent)
    {
        GameObject root = CreateRoot("ArrowCluster", position, Quaternion.identity, parent);
        int count = Random.Range(2, 5);
        for (int i = 0; i < count; i++)
        {
            Vector3 local = new Vector3(Random.Range(-0.45f, 0.45f), 0.22f, Random.Range(-0.45f, 0.45f));
            Quaternion rot = Quaternion.Euler(Random.Range(55f, 78f), Random.Range(0f, 360f), 0f);
            CreatePrimitiveProp(root.transform, "ArrowShaft_" + i, PrimitiveType.Cylinder, local, rot, new Vector3(0.018f, 0.42f, 0.018f), Wood);
            CreatePrimitiveProp(root.transform, "ArrowHead_" + i, PrimitiveType.Cube, local + new Vector3(0f, 0.2f, 0f), rot, new Vector3(0.06f, 0.08f, 0.03f), new Color(0.54f, 0.54f, 0.5f));
        }
        return root;
    }

    public static GameObject CreateWeaponRack(Vector3 position, Quaternion rotation, Transform parent)
    {
        GameObject root = CreateRoot("OrcWeaponRack", position, rotation, parent);
        CreatePrimitiveProp(root.transform, "RackBase", PrimitiveType.Cube, new Vector3(0f, 0.25f, 0f), Quaternion.identity, new Vector3(1.4f, 0.16f, 0.42f), DarkWood);
        CreatePrimitiveProp(root.transform, "RackBack", PrimitiveType.Cube, new Vector3(0f, 1.0f, -0.18f), Quaternion.identity, new Vector3(1.55f, 1.25f, 0.12f), Wood);
        for (int i = 0; i < 4; i++)
        {
            float x = -0.55f + i * 0.36f;
            CreatePrimitiveProp(root.transform, "RackSpear_" + i, PrimitiveType.Cylinder, new Vector3(x, 1.0f, 0.05f), Quaternion.Euler(8f, 0f, Random.Range(-8f, 8f)), new Vector3(0.035f, 0.95f, 0.035f), Wood);
            CreatePrimitiveProp(root.transform, "RackBlade_" + i, PrimitiveType.Cube, new Vector3(x, 1.95f, 0.05f), Quaternion.Euler(0f, 0f, 45f), new Vector3(0.14f, 0.2f, 0.04f), new Color(0.55f, 0.54f, 0.5f));
        }
        return root;
    }

    public static GameObject CreateCage(Vector3 position, Quaternion rotation, float scale, Transform parent)
    {
        GameObject root = CreateRoot("OrcCage", position, rotation, parent);
        CreatePrimitiveProp(root.transform, "Floor", PrimitiveType.Cube, new Vector3(0f, 0.08f * scale, 0f), Quaternion.identity, new Vector3(1.9f * scale, 0.15f * scale, 1.9f * scale), DarkWood);
        CreatePrimitiveProp(root.transform, "Roof", PrimitiveType.Cube, new Vector3(0f, 2f * scale, 0f), Quaternion.identity, new Vector3(1.9f * scale, 0.15f * scale, 1.9f * scale), DarkWood);
        for (int i = 0; i < 8; i++)
        {
            float x = (i % 4 == 0 || i % 4 == 3) ? -0.85f * scale : 0.85f * scale;
            float z = (i < 4) ? -0.85f * scale + (i % 4) * 0.57f * scale : 0.85f * scale - (i % 4) * 0.57f * scale;
            CreatePrimitiveProp(root.transform, "Bar_" + i, PrimitiveType.Cylinder, new Vector3(x, 1.05f * scale, z), Quaternion.identity, new Vector3(0.045f * scale, 1.0f * scale, 0.045f * scale), Wood);
        }
        return root;
    }

    public static GameObject CreateChiefPlatform(Vector3 position, Quaternion rotation, float scale, Transform parent)
    {
        GameObject root = CreateRoot("OrcChiefPlatform", position, rotation, parent);
        CreatePrimitiveProp(root.transform, "PlatformBase", PrimitiveType.Cube, new Vector3(0f, 0.35f * scale, 0f), Quaternion.identity, new Vector3(5.2f * scale, 0.7f * scale, 3.8f * scale), DarkWood);
        CreatePrimitiveProp(root.transform, "BackBannerPole", PrimitiveType.Cylinder, new Vector3(0f, 2.0f * scale, -1.55f * scale), Quaternion.identity, new Vector3(0.1f * scale, 2.0f * scale, 0.1f * scale), Wood);
        CreatePrimitiveProp(root.transform, "ChiefBanner", PrimitiveType.Cube, new Vector3(0f, 3.15f * scale, -1.48f * scale), Quaternion.identity, new Vector3(2.8f * scale, 1.1f * scale, 0.08f * scale), OrcRed);
        CreatePrimitiveProp(root.transform, "SkullCenter", PrimitiveType.Sphere, new Vector3(0f, 3.18f * scale, -1.38f * scale), Quaternion.identity, new Vector3(0.32f * scale, 0.26f * scale, 0.08f * scale), Bone);
        CreatePrimitiveProp(root.transform, "Step", PrimitiveType.Cube, new Vector3(0f, 0.16f * scale, 2.1f * scale), Quaternion.identity, new Vector3(3.4f * scale, 0.32f * scale, 0.7f * scale), Wood);
        return root;
    }

    public static GameObject CreateOrcGate(Vector3 position, Quaternion rotation, float scale, Transform parent)
    {
        GameObject root = CreateRoot("OrcStrongholdGate", position, rotation, parent);
        CreatePrimitiveProp(root.transform, "LeftPost", PrimitiveType.Cylinder, new Vector3(-3.1f * scale, 2.1f * scale, 0f), Quaternion.identity, new Vector3(0.22f * scale, 2.1f * scale, 0.22f * scale), DarkWood);
        CreatePrimitiveProp(root.transform, "RightPost", PrimitiveType.Cylinder, new Vector3(3.1f * scale, 2.1f * scale, 0f), Quaternion.identity, new Vector3(0.22f * scale, 2.1f * scale, 0.22f * scale), DarkWood);
        CreatePrimitiveProp(root.transform, "TopBeam", PrimitiveType.Cube, new Vector3(0f, 4.15f * scale, 0f), Quaternion.identity, new Vector3(6.8f * scale, 0.34f * scale, 0.34f * scale), Wood);
        CreatePrimitiveProp(root.transform, "RedWarning", PrimitiveType.Cube, new Vector3(0f, 3.5f * scale, -0.08f * scale), Quaternion.identity, new Vector3(1.9f * scale, 0.62f * scale, 0.08f * scale), OrcRed);
        for (int i = 0; i < 5; i++)
        {
            float x = -2.2f * scale + i * 1.1f * scale;
            CreatePrimitiveProp(root.transform, "GateSpike_" + i, PrimitiveType.Cylinder, new Vector3(x, 4.85f * scale, 0f), Quaternion.identity, new Vector3(0.08f * scale, 0.65f * scale, 0.08f * scale), Bone);
        }
        return root;
    }

    public static GameObject CreateRockSpire(Vector3 position, float scale, Transform parent)
    {
        // A6 safety: RockSpire was the easiest legacy path for tall mountains to re-enter
        // the Level0 orc arena. It is now allowed only as a very far, low boulder cluster.
        float safeScale = Mathf.Max(0.1f, scale);
        float footprint = safeScale * 8f;

        if (L0OrcArenaConfig.GetArenaLocalRadius(position) - footprint < L0OrcArenaConfig.DistantBackdropMinRadius)
            return null;

        if (!L0OrcArenaConfig.IsBackdropFootprintOutsideVisualForbiddenZones(position, footprint))
            return null;

        GameObject root = CreateRoot("RockSpire_DISABLED_LowFarBoulder", position, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f), parent);
        CreatePrimitiveProp(root.transform, "LowBoulderA_NoCollider", PrimitiveType.Sphere, new Vector3(-0.45f * safeScale, 0.28f * safeScale, 0f), Quaternion.identity, new Vector3(1.7f * safeScale, 0.45f * safeScale, 1.25f * safeScale), new Color(0.18f, 0.21f, 0.24f));
        CreatePrimitiveProp(root.transform, "LowBoulderB_NoCollider", PrimitiveType.Sphere, new Vector3(0.55f * safeScale, 0.22f * safeScale, 0.22f * safeScale), Quaternion.identity, new Vector3(1.25f * safeScale, 0.34f * safeScale, 1.05f * safeScale), new Color(0.13f, 0.16f, 0.19f));
        CreatePrimitiveProp(root.transform, "LowBoulderC_NoCollider", PrimitiveType.Cube, new Vector3(0f, 0.12f * safeScale, -0.42f * safeScale), Quaternion.Euler(0f, Random.Range(-12f, 12f), 0f), new Vector3(2.2f * safeScale, 0.24f * safeScale, 1.15f * safeScale), new Color(0.10f, 0.13f, 0.16f));
        return root;
    }

    public static GameObject CreateExitArch(Vector3 position, Quaternion rotation, float scale, Transform parent)
    {
        GameObject root = CreateRoot("ExitPassWoodenArch", position, rotation, parent);
        CreatePrimitiveProp(root.transform, "LeftArchPost", PrimitiveType.Cylinder, new Vector3(-2.5f * scale, 1.75f * scale, 0f), Quaternion.Euler(0f, 0f, -4f), new Vector3(0.16f * scale, 1.75f * scale, 0.16f * scale), DarkWood);
        CreatePrimitiveProp(root.transform, "RightArchPost", PrimitiveType.Cylinder, new Vector3(2.5f * scale, 1.75f * scale, 0f), Quaternion.Euler(0f, 0f, 4f), new Vector3(0.16f * scale, 1.75f * scale, 0.16f * scale), DarkWood);
        CreatePrimitiveProp(root.transform, "ArchBeam", PrimitiveType.Cube, new Vector3(0f, 3.35f * scale, 0f), Quaternion.identity, new Vector3(5.4f * scale, 0.28f * scale, 0.28f * scale), Wood);
        CreatePrimitiveProp(root.transform, "HangingCloth", PrimitiveType.Cube, new Vector3(0f, 2.85f * scale, -0.08f * scale), Quaternion.identity, new Vector3(1.5f * scale, 0.6f * scale, 0.08f * scale), new Color(0.33f, 0.05f, 0.03f));
        return root;
    }

    public static GameObject CreateOrcTuskPillar(Vector3 position, Quaternion rotation, float scale, Transform parent)
    {
        GameObject root = CreateRoot("OrcGateTuskPillar", position, rotation, parent);
        CreatePrimitiveProp(root.transform, "GroundSocket", PrimitiveType.Cylinder, new Vector3(0f, 0.16f * scale, 0f), Quaternion.identity, new Vector3(0.55f * scale, 0.16f * scale, 0.55f * scale), DarkWood);
        CreatePrimitiveProp(root.transform, "MainFang", PrimitiveType.Cylinder, new Vector3(0f, 2.0f * scale, 0f), Quaternion.Euler(-12f, 0f, 18f), new Vector3(0.18f * scale, 1.95f * scale, 0.18f * scale), Bone);
        CreatePrimitiveProp(root.transform, "FangTip", PrimitiveType.Cube, new Vector3(0.45f * scale, 3.82f * scale, 0f), Quaternion.Euler(0f, 0f, 48f), new Vector3(0.28f * scale, 0.42f * scale, 0.28f * scale), Bone);
        CreatePrimitiveProp(root.transform, "BindingLow", PrimitiveType.Cube, new Vector3(0.03f * scale, 0.95f * scale, 0f), Quaternion.Euler(0f, 0f, 15f), new Vector3(0.6f * scale, 0.12f * scale, 0.12f * scale), OrcRed);
        CreatePrimitiveProp(root.transform, "BindingHigh", PrimitiveType.Cube, new Vector3(0.18f * scale, 2.25f * scale, 0f), Quaternion.Euler(0f, 0f, 15f), new Vector3(0.55f * scale, 0.10f * scale, 0.10f * scale), DarkWood);
        CreatePrimitiveProp(root.transform, "SmallSkull", PrimitiveType.Sphere, new Vector3(-0.38f * scale, 1.35f * scale, 0.08f * scale), Quaternion.identity, new Vector3(0.28f * scale, 0.22f * scale, 0.12f * scale), Bone);
        return root;
    }

    public static GameObject CreateShamanTower(Vector3 position, Quaternion rotation, float scale, Transform parent)
    {
        GameObject root = CreateRoot("OrcShamanTower", position, rotation, parent);
        float h = 6.4f * scale;
        CreatePrimitiveProp(root.transform, "StoneFoot", PrimitiveType.Cylinder, new Vector3(0f, 0.18f * scale, 0f), Quaternion.identity, new Vector3(1.35f * scale, 0.18f * scale, 1.35f * scale), new Color(0.20f, 0.18f, 0.15f));

        for (int i = 0; i < 4; i++)
        {
            float x = (i < 2 ? -1f : 1f) * 1.05f * scale;
            float z = (i % 2 == 0 ? -1f : 1f) * 1.05f * scale;
            CreatePrimitiveProp(root.transform, "Leg_" + i, PrimitiveType.Cylinder, new Vector3(x, h * 0.42f, z), Quaternion.Euler(z * 3f, 0f, -x * 5f), new Vector3(0.11f * scale, h * 0.44f, 0.11f * scale), DarkWood);
        }

        CreatePrimitiveProp(root.transform, "HighPlatform", PrimitiveType.Cube, new Vector3(0f, h * 0.82f, 0f), Quaternion.identity, new Vector3(3.0f * scale, 0.28f * scale, 3.0f * scale), Wood);
        CreatePrimitiveProp(root.transform, "RagRoof", PrimitiveType.Cube, new Vector3(0f, h * 0.98f, 0f), Quaternion.Euler(0f, 45f, 0f), new Vector3(3.6f * scale, 0.36f * scale, 3.6f * scale), OrcCloth);
        CreatePrimitiveProp(root.transform, "RedHangingBanner", PrimitiveType.Cube, new Vector3(0f, h * 0.68f, 1.42f * scale), Quaternion.identity, new Vector3(1.15f * scale, 1.7f * scale, 0.08f * scale), OrcRed);
        CreatePrimitiveProp(root.transform, "SkullBannerMark", PrimitiveType.Sphere, new Vector3(0f, h * 0.74f, 1.48f * scale), Quaternion.identity, new Vector3(0.32f * scale, 0.25f * scale, 0.09f * scale), Bone);
        CreatePrimitiveProp(root.transform, "SignalPole", PrimitiveType.Cylinder, new Vector3(0f, h * 1.18f, 0f), Quaternion.identity, new Vector3(0.08f * scale, 1.25f * scale, 0.08f * scale), DarkWood);
        CreatePrimitiveProp(root.transform, "SignalFlag", PrimitiveType.Cube, new Vector3(0.55f * scale, h * 1.46f, 0f), Quaternion.Euler(0f, 0f, -7f), new Vector3(1.25f * scale, 0.78f * scale, 0.07f * scale), new Color(0.48f, 0.015f, 0.015f));
        return root;
    }

    public static GameObject CreateWarAltarCircle(Vector3 position, Quaternion rotation, float scale, Transform parent)
    {
        GameObject root = CreateRoot("OrcWarAltarCircle", position, rotation, parent);
        CreatePrimitiveProp(root.transform, "StoneRingBase", PrimitiveType.Cylinder, new Vector3(0f, 0.07f * scale, 0f), Quaternion.identity, new Vector3(2.9f * scale, 0.07f * scale, 2.9f * scale), new Color(0.16f, 0.13f, 0.10f), true, false);
        CreatePrimitiveProp(root.transform, "AltarSlab", PrimitiveType.Cube, new Vector3(0f, 0.42f * scale, 0f), Quaternion.Euler(0f, 8f, 0f), new Vector3(2.0f * scale, 0.42f * scale, 1.35f * scale), DarkWood);
        CreatePrimitiveProp(root.transform, "RedMark", PrimitiveType.Cube, new Vector3(0f, 0.66f * scale, 0.05f * scale), Quaternion.identity, new Vector3(1.25f * scale, 0.06f * scale, 0.58f * scale), OrcRed);

        for (int i = 0; i < 8; i++)
        {
            float a = i * 45f;
            Vector3 p = Quaternion.Euler(0f, a, 0f) * new Vector3(0f, 0.22f * scale, 2.35f * scale);
            CreatePrimitiveProp(root.transform, "RingStone_" + i, PrimitiveType.Sphere, p, Quaternion.identity, new Vector3(0.35f * scale, 0.18f * scale, 0.28f * scale), new Color(0.24f, 0.22f, 0.19f));
        }

        CreatePrimitiveProp(root.transform, "FireBowlL", PrimitiveType.Cylinder, new Vector3(-1.45f * scale, 0.55f * scale, 0.15f * scale), Quaternion.Euler(90f, 0f, 0f), new Vector3(0.28f * scale, 0.15f * scale, 0.28f * scale), new Color(0.18f, 0.16f, 0.15f));
        CreatePrimitiveProp(root.transform, "FireBowlR", PrimitiveType.Cylinder, new Vector3(1.45f * scale, 0.55f * scale, 0.15f * scale), Quaternion.Euler(90f, 0f, 0f), new Vector3(0.28f * scale, 0.15f * scale, 0.28f * scale), new Color(0.18f, 0.16f, 0.15f));
        CreatePrimitiveProp(root.transform, "FireL", PrimitiveType.Sphere, new Vector3(-1.45f * scale, 0.85f * scale, 0.15f * scale), Quaternion.identity, new Vector3(0.35f * scale, 0.52f * scale, 0.35f * scale), Fire, false, true);
        CreatePrimitiveProp(root.transform, "FireR", PrimitiveType.Sphere, new Vector3(1.45f * scale, 0.85f * scale, 0.15f * scale), Quaternion.identity, new Vector3(0.35f * scale, 0.52f * scale, 0.35f * scale), Fire, false, true);
        return root;
    }

    public static GameObject CreateFutureLevelTwoExitPlatform(Vector3 position, Quaternion rotation, float scale, Transform parent)
    {
        GameObject root = CreateRoot("Future_Level_Two_Exit_Platform", position, rotation, parent);
        CreatePrimitiveProp(root.transform, "StoneCircleFloor", PrimitiveType.Cylinder, new Vector3(0f, 0.08f * scale, 0f), Quaternion.identity, new Vector3(4.2f * scale, 0.08f * scale, 4.2f * scale), new Color(0.21f, 0.16f, 0.11f), true, false);
        CreatePrimitiveProp(root.transform, "InnerDarkCircle", PrimitiveType.Cylinder, new Vector3(0f, 0.13f * scale, 0f), Quaternion.identity, new Vector3(2.7f * scale, 0.035f * scale, 2.7f * scale), new Color(0.10f, 0.065f, 0.045f));
        CreatePrimitiveProp(root.transform, "BackLeftPost", PrimitiveType.Cylinder, new Vector3(-2.3f * scale, 1.6f * scale, -1.1f * scale), Quaternion.Euler(0f, 0f, -4f), new Vector3(0.16f * scale, 1.6f * scale, 0.16f * scale), DarkWood);
        CreatePrimitiveProp(root.transform, "BackRightPost", PrimitiveType.Cylinder, new Vector3(2.3f * scale, 1.6f * scale, -1.1f * scale), Quaternion.Euler(0f, 0f, 4f), new Vector3(0.16f * scale, 1.6f * scale, 0.16f * scale), DarkWood);
        CreatePrimitiveProp(root.transform, "ExitBeam", PrimitiveType.Cube, new Vector3(0f, 3.0f * scale, -1.1f * scale), Quaternion.identity, new Vector3(5.2f * scale, 0.26f * scale, 0.26f * scale), Wood);
        CreatePrimitiveProp(root.transform, "TwoBanner", PrimitiveType.Cube, new Vector3(0f, 2.45f * scale, -1.18f * scale), Quaternion.identity, new Vector3(1.8f * scale, 0.72f * scale, 0.07f * scale), OrcRed);
        CreatePrimitiveProp(root.transform, "TorchL", PrimitiveType.Sphere, new Vector3(-3.1f * scale, 1.1f * scale, 1.15f * scale), Quaternion.identity, new Vector3(0.35f * scale, 0.55f * scale, 0.35f * scale), Fire, false, true);
        CreatePrimitiveProp(root.transform, "TorchR", PrimitiveType.Sphere, new Vector3(3.1f * scale, 1.1f * scale, 1.15f * scale), Quaternion.identity, new Vector3(0.35f * scale, 0.55f * scale, 0.35f * scale), Fire, false, true);
        return root;
    }


    public static GameObject CreateSimpleCart(Vector3 position, Quaternion rotation, Transform parent)
    {
        GameObject root = CreateRoot("VillageCart", position, rotation, parent);
        CreatePrimitiveProp(root.transform, "CartBody", PrimitiveType.Cube, new Vector3(0f, 0.42f, 0f), Quaternion.identity, new Vector3(2.55f, 0.52f, 1.28f), Wood);
        CreatePrimitiveProp(root.transform, "SideRailA", PrimitiveType.Cube, new Vector3(0f, 0.82f, 0.68f), Quaternion.identity, new Vector3(2.65f, 0.18f, 0.14f), DarkWood);
        CreatePrimitiveProp(root.transform, "SideRailB", PrimitiveType.Cube, new Vector3(0f, 0.82f, -0.68f), Quaternion.identity, new Vector3(2.65f, 0.18f, 0.14f), DarkWood);
        CreatePrimitiveProp(root.transform, "WheelL", PrimitiveType.Cylinder, new Vector3(-1.18f, 0.27f, -0.78f), Quaternion.Euler(90f, 0f, 0f), new Vector3(0.43f, 0.1f, 0.43f), DarkWood);
        CreatePrimitiveProp(root.transform, "WheelR", PrimitiveType.Cylinder, new Vector3(1.18f, 0.27f, -0.78f), Quaternion.Euler(90f, 0f, 0f), new Vector3(0.43f, 0.1f, 0.43f), DarkWood);
        CreatePrimitiveProp(root.transform, "FrontShaft", PrimitiveType.Cylinder, new Vector3(0f, 0.32f, 1.35f), Quaternion.Euler(75f, 0f, 0f), new Vector3(0.08f, 1.2f, 0.08f), Wood);
        return root;
    }

    public static GameObject CreateSpikeCluster(Vector3 position, Quaternion rotation, float scale, Transform parent)
    {
        GameObject root = CreateRoot("SpikeCluster", position, rotation, parent);
        int count = Random.Range(3, 6);
        for (int i = 0; i < count; i++)
        {
            float x = Random.Range(-0.75f, 0.75f) * scale;
            float z = Random.Range(-0.45f, 0.45f) * scale;
            float h = Random.Range(1.25f, 2.1f) * scale;
            Quaternion rot = Quaternion.Euler(Random.Range(10f, 28f), Random.Range(0f, 360f), Random.Range(-12f, 12f));
            CreatePrimitiveProp(root.transform, "SpikeBody_" + i, PrimitiveType.Cylinder, new Vector3(x, h * 0.45f, z), rot, new Vector3(0.06f * scale, h * 0.5f, 0.06f * scale), DarkWood);
            CreatePrimitiveProp(root.transform, "SpikeTip_" + i, PrimitiveType.Cube, new Vector3(x, h, z), rot * Quaternion.Euler(45f, 0f, 45f), new Vector3(0.16f * scale, 0.22f * scale, 0.16f * scale), Bone);
        }
        return root;
    }

    public static GameObject CreateOrcTotem(Vector3 position, Quaternion rotation, float scale, Transform parent)
    {
        GameObject root = CreateRoot("OrcTotem", position, rotation, parent);
        CreatePrimitiveProp(root.transform, "MainPole", PrimitiveType.Cylinder, new Vector3(0f, 2.25f * scale, 0f), Quaternion.identity, new Vector3(0.16f * scale, 2.25f * scale, 0.16f * scale), DarkWood);
        CreatePrimitiveProp(root.transform, "CrossBeam", PrimitiveType.Cube, new Vector3(0f, 3.4f * scale, 0f), Quaternion.Euler(0f, 0f, 6f), new Vector3(2.3f * scale, 0.18f * scale, 0.18f * scale), Wood);
        CreatePrimitiveProp(root.transform, "RedRag", PrimitiveType.Cube, new Vector3(0.52f * scale, 3.0f * scale, 0.04f * scale), Quaternion.Euler(0f, 0f, -12f), new Vector3(0.9f * scale, 0.75f * scale, 0.06f * scale), OrcRed);
        CreatePrimitiveProp(root.transform, "BoneMask", PrimitiveType.Sphere, new Vector3(0f, 3.75f * scale, 0.08f * scale), Quaternion.identity, new Vector3(0.46f * scale, 0.34f * scale, 0.14f * scale), Bone);
        CreatePrimitiveProp(root.transform, "HornL", PrimitiveType.Cylinder, new Vector3(-0.36f * scale, 3.95f * scale, 0.08f * scale), Quaternion.Euler(0f, 0f, 55f), new Vector3(0.05f * scale, 0.42f * scale, 0.05f * scale), Bone);
        CreatePrimitiveProp(root.transform, "HornR", PrimitiveType.Cylinder, new Vector3(0.36f * scale, 3.95f * scale, 0.08f * scale), Quaternion.Euler(0f, 0f, -55f), new Vector3(0.05f * scale, 0.42f * scale, 0.05f * scale), Bone);
        return root;
    }

    public static GameObject CreateWoodBridge(Vector3 position, Quaternion rotation, float scale, Transform parent)
    {
        GameObject root = CreateRoot("ExitWoodBridge", position, rotation, parent);
        for (int i = 0; i < 7; i++)
        {
            float z = -2.4f * scale + i * 0.8f * scale;
            CreatePrimitiveProp(root.transform, "Plank_" + i, PrimitiveType.Cube, new Vector3(0f, 0.12f * scale, z), Quaternion.Euler(0f, Random.Range(-2f, 2f), 0f), new Vector3(4.8f * scale, 0.16f * scale, 0.55f * scale), (i % 2 == 0) ? Wood : DarkWood);
        }
        CreatePrimitiveProp(root.transform, "RailL", PrimitiveType.Cube, new Vector3(-2.55f * scale, 0.55f * scale, 0f), Quaternion.identity, new Vector3(0.18f * scale, 0.22f * scale, 5.8f * scale), DarkWood);
        CreatePrimitiveProp(root.transform, "RailR", PrimitiveType.Cube, new Vector3(2.55f * scale, 0.55f * scale, 0f), Quaternion.identity, new Vector3(0.18f * scale, 0.22f * scale, 5.8f * scale), DarkWood);
        return root;
    }

    public static GameObject CreatePointLight(string name, Vector3 position, Color color, float intensity, float range, Transform parent)
    {
        GameObject lightObj = new GameObject(name);
        lightObj.transform.SetParent(parent, true);
        lightObj.transform.position = position;
        Light light = lightObj.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = color;
        light.intensity = intensity;
        light.range = range;
        return lightObj;
    }


    public static GameObject CreateLargeVillageHall(Vector3 position, Quaternion rotation, float scale, Transform parent)
    {
        GameObject root = CreateRoot("VillageHall", position, rotation, parent);
        float w = 4.4f * scale;
        float h = 2.85f * scale;
        float d = 4.9f * scale;
        CreatePrimitiveProp(root.transform, "HallWalls", PrimitiveType.Cube, new Vector3(0f, h * 0.5f, 0f), Quaternion.identity, new Vector3(w, h, d), new Color(0.50f, 0.42f, 0.31f));
        CreatePrimitiveProp(root.transform, "HallRoofA", PrimitiveType.Cube, new Vector3(0f, h + 0.45f * scale, -0.5f * scale), Quaternion.Euler(23f, 0f, 0f), new Vector3(w + 0.55f * scale, 0.55f * scale, d + 0.55f * scale), new Color(0.43f, 0.18f, 0.10f));
        CreatePrimitiveProp(root.transform, "HallRoofB", PrimitiveType.Cube, new Vector3(0f, h + 0.45f * scale, 0.5f * scale), Quaternion.Euler(-23f, 0f, 0f), new Vector3(w + 0.55f * scale, 0.55f * scale, d + 0.55f * scale), new Color(0.43f, 0.18f, 0.10f));
        CreatePrimitiveProp(root.transform, "WideDoor", PrimitiveType.Cube, new Vector3(0f, 0.82f * scale, d * 0.5f + 0.045f), Quaternion.identity, new Vector3(1.0f * scale, 1.55f * scale, 0.08f * scale), DarkWood);
        CreatePrimitiveProp(root.transform, "WarmWindowL", PrimitiveType.Cube, new Vector3(-w * 0.32f, 1.55f * scale, d * 0.5f + 0.05f), Quaternion.identity, new Vector3(0.48f * scale, 0.48f * scale, 0.07f * scale), new Color(1f, 0.66f, 0.25f), false, true);
        CreatePrimitiveProp(root.transform, "WarmWindowR", PrimitiveType.Cube, new Vector3(w * 0.32f, 1.55f * scale, d * 0.5f + 0.05f), Quaternion.identity, new Vector3(0.48f * scale, 0.48f * scale, 0.07f * scale), new Color(1f, 0.66f, 0.25f), false, true);
        CreatePrimitiveProp(root.transform, "Chimney", PrimitiveType.Cube, new Vector3(w * 0.25f, h + 1.0f * scale, -d * 0.2f), Quaternion.identity, new Vector3(0.42f * scale, 1.15f * scale, 0.42f * scale), new Color(0.25f, 0.22f, 0.19f));
        return root;
    }

    public static GameObject CreateMarketCanopy(Vector3 position, Quaternion rotation, float scale, Transform parent)
    {
        GameObject root = CreateRoot("VillageCanopy", position, rotation, parent);
        CreatePrimitiveProp(root.transform, "PostA", PrimitiveType.Cylinder, new Vector3(-1.25f * scale, 0.85f * scale, -0.8f * scale), Quaternion.identity, new Vector3(0.06f * scale, 0.85f * scale, 0.06f * scale), Wood);
        CreatePrimitiveProp(root.transform, "PostB", PrimitiveType.Cylinder, new Vector3(1.25f * scale, 0.85f * scale, -0.8f * scale), Quaternion.identity, new Vector3(0.06f * scale, 0.85f * scale, 0.06f * scale), Wood);
        CreatePrimitiveProp(root.transform, "PostC", PrimitiveType.Cylinder, new Vector3(-1.25f * scale, 0.85f * scale, 0.8f * scale), Quaternion.identity, new Vector3(0.06f * scale, 0.85f * scale, 0.06f * scale), Wood);
        CreatePrimitiveProp(root.transform, "PostD", PrimitiveType.Cylinder, new Vector3(1.25f * scale, 0.85f * scale, 0.8f * scale), Quaternion.identity, new Vector3(0.06f * scale, 0.85f * scale, 0.06f * scale), Wood);
        CreatePrimitiveProp(root.transform, "Canvas", PrimitiveType.Cube, new Vector3(0f, 1.75f * scale, 0f), Quaternion.Euler(0f, 0f, 2f), new Vector3(2.9f * scale, 0.12f * scale, 2.0f * scale), new Color(0.55f, 0.42f, 0.24f));
        CreatePrimitiveProp(root.transform, "Table", PrimitiveType.Cube, new Vector3(0f, 0.55f * scale, 0f), Quaternion.identity, new Vector3(1.9f * scale, 0.28f * scale, 0.75f * scale), Wood);
        return root;
    }

    public static GameObject CreateGardenPatch(Vector3 position, Quaternion rotation, float scale, Transform parent)
    {
        GameObject root = CreateRoot("VillageGarden", position, rotation, parent);
        CreatePrimitiveProp(root.transform, "Soil", PrimitiveType.Cube, Vector3.zero, Quaternion.identity, new Vector3(4.2f * scale, 0.06f, 2.3f * scale), new Color(0.20f, 0.13f, 0.07f));
        for (int i = 0; i < 4; i++)
        {
            float x = -1.55f * scale + i * 1.05f * scale;
            CreatePrimitiveProp(root.transform, "CropRow_" + i, PrimitiveType.Cube, new Vector3(x, 0.12f, 0f), Quaternion.identity, new Vector3(0.2f * scale, 0.12f, 2.0f * scale), new Color(0.13f, 0.30f, 0.10f));
        }
        return root;
    }

    public static GameObject CreateClothesLine(Vector3 position, Quaternion rotation, float scale, Transform parent)
    {
        GameObject root = CreateRoot("VillageClothesLine", position, rotation, parent);
        CreatePrimitiveProp(root.transform, "PostL", PrimitiveType.Cylinder, new Vector3(-1.5f * scale, 0.9f * scale, 0f), Quaternion.identity, new Vector3(0.055f * scale, 0.9f * scale, 0.055f * scale), Wood);
        CreatePrimitiveProp(root.transform, "PostR", PrimitiveType.Cylinder, new Vector3(1.5f * scale, 0.9f * scale, 0f), Quaternion.identity, new Vector3(0.055f * scale, 0.9f * scale, 0.055f * scale), Wood);
        CreatePrimitiveProp(root.transform, "Line", PrimitiveType.Cube, new Vector3(0f, 1.65f * scale, 0f), Quaternion.identity, new Vector3(3.1f * scale, 0.035f * scale, 0.035f * scale), new Color(0.18f, 0.16f, 0.12f));
        CreatePrimitiveProp(root.transform, "ClothA", PrimitiveType.Cube, new Vector3(-0.55f * scale, 1.33f * scale, 0.02f * scale), Quaternion.Euler(0f, 0f, -3f), new Vector3(0.5f * scale, 0.62f * scale, 0.04f * scale), new Color(0.58f, 0.50f, 0.38f));
        CreatePrimitiveProp(root.transform, "ClothB", PrimitiveType.Cube, new Vector3(0.35f * scale, 1.30f * scale, 0.02f * scale), Quaternion.Euler(0f, 0f, 4f), new Vector3(0.42f * scale, 0.55f * scale, 0.04f * scale), new Color(0.42f, 0.46f, 0.50f));
        return root;
    }

    public static GameObject CreateWheelRutMark(Vector3 position, Quaternion rotation, float length, Transform parent)
    {
        GameObject root = CreateRoot("RoadWheelRut", position, rotation, parent);
        CreatePrimitiveProp(root.transform, "Rut", PrimitiveType.Cube, Vector3.zero, Quaternion.identity, new Vector3(0.36f, 0.025f, Mathf.Max(0.6f, length)), new Color(0.18f, 0.145f, 0.095f));
        return root;
    }

    public static GameObject CreatePathMarkerStones(Vector3 position, Quaternion rotation, float scale, Transform parent)
    {
        GameObject root = CreateRoot("RoadEdgeStones", position, rotation, parent);
        int count = Random.Range(2, 5);
        for (int i = 0; i < count; i++)
        {
            Vector3 local = new Vector3(Random.Range(-0.7f, 0.7f) * scale, 0.08f * scale, Random.Range(-0.45f, 0.45f) * scale);
            CreatePrimitiveProp(root.transform, "Stone_" + i, PrimitiveType.Sphere, local, Quaternion.identity, new Vector3(Random.Range(0.22f, 0.45f) * scale, Random.Range(0.12f, 0.22f) * scale, Random.Range(0.22f, 0.45f) * scale), Stone);
        }
        return root;
    }

    public static GameObject CreateRoadEdgeRubble(Vector3 position, Quaternion rotation, float scale, Transform parent)
    {
        GameObject root = CreateRoot("RoadRubble", position, rotation, parent);
        CreatePrimitiveProp(root.transform, "BrokenBoardA", PrimitiveType.Cube, new Vector3(-0.35f * scale, 0.14f * scale, 0f), Quaternion.Euler(0f, 20f, 10f), new Vector3(1.4f * scale, 0.12f * scale, 0.16f * scale), DarkWood);
        CreatePrimitiveProp(root.transform, "BrokenBoardB", PrimitiveType.Cube, new Vector3(0.35f * scale, 0.13f * scale, 0.22f * scale), Quaternion.Euler(0f, -35f, -8f), new Vector3(1.0f * scale, 0.1f * scale, 0.14f * scale), Wood);
        CreatePrimitiveProp(root.transform, "Rock", PrimitiveType.Sphere, new Vector3(0.1f * scale, 0.15f * scale, -0.35f * scale), Quaternion.identity, new Vector3(0.42f * scale, 0.24f * scale, 0.36f * scale), Stone);
        return root;
    }

    public static GameObject CreateHugeOrcHut(Vector3 position, Quaternion rotation, float scale, Transform parent)
    {
        GameObject root = CreateRoot("HugeOrcHut", position, rotation, parent);
        CreatePrimitiveProp(root.transform, "Base", PrimitiveType.Cube, new Vector3(0f, 1.2f * scale, 0f), Quaternion.Euler(0f, 0f, Random.Range(-3f, 3f)), new Vector3(2.8f * scale, 2.4f * scale, 3.0f * scale), new Color(0.10f, 0.075f, 0.055f));
        CreatePrimitiveProp(root.transform, "SlabRoof", PrimitiveType.Cube, new Vector3(0f, 2.75f * scale, 0f), Quaternion.Euler(Random.Range(-4f, 4f), 0f, Random.Range(-8f, 8f)), new Vector3(3.4f * scale, 0.42f * scale, 3.5f * scale), OrcCloth);
        CreatePrimitiveProp(root.transform, "Entrance", PrimitiveType.Cube, new Vector3(0f, 0.95f * scale, 1.55f * scale), Quaternion.identity, new Vector3(0.95f * scale, 1.25f * scale, 0.09f * scale), new Color(0.02f, 0.015f, 0.012f));
        CreatePrimitiveProp(root.transform, "RedCloth", PrimitiveType.Cube, new Vector3(0f, 2.05f * scale, 1.58f * scale), Quaternion.identity, new Vector3(1.2f * scale, 0.5f * scale, 0.08f * scale), OrcRed);
        CreatePrimitiveProp(root.transform, "HornL", PrimitiveType.Cylinder, new Vector3(-1.25f * scale, 3.05f * scale, 1.15f * scale), Quaternion.Euler(0f, 0f, -42f), new Vector3(0.06f * scale, 0.8f * scale, 0.06f * scale), Bone);
        CreatePrimitiveProp(root.transform, "HornR", PrimitiveType.Cylinder, new Vector3(1.25f * scale, 3.05f * scale, 1.15f * scale), Quaternion.Euler(0f, 0f, 42f), new Vector3(0.06f * scale, 0.8f * scale, 0.06f * scale), Bone);
        return root;
    }

    public static GameObject CreateOrcWarDrum(Vector3 position, Quaternion rotation, float scale, Transform parent)
    {
        GameObject root = CreateRoot("OrcWarDrum", position, rotation, parent);
        CreatePrimitiveProp(root.transform, "DrumBody", PrimitiveType.Cylinder, new Vector3(0f, 0.65f * scale, 0f), Quaternion.Euler(90f, 0f, 0f), new Vector3(1.2f * scale, 0.95f * scale, 1.2f * scale), new Color(0.20f, 0.10f, 0.055f));
        CreatePrimitiveProp(root.transform, "BoneMark", PrimitiveType.Cube, new Vector3(0f, 0.65f * scale, 0.98f * scale), Quaternion.Euler(0f, 0f, 45f), new Vector3(0.55f * scale, 0.12f * scale, 0.05f * scale), Bone);
        CreatePrimitiveProp(root.transform, "StickA", PrimitiveType.Cylinder, new Vector3(-0.75f * scale, 1.35f * scale, 0.5f * scale), Quaternion.Euler(30f, 0f, 35f), new Vector3(0.055f * scale, 0.85f * scale, 0.055f * scale), Wood);
        CreatePrimitiveProp(root.transform, "StickB", PrimitiveType.Cylinder, new Vector3(0.75f * scale, 1.35f * scale, 0.5f * scale), Quaternion.Euler(30f, 0f, -35f), new Vector3(0.055f * scale, 0.85f * scale, 0.055f * scale), Wood);
        return root;
    }

    public static GameObject CreateArenaBarrier(Vector3 position, Quaternion rotation, float scale, Transform parent)
    {
        GameObject root = CreateRoot("ArenaBarrier", position, rotation, parent);
        CreatePrimitiveProp(root.transform, "PostL", PrimitiveType.Cylinder, new Vector3(-0.9f * scale, 0.95f * scale, 0f), Quaternion.identity, new Vector3(0.08f * scale, 0.95f * scale, 0.08f * scale), DarkWood);
        CreatePrimitiveProp(root.transform, "PostR", PrimitiveType.Cylinder, new Vector3(0.9f * scale, 0.95f * scale, 0f), Quaternion.identity, new Vector3(0.08f * scale, 0.95f * scale, 0.08f * scale), DarkWood);
        CreatePrimitiveProp(root.transform, "RailA", PrimitiveType.Cube, new Vector3(0f, 0.68f * scale, 0f), Quaternion.Euler(0f, 0f, 2f), new Vector3(1.95f * scale, 0.13f * scale, 0.12f * scale), Wood);
        CreatePrimitiveProp(root.transform, "RailB", PrimitiveType.Cube, new Vector3(0f, 1.08f * scale, 0f), Quaternion.Euler(0f, 0f, -3f), new Vector3(1.85f * scale, 0.13f * scale, 0.12f * scale), DarkWood);
        CreatePrimitiveProp(root.transform, "SpikeL", PrimitiveType.Cube, new Vector3(-0.9f * scale, 1.82f * scale, 0f), Quaternion.Euler(45f, 0f, 45f), new Vector3(0.18f * scale, 0.28f * scale, 0.18f * scale), Bone);
        CreatePrimitiveProp(root.transform, "SpikeR", PrimitiveType.Cube, new Vector3(0.9f * scale, 1.82f * scale, 0f), Quaternion.Euler(45f, 0f, 45f), new Vector3(0.18f * scale, 0.28f * scale, 0.18f * scale), Bone);
        CreatePrimitiveProp(root.transform, "RedRag", PrimitiveType.Cube, new Vector3(0.3f * scale, 0.88f * scale, 0.06f * scale), Quaternion.Euler(0f, 0f, -12f), new Vector3(0.55f * scale, 0.34f * scale, 0.04f * scale), OrcRed);
        return root;
    }

    public static GameObject CreateBonePile(Vector3 position, float scale, Transform parent)
    {
        GameObject root = CreateRoot("BonePile", position, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f), parent);
        for (int i = 0; i < 5; i++)
        {
            float x = Random.Range(-0.35f, 0.35f) * scale;
            float z = Random.Range(-0.35f, 0.35f) * scale;
            float len = Random.Range(0.35f, 0.7f) * scale;
            CreatePrimitiveProp(root.transform, "Bone_" + i, PrimitiveType.Cylinder, new Vector3(x, 0.08f * scale + i * 0.02f * scale, z), Quaternion.Euler(Random.Range(0f, 180f), Random.Range(0f, 180f), Random.Range(0f, 180f)), new Vector3(0.045f * scale, len, 0.045f * scale), Bone);
        }
        CreatePrimitiveProp(root.transform, "Skull", PrimitiveType.Sphere, new Vector3(0.1f * scale, 0.18f * scale, -0.08f * scale), Quaternion.identity, new Vector3(0.22f * scale, 0.16f * scale, 0.22f * scale), Bone);
        return root;
    }

    public static GameObject CreateTorchPost(Vector3 position, Quaternion rotation, float scale, Transform parent)
    {
        GameObject root = CreateRoot("TorchPost", position, rotation, parent);
        CreatePrimitiveProp(root.transform, "Post", PrimitiveType.Cylinder, new Vector3(0f, 1.1f * scale, 0f), Quaternion.identity, new Vector3(0.07f * scale, 1.1f * scale, 0.07f * scale), DarkWood);
        CreatePrimitiveProp(root.transform, "Cross", PrimitiveType.Cube, new Vector3(0.18f * scale, 2.0f * scale, 0f), Quaternion.identity, new Vector3(0.55f * scale, 0.08f * scale, 0.08f * scale), Wood);
        CreatePrimitiveProp(root.transform, "Bowl", PrimitiveType.Cylinder, new Vector3(0.35f * scale, 1.92f * scale, 0f), Quaternion.Euler(90f, 0f, 0f), new Vector3(0.18f * scale, 0.14f * scale, 0.18f * scale), new Color(0.18f, 0.16f, 0.15f));
        CreatePrimitiveProp(root.transform, "FlameCore", PrimitiveType.Sphere, new Vector3(0.35f * scale, 2.12f * scale, 0f), Quaternion.identity, new Vector3(0.26f * scale, 0.42f * scale, 0.26f * scale), Fire, false, true);
        CreatePrimitiveProp(root.transform, "FlameTip", PrimitiveType.Sphere, new Vector3(0.38f * scale, 2.44f * scale, 0f), Quaternion.identity, new Vector3(0.14f * scale, 0.28f * scale, 0.14f * scale), Color.yellow, false, true);
        return root;
    }

    // ---------- Step 4B: Village life props ----------

    public static GameObject CreateBarrel(Vector3 position, Quaternion rotation, Transform parent)
    {
        GameObject root = CreateRoot("Barrel", position, rotation, parent);
        CreatePrimitiveProp(root.transform, "Body", PrimitiveType.Cylinder, new Vector3(0f, 0.45f, 0f), Quaternion.identity, new Vector3(0.55f, 0.45f, 0.55f), Wood);
        CreatePrimitiveProp(root.transform, "BandTop", PrimitiveType.Cylinder, new Vector3(0f, 0.72f, 0f), Quaternion.identity, new Vector3(0.58f, 0.03f, 0.58f), new Color(0.22f, 0.20f, 0.18f));
        CreatePrimitiveProp(root.transform, "BandBot", PrimitiveType.Cylinder, new Vector3(0f, 0.18f, 0f), Quaternion.identity, new Vector3(0.58f, 0.03f, 0.58f), new Color(0.22f, 0.20f, 0.18f));
        CreatePrimitiveProp(root.transform, "Lid", PrimitiveType.Cylinder, new Vector3(0f, 0.91f, 0f), Quaternion.identity, new Vector3(0.52f, 0.02f, 0.52f), DarkWood);
        return root;
    }

    public static GameObject CreateWoodPile(Vector3 position, Quaternion rotation, float scale, Transform parent)
    {
        GameObject root = CreateRoot("WoodPile", position, rotation, parent);
        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 4 - row; col++)
            {
                float x = (col - (3 - row) * 0.5f + 0.5f) * 0.22f * scale;
                float y = 0.1f * scale + row * 0.19f * scale;
                CreatePrimitiveProp(root.transform, "Log_" + row + "_" + col, PrimitiveType.Cylinder,
                    new Vector3(x, y, 0f), Quaternion.Euler(0f, 0f, 90f),
                    new Vector3(0.09f * scale, 0.45f * scale, 0.09f * scale), Wood);
            }
        }
        return root;
    }

    public static GameObject CreateWaterTrough(Vector3 position, Quaternion rotation, float scale, Transform parent)
    {
        GameObject root = CreateRoot("WaterTrough", position, rotation, parent);
        CreatePrimitiveProp(root.transform, "Basin", PrimitiveType.Cube, new Vector3(0f, 0.3f * scale, 0f), Quaternion.identity, new Vector3(1.4f * scale, 0.35f * scale, 0.6f * scale), Wood);
        CreatePrimitiveProp(root.transform, "Water", PrimitiveType.Cube, new Vector3(0f, 0.42f * scale, 0f), Quaternion.identity, new Vector3(1.2f * scale, 0.06f * scale, 0.42f * scale), new Color(0.08f, 0.15f, 0.22f));
        CreatePrimitiveProp(root.transform, "LegL", PrimitiveType.Cube, new Vector3(-0.55f * scale, 0.13f * scale, 0f), Quaternion.identity, new Vector3(0.12f * scale, 0.26f * scale, 0.5f * scale), DarkWood);
        CreatePrimitiveProp(root.transform, "LegR", PrimitiveType.Cube, new Vector3(0.55f * scale, 0.13f * scale, 0f), Quaternion.identity, new Vector3(0.12f * scale, 0.26f * scale, 0.5f * scale), DarkWood);
        return root;
    }

    public static GameObject CreateFlowerBed(Vector3 position, Quaternion rotation, float scale, Transform parent, Color flowerColor)
    {
        GameObject root = CreateRoot("FlowerBed", position, rotation, parent);
        CreatePrimitiveProp(root.transform, "Soil", PrimitiveType.Cube, Vector3.zero, Quaternion.identity, new Vector3(1.2f * scale, 0.04f, 0.8f * scale), new Color(0.18f, 0.12f, 0.06f));
        for (int i = 0; i < 5; i++)
        {
            float fx = Random.Range(-0.45f, 0.45f) * scale;
            float fz = Random.Range(-0.28f, 0.28f) * scale;
            CreatePrimitiveProp(root.transform, "Stem_" + i, PrimitiveType.Cylinder, new Vector3(fx, 0.14f * scale, fz), Quaternion.identity, new Vector3(0.02f * scale, 0.14f * scale, 0.02f * scale), new Color(0.15f, 0.35f, 0.12f));
            CreatePrimitiveProp(root.transform, "Bloom_" + i, PrimitiveType.Sphere, new Vector3(fx, 0.3f * scale, fz), Quaternion.identity, new Vector3(0.12f * scale, 0.09f * scale, 0.12f * scale), flowerColor);
        }
        return root;
    }

    // ---------- Low-level creation ----------

    private static GameObject CreateRoot(string name, Vector3 position, Quaternion rotation, Transform parent)
    {
        GameObject root = new GameObject(name);
        root.transform.SetParent(parent, true);
        root.transform.position = position;
        root.transform.rotation = rotation;
        return root;
    }

    private static GameObject CreatePrimitiveProp(Transform parent, string name, PrimitiveType type, Vector3 localPosition, Quaternion localRotation, Vector3 localScale, Color color)
    {
        return CreatePrimitiveProp(parent, name, type, localPosition, localRotation, localScale, color, false, false, false);
    }

    private static GameObject CreatePrimitiveProp(Transform parent, string name, PrimitiveType type, Vector3 localPosition, Quaternion localRotation, Vector3 localScale, Color color, bool keepCollider, bool emission)
    {
        return CreatePrimitiveProp(parent, name, type, localPosition, localRotation, localScale, color, keepCollider, emission, false);
    }

    private static GameObject CreatePrimitiveProp(Transform parent, string name, PrimitiveType type, Vector3 localPosition, Quaternion localRotation, Vector3 localScale, Color color, bool keepCollider, bool emission, bool transparent)
    {
        GameObject obj = GameObject.CreatePrimitive(type);
        obj.name = name;
        obj.transform.SetParent(parent, false);
        obj.transform.localPosition = localPosition;
        obj.transform.localRotation = localRotation;
        obj.transform.localScale = localScale;

        if (!keepCollider)
        {
            RemoveCollider(obj);
        }

        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = CreateMaterial(color, emission, transparent);
        }

        return obj;
    }

    private static Material CreateMaterial(Color color, bool emission, bool transparent)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("URP/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        if (shader == null) shader = Shader.Find("Diffuse");
        if (shader == null) shader = Shader.Find("Sprites/Default");

        Material mat = new Material(shader);
        SetMaterialColor(mat, color);

        if (emission)
        {
            mat.EnableKeyword("_EMISSION");
            if (mat.HasProperty("_EmissionColor"))
            {
                mat.SetColor("_EmissionColor", color * 1.7f);
            }
        }

        if (transparent)
        {
            ConfigureTransparent(mat, color);
        }

        return mat;
    }

    private static void SetMaterialColor(Material mat, Color color)
    {
        if (mat.HasProperty("_BaseColor"))
        {
            mat.SetColor("_BaseColor", color);
        }
        if (mat.HasProperty("_Color"))
        {
            mat.SetColor("_Color", color);
        }
    }

    private static void ConfigureTransparent(Material mat, Color color)
    {
        color.a = Mathf.Clamp01(color.a);
        SetMaterialColor(mat, color);

        if (mat.HasProperty("_Surface")) mat.SetFloat("_Surface", 1f);
        if (mat.HasProperty("_Blend")) mat.SetFloat("_Blend", 0f);
        if (mat.HasProperty("_Mode")) mat.SetFloat("_Mode", 3f);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;
    }

    private static void RemoveCollider(GameObject obj)
    {
        Collider col = obj.GetComponent<Collider>();
        if (col != null)
        {
            if (Application.isPlaying)
            {
                UnityEngine.Object.Destroy(col);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(col);
            }
        }
    }

    private static string SafeName(string label)
    {
        if (string.IsNullOrEmpty(label)) return "Empty";
        return label.Replace(" ", "_").Replace("/", "_").Replace("\\", "_").Replace(":", "_");
    }
}

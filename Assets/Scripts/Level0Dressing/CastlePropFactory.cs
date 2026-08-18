using UnityEngine;

public static class CastlePropFactory
{
    private static Material CreateMaterial(Color color, float roughness = 1f)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        if (shader == null) shader = Shader.Find("Diffuse");

        Material mat = new Material(shader);
        if (mat.HasProperty("_BaseColor"))
            mat.SetColor("_BaseColor", color);
        if (mat.HasProperty("_Color"))
            mat.color = color;
        if (mat.HasProperty("_Glossiness"))
            mat.SetFloat("_Glossiness", 1f - roughness);
        if (mat.HasProperty("_Smoothness"))
            mat.SetFloat("_Smoothness", 1f - roughness);
        return mat;
    }

    public static GameObject CreateCampfire(Vector3 position, Transform parent)
    {
        GameObject campfire = new GameObject("Prop_Campfire");
        campfire.transform.SetParent(parent);
        campfire.transform.position = position;

        Material stoneMat = CreateMaterial(new Color(0.4f, 0.4f, 0.4f));
        for (int i = 0; i < 6; i++)
        {
            float angle = i * Mathf.PI * 2f / 6f;
            Vector3 stonePos = new Vector3(Mathf.Cos(angle) * 0.6f, 0.1f, Mathf.Sin(angle) * 0.6f);

            GameObject stone = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stone.transform.SetParent(campfire.transform);
            stone.transform.localPosition = stonePos;
            stone.transform.localScale = new Vector3(0.25f, 0.2f, 0.25f);
            stone.GetComponent<Renderer>().material = stoneMat;
            RemoveCollider(stone);
        }

        Material woodMat = CreateMaterial(new Color(0.35f, 0.2f, 0.08f));
        for (int i = 0; i < 3; i++)
        {
            GameObject log = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            log.transform.SetParent(campfire.transform);
            log.transform.localPosition = new Vector3(0, 0.15f, 0);
            log.transform.localRotation = Quaternion.Euler(15, i * 60, 90);
            log.transform.localScale = new Vector3(0.12f, 0.5f, 0.12f);
            log.GetComponent<Renderer>().material = woodMat;
            RemoveCollider(log);
        }

        GameObject flame = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        flame.name = "Campfire_Flame";
        flame.transform.SetParent(campfire.transform);
        flame.transform.localPosition = new Vector3(0f, 0.42f, 0f);
        flame.transform.localScale = new Vector3(0.42f, 0.55f, 0.42f);
        flame.GetComponent<Renderer>().material = CreateMaterial(new Color(1f, 0.35f, 0.05f));
        RemoveCollider(flame);

        GameObject fireLight = new GameObject("Campfire_Light");
        fireLight.transform.SetParent(campfire.transform);
        fireLight.transform.localPosition = new Vector3(0, 0.4f, 0);

        Light lt = fireLight.AddComponent<Light>();
        lt.type = LightType.Point;
        lt.color = new Color(1f, 0.45f, 0.1f);
        lt.range = 8f;
        lt.intensity = 1.5f;

        return campfire;
    }

    public static GameObject CreateWeaponRack(Vector3 position, Transform parent)
    {
        GameObject rack = new GameObject("Prop_WeaponRack");
        rack.transform.SetParent(parent);
        rack.transform.position = position;

        Material woodMat = CreateMaterial(new Color(0.3f, 0.18f, 0.08f));
        Material ironMat = CreateMaterial(new Color(0.6f, 0.6f, 0.6f));
        Material bowMat = CreateMaterial(new Color(0.45f, 0.25f, 0.08f));

        GameObject leftLeg = CreateCube("LeftLeg", rack.transform, new Vector3(-0.8f, 0.75f, 0), new Vector3(0.15f, 1.5f, 0.3f), woodMat);
        GameObject rightLeg = CreateCube("RightLeg", rack.transform, new Vector3(0.8f, 0.75f, 0), new Vector3(0.15f, 1.5f, 0.3f), woodMat);
        GameObject bar = CreateCube("TopBar", rack.transform, new Vector3(0, 1.2f, 0), new Vector3(1.8f, 0.15f, 0.15f), woodMat);
        GameObject bottom = CreateCube("BottomBar", rack.transform, new Vector3(0, 0.35f, 0), new Vector3(1.8f, 0.12f, 0.12f), woodMat);

        GameObject sword = CreateCube("Decor_Sword", rack.transform, new Vector3(-0.45f, 0.75f, -0.12f), new Vector3(0.08f, 1.1f, 0.03f), ironMat);
        sword.transform.localRotation = Quaternion.Euler(0, 0, -8);

        GameObject spear = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        spear.name = "Decor_Spear";
        spear.transform.SetParent(rack.transform);
        spear.transform.localPosition = new Vector3(0.15f, 0.85f, -0.08f);
        spear.transform.localRotation = Quaternion.Euler(0, 0, -12);
        spear.transform.localScale = new Vector3(0.04f, 1.4f, 0.04f);
        spear.GetComponent<Renderer>().material = woodMat;
        RemoveCollider(spear);

        GameObject spearTip = CreateCube("Decor_SpearTip", rack.transform, new Vector3(0.38f, 1.55f, -0.08f), new Vector3(0.12f, 0.22f, 0.05f), ironMat);
        spearTip.transform.localRotation = Quaternion.Euler(0, 0, -12);

        GameObject bow = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        bow.name = "Decor_Bow";
        bow.transform.SetParent(rack.transform);
        bow.transform.localPosition = new Vector3(0.58f, 0.78f, -0.1f);
        bow.transform.localRotation = Quaternion.Euler(0, 0, 18);
        bow.transform.localScale = new Vector3(0.035f, 0.85f, 0.035f);
        bow.GetComponent<Renderer>().material = bowMat;
        RemoveCollider(bow);

        return rack;
    }

    public static GameObject CreateBlacksmithAnvil(Vector3 position, Transform parent)
    {
        GameObject anvilGroup = new GameObject("Prop_AnvilArea");
        anvilGroup.transform.SetParent(parent);
        anvilGroup.transform.position = position;

        Material stumpMat = CreateMaterial(new Color(0.45f, 0.3f, 0.15f));
        Material metalMat = CreateMaterial(new Color(0.2f, 0.2f, 0.23f));

        GameObject stump = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        stump.transform.SetParent(anvilGroup.transform);
        stump.transform.localPosition = new Vector3(0, 0.3f, 0);
        stump.transform.localScale = new Vector3(0.6f, 0.3f, 0.6f);
        stump.GetComponent<Renderer>().material = stumpMat;
        RemoveCollider(stump);

        CreateCube("AnvilBase", anvilGroup.transform, new Vector3(0, 0.7f, 0), new Vector3(0.35f, 0.25f, 0.7f), metalMat);
        CreateCube("AnvilTop", anvilGroup.transform, new Vector3(0, 0.88f, 0), new Vector3(0.85f, 0.16f, 0.32f), metalMat);

        GameObject hammer = CreateCube("Decor_Hammer", anvilGroup.transform, new Vector3(0.6f, 0.92f, 0f), new Vector3(0.5f, 0.08f, 0.12f), metalMat);
        hammer.transform.localRotation = Quaternion.Euler(0, 20, 0);

        return anvilGroup;
    }

    public static GameObject CreateKingNoteTable(Vector3 position, Transform parent)
    {
        GameObject tableGroup = new GameObject("Prop_KingNoteTable");
        tableGroup.transform.SetParent(parent);
        tableGroup.transform.position = position;

        Material woodMat = CreateMaterial(new Color(0.4f, 0.25f, 0.1f));
        Material paperMat = CreateMaterial(new Color(0.95f, 0.9f, 0.75f));
        Material sealMat = CreateMaterial(new Color(0.7f, 0.04f, 0.04f));

        CreateCube("TableTop", tableGroup.transform, new Vector3(0, 0.85f, 0), new Vector3(1.4f, 0.1f, 0.8f), woodMat);

        for (int x = -1; x <= 1; x += 2)
        {
            for (int z = -1; z <= 1; z += 2)
            {
                CreateCube("TableLeg", tableGroup.transform, new Vector3(x * 0.55f, 0.4f, z * 0.28f), new Vector3(0.12f, 0.8f, 0.12f), woodMat);
            }
        }

        GameObject note = CreateCube("Decor_KingNote", tableGroup.transform, new Vector3(0, 0.92f, 0), new Vector3(0.42f, 0.02f, 0.6f), paperMat);
        note.transform.localRotation = Quaternion.Euler(0, 15f, 0);

        GameObject seal = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        seal.name = "Decor_RedSeal";
        seal.transform.SetParent(tableGroup.transform);
        seal.transform.localPosition = new Vector3(0.16f, 0.955f, 0.05f);
        seal.transform.localScale = new Vector3(0.12f, 0.03f, 0.12f);
        seal.GetComponent<Renderer>().material = sealMat;
        RemoveCollider(seal);

        CreateFloatingLabel("Указ Короля", tableGroup.transform, new Vector3(0, 1.5f, 0), Color.yellow);

        return tableGroup;
    }

    public static GameObject CreateQuestArrow(Vector3 position, Vector3 direction, string label, Transform parent)
    {
        GameObject arrowObj = new GameObject("Prop_QuestArrow_" + label);
        arrowObj.transform.SetParent(parent);
        arrowObj.transform.position = position;

        if (direction != Vector3.zero)
            arrowObj.transform.rotation = Quaternion.LookRotation(direction);

        Material signMat = CreateMaterial(new Color(0.5f, 0.35f, 0.2f));

        GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        post.transform.SetParent(arrowObj.transform);
        post.transform.localPosition = new Vector3(0, 0.6f, 0);
        post.transform.localScale = new Vector3(0.1f, 0.6f, 0.1f);
        post.GetComponent<Renderer>().material = signMat;
        RemoveCollider(post);

        CreateCube("ArrowBoard", arrowObj.transform, new Vector3(0, 1.2f, 0.2f), new Vector3(0.15f, 0.3f, 0.7f), signMat);
        CreateFloatingLabel(label, arrowObj.transform, new Vector3(0, 1.5f, 0), Color.white);

        return arrowObj;
    }

    public static GameObject CreateTorch(Vector3 position, Transform parent)
    {
        GameObject torch = new GameObject("Prop_Torch");
        torch.transform.SetParent(parent);
        torch.transform.position = position;

        Material stickMat = CreateMaterial(new Color(0.25f, 0.15f, 0.05f));
        Material blackMat = CreateMaterial(Color.black);
        Material flameMat = CreateMaterial(new Color(1f, 0.35f, 0.05f));

        GameObject stick = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        stick.transform.SetParent(torch.transform);
        stick.transform.localPosition = new Vector3(0, 0.5f, 0);
        stick.transform.localScale = new Vector3(0.06f, 0.5f, 0.06f);
        stick.GetComponent<Renderer>().material = stickMat;
        RemoveCollider(stick);

        CreateCube("TorchTip", torch.transform, new Vector3(0, 1.05f, 0), new Vector3(0.12f, 0.15f, 0.12f), blackMat);

        GameObject flame = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        flame.name = "Torch_Flame";
        flame.transform.SetParent(torch.transform);
        flame.transform.localPosition = new Vector3(0, 1.25f, 0);
        flame.transform.localScale = new Vector3(0.18f, 0.22f, 0.18f);
        flame.GetComponent<Renderer>().material = flameMat;
        RemoveCollider(flame);

        GameObject lightObj = new GameObject("Torch_Light");
        lightObj.transform.SetParent(torch.transform);
        lightObj.transform.localPosition = new Vector3(0, 1.2f, 0);

        Light lt = lightObj.AddComponent<Light>();
        lt.type = LightType.Point;
        lt.color = new Color(1f, 0.6f, 0.2f);
        lt.range = 5f;
        lt.intensity = 1f;

        return torch;
    }

    public static GameObject CreateCratesAndBags(Vector3 position, Transform parent)
    {
        GameObject group = new GameObject("Prop_CratesAndBags");
        group.transform.SetParent(parent);
        group.transform.position = position;

        Material crateMat = CreateMaterial(new Color(0.55f, 0.4f, 0.25f));
        Material sackMat = CreateMaterial(new Color(0.65f, 0.55f, 0.45f));

        CreateCube("Crate_Big", group.transform, new Vector3(-0.3f, 0.35f, 0), new Vector3(0.7f, 0.7f, 0.7f), crateMat);

        GameObject box2 = CreateCube("Crate_Small", group.transform, new Vector3(-0.25f, 0.95f, 0.05f), new Vector3(0.5f, 0.5f, 0.5f), crateMat);
        box2.transform.localRotation = Quaternion.Euler(0, 20f, 0);

        GameObject sack = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sack.name = "Sack";
        sack.transform.SetParent(group.transform);
        sack.transform.localPosition = new Vector3(0.4f, 0.3f, -0.1f);
        sack.transform.localScale = new Vector3(0.5f, 0.6f, 0.5f);
        sack.GetComponent<Renderer>().material = sackMat;
        RemoveCollider(sack);

        return group;
    }

    private static GameObject CreateCube(string name, Transform parent, Vector3 localPosition, Vector3 localScale, Material mat)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.name = name;
        obj.transform.SetParent(parent);
        obj.transform.localPosition = localPosition;
        obj.transform.localScale = localScale;
        obj.GetComponent<Renderer>().material = mat;
        RemoveCollider(obj);
        return obj;
    }

    private static void CreateFloatingLabel(string text, Transform parent, Vector3 localPosition, Color color)
    {
        GameObject labelObj = new GameObject("Label_" + text);
        labelObj.transform.SetParent(parent);
        labelObj.transform.localPosition = localPosition;
        labelObj.AddComponent<SimpleBillboardLabel>();

        TextMesh tm = labelObj.AddComponent<TextMesh>();
        tm.text = text;
        tm.fontSize = 24;
        tm.characterSize = 0.08f;
        tm.alignment = TextAlignment.Center;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.color = color;
    }

    // --- Новые пропсы (шаг 3: стартовая зона) ---

    public static GameObject CreateGateTorch(Vector3 position, Transform parent, float height = 2.2f)
    {
        GameObject torch = new GameObject("Prop_GateTorch");
        torch.transform.SetParent(parent);
        torch.transform.position = position;

        Material stoneMat = CreateMaterial(new Color(0.35f, 0.35f, 0.38f), 0.9f);
        Material ironMat = CreateMaterial(new Color(0.15f, 0.15f, 0.18f), 0.8f);
        Material flameMat = CreateMaterial(new Color(1f, 0.4f, 0.05f));

        GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pillar.name = "TorchPillar";
        pillar.transform.SetParent(torch.transform);
        pillar.transform.localPosition = new Vector3(0, height * 0.5f, 0);
        pillar.transform.localScale = new Vector3(0.12f, height * 0.5f, 0.12f);
        pillar.GetComponent<Renderer>().material = stoneMat;
        RemoveCollider(pillar);

        CreateCube("TorchBracket", torch.transform,
            new Vector3(0, height - 0.15f, 0), new Vector3(0.22f, 0.06f, 0.22f), ironMat);

        CreateCube("TorchCup", torch.transform,
            new Vector3(0, height, 0), new Vector3(0.18f, 0.12f, 0.18f), ironMat);

        GameObject flame = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        flame.name = "GateTorch_Flame";
        flame.transform.SetParent(torch.transform);
        flame.transform.localPosition = new Vector3(0, height + 0.15f, 0);
        flame.transform.localScale = new Vector3(0.24f, 0.32f, 0.24f);
        flame.GetComponent<Renderer>().material = flameMat;
        RemoveCollider(flame);

        GameObject lightObj = new GameObject("GateTorch_Light");
        lightObj.transform.SetParent(torch.transform);
        lightObj.transform.localPosition = new Vector3(0, height + 0.1f, 0);
        Light lt = lightObj.AddComponent<Light>();
        lt.type = LightType.Point;
        lt.color = new Color(1f, 0.55f, 0.15f);
        lt.range = 10f;
        lt.intensity = 1.8f;

        return torch;
    }

    public static GameObject CreatePathStone(Vector3 position, Transform parent, float sizeVariation = 1f)
    {
        GameObject stone = GameObject.CreatePrimitive(PrimitiveType.Cube);
        stone.name = "Prop_PathStone";
        stone.transform.SetParent(parent);
        stone.transform.position = position;
        float w = 0.8f * sizeVariation;
        float d = 0.6f * sizeVariation;
        stone.transform.localScale = new Vector3(w, 0.06f, d);
        stone.transform.localRotation = Quaternion.Euler(0, Random.Range(-15f, 15f), 0);

        Material mat = CreateMaterial(new Color(0.45f, 0.42f, 0.38f), 0.85f);
        stone.GetComponent<Renderer>().material = mat;
        RemoveCollider(stone);
        return stone;
    }

    public static GameObject CreateDirectionSign(Vector3 position, Vector3 direction, string label, Transform parent)
    {
        GameObject sign = new GameObject("Prop_DirectionSign");
        sign.transform.SetParent(parent);
        sign.transform.position = position;
        if (direction != Vector3.zero)
            sign.transform.rotation = Quaternion.LookRotation(direction);

        Material woodMat = CreateMaterial(new Color(0.4f, 0.28f, 0.12f));
        Material boardMat = CreateMaterial(new Color(0.5f, 0.38f, 0.2f));

        GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        post.name = "SignPost";
        post.transform.SetParent(sign.transform);
        post.transform.localPosition = new Vector3(0, 0.8f, 0);
        post.transform.localScale = new Vector3(0.1f, 0.8f, 0.1f);
        post.GetComponent<Renderer>().material = woodMat;
        RemoveCollider(post);

        CreateCube("SignBoard", sign.transform,
            new Vector3(0, 1.55f, 0.25f), new Vector3(0.12f, 0.35f, 0.9f), boardMat);

        CreateCube("SignArrowTip", sign.transform,
            new Vector3(0, 1.55f, 0.78f), new Vector3(0.12f, 0.2f, 0.2f), boardMat);

        CreateFloatingLabel(label, sign.transform, new Vector3(0, 1.9f, 0.25f), new Color(1f, 0.9f, 0.6f));

        return sign;
    }

    public static GameObject CreateBench(Vector3 position, float yRotation, Transform parent)
    {
        GameObject bench = new GameObject("Prop_Bench");
        bench.transform.SetParent(parent);
        bench.transform.position = position;
        bench.transform.rotation = Quaternion.Euler(0, yRotation, 0);

        Material woodMat = CreateMaterial(new Color(0.38f, 0.24f, 0.1f));

        CreateCube("BenchSeat", bench.transform,
            new Vector3(0, 0.4f, 0), new Vector3(1.2f, 0.08f, 0.4f), woodMat);

        CreateCube("BenchLegL", bench.transform,
            new Vector3(-0.45f, 0.2f, 0), new Vector3(0.08f, 0.4f, 0.35f), woodMat);
        CreateCube("BenchLegR", bench.transform,
            new Vector3(0.45f, 0.2f, 0), new Vector3(0.08f, 0.4f, 0.35f), woodMat);

        return bench;
    }

    public static GameObject CreateBarrel(Vector3 position, Transform parent)
    {
        GameObject barrel = new GameObject("Prop_Barrel");
        barrel.transform.SetParent(parent);
        barrel.transform.position = position;

        Material woodMat = CreateMaterial(new Color(0.42f, 0.28f, 0.12f));
        Material bandMat = CreateMaterial(new Color(0.3f, 0.3f, 0.3f));

        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        body.name = "BarrelBody";
        body.transform.SetParent(barrel.transform);
        body.transform.localPosition = new Vector3(0, 0.4f, 0);
        body.transform.localScale = new Vector3(0.5f, 0.4f, 0.5f);
        body.GetComponent<Renderer>().material = woodMat;
        RemoveCollider(body);

        CreateCube("BarrelBandTop", barrel.transform,
            new Vector3(0, 0.7f, 0), new Vector3(0.52f, 0.04f, 0.52f), bandMat);
        CreateCube("BarrelBandBot", barrel.transform,
            new Vector3(0, 0.15f, 0), new Vector3(0.52f, 0.04f, 0.52f), bandMat);

        return barrel;
    }

    public static GameObject CreateWarmGateLight(Vector3 position, Transform parent)
    {
        GameObject lightObj = new GameObject("CastleGate_WarmLight");
        lightObj.transform.SetParent(parent);
        lightObj.transform.position = position;

        Light lt = lightObj.AddComponent<Light>();
        lt.type = LightType.Point;
        lt.color = new Color(1f, 0.75f, 0.4f);
        lt.range = 18f;
        lt.intensity = 2.2f;

        return lightObj;
    }

    public static GameObject CreateFlowerCluster(Vector3 position, Transform parent, Color color)
    {
        GameObject cluster = new GameObject("Prop_FlowerCluster");
        cluster.transform.SetParent(parent);
        cluster.transform.position = position;

        Material stemMat = CreateMaterial(new Color(0.2f, 0.45f, 0.15f));
        Material petalMat = CreateMaterial(color);

        for (int i = 0; i < 4; i++)
        {
            float offX = Random.Range(-0.3f, 0.3f);
            float offZ = Random.Range(-0.3f, 0.3f);
            float h = Random.Range(0.3f, 0.5f);

            GameObject stem = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            stem.name = "Stem";
            stem.transform.SetParent(cluster.transform);
            stem.transform.localPosition = new Vector3(offX, h * 0.5f, offZ);
            stem.transform.localScale = new Vector3(0.03f, h * 0.5f, 0.03f);
            stem.GetComponent<Renderer>().material = stemMat;
            RemoveCollider(stem);

            GameObject petal = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            petal.name = "Flower";
            petal.transform.SetParent(cluster.transform);
            petal.transform.localPosition = new Vector3(offX, h + 0.05f, offZ);
            petal.transform.localScale = new Vector3(0.12f, 0.08f, 0.12f);
            petal.GetComponent<Renderer>().material = petalMat;
            RemoveCollider(petal);
        }

        return cluster;
    }

    public static GameObject CreateGrassClump(Vector3 position, Transform parent)
    {
        GameObject clump = new GameObject("Prop_GrassClump");
        clump.transform.SetParent(parent);
        clump.transform.position = position;

        Material grassMat = CreateMaterial(new Color(0.25f, 0.5f, 0.18f));

        for (int i = 0; i < 6; i++)
        {
            float offX = Random.Range(-0.25f, 0.25f);
            float offZ = Random.Range(-0.25f, 0.25f);
            float h = Random.Range(0.15f, 0.35f);

            GameObject blade = GameObject.CreatePrimitive(PrimitiveType.Cube);
            blade.name = "Blade";
            blade.transform.SetParent(clump.transform);
            blade.transform.localPosition = new Vector3(offX, h * 0.5f, offZ);
            blade.transform.localScale = new Vector3(0.03f, h, 0.01f);
            blade.transform.localRotation = Quaternion.Euler(Random.Range(-10f, 10f), Random.Range(0f, 360f), 0);
            blade.GetComponent<Renderer>().material = grassMat;
            RemoveCollider(blade);
        }

        return clump;
    }

    private static void RemoveCollider(GameObject obj)
    {
        Collider col = obj.GetComponent<Collider>();

        if (col != null)
        {
            if (Application.isPlaying)
                Object.Destroy(col);
            else
                Object.DestroyImmediate(col);
        }
    }
}

using UnityEngine;

public enum CastleCharacterType
{
    Elder,
    Blacksmith,
    InjuredGuard,
    Refugee,
    Villager
}

public static class CastleCharacterVisualFactory
{
    private static Material GetMaterial(Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        if (shader == null) shader = Shader.Find("Diffuse");

        Material mat = new Material(shader);
        if (mat.HasProperty("_BaseColor"))
            mat.SetColor("_BaseColor", color);
        if (mat.HasProperty("_Color"))
            mat.color = color;
        return mat;
    }

    public static GameObject CreateCharacter(CastleCharacterType type, string displayName, Vector3 position, Quaternion rotation, Transform parent)
    {
        GameObject charRoot = new GameObject("VisualNPC_" + type + "_" + displayName);
        charRoot.transform.SetParent(parent);
        charRoot.transform.position = position;
        charRoot.transform.rotation = rotation;

        Color torsoColor = Color.grey;
        Color legsColor = Color.black;
        Color detailColor = Color.white;

        switch (type)
        {
            case CastleCharacterType.Elder:
                torsoColor = new Color(0.15f, 0.15f, 0.4f);
                legsColor = new Color(0.1f, 0.1f, 0.2f);
                detailColor = new Color(0.9f, 0.9f, 0.9f);
                break;

            case CastleCharacterType.Blacksmith:
                torsoColor = new Color(0.4f, 0.2f, 0.1f);
                legsColor = new Color(0.2f, 0.15f, 0.1f);
                detailColor = new Color(0.15f, 0.15f, 0.15f);
                break;

            case CastleCharacterType.InjuredGuard:
                torsoColor = new Color(0.6f, 0.1f, 0.1f);
                legsColor = new Color(0.3f, 0.3f, 0.3f);
                detailColor = Color.red;
                break;

            case CastleCharacterType.Refugee:
                torsoColor = new Color(0.5f, 0.45f, 0.35f);
                legsColor = new Color(0.3f, 0.25f, 0.2f);
                detailColor = new Color(0.4f, 0.35f, 0.3f);
                break;

            case CastleCharacterType.Villager:
                torsoColor = new Color(0.35f, 0.45f, 0.35f);
                legsColor = new Color(0.22f, 0.22f, 0.18f);
                detailColor = new Color(0.7f, 0.7f, 0.6f);
                break;
        }

        Material skinMat = GetMaterial(new Color(0.86f, 0.68f, 0.52f));
        Material torsoMat = GetMaterial(torsoColor);
        Material legsMat = GetMaterial(legsColor);
        Material detailMat = GetMaterial(detailColor);
        Material blackMat = GetMaterial(Color.black);
        Material bootsMat = GetMaterial(new Color(0.22f, 0.14f, 0.06f));
        Material beltMat = GetMaterial(new Color(0.18f, 0.12f, 0.05f));

        Color dark = torsoColor * 0.65f; dark.a = 1f;
        Material darkMat = GetMaterial(dark);

        Transform t = charRoot.transform;

        // --- Торс ---
        GameObject torso = CreatePrimitive(t, "Torso", PrimitiveType.Capsule, new Vector3(0, 0.98f, 0), new Vector3(0.48f, 0.58f, 0.40f), torsoMat);
        CreatePrimitive(t, "Chest", PrimitiveType.Cube, new Vector3(0, 1.08f, 0.04f), new Vector3(0.42f, 0.36f, 0.10f), darkMat);
        CreatePrimitive(t, "Shoulder_L", PrimitiveType.Sphere, new Vector3(-0.30f, 1.32f, 0), new Vector3(0.16f, 0.14f, 0.14f), torsoMat);
        CreatePrimitive(t, "Shoulder_R", PrimitiveType.Sphere, new Vector3(0.30f, 1.32f, 0), new Vector3(0.16f, 0.14f, 0.14f), torsoMat);

        // --- Голова ---
        CreatePrimitive(t, "Neck", PrimitiveType.Cylinder, new Vector3(0, 1.45f, 0), new Vector3(0.10f, 0.08f, 0.10f), skinMat);
        GameObject head = CreatePrimitive(t, "Head", PrimitiveType.Sphere, new Vector3(0, 1.62f, 0), new Vector3(0.32f, 0.34f, 0.30f), skinMat);
        CreatePrimitive(t, "Eye_L", PrimitiveType.Sphere, new Vector3(-0.07f, 1.65f, 0.12f), new Vector3(0.05f, 0.04f, 0.03f), blackMat);
        CreatePrimitive(t, "Eye_R", PrimitiveType.Sphere, new Vector3(0.07f, 1.65f, 0.12f), new Vector3(0.05f, 0.04f, 0.03f), blackMat);

        // --- Руки ---
        CreatePrimitive(t, "UpperArm_L", PrimitiveType.Cylinder, new Vector3(-0.34f, 1.16f, 0), new Vector3(0.10f, 0.22f, 0.10f), torsoMat);
        CreatePrimitive(t, "ForeArm_L", PrimitiveType.Cylinder, new Vector3(-0.38f, 0.78f, 0.04f), new Vector3(0.08f, 0.22f, 0.08f), skinMat);
        CreatePrimitive(t, "Hand_L", PrimitiveType.Sphere, new Vector3(-0.40f, 0.56f, 0.06f), new Vector3(0.08f, 0.06f, 0.06f), skinMat);
        CreatePrimitive(t, "UpperArm_R", PrimitiveType.Cylinder, new Vector3(0.34f, 1.16f, 0), new Vector3(0.10f, 0.22f, 0.10f), torsoMat);
        CreatePrimitive(t, "ForeArm_R", PrimitiveType.Cylinder, new Vector3(0.38f, 0.78f, 0.04f), new Vector3(0.08f, 0.22f, 0.08f), skinMat);
        CreatePrimitive(t, "Hand_R", PrimitiveType.Sphere, new Vector3(0.40f, 0.56f, 0.06f), new Vector3(0.08f, 0.06f, 0.06f), skinMat);

        // --- Пояс ---
        CreatePrimitive(t, "Belt", PrimitiveType.Cube, new Vector3(0, 0.72f, 0), new Vector3(0.46f, 0.08f, 0.38f), beltMat);
        CreatePrimitive(t, "BeltBuckle", PrimitiveType.Cube, new Vector3(0, 0.72f, 0.18f), new Vector3(0.06f, 0.06f, 0.03f), GetMaterial(new Color(0.65f, 0.55f, 0.20f)));

        // --- Ноги + сапоги ---
        CreatePrimitive(t, "Thigh_L", PrimitiveType.Cylinder, new Vector3(-0.12f, 0.52f, 0), new Vector3(0.12f, 0.22f, 0.12f), legsMat);
        CreatePrimitive(t, "Shin_L", PrimitiveType.Cylinder, new Vector3(-0.13f, 0.22f, 0), new Vector3(0.10f, 0.18f, 0.10f), legsMat);
        CreatePrimitive(t, "Thigh_R", PrimitiveType.Cylinder, new Vector3(0.12f, 0.52f, 0), new Vector3(0.12f, 0.22f, 0.12f), legsMat);
        CreatePrimitive(t, "Shin_R", PrimitiveType.Cylinder, new Vector3(0.13f, 0.22f, 0), new Vector3(0.10f, 0.18f, 0.10f), legsMat);
        CreatePrimitive(t, "Boot_L", PrimitiveType.Cube, new Vector3(-0.13f, 0.06f, 0.03f), new Vector3(0.12f, 0.12f, 0.18f), bootsMat);
        CreatePrimitive(t, "Boot_R", PrimitiveType.Cube, new Vector3(0.13f, 0.06f, 0.03f), new Vector3(0.12f, 0.12f, 0.18f), bootsMat);

        // --- Роль ---
        if (type == CastleCharacterType.Elder)
        {
            CreatePrimitive(t, "Beard", PrimitiveType.Cube, new Vector3(0, 1.48f, 0.10f), new Vector3(0.16f, 0.18f, 0.08f), detailMat);
            CreatePrimitive(t, "BeardTip", PrimitiveType.Cube, new Vector3(0, 1.36f, 0.08f), new Vector3(0.10f, 0.12f, 0.06f), detailMat);
            CreatePrimitive(t, "HairBack", PrimitiveType.Cube, new Vector3(0, 1.72f, -0.08f), new Vector3(0.28f, 0.14f, 0.14f), detailMat);
            CreatePrimitive(t, "Staff", PrimitiveType.Cylinder, new Vector3(0.48f, 0.85f, 0.2f), new Vector3(0.04f, 0.80f, 0.04f), GetMaterial(new Color(0.35f, 0.20f, 0.08f)));
            CreatePrimitive(t, "StaffGem", PrimitiveType.Sphere, new Vector3(0.48f, 1.66f, 0.2f), new Vector3(0.08f, 0.08f, 0.08f), GetMaterial(new Color(0.25f, 0.55f, 0.95f)));
            CreatePrimitive(t, "Collar", PrimitiveType.Cube, new Vector3(0, 1.38f, 0), new Vector3(0.44f, 0.06f, 0.36f), darkMat);
        }
        else if (type == CastleCharacterType.Blacksmith)
        {
            CreatePrimitive(t, "Apron", PrimitiveType.Cube, new Vector3(0, 0.88f, 0.20f), new Vector3(0.36f, 0.50f, 0.04f), GetMaterial(new Color(0.16f, 0.10f, 0.05f)));
            CreatePrimitive(t, "ApronStrap_L", PrimitiveType.Cube, new Vector3(-0.14f, 1.22f, 0.12f), new Vector3(0.04f, 0.16f, 0.03f), GetMaterial(new Color(0.16f, 0.10f, 0.05f)));
            CreatePrimitive(t, "ApronStrap_R", PrimitiveType.Cube, new Vector3(0.14f, 1.22f, 0.12f), new Vector3(0.04f, 0.16f, 0.03f), GetMaterial(new Color(0.16f, 0.10f, 0.05f)));
            CreatePrimitive(t, "Headband", PrimitiveType.Cube, new Vector3(0, 1.72f, 0), new Vector3(0.34f, 0.06f, 0.32f), GetMaterial(new Color(0.35f, 0.22f, 0.10f)));
            t.Find("Shoulder_L").localScale = new Vector3(0.20f, 0.16f, 0.16f);
            t.Find("Shoulder_R").localScale = new Vector3(0.20f, 0.16f, 0.16f);
        }
        else if (type == CastleCharacterType.InjuredGuard)
        {
            CreatePrimitive(t, "Bandage", PrimitiveType.Cube, new Vector3(0, 1.66f, 0.02f), new Vector3(0.34f, 0.06f, 0.32f), detailMat);
            CreatePrimitive(t, "BloodStain", PrimitiveType.Sphere, new Vector3(-0.06f, 1.67f, 0.14f), new Vector3(0.06f, 0.04f, 0.02f), GetMaterial(new Color(0.55f, 0.05f, 0.03f)));
            CreatePrimitive(t, "ChestPlate", PrimitiveType.Cube, new Vector3(0, 1.12f, 0.15f), new Vector3(0.36f, 0.30f, 0.06f), GetMaterial(new Color(0.28f, 0.28f, 0.30f)));
            CreatePrimitive(t, "Shield", PrimitiveType.Cube, new Vector3(-0.42f, 0.50f, -0.12f), new Vector3(0.05f, 0.50f, 0.34f), GetMaterial(new Color(0.22f, 0.22f, 0.25f)));
            torso.transform.localPosition = new Vector3(0, 0.88f, 0.06f);
            torso.transform.localRotation = Quaternion.Euler(12, 0, 0);
            head.transform.localPosition = new Vector3(0, 1.52f, 0.12f);
        }
        else if (type == CastleCharacterType.Refugee || type == CastleCharacterType.Villager)
        {
            CreatePrimitive(t, "Headscarf", PrimitiveType.Cube, new Vector3(0, 1.74f, -0.02f), new Vector3(0.36f, 0.10f, 0.34f), GetMaterial(detailColor * 0.85f));
            CreatePrimitive(t, "Skirt", PrimitiveType.Cube, new Vector3(0, 0.50f, 0), new Vector3(0.42f, 0.36f, 0.36f), darkMat);
        }

        GameObject textObj = new GameObject("NPC_Label");
        textObj.transform.SetParent(charRoot.transform);
        textObj.transform.localPosition = new Vector3(0, 2.2f, 0);
        textObj.AddComponent<SimpleBillboardLabel>();

        TextMesh tm = textObj.AddComponent<TextMesh>();
        tm.text = displayName;
        tm.fontSize = 24;
        tm.characterSize = 0.07f;
        tm.alignment = TextAlignment.Center;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.color = type == CastleCharacterType.InjuredGuard ? Color.red : Color.white;

        return charRoot;
    }

    private static GameObject CreatePrimitive(Transform parent, string name, PrimitiveType type, Vector3 localPosition, Vector3 localScale, Material mat)
    {
        GameObject obj = GameObject.CreatePrimitive(type);
        obj.name = name;
        obj.transform.SetParent(parent);
        obj.transform.localPosition = localPosition;
        obj.transform.localScale = localScale;
        obj.GetComponent<Renderer>().material = mat;

        Collider col = obj.GetComponent<Collider>();
        if (col != null)
        {
            if (Application.isPlaying)
                Object.Destroy(col);
            else
                Object.DestroyImmediate(col);
        }

        return obj;
    }
}

using UnityEngine;

/// <summary>
/// Фабрика визуальных пропсов для 0 уровня.
/// Все методы создают объекты из примитивов, удаляют коллайдеры.
/// </summary>
public static class WorldDressingPropFactory
{
    // ---------- Вспомогательные методы создания примитивов ----------
    private static Material CreateMaterial(Color color, Color? emissionColor = null, float emissionIntensity = 1f)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        if (shader == null) shader = Shader.Find("Diffuse");
        if (shader == null) shader = Shader.Find("Sprites/Default");

        Material mat = new Material(shader);
        if (mat.HasProperty("_BaseColor"))
            mat.SetColor("_BaseColor", color);
        if (mat.HasProperty("_Color"))
            mat.color = color;

        if (emissionColor.HasValue)
        {
            Color finalEmission = emissionColor.Value * emissionIntensity;
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", finalEmission);
        }

        return mat;
    }

    public static GameObject CreateSimpleProp(
        Transform parent,
        string name,
        PrimitiveType type,
        Vector3 localPos,
        Vector3 scale,
        Color color,
        bool hasCollider = false,
        Color? emissionColor = null,
        float emissionIntensity = 1f)
    {
        GameObject obj = GameObject.CreatePrimitive(type);
        obj.name = name;
        obj.transform.SetParent(parent, false);
        obj.transform.localPosition = localPos;
        obj.transform.localRotation = Quaternion.identity;
        obj.transform.localScale = scale;

        if (!hasCollider) RemoveCollider(obj);

        Renderer rend = obj.GetComponent<Renderer>();
        if (rend != null) rend.material = CreateMaterial(color, emissionColor, emissionIntensity);

        return obj;
    }

    public static GameObject CreateWorldProp(
        Transform parent,
        string name,
        PrimitiveType type,
        Vector3 worldPos,
        Vector3 scale,
        Color color,
        bool hasCollider = false,
        Color? emissionColor = null,
        float emissionIntensity = 1f)
    {
        GameObject obj = GameObject.CreatePrimitive(type);
        obj.name = name;
        obj.transform.SetParent(parent, true);
        obj.transform.position = worldPos;
        obj.transform.rotation = Quaternion.identity;
        obj.transform.localScale = scale;

        if (!hasCollider) RemoveCollider(obj);

        Renderer rend = obj.GetComponent<Renderer>();
        if (rend != null) rend.material = CreateMaterial(color, emissionColor, emissionIntensity);

        return obj;
    }

    private static void RemoveCollider(GameObject obj)
    {
        Collider col = obj.GetComponent<Collider>();
        if (col != null)
        {
            if (Application.isPlaying) Object.Destroy(col);
            else Object.DestroyImmediate(col);
        }
    }

    // ---------- Пропсы ----------

    public static void CreateSimpleHouse(Vector3 position, Quaternion rotation, float scale, bool broken, Transform parent)
    {
        GameObject house = new GameObject("House");
        house.transform.SetParent(parent, true);
        house.transform.position = position;
        house.transform.rotation = rotation;

        float w = 1.8f * scale;
        float d = 2.0f * scale;
        float h = 2.2f * scale;

        // Стены
        Color wallColor = broken ? new Color(0.3f, 0.25f, 0.2f) : new Color(0.5f, 0.4f, 0.3f);
        CreateSimpleProp(house.transform, "Walls", PrimitiveType.Cube, Vector3.zero, new Vector3(w, h, d), wallColor, false);

        // Крыша (двускатная)
        Color roofColor = broken ? new Color(0.2f, 0.1f, 0.05f) : new Color(0.4f, 0.2f, 0.1f);
        float rHeight = 1.0f * scale;
        GameObject roofLeft = CreateSimpleProp(house.transform, "RoofL", PrimitiveType.Cube, new Vector3(0f, h * 0.5f + rHeight * 0.3f, 0f), new Vector3(w + 0.3f, rHeight * 0.3f, d + 0.3f), roofColor, false);
        roofLeft.transform.localRotation = Quaternion.Euler(25f, 0f, 0f);
        GameObject roofRight = CreateSimpleProp(house.transform, "RoofR", PrimitiveType.Cube, new Vector3(0f, h * 0.5f + rHeight * 0.3f, 0f), new Vector3(w + 0.3f, rHeight * 0.3f, d + 0.3f), roofColor, false);
        roofRight.transform.localRotation = Quaternion.Euler(-25f, 0f, 0f);

        // Если дом повреждён – добавляем дыру (тёмный куб)
        if (broken)
        {
            CreateSimpleProp(house.transform, "Hole", PrimitiveType.Cube, new Vector3(0.2f * scale, h * 0.3f, d * 0.5f + 0.02f), new Vector3(0.5f * scale, 0.6f * scale, 0.06f), new Color(0.05f, 0.05f, 0.05f), false);
        }

        // Дверь
        CreateSimpleProp(house.transform, "Door", PrimitiveType.Cube, new Vector3(0f, -h * 0.15f, d * 0.5f + 0.02f), new Vector3(0.4f * scale, 1.0f * scale, 0.06f), new Color(0.2f, 0.12f, 0.05f), false);

        // Окна (только если дом не повреждён – или с огнём)
        if (!broken || Random.value > 0.5f)
        {
            bool glow = !broken && Random.value > 0.3f;
            Color windowColor = glow ? new Color(1f, 0.6f, 0.2f) : new Color(0.1f, 0.1f, 0.15f);
            Material windowMat = CreateMaterial(windowColor);
            windowMat.color = windowColor;
            if (glow) { windowMat.EnableKeyword("_EMISSION"); windowMat.SetColor("_EmissionColor", windowColor * 1.2f); }
            CreateSimpleProp(house.transform, "WindowL", PrimitiveType.Cube, new Vector3(-w * 0.35f, h * 0.1f, d * 0.5f + 0.02f), new Vector3(0.25f * scale, 0.35f * scale, 0.05f), windowColor, false, glow ? windowColor : (Color?)null, 1.2f);
            CreateSimpleProp(house.transform, "WindowR", PrimitiveType.Cube, new Vector3(w * 0.35f, h * 0.1f, d * 0.5f + 0.02f), new Vector3(0.25f * scale, 0.35f * scale, 0.05f), windowColor, false, glow ? windowColor : (Color?)null, 1.2f);
        }

        // Дым из трубы – если дом не повреждён
        if (!broken && Random.value > 0.5f)
        {
            // Просто маленький серый куб над трубой – имитация дыма (можно заменить на частицы)
            CreateSimpleProp(house.transform, "Smoke", PrimitiveType.Cube, new Vector3(w * 0.3f, h * 0.5f + rHeight * 0.8f, -d * 0.2f), new Vector3(0.15f * scale, 0.25f * scale, 0.15f * scale), new Color(0.5f, 0.5f, 0.5f, 0.3f), false);
        }
    }

    public static void CreateTent(Vector3 position, Quaternion rotation, float scale, Transform parent)
    {
        GameObject tent = new GameObject("Tent");
        tent.transform.SetParent(parent, true);
        tent.transform.position = position;
        tent.transform.rotation = rotation;

        float w = 1.8f * scale;
        float d = 1.8f * scale;
        float h = 1.5f * scale;

        // Основание (цилиндр)
        CreateSimpleProp(tent.transform, "Base", PrimitiveType.Cylinder, new Vector3(0f, h * 0.3f, 0f), new Vector3(w, h * 0.4f, d), new Color(0.15f, 0.1f, 0.08f), false);
        // Верх (сфера)
        CreateSimpleProp(tent.transform, "Top", PrimitiveType.Sphere, new Vector3(0f, h * 0.75f, 0f), new Vector3(w * 0.9f, h * 0.5f, d * 0.9f), new Color(0.15f, 0.1f, 0.08f), false);
        // Вход (чёрный)
        CreateSimpleProp(tent.transform, "Entrance", PrimitiveType.Cube, new Vector3(0f, h * 0.3f, d * 0.5f + 0.02f), new Vector3(0.4f * scale, h * 0.45f, 0.05f), new Color(0.05f, 0.05f, 0.05f), false);
        // Колья
        CreateSimpleProp(tent.transform, "Pole1", PrimitiveType.Cylinder, new Vector3(-w * 0.5f, 0.15f, -d * 0.5f), new Vector3(0.06f * scale, h * 0.4f, 0.06f * scale), new Color(0.25f, 0.15f, 0.08f), false);
        CreateSimpleProp(tent.transform, "Pole2", PrimitiveType.Cylinder, new Vector3(w * 0.5f, 0.15f, -d * 0.5f), new Vector3(0.06f * scale, h * 0.4f, 0.06f * scale), new Color(0.25f, 0.15f, 0.08f), false);
        CreateSimpleProp(tent.transform, "Pole3", PrimitiveType.Cylinder, new Vector3(-w * 0.5f, 0.15f, d * 0.5f), new Vector3(0.06f * scale, h * 0.4f, 0.06f * scale), new Color(0.25f, 0.15f, 0.08f), false);
        CreateSimpleProp(tent.transform, "Pole4", PrimitiveType.Cylinder, new Vector3(w * 0.5f, 0.15f, d * 0.5f), new Vector3(0.06f * scale, h * 0.4f, 0.06f * scale), new Color(0.25f, 0.15f, 0.08f), false);
    }

    public static void CreateCampfire(Vector3 position, Transform parent)
    {
        GameObject fire = new GameObject("Campfire");
        fire.transform.SetParent(parent, true);
        fire.transform.position = position;

        // Брёвна
        for (int i = 0; i < 4; i++)
        {
            float angle = i * 90f * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * 0.6f;
            GameObject log = CreateSimpleProp(fire.transform, "Log", PrimitiveType.Cube, offset + Vector3.up * 0.15f, new Vector3(1.0f, 0.15f, 0.12f), new Color(0.25f, 0.15f, 0.08f), false);
            log.transform.localRotation = Quaternion.Euler(0f, angle * Mathf.Rad2Deg, 20f);
        }
        // Огонь
        Color fireColor = new Color(1f, 0.5f, 0.1f);
        CreateSimpleProp(fire.transform, "Flame", PrimitiveType.Sphere, Vector3.up * 0.4f, new Vector3(0.6f, 0.8f, 0.6f), fireColor, false, fireColor, 1.5f);
    }

    public static void CreateOrcBanner(Vector3 position, Quaternion rotation, Transform parent)
    {
        GameObject banner = new GameObject("OrcBanner");
        banner.transform.SetParent(parent, true);
        banner.transform.position = position;
        banner.transform.rotation = rotation;

        Color bannerColor = new Color(0.6f, 0.05f, 0.05f);
        // Древко
        CreateSimpleProp(banner.transform, "Pole", PrimitiveType.Cylinder, Vector3.up * 1.2f, new Vector3(0.05f, 2.4f, 0.05f), new Color(0.3f, 0.2f, 0.1f), false);
        // Полотнище (два куба)
        GameObject cloth1 = CreateSimpleProp(banner.transform, "Cloth1", PrimitiveType.Cube, new Vector3(0.2f, 2.0f, 0f), new Vector3(0.8f, 0.6f, 0.04f), bannerColor, false);
        cloth1.transform.localRotation = Quaternion.Euler(0f, 0f, -15f);
        GameObject cloth2 = CreateSimpleProp(banner.transform, "Cloth2", PrimitiveType.Cube, new Vector3(0.6f, 1.6f, 0f), new Vector3(0.5f, 0.6f, 0.04f), bannerColor, false);
        cloth2.transform.localRotation = Quaternion.Euler(0f, 0f, -30f);
        // Символ (череп) – сфера
        CreateSimpleProp(banner.transform, "Skull", PrimitiveType.Sphere, new Vector3(0.4f, 1.9f, 0.05f), new Vector3(0.1f, 0.1f, 0.04f), new Color(0.7f, 0.6f, 0.5f), false);
    }

    public static void CreateSpearInGround(Vector3 position, Quaternion rotation, Transform parent)
    {
        GameObject spear = new GameObject("Spear");
        spear.transform.SetParent(parent, true);
        spear.transform.position = position;
        spear.transform.rotation = rotation;

        CreateSimpleProp(spear.transform, "Shaft", PrimitiveType.Cylinder, Vector3.up * 0.6f, new Vector3(0.03f, 1.2f, 0.03f), new Color(0.3f, 0.2f, 0.1f), false);
        CreateSimpleProp(spear.transform, "Head", PrimitiveType.Cube, Vector3.up * 1.2f, new Vector3(0.1f, 0.15f, 0.04f), new Color(0.6f, 0.6f, 0.6f), false);
    }

    public static void CreateBarrel(Vector3 position, Transform parent)
    {
        GameObject barrel = new GameObject("Barrel");
        barrel.transform.SetParent(parent, true);
        barrel.transform.position = position;
        barrel.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

        Color wood = new Color(0.25f, 0.15f, 0.08f);
        CreateSimpleProp(barrel.transform, "Body", PrimitiveType.Cylinder, Vector3.up * 0.3f, new Vector3(0.4f, 0.6f, 0.4f), wood, false);
        CreateSimpleProp(barrel.transform, "Hoop1", PrimitiveType.Cylinder, Vector3.up * 0.1f, new Vector3(0.45f, 0.04f, 0.45f), new Color(0.2f, 0.2f, 0.2f), false);
        CreateSimpleProp(barrel.transform, "Hoop2", PrimitiveType.Cylinder, Vector3.up * 0.5f, new Vector3(0.45f, 0.04f, 0.45f), new Color(0.2f, 0.2f, 0.2f), false);
    }

    public static void CreateStake(Vector3 position, Transform parent)
    {
        GameObject stake = new GameObject("Stake");
        stake.transform.SetParent(parent, true);
        stake.transform.position = position;

        CreateSimpleProp(stake.transform, "Body", PrimitiveType.Cylinder, Vector3.up * 0.8f, new Vector3(0.08f, 1.6f, 0.08f), new Color(0.25f, 0.15f, 0.08f), false);
        CreateSimpleProp(stake.transform, "Tip", PrimitiveType.Cube, Vector3.up * 1.6f, new Vector3(0.12f, 0.25f, 0.12f), new Color(0.25f, 0.15f, 0.08f), false).transform.localRotation = Quaternion.Euler(45f, 0f, 0f);
    }

    public static void CreateBattleDebris(Vector3 position, Transform parent)
    {
        GameObject debris = new GameObject("Debris");
        debris.transform.SetParent(parent, true);
        debris.transform.position = position;

        // Сломанный щит
        GameObject shield = CreateSimpleProp(debris.transform, "Shield", PrimitiveType.Cube, Vector3.up * 0.1f, new Vector3(0.4f, 0.05f, 0.4f), new Color(0.3f, 0.2f, 0.15f), false);
        shield.transform.localRotation = Quaternion.Euler(Random.Range(0f, 90f), Random.Range(0f, 360f), 0f);
        // Стрела
        GameObject arrow = CreateSimpleProp(debris.transform, "Arrow", PrimitiveType.Cylinder, Vector3.up * 0.1f + new Vector3(Random.Range(-0.3f, 0.3f), 0f, Random.Range(-0.3f, 0.3f)), new Vector3(0.02f, 0.4f, 0.02f), new Color(0.3f, 0.2f, 0.1f), false);
        arrow.transform.localRotation = Quaternion.Euler(Random.Range(0f, 90f), Random.Range(0f, 360f), 0f);
        // Череп
        if (Random.value > 0.5f)
        {
            CreateSimpleProp(debris.transform, "Skull", PrimitiveType.Sphere, Vector3.up * 0.1f + new Vector3(Random.Range(-0.2f, 0.2f), 0f, Random.Range(-0.2f, 0.2f)), new Vector3(0.1f, 0.08f, 0.1f), new Color(0.7f, 0.6f, 0.5f), false);
        }
    }

    public static void CreateTorch(Vector3 position, Transform parent)
    {
        GameObject torch = new GameObject("Torch");
        torch.transform.SetParent(parent, true);
        torch.transform.position = position;

        CreateSimpleProp(torch.transform, "Post", PrimitiveType.Cylinder, Vector3.up * 1.0f, new Vector3(0.06f, 2.0f, 0.06f), new Color(0.3f, 0.2f, 0.1f), false);
        // Огонь
        Color fireColor = new Color(1f, 0.5f, 0.1f);
        CreateSimpleProp(torch.transform, "Flame", PrimitiveType.Sphere, Vector3.up * 2.0f, new Vector3(0.15f, 0.2f, 0.15f), fireColor, false, fireColor, 1.2f);
        // Свет (опционально) – можно добавить, но для производительности оставляем только визуал
    }

    public static void CreateWell(Vector3 position, Transform parent)
    {
        GameObject well = new GameObject("Well");
        well.transform.SetParent(parent, true);
        well.transform.position = position;

        CreateSimpleProp(well.transform, "Ring", PrimitiveType.Cylinder, Vector3.up * 0.3f, new Vector3(0.6f, 0.25f, 0.6f), new Color(0.4f, 0.35f, 0.3f), false);
        CreateSimpleProp(well.transform, "Water", PrimitiveType.Cylinder, Vector3.up * 0.15f, new Vector3(0.4f, 0.05f, 0.4f), new Color(0.05f, 0.1f, 0.2f), false);
        CreateSimpleProp(well.transform, "Post1", PrimitiveType.Cylinder, new Vector3(-0.5f, 0.7f, 0f), new Vector3(0.06f, 0.7f, 0.06f), new Color(0.3f, 0.2f, 0.1f), false);
        CreateSimpleProp(well.transform, "Post2", PrimitiveType.Cylinder, new Vector3(0.5f, 0.7f, 0f), new Vector3(0.06f, 0.7f, 0.06f), new Color(0.3f, 0.2f, 0.1f), false);
        CreateSimpleProp(well.transform, "Beam", PrimitiveType.Cube, new Vector3(0f, 0.9f, 0f), new Vector3(1.1f, 0.06f, 0.06f), new Color(0.3f, 0.2f, 0.1f), false);
        CreateSimpleProp(well.transform, "Bucket", PrimitiveType.Cube, new Vector3(0f, 0.3f, 0.2f), new Vector3(0.2f, 0.2f, 0.2f), new Color(0.3f, 0.2f, 0.1f), false);
    }

    public static void CreateCart(Vector3 position, Quaternion rotation, Transform parent)
    {
        GameObject cart = new GameObject("Cart");
        cart.transform.SetParent(parent, true);
        cart.transform.position = position;
        cart.transform.rotation = rotation;

        CreateSimpleProp(cart.transform, "Body", PrimitiveType.Cube, new Vector3(0f, 0.3f, 0f), new Vector3(1.6f, 0.4f, 0.9f), new Color(0.3f, 0.2f, 0.1f), false);
        CreateSimpleProp(cart.transform, "WheelL", PrimitiveType.Cylinder, new Vector3(-0.8f, 0.15f, -0.5f), new Vector3(0.25f, 0.06f, 0.25f), new Color(0.3f, 0.2f, 0.1f), false);
        CreateSimpleProp(cart.transform, "WheelR", PrimitiveType.Cylinder, new Vector3(0.8f, 0.15f, -0.5f), new Vector3(0.25f, 0.06f, 0.25f), new Color(0.3f, 0.2f, 0.1f), false);
        CreateSimpleProp(cart.transform, "Shaft", PrimitiveType.Cylinder, new Vector3(0f, 0.2f, 0.8f), new Vector3(0.06f, 0.06f, 0.6f), new Color(0.3f, 0.2f, 0.1f), false);
    }

    public static void CreateHaystack(Vector3 position, Transform parent)
    {
        GameObject hay = new GameObject("Haystack");
        hay.transform.SetParent(parent, true);
        hay.transform.position = position;

        Color hayColor = new Color(0.75f, 0.65f, 0.2f);
        CreateSimpleProp(hay.transform, "Base", PrimitiveType.Cube, Vector3.up * 0.25f, new Vector3(1.2f, 0.5f, 1.0f), hayColor, false);
        CreateSimpleProp(hay.transform, "Top", PrimitiveType.Cube, Vector3.up * 0.6f, new Vector3(0.9f, 0.4f, 0.8f), hayColor, false);
    }

    public static void CreateFenceSegment(Vector3 start, Vector3 end, Transform parent)
    {
        Vector3 dir = (end - start);
        float dist = dir.magnitude;
        if (dist < 0.05f) return;

        dir.Normalize();
        int posts = Mathf.Max(2, Mathf.RoundToInt(dist / 1.5f) + 1);

        for (int i = 0; i < posts; i++)
        {
            float t = i / (float)(posts - 1);
            Vector3 pos = Vector3.Lerp(start, end, t);
            pos.y = 0f;

            CreateWorldProp(parent, "FencePost_" + i, PrimitiveType.Cylinder, pos + Vector3.up * 0.35f, new Vector3(0.06f, 0.7f, 0.06f), new Color(0.3f, 0.2f, 0.1f), false);

            if (i < posts - 1)
            {
                float nextT = (i + 1) / (float)(posts - 1);
                Vector3 nextPos = Vector3.Lerp(start, end, nextT);
                nextPos.y = 0f;

                Vector3 railPos = (pos + nextPos) * 0.5f + Vector3.up * 0.65f;
                float railLen = Vector3.Distance(pos, nextPos);

                GameObject rail = CreateWorldProp(parent, "FenceRail_" + i, PrimitiveType.Cube, railPos, new Vector3(0.04f, 0.04f, railLen), new Color(0.3f, 0.2f, 0.1f), false);
                rail.transform.rotation = Quaternion.LookRotation(dir);
            }
        }
    }
}

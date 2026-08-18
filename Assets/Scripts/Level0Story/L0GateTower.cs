using UnityEngine;

/// <summary>
/// Лаба №7(1): деревянная сторожевая вышка у ворот. Даёт игроку возвышенную
/// позицию для лука над зоной удержания. Пандус — наклонный коллайдер, по которому
/// поднимается CharacterController (уклон ~30°, в пределах slopeLimit).
///
/// Строится кодом один раз. Поставлена слева от дороги, у края зоны ворот,
/// вне footprint замка/кузницы/деревни.
/// </summary>
public class L0GateTower : MonoBehaviour
{
    public static L0GateTower Instance { get; private set; }

    // R6: дальше влево и вниз по дороге — вне переднего обзора игрока (тот смотрит вдоль -Z).
    // Пандус (+Z) выходит в открытый грунт у дороги, не задевая замок/footprint.
    public Vector3 baseCenter = new Vector3(-16f, 0f, -24f);
    public float platformHeight = 4f;

    static readonly Color Wood      = new Color(0.46f, 0.30f, 0.14f);
    static readonly Color WoodDark  = new Color(0.32f, 0.20f, 0.09f);
    static readonly Color WoodLight = new Color(0.55f, 0.38f, 0.18f);

    public static L0GateTower Ensure()
    {
        if (Instance != null) return Instance;
        L0GateTower found = FindFirstObjectByType<L0GateTower>();
        if (found != null) { Instance = found; return found; }

        GameObject go = new GameObject("L0_GATE_TOWER");
        return go.AddComponent<L0GateTower>();
    }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        Build();
    }

    void Build()
    {
        GameObject root = new GameObject("GATE_TOWER_GEO");
        root.transform.position = baseCenter;
        root.transform.SetParent(transform);

        float h = platformHeight;
        float half = 2f; // полуширина площадки (4x4)

        // ── Опоры (4 столба) ──
        float[] xs = { -half + 0.4f, half - 0.4f };
        float[] zs = { -half + 0.4f, half - 0.4f };
        foreach (float px in xs)
            foreach (float pz in zs)
                Cyl(root, new Vector3(px, h * 0.5f, pz), new Vector3(0.28f, h * 0.5f, 0.28f), WoodDark, true);

        // Поперечные балки (жёсткость) на середине высоты
        Box(root, new Vector3(0f, h * 0.5f, -half + 0.4f), new Vector3(4f, 0.16f, 0.16f), WoodDark, false);
        Box(root, new Vector3(0f, h * 0.5f, half - 0.4f),  new Vector3(4f, 0.16f, 0.16f), WoodDark, false);

        // ── Площадка (ходибельная) ──
        Box(root, new Vector3(0f, h, 0f), new Vector3(4.2f, 0.4f, 4.2f), Wood, true);

        // ── Перила (3 стороны, проём со стороны пандуса = +Z) ──
        float ry = h + 0.7f;
        Box(root, new Vector3(0f, ry, -half), new Vector3(4.2f, 0.9f, 0.14f), WoodLight, true); // дальняя (к оркам)
        Box(root, new Vector3(-half, ry, 0f), new Vector3(0.14f, 0.9f, 4.2f), WoodLight, true); // левая
        Box(root, new Vector3(half, ry, 0f),  new Vector3(0.14f, 0.9f, 4.2f), WoodLight, true); // правая

        // ── Пандус (наклонный коллайдер от земли к площадке) ──
        // Низ — со стороны ворот (+Z), верх — у переднего края площадки.
        float run = 7f;
        float rise = h;
        float angle = Mathf.Atan2(rise, run) * Mathf.Rad2Deg; // ~29.7°
        float length = Mathf.Sqrt(run * run + rise * rise);

        // Локально: середина между точкой земли (0,0, half+? ) и верхом площадки.
        Vector3 lowEnd  = new Vector3(0f, 0.02f, half + (run - half) ); // далеко на +Z, на земле
        Vector3 highEnd = new Vector3(0f, rise, half - 0.3f);           // у края площадки
        Vector3 mid = (lowEnd + highEnd) * 0.5f;

        GameObject ramp = Box(root, mid, new Vector3(1.8f, 0.3f, length), Wood, true);
        ramp.name = "Tower_Ramp";
        ramp.transform.localRotation = Quaternion.Euler(angle, 0f, 0f);

        // Бортики пандуса, чтобы не свалиться
        GameObject railL = Box(root, mid + new Vector3(-0.9f, 0.45f, 0f), new Vector3(0.12f, 0.6f, length), WoodLight, false);
        railL.transform.localRotation = Quaternion.Euler(angle, 0f, 0f);
        GameObject railR = Box(root, mid + new Vector3(0.9f, 0.45f, 0f), new Vector3(0.12f, 0.6f, length), WoodLight, false);
        railR.transform.localRotation = Quaternion.Euler(angle, 0f, 0f);

        // Флаг сверху (маркер позиции)
        GameObject pole = Cyl(root, new Vector3(half - 0.3f, h + 1.6f, -half + 0.3f), new Vector3(0.06f, 0.9f, 0.06f), WoodDark, false);
        Box(pole, new Vector3(0f, 0.7f, 0.35f), new Vector3(0.04f, 0.4f, 0.7f), new Color(0.7f, 0.12f, 0.10f), false);
    }

    // ── Хелперы примитивов ──

    GameObject Box(GameObject parent, Vector3 localPos, Vector3 scale, Color c, bool keepCollider)
    {
        GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Setup(g, parent, localPos, scale, c, keepCollider);
        return g;
    }

    GameObject Cyl(GameObject parent, Vector3 localPos, Vector3 scale, Color c, bool keepCollider)
    {
        GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        Setup(g, parent, localPos, scale, c, keepCollider);
        return g;
    }

    void Setup(GameObject g, GameObject parent, Vector3 localPos, Vector3 scale, Color c, bool keepCollider)
    {
        g.transform.SetParent(parent.transform, false);
        g.transform.localPosition = localPos;
        g.transform.localScale = scale;
        g.transform.localRotation = Quaternion.identity;

        if (!keepCollider)
        {
            Collider col = g.GetComponent<Collider>();
            if (col != null) Destroy(col);
        }

        Renderer r = g.GetComponent<Renderer>();
        if (r != null)
        {
            Material m = new Material(LitShader()) { color = c };
            m.SetFloat("_Metallic", 0.0f);
            m.SetFloat("_Smoothness", 0.25f);
            r.material = m;
        }
    }

    static Shader LitShader() =>
        Shader.Find("Universal Render Pipeline/Lit")
        ?? Shader.Find("Standard")
        ?? Shader.Find("Diffuse");
}

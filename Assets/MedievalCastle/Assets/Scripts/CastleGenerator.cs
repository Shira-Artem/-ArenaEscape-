using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Главный файл запуска сцены "Средневековый замок".
/// ЛР3: чистый blockout из примитивов.
/// ЛР4: тот же уровень + окружение, материалы, декор и попытка использовать GLB-ассеты.
/// Автор: Широчин Артём, ИВТ-365.
/// </summary>
public class CastleGenerator : MonoBehaviour
{
    public enum LabMode
    {
        Lab3_Blockout,
        Lab4_Assets
    }

    [Header("Режим лабораторной")]
    public LabMode mode = LabMode.Lab3_Blockout;

    [Header("Что создавать")]
    public bool addTerrain = true;
    public bool addForestEnvironment = true;
    public bool addDecor = true;
    public bool spawnPlayer = true;

    [Header("ЛР4: импортированные ассеты")]
    public bool useImportedGlbAssets = true;
    public string glbFolder = "Assets/Models/GLB/";

    private CastleContext ctx;

    private void Start()
    {
        ClearGeneratedChildren();

        ctx = new CastleContext(this, mode, useImportedGlbAssets, glbFolder);
        ctx.CreateMaterials();
        SetupLighting();

        ctx.Root = new GameObject(mode == LabMode.Lab3_Blockout
            ? "=== GENERATED_CASTLE_LAB3_BLOCKOUT ==="
            : "=== GENERATED_CASTLE_LAB4_ASSETS ===");
        ctx.Root.transform.SetParent(transform);

        if (addTerrain)
        {
            CastleEnvironmentBuilder.Build(ctx, addForestEnvironment);
        }

        CastleBlockoutBuilder.Build(ctx);

        if (mode == LabMode.Lab4_Assets && addDecor)
        {
            CastleDecorBuilder.Build(ctx);
        }

        if (spawnPlayer)
        {
            SpawnPlayer();
        }

        Debug.Log("[CastleGenerator] Сцена собрана: Ворота → Двор → Донжон/Тронный зал → Подземелье.");
    }

    private void ClearGeneratedChildren()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }

    private void SetupLighting()
    {
        Light sun = FindFirstObjectByType<Light>();

        if (sun == null)
        {
            GameObject sunObj = new GameObject("Directional Light");
            sun = sunObj.AddComponent<Light>();
        }

        sun.name = mode == LabMode.Lab3_Blockout
            ? "Directional Light - Lab3 Neutral"
            : "Directional Light - Lab4 Warm Forest";

        sun.type = LightType.Directional;
        sun.transform.rotation = Quaternion.Euler(48f, -35f, 0f);

        if (mode == LabMode.Lab3_Blockout)
        {
            sun.intensity = 1.05f;
            sun.color = Color.white;
            RenderSettings.ambientLight = new Color(0.55f, 0.55f, 0.55f);
            RenderSettings.fog = false;
        }
        else
        {
            sun.intensity = 0.9f;
            sun.color = new Color(1f, 0.86f, 0.62f);
            RenderSettings.ambientLight = new Color(0.34f, 0.36f, 0.42f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.48f, 0.58f, 0.66f);
            RenderSettings.fogDensity = 0.008f;
        }
    }

    private void SpawnPlayer()
    {
        Camera existingCamera = Camera.main;

        CastlePlayerController[] oldPlayers = FindObjectsOfType<CastlePlayerController>();
        foreach (CastlePlayerController oldPlayer in oldPlayers)
        {
            Camera childCamera = oldPlayer.GetComponentInChildren<Camera>();
            if (childCamera != null)
            {
                childCamera.transform.SetParent(null);
                existingCamera = childCamera;
            }

            Destroy(oldPlayer.gameObject);
        }

        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.transform.position = new Vector3(0f, 1.25f, -32f);
        player.transform.rotation = Quaternion.Euler(0f, 0f, 0f);

        Renderer playerRenderer = player.GetComponent<Renderer>();
        if (playerRenderer != null)
        {
            playerRenderer.material = ctx.MatPlayer;
        }

        Rigidbody rb = player.AddComponent<Rigidbody>();
        rb.mass = 80f;
        rb.freezeRotation = true;

        player.AddComponent<CastlePlayerController>();

        Camera cam = existingCamera;

        if (cam == null)
        {
            GameObject camObj = new GameObject("Main Camera");
            cam = camObj.AddComponent<Camera>();
            cam.tag = "MainCamera";
        }

        cam.transform.SetParent(player.transform);
        cam.transform.localPosition = new Vector3(0f, 0.72f, 0f);
        cam.transform.localRotation = Quaternion.identity;
        cam.nearClipPlane = 0.05f;
        cam.fieldOfView = 68f;

        CastleCameraLook look = cam.GetComponent<CastleCameraLook>();
        if (look == null)
        {
            look = cam.gameObject.AddComponent<CastleCameraLook>();
        }

        look.player = player.transform;
        look.sensitivity = 135f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    /// <summary>
    /// Общий контекст сцены: материалы, границы замка и методы создания объектов.
    /// Остальные builder-файлы используют его, чтобы код не дублировался.
    /// </summary>
    public class CastleContext
    {
        public readonly CastleGenerator Owner;
        public readonly LabMode Mode;
        public readonly bool UseImportedGlbAssets;
        public readonly string GlbFolder;

        public GameObject Root;

        public Material MatStone;
        public Material MatStoneLight;
        public Material MatFloor;
        public Material MatWood;
        public Material MatIron;
        public Material MatGrass;
        public Material MatDirt;
        public Material MatRoad;
        public Material MatRed;
        public Material MatDark;
        public Material MatFire;
        public Material MatLeaves;
        public Material MatTrunk;
        public Material MatRock;
        public Material MatPlayer;
        public Material MatWater;

        public readonly float LeftX = -17f;
        public readonly float RightX = 17f;
        public readonly float FrontZ = -15f;
        public readonly float BackZ = 15f;

        public CastleContext(CastleGenerator owner, LabMode mode, bool useImportedGlbAssets, string glbFolder)
        {
            Owner = owner;
            Mode = mode;
            UseImportedGlbAssets = useImportedGlbAssets;
            GlbFolder = glbFolder;
        }

        public void CreateMaterials()
        {
            if (Mode == LabMode.Lab3_Blockout)
            {
                MatStone = NewMaterial(new Color(0.48f, 0.48f, 0.48f));
                MatStoneLight = NewMaterial(new Color(0.62f, 0.62f, 0.62f));
                MatFloor = NewMaterial(new Color(0.40f, 0.40f, 0.40f));
                MatWood = NewMaterial(new Color(0.52f, 0.52f, 0.52f));
                MatIron = NewMaterial(new Color(0.20f, 0.20f, 0.20f));
                MatGrass = NewMaterial(new Color(0.45f, 0.45f, 0.45f));
                MatDirt = NewMaterial(new Color(0.35f, 0.35f, 0.35f));
                MatRoad = NewMaterial(new Color(0.50f, 0.50f, 0.50f));
                MatRed = NewMaterial(new Color(0.58f, 0.58f, 0.58f));
                MatLeaves = NewMaterial(new Color(0.50f, 0.50f, 0.50f));
                MatTrunk = NewMaterial(new Color(0.42f, 0.42f, 0.42f));
                MatRock = NewMaterial(new Color(0.38f, 0.38f, 0.38f));
                MatWater = NewMaterial(new Color(0.35f, 0.35f, 0.35f));
            }
            else
            {
                MatStone = NewMaterial(new Color(0.34f, 0.33f, 0.30f));
                MatStoneLight = NewMaterial(new Color(0.62f, 0.60f, 0.54f));
                MatFloor = NewMaterial(new Color(0.42f, 0.39f, 0.33f));
                MatWood = NewMaterial(new Color(0.28f, 0.17f, 0.08f));
                MatIron = NewMaterial(new Color(0.12f, 0.13f, 0.14f));
                MatGrass = NewMaterial(new Color(0.22f, 0.45f, 0.20f));
                MatDirt = NewMaterial(new Color(0.36f, 0.26f, 0.14f));
                MatRoad = NewMaterial(new Color(0.48f, 0.41f, 0.31f));
                MatRed = NewMaterial(new Color(0.70f, 0.08f, 0.06f));
                MatLeaves = NewMaterial(new Color(0.13f, 0.34f, 0.14f));
                MatTrunk = NewMaterial(new Color(0.22f, 0.13f, 0.06f));
                MatRock = NewMaterial(new Color(0.36f, 0.36f, 0.34f));
                MatWater = NewMaterial(new Color(0.18f, 0.34f, 0.55f));
            }

            MatDark = NewMaterial(new Color(0.04f, 0.04f, 0.04f));
            MatFire = NewMaterial(new Color(1f, 0.45f, 0.05f), true);
            MatPlayer = NewMaterial(new Color(0.05f, 0.85f, 1f));
        }

        public Material NewMaterial(Color color, bool emissive = false)
        {
            Shader shader = Shader.Find("Standard");

            if (shader == null)
            {
                shader = Shader.Find("Universal Render Pipeline/Lit");
            }

            if (shader == null)
            {
                shader = Shader.Find("Diffuse");
            }

            Material material = new Material(shader);

            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", color);
            }

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            if (emissive)
            {
                material.EnableKeyword("_EMISSION");

                if (material.HasProperty("_EmissionColor"))
                {
                    material.SetColor("_EmissionColor", color * 1.4f);
                }
            }

            return material;
        }

        public GameObject Child(GameObject parent, string name)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent.transform);
            return obj;
        }

        public GameObject Cube(GameObject parent, string name, Vector3 position, Vector3 scale, Material material, bool collider = true)
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.name = name;
            obj.transform.SetParent(parent.transform);
            obj.transform.position = position;
            obj.transform.localScale = scale;
            ApplyMaterial(obj, material);
            ToggleCollider(obj, collider);
            return obj;
        }

        public GameObject Cylinder(GameObject parent, string name, Vector3 position, Vector3 scale, Material material, bool collider = true)
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            obj.name = name;
            obj.transform.SetParent(parent.transform);
            obj.transform.position = position;
            obj.transform.localScale = scale;
            ApplyMaterial(obj, material);
            ToggleCollider(obj, collider);
            return obj;
        }

        public GameObject Sphere(GameObject parent, string name, Vector3 position, Vector3 scale, Material material, bool collider = true)
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            obj.name = name;
            obj.transform.SetParent(parent.transform);
            obj.transform.position = position;
            obj.transform.localScale = scale;
            ApplyMaterial(obj, material);
            ToggleCollider(obj, collider);
            return obj;
        }

        public Light PointLight(GameObject parent, string name, Vector3 position, Color color, float intensity, float range, bool flicker = false)
        {
            GameObject lightObj = new GameObject(name);
            lightObj.transform.SetParent(parent.transform);
            lightObj.transform.position = position;

            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.intensity = intensity;
            light.range = range;

            if (flicker)
            {
                FlickerLightEffect effect = lightObj.AddComponent<FlickerLightEffect>();
                effect.baseIntensity = intensity;
            }

            return light;
        }

        public GameObject TryImportedAsset(GameObject parent, string fileName, string objectName, Vector3 position, Vector3 scale, Quaternion rotation)
        {
            if (!UseImportedGlbAssets)
            {
                return null;
            }

#if UNITY_EDITOR
            string path = GlbFolder;
            if (!path.EndsWith("/"))
            {
                path += "/";
            }

            path += fileName;

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab != null)
            {
                GameObject instance = Object.Instantiate(prefab);
                instance.name = objectName;
                instance.transform.SetParent(parent.transform);
                instance.transform.position = position;
                instance.transform.rotation = rotation;
                instance.transform.localScale = scale;
                Debug.Log("[Lab4] Использован импортированный ассет: " + path);
                return instance;
            }

            Debug.LogWarning("[Lab4] Ассет не найден: " + path + ". Оставлен fallback из примитивов.");
#endif

            return null;
        }

        private void ApplyMaterial(GameObject obj, Material material)
        {
            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer != null && material != null)
            {
                renderer.material = material;
            }
        }

        private void ToggleCollider(GameObject obj, bool collider)
        {
            if (!collider)
            {
                Collider component = obj.GetComponent<Collider>();
                if (component != null)
                {
                    Object.Destroy(component);
                }
            }
        }
    }
}

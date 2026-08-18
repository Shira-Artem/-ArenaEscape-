using UnityEngine;
using UnityEngine.SceneManagement;

public class L0QuickExitToOne : MonoBehaviour
{
    public string nextSceneName = "two";
    public bool debugAlwaysShowPortal = false;
    public Vector3 fallbackPosition = new Vector3(10f, 1f, -275f);
    public bool useFallbackWhenAtOrigin = true;
    public float interactionRadius = 5f;

    [Header("Optional Route Markers")]
    public bool buildTemporaryRoad = false;
    public float roadWidth = 10f;

    private const string PortalName = "LEVEL0_QUICK_EXIT_TO_ONE";
    private static readonly Vector3[] RoutePoints =
    {
        new Vector3(0f, 0.22f, -35f),
        new Vector3(4f, 0.22f, -100f),
        new Vector3(10f, 0.22f, -275f)
    };

    private GameProgressManager progress;
    private LevelTransitionPortal transitionPortal;
    private Level0ExitPortal exitPortal;
    private Transform routeRoot;
    private Transform playerTransform;
    private bool visualBuilt;
    private bool roadBuilt;
    private bool lastVisible;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoEnsureInLevel0()
    {
        if (SceneManager.GetActiveScene().name == "Level0_Castl")
            Ensure();
    }

    public static L0QuickExitToOne Ensure()
    {
        GameObject obj = GameObject.Find(PortalName);
        if (obj == null)
            obj = new GameObject(PortalName);

        L0QuickExitToOne helper = obj.GetComponent<L0QuickExitToOne>();
        if (helper == null)
            helper = obj.AddComponent<L0QuickExitToOne>();

        return helper;
    }

    private void Awake()
    {
        gameObject.name = PortalName;
    }

    private void Start()
    {
        progress = GameProgressManager.Ensure();
        ConfigurePortal();
        RefreshVisibility(true);
    }

    private void Update()
    {
        if (progress == null)
            progress = GameProgressManager.Ensure();

        ConfigurePortal();
        RefreshVisibility(false);
        ShowTwoPromptIfNear();
    }

    private void ConfigurePortal()
    {
        if (useFallbackWhenAtOrigin && transform.position == Vector3.zero)
            transform.position = fallbackPosition;

        if (transitionPortal == null)
            transitionPortal = GetComponent<LevelTransitionPortal>();
        if (transitionPortal == null)
            transitionPortal = gameObject.AddComponent<LevelTransitionPortal>();

        transitionPortal.nextSceneName = nextSceneName;
        transitionPortal.loadSceneOnUse = false;
        transitionPortal.requireUnlock = true;
        transitionPortal.interactionRadius = interactionRadius;
        transitionPortal.createVisualMarker = false;

        if (exitPortal == null)
            exitPortal = GetComponent<Level0ExitPortal>();
        if (exitPortal == null)
            exitPortal = gameObject.AddComponent<Level0ExitPortal>();

        exitPortal.nextSceneName = nextSceneName;
        exitPortal.loadNextScene = true;
        exitPortal.requireOrcVillageCleared = false;
        exitPortal.debugAllowAfterGateRaidCleared = true;
        exitPortal.hideUntilUnlocked = true;
        exitPortal.unlockedMessage = "F — перейти на уровень: two";
        exitPortal.lockedMessage = "Тестовый переход откроется после зачистки рейда у ворот.";

        if (!visualBuilt)
            BuildPortalVisual();

        if (buildTemporaryRoad && !roadBuilt)
            BuildTemporaryRouteMarkers();
    }

    private bool ShouldShowPortal()
    {
        return debugAlwaysShowPortal || (progress != null && progress.castleGateRaidCleared);
    }

    private void ShowTwoPromptIfNear()
    {
        if (!ShouldShowPortal())
            return;

        FindPlayerIfNeeded();
        if (playerTransform == null)
            return;

        if (Vector3.Distance(transform.position, playerTransform.position) <= interactionRadius)
            TransitionHintUI.Show("F — перейти на уровень: two", 0.08f);
    }

    private void FindPlayerIfNeeded()
    {
        if (playerTransform != null)
            return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;
    }

    private void BuildPortalVisual()
    {
        visualBuilt = true;

        Material stone = MakeMaterial(new Color(0.10f, 0.11f, 0.14f));
        Material cyanGlow = MakeGlowMaterial(new Color(0.10f, 0.80f, 1f), 1.8f);
        Material violetGlow = MakeGlowMaterial(new Color(0.45f, 0.18f, 1f), 1.25f);
        Material gold = MakeGlowMaterial(new Color(1f, 0.78f, 0.16f), 0.7f);
        Material fire = MakeGlowMaterial(new Color(1f, 0.35f, 0.05f), 1.6f);

        GameObject baseRing = Prim(PrimitiveType.Cylinder, transform, "QuickExit_Base", new Vector3(0f, -0.45f, 0f), Quaternion.identity, new Vector3(4.2f, 0.16f, 4.2f), stone);
        GameObject leftPillar = Prim(PrimitiveType.Cylinder, transform, "QuickExit_Pillar_L", new Vector3(-1.65f, 1.2f, 0f), Quaternion.identity, new Vector3(0.28f, 2.6f, 0.28f), stone);
        GameObject rightPillar = Prim(PrimitiveType.Cylinder, transform, "QuickExit_Pillar_R", new Vector3(1.65f, 1.2f, 0f), Quaternion.identity, new Vector3(0.28f, 2.6f, 0.28f), stone);
        GameObject top = Prim(PrimitiveType.Cube, transform, "QuickExit_Top", new Vector3(0f, 3.65f, 0f), Quaternion.identity, new Vector3(4.1f, 0.28f, 0.42f), gold);
        GameObject column = Prim(PrimitiveType.Cylinder, transform, "QuickExit_GlowColumn", new Vector3(0f, 1.45f, 0f), Quaternion.identity, new Vector3(1.25f, 2.9f, 1.25f), cyanGlow);
        GameObject core = Prim(PrimitiveType.Sphere, transform, "QuickExit_GlowCore", new Vector3(0f, 1.7f, 0f), Quaternion.identity, new Vector3(2.0f, 2.45f, 0.35f), violetGlow);

        DestroyCollider(baseRing);
        DestroyCollider(leftPillar);
        DestroyCollider(rightPillar);
        DestroyCollider(top);
        DestroyCollider(column);
        DestroyCollider(core);

        AddTorch(new Vector3(-3.1f, 0f, -2.6f), fire);
        AddTorch(new Vector3(3.1f, 0f, -2.6f), fire);
        AddTorch(new Vector3(-3.1f, 0f, 2.6f), fire);
        AddTorch(new Vector3(3.1f, 0f, 2.6f), fire);
        AddFloatingLabel();
    }

    private void BuildTemporaryRouteMarkers()
    {
        roadBuilt = true;

        GameObject root = new GameObject("QuickExit_RouteMarkers");
        root.transform.SetParent(transform, true);
        routeRoot = root.transform;

        Material edge = MakeMaterial(new Color(0.16f, 0.10f, 0.06f));
        Material fire = MakeGlowMaterial(new Color(1f, 0.36f, 0.05f), 1.45f);

        for (int i = 0; i < RoutePoints.Length - 1; i++)
        {
            Vector3 a = RoutePoints[i];
            Vector3 b = RoutePoints[i + 1];
            Vector3 dir = b - a;
            dir.y = 0f;
            Vector3 side = Vector3.Cross(Vector3.up, dir.normalized);
            int markerCount = Mathf.Max(3, Mathf.RoundToInt(dir.magnitude / 14f));

            for (int j = 0; j <= markerCount; j++)
            {
                float t = j / (float)markerCount;
                Vector3 p = Vector3.Lerp(a, b, t);
                CreateRoadEdgeMarker(p + side * (roadWidth * 0.5f), edge);
                CreateRoadEdgeMarker(p - side * (roadWidth * 0.5f), edge);

                if (j % 2 == 0)
                {
                    AddRoadTorch(p + side * (roadWidth * 0.6f), fire);
                    AddRoadTorch(p - side * (roadWidth * 0.6f), fire);
                }
            }
        }
    }

    private void CreateRoadEdgeMarker(Vector3 position, Material material)
    {
        position.y = 0.75f;
        CreateWorldPrim(PrimitiveType.Cylinder, "QuickExit_RoadStake", position, Quaternion.identity, new Vector3(0.16f, 0.75f, 0.16f), material);
    }

    private void AddRoadTorch(Vector3 position, Material fireMaterial)
    {
        position.y = 0f;
        Material wood = MakeMaterial(new Color(0.20f, 0.11f, 0.04f));
        GameObject post = CreateWorldPrim(PrimitiveType.Cylinder, "QuickExit_RoadTorch_Post", position + Vector3.up * 0.95f, Quaternion.identity, new Vector3(0.09f, 0.95f, 0.09f), wood);
        GameObject flame = CreateWorldPrim(PrimitiveType.Sphere, "QuickExit_RoadTorch_Flame", position + Vector3.up * 1.95f, Quaternion.identity, new Vector3(0.36f, 0.55f, 0.36f), fireMaterial);
        DestroyCollider(post);
        DestroyCollider(flame);
    }

    private void AddTorch(Vector3 localPosition, Material fireMaterial)
    {
        Material wood = MakeMaterial(new Color(0.20f, 0.11f, 0.04f));
        GameObject post = Prim(PrimitiveType.Cylinder, transform, "QuickExit_Torch_Post", localPosition + Vector3.up * 0.9f, Quaternion.identity, new Vector3(0.11f, 0.9f, 0.11f), wood);
        GameObject flame = Prim(PrimitiveType.Sphere, transform, "QuickExit_Torch_Flame", localPosition + Vector3.up * 1.9f, Quaternion.identity, new Vector3(0.45f, 0.7f, 0.45f), fireMaterial);
        DestroyCollider(post);
        DestroyCollider(flame);
    }

    private void AddFloatingLabel()
    {
        GameObject labelObj = new GameObject("QuickExit_Label");
        labelObj.transform.SetParent(transform, false);
        labelObj.transform.localPosition = new Vector3(0f, 4.35f, 0f);
        labelObj.transform.localRotation = Quaternion.identity;

        TextMesh text = labelObj.AddComponent<TextMesh>();
        text.text = "ПЕРЕХОД НА УРОВЕНЬ TWO";
        text.anchor = TextAnchor.MiddleCenter;
        text.alignment = TextAlignment.Center;
        text.characterSize = 0.28f;
        text.fontSize = 52;
        text.color = new Color(0.4f, 1f, 1f);

        L0Label label = labelObj.AddComponent<L0Label>();
        label.visibleDistance = 80f;
        label.fadeByDistance = false;
    }

    private GameObject Prim(PrimitiveType type, Transform parent, string name, Vector3 localPosition, Quaternion localRotation, Vector3 localScale, Material material)
    {
        GameObject obj = GameObject.CreatePrimitive(type);
        obj.name = name;
        obj.transform.SetParent(parent, false);
        obj.transform.localPosition = localPosition;
        obj.transform.localRotation = localRotation;
        obj.transform.localScale = localScale;

        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
            renderer.sharedMaterial = material;

        return obj;
    }

    private GameObject CreateWorldPrim(PrimitiveType type, string name, Vector3 position, Quaternion rotation, Vector3 scale, Material material)
    {
        GameObject obj = GameObject.CreatePrimitive(type);
        obj.name = name;
        obj.transform.SetParent(routeRoot, true);
        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.transform.localScale = scale;

        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
            renderer.sharedMaterial = material;

        DestroyCollider(obj);
        return obj;
    }

    private static Material MakeMaterial(Color color)
    {
        Material material = new Material(Shader.Find("Standard"));
        material.color = color;
        return material;
    }

    private static Material MakeGlowMaterial(Color color, float intensity)
    {
        Material material = MakeMaterial(color);
        material.EnableKeyword("_EMISSION");
        material.SetColor("_EmissionColor", color * intensity);
        return material;
    }

    private static void DestroyCollider(GameObject obj)
    {
        Collider collider = obj.GetComponent<Collider>();
        if (collider != null)
            Destroy(collider);
    }

    private void RefreshVisibility(bool force)
    {
        bool visible = ShouldShowPortal();
        if (!force && visible == lastVisible)
            return;

        lastVisible = visible;

        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
        foreach (Renderer renderer in renderers)
        {
            if (renderer != null)
                renderer.enabled = visible;
        }

        Collider[] colliders = GetComponentsInChildren<Collider>(true);
        foreach (Collider collider in colliders)
        {
            if (collider != null)
                collider.enabled = visible;
        }
    }
}

using UnityEngine;

/// <summary>
/// Процедурно строит мост / дорогу / переход между зонами из стандартных примитивов Unity.
/// Полностью независим от GameManager, EnemyAI, CastlePrologueBuilder и других систем.
/// </summary>
public enum BridgeStyle
{
    StoneBridge,
    WoodenBridge,
    OrcCampBridge,
    CastleRoad,
    MagicGate
}

public class TransitionBridgeBuilder : MonoBehaviour
{
    [Header("Bridge")]
    public BridgeStyle style = BridgeStyle.WoodenBridge;
    public Vector3 startPoint = Vector3.zero;
    public Vector3 endPoint = new Vector3(0f, 0f, -25f);
    public float width = 4f;
    public int segmentCount = 12;
    public bool buildOnStart = true;
    public bool clearBeforeBuild = true;

    [Header("Decor")]
    public string signText = "К следующей зоне";
    public bool addTorches = true;
    public bool addEndArch = true;

    private GameObject container;

    private void Start()
    {
        if (buildOnStart)
            BuildBridge();
    }

    [ContextMenu("Build Bridge")]
    public void BuildBridge()
    {
        if (clearBeforeBuild)
            ClearBridge();

        segmentCount = Mathf.Max(1, segmentCount);
        width = Mathf.Max(1f, width);

        container = new GameObject("Generated_Bridge_Visuals");
        container.transform.SetParent(transform, false);

        Vector3 delta = endPoint - startPoint;
        float totalLength = Mathf.Max(0.1f, delta.magnitude);
        Vector3 direction = delta.normalized;
        Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);

        float segmentLength = totalLength / segmentCount;

        Material baseMat = CreateMaterialForStyle(style, false);
        Material decorMat = CreateMaterialForStyle(style, true);
        Material accentMat = CreateAccentMaterialForStyle(style);

        for (int i = 0; i < segmentCount; i++)
        {
            Vector3 center = startPoint + direction * ((i + 0.5f) * segmentLength);

            BuildRoadSegment(center, rotation, segmentLength, baseMat);

            if (style != BridgeStyle.MagicGate)
            {
                Vector3 rightOffset = rotation * Vector3.right * (width * 0.5f);
                Vector3 leftOffset = rotation * Vector3.left * (width * 0.5f);

                BuildRail(center + leftOffset, rotation, segmentLength, decorMat);
                BuildRail(center + rightOffset, rotation, segmentLength, decorMat);

                if (i % 2 == 0)
                {
                    Vector3 postPos = startPoint + direction * (i * segmentLength);
                    BuildPillar(postPos + leftOffset, decorMat, accentMat);
                    BuildPillar(postPos + rightOffset, decorMat, accentMat);
                }
            }

            if (style == BridgeStyle.OrcCampBridge && i % 3 == 1)
                BuildSpikeCluster(center, rotation, accentMat);
        }

        BuildSign(rotation, decorMat);
        BuildEndFeatures(rotation, decorMat, accentMat);
    }

    [ContextMenu("Clear Bridge")]
    public void ClearBridge()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);

            if (child != null && child.name == "Generated_Bridge_Visuals")
            {
                if (Application.isPlaying)
                    Destroy(child.gameObject);
                else
                    DestroyImmediate(child.gameObject);
            }
        }

        container = null;
    }

    private void BuildRoadSegment(Vector3 center, Quaternion rotation, float segmentLength, Material mat)
    {
        GameObject road = GameObject.CreatePrimitive(PrimitiveType.Cube);
        road.name = "Road_Segment";
        road.transform.SetParent(container.transform);
        road.transform.position = center;
        road.transform.rotation = rotation;

        float lengthScale = (style == BridgeStyle.WoodenBridge || style == BridgeStyle.OrcCampBridge) ? 0.88f : 1.02f;
        float height = style == BridgeStyle.CastleRoad ? 0.14f : 0.22f;

        road.transform.localScale = new Vector3(width, height, segmentLength * lengthScale);
        road.GetComponent<Renderer>().material = mat;
    }

    private void BuildRail(Vector3 center, Quaternion rotation, float segmentLength, Material mat)
    {
        GameObject rail = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rail.name = "Bridge_Rail";
        rail.transform.SetParent(container.transform);
        rail.transform.position = center + Vector3.up * 0.55f;
        rail.transform.rotation = rotation;
        rail.transform.localScale = new Vector3(0.22f, 0.35f, segmentLength * 0.9f);
        rail.GetComponent<Renderer>().material = mat;
    }

    private void BuildPillar(Vector3 pos, Material mat, Material accentMat)
    {
        GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pillar.name = "Bridge_Pillar";
        pillar.transform.SetParent(container.transform);
        pillar.transform.position = pos + Vector3.up * 0.75f;
        pillar.transform.localScale = new Vector3(0.25f, 0.75f, 0.25f);
        pillar.GetComponent<Renderer>().material = mat;

        if (!addTorches)
            return;

        GameObject torch = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        torch.name = "Torch";
        torch.transform.SetParent(container.transform);
        torch.transform.position = pos + Vector3.up * 1.65f;
        torch.transform.localScale = new Vector3(0.12f, 0.32f, 0.12f);
        torch.GetComponent<Renderer>().material = accentMat;

        GameObject flame = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        flame.name = "Torch_Flame";
        flame.transform.SetParent(container.transform);
        flame.transform.position = pos + Vector3.up * 2.05f;
        flame.transform.localScale = new Vector3(0.35f, 0.35f, 0.35f);
        flame.GetComponent<Renderer>().material = CreateFlameMaterial();

        GameObject lightObj = new GameObject("Torch_Light");
        lightObj.transform.SetParent(container.transform);
        lightObj.transform.position = pos + Vector3.up * 2.05f;

        Light light = lightObj.AddComponent<Light>();
        light.type = LightType.Point;
        light.range = style == BridgeStyle.MagicGate ? 7f : 5f;
        light.intensity = 1.15f;
        light.color = style == BridgeStyle.OrcCampBridge ? Color.red : new Color(1f, 0.55f, 0.1f);
    }

    private void BuildSpikeCluster(Vector3 center, Quaternion rotation, Material accentMat)
    {
        Vector3 side = rotation * Vector3.right;
        Vector3 forward = rotation * Vector3.forward;

        for (int i = -1; i <= 1; i++)
        {
            Vector3 pos = center + side * (width * 0.65f) + forward * (i * 0.35f);

            GameObject spike = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            spike.name = "Orc_Spike";
            spike.transform.SetParent(container.transform);
            spike.transform.position = pos + Vector3.up * 0.55f;
            spike.transform.rotation = Quaternion.LookRotation(side, Vector3.up) * Quaternion.Euler(65f, 0f, 0f);
            spike.transform.localScale = new Vector3(0.08f, 0.55f, 0.08f);
            spike.GetComponent<Renderer>().material = accentMat;
        }
    }

    private void BuildSign(Quaternion rotation, Material mat)
    {
        Vector3 right = rotation * Vector3.right;
        Vector3 signPos = startPoint - (rotation * Vector3.forward) * 1.2f - right * (width * 0.75f);

        GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        post.name = "Transition_Sign_Post";
        post.transform.SetParent(container.transform);
        post.transform.position = signPos + Vector3.up * 0.9f;
        post.transform.localScale = new Vector3(0.12f, 0.9f, 0.12f);
        post.GetComponent<Renderer>().material = mat;

        GameObject board = GameObject.CreatePrimitive(PrimitiveType.Cube);
        board.name = "Transition_Sign_Board";
        board.transform.SetParent(container.transform);
        board.transform.position = signPos + Vector3.up * 1.75f;
        board.transform.rotation = rotation;
        board.transform.localScale = new Vector3(2.2f, 0.42f, 0.12f);
        board.GetComponent<Renderer>().material = mat;

        GameObject labelObj = new GameObject("Transition_Sign_Text");
        labelObj.transform.SetParent(container.transform);
        labelObj.transform.position = board.transform.position + (rotation * Vector3.back) * 0.08f;
        labelObj.transform.rotation = rotation * Quaternion.Euler(0f, 180f, 0f);

        TextMesh label = labelObj.AddComponent<TextMesh>();
        label.text = signText;
        label.characterSize = 0.18f;
        label.anchor = TextAnchor.MiddleCenter;
        label.alignment = TextAlignment.Center;
        label.color = Color.white;
    }

    private void BuildEndFeatures(Quaternion rotation, Material mat, Material accentMat)
    {
        Vector3 right = rotation * Vector3.right;
        Vector3 leftOffset = -right * (width * 0.55f);
        Vector3 rightOffset = right * (width * 0.55f);

        if (addEndArch || style == BridgeStyle.StoneBridge || style == BridgeStyle.CastleRoad || style == BridgeStyle.MagicGate)
        {
            GameObject left = GameObject.CreatePrimitive(PrimitiveType.Cube);
            left.name = "End_Arch_Left";
            left.transform.SetParent(container.transform);
            left.transform.position = endPoint + leftOffset + Vector3.up * 2f;
            left.transform.rotation = rotation;
            left.transform.localScale = new Vector3(0.55f, 4f, 0.55f);
            left.GetComponent<Renderer>().material = mat;

            GameObject rightObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightObj.name = "End_Arch_Right";
            rightObj.transform.SetParent(container.transform);
            rightObj.transform.position = endPoint + rightOffset + Vector3.up * 2f;
            rightObj.transform.rotation = rotation;
            rightObj.transform.localScale = new Vector3(0.55f, 4f, 0.55f);
            rightObj.GetComponent<Renderer>().material = mat;

            GameObject top = GameObject.CreatePrimitive(PrimitiveType.Cube);
            top.name = "End_Arch_Top";
            top.transform.SetParent(container.transform);
            top.transform.position = endPoint + Vector3.up * 4.1f;
            top.transform.rotation = rotation;
            top.transform.localScale = new Vector3(width + 1.2f, 0.55f, 0.55f);
            top.GetComponent<Renderer>().material = mat;
        }

        if (style == BridgeStyle.MagicGate)
        {
            GameObject orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            orb.name = "Magic_Gate_Orb";
            orb.transform.SetParent(container.transform);
            orb.transform.position = endPoint + Vector3.up * 2.2f;
            orb.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
            orb.GetComponent<Renderer>().material = accentMat;
        }
    }

    private Material CreateMaterialForStyle(BridgeStyle targetStyle, bool decor)
    {
        Material mat = new Material(Shader.Find("Standard"));

        switch (targetStyle)
        {
            case BridgeStyle.StoneBridge:
                mat.color = decor ? new Color(0.28f, 0.28f, 0.28f) : new Color(0.42f, 0.42f, 0.42f);
                break;

            case BridgeStyle.WoodenBridge:
                mat.color = decor ? new Color(0.30f, 0.18f, 0.08f) : new Color(0.46f, 0.29f, 0.12f);
                break;

            case BridgeStyle.OrcCampBridge:
                mat.color = decor ? new Color(0.48f, 0.05f, 0.04f) : new Color(0.24f, 0.16f, 0.10f);
                break;

            case BridgeStyle.CastleRoad:
                mat.color = decor ? new Color(0.60f, 0.58f, 0.50f) : new Color(0.45f, 0.45f, 0.40f);
                break;

            case BridgeStyle.MagicGate:
                mat.color = decor ? new Color(0f, 0.75f, 1f) : new Color(0.12f, 0.12f, 0.24f);
                break;
        }

        return mat;
    }

    private Material CreateAccentMaterialForStyle(BridgeStyle targetStyle)
    {
        Material mat = new Material(Shader.Find("Standard"));

        if (targetStyle == BridgeStyle.OrcCampBridge)
            mat.color = new Color(0.65f, 0.04f, 0.02f);
        else if (targetStyle == BridgeStyle.MagicGate)
            mat.color = new Color(0f, 0.85f, 1f);
        else
            mat.color = new Color(0.85f, 0.55f, 0.12f);

        return mat;
    }

    private Material CreateFlameMaterial()
    {
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = style == BridgeStyle.OrcCampBridge ? Color.red : new Color(1f, 0.45f, 0.05f);
        return mat;
    }
}

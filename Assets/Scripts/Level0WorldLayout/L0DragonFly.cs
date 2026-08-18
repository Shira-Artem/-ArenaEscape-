using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Процедурный декоративный дракон для L0Layout v6.
/// v6: более хищный силуэт — крупнее голова, рога, шипы на спине, лапы и более "крылатая" форма.
/// Чисто визуальный модуль: без урона, коллизий и логики AI.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class L0DragonFly : MonoBehaviour
{
    private enum DragonState { Cruising, Swooping, Torching, Recovering }

    [Header("Orbit Placement")]
    public Vector3 orbitCenter = new Vector3(18f, 0f, -145f);
    public float startAngleDegrees = -18f;
    public float baseFlyRadius = 18f;
    public float cruiseHeight = 26f;
    public float attackHeight = 14f;

    [Header("Flight")]
    [SerializeField] private DragonState currentState = DragonState.Cruising;
    public float cruiseSpeed = 0.32f;
    public float attackSpeed = 0.86f;
    public float torchSpeed = 0.16f;
    public float cruiseDuration = 7.8f;
    public float swoopDuration = 2.6f;
    public float fireDuration = 3.2f;
    public float recoverDuration = 3.8f;

    [Header("Anatomy")]
    [Range(18, 80)] public int spineSegments = 46;
    [Range(8, 28)] public int radialSegments = 16;
    public float bodyLength = 15.4f;
    public float maxRadius = 1.35f;
    public float wingSpan = 10.2f;
    public float bodyWaveAmplitude = 0.24f;

    [Header("Fire")]
    public bool enableFireParticles = true;
    public float fireCoreRate = 88f;
    public float fireTrailRate = 48f;
    public float smokeRate = 20f;

    private Mesh dragonMesh;
    private Material dragonMaterial;
    private Material eyeMaterial;
    private Material fireMaterial;
    private Material smokeMaterial;

    private readonly List<Vector3> baseVertices = new List<Vector3>(1800);
    private Vector3[] dynamicVertices;
    private int leftWingStart;
    private int rightWingStart;
    private const int WingRows = 8;
    private const int WingCols = 9;

    private float flightAngle;
    private float stateTimer;
    private float currentSpeed;
    private float targetRadius;
    private float targetHeight;
    private float currentRoll;
    private float currentPitch;
    private float jawOpenFactor;
    private bool initialized;

    private Transform mouthAnchor;
    private Light mouthLight;
    private ParticleSystem fireCore;
    private ParticleSystem fireTrail;
    private ParticleSystem smoke;

    private readonly List<Object> generatedDetails = new List<Object>(32);

    private void Start()
    {
        InitializeNow();
    }

    private void Update()
    {
        if (!initialized) InitializeNow();
        if (!Application.isPlaying) return;

        HandleStates();
        AnimateAndMove();
    }

    private void OnValidate()
    {
        spineSegments = Mathf.Clamp(spineSegments, 18, 80);
        radialSegments = Mathf.Clamp(radialSegments, 8, 28);
        bodyLength = Mathf.Max(4f, bodyLength);
        maxRadius = Mathf.Max(0.2f, maxRadius);
        wingSpan = Mathf.Max(2f, wingSpan);
        baseFlyRadius = Mathf.Max(8f, baseFlyRadius);
        cruiseHeight = Mathf.Max(3f, cruiseHeight);
        attackHeight = Mathf.Clamp(attackHeight, 2f, Mathf.Max(3f, cruiseHeight - 1f));
    }

    private void OnDestroy()
    {
        SafeDestroy(dragonMaterial);
        SafeDestroy(eyeMaterial);
        SafeDestroy(fireMaterial);
        SafeDestroy(smokeMaterial);
        SafeDestroy(dragonMesh);
    }

    public void InitializeNow()
    {
        if (initialized) return;

        MeshFilter meshFilter = GetComponent<MeshFilter>();
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();

        dragonMesh = new Mesh { name = "L0_ProceduralDragon_v6" };
        dragonMesh.MarkDynamic();
        meshFilter.sharedMesh = dragonMesh;

        dragonMaterial = CreateLitMaterial("M_L0_Dragon", new Color(0.42f, 0.05f, 0.04f, 1f), 0.14f, 0.48f, false);
        if (dragonMaterial.HasProperty("_EmissionColor")) dragonMaterial.SetColor("_EmissionColor", new Color(0.42f, 0.03f, 0.02f, 1f) * 0.45f);
        eyeMaterial = CreateLitMaterial("M_L0_DragonEyes", new Color(1f, 0.14f, 0.04f, 1f), 0f, 0.92f, false);
        if (eyeMaterial.HasProperty("_EmissionColor")) eyeMaterial.SetColor("_EmissionColor", new Color(1f, 0.08f, 0.02f, 1f) * 3.0f);
        meshRenderer.sharedMaterial = dragonMaterial;
        meshRenderer.shadowCastingMode = ShadowCastingMode.On;
        meshRenderer.receiveShadows = false;

        GenerateTopology();
        CreateDetailChildren();
        CreateMouthRigAndParticles();

        flightAngle = startAngleDegrees * Mathf.Deg2Rad;
        stateTimer = Mathf.Max(0.1f, cruiseDuration);
        currentSpeed = cruiseSpeed;
        targetRadius = baseFlyRadius;
        targetHeight = cruiseHeight;

        transform.position = orbitCenter + new Vector3(Mathf.Cos(flightAngle) * baseFlyRadius, cruiseHeight, Mathf.Sin(flightAngle) * baseFlyRadius);
        transform.rotation = Quaternion.LookRotation(new Vector3(-Mathf.Sin(flightAngle), 0f, Mathf.Cos(flightAngle)).normalized, Vector3.up);

        initialized = true;
        AnimateAndMove();
        SetFireEmission(false);
    }

    private void HandleStates()
    {
        stateTimer -= Time.deltaTime;

        switch (currentState)
        {
            case DragonState.Cruising:
                currentSpeed = Mathf.Lerp(currentSpeed, cruiseSpeed, Time.deltaTime * 2f);
                targetRadius = Mathf.Lerp(targetRadius, baseFlyRadius, Time.deltaTime * 1.2f);
                targetHeight = Mathf.Lerp(targetHeight, cruiseHeight, Time.deltaTime * 1.2f);
                jawOpenFactor = Mathf.Lerp(jawOpenFactor, 0.05f, Time.deltaTime * 4f);
                SetMouthLight(0f);
                SetFireEmission(false);
                if (stateTimer <= 0f)
                {
                    currentState = DragonState.Swooping;
                    stateTimer = Mathf.Max(0.1f, swoopDuration);
                }
                break;

            case DragonState.Swooping:
                currentSpeed = Mathf.Lerp(currentSpeed, attackSpeed, Time.deltaTime * 3.6f);
                targetRadius = Mathf.Lerp(targetRadius, baseFlyRadius * 0.70f, Time.deltaTime * 2.6f);
                targetHeight = Mathf.Lerp(targetHeight, attackHeight, Time.deltaTime * 2.8f);
                jawOpenFactor = Mathf.Lerp(jawOpenFactor, 0.45f, Time.deltaTime * 3.2f);
                SetMouthLight(0.7f + Mathf.Sin(Time.time * 18f) * 0.2f);
                SetFireEmission(false);
                if (stateTimer <= 0f)
                {
                    currentState = DragonState.Torching;
                    stateTimer = Mathf.Max(0.1f, fireDuration);
                }
                break;

            case DragonState.Torching:
                currentSpeed = Mathf.Lerp(currentSpeed, torchSpeed, Time.deltaTime * 5f);
                targetHeight = Mathf.Lerp(targetHeight, attackHeight + 2f, Time.deltaTime * 2f);
                jawOpenFactor = Mathf.Lerp(jawOpenFactor, 1f, Time.deltaTime * 8f);
                SetMouthLight(4.5f + Mathf.Sin(Time.time * 30f) * 1.0f);
                SetFireEmission(enableFireParticles);
                if (stateTimer <= 0f)
                {
                    currentState = DragonState.Recovering;
                    stateTimer = Mathf.Max(0.1f, recoverDuration);
                }
                break;

            case DragonState.Recovering:
                currentSpeed = Mathf.Lerp(currentSpeed, attackSpeed * 0.75f, Time.deltaTime * 2.2f);
                targetRadius = Mathf.Lerp(targetRadius, baseFlyRadius * 1.1f, Time.deltaTime * 2f);
                targetHeight = Mathf.Lerp(targetHeight, cruiseHeight + 3f, Time.deltaTime * 2f);
                jawOpenFactor = Mathf.Lerp(jawOpenFactor, 0.08f, Time.deltaTime * 3.5f);
                SetMouthLight(0f);
                SetFireEmission(false);
                if (stateTimer <= 0f)
                {
                    currentState = DragonState.Cruising;
                    stateTimer = Mathf.Max(0.1f, cruiseDuration);
                }
                break;
        }
    }

    private void AnimateAndMove()
    {
        flightAngle += currentSpeed * Time.deltaTime;

        Vector3 targetPosition = orbitCenter + new Vector3(Mathf.Cos(flightAngle) * targetRadius, targetHeight, Mathf.Sin(flightAngle) * targetRadius);
        Vector3 moveDir = targetPosition - transform.position;

        if (moveDir.sqrMagnitude > 0.0001f)
        {
            float targetRoll;
            float targetPitch;

            switch (currentState)
            {
                case DragonState.Swooping:
                    targetRoll = -40f; targetPitch = 18f; break;
                case DragonState.Torching:
                    targetRoll = -50f; targetPitch = -4f; break;
                case DragonState.Recovering:
                    targetRoll = 16f; targetPitch = -18f; break;
                default:
                    targetRoll = -18f; targetPitch = 0f; break;
            }

            currentRoll = Mathf.Lerp(currentRoll, targetRoll, Time.deltaTime * 3f);
            currentPitch = Mathf.Lerp(currentPitch, targetPitch, Time.deltaTime * 3f);

            Quaternion lookRot = Quaternion.LookRotation(moveDir.normalized, Vector3.up);
            Quaternion bankRot = Quaternion.Euler(currentPitch, 0f, currentRoll);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot * bankRot, Time.deltaTime * 4f);
        }

        transform.position = targetPosition;

        if (dynamicVertices == null || dynamicVertices.Length != baseVertices.Count) return;

        for (int i = 0; i < dynamicVertices.Length; i++)
            dynamicVertices[i] = baseVertices[i];

        AnimateBodyAndJaw();
        AnimateWings();

        dragonMesh.vertices = dynamicVertices;
        dragonMesh.RecalculateNormals();
        dragonMesh.RecalculateBounds();

        UpdateMouthAnchor();
    }

    private void AnimateBodyAndJaw()
    {
        int jawStartRing = Mathf.Max(0, spineSegments - 6);

        for (int i = 0; i <= spineSegments; i++)
        {
            float u = i / (float)spineSegments;
            float waveFade = Mathf.Sin(u * Mathf.PI);
            float spineWave = Mathf.Sin(Time.time * 3.1f - u * 7.1f) * bodyWaveAmplitude * waveFade;
            float neckCounterWave = Mathf.Sin(Time.time * 4.6f + u * 5.2f) * 0.07f * Mathf.SmoothStep(0.55f, 1f, u);

            for (int j = 0; j < radialSegments; j++)
            {
                int idx = i * radialSegments + j;
                dynamicVertices[idx].x += spineWave + neckCounterWave;

                if (i >= jawStartRing && dynamicVertices[idx].y < 0f)
                {
                    float jawStrength = Mathf.InverseLerp(jawStartRing, spineSegments, i);
                    dynamicVertices[idx].y -= jawOpenFactor * 0.58f * jawStrength;
                    dynamicVertices[idx].z += jawOpenFactor * 0.32f * jawStrength;
                }
            }
        }
    }

    private void AnimateWings()
    {
        float frequency;
        float amplitude;
        float foldFactor;

        switch (currentState)
        {
            case DragonState.Swooping:
                frequency = 2.0f; amplitude = 0.22f; foldFactor = 0.70f; break;
            case DragonState.Torching:
                frequency = 1.25f; amplitude = 0.40f; foldFactor = 0.42f; break;
            case DragonState.Recovering:
                frequency = 5.6f; amplitude = 1.25f; foldFactor = 0.08f; break;
            default:
                frequency = 3.0f; amplitude = 0.85f; foldFactor = 0.0f; break;
        }

        AnimateWingBlock(leftWingStart, -1f, frequency, amplitude, foldFactor);
        AnimateWingBlock(rightWingStart, 1f, frequency, amplitude, foldFactor);
    }

    private void AnimateWingBlock(int startIndex, float sideSign, float frequency, float amplitude, float foldFactor)
    {
        for (int r = 0; r < WingRows; r++)
        {
            float rowFactor = r / (float)(WingRows - 1);
            for (int c = 0; c < WingCols; c++)
            {
                int idx = startIndex + r * WingCols + c;
                float colFactor = c / (float)(WingCols - 1);
                float flap = Mathf.Sin(Time.time * frequency - colFactor * 3.6f + rowFactor * 0.8f) * amplitude * Mathf.Pow(colFactor, 1.6f);
                dynamicVertices[idx].y += flap - rowFactor * 0.08f;
                dynamicVertices[idx].x -= sideSign * foldFactor * colFactor * wingSpan * 0.34f;
                dynamicVertices[idx].z -= foldFactor * (0.7f + rowFactor) * 1.5f;
            }
        }
    }

    private void GenerateTopology()
    {
        List<int> triangles = new List<int>(4600);
        baseVertices.Clear();

        for (int i = 0; i <= spineSegments; i++)
        {
            float u = i / (float)spineSegments;
            float bodyProfile = Mathf.Pow(Mathf.Sin(u * Mathf.PI), 0.64f);
            float radius = maxRadius * Mathf.Max(0.12f, bodyProfile);

            if (u < 0.12f)
            {
                radius *= Mathf.Lerp(0.14f, 0.82f, u / 0.12f);
            }
            else if (u > 0.72f)
            {
                float headT = Mathf.InverseLerp(0.72f, 1f, u);
                radius = Mathf.Lerp(radius, maxRadius * 0.60f, headT);
            }

            float zPos = Mathf.Lerp(-bodyLength * 0.60f, bodyLength * 0.50f, u);
            float neckLift = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.60f, 1f, u)) * maxRadius * 0.46f;
            float bellyDrop = Mathf.Sin(u * Mathf.PI) * maxRadius * 0.06f;
            float headWidth = u > 0.80f ? Mathf.Lerp(1f, 1.18f, Mathf.InverseLerp(0.80f, 1f, u)) : 1f;

            for (int j = 0; j < radialSegments; j++)
            {
                float theta = j / (float)radialSegments * Mathf.PI * 2f;
                float x = Mathf.Cos(theta) * radius * 0.90f * headWidth;
                float y = Mathf.Sin(theta) * radius + neckLift - bellyDrop;
                if (u > 0.84f && Mathf.Sin(theta) < -0.3f) y -= maxRadius * 0.10f; // lower jaw bulk
                baseVertices.Add(new Vector3(x, y, zPos));
            }
        }

        for (int i = 0; i < spineSegments; i++)
        {
            for (int j = 0; j < radialSegments; j++)
            {
                int nextJ = (j + 1) % radialSegments;
                int v0 = i * radialSegments + j;
                int v1 = i * radialSegments + nextJ;
                int v2 = (i + 1) * radialSegments + j;
                int v3 = (i + 1) * radialSegments + nextJ;
                triangles.Add(v0); triangles.Add(v2); triangles.Add(v1);
                triangles.Add(v1); triangles.Add(v2); triangles.Add(v3);
            }
        }

        int wingAttachRing = Mathf.RoundToInt(spineSegments * 0.48f);
        float attachZ = Mathf.Lerp(-bodyLength * 0.60f, bodyLength * 0.50f, wingAttachRing / (float)spineSegments);
        Vector3 leftAnchor = new Vector3(-maxRadius * 0.48f, maxRadius * 0.55f, attachZ - 0.4f);
        Vector3 rightAnchor = new Vector3(maxRadius * 0.48f, maxRadius * 0.55f, attachZ - 0.4f);

        leftWingStart = baseVertices.Count;
        BuildWingVertices(leftAnchor, -1f);
        BuildWingTriangles(triangles, leftWingStart, false);

        rightWingStart = baseVertices.Count;
        BuildWingVertices(rightAnchor, 1f);
        BuildWingTriangles(triangles, rightWingStart, true);

        AddSimpleHornsAndTailFin(triangles);

        dynamicVertices = baseVertices.ToArray();
        dragonMesh.Clear();
        dragonMesh.vertices = dynamicVertices;
        dragonMesh.triangles = triangles.ToArray();
        dragonMesh.RecalculateNormals();
        dragonMesh.RecalculateBounds();
    }

    private void BuildWingVertices(Vector3 anchor, float sideSign)
    {
        for (int r = 0; r < WingRows; r++)
        {
            float rowFactor = r / (float)(WingRows - 1);
            for (int c = 0; c < WingCols; c++)
            {
                float colFactor = c / (float)(WingCols - 1);
                float span = colFactor * wingSpan;
                float drop = 0.18f + rowFactor * 0.42f + Mathf.Pow(colFactor, 1.25f) * 0.55f;
                float sweepBack = rowFactor * bodyLength * 0.36f + Mathf.Pow(colFactor, 1.15f) * 1.2f;
                float ribCurve = Mathf.Sin(rowFactor * Mathf.PI) * 0.42f;
                Vector3 p = anchor + new Vector3(sideSign * span, -drop + ribCurve, -sweepBack);
                p.y += Mathf.Sin(colFactor * Mathf.PI) * 0.32f - rowFactor * 0.05f;
                baseVertices.Add(p);
            }
        }
    }

    private void BuildWingTriangles(List<int> triangles, int start, bool flip)
    {
        for (int r = 0; r < WingRows - 1; r++)
        {
            for (int c = 0; c < WingCols - 1; c++)
            {
                int v0 = start + r * WingCols + c;
                int v1 = v0 + 1;
                int v2 = start + (r + 1) * WingCols + c;
                int v3 = v2 + 1;
                if (!flip)
                {
                    triangles.Add(v0); triangles.Add(v1); triangles.Add(v2);
                    triangles.Add(v1); triangles.Add(v3); triangles.Add(v2);
                }
                else
                {
                    triangles.Add(v0); triangles.Add(v2); triangles.Add(v1);
                    triangles.Add(v1); triangles.Add(v2); triangles.Add(v3);
                }
            }
        }
    }

    private void AddSimpleHornsAndTailFin(List<int> triangles)
    {
        float headZ = bodyLength * 0.50f;
        AddPyramid(triangles, new Vector3(-0.40f, maxRadius * 1.0f, headZ - 0.7f), 0.20f, 1.0f);
        AddPyramid(triangles, new Vector3(0.40f, maxRadius * 1.0f, headZ - 0.7f), 0.20f, 1.0f);
        AddPyramid(triangles, new Vector3(0f, maxRadius * 0.8f, headZ - 1.1f), 0.12f, 0.5f);

        int start = baseVertices.Count;
        float tailZ = -bodyLength * 0.60f;
        baseVertices.Add(new Vector3(0f, 0.05f, tailZ));
        baseVertices.Add(new Vector3(0f, 1.25f, tailZ + 1.2f));
        baseVertices.Add(new Vector3(0f, 0.05f, tailZ + 2.0f));
        triangles.Add(start); triangles.Add(start + 1); triangles.Add(start + 2);
        triangles.Add(start + 2); triangles.Add(start + 1); triangles.Add(start);
    }

    private void AddPyramid(List<int> triangles, Vector3 center, float halfBase, float height)
    {
        int start = baseVertices.Count;
        baseVertices.Add(center + new Vector3(-halfBase, 0f, -halfBase));
        baseVertices.Add(center + new Vector3(halfBase, 0f, -halfBase));
        baseVertices.Add(center + new Vector3(halfBase, 0f, halfBase));
        baseVertices.Add(center + new Vector3(-halfBase, 0f, halfBase));
        baseVertices.Add(center + Vector3.up * height);

        triangles.Add(start); triangles.Add(start + 4); triangles.Add(start + 1);
        triangles.Add(start + 1); triangles.Add(start + 4); triangles.Add(start + 2);
        triangles.Add(start + 2); triangles.Add(start + 4); triangles.Add(start + 3);
        triangles.Add(start + 3); triangles.Add(start + 4); triangles.Add(start);
        triangles.Add(start); triangles.Add(start + 1); triangles.Add(start + 2);
        triangles.Add(start); triangles.Add(start + 2); triangles.Add(start + 3);
    }

    private void CreateDetailChildren()
    {
        CreateHornDetails();
        CreateBackSpikes();
        CreateLegs();
        CreateTailBlade();
        CreateEyes();
    }

    private void CreateHornDetails()
    {
        CreateDetailPrimitive("Horn_L", PrimitiveType.Cube, new Vector3(-0.44f, maxRadius * 1.02f, bodyLength * 0.42f), new Vector3(0.18f, 0.18f, 1.6f), Quaternion.Euler(-25f, -15f, 24f), dragonMaterial);
        CreateDetailPrimitive("Horn_R", PrimitiveType.Cube, new Vector3(0.44f, maxRadius * 1.02f, bodyLength * 0.42f), new Vector3(0.18f, 0.18f, 1.6f), Quaternion.Euler(-25f, 15f, -24f), dragonMaterial);
        CreateDetailPrimitive("JawSpike_L", PrimitiveType.Cube, new Vector3(-0.38f, -maxRadius * 0.18f, bodyLength * 0.46f), new Vector3(0.12f, 0.12f, 0.82f), Quaternion.Euler(20f, -12f, 12f), dragonMaterial);
        CreateDetailPrimitive("JawSpike_R", PrimitiveType.Cube, new Vector3(0.38f, -maxRadius * 0.18f, bodyLength * 0.46f), new Vector3(0.12f, 0.12f, 0.82f), Quaternion.Euler(20f, 12f, -12f), dragonMaterial);
    }

    private void CreateBackSpikes()
    {
        for (int i = 0; i < 9; i++)
        {
            float t = i / 8f;
            float z = Mathf.Lerp(-bodyLength * 0.18f, bodyLength * 0.34f, t);
            float y = maxRadius * (0.62f + (1f - Mathf.Abs(t - 0.5f) * 1.2f) * 0.22f);
            float height = Mathf.Lerp(0.55f, 1.1f, 1f - Mathf.Abs(t - 0.5f) * 1.4f);
            CreateDetailPrimitive("BackSpike_" + i, PrimitiveType.Cube, new Vector3(0f, y, z), new Vector3(0.12f, height, 0.36f), Quaternion.Euler(-8f, 0f, 0f), dragonMaterial);
        }
    }

    private void CreateLegs()
    {
        CreateLegSet("Front", bodyLength * 0.12f, 0.65f, 0.95f);
        CreateLegSet("Rear", -bodyLength * 0.18f, 0.8f, 1.08f);
    }

    private void CreateLegSet(string prefix, float z, float x, float len)
    {
        CreateDetailPrimitive(prefix + "Leg_L", PrimitiveType.Cube, new Vector3(-x, -maxRadius * 0.55f, z), new Vector3(0.18f, len, 0.18f), Quaternion.Euler(20f, 0f, 10f), dragonMaterial);
        CreateDetailPrimitive(prefix + "Leg_R", PrimitiveType.Cube, new Vector3(x, -maxRadius * 0.55f, z), new Vector3(0.18f, len, 0.18f), Quaternion.Euler(20f, 0f, -10f), dragonMaterial);
        CreateDetailPrimitive(prefix + "Claw_L", PrimitiveType.Cube, new Vector3(-x, -maxRadius * 1.0f, z + 0.15f), new Vector3(0.36f, 0.08f, 0.42f), Quaternion.Euler(8f, 0f, 10f), dragonMaterial);
        CreateDetailPrimitive(prefix + "Claw_R", PrimitiveType.Cube, new Vector3(x, -maxRadius * 1.0f, z + 0.15f), new Vector3(0.36f, 0.08f, 0.42f), Quaternion.Euler(8f, 0f, -10f), dragonMaterial);
    }

    private void CreateTailBlade()
    {
        CreateDetailPrimitive("TailBlade", PrimitiveType.Cube, new Vector3(0f, 0.22f, -bodyLength * 0.68f), new Vector3(0.12f, 1.2f, 0.7f), Quaternion.Euler(0f, 0f, 0f), dragonMaterial);
    }

    private void CreateEyes()
    {
        CreateDetailPrimitive("Eye_L", PrimitiveType.Sphere, new Vector3(-0.20f, maxRadius * 0.35f, bodyLength * 0.47f), new Vector3(0.16f, 0.10f, 0.10f), Quaternion.identity, eyeMaterial);
        CreateDetailPrimitive("Eye_R", PrimitiveType.Sphere, new Vector3(0.20f, maxRadius * 0.35f, bodyLength * 0.47f), new Vector3(0.16f, 0.10f, 0.10f), Quaternion.identity, eyeMaterial);
    }

    private void CreateDetailPrimitive(string name, PrimitiveType type, Vector3 localPos, Vector3 localScale, Quaternion localRot, Material material)
    {
        GameObject part = GameObject.CreatePrimitive(type);
        part.name = name;
        part.transform.SetParent(transform, false);
        part.transform.localPosition = localPos;
        part.transform.localRotation = localRot;
        part.transform.localScale = localScale;

        Collider col = part.GetComponent<Collider>();
        if (col != null) SafeDestroy(col);

        Renderer r = part.GetComponent<Renderer>();
        if (r != null)
        {
            r.sharedMaterial = material;
            r.shadowCastingMode = ShadowCastingMode.On;
            r.receiveShadows = false;
        }

        generatedDetails.Add(part);
    }

    private void CreateMouthRigAndParticles()
    {
        GameObject mouthObject = new GameObject("Dragon_Mouth_Rig");
        mouthObject.transform.SetParent(transform, false);
        mouthAnchor = mouthObject.transform;
        generatedDetails.Add(mouthObject);

        mouthLight = mouthObject.AddComponent<Light>();
        mouthLight.type = LightType.Point;
        mouthLight.color = new Color(1f, 0.18f, 0.03f, 1f);
        mouthLight.range = 18f;
        mouthLight.intensity = 0f;

        fireMaterial = CreateParticleMaterial("M_L0_DragonFire", new Color(1f, 0.22f, 0.03f, 1f), false);
        smokeMaterial = CreateParticleMaterial("M_L0_DragonSmoke", new Color(0.2f, 0.2f, 0.2f, 0.35f), true);

        fireCore = CreateParticleLayer("Fire_Core", fireMaterial, new Color(1f, 0.72f, 0.08f, 1f), 0.35f, 0.7f, 18f, 24f, 6f, 0.06f, 120);
        fireTrail = CreateParticleLayer("Fire_Trail", fireMaterial, new Color(1f, 0.12f, 0.02f, 1f), 0.55f, 1.2f, 11f, 17f, 12f, 0.12f, 120);
        smoke = CreateParticleLayer("Smoke", smokeMaterial, new Color(0.18f, 0.18f, 0.18f, 0.38f), 1.3f, 2.2f, 4f, 8f, 18f, 0.22f, 90);

        SetFireEmission(false);
    }

    private ParticleSystem CreateParticleLayer(string layerName, Material material, Color startColor, float minLife, float maxLife, float minSpeed, float maxSpeed, float coneAngle, float radius, int maxParticles)
    {
        GameObject layer = new GameObject(layerName);
        layer.transform.SetParent(mouthAnchor, false);
        generatedDetails.Add(layer);

        ParticleSystem ps = layer.AddComponent<ParticleSystem>();
        ParticleSystem.MainModule main = ps.main;
        main.loop = true;
        main.playOnAwake = false;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startLifetime = new ParticleSystem.MinMaxCurve(minLife, maxLife);
        main.startSpeed = new ParticleSystem.MinMaxCurve(minSpeed, maxSpeed);
        main.startSize = new ParticleSystem.MinMaxCurve(0.18f, 0.55f);
        main.startColor = startColor;
        main.maxParticles = maxParticles;

        ParticleSystem.EmissionModule emission = ps.emission;
        emission.enabled = false;
        emission.rateOverTime = 0f;

        ParticleSystem.ShapeModule shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = coneAngle;
        shape.radius = radius;
        shape.length = 0.8f;

        ParticleSystem.ColorOverLifetimeModule colorOverLife = ps.colorOverLifetime;
        colorOverLife.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(startColor, 0f),
                new GradientColorKey(new Color(0.25f, 0.03f, 0f, startColor.a), 1f)
            },
            new[]
            {
                new GradientAlphaKey(startColor.a, 0f),
                new GradientAlphaKey(0f, 1f)
            });
        colorOverLife.color = gradient;

        ParticleSystem.SizeOverLifetimeModule sizeOverLife = ps.sizeOverLifetime;
        sizeOverLife.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve(new Keyframe(0f, 0.35f), new Keyframe(0.35f, 1f), new Keyframe(1f, 1.9f));
        sizeOverLife.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        ParticleSystemRenderer renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.sharedMaterial = material;
        return ps;
    }

    private void UpdateMouthAnchor()
    {
        if (mouthAnchor == null) return;
        float headZ = bodyLength * 0.50f + 0.75f;
        Vector3 localMouth = new Vector3(0f, maxRadius * 0.05f - jawOpenFactor * 0.26f, headZ);
        mouthAnchor.position = transform.TransformPoint(localMouth);
        mouthAnchor.rotation = transform.rotation;
    }

    private void SetMouthLight(float targetIntensity)
    {
        if (mouthLight == null) return;
        mouthLight.intensity = Mathf.Lerp(mouthLight.intensity, Mathf.Max(0f, targetIntensity), Time.deltaTime * 8f);
    }

    private void SetFireEmission(bool active)
    {
        SetEmissionRate(fireCore, active ? fireCoreRate : 0f);
        SetEmissionRate(fireTrail, active ? fireTrailRate : 0f);
        SetEmissionRate(smoke, active ? smokeRate : 0f);

        if (active)
        {
            if (fireCore != null && !fireCore.isPlaying) fireCore.Play();
            if (fireTrail != null && !fireTrail.isPlaying) fireTrail.Play();
            if (smoke != null && !smoke.isPlaying) smoke.Play();
        }
    }

    private static void SetEmissionRate(ParticleSystem ps, float rate)
    {
        if (ps == null) return;
        ParticleSystem.EmissionModule emission = ps.emission;
        emission.enabled = rate > 0.01f;
        emission.rateOverTime = rate;
    }

    private static Material CreateLitMaterial(string materialName, Color color, float metallic, float smoothness, bool transparent)
    {
        Shader shader = FindBestLitShader();
        Material material = new Material(shader) { name = materialName };
        ApplyColor(material, color);
        if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", metallic);
        if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
        if (material.HasProperty("_Glossiness")) material.SetFloat("_Glossiness", smoothness);
        if (transparent) MakeTransparent(material);
        return material;
    }

    private static Material CreateParticleMaterial(string materialName, Color color, bool transparent)
    {
        Shader shader = Shader.Find("Legacy Shaders/Particles/Alpha Blended");
        if (shader == null) shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (shader == null) shader = Shader.Find("Sprites/Default");
        if (shader == null) shader = FindBestLitShader();

        Material material = new Material(shader) { name = materialName };
        ApplyColor(material, color);
        if (transparent) MakeTransparent(material);
        return material;
    }

    private static Shader FindBestLitShader()
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        if (shader == null) shader = Shader.Find("Diffuse");
        if (shader == null) shader = Shader.Find("Hidden/InternalErrorShader");
        return shader;
    }

    private static void ApplyColor(Material material, Color color)
    {
        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
        if (material.HasProperty("_Color")) material.SetColor("_Color", color);
        if (material.HasProperty("_EmissionColor"))
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", color * 0.8f);
        }
    }

    private static void MakeTransparent(Material material)
    {
        if (material.HasProperty("_Surface")) material.SetFloat("_Surface", 1f);
        if (material.HasProperty("_Mode")) material.SetFloat("_Mode", 3f);
        material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = (int)RenderQueue.Transparent;
    }

    private static void SafeDestroy(Object obj)
    {
        if (obj == null) return;
        if (Application.isPlaying) Destroy(obj);
        else DestroyImmediate(obj);
    }
}

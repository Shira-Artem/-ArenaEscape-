using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CastlePrologueBuilder : MonoBehaviour
{
    public static CastlePrologueBuilder Instance { get; private set; }

    [Header("Build")]
    public bool buildOnStart = true;
    public bool clearBeforeBuild = true;
    public bool resetProgressOnStart = true;
    public bool addLevel0Setup = true;
    public bool lockWeaponsUntilFound = true;
    public bool autoStartGateRaidAfterIntro = true;

    [Header("Interaction")]
    public Transform player;
    public float interactionDistance = 4.0f;
    public float labelVisibleDistance = 10f;
    public bool createWorldLabels = false;
    public bool showLegacyTaskPanel = false;

    [Header("Story Positions")]
    public Vector3 elderSpot = new Vector3(-4.8f, 0.05f, -26.8f);
    public Vector3 refugeeSpot = new Vector3(-24f, 0.05f, -45f);
    public Vector3 guardSpot = new Vector3(-3.8f, 0.05f, -17.7f);
    public Vector3 blacksmithSpot = new Vector3(5.75f, 0.82f, -23.8f);  // центр платформы кузницы, лицом ко входу (не в текстурах)
    public Vector3 weaponRackSpot = new Vector3(10.3f, 0.00f, -25.0f);  // у восточной стены кузницы, уровень земли
    public Vector3 kingNoteSpot = new Vector3(0f, 0.92f, -22.7f);
    public Vector3 campfireSpot = new Vector3(-22f, 0.05f, -44f);

    private GameProgressManager progress;
    private GameObject root;
    private readonly List<L0StoryInteractable> interactables = new List<L0StoryInteractable>();
    private readonly List<L0StoryLabel> labels = new List<L0StoryLabel>();
    private readonly List<GameObject> weaponObjects = new List<GameObject>();

    private L0StoryInteractable currentTarget;
    private GameObject noteObject;

    private WeaponController weaponController;
    private Camera weaponCamera;
    private L0ObjectiveGuide objectiveGuide;

    private string message = "";
    private float messageUntil;
    private string dialogueSpeaker = "";
    private string dialogueText = "";
    private float dialogueUntil;

    private GUIStyle panelStyle;
    private GUIStyle titleStyle;
    private GUIStyle normalStyle;
    private GUIStyle doneStyle;
    private GUIStyle mutedStyle;
    private GUIStyle dialogueNameStyle;
    private GUIStyle dialogueTextStyle;
    private GUIStyle promptStyle;

    private IEnumerator Start()
    {
        Instance = this;
        progress = GameProgressManager.Ensure();

        if (resetProgressOnStart)
            progress.ResetCastleProgress();

        // Даём CastleGenerator построить замок.
        yield return null;
        yield return null;
        yield return null;
        yield return null;

        if (addLevel0Setup)
        {
            Level0CastleSetup setup = GetComponent<Level0CastleSetup>();
            if (setup == null)
                setup = gameObject.AddComponent<Level0CastleSetup>();

            setup.setupOnStart = false;
            setup.SetupArenaPlayer();
        }

        yield return null;
        yield return null;

        L0StoryAudio.Ensure();
        EnsureLevel0HudServices();

        // Единая система крафта: руда на дороге → огненные стрелы (см. L0CraftingStation).
        // Старый L0BlacksmithQuest (+15 урона мечу) удалён как дубль.
        L0CraftingStation.Ensure();

        // Форсируем корректную позицию кузнеца (на случай старого значения в сцене):
        // центр платформы, лицом ко входу — не клиппится в забор/стойку.
        blacksmithSpot = new Vector3(5.75f, 0.82f, -23.8f);

        FindPlayer();
        FindWeaponController();

        if (buildOnStart)
            BuildStory();

        ApplyWeaponLock();
        RefreshObjectiveGuide();

        // R5: короче по времени, чтобы интро не висело и не закрывало обзор.
        ShowDialogue(
            "Средневековый замок",
            "Король-Защитник убит. Орки пришли из гор. Найди старосту у ворот замка.",
            5f
        );
    }

    private void Update()
    {
        if (progress == null)
            progress = GameProgressManager.Ensure();

        if (player == null)
            FindPlayer();

        RefreshStoryObjects();
        ApplyWeaponLock();
        UpdateLabels();
        FindNearestInteractable();
        RefreshObjectiveGuide();

        if (currentTarget != null && Input.GetKeyDown(KeyCode.F))
            currentTarget.Interact(this);
    }

    [ContextMenu("Build Story")]
    public void BuildStory()
    {
        if (clearBeforeBuild)
            ClearStory();

        root = new GameObject("LEVEL0_STORY_CONTENT");
        root.transform.SetParent(transform);

        interactables.Clear();
        labels.Clear();
        weaponObjects.Clear();
        noteObject = null;

        BuildStoryProps();

        CreateNpc("Староста", "Elder",
            "Король пал этой ночью. Орки идут из гор: жгут деревни, тащат людей в свой лагерь. Нам нужен герой, который откроет путь и остановит их.",
            elderSpot, new Color(0.25f, 0.55f, 0.95f));

        CreateNpc("Жительница", "Refugee",
            "Моя деревня горела. Мы успели добежать до замка, но остальные остались за воротами. Орки ведут пленников к своей деревне у гор.",
            refugeeSpot, new Color(0.75f, 0.45f, 1f));

        CreateNpc("Стражник", "Guard",
            "Ворота заперты. Я не выпущу тебя без подготовки. Поговори с кузнецом, проверь оружие и возьми королевский указ.",
            guardSpot, new Color(0.85f, 0.18f, 0.16f));

        CreateNpc("Кузнец", "Blacksmith",
            "Оружия мало, но я сохранил меч, копьё и лук. Проверь всё на стойке, потом возьми указ короля. " +
            "А найдёшь на дороге 3 куска огненной руды — принеси, и я выкую тебе огненные стрелы.",
            blacksmithSpot, new Color(1f, 0.55f, 0.18f));

        // Оружие приподнято (+0.55f Y) — висит на стойке, не лежит в земле
        CreateWeapon("Меч",   GameProgressManager.WeaponId.Sword, "Меч проверен. Ближний бой готов.",                     weaponRackSpot + new Vector3(-1.0f, 0.55f, 0.20f), 0);
        CreateWeapon("Копьё", GameProgressManager.WeaponId.Spear, "Копьё проверено. Им можно держать орков на расстоянии.", weaponRackSpot + new Vector3( 0.0f, 0.55f, 0.20f), 1);
        CreateWeapon("Лук",   GameProgressManager.WeaponId.Bow,   "Лук проверен. Теперь можно бить орков издалека.",        weaponRackSpot + new Vector3( 1.0f, 0.55f, 0.20f), 2);

        CreateNote();

        RefreshStoryObjects();
        RefreshObjectiveGuide();
    }

    [ContextMenu("Clear Story")]
    public void ClearStory()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            if (child != null && child.name == "LEVEL0_STORY_CONTENT")
                Destroy(child.gameObject);
        }

        interactables.Clear();
        labels.Clear();
        weaponObjects.Clear();
        noteObject = null;
    }

    private void BuildStoryProps()
    {
        CreateCampfire(campfireSpot);
        CreateTable(new Vector3(0f, 0.05f, -22.7f));
        CreateWeaponRack(weaponRackSpot);
        // Наковальня в L0BlacksmithArea уже есть — дубликат убран
        BuildNpcEnvironment();
    }

    private void BuildNpcEnvironment()
    {
        Transform r = root.transform;

        // --- У СТАРОСТЫ (elder: -4.8, 0.05, -26.8) ---
        CastlePropFactory.CreateBench(elderSpot + new Vector3(-1.8f, 0f, -0.8f), 35f, r);
        CastlePropFactory.CreateBench(elderSpot + new Vector3(1.6f, 0f, -1.0f), -20f, r);
        CastlePropFactory.CreateGateTorch(elderSpot + new Vector3(-2.5f, 0f, 1.2f), r, 1.8f);
        CastlePropFactory.CreateCratesAndBags(elderSpot + new Vector3(2.2f, 0f, 0.5f), r);
        CastlePropFactory.CreateBarrel(elderSpot + new Vector3(-1.5f, 0f, 1.5f), r);

        // --- У СТРАЖНИКА (guard: -3.8, 0.05, -17.7) ---
        CastlePropFactory.CreateGateTorch(guardSpot + new Vector3(-2.0f, 0f, 0.5f), r, 2.2f);
        CastlePropFactory.CreateGateTorch(guardSpot + new Vector3(2.0f, 0f, 0.5f), r, 2.2f);
        CastlePropFactory.CreateBarrel(guardSpot + new Vector3(1.6f, 0f, -0.6f), r);
        CastlePropFactory.CreateCratesAndBags(guardSpot + new Vector3(-1.8f, 0f, -0.4f), r);

        // --- У БЕЖЕНКИ (refugee: -24, 0.05, -45) ---
        CastlePropFactory.CreateCampfire(refugeeSpot + new Vector3(2.5f, 0f, -1.5f), r);
        CastlePropFactory.CreateBench(refugeeSpot + new Vector3(1.5f, 0f, -2.5f), 60f, r);
        CastlePropFactory.CreateBench(refugeeSpot + new Vector3(3.5f, 0f, -0.5f), -30f, r);
        CastlePropFactory.CreateCratesAndBags(refugeeSpot + new Vector3(-1.8f, 0f, -1.0f), r);
        CastlePropFactory.CreateBarrel(refugeeSpot + new Vector3(-2.2f, 0f, 0.5f), r);
        CastlePropFactory.CreateBarrel(refugeeSpot + new Vector3(-1.6f, 0f, 1.2f), r);
    }

    private void CreateNpc(string name, string role, string text, Vector3 pos, Color color)
    {
        GameObject npc = new GameObject("NPC_" + name);
        npc.transform.SetParent(root.transform);
        npc.transform.position = pos;

        L0StoryNpc interact = npc.AddComponent<L0StoryNpc>();
        interact.displayName = name;
        interact.role = role;
        interact.dialogueText = text;

        // Твёрдое тело — игрок больше не проходит сквозь NPC (фикс «проваливается в текстуры»).
        // Радиус 0.4 не мешает взаимодействию (оно по дистанции 3.5 м).
        CapsuleCollider body = npc.AddComponent<CapsuleCollider>();
        body.center = new Vector3(0f, 0.95f, 0f);
        body.radius = 0.4f;
        body.height = 1.9f;

        BuildHumanoid(npc.transform, color, role);
        CreateLabel(npc.transform, name, 2.1f);

        interactables.Add(interact);
    }

    private void BuildHumanoid(Transform parent, Color body, string role)
    {
        Color skin = new Color(0.86f, 0.68f, 0.52f);
        Color dark = body * 0.65f;
        dark.a = 1f;
        Color boots = new Color(0.22f, 0.14f, 0.06f);
        Color belt = new Color(0.18f, 0.12f, 0.05f);
        Color pants = new Color(0.15f, 0.13f, 0.10f);

        // --- Торс (капсула + грудь) ---
        CreatePrimitive(parent, "Body", PrimitiveType.Capsule, new Vector3(0, 0.98f, 0), new Vector3(0.48f, 0.58f, 0.40f), body);
        CreatePrimitive(parent, "Chest", PrimitiveType.Cube, new Vector3(0, 1.08f, 0.04f), new Vector3(0.42f, 0.36f, 0.10f), dark);

        // --- Плечи ---
        CreatePrimitive(parent, "Shoulder_L", PrimitiveType.Sphere, new Vector3(-0.30f, 1.32f, 0), new Vector3(0.16f, 0.14f, 0.14f), body);
        CreatePrimitive(parent, "Shoulder_R", PrimitiveType.Sphere, new Vector3(0.30f, 1.32f, 0), new Vector3(0.16f, 0.14f, 0.14f), body);

        // --- Голова (крупнее, + шея) ---
        CreatePrimitive(parent, "Neck", PrimitiveType.Cylinder, new Vector3(0, 1.45f, 0), new Vector3(0.10f, 0.08f, 0.10f), skin);
        CreatePrimitive(parent, "Head", PrimitiveType.Sphere, new Vector3(0, 1.62f, 0), new Vector3(0.32f, 0.34f, 0.30f), skin);

        // --- Глаза ---
        CreatePrimitive(parent, "Eye_L", PrimitiveType.Sphere, new Vector3(-0.07f, 1.65f, 0.12f), new Vector3(0.05f, 0.04f, 0.03f), new Color(0.15f, 0.12f, 0.08f));
        CreatePrimitive(parent, "Eye_R", PrimitiveType.Sphere, new Vector3(0.07f, 1.65f, 0.12f), new Vector3(0.05f, 0.04f, 0.03f), new Color(0.15f, 0.12f, 0.08f));

        // --- Нос ---
        CreatePrimitive(parent, "Nose", PrimitiveType.Cube, new Vector3(0, 1.60f, 0.14f), new Vector3(0.04f, 0.06f, 0.04f), skin * 0.92f);

        // --- Руки (плечо + предплечье + кисть) ---
        CreatePrimitive(parent, "UpperArm_L", PrimitiveType.Cylinder, new Vector3(-0.34f, 1.16f, 0), new Vector3(0.10f, 0.22f, 0.10f), body).transform.localRotation = Quaternion.Euler(0, 0, -12f);
        CreatePrimitive(parent, "ForeArm_L", PrimitiveType.Cylinder, new Vector3(-0.38f, 0.78f, 0.04f), new Vector3(0.08f, 0.22f, 0.08f), skin);
        CreatePrimitive(parent, "Hand_L", PrimitiveType.Sphere, new Vector3(-0.40f, 0.56f, 0.06f), new Vector3(0.08f, 0.06f, 0.06f), skin);

        CreatePrimitive(parent, "UpperArm_R", PrimitiveType.Cylinder, new Vector3(0.34f, 1.16f, 0), new Vector3(0.10f, 0.22f, 0.10f), body).transform.localRotation = Quaternion.Euler(0, 0, 12f);
        CreatePrimitive(parent, "ForeArm_R", PrimitiveType.Cylinder, new Vector3(0.38f, 0.78f, 0.04f), new Vector3(0.08f, 0.22f, 0.08f), skin);
        CreatePrimitive(parent, "Hand_R", PrimitiveType.Sphere, new Vector3(0.40f, 0.56f, 0.06f), new Vector3(0.08f, 0.06f, 0.06f), skin);

        // --- Пояс ---
        CreatePrimitive(parent, "Belt", PrimitiveType.Cube, new Vector3(0, 0.72f, 0), new Vector3(0.46f, 0.08f, 0.38f), belt);
        CreatePrimitive(parent, "BeltBuckle", PrimitiveType.Cube, new Vector3(0, 0.72f, 0.18f), new Vector3(0.06f, 0.06f, 0.03f), new Color(0.65f, 0.55f, 0.20f));

        // --- Ноги (бёдра + голени) ---
        CreatePrimitive(parent, "Thigh_L", PrimitiveType.Cylinder, new Vector3(-0.12f, 0.52f, 0), new Vector3(0.12f, 0.22f, 0.12f), pants);
        CreatePrimitive(parent, "Shin_L", PrimitiveType.Cylinder, new Vector3(-0.13f, 0.22f, 0), new Vector3(0.10f, 0.18f, 0.10f), pants);
        CreatePrimitive(parent, "Thigh_R", PrimitiveType.Cylinder, new Vector3(0.12f, 0.52f, 0), new Vector3(0.12f, 0.22f, 0.12f), pants);
        CreatePrimitive(parent, "Shin_R", PrimitiveType.Cylinder, new Vector3(0.13f, 0.22f, 0), new Vector3(0.10f, 0.18f, 0.10f), pants);

        // --- Сапоги ---
        CreatePrimitive(parent, "Boot_L", PrimitiveType.Cube, new Vector3(-0.13f, 0.06f, 0.03f), new Vector3(0.12f, 0.12f, 0.18f), boots);
        CreatePrimitive(parent, "Boot_R", PrimitiveType.Cube, new Vector3(0.13f, 0.06f, 0.03f), new Vector3(0.12f, 0.12f, 0.18f), boots);

        // --- Роль-специфичное ---
        if (role == "Elder")
        {
            // Борода
            CreatePrimitive(parent, "Beard", PrimitiveType.Cube, new Vector3(0, 1.48f, 0.10f), new Vector3(0.16f, 0.18f, 0.08f), new Color(0.85f, 0.82f, 0.78f));
            CreatePrimitive(parent, "BeardTip", PrimitiveType.Cube, new Vector3(0, 1.36f, 0.08f), new Vector3(0.10f, 0.12f, 0.06f), new Color(0.80f, 0.78f, 0.72f));
            // Волосы / шляпа
            CreatePrimitive(parent, "HairBack", PrimitiveType.Cube, new Vector3(0, 1.72f, -0.08f), new Vector3(0.28f, 0.14f, 0.14f), new Color(0.82f, 0.80f, 0.75f));
            // Посох с навершием
            CreatePrimitive(parent, "Staff", PrimitiveType.Cylinder, new Vector3(0.48f, 0.85f, 0), new Vector3(0.04f, 0.80f, 0.04f), new Color(0.35f, 0.20f, 0.08f));
            CreatePrimitive(parent, "StaffGem", PrimitiveType.Sphere, new Vector3(0.48f, 1.66f, 0), new Vector3(0.08f, 0.08f, 0.08f), new Color(0.25f, 0.55f, 0.95f));
            // Мантия / воротник
            CreatePrimitive(parent, "Collar", PrimitiveType.Cube, new Vector3(0, 1.38f, 0), new Vector3(0.44f, 0.06f, 0.36f), dark);
        }
        else if (role == "Guard")
        {
            // Повязка на голове
            CreatePrimitive(parent, "Bandage", PrimitiveType.Cube, new Vector3(0, 1.66f, 0.02f), new Vector3(0.34f, 0.06f, 0.32f), Color.white);
            CreatePrimitive(parent, "BandageKnot", PrimitiveType.Cube, new Vector3(0.14f, 1.68f, -0.10f), new Vector3(0.08f, 0.04f, 0.06f), Color.white);
            // Пятно крови
            CreatePrimitive(parent, "BloodStain", PrimitiveType.Sphere, new Vector3(-0.06f, 1.67f, 0.14f), new Vector3(0.06f, 0.04f, 0.02f), new Color(0.55f, 0.05f, 0.03f));
            // Нагрудник стражника
            CreatePrimitive(parent, "ChestPlate", PrimitiveType.Cube, new Vector3(0, 1.12f, 0.15f), new Vector3(0.36f, 0.30f, 0.06f), new Color(0.28f, 0.28f, 0.30f));
            // Щит прислонённый
            CreatePrimitive(parent, "Shield", PrimitiveType.Cube, new Vector3(-0.42f, 0.50f, -0.12f), new Vector3(0.05f, 0.50f, 0.34f), new Color(0.22f, 0.22f, 0.25f)).transform.localRotation = Quaternion.Euler(0, 8f, 12f);
            // Наклон (раненый)
            parent.Find("Body").localPosition = new Vector3(0, 0.88f, 0.06f);
            parent.Find("Body").localRotation = Quaternion.Euler(12, 0, 0);
            parent.Find("Head").localPosition = new Vector3(0, 1.52f, 0.12f);
        }
        else if (role == "Blacksmith")
        {
            // Кузнец — здоровяк, заметно крупнее прочих NPC.
            parent.localScale = Vector3.one * 1.15f;
            // Фартук кожаный
            CreatePrimitive(parent, "Apron", PrimitiveType.Cube, new Vector3(0, 0.88f, 0.20f), new Vector3(0.36f, 0.50f, 0.04f), new Color(0.16f, 0.10f, 0.05f));
            CreatePrimitive(parent, "ApronStrap_L", PrimitiveType.Cube, new Vector3(-0.14f, 1.22f, 0.12f), new Vector3(0.04f, 0.16f, 0.03f), new Color(0.16f, 0.10f, 0.05f));
            CreatePrimitive(parent, "ApronStrap_R", PrimitiveType.Cube, new Vector3(0.14f, 1.22f, 0.12f), new Vector3(0.04f, 0.16f, 0.03f), new Color(0.16f, 0.10f, 0.05f));
            // Повязка на голову
            CreatePrimitive(parent, "Headband", PrimitiveType.Cube, new Vector3(0, 1.72f, 0), new Vector3(0.34f, 0.06f, 0.32f), new Color(0.35f, 0.22f, 0.10f));
            // Щипцы в руке
            CreatePrimitive(parent, "TongsHandle", PrimitiveType.Cylinder, new Vector3(-0.42f, 0.62f, 0.14f), new Vector3(0.03f, 0.28f, 0.03f), new Color(0.20f, 0.20f, 0.22f));
            CreatePrimitive(parent, "TongsJaw", PrimitiveType.Cube, new Vector3(-0.42f, 0.38f, 0.18f), new Vector3(0.04f, 0.04f, 0.12f), new Color(0.20f, 0.20f, 0.22f));
            // Мускулатура (шире плечи)
            parent.Find("Shoulder_L").localScale = new Vector3(0.20f, 0.16f, 0.16f);
            parent.Find("Shoulder_R").localScale = new Vector3(0.20f, 0.16f, 0.16f);
        }
        else if (role == "Refugee")
        {
            // Платок на голове
            CreatePrimitive(parent, "Headscarf", PrimitiveType.Cube, new Vector3(0, 1.74f, -0.02f), new Vector3(0.36f, 0.10f, 0.34f), new Color(0.55f, 0.42f, 0.30f));
            CreatePrimitive(parent, "ScarfDrape", PrimitiveType.Cube, new Vector3(0.12f, 1.58f, -0.08f), new Vector3(0.08f, 0.20f, 0.06f), new Color(0.50f, 0.40f, 0.28f));
            // Корзина в руке
            CreatePrimitive(parent, "Basket", PrimitiveType.Cylinder, new Vector3(0.42f, 0.60f, 0.12f), new Vector3(0.18f, 0.14f, 0.18f), new Color(0.55f, 0.40f, 0.22f));
            CreatePrimitive(parent, "BasketRim", PrimitiveType.Cube, new Vector3(0.42f, 0.74f, 0.12f), new Vector3(0.20f, 0.02f, 0.20f), new Color(0.50f, 0.38f, 0.20f));
            // Юбка
            CreatePrimitive(parent, "Skirt", PrimitiveType.Cube, new Vector3(0, 0.50f, 0), new Vector3(0.42f, 0.36f, 0.36f), body * 0.85f);
        }
    }

    private void CreateWeapon(string name, GameProgressManager.WeaponId id, string pickupText, Vector3 pos, int visual)
    {
        GameObject obj = new GameObject("Pickup_" + name);
        obj.transform.SetParent(root.transform);
        obj.transform.position = pos;

        L0StoryWeapon interact = obj.AddComponent<L0StoryWeapon>();
        interact.displayName = name;
        interact.weaponId = id;
        interact.pickupText = pickupText;

        if (visual == 0)
            CreatePrimitive(obj.transform, "Blade", PrimitiveType.Cube, new Vector3(0, 0.55f, 0), new Vector3(0.07f, 0.85f, 0.04f), new Color(0.8f, 0.86f, 0.95f));
        else if (visual == 1)
            CreatePrimitive(obj.transform, "Shaft", PrimitiveType.Cylinder, new Vector3(0, 0.65f, 0), new Vector3(0.035f, 0.9f, 0.035f), new Color(0.42f, 0.24f, 0.08f));
        else
            CreatePrimitive(obj.transform, "Bow", PrimitiveType.Cylinder, new Vector3(0, 0.45f, 0), new Vector3(0.05f, 0.55f, 0.05f), new Color(0.48f, 0.28f, 0.08f));

        obj.transform.rotation = Quaternion.Euler(0, 0, -48f);

        CreateLabel(obj.transform, name, 1.1f);
        interactables.Add(interact);
        weaponObjects.Add(obj);
    }

    private void CreateNote()
    {
        GameObject obj = new GameObject("Pickup_Указ Короля");
        obj.transform.SetParent(root.transform);
        obj.transform.position = kingNoteSpot;

        L0StoryNote note = obj.AddComponent<L0StoryNote>();
        note.displayName = "Указ Короля";
        note.noteText = "Последний указ Короля: “Защитите людей. Освободите деревню и уничтожьте орочий лагерь у гор.”";

        CreatePrimitive(obj.transform, "Scroll", PrimitiveType.Cube, new Vector3(0, 0.05f, 0), new Vector3(0.55f, 0.06f, 0.35f), new Color(0.95f, 0.90f, 0.55f));
        CreatePrimitive(obj.transform, "Seal", PrimitiveType.Sphere, new Vector3(0.15f, 0.10f, -0.02f), new Vector3(0.10f, 0.10f, 0.035f), new Color(0.7f, 0.05f, 0.04f));

        CreateLabel(obj.transform, "Указ Короля", 0.7f);

        interactables.Add(note);
        noteObject = obj;
    }

    private void RefreshStoryObjects()
    {
        if (progress == null)
            return;

        bool weaponsVisible = progress.talkedToBlacksmith;
        foreach (GameObject obj in weaponObjects)
        {
            if (obj != null)
                obj.SetActive(weaponsVisible);
        }

        if (noteObject != null)
            noteObject.SetActive(progress.HasAllWeapons());
    }

    private void ApplyWeaponLock()
    {
        if (!lockWeaponsUntilFound)
            return;

        FindWeaponController();

        bool unlocked = progress != null && progress.HasAllWeapons();

        if (weaponController != null)
            weaponController.enabled = unlocked;

        if (weaponCamera != null)
        {
            Renderer[] renderers = weaponCamera.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer r in renderers)
            {
                if (r != null)
                    r.enabled = unlocked;
            }
        }
    }

    private void FindWeaponController()
    {
        if (weaponController != null)
            return;

        Camera cam = Camera.main;
        if (cam == null)
            return;

        weaponCamera = cam;
        weaponController = cam.GetComponent<WeaponController>();
    }

    private void FindPlayer()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
            player = p.transform;
    }

    private void FindNearestInteractable()
    {
        currentTarget = null;

        if (player == null)
            return;

        // Окно крафта открыто — ничего не подсвечиваем/не берём.
        if (L0CraftingUI.IsOpen)
            return;

        Camera cam = Camera.main;
        Vector3 eye = cam != null ? cam.transform.position : player.position;
        Vector3 fwd = cam != null ? cam.transform.forward : player.forward;

        // Выбор по ВЗГЛЯДУ (на что смотришь), с откатом на ближайшее —
        // иначе среди кучки предметов (оружие на стойке) трудно прицелиться.
        float bestDot = 0.25f;            // конус ~75°
        L0StoryInteractable looked = null;
        float nearestDist = interactionDistance;
        L0StoryInteractable nearest = null;

        foreach (L0StoryInteractable i in interactables)
        {
            if (i == null || !i.gameObject.activeInHierarchy)
                continue;

            // Кузнец после разговора не перехватывает E — чтобы спокойно открыть крафт
            // и брать оружие рядом, не «залипая» на NPC.
            L0StoryNpc npc = i as L0StoryNpc;
            if (npc != null && npc.role == "Blacksmith" && progress != null && progress.talkedToBlacksmith)
                continue;

            Vector3 to = i.transform.position - eye;
            float d = to.magnitude;
            if (d > interactionDistance)
                continue;

            if (d < nearestDist) { nearestDist = d; nearest = i; }

            float dot = Vector3.Dot(fwd, to.normalized);
            if (dot > bestDot) { bestDot = dot; looked = i; }
        }

        currentTarget = looked != null ? looked : nearest;
    }

    private void UpdateLabels()
    {
        foreach (L0StoryLabel label in labels)
        {
            if (label != null)
                label.player = player;
        }
    }

    private void EnsureLevel0HudServices()
    {
        objectiveGuide = L0ObjectiveGuide.Ensure();

        L0HudCleaner cleaner = FindFirstObjectByType<L0HudCleaner>();
        if (cleaner == null)
        {
            GameObject obj = new GameObject("L0_HUD_CLEANER");
            cleaner = obj.AddComponent<L0HudCleaner>();
        }

        cleaner.Apply();
    }

    private void RefreshObjectiveGuide()
    {
        if (objectiveGuide == null)
            objectiveGuide = L0ObjectiveGuide.Ensure();

        if (objectiveGuide == null || progress == null)
            return;

        objectiveGuide.SetObjective(progress.GetCurrentCastleObjective(), GetCurrentObjectiveTarget());
    }

    private Transform GetCurrentObjectiveTarget()
    {
        if (progress == null)
            return null;

        if (!progress.talkedToElder) return FindNpcTarget("Elder");
        if (!progress.talkedToRefugee) return FindNpcTarget("Refugee");
        if (!progress.talkedToGuard) return FindNpcTarget("Guard");
        if (!progress.talkedToBlacksmith) return FindNpcTarget("Blacksmith");
        if (!progress.hasSword) return FindWeaponTarget(GameProgressManager.WeaponId.Sword);
        if (!progress.hasSpear) return FindWeaponTarget(GameProgressManager.WeaponId.Spear);
        if (!progress.hasBow) return FindWeaponTarget(GameProgressManager.WeaponId.Bow);
        if (!progress.hasKingNote) return noteObject != null ? noteObject.transform : null;

        CastleIntroGate gate = FindFirstObjectByType<CastleIntroGate>();
        if (progress.castleGateRaidCleared && !progress.castleGateOpened)
            return gate != null ? gate.transform : null;

        Level0OrcRaidManager raid = FindFirstObjectByType<Level0OrcRaidManager>();
        if (progress.castleGateRaidStarted && !progress.castleGateRaidCleared)
            return raid != null ? raid.transform : (gate != null ? gate.transform : null);

        Level0OrcVillageManager village = FindFirstObjectByType<Level0OrcVillageManager>();
        if (progress.castleGateOpened && !progress.orcVillageCleared)
            return village != null ? village.GetObjectiveTarget() : null;

        Level0ExitPortal exit = FindFirstObjectByType<Level0ExitPortal>();
        if (progress.level0ExitUnlocked)
            return exit != null ? exit.transform : null;

        return null;
    }

    private Transform FindNpcTarget(string role)
    {
        foreach (L0StoryInteractable interactable in interactables)
        {
            L0StoryNpc npc = interactable as L0StoryNpc;
            if (npc != null && npc.role == role && npc.gameObject.activeInHierarchy)
                return npc.transform;
        }

        return null;
    }

    private Transform FindWeaponTarget(GameProgressManager.WeaponId id)
    {
        foreach (L0StoryInteractable interactable in interactables)
        {
            L0StoryWeapon weapon = interactable as L0StoryWeapon;
            if (weapon != null && weapon.weaponId == id && weapon.gameObject.activeInHierarchy)
                return weapon.transform;
        }

        return null;
    }

    public void RegisterNpcTalk(string role, string speaker, string text)
    {
        if (!CanTalk(role))
        {
            ShowDialogue(speaker, GetBlockedText(role), 4.5f);
            return;
        }

        progress.RegisterNpcTalk(role);
        L0StoryAudio.Ensure().PlayTask();
        ShowDialogue(speaker, text, 8f);

        if (role == "Elder")
            ShowMessage("Теперь найди жительницу в деревне слева от дороги.", 5f);
        else if (role == "Refugee")
            ShowMessage("Вернись к раненому стражнику у ворот.", 5f);
        else if (role == "Guard")
            ShowMessage("Теперь найди кузнеца у правой стены замка.", 5f);
        else if (role == "Blacksmith")
            ShowMessage("Проверь меч, копьё и лук на стойке.", 5f);

        RefreshStoryObjects();
        RefreshObjectiveGuide();
    }

    private bool CanTalk(string role)
    {
        if (role == "Elder") return true;
        if (role == "Refugee") return progress.talkedToElder;
        if (role == "Guard") return progress.talkedToElder && progress.talkedToRefugee;
        if (role == "Blacksmith") return progress.talkedToElder && progress.talkedToRefugee && progress.talkedToGuard;
        return true;
    }

    private string GetBlockedText(string role)
    {
        if (role == "Refugee") return "Сначала поговори со старостой у ворот. Он объяснит, что произошло.";
        if (role == "Guard") return "Сначала выслушай жительницу. Она видела, куда ушли орки.";
        if (role == "Blacksmith") return "Сначала поговори со стражником у ворот.";
        return "Следуй текущей цели.";
    }

    public void RegisterWeaponPickup(GameProgressManager.WeaponId id, string text)
    {
        if (!progress.talkedToBlacksmith)
        {
            ShowMessage("Сначала поговори с кузнецом.", 4f);
            return;
        }

        progress.RegisterWeapon(id);
        L0StoryAudio.Ensure().PlayTask();
        ShowMessage(text, 4.5f);

        if (progress.HasAllWeapons())
            ShowDialogue("Кузнец", "Теперь ты вооружён. Возьми указ короля на столе у ворот.", 6f);

        RefreshStoryObjects();
        RefreshObjectiveGuide();
    }

    public void RegisterKingNotePickup(string text)
    {
        if (!progress.HasAllWeapons())
        {
            ShowMessage("Сначала проверь всё оружие.", 4f);
            return;
        }

        progress.RegisterKingNote();
        L0StoryAudio.Ensure().PlayTask();
        ShowDialogue("Указ Короля", text, 8f);

        ShowMessage("Пролог выполнен. Вернись к воротам — начинается тревога.", 7f);

        if (autoStartGateRaidAfterIntro)
        {
            Level0GameFlow flow = FindFirstObjectByType<Level0GameFlow>();
            if (flow != null)
                flow.StartGateRaid();
        }

        RefreshStoryObjects();
        RefreshObjectiveGuide();
    }

    public bool IsIntroCompleted()
    {
        return progress != null && progress.IsCastleIntroCompleted();
    }

    public void ShowMessage(string text, float duration)
    {
        message = text;
        messageUntil = Time.time + duration;
        Debug.Log("[CastlePrologueBuilder] " + text);
    }

    public void ShowDialogue(string speaker, string text, float duration)
    {
        dialogueSpeaker = speaker;
        dialogueText = text;
        dialogueUntil = Time.time + duration;
        Debug.Log("[Dialogue] " + speaker + ": " + text);
    }

    public void StartFireAwakening()
    {
        StartCoroutine(FireAwakeningSequence());
    }

    private IEnumerator FireAwakeningSequence()
    {
        ShowDialogue("Дух Короля", "Я чувствую... огонь пробуждается в моих руках!", 5f);

        GameObject playerObj = player != null ? player.gameObject : GameObject.FindWithTag("Player");
        GameObject aura = null;
        Light auraLight = null;

        if (playerObj != null)
        {
            aura = new GameObject("FireAura");
            aura.transform.SetParent(playerObj.transform, false);
            aura.transform.localPosition = Vector3.up * 1f;

            auraLight = aura.AddComponent<Light>();
            auraLight.type = LightType.Point;
            auraLight.color = new Color(1f, 0.5f, 0.08f);
            auraLight.range = 10f;
            auraLight.intensity = 0f;

            if (FeedbackManager.Instance != null)
            {
                Vector3 pos = playerObj.transform.position + Vector3.up * 1.2f;
                FeedbackManager.Instance.SpawnBurst(pos, new Color(1f, 0.6f, 0.1f), new Color(1f, 0.2f, 0f), 40, 6f, 0.25f);
                FeedbackManager.Instance.SpawnRing(pos, new Color(1f, 0.45f, 0.05f), 30, 3f);
            }
        }

        float t = 0f;
        while (t < 1.5f)
        {
            t += Time.deltaTime;
            if (auraLight != null)
                auraLight.intensity = Mathf.Lerp(0f, 4f, t / 1.5f);
            yield return null;
        }

        yield return new WaitForSeconds(1f);
        ShowDialogue("Дух Короля", "Убивай орков — копи заряд. Когда полоска полная, нажми [Q] — огненный шар!", 6f);
        GameManager.Instance?.ShowMessage("Новая способность: Огненный шар [Q]!", GameManager.MsgType.Default, 4f);

        yield return new WaitForSeconds(4f);

        t = 0f;
        while (t < 2f)
        {
            t += Time.deltaTime;
            if (auraLight != null)
                auraLight.intensity = Mathf.Lerp(4f, 0f, t / 2f);
            yield return null;
        }

        if (aura != null)
            Destroy(aura);
    }

    private GameObject CreatePrimitive(Transform parent, string name, PrimitiveType type, Vector3 localPos, Vector3 localScale, Color color)
    {
        GameObject obj = GameObject.CreatePrimitive(type);
        obj.name = name;
        obj.transform.SetParent(parent);
        obj.transform.localPosition = localPos;
        obj.transform.localRotation = Quaternion.identity;
        obj.transform.localScale = localScale;

        Collider col = obj.GetComponent<Collider>();
        if (col != null)
            Destroy(col);

        Renderer r = obj.GetComponent<Renderer>();
        if (r != null)
        {
            r.material = new Material(Shader.Find("Standard"));
            r.material.color = color;
        }

        return obj;
    }

    private void CreateLabel(Transform parent, string text, float height)
    {
        if (!createWorldLabels)
            return;

        GameObject obj = new GameObject("Label_" + text);
        obj.transform.SetParent(parent);
        obj.transform.localPosition = new Vector3(0, height, 0);

        TextMesh mesh = obj.AddComponent<TextMesh>();
        mesh.text = text;
        mesh.characterSize = 0.085f;
        mesh.anchor = TextAnchor.MiddleCenter;
        mesh.alignment = TextAlignment.Center;
        mesh.color = Color.white;

        L0StoryLabel label = obj.AddComponent<L0StoryLabel>();
        label.visibleDistance = labelVisibleDistance;
        labels.Add(label);
    }

    private void CreateCampfire(Vector3 pos)
    {
        GameObject obj = new GameObject("Story_Campfire");
        obj.transform.SetParent(root.transform);
        obj.transform.position = pos;
        CreatePrimitive(obj.transform, "Flame", PrimitiveType.Sphere, new Vector3(0, 0.35f, 0), new Vector3(0.35f, 0.45f, 0.35f), new Color(1f, 0.35f, 0.05f));
    }

    private void CreateTable(Vector3 pos)
    {
        GameObject obj = new GameObject("Story_Table");
        obj.transform.SetParent(root.transform);
        obj.transform.position = pos;
        CreatePrimitive(obj.transform, "Top", PrimitiveType.Cube, new Vector3(0, 0.65f, 0), new Vector3(1.4f, 0.10f, 0.8f), new Color(0.35f, 0.18f, 0.06f));
    }

    private void CreateWeaponRack(Vector3 pos)
    {
        GameObject obj = new GameObject("Story_WeaponRack");
        obj.transform.SetParent(root.transform);
        obj.transform.position = pos;

        Color wood   = new Color(0.30f, 0.16f, 0.06f);
        Color woodL  = new Color(0.44f, 0.26f, 0.10f);
        Color iron   = new Color(0.50f, 0.50f, 0.54f);
        Color gold   = new Color(0.80f, 0.65f, 0.12f);

        // Задняя стена (фон за оружием)
        CreatePrimitive(obj.transform, "RackBack",    PrimitiveType.Cube, new Vector3(0,    1.10f,  -0.10f), new Vector3(2.8f, 2.0f, 0.10f), new Color(0.22f, 0.14f, 0.05f));

        // Два вертикальных столба
        CreatePrimitive(obj.transform, "PostL",       PrimitiveType.Cube, new Vector3(-1.3f, 1.0f, 0f),    new Vector3(0.12f, 2.0f, 0.12f), wood);
        CreatePrimitive(obj.transform, "PostR",       PrimitiveType.Cube, new Vector3( 1.3f, 1.0f, 0f),    new Vector3(0.12f, 2.0f, 0.12f), wood);
        // Шапки столбов
        CreatePrimitive(obj.transform, "CapL",        PrimitiveType.Cube, new Vector3(-1.3f, 2.06f, 0f),   new Vector3(0.18f, 0.06f, 0.18f), woodL);
        CreatePrimitive(obj.transform, "CapR",        PrimitiveType.Cube, new Vector3( 1.3f, 2.06f, 0f),   new Vector3(0.18f, 0.06f, 0.18f), woodL);

        // Перекладины
        CreatePrimitive(obj.transform, "BeamTop",     PrimitiveType.Cube, new Vector3(0, 1.92f, 0f),       new Vector3(2.72f, 0.08f, 0.12f), woodL);
        CreatePrimitive(obj.transform, "BeamMid",     PrimitiveType.Cube, new Vector3(0, 1.30f, 0f),       new Vector3(2.72f, 0.07f, 0.10f), woodL);
        CreatePrimitive(obj.transform, "BeamBot",     PrimitiveType.Cube, new Vector3(0, 0.62f, 0f),       new Vector3(2.72f, 0.06f, 0.10f), woodL);

        // Крючки на средней перекладине
        CreatePrimitive(obj.transform, "Hook0",       PrimitiveType.Cube, new Vector3(-1.0f, 1.26f, 0.06f), new Vector3(0.04f, 0.08f, 0.06f), iron);
        CreatePrimitive(obj.transform, "Hook1",       PrimitiveType.Cube, new Vector3( 0.0f, 1.26f, 0.06f), new Vector3(0.04f, 0.08f, 0.06f), iron);
        CreatePrimitive(obj.transform, "Hook2",       PrimitiveType.Cube, new Vector3( 1.0f, 1.26f, 0.06f), new Vector3(0.04f, 0.08f, 0.06f), iron);

        // Золотой знак «К» (кузня) на фоне
        CreatePrimitive(obj.transform, "SignV",       PrimitiveType.Cube, new Vector3(0, 1.75f, -0.06f),   new Vector3(0.06f, 0.42f, 0.04f), gold);
        CreatePrimitive(obj.transform, "SignArmT",    PrimitiveType.Cube, new Vector3(0.14f, 1.90f, -0.06f), new Vector3(0.22f, 0.05f, 0.04f), gold);
        CreatePrimitive(obj.transform, "SignArmB",    PrimitiveType.Cube, new Vector3(0.14f, 1.60f, -0.06f), new Vector3(0.22f, 0.05f, 0.04f), gold);

        // Тёплый свет над стойкой
        GameObject lightGo = new GameObject("RackLight");
        lightGo.transform.SetParent(obj.transform);
        lightGo.transform.localPosition = new Vector3(0, 2.4f, 0.2f);
        Light lt = lightGo.AddComponent<Light>();
        lt.type = LightType.Point; lt.color = new Color(1f, 0.80f, 0.50f);
        lt.range = 5.0f; lt.intensity = 1.2f;
    }

    private void CreateAnvil(Vector3 pos)
    {
        GameObject obj = new GameObject("Story_Anvil");
        obj.transform.SetParent(root.transform);
        obj.transform.position = pos;
        CreatePrimitive(obj.transform, "AnvilTop", PrimitiveType.Cube, new Vector3(0, 0.60f, 0), new Vector3(0.82f, 0.16f, 0.30f), new Color(0.42f, 0.42f, 0.45f));
    }

    private void InitStyles()
    {
        if (panelStyle != null)
            return;

        panelStyle = new GUIStyle(GUI.skin.box);
        panelStyle.padding = new RectOffset(9, 9, 7, 7);

        titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.fontSize = 16;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.normal.textColor = new Color(1f, 0.82f, 0.25f);

        normalStyle = new GUIStyle(GUI.skin.label);
        normalStyle.fontSize = 12;
        normalStyle.wordWrap = true;
        normalStyle.normal.textColor = Color.white;

        doneStyle = new GUIStyle(normalStyle);
        doneStyle.normal.textColor = new Color(0.35f, 1f, 0.35f);

        mutedStyle = new GUIStyle(normalStyle);
        mutedStyle.normal.textColor = new Color(0.75f, 0.75f, 0.75f);

        dialogueNameStyle = new GUIStyle(GUI.skin.label);
        dialogueNameStyle.fontSize = 19;
        dialogueNameStyle.fontStyle = FontStyle.Bold;
        dialogueNameStyle.normal.textColor = new Color(1f, 0.82f, 0.25f);

        dialogueTextStyle = new GUIStyle(GUI.skin.label);
        dialogueTextStyle.fontSize = 15;
        dialogueTextStyle.wordWrap = true;
        dialogueTextStyle.normal.textColor = Color.white;

        promptStyle = new GUIStyle(GUI.skin.box);
        promptStyle.fontSize = 15;
        promptStyle.alignment = TextAnchor.MiddleCenter;
    }

    private void OnGUI()
    {
        InitStyles();

        if (progress == null)
            progress = GameProgressManager.Ensure();

        if (showLegacyTaskPanel)
            DrawHud();

        DrawPrompt();
        DrawMessage();
        DrawDialogue();
    }

    private void DrawHud()
    {
        float w = 380f;
        float h = Input.GetKey(KeyCode.Tab) ? 285f : 110f;

        GUILayout.BeginArea(new Rect(14, 14, w, h), panelStyle);
        GUILayout.Label("ПРОЛОГ: ЗАМОК", titleStyle);
        GUILayout.Label("Прогресс: " + progress.GetCastleTaskDoneCount() + "/" + progress.GetCastleTaskTotalCount(), normalStyle);
        GUILayout.Label("Цель: " + progress.GetCurrentCastleObjective(), normalStyle);

        if (!Input.GetKey(KeyCode.Tab))
        {
            GUILayout.Label("Tab — список задач", mutedStyle);
        }
        else
        {
            GUILayout.Space(4);
            Task(progress.talkedToElder, "Староста у ворот");
            Task(progress.talkedToRefugee, "Жительница в деревне");
            Task(progress.talkedToGuard, "Раненый стражник");
            Task(progress.talkedToBlacksmith, "Кузнец");
            Task(progress.hasSword, "Меч");
            Task(progress.hasSpear, "Копьё");
            Task(progress.hasBow, "Лук");
            Task(progress.hasKingNote, "Указ короля");
        }

        GUILayout.EndArea();
    }

    private void Task(bool done, string text)
    {
        GUILayout.Label((done ? "✓ " : "□ ") + text, done ? doneStyle : normalStyle);
    }

    private void DrawPrompt()
    {
        if (currentTarget == null)
            return;

        GUI.Box(new Rect(Screen.width / 2f - 210f, Screen.height - 76f, 420f, 36f), "F — " + currentTarget.displayName, promptStyle);
    }

    private void DrawMessage()
    {
        if (string.IsNullOrEmpty(message) || Time.time >= messageUntil)
            return;

        GUI.Box(new Rect(Screen.width / 2f - 330f, Screen.height - 130f, 660f, 38f), message);
    }

    private void DrawDialogue()
    {
        if (string.IsNullOrEmpty(dialogueText) || Time.time >= dialogueUntil)
            return;

        // R5: компактнее и ниже — не перекрывает центр обзора.
        float w = Mathf.Min(560f, Screen.width - 80f);
        float h = 110f;
        float x = Screen.width * 0.5f - w * 0.5f;
        float y = Screen.height - 168f;

        GUI.Box(new Rect(x, y, w, h), "");
        GUI.Label(new Rect(x + 16, y + 9, w - 32, 26), dialogueSpeaker, dialogueNameStyle);
        GUI.Label(new Rect(x + 16, y + 38, w - 32, 64), dialogueText, dialogueTextStyle);
    }
}

public abstract class L0StoryInteractable : MonoBehaviour
{
    public string displayName;
    public abstract void Interact(CastlePrologueBuilder builder);
}

public class L0StoryNpc : L0StoryInteractable
{
    public string role;
    [TextArea(2, 5)] public string dialogueText;

    public override void Interact(CastlePrologueBuilder builder)
    {
        builder.RegisterNpcTalk(role, displayName, dialogueText);
    }
}

public class L0StoryWeapon : L0StoryInteractable
{
    public GameProgressManager.WeaponId weaponId;
    public string pickupText;

    public override void Interact(CastlePrologueBuilder builder)
    {
        builder.RegisterWeaponPickup(weaponId, pickupText);
        gameObject.SetActive(false);
    }
}

public class L0StoryNote : L0StoryInteractable
{
    public string noteText;

    public override void Interact(CastlePrologueBuilder builder)
    {
        builder.RegisterKingNotePickup(noteText);
        gameObject.SetActive(false);
    }
}

public class L0StoryLabel : MonoBehaviour
{
    public Transform player;
    public float visibleDistance = 10f;

    private TextMesh textMesh;

    private void Awake()
    {
        textMesh = GetComponent<TextMesh>();
    }

    private void LateUpdate()
    {
        Camera cam = Camera.main;
        if (cam == null)
            return;

        transform.rotation = Quaternion.LookRotation(transform.position - cam.transform.position);

        if (textMesh == null)
            textMesh = GetComponent<TextMesh>();

        if (textMesh == null || player == null)
            return;

        float distance = Vector3.Distance(player.position, transform.position);
        float alpha = Mathf.Clamp01(1f - distance / visibleDistance);

        Color c = textMesh.color;
        c.a = alpha;
        textMesh.color = c;
    }
}

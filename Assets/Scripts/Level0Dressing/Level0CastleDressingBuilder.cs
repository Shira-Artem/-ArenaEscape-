using System.Collections;
using UnityEngine;

/// <summary>
/// Визуальный слой замковой зоны Level0 (Z > -35).
/// Создаёт пропсы двора, деревья/камни у ворот, дорожку, указатели, освещение.
/// Не трогает NPC-логику (CastlePrologueBuilder) и генератор замка (CastleGenerator).
/// </summary>
public class Level0CastleDressingBuilder : MonoBehaviour
{
    public bool buildOnStart = true;
    public bool clearBeforeBuild = true;

    [Header("Layer Separation")]
    public bool createNpcVisuals = false;
    public bool createProps = true;
    public bool createGateArea = true;
    public bool createPathway = true;
    public bool createNature = true;

    [Header("Exact Castle Coordinates")]
    public Vector3 elderSpot = new Vector3(-3.3f, 0.05f, -27f);
    public Vector3 refugeeSpot = new Vector3(3.3f, 0.05f, -27.2f);
    public Vector3 guardSpot = new Vector3(-3.8f, 0.05f, -19f);
    public Vector3 blacksmithSpot = new Vector3(5.5f, 0.6f, -23.8f);
    public Vector3 weaponRackSpot = new Vector3(7.4f, 0.05f, -25.2f);
    public Vector3 kingNoteTableSpot = new Vector3(0f, 0.05f, -24f);
    public Vector3 campfireSpot = new Vector3(2.2f, 0.05f, -29.4f);

    [Header("Gate & Road")]
    public Vector3 castleGatePoint = new Vector3(0f, 0f, -16.25f);
    public Vector3 roadStartPoint = new Vector3(0f, 0f, -34f);

    // Prefab paths (Pure Poly)
    private static readonly string Tree02 = "Assets/Pure Poly/Free Low Poly Nature Pack/Prefabs/PP_Tree_02.prefab";
    private static readonly string Tree10 = "Assets/Pure Poly/Free Low Poly Nature Pack/Prefabs/PP_Tree_10.prefab";
    private static readonly string Birch05 = "Assets/Pure Poly/Free Low Poly Nature Pack/Prefabs/PP_Birch_Tree_05.prefab";
    private static readonly string RockMoss09 = "Assets/Pure Poly/Free Low Poly Nature Pack/Prefabs/PP_Rock_Moss_Grown_09.prefab";
    private static readonly string RockMoss11 = "Assets/Pure Poly/Free Low Poly Nature Pack/Prefabs/PP_Rock_Moss_Grown_11.prefab";
    private static readonly string Grass11 = "Assets/Pure Poly/Free Low Poly Nature Pack/Prefabs/PP_Grass_11.prefab";

    private GameObject dressingContainer;

    private void Start()
    {
        if (buildOnStart)
            BuildDressing();
        // Тинтим замок через 0.1с — к этому моменту CastleGenerator уже собрал замок
        Invoke(nameof(TintCastle), 0.1f);
    }

    private void TintCastle()
    {
        CastleGenerator gen = FindObjectOfType<CastleGenerator>();
        if (gen == null) return;
        Color warmTint = new Color(1.0f, 0.91f, 0.78f); // тёплый песчаник
        Renderer[] renderers = gen.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            Material[] mats = renderers[i].materials;
            bool changed = false;
            for (int m = 0; m < mats.Length; m++)
            {
                if (mats[m] == null) continue;
                if (mats[m].HasProperty("_BaseColor"))
                { mats[m].SetColor("_BaseColor", mats[m].GetColor("_BaseColor") * warmTint); changed = true; }
                else if (mats[m].HasProperty("_Color"))
                { mats[m].color = mats[m].color * warmTint; changed = true; }
            }
            if (changed) renderers[i].materials = mats;
        }
    }

    [ContextMenu("Build Dressing")]
    public void BuildDressing()
    {
        if (clearBeforeBuild)
            ClearDressing();

        dressingContainer = new GameObject("Generated_Level0_Dressing");
        dressingContainer.transform.SetParent(transform, false);

        Transform root = dressingContainer.transform;

        if (createNpcVisuals)
        {
            CastleCharacterVisualFactory.CreateCharacter(CastleCharacterType.Elder, "Староста Алдус", elderSpot, Quaternion.Euler(0, 180, 0), root);
            CastleCharacterVisualFactory.CreateCharacter(CastleCharacterType.Blacksmith, "Кузнец Брок", blacksmithSpot, Quaternion.Euler(0, 90, 0), root);
            CastleCharacterVisualFactory.CreateCharacter(CastleCharacterType.InjuredGuard, "Раненый Олоф", guardSpot, Quaternion.Euler(0, -45, 0), root);
            CastleCharacterVisualFactory.CreateCharacter(CastleCharacterType.Refugee, "Беженка Марта", refugeeSpot, Quaternion.Euler(0, 135, 0), root);
            CastleCharacterVisualFactory.CreateCharacter(CastleCharacterType.Villager, "Перепуганный житель", campfireSpot + new Vector3(1.2f, 0, 0.8f), Quaternion.identity, root);
        }

        // Зона кузнеца — всегда, независимо от флагов
        L0BlacksmithArea.Build(root);

        if (createProps)
            BuildCourtyardProps(root);

        if (createGateArea)
            BuildGateArea(root);

        if (createPathway)
            BuildPathway(root);

        if (createNature)
            BuildNature(root);

        Debug.Log("[Level0CastleDressingBuilder] Визуальный слой замка обновлён (шаг 3).");
    }

    private void BuildCourtyardProps(Transform root)
    {
        CastlePropFactory.CreateCampfire(campfireSpot, root);
        CastlePropFactory.CreateWeaponRack(weaponRackSpot, root);
        // Наковальня теперь строится в L0BlacksmithArea.Build()
        CastlePropFactory.CreateKingNoteTable(kingNoteTableSpot, root);
        CastlePropFactory.CreateCratesAndBags(refugeeSpot + new Vector3(-0.8f, 0, 0.4f), root);

        CastlePropFactory.CreateTorch(elderSpot + new Vector3(-1.2f, 0, 0.8f), root);
        CastlePropFactory.CreateTorch(guardSpot + new Vector3(0.8f, 0, -0.8f), root);

        // Лавка у старосты — место для разговора
        CastlePropFactory.CreateBench(elderSpot + new Vector3(1.5f, 0, -0.3f), 160f, root);

        // Бочки у беженцев (бочки у кузнеца теперь в L0BlacksmithArea)
        CastlePropFactory.CreateBarrel(refugeeSpot + new Vector3(0.8f, 0, -0.5f), root);

        // Ящики у стражника
        CastlePropFactory.CreateCratesAndBags(guardSpot + new Vector3(1.2f, 0, 0.5f), root);
    }

    private void BuildGateArea(Transform root)
    {
        // Два больших факела у ворот замка — встречают игрока
        CastlePropFactory.CreateGateTorch(castleGatePoint + new Vector3(-2.5f, 0, 0.5f), root, 2.5f);
        CastlePropFactory.CreateGateTorch(castleGatePoint + new Vector3(2.5f, 0, 0.5f), root, 2.5f);

        // Ещё два факела чуть дальше — обрамляют выход
        CastlePropFactory.CreateGateTorch(castleGatePoint + new Vector3(-2.0f, 0, -3f), root, 2.0f);
        CastlePropFactory.CreateGateTorch(castleGatePoint + new Vector3(2.0f, 0, -3f), root, 2.0f);

        // Тёплый свет у ворот — создаёт уютную атмосферу старта
        CastlePropFactory.CreateWarmGateLight(castleGatePoint + new Vector3(0, 3.5f, -1f), root);

        // Указатель "К деревне" сразу после выхода из ворот
        Vector3 signPos = castleGatePoint + new Vector3(3.0f, 0, -5f);
        Vector3 toVillage = (new Vector3(-24f, 0, -45f) - signPos).normalized;
        CastlePropFactory.CreateDirectionSign(signPos, toVillage, "К деревне →", root);

        // Указатель к дороге (прямо)
        Vector3 signPos2 = castleGatePoint + new Vector3(-2.8f, 0, -8f);
        Vector3 toRoad = (roadStartPoint - signPos2).normalized;
        CastlePropFactory.CreateQuestArrow(signPos2, toRoad, "На дорогу ↓", root);
    }

    private void BuildPathway(Transform root)
    {
        // Каменная дорожка от ворот к площади двора (Z от -17 до -26)
        float startZ = castleGatePoint.z - 1f;
        float endZ = kingNoteTableSpot.z;
        int stoneCount = 12;

        for (int i = 0; i < stoneCount; i++)
        {
            float t = (float)i / (stoneCount - 1);
            float z = Mathf.Lerp(startZ, endZ, t);
            float x = Mathf.Sin(t * Mathf.PI * 0.5f) * 0.3f;
            float sizeVar = Random.Range(0.8f, 1.2f);
            CastlePropFactory.CreatePathStone(new Vector3(x, 0.02f, z), root, sizeVar);

            // Второй камень рядом для ширины дорожки
            if (i % 2 == 0)
            {
                CastlePropFactory.CreatePathStone(
                    new Vector3(x + Random.Range(0.5f, 0.8f), 0.02f, z + Random.Range(-0.2f, 0.2f)),
                    root, sizeVar * 0.9f);
            }
        }

        // Травяные вставки по бокам дорожки
        for (int i = 0; i < 6; i++)
        {
            float t = (float)i / 5f;
            float z = Mathf.Lerp(startZ, endZ, t);
            CastlePropFactory.CreateGrassClump(new Vector3(-1.2f + Random.Range(-0.3f, 0.3f), 0, z), root);
            CastlePropFactory.CreateGrassClump(new Vector3(1.2f + Random.Range(-0.3f, 0.3f), 0, z), root);
        }
    }

    private void BuildNature(Transform root)
    {
        // Деревья вдоль пути от ворот к дороге — создают "аллею"
        PlacePrefabTree(Tree02, castleGatePoint + new Vector3(-6f, 0, -2f), 1.1f, root, "Tree_GateL1");
        PlacePrefabTree(Tree10, castleGatePoint + new Vector3(6.5f, 0, -4f), 1.0f, root, "Tree_GateR1");
        PlacePrefabTree(Birch05, castleGatePoint + new Vector3(-5f, 0, -10f), 0.9f, root, "Birch_PathL1");
        PlacePrefabTree(Tree02, castleGatePoint + new Vector3(5.5f, 0, -12f), 1.2f, root, "Tree_PathR1");

        // Деревья у двора
        PlacePrefabTree(Tree10, elderSpot + new Vector3(-4f, 0, 1f), 0.85f, root, "Tree_ElderNear");
        PlacePrefabTree(Birch05, refugeeSpot + new Vector3(3f, 0, -1f), 0.95f, root, "Birch_RefugeeNear");

        // Камни по бокам дороги от замка
        PlacePrefabRock(RockMoss09, castleGatePoint + new Vector3(-4f, 0, -6f), 0.7f, root, "Rock_GateL1");
        PlacePrefabRock(RockMoss11, castleGatePoint + new Vector3(4.5f, 0, -8f), 0.6f, root, "Rock_GateR1");
        PlacePrefabRock(RockMoss09, castleGatePoint + new Vector3(-3.5f, 0, -14f), 0.5f, root, "Rock_PathL2");
        PlacePrefabRock(RockMoss11, castleGatePoint + new Vector3(3.8f, 0, -16f), 0.55f, root, "Rock_PathR2");

        // Камни у ворот — обрамляют вход
        PlacePrefabRock(RockMoss11, castleGatePoint + new Vector3(-3.2f, 0, 1f), 0.45f, root, "Rock_EntrL");
        PlacePrefabRock(RockMoss09, castleGatePoint + new Vector3(3.5f, 0, 0.8f), 0.4f, root, "Rock_EntrR");

        // Трава — патчи у дороги
        PlacePrefabGrass(Grass11, castleGatePoint + new Vector3(-3f, 0, -4f), 0.8f, root, "Grass_GL1");
        PlacePrefabGrass(Grass11, castleGatePoint + new Vector3(3.5f, 0, -6f), 0.7f, root, "Grass_GR1");
        PlacePrefabGrass(Grass11, castleGatePoint + new Vector3(-4f, 0, -11f), 0.9f, root, "Grass_PL1");
        PlacePrefabGrass(Grass11, castleGatePoint + new Vector3(4f, 0, -13f), 0.85f, root, "Grass_PR1");

        // Цветы у двора — контраст с орочьей зоной дальше
        CastlePropFactory.CreateFlowerCluster(elderSpot + new Vector3(2.5f, 0, 0.5f), root, new Color(1f, 0.85f, 0.2f));
        CastlePropFactory.CreateFlowerCluster(kingNoteTableSpot + new Vector3(-2f, 0, 0.8f), root, new Color(0.9f, 0.3f, 0.35f));
        CastlePropFactory.CreateFlowerCluster(campfireSpot + new Vector3(-1.5f, 0, 0.5f), root, new Color(0.85f, 0.75f, 1f));
    }

    // --- Загрузка и установка prefab'ов ---

    private void PlacePrefabTree(string path, Vector3 pos, float scale, Transform parent, string name)
    {
        GameObject prefab = L0Util.LoadPrefab(path);
        if (prefab == null)
        {
            L0Util.CreateFallbackTree(pos, scale, parent, name);
            return;
        }
        GameObject obj = Instantiate(prefab, pos, Quaternion.Euler(0, Random.Range(0f, 360f), 0), parent);
        obj.name = name;
        obj.transform.localScale = Vector3.one * scale;
        L0Util.FixMaterials(obj);
        L0Util.TintTree(obj);
        // Коллайдеры оставляем — деревья блокируют ходьбу
    }

    private void PlacePrefabRock(string path, Vector3 pos, float scale, Transform parent, string name)
    {
        GameObject prefab = L0Util.LoadPrefab(path);
        if (prefab == null)
        {
            CreateFallbackRock(pos, scale, parent, name);
            return;
        }
        GameObject obj = Instantiate(prefab, pos, Quaternion.Euler(0, Random.Range(0f, 360f), 0), parent);
        obj.name = name;
        obj.transform.localScale = Vector3.one * scale;
        L0Util.FixMaterials(obj);
        L0Util.TintMaterials(obj, new Color(0.78f, 0.86f, 0.62f));
        // Коллайдеры оставляем — камни блокируют ходьбу
    }

    private void PlacePrefabGrass(string path, Vector3 pos, float scale, Transform parent, string name)
    {
        GameObject prefab = L0Util.LoadPrefab(path);
        if (prefab == null) return;
        GameObject obj = Instantiate(prefab, pos, Quaternion.Euler(0, Random.Range(0f, 360f), 0), parent);
        obj.name = name;
        obj.transform.localScale = Vector3.one * scale;
        L0Util.FixMaterials(obj);
        L0Util.RemoveAllColliders(obj);
    }

    private void CreateFallbackRock(Vector3 pos, float scale, Transform parent, string name)
    {
        GameObject rock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        rock.name = name;
        rock.transform.SetParent(parent);
        rock.transform.position = pos;
        rock.transform.localScale = new Vector3(1.2f * scale, 0.7f * scale, 1f * scale);
        rock.GetComponent<Renderer>().material = L0Util.CreateSimpleMat(new Color(0.4f, 0.42f, 0.38f));
        L0Util.RemoveColliderSingle(rock);
    }

    [ContextMenu("Clear Dressing")]
    public void ClearDressing()
    {
        if (dressingContainer != null)
        {
            L0Util.SafeDestroy(dressingContainer);
            dressingContainer = null;
        }

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            if (child != null && child.name == "Generated_Level0_Dressing")
                L0Util.SafeDestroy(child.gameObject);
        }
    }
}

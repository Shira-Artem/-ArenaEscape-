using UnityEngine;

/// <summary>
/// Осадный лагерь орков для Level0.
/// Полукруг палаток лицом к замку, осадные орудия, V-частокол, тотем, клетка, костры с партиклами.
/// </summary>
public static class Level0OrcSiegeCampDressing
{
    private static readonly string SmokePrefabPath = "Assets/msVFX_Free Smoke Effects Pack/Prefabs/msVFX_Stylized Smoke 2.prefab";
    private static readonly string RockPrefabPath = "Assets/Pure Poly/Free Low Poly Nature Pack/Prefabs/PP_Rock_Moss_Grown_11.prefab";

    public static void Build(GameObject container, Vector3 center, int tentCount)
    {
        Transform root = container.transform;

        BuildGround(root, center);
        BuildTents(root, center);
        BuildCentralCampfire(root, center);
        BuildBatteringRam(root, center);
        BuildCatapult(root, center);
        BuildPalisade(root, center);
        BuildPrisonerCage(root, center);
        BuildTotemPole(root, center);
        BuildWatchTower(root, center);
        BuildWeaponRack(root, center);
        BuildSupplies(root, center);
        BuildBanners(root, center);
        BuildAtmosphere(root, center);
    }

    // ═══════════════════ ЗЕМЛЯ ЛАГЕРЯ ═══════════════════

    private static void BuildGround(Transform root, Vector3 c)
    {
        Color dirt = new Color(0.22f, 0.16f, 0.10f);
        Color darkDirt = new Color(0.18f, 0.12f, 0.07f);

        WorldDressingPropFactory.CreateSimpleProp(root, "CampGround", PrimitiveType.Cylinder,
            c + new Vector3(0f, 0.01f, 0f), new Vector3(18f, 0.02f, 18f), dirt, false);
        WorldDressingPropFactory.CreateSimpleProp(root, "CampGroundInner", PrimitiveType.Cylinder,
            c + new Vector3(0f, 0.02f, 0f), new Vector3(10f, 0.02f, 10f), darkDirt, false);
    }

    // ═══════════════════ 5 ПАЛАТОК ПОЛУКРУГОМ ═══════════════════

    private static void BuildTents(Transform root, Vector3 c)
    {
        // Полукруг, открытая сторона — тыл (от замка), фасады смотрят на замок (к +Z)
        float radius = 7f;
        float[] angles = { -60f, -30f, 0f, 30f, 60f };
        float[] scales = { 0.85f, 0.9f, 1.3f, 0.9f, 0.85f };
        string[] names = { "TentSmall_L", "TentMid_L", "CommandTent", "TentMid_R", "TentSmall_R" };

        for (int i = 0; i < 5; i++)
        {
            float rad = angles[i] * Mathf.Deg2Rad;
            Vector3 pos = c + new Vector3(Mathf.Sin(rad) * radius, 0f, Mathf.Cos(rad) * radius);
            float facingAngle = Mathf.Atan2(c.x - pos.x, c.z - pos.z) * Mathf.Rad2Deg + 180f;
            WorldDressingPropFactory.CreateTent(pos, Quaternion.Euler(0f, facingAngle, 0f), scales[i], root);

            // Малый костёр у каждой палатки
            Vector3 fireOffset = new Vector3(Mathf.Sin(rad) * (radius - 2f), 0f, Mathf.Cos(rad) * (radius - 2f));
            CreateSmallFire(root, c + fireOffset, names[i] + "_fire");
        }

        // Красное знамя на командирской палатке
        WorldDressingPropFactory.CreateOrcBanner(
            c + new Vector3(0f, 0f, radius),
            Quaternion.Euler(0f, 180f, 0f), root);
    }

    // ═══════════════════ ЦЕНТРАЛЬНЫЙ КОСТЁР ═══════════════════

    private static void BuildCentralCampfire(Transform root, Vector3 c)
    {
        WorldDressingPropFactory.CreateCampfire(c, root);
        L0Props.CreatePointLight("SiegeCampFire", c + Vector3.up * 0.8f,
            new Color(1f, 0.45f, 0.1f), 2f, 14f, root);

        // Каменное кольцо
        for (int i = 0; i < 8; i++)
        {
            float angle = i * 45f * Mathf.Deg2Rad;
            Vector3 stonePos = c + new Vector3(Mathf.Cos(angle) * 1.0f, 0.08f, Mathf.Sin(angle) * 1.0f);
            WorldDressingPropFactory.CreateSimpleProp(root, "FireStone_" + i, PrimitiveType.Sphere,
                stonePos, new Vector3(0.2f, 0.15f, 0.2f), new Color(0.4f, 0.38f, 0.35f), false);
        }

        // Брёвна-сиденья
        Vector3[] seatPositions = {
            c + new Vector3(2f, 0.12f, 0.5f),
            c + new Vector3(-1.8f, 0.12f, 1.2f),
            c + new Vector3(0.5f, 0.12f, -2f),
            c + new Vector3(-1.5f, 0.12f, -1.5f),
        };
        float[] seatAngles = { 20f, 70f, -15f, 120f };
        for (int i = 0; i < seatPositions.Length; i++)
        {
            GameObject seat = WorldDressingPropFactory.CreateSimpleProp(root, "SeatLog_" + i, PrimitiveType.Cylinder,
                seatPositions[i], new Vector3(0.18f, 0.6f, 0.18f), new Color(0.25f, 0.15f, 0.08f), false);
            seat.transform.rotation = Quaternion.Euler(90f, seatAngles[i], 0f);
        }

        // Партиклы искр от центрального костра
        CreateFireParticles(root, c + Vector3.up * 0.3f);

        // Дым
        TryInstantiateSmoke(root, c + Vector3.up * 1.5f, 0.6f);
    }

    // ═══════════════════ ТАРАН ═══════════════════

    private static void BuildBatteringRam(Transform root, Vector3 c)
    {
        Vector3 ramPos = c + new Vector3(-5f, 0f, 5f);
        GameObject ram = new GameObject("BatteringRam");
        ram.transform.SetParent(root, true);
        ram.transform.position = ramPos;
        ram.transform.rotation = Quaternion.Euler(0f, 180f, 0f); // к замку

        Color wood = new Color(0.25f, 0.15f, 0.08f);
        Color iron = new Color(0.35f, 0.35f, 0.35f);

        // Бревно-таран
        WorldDressingPropFactory.CreateSimpleProp(ram.transform, "RamLog", PrimitiveType.Cylinder,
            new Vector3(0f, 1.2f, 0f), new Vector3(0.3f, 2.5f, 0.3f), wood, false)
            .transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

        // Железный наконечник
        WorldDressingPropFactory.CreateSimpleProp(ram.transform, "RamHead", PrimitiveType.Sphere,
            new Vector3(0f, 1.2f, 2.5f), new Vector3(0.5f, 0.5f, 0.7f), iron, false);

        // 4 ноги
        float legH = 1.0f;
        Vector3[] legs = { new Vector3(-0.6f, 0, -0.8f), new Vector3(0.6f, 0, -0.8f),
                           new Vector3(-0.6f, 0, 0.8f), new Vector3(0.6f, 0, 0.8f) };
        for (int i = 0; i < legs.Length; i++)
        {
            WorldDressingPropFactory.CreateSimpleProp(ram.transform, "RamLeg_" + i, PrimitiveType.Cylinder,
                legs[i] + Vector3.up * (legH * 0.5f), new Vector3(0.1f, legH, 0.1f), wood, false);
        }

        // Платформа (перекладина)
        WorldDressingPropFactory.CreateSimpleProp(ram.transform, "RamBeam", PrimitiveType.Cube,
            new Vector3(0f, 1.5f, 0f), new Vector3(1.4f, 0.1f, 0.1f), wood, false);
    }

    // ═══════════════════ КАТАПУЛЬТА ═══════════════════

    private static void BuildCatapult(Transform root, Vector3 c)
    {
        Vector3 catPos = c + new Vector3(5f, 0f, 6f);
        GameObject cat = new GameObject("Catapult");
        cat.transform.SetParent(root, true);
        cat.transform.position = catPos;
        cat.transform.rotation = Quaternion.Euler(0f, 170f, 0f);

        Color wood = new Color(0.28f, 0.18f, 0.09f);

        // Рама
        WorldDressingPropFactory.CreateSimpleProp(cat.transform, "Frame", PrimitiveType.Cube,
            new Vector3(0f, 0.3f, 0f), new Vector3(2.5f, 0.6f, 1.5f), wood, false);

        // Рычаг
        GameObject lever = WorldDressingPropFactory.CreateSimpleProp(cat.transform, "Lever", PrimitiveType.Cylinder,
            new Vector3(0f, 1.2f, 0.5f), new Vector3(0.12f, 1.8f, 0.12f), wood, false);
        lever.transform.localRotation = Quaternion.Euler(40f, 0f, 0f);

        // Чаша
        WorldDressingPropFactory.CreateSimpleProp(cat.transform, "Bowl", PrimitiveType.Cube,
            new Vector3(0f, 2.2f, 1.5f), new Vector3(0.6f, 0.15f, 0.6f), wood, false);

        // Колёса
        WorldDressingPropFactory.CreateSimpleProp(cat.transform, "WheelL", PrimitiveType.Cylinder,
            new Vector3(-1.3f, 0.25f, 0f), new Vector3(0.5f, 0.08f, 0.5f), new Color(0.2f, 0.12f, 0.06f), false);
        WorldDressingPropFactory.CreateSimpleProp(cat.transform, "WheelR", PrimitiveType.Cylinder,
            new Vector3(1.3f, 0.25f, 0f), new Vector3(0.5f, 0.08f, 0.5f), new Color(0.2f, 0.12f, 0.06f), false);

        // Камни рядом (снаряды)
        WorldDressingPropFactory.CreateSimpleProp(root, "CatStone1", PrimitiveType.Sphere,
            catPos + new Vector3(-1.5f, 0.2f, -0.5f), new Vector3(0.4f, 0.35f, 0.4f), new Color(0.45f, 0.42f, 0.38f), false);
        WorldDressingPropFactory.CreateSimpleProp(root, "CatStone2", PrimitiveType.Sphere,
            catPos + new Vector3(-1.2f, 0.18f, -1f), new Vector3(0.35f, 0.3f, 0.35f), new Color(0.42f, 0.4f, 0.36f), false);
    }

    // ═══════════════════ V-ЧАСТОКОЛ ═══════════════════

    private static void BuildPalisade(Transform root, Vector3 c)
    {
        // V-форма, остриё к замку (+Z), открытый тыл (-Z)
        float halfAngle = 60f;
        float armLength = 12f;
        float stakeSpacing = 0.5f;
        int stakesPerArm = Mathf.RoundToInt(armLength / stakeSpacing);

        Color stake = new Color(0.22f, 0.14f, 0.07f);

        for (int arm = 0; arm < 2; arm++)
        {
            float sign = arm == 0 ? -1f : 1f;
            float angleRad = (90f + sign * halfAngle) * Mathf.Deg2Rad;
            Vector3 dir = new Vector3(Mathf.Cos(angleRad), 0f, Mathf.Sin(angleRad));
            Vector3 tip = c + new Vector3(0f, 0f, 10f);

            for (int i = 0; i < stakesPerArm; i++)
            {
                Vector3 pos = tip + dir * (i * stakeSpacing);

                // Высокие колья
                WorldDressingPropFactory.CreateSimpleProp(root, "PStake_" + arm + "_" + i, PrimitiveType.Cylinder,
                    pos + Vector3.up * 1.2f, new Vector3(0.07f, 2.4f, 0.07f), stake, false);

                // Заострённый верх
                WorldDressingPropFactory.CreateSimpleProp(root, "PStakeTip_" + arm + "_" + i, PrimitiveType.Cube,
                    pos + Vector3.up * 2.4f, new Vector3(0.1f, 0.2f, 0.1f), stake, false)
                    .transform.rotation = Quaternion.Euler(45f, 0f, 0f);

                // Череп на каждом 6м коле
                if (i > 0 && i % 6 == 0)
                {
                    WorldDressingPropFactory.CreateSimpleProp(root, "PSkull_" + arm + "_" + i, PrimitiveType.Sphere,
                        pos + Vector3.up * 2.6f, new Vector3(0.12f, 0.1f, 0.12f), new Color(0.7f, 0.6f, 0.5f), false);
                }
            }
        }

        // Поперечные перекладины
        for (int arm = 0; arm < 2; arm++)
        {
            float sign = arm == 0 ? -1f : 1f;
            float angleRad = (90f + sign * halfAngle) * Mathf.Deg2Rad;
            Vector3 dir = new Vector3(Mathf.Cos(angleRad), 0f, Mathf.Sin(angleRad));
            Vector3 tip = c + new Vector3(0f, 0f, 10f);

            for (int i = 0; i < stakesPerArm - 1; i += 3)
            {
                Vector3 pos = tip + dir * ((i + 1.5f) * stakeSpacing);
                float railLen = stakeSpacing * 3f;
                GameObject rail = WorldDressingPropFactory.CreateSimpleProp(root, "PRail_" + arm + "_" + i, PrimitiveType.Cube,
                    pos + Vector3.up * 1.6f, new Vector3(0.05f, 0.05f, railLen), stake, false);
                rail.transform.rotation = Quaternion.LookRotation(dir);
            }
        }
    }

    // ═══════════════════ КЛЕТКА ДЛЯ ПЛЕННИКОВ ═══════════════════

    private static void BuildPrisonerCage(Transform root, Vector3 c)
    {
        Vector3 cagePos = c + new Vector3(-7f, 0f, -2f);
        CreateCage(root, cagePos);
    }

    public static void CreateCage(Transform root, Vector3 pos)
    {
        GameObject cage = new GameObject("PrisonerCage");
        cage.transform.SetParent(root, true);
        cage.transform.position = pos;

        Color iron = new Color(0.3f, 0.28f, 0.25f);
        float w = 1.5f, h = 2.2f, d = 1.5f;

        // Вертикальные прутья
        for (int x = 0; x < 3; x++)
            for (int z = 0; z < 3; z++)
            {
                if (x == 1 && z == 1) continue; // центр пустой
                float px = (x - 1) * (w * 0.5f);
                float pz = (z - 1) * (d * 0.5f);
                WorldDressingPropFactory.CreateSimpleProp(cage.transform, "Bar_" + x + z, PrimitiveType.Cylinder,
                    new Vector3(px, h * 0.5f, pz), new Vector3(0.04f, h, 0.04f), iron, false);
            }

        // Горизонтальные перекладины (верх и низ)
        WorldDressingPropFactory.CreateSimpleProp(cage.transform, "TopFrame", PrimitiveType.Cube,
            Vector3.up * h, new Vector3(w, 0.04f, d), iron, false);
        WorldDressingPropFactory.CreateSimpleProp(cage.transform, "BottomFrame", PrimitiveType.Cube,
            Vector3.up * 0.02f, new Vector3(w, 0.04f, d), iron, false);
    }

    // ═══════════════════ ТОТЕМНЫЙ СТОЛБ ═══════════════════

    private static void BuildTotemPole(Transform root, Vector3 c)
    {
        Vector3 totemPos = c + new Vector3(0f, 0f, 3f);
        GameObject totem = new GameObject("TotemPole");
        totem.transform.SetParent(root, true);
        totem.transform.position = totemPos;

        Color wood = new Color(0.2f, 0.12f, 0.06f);
        Color red = new Color(0.5f, 0.05f, 0.02f);
        Color bone = new Color(0.7f, 0.6f, 0.5f);

        // Столб
        WorldDressingPropFactory.CreateSimpleProp(totem.transform, "Pole", PrimitiveType.Cylinder,
            Vector3.up * 2f, new Vector3(0.18f, 4f, 0.18f), wood, false);

        // Черепа
        WorldDressingPropFactory.CreateSimpleProp(totem.transform, "Skull1", PrimitiveType.Sphere,
            new Vector3(0.15f, 3.5f, 0f), new Vector3(0.18f, 0.15f, 0.18f), bone, false);
        WorldDressingPropFactory.CreateSimpleProp(totem.transform, "Skull2", PrimitiveType.Sphere,
            new Vector3(-0.12f, 2.8f, 0.1f), new Vector3(0.16f, 0.13f, 0.16f), bone, false);
        WorldDressingPropFactory.CreateSimpleProp(totem.transform, "Skull3", PrimitiveType.Sphere,
            new Vector3(0.1f, 2.2f, -0.12f), new Vector3(0.14f, 0.12f, 0.14f), bone, false);

        // Тряпки
        WorldDressingPropFactory.CreateSimpleProp(totem.transform, "Cloth1", PrimitiveType.Cube,
            new Vector3(0.2f, 3.2f, 0f), new Vector3(0.5f, 0.3f, 0.03f), red, false)
            .transform.localRotation = Quaternion.Euler(0f, 0f, -20f);
        WorldDressingPropFactory.CreateSimpleProp(totem.transform, "Cloth2", PrimitiveType.Cube,
            new Vector3(-0.15f, 2.5f, 0.1f), new Vector3(0.4f, 0.25f, 0.03f), red, false)
            .transform.localRotation = Quaternion.Euler(5f, 30f, 15f);

        // Свечение
        L0Props.CreatePointLight("TotemGlow", totemPos + Vector3.up * 3f,
            new Color(0.8f, 0.2f, 0.05f), 0.8f, 6f, root);
    }

    // ═══════════════════ ДОЗОРНАЯ ВЫШКА ═══════════════════

    private static void BuildWatchTower(Transform root, Vector3 c)
    {
        Vector3 towerPos = c + new Vector3(3f, 0f, 9f);
        GameObject tower = new GameObject("WatchTower");
        tower.transform.SetParent(root, true);
        tower.transform.position = towerPos;

        Color wood = new Color(0.25f, 0.15f, 0.08f);
        float h = 5.5f;

        // 4 ноги
        float legSpacing = 0.8f;
        Vector3[] legOffsets = {
            new Vector3(-legSpacing, 0, -legSpacing), new Vector3(legSpacing, 0, -legSpacing),
            new Vector3(-legSpacing, 0, legSpacing), new Vector3(legSpacing, 0, legSpacing)
        };
        for (int i = 0; i < 4; i++)
        {
            WorldDressingPropFactory.CreateSimpleProp(tower.transform, "TowerLeg_" + i, PrimitiveType.Cylinder,
                legOffsets[i] + Vector3.up * (h * 0.5f), new Vector3(0.12f, h, 0.12f), wood, false);
        }

        // Площадка
        WorldDressingPropFactory.CreateSimpleProp(tower.transform, "TowerPlatform", PrimitiveType.Cube,
            Vector3.up * h, new Vector3(2.2f, 0.1f, 2.2f), wood, false);

        // Перила
        WorldDressingPropFactory.CreateSimpleProp(tower.transform, "TowerRail_F", PrimitiveType.Cube,
            new Vector3(0f, h + 0.5f, 1.1f), new Vector3(2.2f, 0.06f, 0.06f), wood, false);
        WorldDressingPropFactory.CreateSimpleProp(tower.transform, "TowerRail_B", PrimitiveType.Cube,
            new Vector3(0f, h + 0.5f, -1.1f), new Vector3(2.2f, 0.06f, 0.06f), wood, false);
        WorldDressingPropFactory.CreateSimpleProp(tower.transform, "TowerRail_L", PrimitiveType.Cube,
            new Vector3(-1.1f, h + 0.5f, 0f), new Vector3(0.06f, 0.06f, 2.2f), wood, false);
        WorldDressingPropFactory.CreateSimpleProp(tower.transform, "TowerRail_R", PrimitiveType.Cube,
            new Vector3(1.1f, h + 0.5f, 0f), new Vector3(0.06f, 0.06f, 2.2f), wood, false);

        // Факел на вышке
        WorldDressingPropFactory.CreateTorch(towerPos + new Vector3(0f, h + 0.2f, 1.2f), root);
    }

    // ═══════════════════ СТОЙКА С ОРУЖИЕМ ═══════════════════

    private static void BuildWeaponRack(Transform root, Vector3 c)
    {
        Vector3 rackPos = c + new Vector3(7f, 0f, 0f);
        GameObject rack = new GameObject("WeaponRack");
        rack.transform.SetParent(root, true);
        rack.transform.position = rackPos;

        Color wood = new Color(0.28f, 0.18f, 0.09f);
        Color metal = new Color(0.45f, 0.45f, 0.45f);

        // Рама
        WorldDressingPropFactory.CreateSimpleProp(rack.transform, "RackPostL", PrimitiveType.Cylinder,
            new Vector3(-0.5f, 0.7f, 0f), new Vector3(0.06f, 1.4f, 0.06f), wood, false);
        WorldDressingPropFactory.CreateSimpleProp(rack.transform, "RackPostR", PrimitiveType.Cylinder,
            new Vector3(0.5f, 0.7f, 0f), new Vector3(0.06f, 1.4f, 0.06f), wood, false);
        WorldDressingPropFactory.CreateSimpleProp(rack.transform, "RackBar", PrimitiveType.Cube,
            new Vector3(0f, 1.2f, 0f), new Vector3(1.2f, 0.05f, 0.05f), wood, false);

        // Копья (прислонены)
        for (int i = 0; i < 3; i++)
        {
            GameObject spear = WorldDressingPropFactory.CreateSimpleProp(rack.transform, "RackSpear_" + i, PrimitiveType.Cylinder,
                new Vector3(-0.3f + i * 0.3f, 0.8f, 0.08f), new Vector3(0.03f, 1.5f, 0.03f), wood, false);
            spear.transform.localRotation = Quaternion.Euler(5f, 0f, 3f - i * 3f);

            WorldDressingPropFactory.CreateSimpleProp(rack.transform, "RackSpearHead_" + i, PrimitiveType.Cube,
                new Vector3(-0.3f + i * 0.3f, 1.55f, 0.08f), new Vector3(0.06f, 0.12f, 0.03f), metal, false);
        }

        // Топоры
        for (int i = 0; i < 2; i++)
        {
            float x = -0.15f + i * 0.3f;
            WorldDressingPropFactory.CreateSimpleProp(rack.transform, "RackAxeHandle_" + i, PrimitiveType.Cylinder,
                new Vector3(x, 0.5f, -0.08f), new Vector3(0.03f, 0.8f, 0.03f), wood, false)
                .transform.localRotation = Quaternion.Euler(0f, 0f, -10f + i * 20f);
            WorldDressingPropFactory.CreateSimpleProp(rack.transform, "RackAxeHead_" + i, PrimitiveType.Cube,
                new Vector3(x - 0.08f + i * 0.16f, 0.9f, -0.08f), new Vector3(0.15f, 0.1f, 0.03f), metal, false);
        }
    }

    // ═══════════════════ ПРИПАСЫ ═══════════════════

    private static void BuildSupplies(Transform root, Vector3 c)
    {
        // Бочки и ящики у командирской палатки
        Vector3 supplyBase = c + new Vector3(1.5f, 0f, 8f);

        WorldDressingPropFactory.CreateBarrel(supplyBase, root);
        WorldDressingPropFactory.CreateBarrel(supplyBase + new Vector3(0.8f, 0f, 0.3f), root);
        WorldDressingPropFactory.CreateBarrel(supplyBase + new Vector3(0.4f, 0f, -0.7f), root);

        // Ящики
        Color crate = new Color(0.3f, 0.2f, 0.1f);
        WorldDressingPropFactory.CreateSimpleProp(root, "Crate1", PrimitiveType.Cube,
            supplyBase + new Vector3(-0.6f, 0.2f, 0.2f), new Vector3(0.5f, 0.4f, 0.5f), crate, false);
        WorldDressingPropFactory.CreateSimpleProp(root, "Crate2", PrimitiveType.Cube,
            supplyBase + new Vector3(-0.5f, 0.55f, 0.3f), new Vector3(0.4f, 0.3f, 0.4f), crate, false)
            .transform.rotation = Quaternion.Euler(0f, 25f, 0f);

        // Мешки
        WorldDressingPropFactory.CreateSimpleProp(root, "Sack1", PrimitiveType.Sphere,
            supplyBase + new Vector3(1.2f, 0.12f, 0.8f), new Vector3(0.35f, 0.25f, 0.3f), new Color(0.55f, 0.45f, 0.3f), false);
        WorldDressingPropFactory.CreateSimpleProp(root, "Sack2", PrimitiveType.Sphere,
            supplyBase + new Vector3(1.5f, 0.1f, 0.3f), new Vector3(0.3f, 0.2f, 0.28f), new Color(0.5f, 0.4f, 0.28f), false);
    }

    // ═══════════════════ ЗНАМЁНА ═══════════════════

    private static void BuildBanners(Transform root, Vector3 c)
    {
        WorldDressingPropFactory.CreateOrcBanner(c + new Vector3(-8f, 0f, 6f), Quaternion.Euler(0f, -30f, 0f), root);
        WorldDressingPropFactory.CreateOrcBanner(c + new Vector3(8f, 0f, 6f), Quaternion.Euler(0f, 30f, 0f), root);
        WorldDressingPropFactory.CreateOrcBanner(c + new Vector3(-6f, 0f, -5f), Quaternion.Euler(0f, -60f, 0f), root);
    }

    // ═══════════════════ АТМОСФЕРА ═══════════════════

    private static void BuildAtmosphere(Transform root, Vector3 c)
    {
        // Факелы на частоколе
        float halfAngle = 60f;
        for (int arm = 0; arm < 2; arm++)
        {
            float sign = arm == 0 ? -1f : 1f;
            float angleRad = (90f + sign * halfAngle) * Mathf.Deg2Rad;
            Vector3 dir = new Vector3(Mathf.Cos(angleRad), 0f, Mathf.Sin(angleRad));
            Vector3 tip = c + new Vector3(0f, 0f, 10f);

            for (int i = 2; i < 24; i += 5)
            {
                Vector3 torchPos = tip + dir * (i * 0.5f);
                WorldDressingPropFactory.CreateTorch(torchPos, root);
                L0Props.CreatePointLight("PalisadeTorch_" + arm + "_" + i, torchPos + Vector3.up * 2f,
                    new Color(1f, 0.5f, 0.1f), 0.8f, 5f, root);
            }
        }

        // Следы крови у лагеря
        Color blood = new Color(0.35f, 0.04f, 0.02f);
        WorldDressingPropFactory.CreateSimpleProp(root, "Blood1", PrimitiveType.Cube,
            c + new Vector3(2f, 0.015f, 4f), new Vector3(0.6f, 0.01f, 0.4f), blood, false);
        WorldDressingPropFactory.CreateSimpleProp(root, "Blood2", PrimitiveType.Cube,
            c + new Vector3(-3f, 0.015f, 2f), new Vector3(0.5f, 0.01f, 0.6f), blood, false);

        // Камни (ассеты если есть)
        TryInstantiateRock(root, c + new Vector3(-9f, 0f, 3f), 0.7f);
        TryInstantiateRock(root, c + new Vector3(9f, 0f, -3f), 0.5f);
    }

    // ═══════════════════ УТИЛИТЫ ═══════════════════

    private static void CreateSmallFire(Transform root, Vector3 pos, string name)
    {
        Color fireColor = new Color(1f, 0.4f, 0.08f);
        WorldDressingPropFactory.CreateSimpleProp(root, name, PrimitiveType.Sphere,
            pos + Vector3.up * 0.2f, new Vector3(0.25f, 0.35f, 0.25f), fireColor, false, fireColor, 1.5f);
        L0Props.CreatePointLight(name + "_light", pos + Vector3.up * 0.5f,
            new Color(1f, 0.45f, 0.1f), 0.6f, 4f, root);
    }

    private static void CreateFireParticles(Transform root, Vector3 pos)
    {
        GameObject psObj = new GameObject("FireSparks");
        psObj.transform.SetParent(root, true);
        psObj.transform.position = pos;

        ParticleSystem ps = psObj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 1.5f;
        main.startSpeed = 1.5f;
        main.startSize = 0.06f;
        main.startColor = new Color(1f, 0.5f, 0.1f, 0.8f);
        main.maxParticles = 30;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.rateOverTime = 10f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.3f;

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(1f, 0.5f, 0.1f), 0f),
                new GradientColorKey(new Color(1f, 0.2f, 0f), 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(0.8f, 0f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = grad;

        var renderer = psObj.GetComponent<ParticleSystemRenderer>();
        Shader particleShader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (particleShader == null) particleShader = Shader.Find("Particles/Standard Unlit");
        if (particleShader == null) particleShader = Shader.Find("Unlit/Color");
        renderer.material = new Material(particleShader);
        renderer.material.color = new Color(1f, 0.5f, 0.1f, 0.8f);
    }

    private static void TryInstantiateSmoke(Transform parent, Vector3 position, float scale)
    {
        GameObject prefab = LoadPrefab(SmokePrefabPath);
        if (prefab != null)
        {
            GameObject smoke = Object.Instantiate(prefab, position, Quaternion.identity, parent);
            smoke.name = "CampSmoke_VFX";
            smoke.transform.localScale = Vector3.one * scale;
            FixPrefabMaterials(smoke);
        }
        else
        {
            GameObject smokeObj = new GameObject("FallbackCampSmoke");
            smokeObj.transform.SetParent(parent, true);
            smokeObj.transform.position = position;

            ParticleSystem ps = smokeObj.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 3f;
            main.startSpeed = 0.4f;
            main.startSize = 0.5f;
            main.startColor = new Color(0.3f, 0.3f, 0.3f, 0.4f);
            main.maxParticles = 15;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = 4f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.2f;

            var velocityOverLifetime = ps.velocityOverLifetime;
            velocityOverLifetime.enabled = true;
            velocityOverLifetime.y = 0.7f;

            var renderer = smokeObj.GetComponent<ParticleSystemRenderer>();
            Shader particleShader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (particleShader == null) particleShader = Shader.Find("Particles/Standard Unlit");
        if (particleShader == null) particleShader = Shader.Find("Unlit/Color");
        renderer.material = new Material(particleShader);
            renderer.material.color = new Color(0.3f, 0.3f, 0.3f, 0.4f);
        }
    }

    private static void TryInstantiateRock(Transform parent, Vector3 pos, float scale)
    {
        GameObject prefab = LoadPrefab(RockPrefabPath);
        if (prefab != null)
        {
            GameObject rock = Object.Instantiate(prefab, pos, Quaternion.Euler(0, scale * 100f, 0), parent);
            rock.name = "CampRock";
            rock.transform.localScale = Vector3.one * scale;
            FixPrefabMaterials(rock);
        }
        else
        {
            WorldDressingPropFactory.CreateSimpleProp(parent, "CampRock", PrimitiveType.Sphere,
                pos + Vector3.up * 0.2f, new Vector3(0.6f * scale, 0.4f * scale, 0.5f * scale),
                new Color(0.45f, 0.42f, 0.38f), false);
        }
    }

    private static void FixPrefabMaterials(GameObject obj)
    {
        Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
        Shader standard = Shader.Find("Standard");
        Shader fallback = urpLit != null ? urpLit : standard;
        if (fallback == null) fallback = Shader.Find("Diffuse");
        if (fallback == null) return;

        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            Material[] mats = renderers[i].materials;
            bool changed = false;
            for (int m = 0; m < mats.Length; m++)
            {
                bool isBroken = mats[m] == null || mats[m].shader == null
                    || mats[m].shader.name.Contains("Error")
                    || mats[m].shader.name.Contains("Hidden/InternalErrorShader")
                    || mats[m].shader.name == "Hidden/InternalErrorShader";

                if (!isBroken) continue;

                Color origColor = new Color(0.5f, 0.5f, 0.5f);
                Texture origTex = null;

                if (mats[m] != null)
                {
                    if (mats[m].HasProperty("_BaseColor"))
                        origColor = mats[m].GetColor("_BaseColor");
                    else if (mats[m].HasProperty("_Color"))
                        origColor = mats[m].color;

                    if (mats[m].HasProperty("_BaseMap"))
                        origTex = mats[m].GetTexture("_BaseMap");
                    else if (mats[m].HasProperty("_MainTex"))
                        origTex = mats[m].GetTexture("_MainTex");
                }

                mats[m] = new Material(fallback);
                if (mats[m].HasProperty("_BaseColor"))
                    mats[m].SetColor("_BaseColor", origColor);
                if (mats[m].HasProperty("_Color"))
                    mats[m].color = origColor;
                if (origTex != null)
                {
                    if (mats[m].HasProperty("_BaseMap"))
                        mats[m].SetTexture("_BaseMap", origTex);
                    if (mats[m].HasProperty("_MainTex"))
                        mats[m].mainTexture = origTex;
                }
                changed = true;
            }
            if (changed)
                renderers[i].materials = mats;
        }
    }

    private static GameObject LoadPrefab(string path) => L0Util.LoadPrefab(path);
}

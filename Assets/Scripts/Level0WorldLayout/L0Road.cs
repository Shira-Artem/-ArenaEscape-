using UnityEngine;

/// <summary>
/// Шаг 5: Дорога от замка к орочьей базе.
/// Три зоны: мирная (деревня→поворот), тревожная (поворот→поле боя), мрачная (поле боя→ворота орков).
/// Маркеры, следы орков, сломанная техника, нарастание напряжения.
/// </summary>
public static class L0Road
{
    public static void Build(GameObject container, Vector3 castleGatePoint, Vector3 roadStart, Vector3 roadEnd, Vector3 orcStrongholdCenter, float orcBaseRadius, bool createAtmosphere, bool createExtraAtmosphere, bool createSightLine)
    {
        if (container == null)
            return;

        Transform parent = container.transform;
        roadStart = L0OrcArenaConfig.RoadStart;
        Vector3 roadMid = L0OrcArenaConfig.RoadMid;
        Vector3 baseEntrance = L0OrcArenaConfig.BaseEntrance;

        CreateRoadSegment(parent, roadStart, roadMid, "CleanRoad_StartToMid");
        CreateRoadSegment(parent, roadMid, baseEntrance, "CleanRoad_MidToBase");
        L0Props.CreateGroundPatchWalkable(castleGatePoint + new Vector3(0f, 0f, -7f), new Vector3(L0OrcArenaConfig.RoadWidth, 0.07f, 12f), new Color(0.48f, 0.38f, 0.24f), parent, "CleanGateRoadMouth", Quaternion.identity);

        BuildPeacefulZone(parent);
        BuildTenseZone(parent);
        BuildBattlefieldZone(parent);
        BuildDarkApproach(parent);

        L0Props.CreateRouteSign("К ЛОГОВУ →", L0OrcArenaConfig.GetRoadPoint(0.72f) + new Vector3(-8.4f, 0f, 0.4f), Quaternion.Euler(0f, 24f, 0f), parent);
    }

    /// <summary>
    /// Зона 1: Мирная (t=0.05..0.35, Z≈-38..-55). Факелы, камни, указатели.
    /// </summary>
    private static void BuildPeacefulZone(Transform parent)
    {
        // Факелы по бокам у начала дороги
        Vector3 p1 = GetRoadEdge(0.06f, -1f, 6f);
        L0Props.CreateTorchPost(p1, FaceRoad(0.06f), 1.0f, parent);
        Vector3 p2 = GetRoadEdge(0.06f, 1f, 6f);
        L0Props.CreateTorchPost(p2, FaceRoad(0.06f), 1.0f, parent);

        // Камни-бордюры
        L0Props.CreatePathMarkerStones(GetRoadEdge(0.14f, -1f, 7f), Quaternion.Euler(0f, 20f, 0f), 1.0f, parent);
        L0Props.CreatePathMarkerStones(GetRoadEdge(0.14f, 1f, 7.5f), Quaternion.Euler(0f, 50f, 0f), 0.9f, parent);

        // Факел + лавка — точка отдыха
        Vector3 restSpot = GetRoadEdge(0.22f, -1f, 7f);
        L0Props.CreateTorchPost(restSpot, FaceRoad(0.22f), 0.95f, parent);
        L0Props.CreateBench(restSpot + new Vector3(-1.5f, 0f, 0f), Quaternion.Euler(0f, 80f, 0f), parent);

        // Камни справа
        L0Props.CreatePathMarkerStones(GetRoadEdge(0.30f, 1f, 8f), Quaternion.Euler(0f, 110f, 0f), 1.1f, parent);

        // Указатель
        L0Props.CreateRouteSign("Деревня ←", GetRoadEdge(0.10f, 1f, 9f), Quaternion.Euler(0f, -45f, 0f), parent);

        // Колеи от колёс
        Vector3 rut1 = L0OrcArenaConfig.GetRoadPoint(0.12f) + new Vector3(-2f, 0.01f, 0f);
        L0Props.CreateWheelRutMark(rut1, FaceRoad(0.12f), 8f, parent);
        Vector3 rut2 = L0OrcArenaConfig.GetRoadPoint(0.20f) + new Vector3(1.5f, 0.01f, 0f);
        L0Props.CreateWheelRutMark(rut2, FaceRoad(0.20f), 6f, parent);
    }

    /// <summary>
    /// Зона 2: Тревожная (t=0.35..0.55, Z≈-55..-75). Следы орков, сломанная телега, колья.
    /// </summary>
    private static void BuildTenseZone(Transform parent)
    {
        // Первые следы орков: сломанная телега поперёк дороги
        Vector3 cartPos = L0OrcArenaConfig.GetRoadPoint(0.38f) + new Vector3(5f, 0f, 1f);
        L0Props.CreateBrokenCart(cartPos, Quaternion.Euler(0f, -35f, 0f), parent);

        // Ящики рядом с телегой — рассыпанный груз
        L0Props.CreateCratesAndBags(cartPos + new Vector3(2f, 0f, -1.5f), Quaternion.Euler(0f, 60f, 0f), parent);
        L0Props.CreateCratesAndBags(cartPos + new Vector3(-1.5f, 0f, 2f), Quaternion.Euler(0f, -20f, 0f), parent);

        // Факел-маркер (тусклее — уже не мирная зона)
        L0Props.CreateTorchPost(GetRoadEdge(0.40f, -1f, 7f), FaceRoad(0.40f), 0.85f, parent);

        // Оркские колья появляются — первые признаки врага
        L0Props.CreateSpikeCluster(GetRoadEdge(0.45f, 1f, 9f), Quaternion.Euler(0f, 30f, 0f), 0.7f, parent);
        L0Props.CreateSpikeCluster(GetRoadEdge(0.50f, -1f, 8f), Quaternion.Euler(0f, -15f, 0f), 0.65f, parent);

        // Стрелы в земле — кто-то стрелял
        L0Props.CreateBattleArrowCluster(L0OrcArenaConfig.GetRoadPoint(0.42f) + new Vector3(-3f, 0f, 0.5f), parent);
        L0Props.CreateBattleArrowCluster(L0OrcArenaConfig.GetRoadPoint(0.48f) + new Vector3(2f, 0f, -1f), parent);

        // Сломанный щит у обочины
        L0Props.CreateBrokenShield(GetRoadEdge(0.46f, -1f, 5f), Quaternion.Euler(0f, 140f, 0f), parent);

        // Указатель-предупреждение
        L0Props.CreateRouteSign("⚠ Опасно!", GetRoadEdge(0.52f, 1f, 8f), Quaternion.Euler(0f, 15f, 0f), parent);

        // Камни
        L0Props.CreatePathMarkerStones(GetRoadEdge(0.43f, 1f, 7.5f), Quaternion.Euler(0f, 70f, 0f), 1.0f, parent);
    }

    /// <summary>
    /// Зона 3: Поле боя (t=0.55..0.75, Z≈-75..-100). Обломки баррикад, копья, щиты, гарь.
    /// </summary>
    private static void BuildBattlefieldZone(Transform parent)
    {
        float tCenter = 0.65f;
        Vector3 battleCenter = L0OrcArenaConfig.GetRoadPoint(tCenter);

        // Тёмное пятно гари на земле — здесь был бой
        L0Props.CreateGroundPatch(battleCenter + new Vector3(-2f, 0.02f, 0f), new Vector3(12f, 0.05f, 8f), new Color(0.12f, 0.10f, 0.08f), parent, "BattlefieldScorch");

        // Обломки баррикад
        L0Props.CreateRoadEdgeRubble(battleCenter + new Vector3(-4f, 0f, 2f), Quaternion.Euler(0f, 30f, 0f), 1.2f, parent);
        L0Props.CreateRoadEdgeRubble(battleCenter + new Vector3(3f, 0f, -1.5f), Quaternion.Euler(0f, -45f, 0f), 1.0f, parent);

        // Копья воткнуты в землю
        L0Props.CreateSpearInGround(battleCenter + new Vector3(-5f, 0f, 1f), Quaternion.Euler(0f, 20f, 0f), parent);
        L0Props.CreateSpearInGround(battleCenter + new Vector3(4f, 0f, 3f), Quaternion.Euler(0f, -30f, 0f), parent);
        L0Props.CreateSpearInGround(battleCenter + new Vector3(1f, 0f, -3f), Quaternion.Euler(0f, 75f, 0f), parent);

        // Сломанные щиты
        L0Props.CreateBrokenShield(battleCenter + new Vector3(-3f, 0f, -2f), Quaternion.Euler(0f, 50f, 0f), parent);
        L0Props.CreateBrokenShield(battleCenter + new Vector3(5f, 0f, 1f), Quaternion.Euler(0f, -70f, 0f), parent);

        // Стрелы повсюду
        L0Props.CreateBattleArrowCluster(battleCenter + new Vector3(-1f, 0f, 2.5f), parent);
        L0Props.CreateBattleArrowCluster(battleCenter + new Vector3(2.5f, 0f, -2f), parent);
        L0Props.CreateBattleArrowCluster(battleCenter + new Vector3(-4f, 0f, -1f), parent);

        // Кострище — потухший
        L0Props.CreateCampfire(battleCenter + new Vector3(6f, 0f, 0f), 1.3f, parent);

        // Оркские колья и тотем
        L0Props.CreateSpikeCluster(GetRoadEdge(0.60f, -1f, 9f), Quaternion.Euler(0f, 10f, 0f), 0.9f, parent);
        L0Props.CreateSpikeCluster(GetRoadEdge(0.70f, 1f, 8f), Quaternion.Euler(0f, -25f, 0f), 0.85f, parent);
        L0Props.CreateOrcTotem(GetRoadEdge(0.66f, 1f, 10f), Quaternion.Euler(0f, -15f, 0f), 0.8f, parent);

        // Факелы — тусклые, орочьи
        L0Props.CreateTorchPost(GetRoadEdge(0.58f, -1f, 7f), FaceRoad(0.58f), 0.8f, parent);
        L0Props.CreateTorchPost(GetRoadEdge(0.72f, 1f, 7f), FaceRoad(0.72f), 0.75f, parent);
    }

    /// <summary>
    /// Зона 4: Подход к базе (t=0.75..0.95, Z≈-100..-118). Оркская территория.
    /// </summary>
    private static void BuildDarkApproach(Transform parent)
    {
        // Палисады вдоль дороги — орки обозначили свою территорию
        L0Props.CreatePalisadeSegment(GetRoadEdge(0.78f, -1f, 8f), Quaternion.Euler(0f, 10f, 0f), 1.0f, parent);
        L0Props.CreatePalisadeSegment(GetRoadEdge(0.78f, 1f, 8f), Quaternion.Euler(0f, -5f, 0f), 1.0f, parent);
        L0Props.CreatePalisadeSegment(GetRoadEdge(0.85f, -1f, 7.5f), Quaternion.Euler(0f, 15f, 0f), 0.95f, parent);
        L0Props.CreatePalisadeSegment(GetRoadEdge(0.85f, 1f, 7.5f), Quaternion.Euler(0f, -10f, 0f), 0.95f, parent);

        // Оркские знамёна
        L0Props.CreateOrcBanner(GetRoadEdge(0.80f, -1f, 6f), Quaternion.Euler(0f, 25f, 0f), parent);
        L0Props.CreateOrcBanner(GetRoadEdge(0.88f, 1f, 6f), Quaternion.Euler(0f, -20f, 0f), parent);

        // Колья с черепами
        L0Props.CreateSpikeCluster(GetRoadEdge(0.82f, 1f, 9f), Quaternion.Euler(0f, 40f, 0f), 1.0f, parent);
        L0Props.CreateSpikeCluster(GetRoadEdge(0.90f, -1f, 8f), Quaternion.Euler(0f, -30f, 0f), 0.95f, parent);

        // Тотемы
        L0Props.CreateOrcTotem(GetRoadEdge(0.83f, -1f, 10f), Quaternion.Euler(0f, 5f, 0f), 0.9f, parent);
        L0Props.CreateOrcTotem(GetRoadEdge(0.92f, 1f, 10f), Quaternion.Euler(0f, -15f, 0f), 0.85f, parent);

        // Кости на дороге
        L0Props.CreateBonePile(L0OrcArenaConfig.GetRoadPoint(0.80f) + new Vector3(-3f, 0f, 1f), 0.8f, parent);
        L0Props.CreateBonePile(L0OrcArenaConfig.GetRoadPoint(0.87f) + new Vector3(2f, 0f, -0.5f), 0.7f, parent);

        // Тёмное пятно земли — территория орков
        Vector3 darkCenter = L0OrcArenaConfig.GetRoadPoint(0.85f);
        L0Props.CreateGroundPatch(darkCenter + new Vector3(0f, 0.015f, 0f), new Vector3(16f, 0.04f, 14f), new Color(0.10f, 0.08f, 0.06f), parent, "OrcTerritoryScorch");

        // Указатель-предупреждение
        L0Props.CreateRouteSign("⚔ ВПЕРЕДИ ОРКИ", GetRoadEdge(0.76f, -1f, 9f), Quaternion.Euler(0f, 20f, 0f), parent);

        // Факелы — последние перед базой
        L0Props.CreateTorchPost(GetRoadEdge(0.92f, -1f, 6.5f), FaceRoad(0.92f), 0.85f, parent);
        L0Props.CreateTorchPost(GetRoadEdge(0.92f, 1f, 6.5f), FaceRoad(0.92f), 0.85f, parent);
    }

    // --- Helpers ---

    private static void CreateRoadSegment(Transform parent, Vector3 start, Vector3 end, string name)
    {
        Vector3 delta = end - start;
        delta.y = 0f;
        if (delta.sqrMagnitude < 0.01f)
            return;

        Vector3 mid = (start + end) * 0.5f;
        Quaternion rotation = Quaternion.LookRotation(delta.normalized, Vector3.up);
        L0Props.CreateGroundPatchWalkable(mid, new Vector3(L0OrcArenaConfig.RoadWidth, 0.07f, delta.magnitude), L0OrcArenaMaterials.Road, parent, name, rotation);
    }

    private static Vector3 GetRoadEdge(float t, float side, float distance)
    {
        Vector3 point = L0OrcArenaConfig.GetRoadPoint(t);
        Vector3 dir = L0OrcArenaConfig.GetRoadPoint(Mathf.Clamp01(t + 0.03f)) - L0OrcArenaConfig.GetRoadPoint(Mathf.Clamp01(t - 0.03f));
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.01f) dir = Vector3.forward;
        dir.Normalize();
        Vector3 sideVec = Vector3.Cross(Vector3.up, dir).normalized;
        return point + sideVec * (side * distance);
    }

    private static Quaternion FaceRoad(float t)
    {
        Vector3 dir = L0OrcArenaConfig.GetRoadPoint(Mathf.Clamp01(t + 0.03f)) - L0OrcArenaConfig.GetRoadPoint(Mathf.Clamp01(t - 0.03f));
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.01f) dir = Vector3.forward;
        return Quaternion.LookRotation(dir.normalized, Vector3.up);
    }
}

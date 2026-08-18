using UnityEngine;

/// <summary>
/// Деревня жителей v4: плотная жилая часть слева от дороги + лагерь беженцев у дороги.
/// Только визуал: дома, площадь, телеги, заборы, бытовой декор, тёплые огни.
/// </summary>
public static class L0Village
{
    public static void Build(GameObject container, Vector3 center, int houseCount, float villageRadius, bool createAtmosphere, bool createExtraAtmosphere, bool createRoadsideCamp)
    {
        if (container == null) return;

        Transform parent = container.transform;
        int count = Mathf.Clamp(houseCount, 8, 10);
        float radius = Mathf.Clamp(villageRadius, 22f, 30f);

        Vector3 square = center + new Vector3(-3f, 0f, 0f);

        // Чёткий центр поселения: площадь, очаг, колодец, лавки.
        L0Props.CreateGroundPatch(square, new Vector3(24f, 0.08f, 18f), new Color(0.31f, 0.26f, 0.17f), parent, "VillageSquare_v4");
        L0Props.CreateCampfire(square + new Vector3(2.2f, 0f, -1.2f), 1.85f, parent);
        L0Props.CreateWell(square + new Vector3(-4.7f, 0f, 2.8f), parent);
        L0Props.CreateLargeVillageHall(square + new Vector3(-9.5f, 0f, -6.5f), Quaternion.Euler(0f, 28f, 0f), 1.25f, parent);

        for (int i = 0; i < 4; i++)
        {
            float a = -70f + i * 38f;
            Vector3 pos = square + Quaternion.Euler(0f, a, 0f) * new Vector3(0f, 0f, 6.0f);
            L0Props.CreateBench(pos, Quaternion.Euler(0f, a + 90f, 0f), parent);
        }

        // Дома ставим полукольцом слева от дороги, лицом к площади.
        for (int i = 0; i < count; i++)
        {
            float t = count <= 1 ? 0f : i / (float)(count - 1);
            float angle = Mathf.Lerp(210f, 510f, t) + Random.Range(-9f, 9f);
            float ring = Random.Range(radius * 0.48f, radius * 0.88f);
            Vector3 pos = square + Quaternion.Euler(0f, angle, 0f) * new Vector3(0f, 0f, ring);

            // Главная дорога около x=0: дома держим слева и не ставим на дорожный коридор.
            pos.x = Mathf.Min(pos.x, -9.5f);
            pos.z += Random.Range(-1.8f, 1.8f);
            if (pos.z > -24f) pos.z -= 8f;

            Quaternion rot = Quaternion.LookRotation((square - pos).normalized, Vector3.up);
            float scale = Random.Range(1.18f, 1.62f);
            bool damaged = i == 1 || i == 4 || i == 7;
            L0Props.CreateSimpleHouse(pos, rot, scale, damaged, parent);
        }

        // Небольшие бытовые островки, чтобы поселение выглядело жилым.
        L0Props.CreateMarketCanopy(square + new Vector3(5.8f, 0f, 5.5f), Quaternion.Euler(0f, -30f, 0f), 1.05f, parent);
        L0Props.CreateMarketCanopy(square + new Vector3(-1.0f, 0f, 7.2f), Quaternion.Euler(0f, 12f, 0f), 0.95f, parent);
        L0Props.CreateGardenPatch(square + new Vector3(-14f, 0.04f, 7f), Quaternion.Euler(0f, -8f, 0f), 1.1f, parent);
        L0Props.CreateGardenPatch(square + new Vector3(-16f, 0.04f, -1f), Quaternion.Euler(0f, 10f, 0f), 0.95f, parent);
        L0Props.CreateClothesLine(square + new Vector3(-6f, 0f, 9.2f), Quaternion.Euler(0f, 20f, 0f), 1f, parent);

        L0Props.CreateSimpleCart(square + new Vector3(10f, 0f, 2.5f), Quaternion.Euler(0f, -58f, 0f), parent);
        L0Props.CreateSimpleCart(square + new Vector3(7.6f, 0f, -7.5f), Quaternion.Euler(0f, 42f, 0f), parent);
        L0Props.CreateBrokenCart(square + new Vector3(-12f, 0f, -10f), Quaternion.Euler(0f, 20f, 0f), parent);

        for (int i = 0; i < 12; i++)
        {
            Vector3 pos = square + RandomCircle(Random.Range(6f, 17f));
            pos.x = Mathf.Min(pos.x, -5.5f);
            L0Props.CreateCratesAndBags(pos, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f), parent);
        }

        for (int i = 0; i < 8; i++)
        {
            Vector3 pos = square + RandomCircle(Random.Range(8f, 19f));
            pos.x = Mathf.Min(pos.x, -6.5f);
            L0Props.CreateHaystack(pos, Random.Range(0.9f, 1.35f), parent);
        }

        // Забор собирает деревню в форму, а не дает ей выглядеть россыпью объектов.
        for (int i = 0; i < 18; i++)
        {
            float angle = i * 20f + Random.Range(-5f, 5f);
            Vector3 pos = square + Quaternion.Euler(0f, angle, 0f) * new Vector3(0f, 0f, radius * Random.Range(0.72f, 1.02f));
            pos.x = Mathf.Min(pos.x, -4.8f);
            Quaternion rot = Quaternion.Euler(0f, angle + 90f, 0f);
            L0Props.CreateFenceSegment(pos, rot, parent);
        }

        // Лагерь беженцев/телег справа от дороги. Он даёт контраст: деревня слева, тревога у дороги справа.
        if (createRoadsideCamp)
        {
            Vector3 camp = new Vector3(9.5f, 0f, center.z - 2f);
            L0Props.CreateGroundPatch(camp, new Vector3(18f, 0.07f, 14f), new Color(0.28f, 0.24f, 0.17f), parent, "RoadsideRefugeeCamp_v4");
            L0Props.CreateCampfire(camp + new Vector3(0f, 0f, -1f), 1.1f, parent);
            L0Props.CreateRefugeeTent(camp + new Vector3(-5.0f, 0f, 3.5f), Quaternion.Euler(0f, 120f, 0f), 1.15f, parent);
            L0Props.CreateRefugeeTent(camp + new Vector3(4.6f, 0f, 3.2f), Quaternion.Euler(0f, -135f, 0f), 1.1f, parent);
            L0Props.CreateSimpleCart(camp + new Vector3(5.6f, 0f, -4.1f), Quaternion.Euler(0f, -35f, 0f), parent);
            L0Props.CreateCratesAndBags(camp + new Vector3(-3.9f, 0f, -4.4f), Quaternion.Euler(0f, 15f, 0f), parent);
            L0Props.CreateHaystack(camp + new Vector3(2.8f, 0f, -5.2f), 0.95f, parent);
            L0Props.CreateFenceSegment(camp + new Vector3(-8.4f, 0f, 0f), Quaternion.Euler(0f, 0f, 0f), parent);
            L0Props.CreateFenceSegment(camp + new Vector3(8.4f, 0f, 0f), Quaternion.Euler(0f, 0f, 0f), parent);
        }

        // === Шаг 4B: правая сторона деревни ===
        // 3 дома справа от площади — деревня "обнимает" центр с двух сторон
        BuildRightSideHouses(square, parent);
        BuildVillageLifeProps(square, center, parent);

        L0Props.CreateRouteSign("Деревня жителей", center + new Vector3(4f, 0f, 12.5f), Quaternion.Euler(0f, 120f, 0f), parent);

        if (createAtmosphere)
        {
            L0Props.CreatePointLight("VillageWarmLight_MainFire", square + new Vector3(2.2f, 2.1f, -1.2f), new Color(1f, 0.62f, 0.25f), 1.25f, 10f, parent);
            L0Props.CreatePointLight("VillageWarmLight_RoadCamp", new Vector3(9.5f, 2.0f, center.z - 3f), new Color(1f, 0.62f, 0.26f), 0.8f, 8f, parent);

            L0Props.CreateLantern(square + new Vector3(-8f, 0f, 7f), parent);
            L0Props.CreateLantern(square + new Vector3(7f, 0f, 4f), parent);
            L0Props.CreateLantern(new Vector3(12f, 0f, center.z + 4f), parent);
        }

        if (createAtmosphere && createExtraAtmosphere)
        {
            for (int i = 0; i < 8; i++)
            {
                Vector3 pos = square + RandomCircle(Random.Range(8f, 20f)) + Vector3.up * Random.Range(2.2f, 3.8f);
                pos.x = Mathf.Min(pos.x, -4.5f);
                L0Props.CreateSmokePuff(pos, Random.Range(0.65f, 1.15f), parent);
            }
        }
    }

    /// <summary>
    /// Шаг 4B: 3 дома справа от площади (x > -5), создают двустороннюю застройку.
    /// </summary>
    private static void BuildRightSideHouses(Vector3 square, Transform parent)
    {
        // Дом 1: ближний к площади, лицом к ней
        Vector3 h1 = square + new Vector3(12f, 0f, -2f);
        Quaternion r1 = Quaternion.LookRotation((square - h1).normalized, Vector3.up);
        L0Props.CreateSimpleHouse(h1, r1, 1.35f, false, parent);

        // Дом 2: чуть южнее и дальше
        Vector3 h2 = square + new Vector3(14f, 0f, -8f);
        Quaternion r2 = Quaternion.LookRotation((square - h2).normalized, Vector3.up);
        L0Props.CreateSimpleHouse(h2, r2, 1.25f, false, parent);

        // Дом 3: повреждённый, ближе к дороге (следы набега)
        Vector3 h3 = square + new Vector3(10f, 0f, 5f);
        Quaternion r3 = Quaternion.LookRotation((square - h3).normalized, Vector3.up);
        L0Props.CreateSimpleHouse(h3, r3, 1.15f, true, parent);
    }

    /// <summary>
    /// Шаг 4B: бытовой декор — бочки, дрова, корыто, заборы, цветы.
    /// </summary>
    private static void BuildVillageLifeProps(Vector3 square, Vector3 center, Transform parent)
    {
        // Бочки у домов (левая и правая стороны)
        L0Props.CreateBarrel(square + new Vector3(-11f, 0f, -3f), Quaternion.Euler(0f, 15f, 0f), parent);
        L0Props.CreateBarrel(square + new Vector3(-11.5f, 0f, -3.6f), Quaternion.Euler(0f, -20f, 0f), parent);
        L0Props.CreateBarrel(square + new Vector3(13f, 0f, -4f), Quaternion.Euler(0f, 35f, 0f), parent);
        L0Props.CreateBarrel(square + new Vector3(11f, 0f, 3.5f), Quaternion.Euler(0f, -10f, 0f), parent);

        // Поленницы у домов
        L0Props.CreateWoodPile(square + new Vector3(-13f, 0f, 2f), Quaternion.Euler(0f, 40f, 0f), 1.1f, parent);
        L0Props.CreateWoodPile(square + new Vector3(15f, 0f, -6f), Quaternion.Euler(0f, -25f, 0f), 0.95f, parent);

        // Корыто с водой у правых домов
        L0Props.CreateWaterTrough(square + new Vector3(11.5f, 0f, -1f), Quaternion.Euler(0f, 30f, 0f), 1.0f, parent);

        // Заборы — правая сторона деревни, ограждают правые дома
        L0Props.CreateFenceSegment(square + new Vector3(8f, 0f, -10f), Quaternion.Euler(0f, -15f, 0f), parent);
        L0Props.CreateFenceSegment(square + new Vector3(16f, 0f, -5f), Quaternion.Euler(0f, 75f, 0f), parent);
        L0Props.CreateFenceSegment(square + new Vector3(16f, 0f, 2f), Quaternion.Euler(0f, 85f, 0f), parent);
        L0Props.CreateFenceSegment(square + new Vector3(8f, 0f, 7f), Quaternion.Euler(0f, -5f, 0f), parent);

        // Цветы — контраст с мрачной орочьей зоной
        L0Props.CreateFlowerBed(square + new Vector3(10f, 0f, 1f), Quaternion.Euler(0f, 20f, 0f), 1.0f, parent, new Color(1f, 0.85f, 0.15f));
        L0Props.CreateFlowerBed(square + new Vector3(-9f, 0f, 6f), Quaternion.Euler(0f, -15f, 0f), 0.9f, parent, new Color(0.95f, 0.35f, 0.4f));
        L0Props.CreateFlowerBed(square + new Vector3(14f, 0f, -1f), Quaternion.Euler(0f, 45f, 0f), 0.85f, parent, new Color(0.6f, 0.4f, 0.9f));
        L0Props.CreateFlowerBed(square + new Vector3(-15f, 0f, -4f), Quaternion.Euler(0f, 10f, 0f), 1.0f, parent, new Color(1f, 0.6f, 0.15f));

        // Лавка у правого дома
        L0Props.CreateBench(square + new Vector3(12f, 0f, 0.5f), Quaternion.Euler(0f, -60f, 0f), parent);
    }

    private static Vector3 RandomCircle(float radius)
    {
        Vector2 p = Random.insideUnitCircle * radius;
        return new Vector3(p.x, 0f, p.y);
    }
}

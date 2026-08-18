using UnityEngine;

/// <summary>
/// Decorative Level0 dragon builder.
/// The dragon is a high backdrop pass over the orc arena, not a low obstacle inside it.
/// </summary>
public static class L0Dragon
{
    public static void Build(GameObject container, Vector3 orbitCenter, float orbitRadius, float cruiseHeight, float attackHeight, float visualScale, bool breathesFire, bool useParticles, bool extraAtmosphere)
    {
        if (container == null) return;

        Transform parent = container.transform;

        Vector3 safeOrbitCenter = orbitCenter;
        safeOrbitCenter.y = 0f;

        float safeRadius = Mathf.Max(58f, orbitRadius * 2.9f);
        float safeCruiseHeight = Mathf.Max(62f, cruiseHeight);
        float safeAttackHeight = Mathf.Clamp(Mathf.Max(46f, attackHeight), 45f, safeCruiseHeight - 8f);
        float safeVisualScale = Mathf.Max(2.25f, visualScale * 1.35f);

        if (extraAtmosphere)
        {
            L0Props.CreateSmokePuff(safeOrbitCenter + new Vector3(-22f, 38f, -16f), 1.55f, parent);
            L0Props.CreateSmokePuff(safeOrbitCenter + new Vector3(24f, 42f, 14f), 1.35f, parent);
        }

        GameObject dragonObj = new GameObject("L0_Dragon");
        dragonObj.transform.SetParent(parent, false);
        dragonObj.transform.localScale = Vector3.one * safeVisualScale;

        L0DragonFly dragon = dragonObj.AddComponent<L0DragonFly>();
        dragon.orbitCenter = safeOrbitCenter;
        dragon.baseFlyRadius = safeRadius;
        dragon.cruiseHeight = safeCruiseHeight;
        dragon.attackHeight = safeAttackHeight;
        dragon.enableFireParticles = breathesFire && useParticles;
        dragon.fireCoreRate = useParticles ? 120f : 0f;
        dragon.fireTrailRate = useParticles ? 70f : 0f;
        dragon.smokeRate = useParticles ? 28f : 0f;
        dragon.startAngleDegrees = -35f;
        dragon.cruiseSpeed = 0.18f;
        dragon.attackSpeed = 0.42f;
        dragon.torchSpeed = 0.12f;
        dragon.cruiseDuration = 8.8f;
        dragon.swoopDuration = 3.6f;
        dragon.fireDuration = breathesFire ? 4.2f : 0.2f;
        dragon.recoverDuration = 5.0f;
        dragon.bodyLength = 21.5f;
        dragon.maxRadius = 1.9f;
        dragon.wingSpan = 16.5f;
        dragon.spineSegments = 56;
        dragon.radialSegments = 16;
        dragon.bodyWaveAmplitude = 0.20f;
        dragon.InitializeNow();
    }
}

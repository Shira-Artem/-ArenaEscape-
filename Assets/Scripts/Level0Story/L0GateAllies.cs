using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class L0GateAllies : MonoBehaviour
{
    // R2: число стражи НЕ снижаем (остаётся 6) — ослабляем только урон/HP/темп,
    // чтобы без игрока строй медленно проигрывал, но не вымирал мгновенно.
    public int allyCount = 6;
    public int damage = 14;
    public float moveSpeed = 5.4f;
    public float attackRange = 2.6f;
    public float attackCooldown = 1.7f;
    public float enemyScanInterval = 0.75f;
    public Vector3 spawnCenter = new Vector3(0f, 0.2f, -11.5f);
    public Vector3 guardCenter = new Vector3(0f, 0.2f, -20.5f);
    public float spawnSpread = 3.2f;
    public float guardSpread = 5.4f;

    private static Material bodyMaterial;
    private static Material armorMaterial;
    private static Material skinMaterial;
    private static Material shieldMaterial;
    private static Material weaponMaterial;
    private static Material helmetMaterial;
    private static Material crestMaterial;
    private static Material capeMaterial;
    private static Material emblemMaterial;

    private readonly List<L0GateAllySoldier> soldiers = new List<L0GateAllySoldier>();
    private bool spawned;

    public static L0GateAllies Ensure()
    {
        L0GateAllies allies = FindFirstObjectByType<L0GateAllies>();
        if (allies != null)
            return allies;

        GameObject obj = new GameObject("L0_GATE_ALLIES");
        return obj.AddComponent<L0GateAllies>();
    }

    public void SpawnAllies()
    {
        SpawnAllies(allyCount);
    }

    public void SpawnAllies(int count)
    {
        if (spawned)
            return;

        spawned = true;
        allyCount = Mathf.Clamp(count, 5, 8);
        CacheMaterials();

        for (int i = 0; i < allyCount; i++)
        {
            Vector3 spawnPos = spawnCenter + new Vector3(Mathf.Lerp(-spawnSpread, spawnSpread, allyCount <= 1 ? 0.5f : i / (float)(allyCount - 1)), 0f, Random.Range(-0.4f, 0.4f));
            Vector3 guardPos = guardCenter + new Vector3(Mathf.Lerp(-guardSpread, guardSpread, allyCount <= 1 ? 0.5f : i / (float)(allyCount - 1)), 0f, Random.Range(-1.0f, 1.0f));

            GameObject soldier = new GameObject("L0_Gate_Ally_" + i);
            soldier.transform.SetParent(transform);
            soldier.transform.position = spawnPos;
            soldier.transform.rotation = Quaternion.LookRotation(Vector3.back, Vector3.up);

            BuildSoldierVisual(soldier.transform, i);

            L0GateAllySoldier logic = soldier.AddComponent<L0GateAllySoldier>();
            logic.Init(guardPos, moveSpeed, attackRange, attackCooldown, enemyScanInterval, damage);
            soldiers.Add(logic);
        }
    }

    private static void CacheMaterials()
    {
        if (bodyMaterial != null)
            return;

        // R2: чуть наряднее — насыщенный геральдический синий, стальной доспех,
        // шлем с красным гребнем, плащ, эмблема на щите.
        bodyMaterial = MakeMaterial(new Color(0.16f, 0.32f, 0.76f));
        armorMaterial = MakeMaterial(new Color(0.66f, 0.69f, 0.74f));
        skinMaterial = MakeMaterial(new Color(0.86f, 0.68f, 0.52f));
        shieldMaterial = MakeMaterial(new Color(0.12f, 0.22f, 0.48f));
        weaponMaterial = MakeMaterial(new Color(0.82f, 0.84f, 0.88f));
        helmetMaterial = MakeMaterial(new Color(0.72f, 0.75f, 0.80f));
        crestMaterial = MakeMaterial(new Color(0.80f, 0.12f, 0.10f));
        capeMaterial = MakeMaterial(new Color(0.10f, 0.20f, 0.50f));
        emblemMaterial = MakeMaterial(new Color(0.90f, 0.82f, 0.30f));
    }

    private static Material MakeMaterial(Color color)
    {
        Material material = new Material(Shader.Find("Standard"));
        material.color = color;
        return material;
    }

    private static void BuildSoldierVisual(Transform root, int index)
    {
        GameObject body = Prim(PrimitiveType.Capsule, root, "Body", new Vector3(0f, 0.88f, 0f), new Vector3(0.45f, 0.62f, 0.45f), bodyMaterial);
        GameObject head = Prim(PrimitiveType.Sphere, root, "Head", new Vector3(0f, 1.55f, 0f), new Vector3(0.28f, 0.28f, 0.28f), skinMaterial);
        GameObject chest = Prim(PrimitiveType.Cube, root, "Chest_Armor", new Vector3(0f, 0.92f, -0.24f), new Vector3(0.48f, 0.42f, 0.08f), armorMaterial);

        // R2: шлем + красный гребень + плащ — стражники выглядят солиднее.
        Prim(PrimitiveType.Sphere, root, "Helmet", new Vector3(0f, 1.60f, 0f), new Vector3(0.33f, 0.30f, 0.33f), helmetMaterial);
        Prim(PrimitiveType.Cube, root, "Helmet_Nasal", new Vector3(0f, 1.52f, 0.16f), new Vector3(0.07f, 0.18f, 0.07f), helmetMaterial);
        GameObject crest = Prim(PrimitiveType.Cube, root, "Helmet_Crest", new Vector3(0f, 1.82f, -0.02f), new Vector3(0.06f, 0.16f, 0.34f), crestMaterial);
        crest.transform.localRotation = Quaternion.Euler(12f, 0f, 0f);
        Prim(PrimitiveType.Cube, root, "Cape", new Vector3(0f, 0.92f, -0.30f), new Vector3(0.50f, 0.74f, 0.05f), capeMaterial);
        Prim(PrimitiveType.Cylinder, root, "Leg_L", new Vector3(-0.13f, 0.20f, 0f), new Vector3(0.09f, 0.28f, 0.09f), armorMaterial);
        Prim(PrimitiveType.Cylinder, root, "Leg_R", new Vector3(0.13f, 0.20f, 0f), new Vector3(0.09f, 0.28f, 0.09f), armorMaterial);
        Prim(PrimitiveType.Cylinder, root, "Arm_L", new Vector3(-0.38f, 0.92f, -0.02f), new Vector3(0.07f, 0.30f, 0.07f), bodyMaterial);
        Prim(PrimitiveType.Cylinder, root, "Arm_R", new Vector3(0.38f, 0.92f, -0.02f), new Vector3(0.07f, 0.30f, 0.07f), bodyMaterial);

        GameObject shield = Prim(PrimitiveType.Cube, root, "Shield", new Vector3(-0.55f, 0.95f, -0.20f), new Vector3(0.08f, 0.52f, 0.42f), shieldMaterial);
        shield.transform.localRotation = Quaternion.Euler(0f, 0f, 4f);
        // Геральдическая эмблема (крест) на щите.
        Prim(PrimitiveType.Cube, shield.transform, "Emblem_V", new Vector3(0.6f, 0f, 0f), new Vector3(0.6f, 0.5f, 0.16f), emblemMaterial);
        Prim(PrimitiveType.Cube, shield.transform, "Emblem_H", new Vector3(0.6f, 0f, 0f), new Vector3(0.6f, 0.16f, 0.5f), emblemMaterial);

        if (index % 2 == 0)
        {
            GameObject spear = Prim(PrimitiveType.Cylinder, root, "Spear", new Vector3(0.58f, 1.05f, -0.06f), new Vector3(0.035f, 0.95f, 0.035f), weaponMaterial);
            spear.transform.localRotation = Quaternion.Euler(0f, 0f, -15f);
        }
        else
        {
            GameObject sword = Prim(PrimitiveType.Cube, root, "Sword", new Vector3(0.54f, 1.00f, -0.10f), new Vector3(0.055f, 0.70f, 0.055f), weaponMaterial);
            sword.transform.localRotation = Quaternion.Euler(0f, 0f, -28f);
        }

        DestroyCollider(body);
        DestroyCollider(head);
        DestroyCollider(chest);
    }

    private static GameObject Prim(PrimitiveType type, Transform parent, string name, Vector3 localPos, Vector3 localScale, Material material)
    {
        GameObject obj = GameObject.CreatePrimitive(type);
        obj.name = name;
        obj.transform.SetParent(parent, false);
        obj.transform.localPosition = localPos;
        obj.transform.localRotation = Quaternion.identity;
        obj.transform.localScale = localScale;

        DestroyCollider(obj);

        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
            renderer.sharedMaterial = material;

        return obj;
    }

    private static void DestroyCollider(GameObject obj)
    {
        Collider collider = obj.GetComponent<Collider>();
        if (collider != null)
            Destroy(collider);
    }
}

public class L0GateAllySoldier : MonoBehaviour
{
    private Vector3 guardPosition;
    private float moveSpeed;
    private float attackRange;
    private float attackCooldown;
    private float enemyScanInterval;
    private int damage;
    private float nextAttackTime;
    private float nextScanTime;
    private Transform target;
    private bool guarding;

    // ── HP / гибель (для L0GateDefense, лаба №7(1)) ──
    // R2: ослаблено 60→35 — стража смертнее, орки прорываются.
    public int health = 35;
    public bool IsDead { get; private set; }

    public void Init(Vector3 guardPosition, float moveSpeed, float attackRange, float attackCooldown, float enemyScanInterval, int damage)
    {
        this.guardPosition = guardPosition;
        this.moveSpeed = moveSpeed;
        this.attackRange = attackRange;
        this.attackCooldown = attackCooldown;
        this.enemyScanInterval = enemyScanInterval;
        this.damage = damage;
    }

    public void ApplyDamage(int amount)
    {
        if (IsDead) return;
        health -= amount;
        if (health <= 0) Kill();
    }

    /// Гибель стражника: эффект, заваливание набок, удаление.
    public void Kill()
    {
        if (IsDead) return;
        IsDead = true;

        if (FeedbackManager.Instance != null)
        {
            FeedbackManager.Instance.SpawnBurst(transform.position + Vector3.up,
                new Color(0.7f, 0.1f, 0.08f), new Color(0.4f, 0.05f, 0.04f), 14, 3f);
            FeedbackManager.Instance.FloatText(transform.position + Vector3.up * 1.8f,
                "✝", new Color(0.85f, 0.85f, 0.9f), 1.6f);
        }

        StartCoroutine(DeathFall());
    }

    private IEnumerator DeathFall()
    {
        // Заваливание на спину + проседание в землю, затем удаление.
        Quaternion from = transform.rotation;
        Quaternion to = from * Quaternion.Euler(-82f, 0f, Random.Range(-12f, 12f));
        Vector3 startPos = transform.position;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 1.4f;
            transform.rotation = Quaternion.Slerp(from, to, Mathf.Clamp01(t));
            transform.position = startPos + Vector3.down * (0.4f * Mathf.Clamp01(t));
            yield return null;
        }

        yield return new WaitForSeconds(1.2f);
        Destroy(gameObject);
    }

    private void Update()
    {
        if (IsDead) return;

        GameProgressManager progress = GameProgressManager.Instance;
        if (progress != null && progress.castleGateRaidCleared)
        {
            target = null;
            guarding = true;
            return;
        }

        if (!guarding)
        {
            transform.position = Vector3.MoveTowards(transform.position, guardPosition, moveSpeed * Time.deltaTime);
            Face(guardPosition - transform.position);

            if (Vector3.Distance(transform.position, guardPosition) <= 0.2f)
                guarding = true;

            return;
        }

        if (Time.time >= nextScanTime)
        {
            nextScanTime = Time.time + enemyScanInterval;
            target = FindNearestEnemy();
        }

        if (target == null)
            return;

        Vector3 toTarget = target.position - transform.position;
        Face(toTarget);

        if (toTarget.magnitude <= attackRange && Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackCooldown;
            target.gameObject.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
        }
    }

    private Transform FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Transform best = null;
        float bestSqr = attackRange * attackRange * 5f;

        for (int i = 0; i < enemies.Length; i++)
        {
            GameObject enemy = enemies[i];
            if (enemy == null || !enemy.activeInHierarchy)
                continue;

            float sqr = (enemy.transform.position - transform.position).sqrMagnitude;
            if (sqr < bestSqr)
            {
                bestSqr = sqr;
                best = enemy.transform;
            }
        }

        return best;
    }

    private void Face(Vector3 direction)
    {
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.01f)
            return;

        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction.normalized, Vector3.up), Time.deltaTime * 8f);
    }
}

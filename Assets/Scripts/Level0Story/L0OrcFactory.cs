using UnityEngine;
using UnityEngine.AI;

public class L0OrcFactory : MonoBehaviour
{
    public enum OrcType { Warrior, Archer, Boss, Berserker, Tank, Shaman, Colossus, Wolf, Goblin,
                          Spearman, Necromancer, Warchief, PoisonArcher }

    public float defaultSpeed = 4.2f;
    public int defaultDamage = 14;
    public int defaultHealth = 50;
    public float defaultChaseRange = 28f;
    public float defaultAttackRange = 2.1f;
    public Color skinColor = new Color(0.12f, 0.30f, 0.08f);
    public Color armorColor = new Color(0.22f, 0.22f, 0.24f);

    public static L0OrcFactory Ensure()
    {
        L0OrcFactory factory = FindFirstObjectByType<L0OrcFactory>();
        if (factory != null)
            return factory;

        GameObject obj = new GameObject("L0_ORC_FACTORY");
        return obj.AddComponent<L0OrcFactory>();
    }

    public GameObject CreateWarrior(string objectName, Vector3 position, Transform parent, Transform[] waypoints)
    {
        return CreateOrc(objectName, position, parent, waypoints, false, false);
    }

    public GameObject CreateArcher(string objectName, Vector3 position, Transform parent, Transform[] waypoints)
    {
        return CreateOrc(objectName, position, parent, waypoints, true, false);
    }

    public GameObject CreateBoss(string objectName, Vector3 position, Transform parent, Transform[] waypoints)
    {
        return CreateOrc(objectName, position, parent, waypoints, false, true);
    }

    public GameObject CreateBerserker(string objectName, Vector3 position, Transform parent, Transform[] waypoints)
    {
        return CreateSpecialOrc(objectName, position, parent, waypoints, OrcType.Berserker);
    }

    public GameObject CreateTank(string objectName, Vector3 position, Transform parent, Transform[] waypoints)
    {
        return CreateSpecialOrc(objectName, position, parent, waypoints, OrcType.Tank);
    }

    public GameObject CreateShaman(string objectName, Vector3 position, Transform parent, Transform[] waypoints)
    {
        return CreateSpecialOrc(objectName, position, parent, waypoints, OrcType.Shaman);
    }

    public GameObject CreateColossus(string objectName, Vector3 position, Transform parent, Transform[] waypoints)
    {
        return CreateSpecialOrc(objectName, position, parent, waypoints, OrcType.Colossus);
    }

    public GameObject CreateWolf(string objectName, Vector3 position, Transform parent, Transform[] waypoints)
    {
        return CreateSpecialOrc(objectName, position, parent, waypoints, OrcType.Wolf);
    }

    public GameObject CreateGoblin(string objectName, Vector3 position, Transform parent, Transform[] waypoints)
    {
        return CreateSpecialOrc(objectName, position, parent, waypoints, OrcType.Goblin);
    }

    public GameObject CreateSpearman(string objectName, Vector3 position, Transform parent, Transform[] waypoints)
    {
        return CreateSpecialOrc(objectName, position, parent, waypoints, OrcType.Spearman);
    }

    public GameObject CreateNecromancer(string objectName, Vector3 position, Transform parent, Transform[] waypoints)
    {
        return CreateSpecialOrc(objectName, position, parent, waypoints, OrcType.Necromancer);
    }

    public GameObject CreateWarchief(string objectName, Vector3 position, Transform parent, Transform[] waypoints)
    {
        return CreateSpecialOrc(objectName, position, parent, waypoints, OrcType.Warchief);
    }

    public GameObject CreatePoisonArcher(string objectName, Vector3 position, Transform parent, Transform[] waypoints)
    {
        return CreateSpecialOrc(objectName, position, parent, waypoints, OrcType.PoisonArcher);
    }

    private GameObject CreateSpecialOrc(string objectName, Vector3 position, Transform parent, Transform[] waypoints, OrcType type)
    {
        GameObject root = new GameObject(string.IsNullOrEmpty(objectName) ? "L0_Orc_" + type : objectName);
        root.transform.position = position;
        root.transform.SetParent(parent);
        root.tag = "Enemy";

        bool isTank         = type == OrcType.Tank;
        bool isShaman       = type == OrcType.Shaman;
        bool isBerserker    = type == OrcType.Berserker;
        bool isColossus     = type == OrcType.Colossus;
        bool isWolf         = type == OrcType.Wolf;
        bool isGoblin       = type == OrcType.Goblin;
        bool isSpearman     = type == OrcType.Spearman;
        bool isNecromancer  = type == OrcType.Necromancer;
        bool isWarchief     = type == OrcType.Warchief;
        bool isPoisonArcher = type == OrcType.PoisonArcher;

        float height = isTank ? 2.2f : isColossus ? 2.8f : isWolf ? 0.9f : isGoblin ? 1.2f
                     : isWarchief ? 2.1f : isNecromancer ? 1.7f : 1.8f;
        float radius = isTank ? 0.58f : isColossus ? 0.72f : isWolf ? 0.30f : isGoblin ? 0.28f
                     : isWarchief ? 0.55f : 0.42f;

        CapsuleCollider col = root.AddComponent<CapsuleCollider>();
        col.height = height;
        col.radius = radius;
        col.center = Vector3.up * (height * 0.5f);

        NavMeshAgent agent = root.AddComponent<NavMeshAgent>();
        agent.speed        = isBerserker ? 5.5f : isTank ? 2.2f : isColossus ? 1.6f : isWolf ? 6.5f
                           : isGoblin ? 5.2f : isSpearman ? 4.8f : isNecromancer ? 2.5f
                           : isWarchief ? 3.8f : isPoisonArcher ? 4.0f : 2.8f;
        agent.angularSpeed = isColossus ? 80f : 300f;
        agent.acceleration = isBerserker ? 28f : isColossus ? 8f : isWarchief ? 20f : 14f;
        agent.stoppingDistance = (isShaman || isNecromancer || isPoisonArcher) ? 0.9f : 0.45f;
        agent.height    = col.height;
        agent.radius    = col.radius;
        agent.autoRepath = true;

        EnemyAI ai = root.AddComponent<EnemyAI>();
        ai.maxHp  = isBerserker ? 35 : isTank ? 90 : isColossus ? 220 : isWolf ? 28 : isGoblin ? 22
                  : isSpearman ? 45 : isNecromancer ? 35 : isWarchief ? 120 : isPoisonArcher ? 38 : 40;
        ai.damage = isBerserker ? 18 : isTank ? 20 : isColossus ? 45 : isWolf ? 12 : isGoblin ? 8
                  : isSpearman ? 22 : isWarchief ? 35 : 8;
        ai.chaseRange  = isBerserker ? 32f : isShaman ? 26f : isColossus ? 30f : isWolf ? 35f
                       : isGoblin ? 30f : isNecromancer ? 28f : isWarchief ? 30f : isPoisonArcher ? 32f : defaultChaseRange;
        ai.attackRange = isColossus ? 3.8f : isWolf ? 1.6f : isSpearman ? 3.2f : isWarchief ? 2.8f : defaultAttackRange;
        ai.waypoints   = waypoints ?? new Transform[0];
        ai.useRangedAttack = isShaman || isGoblin || isNecromancer || isPoisonArcher;
        ai.isFinalBoss  = false;
        ai.enemyScoreType = (isColossus || isWarchief || isTank || isBerserker || isNecromancer)
                                                         ? RunScoreManager.EnemyType.Elite
                          : (isShaman || isPoisonArcher) ? RunScoreManager.EnemyType.OrcArcher
                          :                                RunScoreManager.EnemyType.OrcWarrior;

        if (isBerserker || isColossus)  ai.rageHpThreshold = 0.5f;
        if (isWarchief)                  ai.rageHpThreshold = 0.5f;
        if (isWolf)                      ai.rageHpThreshold = 0.99f;
        if (isWolf)                      ai.cooldown = 0.9f;
        if (isGoblin)                    ai.cooldown = 1.0f;
        if (isSpearman)                  ai.cooldown = 1.4f;

        if (isBerserker)     ai.flankRadius = 4.0f;
        if (isWolf)          ai.flankRadius = 5.5f;
        if (isGoblin)        ai.flankRadius = 6.0f;
        if (isWarchief)      ai.flankRadius = 3.5f;
        if (isTank)          ai.flankRadius = 2.0f;
        if (isSpearman)      ai.flankRadius = 4.5f;
        if (isColossus)      ai.flankRadius = 2.5f;

        if (isShaman)
        {
            ai.rangedDamage = 12;
            ai.rangedRange = 22f;
            ai.rangedCooldown = 2.4f;
            ai.rangedProjectileSpeed = 14f;
            ai.rangedProjectileColor = new Color(0.65f, 0.15f, 1f);
            ai.rangedAttackLabel = "CURSE";
            ai.retreatRange = 8f;
        }

        if (isGoblin)
        {
            ai.rangedDamage = 6;
            ai.rangedRange = 18f;
            ai.rangedCooldown = 1.2f;
            ai.rangedProjectileSpeed = 18f;
            ai.rangedProjectileColor = new Color(0.55f, 0.52f, 0.48f);
            ai.rangedAttackLabel = "STONE";
            ai.retreatRange = 8f;
        }

        if (isNecromancer)
        {
            ai.rangedDamage = 14;
            ai.rangedRange = 25f;
            ai.rangedCooldown = 2.0f;
            ai.rangedProjectileSpeed = 16f;
            ai.rangedProjectileColor = new Color(0.15f, 0.85f, 0.20f);
            ai.rangedAttackLabel = "ПРОКЛЯТИЕ";
            ai.retreatRange = 12f;
        }

        if (isPoisonArcher)
        {
            ai.rangedDamage = 8;
            ai.rangedRange = 28f;
            ai.rangedCooldown = 1.0f;
            ai.rangedProjectileSpeed = 28f;
            ai.rangedProjectileColor = new Color(0.30f, 0.80f, 0.15f);
            ai.rangedAttackLabel = "ЯД";
            ai.retreatRange = 14f;
        }

        if (isSpearman) ai.retreatRange = 2.5f;

        root.AddComponent<EnemyHitDetector>();

        // Спецспособности
        if (isBerserker)
        {
            var ab = root.AddComponent<OrcSpecialAbility>();
            ab.abilityType = OrcSpecialAbility.AbilityType.BattleCry;
        }
        else if (isShaman)
        {
            var ab = root.AddComponent<OrcSpecialAbility>();
            ab.abilityType = OrcSpecialAbility.AbilityType.HealPulse;
        }
        else if (isWolf)
        {
            var ab = root.AddComponent<OrcSpecialAbility>();
            ab.abilityType = OrcSpecialAbility.AbilityType.PackHowl;
        }
        else if (isSpearman)
        {
            var ab = root.AddComponent<OrcSpecialAbility>();
            ab.abilityType = OrcSpecialAbility.AbilityType.SpearThrow;
        }
        else if (isNecromancer)
        {
            var ab = root.AddComponent<OrcSpecialAbility>();
            ab.abilityType = OrcSpecialAbility.AbilityType.RaiseDead;
        }
        else if (isWarchief)
        {
            var ab = root.AddComponent<OrcSpecialAbility>();
            ab.abilityType = OrcSpecialAbility.AbilityType.WarCry;
        }
        else if (isPoisonArcher)
        {
            var ab = root.AddComponent<OrcSpecialAbility>();
            ab.abilityType = OrcSpecialAbility.AbilityType.PoisonCloud;
        }

        BuildSpecialVisual(root.transform, type, ai);

        var anim = root.AddComponent<L0OrcAnimator>();
        anim.orcType = type;
        return root;
    }

    public GameObject CreateOrc(string objectName, Vector3 position, Transform parent, Transform[] waypoints, bool archer, bool boss)
    {
        GameObject root = new GameObject(string.IsNullOrEmpty(objectName) ? "L0_Orc" : objectName);
        root.transform.position = position;
        root.transform.SetParent(parent);
        root.tag = "Enemy";

        CapsuleCollider collider = root.AddComponent<CapsuleCollider>();
        collider.height = boss ? 2.4f : 1.8f;
        collider.radius = boss ? 0.60f : 0.44f;
        collider.center = Vector3.up * (collider.height * 0.5f);

        NavMeshAgent agent = root.AddComponent<NavMeshAgent>();
        agent.speed = boss ? defaultSpeed * 1.15f : defaultSpeed;
        agent.angularSpeed = 300f;
        agent.acceleration = 18f;
        agent.stoppingDistance = archer ? 0.9f : 0.45f;
        agent.height = collider.height;
        agent.radius = collider.radius;
        agent.autoRepath = true;

        EnemyAI ai = root.AddComponent<EnemyAI>();
        ai.damage = boss ? defaultDamage + 8 : defaultDamage;
        ai.maxHp = boss ? defaultHealth * 2 : defaultHealth;
        ai.chaseRange = archer ? defaultChaseRange + 5f : defaultChaseRange;
        ai.attackRange = defaultAttackRange;
        ai.waypoints = waypoints ?? new Transform[0];
        ai.useRangedAttack = archer;
        ai.isFinalBoss = false;
        ai.enemyScoreType = boss   ? RunScoreManager.EnemyType.Boss
                          : archer ? RunScoreManager.EnemyType.OrcArcher
                                   : RunScoreManager.EnemyType.OrcWarrior;

        if (archer)
        {
            ai.rangedDamage = 10;
            ai.rangedRange = 24f;
            ai.rangedCooldown = 1.6f;
            ai.rangedProjectileSpeed = 22f;
            ai.rangedProjectileColor = new Color(1f, 0.70f, 0.12f);
            ai.rangedAttackLabel = "SHOT";
            ai.retreatRange = 10f;
        }
        else if (boss)
        {
            ai.flankRadius = 2.5f;
        }
        else
        {
            ai.flankRadius = 3.5f;
        }

        root.AddComponent<EnemyHitDetector>();

        if (boss)
        {
            var ab = root.AddComponent<OrcSpecialAbility>();
            ab.abilityType = OrcSpecialAbility.AbilityType.GroundSlam;
        }

        BuildVisual(root.transform, archer, boss, ai);

        if (boss)
        {
            var anim = root.AddComponent<L0OrcAnimator>();
            anim.orcType = OrcType.Boss;
        }

        return root;
    }

    private void BuildVisual(Transform root, bool archer, bool boss, EnemyAI ai)
    {
        Color skin = boss ? new Color(0.62f, 0.10f, 0.06f) : skinColor;
        Color armor = boss ? new Color(0.35f, 0.06f, 0.04f) : armorColor;
        Color leather = new Color(0.25f, 0.14f, 0.06f);
        Color metal = boss ? new Color(0.28f, 0.08f, 0.04f) : new Color(0.18f, 0.18f, 0.20f);
        Color bone = new Color(0.72f, 0.68f, 0.58f);
        Color cloth = boss ? new Color(0.5f, 0.08f, 0.05f) : new Color(0.14f, 0.10f, 0.08f);

        float scale = boss ? 2.30f : archer ? 1.30f : 1.45f;
        root.localScale = Vector3.one * scale;

        // --- Тело (массивный торс, выпуклая грудь) ---
        GameObject torso = Prim(PrimitiveType.Cube, root, "Torso",
            new Vector3(0f, 0.78f, 0.06f), new Vector3(0.82f, 0.72f, 0.58f), skin);
        Prim(PrimitiveType.Sphere, root, "Belly",
            new Vector3(0f, 0.62f, 0.18f), new Vector3(0.65f, 0.48f, 0.52f), skin);

        // --- Голова (крупная, с челюстью) ---
        GameObject head = Prim(PrimitiveType.Cube, root, "Head",
            new Vector3(0f, 1.28f, 0.10f), new Vector3(0.52f, 0.48f, 0.50f), skin);
        Prim(PrimitiveType.Cube, root, "Jaw",
            new Vector3(0f, 1.08f, 0.22f), new Vector3(0.40f, 0.16f, 0.28f), skin);

        // --- Глаза (маленькие, жёлто-красные, злобные) ---
        Color eyeColor = boss ? new Color(1f, 0.2f, 0.05f) : new Color(0.9f, 0.7f, 0.1f);
        Prim(PrimitiveType.Sphere, root, "Eye_L",
            new Vector3(0.14f, 1.32f, 0.28f), new Vector3(0.09f, 0.07f, 0.06f), eyeColor);
        Prim(PrimitiveType.Sphere, root, "Eye_R",
            new Vector3(-0.14f, 1.32f, 0.28f), new Vector3(0.09f, 0.07f, 0.06f), eyeColor);

        // --- Нагрудник / броня ---
        Prim(PrimitiveType.Cube, root, "ChestPlate",
            new Vector3(0f, 0.82f, 0.30f), new Vector3(0.68f, 0.52f, 0.12f), metal);
        Prim(PrimitiveType.Cube, root, "Belt",
            new Vector3(0f, 0.50f, 0.12f), new Vector3(0.76f, 0.12f, 0.52f), leather);

        // --- Наплечники ---
        Prim(PrimitiveType.Sphere, root, "Shoulder_L",
            new Vector3(0.52f, 1.02f, 0.06f), new Vector3(0.28f, 0.22f, 0.26f), metal);
        Prim(PrimitiveType.Sphere, root, "Shoulder_R",
            new Vector3(-0.52f, 1.02f, 0.06f), new Vector3(0.28f, 0.22f, 0.26f), metal);

        // --- Руки (толстые, мускулистые) ---
        Prim(PrimitiveType.Cylinder, root, "UpperArm_L",
            new Vector3(0.58f, 0.82f, 0.06f), new Vector3(0.16f, 0.28f, 0.16f), skin);
        Prim(PrimitiveType.Cylinder, root, "ForeArm_L",
            new Vector3(0.60f, 0.42f, 0.14f), new Vector3(0.14f, 0.26f, 0.14f), skin);
        Prim(PrimitiveType.Cylinder, root, "UpperArm_R",
            new Vector3(-0.58f, 0.82f, 0.06f), new Vector3(0.16f, 0.28f, 0.16f), skin);
        Prim(PrimitiveType.Cylinder, root, "ForeArm_R",
            new Vector3(-0.60f, 0.42f, 0.14f), new Vector3(0.14f, 0.26f, 0.14f), skin);

        // --- Наручи ---
        Prim(PrimitiveType.Cube, root, "Bracer_L",
            new Vector3(0.60f, 0.44f, 0.14f), new Vector3(0.18f, 0.16f, 0.18f), leather);
        Prim(PrimitiveType.Cube, root, "Bracer_R",
            new Vector3(-0.60f, 0.44f, 0.14f), new Vector3(0.18f, 0.16f, 0.18f), leather);

        // --- Ноги (толстые, с поножами) ---
        Prim(PrimitiveType.Cylinder, root, "Leg_L",
            new Vector3(0.22f, 0.18f, 0f), new Vector3(0.18f, 0.36f, 0.18f), armor);
        Prim(PrimitiveType.Cylinder, root, "Leg_R",
            new Vector3(-0.22f, 0.18f, 0f), new Vector3(0.18f, 0.36f, 0.18f), armor);
        Prim(PrimitiveType.Cube, root, "Boot_L",
            new Vector3(0.22f, -0.06f, 0.04f), new Vector3(0.20f, 0.14f, 0.28f), leather);
        Prim(PrimitiveType.Cube, root, "Boot_R",
            new Vector3(-0.22f, -0.06f, 0.04f), new Vector3(0.20f, 0.14f, 0.28f), leather);

        // --- Набедренная тряпка ---
        Prim(PrimitiveType.Cube, root, "Loincloth",
            new Vector3(0f, 0.38f, 0.14f), new Vector3(0.60f, 0.18f, 0.06f), cloth);

        // --- Шлем / голова босса ---
        if (boss)
        {
            Prim(PrimitiveType.Cube, root, "Helmet",
                new Vector3(0f, 1.46f, 0.06f), new Vector3(0.60f, 0.24f, 0.58f), metal);
            Prim(PrimitiveType.Cube, root, "Horn_L",
                new Vector3(0.32f, 1.68f, -0.06f), new Vector3(0.08f, 0.40f, 0.08f), bone);
            Prim(PrimitiveType.Cube, root, "Horn_R",
                new Vector3(-0.32f, 1.68f, -0.06f), new Vector3(0.08f, 0.40f, 0.08f), bone);
            Prim(PrimitiveType.Cube, root, "HornTip_L",
                new Vector3(0.36f, 1.90f, -0.10f), new Vector3(0.05f, 0.14f, 0.05f), new Color(0.85f, 0.80f, 0.68f));
            Prim(PrimitiveType.Cube, root, "HornTip_R",
                new Vector3(-0.36f, 1.90f, -0.10f), new Vector3(0.05f, 0.14f, 0.05f), new Color(0.85f, 0.80f, 0.68f));
        }

        // --- Оружие ---
        if (archer)
        {
            Prim(PrimitiveType.Cylinder, root, "Bow",
                new Vector3(-0.72f, 0.72f, 0.22f), new Vector3(0.04f, 0.68f, 0.04f), leather);
            Prim(PrimitiveType.Cylinder, root, "Quiver",
                new Vector3(0.30f, 0.90f, -0.22f), new Vector3(0.10f, 0.32f, 0.10f), leather);
            Prim(PrimitiveType.Cube, root, "Arrow1",
                new Vector3(0.28f, 1.18f, -0.22f), new Vector3(0.02f, 0.24f, 0.02f), bone);
            Prim(PrimitiveType.Cube, root, "Arrow2",
                new Vector3(0.32f, 1.16f, -0.22f), new Vector3(0.02f, 0.22f, 0.02f), bone);
        }
        else
        {
            Color axeHead = boss ? new Color(0.30f, 0.04f, 0.02f) : metal;
            Prim(PrimitiveType.Cylinder, root, "AxeHandle",
                new Vector3(-0.72f, 0.65f, 0.22f), new Vector3(0.06f, 0.58f, 0.06f), leather);
            Prim(PrimitiveType.Cube, root, "AxeHead",
                new Vector3(-0.72f, 1.18f, 0.22f), boss ? new Vector3(0.38f, 0.30f, 0.08f) : new Vector3(0.30f, 0.24f, 0.06f), axeHead);
            if (!boss)
            {
                Prim(PrimitiveType.Cube, root, "Shield",
                    new Vector3(0.65f, 0.68f, 0.20f), new Vector3(0.06f, 0.52f, 0.38f), metal);
                Prim(PrimitiveType.Sphere, root, "ShieldBoss",
                    new Vector3(0.68f, 0.70f, 0.40f), new Vector3(0.10f, 0.10f, 0.06f), bone);
            }
        }

        if (ai != null)
            ai.bodyParts = new GameObject[] { torso, head };
    }

    private void BuildSpecialVisual(Transform root, OrcType type, EnemyAI ai)
    {
        switch (type)
        {
            case OrcType.Berserker:    BuildBerserkerVisual(root, ai);    break;
            case OrcType.Tank:         BuildTankVisual(root, ai);         break;
            case OrcType.Shaman:       BuildShamanVisual(root, ai);       break;
            case OrcType.Colossus:     BuildColossusVisual(root, ai);     break;
            case OrcType.Wolf:         BuildWolfVisual(root, ai);         break;
            case OrcType.Goblin:       BuildGoblinVisual(root, ai);       break;
            case OrcType.Spearman:     BuildSpearmanVisual(root, ai);     break;
            case OrcType.Necromancer:  BuildNecromancerVisual(root, ai);  break;
            case OrcType.Warchief:     BuildWarchiefVisual(root, ai);     break;
            case OrcType.PoisonArcher: BuildPoisonArcherVisual(root, ai); break;
        }
    }

    private void BuildBerserkerVisual(Transform root, EnemyAI ai)
    {
        Color skin    = new Color(0.10f, 0.38f, 0.08f);
        Color leather = new Color(0.22f, 0.12f, 0.05f);
        Color metal   = new Color(0.20f, 0.20f, 0.22f);
        Color eye     = new Color(1f, 0.12f, 0.04f);
        Color axe     = new Color(0.25f, 0.25f, 0.28f);
        Color warpaint= new Color(0.72f, 0.05f, 0.04f);

        root.localScale = new Vector3(1.40f, 1.55f, 1.40f);

        // Torso — lean, bare-chested
        GameObject torso = Prim(PrimitiveType.Cube, root, "Torso",
            new Vector3(0f, 0.78f, 0.04f), new Vector3(0.70f, 0.68f, 0.50f), skin);
        Prim(PrimitiveType.Sphere, root, "Belly",
            new Vector3(0f, 0.60f, 0.12f), new Vector3(0.56f, 0.44f, 0.42f), skin);

        // Head
        GameObject head = Prim(PrimitiveType.Cube, root, "Head",
            new Vector3(0f, 1.26f, 0.08f), new Vector3(0.50f, 0.46f, 0.48f), skin);
        Prim(PrimitiveType.Cube, root, "Jaw",
            new Vector3(0f, 1.07f, 0.20f), new Vector3(0.38f, 0.14f, 0.26f), skin);

        // Red glowing eyes
        Prim(PrimitiveType.Sphere, root, "Eye_L",
            new Vector3(0.13f, 1.31f, 0.27f), new Vector3(0.10f, 0.08f, 0.07f), eye);
        Prim(PrimitiveType.Sphere, root, "Eye_R",
            new Vector3(-0.13f, 1.31f, 0.27f), new Vector3(0.10f, 0.08f, 0.07f), eye);

        // Red war-paint stripes across face
        Prim(PrimitiveType.Cube, root, "FacePaint",
            new Vector3(0f, 1.27f, 0.28f), new Vector3(0.36f, 0.05f, 0.03f), warpaint);
        Prim(PrimitiveType.Cube, root, "FacePaint2",
            new Vector3(0.10f, 1.20f, 0.27f), new Vector3(0.12f, 0.04f, 0.03f), warpaint);

        // Spike shoulders (no pauldrons, just bone spikes)
        Prim(PrimitiveType.Cube, root, "Spike_L",
            new Vector3(0.48f, 1.06f, 0.02f), new Vector3(0.08f, 0.24f, 0.08f), metal);
        Prim(PrimitiveType.Cube, root, "Spike_R",
            new Vector3(-0.48f, 1.06f, 0.02f), new Vector3(0.08f, 0.24f, 0.08f), metal);

        // Belt only
        Prim(PrimitiveType.Cube, root, "Belt",
            new Vector3(0f, 0.48f, 0.10f), new Vector3(0.72f, 0.10f, 0.50f), leather);
        Prim(PrimitiveType.Cube, root, "Loincloth",
            new Vector3(0f, 0.36f, 0.12f), new Vector3(0.56f, 0.16f, 0.05f), leather);

        // Arms
        Prim(PrimitiveType.Cylinder, root, "UpperArm_L",
            new Vector3(0.55f, 0.82f, 0.04f), new Vector3(0.15f, 0.27f, 0.15f), skin);
        Prim(PrimitiveType.Cylinder, root, "ForeArm_L",
            new Vector3(0.58f, 0.44f, 0.12f), new Vector3(0.13f, 0.25f, 0.13f), skin);
        Prim(PrimitiveType.Cylinder, root, "UpperArm_R",
            new Vector3(-0.55f, 0.82f, 0.04f), new Vector3(0.15f, 0.27f, 0.15f), skin);
        Prim(PrimitiveType.Cylinder, root, "ForeArm_R",
            new Vector3(-0.58f, 0.44f, 0.12f), new Vector3(0.13f, 0.25f, 0.13f), skin);

        // Legs
        Prim(PrimitiveType.Cylinder, root, "Leg_L",
            new Vector3(0.20f, 0.17f, 0f), new Vector3(0.17f, 0.34f, 0.17f), leather);
        Prim(PrimitiveType.Cylinder, root, "Leg_R",
            new Vector3(-0.20f, 0.17f, 0f), new Vector3(0.17f, 0.34f, 0.17f), leather);
        Prim(PrimitiveType.Cube, root, "Boot_L",
            new Vector3(0.20f, -0.05f, 0.04f), new Vector3(0.19f, 0.12f, 0.26f), leather);
        Prim(PrimitiveType.Cube, root, "Boot_R",
            new Vector3(-0.20f, -0.05f, 0.04f), new Vector3(0.19f, 0.12f, 0.26f), leather);

        // TWO axes (berserker rage) — oversized, menacing
        Prim(PrimitiveType.Cylinder, root, "AxeHandle_L",
            new Vector3(0.68f, 0.65f, 0.20f), new Vector3(0.06f, 0.56f, 0.06f), leather);
        Prim(PrimitiveType.Cube, root, "AxeHead_L",
            new Vector3(0.68f, 1.14f, 0.20f), new Vector3(0.34f, 0.26f, 0.06f), axe);
        Prim(PrimitiveType.Cylinder, root, "AxeHandle_R",
            new Vector3(-0.68f, 0.65f, 0.20f), new Vector3(0.06f, 0.56f, 0.06f), leather);
        Prim(PrimitiveType.Cube, root, "AxeHead_R",
            new Vector3(-0.68f, 1.14f, 0.20f), new Vector3(0.34f, 0.26f, 0.06f), axe);

        if (ai != null) ai.bodyParts = new GameObject[] { torso, head };
        var anim = root.GetComponent<L0OrcAnimator>();
        if (anim != null) anim.headTransform = head.transform;
    }

    private void BuildTankVisual(Transform root, EnemyAI ai)
    {
        Color skin   = new Color(0.18f, 0.36f, 0.14f);
        Color metal  = new Color(0.14f, 0.14f, 0.16f);
        Color darkM  = new Color(0.10f, 0.10f, 0.12f);
        Color leather= new Color(0.24f, 0.13f, 0.05f);
        Color eye    = new Color(0.85f, 0.65f, 0.10f);
        Color bone   = new Color(0.70f, 0.66f, 0.56f);

        root.localScale = new Vector3(2.80f, 2.80f, 2.80f);

        // Wide heavy torso
        GameObject torso = Prim(PrimitiveType.Cube, root, "Torso",
            new Vector3(0f, 0.78f, 0.04f), new Vector3(0.92f, 0.74f, 0.60f), skin);
        Prim(PrimitiveType.Sphere, root, "Belly",
            new Vector3(0f, 0.62f, 0.18f), new Vector3(0.76f, 0.50f, 0.54f), skin);

        // Head with heavy helmet
        GameObject head = Prim(PrimitiveType.Cube, root, "Head",
            new Vector3(0f, 1.28f, 0.08f), new Vector3(0.54f, 0.50f, 0.52f), skin);
        Prim(PrimitiveType.Cube, root, "Jaw",
            new Vector3(0f, 1.08f, 0.22f), new Vector3(0.42f, 0.16f, 0.28f), skin);
        Prim(PrimitiveType.Cube, root, "Helmet",
            new Vector3(0f, 1.46f, 0.04f), new Vector3(0.60f, 0.22f, 0.56f), darkM);
        Prim(PrimitiveType.Cube, root, "HelmetVisor",
            new Vector3(0f, 1.30f, 0.30f), new Vector3(0.46f, 0.14f, 0.08f), darkM);

        // Eyes visible through visor
        Prim(PrimitiveType.Sphere, root, "Eye_L",
            new Vector3(0.14f, 1.33f, 0.30f), new Vector3(0.08f, 0.06f, 0.05f), eye);
        Prim(PrimitiveType.Sphere, root, "Eye_R",
            new Vector3(-0.14f, 1.33f, 0.30f), new Vector3(0.08f, 0.06f, 0.05f), eye);

        // Full plate armor
        Prim(PrimitiveType.Cube, root, "ChestPlate",
            new Vector3(0f, 0.82f, 0.30f), new Vector3(0.76f, 0.56f, 0.12f), darkM);
        Prim(PrimitiveType.Sphere, root, "Shoulder_L",
            new Vector3(0.58f, 1.04f, 0.04f), new Vector3(0.34f, 0.26f, 0.30f), metal);
        Prim(PrimitiveType.Sphere, root, "Shoulder_R",
            new Vector3(-0.58f, 1.04f, 0.04f), new Vector3(0.34f, 0.26f, 0.30f), metal);
        Prim(PrimitiveType.Cube, root, "Belt",
            new Vector3(0f, 0.50f, 0.10f), new Vector3(0.84f, 0.14f, 0.54f), leather);

        // Arms
        Prim(PrimitiveType.Cylinder, root, "UpperArm_L",
            new Vector3(0.62f, 0.82f, 0.04f), new Vector3(0.18f, 0.30f, 0.18f), skin);
        Prim(PrimitiveType.Cylinder, root, "ForeArm_L",
            new Vector3(0.64f, 0.42f, 0.14f), new Vector3(0.16f, 0.28f, 0.16f), skin);
        Prim(PrimitiveType.Cylinder, root, "UpperArm_R",
            new Vector3(-0.62f, 0.82f, 0.04f), new Vector3(0.18f, 0.30f, 0.18f), skin);
        Prim(PrimitiveType.Cylinder, root, "ForeArm_R",
            new Vector3(-0.64f, 0.42f, 0.14f), new Vector3(0.16f, 0.28f, 0.16f), skin);

        // Legs — armored
        Prim(PrimitiveType.Cylinder, root, "Leg_L",
            new Vector3(0.24f, 0.18f, 0f), new Vector3(0.20f, 0.38f, 0.20f), metal);
        Prim(PrimitiveType.Cylinder, root, "Leg_R",
            new Vector3(-0.24f, 0.18f, 0f), new Vector3(0.20f, 0.38f, 0.20f), metal);
        Prim(PrimitiveType.Cube, root, "Boot_L",
            new Vector3(0.24f, -0.06f, 0.04f), new Vector3(0.22f, 0.16f, 0.30f), darkM);
        Prim(PrimitiveType.Cube, root, "Boot_R",
            new Vector3(-0.24f, -0.06f, 0.04f), new Vector3(0.22f, 0.16f, 0.30f), darkM);

        // HUGE tower shield on left arm
        Prim(PrimitiveType.Cube, root, "Shield",
            new Vector3(0.68f, 0.72f, 0.28f), new Vector3(0.08f, 0.90f, 0.65f), darkM);
        Prim(PrimitiveType.Sphere, root, "ShieldBoss",
            new Vector3(0.72f, 0.72f, 0.58f), new Vector3(0.16f, 0.16f, 0.10f), bone);
        Prim(PrimitiveType.Cube, root, "ShieldRim_H",
            new Vector3(0.72f, 0.72f, 0.28f), new Vector3(0.09f, 0.04f, 0.67f), metal);
        Prim(PrimitiveType.Cube, root, "ShieldRim_V",
            new Vector3(0.72f, 0.72f, 0.28f), new Vector3(0.09f, 0.92f, 0.04f), metal);

        // Mace in right hand — heavy
        Prim(PrimitiveType.Cylinder, root, "MaceHandle",
            new Vector3(-0.70f, 0.62f, 0.20f), new Vector3(0.07f, 0.58f, 0.07f), leather);
        Prim(PrimitiveType.Sphere, root, "MaceHead",
            new Vector3(-0.70f, 1.18f, 0.20f), new Vector3(0.28f, 0.28f, 0.28f), darkM);

        if (ai != null) ai.bodyParts = new GameObject[] { torso, head };
        var anim = root.GetComponent<L0OrcAnimator>();
        if (anim != null)
        {
            anim.shieldTransform = root.Find("Shield");
        }
    }

    private void BuildShamanVisual(Transform root, EnemyAI ai)
    {
        Color skin    = new Color(0.32f, 0.20f, 0.38f);
        Color robe    = new Color(0.18f, 0.08f, 0.28f);
        Color robeAcc = new Color(0.45f, 0.12f, 0.60f);
        Color bone    = new Color(0.68f, 0.64f, 0.54f);
        Color eye     = new Color(0.75f, 0.20f, 1f);
        Color orb     = new Color(0.55f, 0.10f, 1f);

        root.localScale = Vector3.one * 1.10f;

        // Frail torso — robe covers it
        GameObject torso = Prim(PrimitiveType.Cube, root, "Torso",
            new Vector3(0f, 0.80f, 0.02f), new Vector3(0.62f, 0.66f, 0.48f), skin);

        // Wide robe skirt (no visible legs)
        Prim(PrimitiveType.Cube, root, "Robe_Upper",
            new Vector3(0f, 0.78f, 0.06f), new Vector3(0.72f, 0.70f, 0.56f), robe);
        Prim(PrimitiveType.Cube, root, "Robe_Lower",
            new Vector3(0f, 0.34f, 0.06f), new Vector3(0.78f, 0.50f, 0.56f), robe);
        Prim(PrimitiveType.Cube, root, "Robe_Hem",
            new Vector3(0f, 0.06f, 0.06f), new Vector3(0.82f, 0.20f, 0.58f), robe);
        // Purple trim
        Prim(PrimitiveType.Cube, root, "RobeTrim",
            new Vector3(0f, 0.50f, 0.30f), new Vector3(0.60f, 0.06f, 0.04f), robeAcc);

        // Head — hooded
        GameObject head = Prim(PrimitiveType.Sphere, root, "Head",
            new Vector3(0f, 1.28f, 0.06f), new Vector3(0.48f, 0.52f, 0.50f), skin);
        Prim(PrimitiveType.Cube, root, "Hood",
            new Vector3(0f, 1.42f, -0.04f), new Vector3(0.56f, 0.34f, 0.54f), robe);
        Prim(PrimitiveType.Cube, root, "HoodBrim",
            new Vector3(0f, 1.28f, 0.16f), new Vector3(0.50f, 0.08f, 0.36f), robe);

        // Glowing purple eyes
        Prim(PrimitiveType.Sphere, root, "Eye_L",
            new Vector3(0.12f, 1.30f, 0.26f), new Vector3(0.09f, 0.08f, 0.06f), eye);
        Prim(PrimitiveType.Sphere, root, "Eye_R",
            new Vector3(-0.12f, 1.30f, 0.26f), new Vector3(0.09f, 0.08f, 0.06f), eye);

        // Thin arms
        Prim(PrimitiveType.Cylinder, root, "Arm_L",
            new Vector3(0.46f, 0.74f, 0.08f), new Vector3(0.11f, 0.44f, 0.11f), skin);
        Prim(PrimitiveType.Cylinder, root, "Arm_R",
            new Vector3(-0.46f, 0.74f, 0.08f), new Vector3(0.11f, 0.44f, 0.11f), skin);

        // Staff in left hand — tall gnarled
        Prim(PrimitiveType.Cylinder, root, "StaffShaft",
            new Vector3(0.58f, 0.60f, 0.16f), new Vector3(0.06f, 1.0f, 0.06f), bone);
        Prim(PrimitiveType.Cube, root, "StaffTop",
            new Vector3(0.58f, 1.58f, 0.16f), new Vector3(0.18f, 0.18f, 0.08f), bone);
        // Floating magic orb above staff — large, glowing
        Prim(PrimitiveType.Sphere, root, "MagicOrb",
            new Vector3(0.58f, 1.82f, 0.14f), new Vector3(0.26f, 0.26f, 0.26f), orb);

        // Skull ornament on belt
        Prim(PrimitiveType.Sphere, root, "Skull",
            new Vector3(0f, 0.52f, 0.30f), new Vector3(0.14f, 0.12f, 0.11f), bone);
        Prim(PrimitiveType.Cube, root, "SkullEye_L",
            new Vector3(0.04f, 0.54f, 0.37f), new Vector3(0.03f, 0.03f, 0.02f), robe);
        Prim(PrimitiveType.Cube, root, "SkullEye_R",
            new Vector3(-0.04f, 0.54f, 0.37f), new Vector3(0.03f, 0.03f, 0.02f), robe);

        if (ai != null) ai.bodyParts = new GameObject[] { torso, head };
        var anim = root.GetComponent<L0OrcAnimator>();
        if (anim != null)
        {
            anim.orbTransform = root.Find("MagicOrb");
        }
    }

    private void BuildColossusVisual(Transform root, EnemyAI ai)
    {
        Color skin  = new Color(0.10f, 0.22f, 0.06f); // почти чёрно-зелёный
        Color crude = new Color(0.26f, 0.12f, 0.05f); // ржавая кожа
        Color darkM = new Color(0.12f, 0.12f, 0.14f); // грубый металл
        Color bone  = new Color(0.65f, 0.60f, 0.50f);
        Color eye   = new Color(1.00f, 0.45f, 0.05f); // горящий оранжевый
        Color club  = new Color(0.22f, 0.14f, 0.07f); // тёмное дерево

        root.localScale = new Vector3(3.8f, 3.8f, 3.8f);

        // Невозможно широкий торс — сутулый
        GameObject torso = Prim(PrimitiveType.Cube, root, "Torso",
            new Vector3(0f, 0.82f, 0.04f), new Vector3(1.02f, 0.84f, 0.72f), skin);
        Prim(PrimitiveType.Sphere, root, "Belly",
            new Vector3(0f, 0.65f, 0.22f), new Vector3(0.88f, 0.60f, 0.62f), skin);
        // Нагрудные костяные пластины
        Prim(PrimitiveType.Cube, root, "Plate_L",
            new Vector3( 0.22f, 0.90f, 0.34f), new Vector3(0.30f, 0.44f, 0.12f), darkM);
        Prim(PrimitiveType.Cube, root, "Plate_R",
            new Vector3(-0.22f, 0.90f, 0.34f), new Vector3(0.30f, 0.44f, 0.12f), darkM);
        Prim(PrimitiveType.Cube, root, "Belt",
            new Vector3(0f, 0.50f, 0.12f), new Vector3(0.94f, 0.16f, 0.62f), crude);

        // Толстая короткая шея — голова кажется маленькой, тело — огромным
        Prim(PrimitiveType.Cylinder, root, "Neck",
            new Vector3(0f, 1.18f, 0.04f), new Vector3(0.32f, 0.18f, 0.32f), skin);

        // Голова относительно небольшая → контраст с телом делает его монстром
        GameObject head = Prim(PrimitiveType.Cube, root, "Head",
            new Vector3(0f, 1.40f, 0.06f), new Vector3(0.50f, 0.46f, 0.48f), skin);
        Prim(PrimitiveType.Cube, root, "Jaw",
            new Vector3(0f, 1.20f, 0.22f), new Vector3(0.38f, 0.15f, 0.26f), skin);

        // Крошечные горящие оранжевые глаза
        Prim(PrimitiveType.Sphere, root, "Eye_L",
            new Vector3( 0.13f, 1.44f, 0.27f), new Vector3(0.09f, 0.07f, 0.06f), eye);
        Prim(PrimitiveType.Sphere, root, "Eye_R",
            new Vector3(-0.13f, 1.44f, 0.27f), new Vector3(0.09f, 0.07f, 0.06f), eye);

        // Сломанные костяные рога
        Prim(PrimitiveType.Cube, root, "Horn_L",
            new Vector3( 0.20f, 1.64f, -0.04f), new Vector3(0.07f, 0.24f, 0.07f), bone);
        Prim(PrimitiveType.Cube, root, "Horn_R",
            new Vector3(-0.20f, 1.64f, -0.04f), new Vector3(0.07f, 0.24f, 0.07f), bone);

        // Плечи — как валуны
        Prim(PrimitiveType.Sphere, root, "Shoulder_L",
            new Vector3( 0.64f, 1.08f, 0.04f), new Vector3(0.46f, 0.38f, 0.44f), darkM);
        Prim(PrimitiveType.Sphere, root, "Shoulder_R",
            new Vector3(-0.64f, 1.08f, 0.04f), new Vector3(0.46f, 0.38f, 0.44f), darkM);

        // Руки — как стволы деревьев
        Prim(PrimitiveType.Cylinder, root, "UpperArm_L",
            new Vector3( 0.66f, 0.78f, 0.04f), new Vector3(0.24f, 0.35f, 0.24f), skin);
        Prim(PrimitiveType.Cylinder, root, "ForeArm_L",
            new Vector3( 0.68f, 0.36f, 0.14f), new Vector3(0.22f, 0.30f, 0.22f), skin);
        Prim(PrimitiveType.Cylinder, root, "UpperArm_R",
            new Vector3(-0.66f, 0.78f, 0.04f), new Vector3(0.24f, 0.35f, 0.24f), skin);
        Prim(PrimitiveType.Cylinder, root, "ForeArm_R",
            new Vector3(-0.68f, 0.36f, 0.14f), new Vector3(0.22f, 0.30f, 0.22f), skin);
        // Костяные обмотки на руках
        Prim(PrimitiveType.Cube, root, "Wrap_L",
            new Vector3( 0.66f, 0.42f, 0.14f), new Vector3(0.30f, 0.10f, 0.30f), bone);
        Prim(PrimitiveType.Cube, root, "Wrap_R",
            new Vector3(-0.66f, 0.42f, 0.14f), new Vector3(0.30f, 0.10f, 0.30f), bone);

        // Короткие массивные ноги — низкий центр тяжести
        Prim(PrimitiveType.Cylinder, root, "Leg_L",
            new Vector3( 0.28f, 0.22f, 0f), new Vector3(0.28f, 0.44f, 0.28f), skin);
        Prim(PrimitiveType.Cylinder, root, "Leg_R",
            new Vector3(-0.28f, 0.22f, 0f), new Vector3(0.28f, 0.44f, 0.28f), skin);
        Prim(PrimitiveType.Cube, root, "Boot_L",
            new Vector3( 0.28f, -0.06f, 0.06f), new Vector3(0.34f, 0.20f, 0.42f), crude);
        Prim(PrimitiveType.Cube, root, "Boot_R",
            new Vector3(-0.28f, -0.06f, 0.06f), new Vector3(0.34f, 0.20f, 0.42f), crude);

        // ОГРОМНАЯ боевая дубина — ствол дерева с железными шипами (левая рука)
        Prim(PrimitiveType.Cylinder, root, "ClubShaft",
            new Vector3(-0.72f, 0.55f, 0.18f), new Vector3(0.14f, 0.70f, 0.14f), club);
        Prim(PrimitiveType.Cube, root, "ClubHead",
            new Vector3(-0.72f, 1.24f, 0.18f), new Vector3(0.42f, 0.40f, 0.42f), darkM);
        // Шипы на дубине
        Prim(PrimitiveType.Sphere, root, "Spike_F",
            new Vector3(-0.72f, 1.28f, 0.42f), new Vector3(0.09f, 0.09f, 0.07f), bone);
        Prim(PrimitiveType.Sphere, root, "Spike_L",
            new Vector3(-0.92f, 1.28f, 0.18f), new Vector3(0.09f, 0.09f, 0.07f), bone);
        Prim(PrimitiveType.Sphere, root, "Spike_R",
            new Vector3(-0.52f, 1.28f, 0.18f), new Vector3(0.09f, 0.09f, 0.07f), bone);
        Prim(PrimitiveType.Sphere, root, "Spike_T",
            new Vector3(-0.72f, 1.46f, 0.18f), new Vector3(0.07f, 0.12f, 0.07f), bone);

        if (ai != null) ai.bodyParts = new GameObject[] { torso, head };

        var anim = root.GetComponent<L0OrcAnimator>();
        if (anim != null)
            anim.colossusBodyTransform = torso.transform;
    }

    private void BuildWolfVisual(Transform root, EnemyAI ai)
    {
        Color fur    = new Color(0.28f, 0.25f, 0.22f);
        Color darkFur = new Color(0.18f, 0.16f, 0.14f);
        Color eye    = new Color(0.90f, 0.80f, 0.10f);
        Color fang   = new Color(0.95f, 0.92f, 0.88f);

        root.localScale = new Vector3(0.9f, 0.55f, 0.9f);

        // Body — long low torso
        GameObject torso = Prim(PrimitiveType.Cube, root, "Torso",
            new Vector3(0f, 0.46f, 0.05f), new Vector3(0.65f, 0.36f, 0.95f), fur);
        Prim(PrimitiveType.Sphere, root, "TorsoRound",
            new Vector3(0f, 0.50f, 0.08f), new Vector3(0.58f, 0.34f, 0.80f), fur);

        // Neck
        Prim(PrimitiveType.Cylinder, root, "Neck",
            new Vector3(0f, 0.62f, 0.44f), new Vector3(0.22f, 0.22f, 0.22f), darkFur);

        // Head — pushed forward
        GameObject head = Prim(PrimitiveType.Cube, root, "Head",
            new Vector3(0f, 0.68f, 0.68f), new Vector3(0.42f, 0.36f, 0.38f), fur);
        // Snout
        Prim(PrimitiveType.Cube, root, "Snout",
            new Vector3(0f, 0.58f, 0.90f), new Vector3(0.26f, 0.22f, 0.32f), darkFur);

        // Ears
        Prim(PrimitiveType.Cube, root, "Ear_L",
            new Vector3(0.16f, 0.88f, 0.68f), new Vector3(0.08f, 0.18f, 0.06f), darkFur);
        Prim(PrimitiveType.Cube, root, "Ear_R",
            new Vector3(-0.16f, 0.88f, 0.68f), new Vector3(0.08f, 0.18f, 0.06f), darkFur);

        // Eyes — yellow
        Prim(PrimitiveType.Sphere, root, "Eye_L",
            new Vector3(0.14f, 0.72f, 0.86f), new Vector3(0.08f, 0.07f, 0.06f), eye);
        Prim(PrimitiveType.Sphere, root, "Eye_R",
            new Vector3(-0.14f, 0.72f, 0.86f), new Vector3(0.08f, 0.07f, 0.06f), eye);

        // Fangs
        Prim(PrimitiveType.Cube, root, "Fang_L",
            new Vector3(0.06f, 0.50f, 0.98f), new Vector3(0.04f, 0.10f, 0.04f), fang);
        Prim(PrimitiveType.Cube, root, "Fang_R",
            new Vector3(-0.06f, 0.50f, 0.98f), new Vector3(0.04f, 0.10f, 0.04f), fang);

        // Legs — 4 short legs
        Prim(PrimitiveType.Cylinder, root, "Leg_FL",
            new Vector3(0.22f, 0.16f, 0.38f), new Vector3(0.11f, 0.30f, 0.11f), fur);
        Prim(PrimitiveType.Cylinder, root, "Leg_FR",
            new Vector3(-0.22f, 0.16f, 0.38f), new Vector3(0.11f, 0.30f, 0.11f), fur);
        Prim(PrimitiveType.Cylinder, root, "Leg_BL",
            new Vector3(0.22f, 0.16f, -0.32f), new Vector3(0.11f, 0.30f, 0.11f), fur);
        Prim(PrimitiveType.Cylinder, root, "Leg_BR",
            new Vector3(-0.22f, 0.16f, -0.32f), new Vector3(0.11f, 0.30f, 0.11f), fur);

        // Tail
        Prim(PrimitiveType.Cube, root, "Tail",
            new Vector3(0f, 0.66f, -0.55f), new Vector3(0.10f, 0.10f, 0.40f), darkFur);
        Prim(PrimitiveType.Sphere, root, "TailTip",
            new Vector3(0f, 0.72f, -0.74f), new Vector3(0.14f, 0.14f, 0.14f), fang);

        if (ai != null) ai.bodyParts = new GameObject[] { torso, head };

        var anim = root.GetComponent<L0OrcAnimator>();
        if (anim == null) anim = root.gameObject.AddComponent<L0OrcAnimator>();
        anim.orcType = OrcType.Wolf;
        anim.colossusBodyTransform = torso.transform;
    }

    private void BuildGoblinVisual(Transform root, EnemyAI ai)
    {
        Color skin   = new Color(0.15f, 0.45f, 0.10f);
        Color dark   = new Color(0.08f, 0.28f, 0.06f);
        Color leather = new Color(0.24f, 0.14f, 0.06f);
        Color eye    = new Color(0.90f, 0.10f, 0.10f);
        Color cloth  = new Color(0.30f, 0.22f, 0.08f);
        Color stone  = new Color(0.55f, 0.52f, 0.48f);

        root.localScale = Vector3.one * 0.75f;

        // Squat body
        GameObject torso = Prim(PrimitiveType.Cube, root, "Torso",
            new Vector3(0f, 0.68f, 0.04f), new Vector3(0.68f, 0.60f, 0.50f), skin);
        Prim(PrimitiveType.Sphere, root, "Belly",
            new Vector3(0f, 0.56f, 0.14f), new Vector3(0.56f, 0.44f, 0.44f), skin);

        // Large head
        GameObject head = Prim(PrimitiveType.Cube, root, "Head",
            new Vector3(0f, 1.18f, 0.06f), new Vector3(0.58f, 0.52f, 0.50f), skin);
        // Long nose
        Prim(PrimitiveType.Cube, root, "Nose",
            new Vector3(0f, 1.14f, 0.32f), new Vector3(0.18f, 0.16f, 0.26f), dark);
        // Big flat ears
        Prim(PrimitiveType.Cube, root, "Ear_L",
            new Vector3(0.40f, 1.24f, 0.02f), new Vector3(0.22f, 0.12f, 0.06f), skin);
        Prim(PrimitiveType.Cube, root, "Ear_R",
            new Vector3(-0.40f, 1.24f, 0.02f), new Vector3(0.22f, 0.12f, 0.06f), skin);
        // Red eyes
        Prim(PrimitiveType.Sphere, root, "Eye_L",
            new Vector3(0.14f, 1.24f, 0.28f), new Vector3(0.10f, 0.09f, 0.08f), eye);
        Prim(PrimitiveType.Sphere, root, "Eye_R",
            new Vector3(-0.14f, 1.24f, 0.28f), new Vector3(0.10f, 0.09f, 0.08f), eye);

        // Belt and cloth
        Prim(PrimitiveType.Cube, root, "Belt",
            new Vector3(0f, 0.46f, 0.12f), new Vector3(0.64f, 0.12f, 0.48f), leather);
        Prim(PrimitiveType.Cube, root, "Loincloth",
            new Vector3(0f, 0.34f, 0.12f), new Vector3(0.52f, 0.16f, 0.06f), cloth);

        // Arms — thin
        Prim(PrimitiveType.Cylinder, root, "UpperArm_L",
            new Vector3(0.50f, 0.76f, 0.06f), new Vector3(0.12f, 0.25f, 0.12f), skin);
        Prim(PrimitiveType.Cylinder, root, "ForeArm_L",
            new Vector3(0.52f, 0.40f, 0.14f), new Vector3(0.10f, 0.22f, 0.10f), skin);
        Prim(PrimitiveType.Cylinder, root, "UpperArm_R",
            new Vector3(-0.50f, 0.76f, 0.06f), new Vector3(0.12f, 0.25f, 0.12f), skin);
        Prim(PrimitiveType.Cylinder, root, "ForeArm_R",
            new Vector3(-0.52f, 0.40f, 0.14f), new Vector3(0.10f, 0.22f, 0.10f), skin);

        // Legs
        Prim(PrimitiveType.Cylinder, root, "Leg_L",
            new Vector3(0.18f, 0.18f, 0f), new Vector3(0.16f, 0.30f, 0.16f), skin);
        Prim(PrimitiveType.Cylinder, root, "Leg_R",
            new Vector3(-0.18f, 0.18f, 0f), new Vector3(0.16f, 0.30f, 0.16f), skin);
        Prim(PrimitiveType.Cube, root, "Boot_L",
            new Vector3(0.18f, -0.04f, 0.04f), new Vector3(0.20f, 0.12f, 0.26f), leather);
        Prim(PrimitiveType.Cube, root, "Boot_R",
            new Vector3(-0.18f, -0.04f, 0.04f), new Vector3(0.20f, 0.12f, 0.26f), leather);

        // Stone pouch (for throwing)
        Prim(PrimitiveType.Sphere, root, "Pouch",
            new Vector3(0.62f, 0.52f, 0.22f), new Vector3(0.16f, 0.14f, 0.14f), stone);
        Prim(PrimitiveType.Sphere, root, "Stone",
            new Vector3(0.62f, 0.40f, 0.26f), new Vector3(0.10f, 0.10f, 0.10f), stone);

        if (ai != null) ai.bodyParts = new GameObject[] { torso, head };

        var anim = root.GetComponent<L0OrcAnimator>();
        if (anim == null) anim = root.gameObject.AddComponent<L0OrcAnimator>();
        anim.orcType = OrcType.Goblin;
        anim.colossusBodyTransform = torso.transform;
    }

    private void BuildSpearmanVisual(Transform root, EnemyAI ai)
    {
        Color skin    = new Color(0.14f, 0.38f, 0.08f);
        Color leather = new Color(0.22f, 0.12f, 0.05f);
        Color metal   = new Color(0.20f, 0.20f, 0.22f);
        Color eye     = new Color(0.85f, 0.75f, 0.10f);
        Color shaft   = new Color(0.45f, 0.32f, 0.12f);
        Color tip     = new Color(0.55f, 0.55f, 0.60f);

        root.localScale = Vector3.one * 1.55f;

        GameObject torso = Prim(PrimitiveType.Cube, root, "Torso",
            new Vector3(0f, 0.78f, 0.04f), new Vector3(0.70f, 0.68f, 0.52f), skin);
        Prim(PrimitiveType.Sphere, root, "Belly",
            new Vector3(0f, 0.62f, 0.14f), new Vector3(0.56f, 0.44f, 0.46f), skin);

        GameObject head = Prim(PrimitiveType.Cube, root, "Head",
            new Vector3(0f, 1.26f, 0.08f), new Vector3(0.48f, 0.44f, 0.48f), skin);
        Prim(PrimitiveType.Cube, root, "Jaw",
            new Vector3(0f, 1.08f, 0.20f), new Vector3(0.36f, 0.14f, 0.26f), skin);
        Prim(PrimitiveType.Cube, root, "Helmet",
            new Vector3(0f, 1.42f, 0.04f), new Vector3(0.52f, 0.20f, 0.50f), metal);

        Prim(PrimitiveType.Sphere, root, "Eye_L",
            new Vector3(0.13f, 1.30f, 0.26f), new Vector3(0.09f, 0.07f, 0.06f), eye);
        Prim(PrimitiveType.Sphere, root, "Eye_R",
            new Vector3(-0.13f, 1.30f, 0.26f), new Vector3(0.09f, 0.07f, 0.06f), eye);

        Prim(PrimitiveType.Cube, root, "ChestPlate",
            new Vector3(0f, 0.80f, 0.28f), new Vector3(0.62f, 0.50f, 0.10f), metal);
        Prim(PrimitiveType.Cube, root, "Belt",
            new Vector3(0f, 0.50f, 0.10f), new Vector3(0.68f, 0.10f, 0.50f), leather);
        Prim(PrimitiveType.Sphere, root, "Shoulder_L",
            new Vector3(0.48f, 1.00f, 0.04f), new Vector3(0.26f, 0.20f, 0.24f), metal);
        Prim(PrimitiveType.Sphere, root, "Shoulder_R",
            new Vector3(-0.48f, 1.00f, 0.04f), new Vector3(0.26f, 0.20f, 0.24f), metal);

        Prim(PrimitiveType.Cylinder, root, "UpperArm_L",
            new Vector3(0.55f, 0.82f, 0.04f), new Vector3(0.14f, 0.26f, 0.14f), skin);
        Prim(PrimitiveType.Cylinder, root, "ForeArm_L",
            new Vector3(0.58f, 0.44f, 0.14f), new Vector3(0.12f, 0.24f, 0.12f), skin);
        Prim(PrimitiveType.Cylinder, root, "UpperArm_R",
            new Vector3(-0.55f, 0.82f, 0.04f), new Vector3(0.14f, 0.26f, 0.14f), skin);
        Prim(PrimitiveType.Cylinder, root, "ForeArm_R",
            new Vector3(-0.58f, 0.44f, 0.14f), new Vector3(0.12f, 0.24f, 0.12f), skin);

        Prim(PrimitiveType.Cylinder, root, "Leg_L",
            new Vector3(0.20f, 0.18f, 0f), new Vector3(0.16f, 0.34f, 0.16f), leather);
        Prim(PrimitiveType.Cylinder, root, "Leg_R",
            new Vector3(-0.20f, 0.18f, 0f), new Vector3(0.16f, 0.34f, 0.16f), leather);
        Prim(PrimitiveType.Cube, root, "Boot_L",
            new Vector3(0.20f, -0.05f, 0.04f), new Vector3(0.18f, 0.12f, 0.26f), leather);
        Prim(PrimitiveType.Cube, root, "Boot_R",
            new Vector3(-0.20f, -0.05f, 0.04f), new Vector3(0.18f, 0.12f, 0.26f), leather);

        // Длинное копьё — держит двумя руками
        Prim(PrimitiveType.Cylinder, root, "SpearShaft",
            new Vector3(-0.55f, 0.65f, 0.22f), new Vector3(0.05f, 0.95f, 0.05f), shaft);
        Prim(PrimitiveType.Cube, root, "SpearTip",
            new Vector3(-0.55f, 1.62f, 0.22f), new Vector3(0.08f, 0.28f, 0.06f), tip);
        Prim(PrimitiveType.Cube, root, "SpearTipSide_L",
            new Vector3(-0.62f, 1.58f, 0.22f), new Vector3(0.05f, 0.14f, 0.04f), tip);
        Prim(PrimitiveType.Cube, root, "SpearTipSide_R",
            new Vector3(-0.48f, 1.58f, 0.22f), new Vector3(0.05f, 0.14f, 0.04f), tip);
        // Нижний конец копья
        Prim(PrimitiveType.Cube, root, "SpearButtCap",
            new Vector3(-0.55f, -0.34f, 0.22f), new Vector3(0.06f, 0.08f, 0.06f), metal);

        if (ai != null) ai.bodyParts = new GameObject[] { torso, head };

        var anim = root.GetComponent<L0OrcAnimator>();
        if (anim != null) anim.spearTransform = root.Find("SpearShaft");
    }

    private void BuildNecromancerVisual(Transform root, EnemyAI ai)
    {
        Color deadSkin = new Color(0.72f, 0.70f, 0.65f);
        Color robe     = new Color(0.08f, 0.08f, 0.10f);
        Color robeAcc  = new Color(0.12f, 0.28f, 0.08f);
        Color bone     = new Color(0.68f, 0.64f, 0.54f);
        Color eye      = new Color(0.15f, 1.00f, 0.20f);
        Color orb      = new Color(0.10f, 0.85f, 0.18f);

        root.localScale = Vector3.one * 1.0f;

        GameObject torso = Prim(PrimitiveType.Cube, root, "Torso",
            new Vector3(0f, 0.76f, 0.02f), new Vector3(0.55f, 0.60f, 0.44f), deadSkin);

        // Чёрный длинный балахон — сгорбленный силуэт
        Prim(PrimitiveType.Cube, root, "Robe_Upper",
            new Vector3(0f, 0.78f, 0.05f), new Vector3(0.66f, 0.66f, 0.52f), robe);
        Prim(PrimitiveType.Cube, root, "Robe_Lower",
            new Vector3(0f, 0.32f, 0.05f), new Vector3(0.72f, 0.48f, 0.52f), robe);
        Prim(PrimitiveType.Cube, root, "Robe_Hem",
            new Vector3(0f, 0.04f, 0.05f), new Vector3(0.76f, 0.18f, 0.54f), robe);
        Prim(PrimitiveType.Cube, root, "RobeTrim",
            new Vector3(0f, 0.48f, 0.28f), new Vector3(0.54f, 0.05f, 0.04f), robeAcc);

        // Сгорбленная голова под капюшоном
        GameObject head = Prim(PrimitiveType.Sphere, root, "Head",
            new Vector3(0f, 1.24f, 0.04f), new Vector3(0.46f, 0.48f, 0.46f), deadSkin);
        Prim(PrimitiveType.Cube, root, "Hood",
            new Vector3(0f, 1.38f, -0.04f), new Vector3(0.52f, 0.30f, 0.50f), robe);
        Prim(PrimitiveType.Cube, root, "HoodBrim",
            new Vector3(0f, 1.26f, 0.14f), new Vector3(0.46f, 0.07f, 0.34f), robe);

        // Зелёные огни вместо глаз
        Prim(PrimitiveType.Sphere, root, "Eye_L",
            new Vector3(0.12f, 1.28f, 0.24f), new Vector3(0.10f, 0.09f, 0.07f), eye);
        Prim(PrimitiveType.Sphere, root, "Eye_R",
            new Vector3(-0.12f, 1.28f, 0.24f), new Vector3(0.10f, 0.09f, 0.07f), eye);

        // Тонкие руки с костяными пальцами
        Prim(PrimitiveType.Cylinder, root, "Arm_L",
            new Vector3(0.42f, 0.72f, 0.06f), new Vector3(0.10f, 0.40f, 0.10f), deadSkin);
        Prim(PrimitiveType.Cylinder, root, "Arm_R",
            new Vector3(-0.42f, 0.72f, 0.06f), new Vector3(0.10f, 0.40f, 0.10f), deadSkin);
        // Костяные пальцы
        for (int f = -1; f <= 1; f++)
        {
            Prim(PrimitiveType.Cube, root, "Finger_L" + f,
                new Vector3(0.42f + f * 0.06f, 0.40f, 0.16f), new Vector3(0.03f, 0.12f, 0.03f), bone);
            Prim(PrimitiveType.Cube, root, "Finger_R" + f,
                new Vector3(-0.42f + f * 0.06f, 0.40f, 0.16f), new Vector3(0.03f, 0.12f, 0.03f), bone);
        }

        // Посох с зелёным светящимся черепом
        Prim(PrimitiveType.Cylinder, root, "StaffShaft",
            new Vector3(0.54f, 0.58f, 0.14f), new Vector3(0.055f, 0.95f, 0.055f), bone);
        Prim(PrimitiveType.Cube, root, "StaffCross",
            new Vector3(0.54f, 1.52f, 0.14f), new Vector3(0.20f, 0.055f, 0.055f), bone);
        // Зелёный череп на вершине
        GameObject necOrb = Prim(PrimitiveType.Sphere, root, "NecOrb",
            new Vector3(0.54f, 1.72f, 0.14f), new Vector3(0.22f, 0.24f, 0.22f), orb);
        // Глазницы черепа
        Prim(PrimitiveType.Sphere, root, "SkullEyeL",
            new Vector3(0.60f, 1.76f, 0.24f), new Vector3(0.06f, 0.06f, 0.04f), robe);
        Prim(PrimitiveType.Sphere, root, "SkullEyeR",
            new Vector3(0.48f, 1.76f, 0.24f), new Vector3(0.06f, 0.06f, 0.04f), robe);

        if (ai != null) ai.bodyParts = new GameObject[] { torso, head };

        var anim = root.GetComponent<L0OrcAnimator>();
        if (anim != null)
        {
            anim.orbTransform = necOrb.transform;
            anim.orcType = L0OrcFactory.OrcType.Necromancer;
        }
    }

    private void BuildWarchiefVisual(Transform root, EnemyAI ai)
    {
        Color skin    = new Color(0.55f, 0.08f, 0.05f);
        Color darkRed = new Color(0.38f, 0.04f, 0.03f);
        Color metal   = new Color(0.18f, 0.18f, 0.20f);
        Color darkM   = new Color(0.12f, 0.10f, 0.12f);
        Color bone    = new Color(0.72f, 0.68f, 0.58f);
        Color eye     = new Color(1.00f, 0.90f, 0.05f);
        Color gold    = new Color(0.80f, 0.62f, 0.05f);

        root.localScale = Vector3.one * 2.0f;

        // Массивный торс
        GameObject torso = Prim(PrimitiveType.Cube, root, "Torso",
            new Vector3(0f, 0.80f, 0.04f), new Vector3(0.88f, 0.74f, 0.62f), skin);
        Prim(PrimitiveType.Sphere, root, "Belly",
            new Vector3(0f, 0.64f, 0.18f), new Vector3(0.72f, 0.52f, 0.56f), skin);

        // Голова с рогатым шлемом
        GameObject head = Prim(PrimitiveType.Cube, root, "Head",
            new Vector3(0f, 1.30f, 0.08f), new Vector3(0.56f, 0.52f, 0.54f), skin);
        Prim(PrimitiveType.Cube, root, "Jaw",
            new Vector3(0f, 1.10f, 0.24f), new Vector3(0.44f, 0.16f, 0.30f), skin);
        Prim(PrimitiveType.Cube, root, "Helmet",
            new Vector3(0f, 1.50f, 0.04f), new Vector3(0.64f, 0.26f, 0.62f), darkM);
        // Массивные рога вождя
        Prim(PrimitiveType.Cube, root, "Horn_L",
            new Vector3(0.36f, 1.74f, -0.06f), new Vector3(0.10f, 0.46f, 0.10f), bone);
        Prim(PrimitiveType.Cube, root, "HornTip_L",
            new Vector3(0.44f, 2.00f, -0.10f), new Vector3(0.06f, 0.16f, 0.06f), new Color(0.90f, 0.85f, 0.70f));
        Prim(PrimitiveType.Cube, root, "Horn_R",
            new Vector3(-0.36f, 1.74f, -0.06f), new Vector3(0.10f, 0.46f, 0.10f), bone);
        Prim(PrimitiveType.Cube, root, "HornTip_R",
            new Vector3(-0.44f, 2.00f, -0.10f), new Vector3(0.06f, 0.16f, 0.06f), new Color(0.90f, 0.85f, 0.70f));
        // Золотая полоса на шлеме — знак вождя
        Prim(PrimitiveType.Cube, root, "HelmetBand",
            new Vector3(0f, 1.50f, 0.34f), new Vector3(0.62f, 0.06f, 0.04f), gold);

        Prim(PrimitiveType.Sphere, root, "Eye_L",
            new Vector3(0.15f, 1.34f, 0.30f), new Vector3(0.10f, 0.08f, 0.07f), eye);
        Prim(PrimitiveType.Sphere, root, "Eye_R",
            new Vector3(-0.15f, 1.34f, 0.30f), new Vector3(0.10f, 0.08f, 0.07f), eye);

        // Тяжёлые наплечники
        Prim(PrimitiveType.Sphere, root, "Shoulder_L",
            new Vector3(0.56f, 1.06f, 0.04f), new Vector3(0.36f, 0.28f, 0.34f), darkM);
        Prim(PrimitiveType.Cube, root, "ShoulderSpike_L",
            new Vector3(0.62f, 1.26f, 0.00f), new Vector3(0.08f, 0.24f, 0.08f), bone);
        Prim(PrimitiveType.Sphere, root, "Shoulder_R",
            new Vector3(-0.56f, 1.06f, 0.04f), new Vector3(0.36f, 0.28f, 0.34f), darkM);
        Prim(PrimitiveType.Cube, root, "ShoulderSpike_R",
            new Vector3(-0.62f, 1.26f, 0.00f), new Vector3(0.08f, 0.24f, 0.08f), bone);

        Prim(PrimitiveType.Cube, root, "ChestPlate",
            new Vector3(0f, 0.84f, 0.32f), new Vector3(0.74f, 0.56f, 0.12f), darkM);
        Prim(PrimitiveType.Cube, root, "ChestRune",
            new Vector3(0f, 0.88f, 0.38f), new Vector3(0.20f, 0.20f, 0.04f), gold);
        Prim(PrimitiveType.Cube, root, "Belt",
            new Vector3(0f, 0.52f, 0.12f), new Vector3(0.80f, 0.14f, 0.56f), darkRed);

        Prim(PrimitiveType.Cylinder, root, "UpperArm_L",
            new Vector3(0.60f, 0.84f, 0.04f), new Vector3(0.18f, 0.30f, 0.18f), skin);
        Prim(PrimitiveType.Cylinder, root, "ForeArm_L",
            new Vector3(0.62f, 0.44f, 0.16f), new Vector3(0.16f, 0.28f, 0.16f), skin);
        Prim(PrimitiveType.Cylinder, root, "UpperArm_R",
            new Vector3(-0.60f, 0.84f, 0.04f), new Vector3(0.18f, 0.30f, 0.18f), skin);
        Prim(PrimitiveType.Cylinder, root, "ForeArm_R",
            new Vector3(-0.62f, 0.44f, 0.16f), new Vector3(0.16f, 0.28f, 0.16f), skin);

        Prim(PrimitiveType.Cylinder, root, "Leg_L",
            new Vector3(0.24f, 0.20f, 0f), new Vector3(0.20f, 0.40f, 0.20f), darkRed);
        Prim(PrimitiveType.Cylinder, root, "Leg_R",
            new Vector3(-0.24f, 0.20f, 0f), new Vector3(0.20f, 0.40f, 0.20f), darkRed);
        Prim(PrimitiveType.Cube, root, "Boot_L",
            new Vector3(0.24f, -0.06f, 0.05f), new Vector3(0.22f, 0.16f, 0.30f), darkM);
        Prim(PrimitiveType.Cube, root, "Boot_R",
            new Vector3(-0.24f, -0.06f, 0.05f), new Vector3(0.22f, 0.16f, 0.30f), darkM);

        // Огромный двуручный топор
        Prim(PrimitiveType.Cylinder, root, "AxeHandle",
            new Vector3(-0.74f, 0.60f, 0.22f), new Vector3(0.07f, 0.72f, 0.07f), new Color(0.28f, 0.16f, 0.06f));
        Prim(PrimitiveType.Cube, root, "AxeHead_L",
            new Vector3(-0.74f, 1.28f, 0.22f), new Vector3(0.48f, 0.36f, 0.08f), darkM);
        Prim(PrimitiveType.Cube, root, "AxeHead_R",
            new Vector3(-0.74f, 1.14f, 0.22f), new Vector3(0.36f, 0.24f, 0.06f), metal);
        // Золотые руны на топоре
        Prim(PrimitiveType.Cube, root, "AxeRune",
            new Vector3(-0.74f, 1.22f, 0.28f), new Vector3(0.14f, 0.08f, 0.03f), gold);

        if (ai != null) ai.bodyParts = new GameObject[] { torso, head };

        var anim = root.GetComponent<L0OrcAnimator>();
        if (anim != null)
        {
            anim.orcType = L0OrcFactory.OrcType.Warchief;
            anim.headTransform = head.transform;
        }
    }

    private void BuildPoisonArcherVisual(Transform root, EnemyAI ai)
    {
        Color skin    = new Color(0.20f, 0.42f, 0.10f);
        Color dark    = new Color(0.12f, 0.26f, 0.06f);
        Color hood    = new Color(0.10f, 0.22f, 0.05f);
        Color leather = new Color(0.22f, 0.14f, 0.06f);
        Color eye     = new Color(0.40f, 1.00f, 0.20f);
        Color poison  = new Color(0.20f, 0.72f, 0.10f);
        Color bone    = new Color(0.68f, 0.64f, 0.54f);

        root.localScale = Vector3.one * 1.2f;

        GameObject torso = Prim(PrimitiveType.Cube, root, "Torso",
            new Vector3(0f, 0.76f, 0.04f), new Vector3(0.66f, 0.64f, 0.50f), skin);
        Prim(PrimitiveType.Sphere, root, "Belly",
            new Vector3(0f, 0.62f, 0.12f), new Vector3(0.56f, 0.42f, 0.44f), skin);

        // Капюшон и голова
        GameObject head = Prim(PrimitiveType.Cube, root, "Head",
            new Vector3(0f, 1.24f, 0.06f), new Vector3(0.46f, 0.42f, 0.46f), skin);
        Prim(PrimitiveType.Cube, root, "Hood",
            new Vector3(0f, 1.38f, -0.04f), new Vector3(0.52f, 0.30f, 0.50f), hood);
        Prim(PrimitiveType.Cube, root, "HoodBrim",
            new Vector3(0f, 1.24f, 0.14f), new Vector3(0.46f, 0.07f, 0.34f), hood);
        Prim(PrimitiveType.Cube, root, "Jaw",
            new Vector3(0f, 1.07f, 0.20f), new Vector3(0.34f, 0.13f, 0.24f), skin);

        // Зелёные глаза-щели
        Prim(PrimitiveType.Sphere, root, "Eye_L",
            new Vector3(0.12f, 1.28f, 0.25f), new Vector3(0.09f, 0.06f, 0.06f), eye);
        Prim(PrimitiveType.Sphere, root, "Eye_R",
            new Vector3(-0.12f, 1.28f, 0.25f), new Vector3(0.09f, 0.06f, 0.06f), eye);

        Prim(PrimitiveType.Cube, root, "Vest",
            new Vector3(0f, 0.80f, 0.28f), new Vector3(0.58f, 0.48f, 0.10f), dark);
        Prim(PrimitiveType.Cube, root, "Belt",
            new Vector3(0f, 0.48f, 0.10f), new Vector3(0.64f, 0.10f, 0.48f), leather);

        Prim(PrimitiveType.Cylinder, root, "UpperArm_L",
            new Vector3(0.52f, 0.80f, 0.04f), new Vector3(0.13f, 0.26f, 0.13f), skin);
        Prim(PrimitiveType.Cylinder, root, "ForeArm_L",
            new Vector3(0.54f, 0.42f, 0.14f), new Vector3(0.11f, 0.22f, 0.11f), skin);
        Prim(PrimitiveType.Cylinder, root, "UpperArm_R",
            new Vector3(-0.52f, 0.80f, 0.04f), new Vector3(0.13f, 0.26f, 0.13f), skin);
        Prim(PrimitiveType.Cylinder, root, "ForeArm_R",
            new Vector3(-0.54f, 0.42f, 0.14f), new Vector3(0.11f, 0.22f, 0.11f), skin);

        Prim(PrimitiveType.Cylinder, root, "Leg_L",
            new Vector3(0.18f, 0.18f, 0f), new Vector3(0.16f, 0.32f, 0.16f), dark);
        Prim(PrimitiveType.Cylinder, root, "Leg_R",
            new Vector3(-0.18f, 0.18f, 0f), new Vector3(0.16f, 0.32f, 0.16f), dark);
        Prim(PrimitiveType.Cube, root, "Boot_L",
            new Vector3(0.18f, -0.04f, 0.04f), new Vector3(0.20f, 0.12f, 0.26f), leather);
        Prim(PrimitiveType.Cube, root, "Boot_R",
            new Vector3(-0.18f, -0.04f, 0.04f), new Vector3(0.20f, 0.12f, 0.26f), leather);

        // Изогнутый лук с зелёной тетивой
        Prim(PrimitiveType.Cylinder, root, "Bow",
            new Vector3(-0.68f, 0.72f, 0.22f), new Vector3(0.04f, 0.72f, 0.04f), dark);
        // Два колчана с отравленными стрелами
        Prim(PrimitiveType.Cylinder, root, "Quiver1",
            new Vector3(0.28f, 0.90f, -0.22f), new Vector3(0.10f, 0.34f, 0.10f), leather);
        Prim(PrimitiveType.Cylinder, root, "Quiver2",
            new Vector3(0.38f, 0.88f, -0.20f), new Vector3(0.08f, 0.30f, 0.08f), dark);
        // Отравленные стрелы (зелёные)
        Prim(PrimitiveType.Cube, root, "PoisonArrow1",
            new Vector3(0.26f, 1.18f, -0.22f), new Vector3(0.03f, 0.26f, 0.03f), poison);
        Prim(PrimitiveType.Cube, root, "PoisonArrow2",
            new Vector3(0.32f, 1.16f, -0.20f), new Vector3(0.03f, 0.22f, 0.03f), poison);
        Prim(PrimitiveType.Cube, root, "PoisonArrow3",
            new Vector3(0.38f, 1.14f, -0.18f), new Vector3(0.03f, 0.20f, 0.03f), bone);
        // Пузырёк с ядом на поясе
        Prim(PrimitiveType.Sphere, root, "PoisonVial",
            new Vector3(0.60f, 0.54f, 0.22f), new Vector3(0.12f, 0.16f, 0.12f), poison);

        if (ai != null) ai.bodyParts = new GameObject[] { torso, head };

        var anim = root.GetComponent<L0OrcAnimator>();
        if (anim != null)
        {
            anim.orcType = L0OrcFactory.OrcType.PoisonArcher;
            anim.headTransform = head.transform;
        }
    }

    private GameObject Prim(PrimitiveType type, Transform parent, string objectName, Vector3 localPosition, Vector3 localScale, Color color)
    {
        GameObject obj = GameObject.CreatePrimitive(type);
        obj.name = objectName;
        obj.transform.SetParent(parent, false);
        obj.transform.localPosition = localPosition;
        obj.transform.localRotation = Quaternion.identity;
        obj.transform.localScale = localScale;

        Collider collider = obj.GetComponent<Collider>();
        if (collider != null)
            Destroy(collider);

        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material material = new Material(Shader.Find("Standard"));
            material.color = color;
            renderer.material = material;
        }

        return obj;
    }
}

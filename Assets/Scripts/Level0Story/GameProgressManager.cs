using UnityEngine;

public class GameProgressManager : MonoBehaviour
{
    public static GameProgressManager Instance { get; private set; }

    public enum WeaponId
    {
        Sword,
        Spear,
        Bow
    }

    public enum Level0State
    {
        Prologue,
        GateRaid,
        GateOpened,
        RoadToOrcVillage,
        OrcVillageFight,
        ExitUnlocked,
        Completed
    }

    [Header("Level 0 State")]
    public Level0State level0State = Level0State.Prologue;

    [Header("NPC")]
    public bool talkedToElder;
    public bool talkedToRefugee;
    public bool talkedToGuard;
    public bool talkedToBlacksmith;

    [Header("Items")]
    public bool hasSword;
    public bool hasSpear;
    public bool hasBow;
    public bool hasKingNote;

    [Header("Gate Raid")]
    public bool castleGateRaidStarted;
    public bool castleGateRaidCleared;
    public bool castleGateOpened;

    [Header("Orc Village")]
    public bool orcVillageDiscovered;
    public bool orcVillageFightStarted;
    public bool orcVillageCleared;
    public bool portalGuardsDefeated;
    public bool level0ExitUnlocked;

    [Header("Future Resources")]
    public int gold;
    public int iron;

    public bool orcRaidStarted
    {
        get { return castleGateRaidStarted; }
        set { castleGateRaidStarted = value; }
    }

    public bool orcRaidCleared
    {
        get { return castleGateRaidCleared; }
        set { castleGateRaidCleared = value; }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public static GameProgressManager Ensure()
    {
        if (Instance != null)
            return Instance;

        GameObject obj = new GameObject("GAME_PROGRESS_MANAGER");
        return obj.AddComponent<GameProgressManager>();
    }

    public void ResetCastleProgress()
    {
        talkedToElder = false;
        talkedToRefugee = false;
        talkedToGuard = false;
        talkedToBlacksmith = false;

        hasSword = false;
        hasSpear = false;
        hasBow = false;
        hasKingNote = false;

        castleGateRaidStarted = false;
        castleGateRaidCleared = false;
        castleGateOpened = false;

        orcVillageDiscovered = false;
        orcVillageFightStarted = false;
        orcVillageCleared = false;
        portalGuardsDefeated = false;
        level0ExitUnlocked = false;

        gold = 0;
        iron = 0;

        level0State = Level0State.Prologue;
    }

    public void RegisterNpcTalk(string role)
    {
        if (role == "Elder") talkedToElder = true;
        if (role == "Refugee") talkedToRefugee = true;
        if (role == "Guard") talkedToGuard = true;
        if (role == "Blacksmith") talkedToBlacksmith = true;
    }

    public void RegisterWeapon(WeaponId id)
    {
        if (id == WeaponId.Sword) hasSword = true;
        if (id == WeaponId.Spear) hasSpear = true;
        if (id == WeaponId.Bow) hasBow = true;
    }

    public void RegisterKingNote()
    {
        hasKingNote = true;
    }

    public bool HasAllWeapons()
    {
        return hasSword && hasSpear && hasBow;
    }

    public bool IsCastleIntroCompleted()
    {
        return talkedToElder
            && talkedToRefugee
            && talkedToGuard
            && talkedToBlacksmith
            && hasSword
            && hasSpear
            && hasBow
            && hasKingNote;
    }

    public int GetCastleTaskDoneCount()
    {
        int c = 0;
        if (talkedToElder) c++;
        if (talkedToRefugee) c++;
        if (talkedToGuard) c++;
        if (talkedToBlacksmith) c++;
        if (hasSword) c++;
        if (hasSpear) c++;
        if (hasBow) c++;
        if (hasKingNote) c++;
        return c;
    }

    public int GetCastleTaskTotalCount()
    {
        return 8;
    }

    public string GetCurrentCastleObjective()
    {
        if (level0State == Level0State.Completed) return "Уровень 0 завершён";
        if (!talkedToElder) return "Поговори со старостой у ворот замка";
        if (!talkedToRefugee) return "Найди жительницу в деревне слева от дороги";
        if (!talkedToGuard) return "Вернись к раненому стражнику у ворот";
        if (!talkedToBlacksmith) return "Найди кузнеца у правой стены замка";
        if (!hasSword || !hasSpear || !hasBow) return "Проверь меч, копьё и лук у кузнеца";
        if (!hasKingNote) return "Возьми указ короля на столе у ворот";
        if (!castleGateRaidStarted) return "Вернись к воротам — начинается тревога";
        if (castleGateRaidStarted && !castleGateRaidCleared && !castleGateOpened) return "Убей первых орков у ворот";
        if (castleGateRaidStarted && !castleGateRaidCleared && castleGateOpened) return "Удержи ворота вместе с защитниками";
        if (castleGateRaidCleared && castleGateOpened && !orcVillageFightStarted && orcVillageDiscovered) return "Войди в логово орков";
        if (castleGateRaidCleared && castleGateOpened && !orcVillageFightStarted) return "Иди к логову орков";
        if (castleGateRaidCleared && !castleGateOpened) return "Открой ворота клавишей E";
        if (orcVillageFightStarted && !orcVillageCleared) return "Убей орков-защитников";
        if (orcVillageCleared && level0ExitUnlocked) return "Путь к порталу открыт";
        if (level0ExitUnlocked) return "Найди выход за логовом";
        return "Путь дальше открыт";
    }

    public void StartGateRaid()
    {
        castleGateRaidStarted = true;
        castleGateRaidCleared = false;
        castleGateOpened = false;
        level0State = Level0State.GateRaid;
    }

    public void MarkGateRaidCleared()
    {
        castleGateRaidStarted = true;
        castleGateRaidCleared = true;
        level0State = castleGateOpened ? Level0State.RoadToOrcVillage : Level0State.GateOpened;

        if (KingAbility.Instance != null && !KingAbility.Instance.unlocked)
        {
            KingAbility.Instance.unlocked = true;
            Debug.Log("[GameProgress] KingAbility UNLOCKED — charge per kill via Q");

            if (CastlePrologueBuilder.Instance != null)
                CastlePrologueBuilder.Instance.StartFireAwakening();
            else
                GameManager.Instance?.ShowMessage("Сила мёртвого короля пробудилась! Убивай орков → заряжай [Q]!", GameManager.MsgType.Default, 5f);
        }
        else if (KingAbility.Instance == null)
        {
            Debug.LogWarning("[GameProgress] KingAbility.Instance is NULL at MarkGateRaidCleared — ability won't unlock!");
        }
    }

    public void MarkOrcRaidStarted()
    {
        StartGateRaid();
    }

    public void MarkOrcRaidCleared()
    {
        MarkGateRaidCleared();
    }

    public void MarkCastleGateOpened()
    {
        castleGateOpened = true;
        level0State = castleGateRaidCleared ? Level0State.RoadToOrcVillage : Level0State.GateRaid;
    }

    public void StartOrcVillageFight()
    {
        orcVillageDiscovered = true;
        orcVillageFightStarted = true;
        level0State = Level0State.OrcVillageFight;
    }

    public void MarkOrcVillageDiscovered()
    {
        orcVillageDiscovered = true;
    }

    public void MarkPortalGuardsDefeated()
    {
        portalGuardsDefeated = true;
    }

    public void MarkOrcVillageCleared()
    {
        orcVillageDiscovered = true;
        orcVillageFightStarted = true;
        orcVillageCleared = true;
        portalGuardsDefeated = true;
        level0ExitUnlocked = true;
        level0State = Level0State.ExitUnlocked;
    }

    public void MarkLevel0Completed()
    {
        level0ExitUnlocked = true;
        level0State = Level0State.Completed;
    }
}

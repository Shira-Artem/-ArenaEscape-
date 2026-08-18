# CLAUDE.md — ArenaEscape (технический ориентир №1)

> **В проекте ВСЕГО ДВА .md-файла-ориентира** (остальные сжаты сюда и удалены 2026-06-28):
> - **`CLAUDE.md`** (этот файл) — ТЕХНИКА: карта кода, архитектура, навигация, координаты,
>   правила размещения, боевые параметры, что НЕ трогать, поток сцен, роадмап «боевого режима».
> - **`GAME_DESIGN_DOC.md`** (корень) — ДИЗАЙН: сюжет, мир, геймплей-петля, враги, очки/рекорды,
>   структура уровней, привязка к лабам, ощущение от игры.
>
> Источник истины по коду — сам код (особенно `L0OrcArenaConfig.cs`, `SceneBuilder.cs`).
> Если этот файл и старый аудит-док расходятся — верь коду и этому файлу.

---

## 📊 СТАТУС ПРОЕКТА (обновлено 2026-06-26)

### Что ГОТОВО (шаги 1–23)
| Шаг | Что | Файлы | Статус |
|-----|-----|-------|--------|
| 1 | Магента-материалы | `L0Util.FixMaterials()` | ✅ |
| 2 | Конфликт систем декора | WorldDressingBuilder отключён | ✅ |
| 3 | Стартовая зона/замок | `Level0CastleDressingBuilder` | ✅ |
| 4A | Лес (220+ деревьев) | `Level0ForestBuilder` | ✅ seed=44018 |
| 4B | Деревня | `L0Village`, `L0Util` | ✅ |
| 5 | Дорога | `L0Road` | ✅ |
| 6 | Ворота арены | `L0OrcArenaGateModule` | ✅ |
| 7 | Декор арены | `L0OrcArenaCampModule` | ✅ seed=70451 |
| 8 | Центральная боевая зона | `L0OrcArenaEnhancement.BuildCentralDread()` | ✅ |
| 9 | Портальная зона | `L0OrcArenaEnhancement.BuildPortalSacredPath()` | ✅ |
| 14 | Портал→конец арены | `L0QuickExitToOne` fallback=(10,1,-275) | ✅ |
| 15 | Визуал орков | `L0OrcFactory.BuildVisual()` | ✅ |
| 16 | 24 орка + Serious Sam | `Level0OrcVillageManager`, `L0OrcFactory` | ✅ |
| 17 | Оружие из ассетов | `WeaponController` + Medieval Weapons | ✅ |
| 18 | Огненный шар короля | `KingAbility`, `Fireball` | ✅ Q при заряде |
| 19 | Зона кузнеца | `L0BlacksmithArea` + exclusion в ForestBuilder | ✅ |
| 19b | Квест кузнеца / крафт | `Crafting/` (руда→огненные стрелы) | ✅ ожидает скрин |
| 20 | Juice боя | `FeedbackManager`, `EnemyAI` (ragdoll+slowmo) | ✅ |
| 21 | Типы орков | `L0OrcFactory` (Berserker/Tank/Shaman) | ✅ |
| 22 | Зелёный мир | `L0Layout`, `Level0ForestBuilder`, TintTree | ✅ |
| 23 | Апгрейд орков | `L0OrcFactory` (scale) + `L0OrcAnimator` | ✅ ожидает скрин |

### Что ОСТАЛОСЬ
| Шаг | Что | Приоритет |
|-----|-----|-----------|
| 11 | Дракон — визуальная доработка | НИЗКИЙ |
| 12 | NPC окружение (декор у замка) | НИЗКИЙ |
| 13 | Финальная полировка (свет, атмосфера) | НИЗКИЙ |

---

## 🎨 БОРЬБА С СЕРОСТЬЮ — ЗАПОМНИТЬ НАВСЕГДА
> Главная боль пользователя — серый унылый мир. Правила цвета:
1. **`L0Util.FixMaterials()` НЕ КРАСИТ** — чинит только сломанные (Error) шейдеры. PP/RPGPP ассеты грузятся «нормальными» серо-зимними и остаются серыми. **После загрузки ассета всегда тинтуй:**
   - Деревья → `L0Util.TintTree(obj)` (greenBoost 0.70,1.0,0.58 + форс если g<r+0.08)
   - Камни/горы/трава → `L0Util.TintMaterials(obj, tint)` (умножает _BaseColor)
   - Замок → `TintCastle()` в `Level0CastleDressingBuilder` (warmTint 1.0,0.91,0.78)
2. **Освещение Level0 ставит `L0Layout.SetupWorldLighting()`** (НЕ EnvironmentAtmosphere — он отключён). Тёплое яркое солнце (intensity 1.5, почти белое 1,0.96,0.84), Trilight ambient, ЛИНЕЙНЫЙ туман start 90 / end 340 (не Exponential — тот топит всё в дымку).
3. **Земля видимая зелёная**: `L0Props.CreateSupportFloor(..., color, visible:true)`. WorldSafetyFloor = (0.28,0.52,0.18).
   - Сцена имеет встроенный terrain Y≈0 в зоне замка. SafetyFloor на Y=-0.6 там НЕВИДИМ (под terrain).
   - **За замком (Z>15):** `BehindCastleGreenFloor` на Y=0.02, 220×100, центр Z=65 — поверх terrain.
4. **Масштаб RPGPP terrain/hills**: `rpgpp_lt_terrain_grass_*` и `rpgpp_lt_hill_small_*` ОГРОМНЫЕ (как горы) → scale 0.3-0.5, НЕ 2-4. PP_Meadow → scale 0.5-0.8.

---

## 🚫 ЗАПРЕТНЫЕ ЗОНЫ — КУДА НЕЛЬЗЯ СТАВИТЬ ПРОПСЫ

### Замок/двор
- Z > -16 (CastleGenerator — НЕ ТРОГАТЬ).
- **FOOTPRINT замка:** `|X|<17 AND -12<Z<26` — сюда деревья/пропсы не ставить.
- `Level0ForestBuilder.IsInsideCastle(x,z)` проверяет это перед каждым `PlaceTree`.

### Зона кузнеца
- **FOOTPRINT исключения деревьев:** `X [-3, 13.5] AND Z [-32, -17]` (расширено под кроны PP-деревьев).
- `Level0ForestBuilder.IsInsideBlacksmith(x,z)` — деревья/трава/камни НЕ спавнятся тут.
- Платформа PY=0.78, расчистка 10×9 м (EARTH) на Y=0.18 перекрывает луга.
- **Центр:** (5.75, -24.5), горн: (7.65, -26.1), наковальня: (5.25, -24.8). **NPC-кузнец:** (5.75, 0.82, -23.8) лицом ко входу (+Z).

### Деревня беженцев
- Центр **(-24, 0, -45)**, радиус **26** → X от **-50 до +2**, Z от **-71 до -19**.
- Дома + NPC тут. Крупное (луга/холмы/terrain-chunks/кусты) — НЕЛЬЗЯ.

### Дорога к оркам
- X≈0..8, ширина 20 — проходимый коридор, не застраивать.
- **Руды квеста кузнеца:** Z=-50, Z=-70, Z=-90 (X=4, 2, 6) — не ставить пропсы рядом.
- Безопасная сторона для крупного декора у дороги: ПРАВАЯ (X > +12).

### За замком (Z>26)
- Безопасно для деревьев и лугов.

---

## 🧭 КООРДИНАТНАЯ ШПАРГАЛКА (LEVEL0)
> Источник истины: `L0OrcArenaConfig.cs`.

### Ключевые точки (world XZ, Y≈0.25)
| Точка | Координата | Что это |
|-------|-----------|---------|
| `castleGatePoint` | **(0, 0, -16.25)** | Ворота замка, старт игрока |
| `blacksmithCenter` | **(5.75, 0.78, -24.5)** | Центр зоны кузнеца |
| `refugeeVillageCenter` | **(-24, 0, -45)** | Центр деревни людей |
| `RoadStart` | **(0, -35)** | Начало дороги к оркам |
| Руда 1 / 2 / 3 | **(4,-50) (2,-70) (6,-90)** | Квест кузнеца |
| `RoadMid` | **(4, -92)** | Середина дороги |
| `BaseEntrance` | **(8, -120)** | Вход на орочью арену (ворота) |
| `BaseCenter` | **(8, -175)** | ЦЕНТР арены (точка отсчёта всего) |
| `PortalShrine` | **(10, -275)** | ПОРТАЛ — дальний конец арены, финал |

### Оси арены (всё считается от `BaseCenter`)
- **`GateDir` = +Z** (от центра к входу). **`BackDir` = -Z** (к порталу). **`SideDir` = +X** (вправо).
- **Формула:** `BaseCenter + SideDir*X + GateDir*Z`. Пример: `c + s*-58 + g*5` = левый передний.

### Радиальные зоны
| Зона | Радиус | Назначение |
|------|--------|-----------|
| BattleCenter | 0–42 | Открытый боевой пятак — НЕ застраивать |
| TransitionAisle | 42–50 | Переход |
| CampRing | 50–74 | Хижины/лагеря |
| CombatStrongpointRing | 58–112 | 8 боевых точек |
| PerimeterRing | 104–140 | Палисад |
| CliffRing | 150–230 | Горы (scale макс 4.5) |

---

## ⚔ БОЕВАЯ СИСТЕМА — ПАРАМЕТРЫ

### Оружие игрока (`WeaponController.cs`)
| Оружие | Урон | Кулдаун | Ассет |
|--------|------|---------|-------|
| Меч | 40 | 0.28с | `Sword.prefab` sc 0.45 |
| Копьё | 65 | 1.2с | `2Spear.prefab` sc 0.4 |
| Лук | 35 | 0.45с | примитивный |
- Крит 15% → ×1.5 урона
- Награда кузнеца теперь = огненные стрелы (`FireArrowMode`), НЕ +урон мечу. Поля `bonusSwordDamage/Spear/Bow`
  в `WeaponController` остались, но никто их не инкрементит (дефолт 0).

### Огненный шар (`KingAbility.cs` + `Fireball.cs`)
- Разблокируется после `MarkGateRaidCleared` + сообщение
- Заряд +20%/убийство (5 убийств = 100%), Q при полном заряде
- Скорость 40 м/с, AoE 80 урона R=5м с falloff
- HUD: полоска заряда под HP, золотое мигание при готовности

### Типы орков (`L0OrcFactory.cs`)
| Тип | Scale | Speed | HP | Оружие | Поведение |
|-----|-------|-------|----|----|-----------|
| Warrior | 1.45 | 4.2 | 50 | Топор+щит | Стандартный |
| Archer | 1.30 | 4.2 | 50 | Лук (range 24, cd 1.6с) | Дальний бой |
| **Boss** | **2.30** | 4.8 | 100 | Топор+рога | Красный, пульсация HP<40% |
| **Berserker** | **1.4×1.55** | 5.5 | 35 | 2 топора | Ярость при HP<50% (speed×1.6) |
| **Tank** | **2.80** | 2.2 | 90 | Булава+щит | Огромный, тяжёлая броня |
| **Shaman** | **1.10** | 2.8 | 40 | Посох+шар | Дальний (range 22), отступает при <8м |

### Псевдо-анимации (`L0OrcAnimator.cs`)
- Берсерк: покачивание головы (sin(t*8) ±0.06)
- Шаман: парящий магический шар (sin(t*2.5)*0.12 по Y)
- Босс: пульсация красного PointLight при HP<40%
- Танк: покачивание щита (sin(t*1.2)*0.03)

### Волны орков (`Level0OrcVillageManager.cs`)
- **24 орка** в 3 зонах (6+8+10), max **8 одновременно**
- Зона 1: 4 воина + 1 лучник + 1 берсерк
- Зона 2: 2 воина + 2 лучника + 2 берсерка + 1 танк + 1 шаман
- Зона 3: 2 берсерка + 1 танк + 1 шаман + 4 лучника + 2 босса
- spawnInterval=0.45с, zoneEntryDelay=0.8с, chaseRange=28

### Juice боя (`FeedbackManager.cs`, `EnemyAI.cs`)
- Hit: маркер + camera shake + floating text + particle burst + blood vignette
- Kill: slowmo (0.35 timeScale / 0.18с, босс 0.15/0.5с) + ragdoll (Rigidbody + Force/Torque на каждый part)

---

## 🔨 КВЕСТ КУЗНЕЦА / КРАФТ (`Assets/Scripts/Crafting/`)

> ЕДИНАЯ система (2026-06-28). Старый `L0BlacksmithQuest.cs` (+15 урона мечу) **УДАЛЁН** как дубль.
> Главная система — `Crafting/` (под лабу №3: ресурс→рецепт→инвентарь→окно→огненные стрелы).

### Файлы
- `L0CraftingStation.cs` — спавн руды, рецепт, инвентарь, HUD-счётчик, награда (`ApplyFireArrows`).
- `L0Inventory.cs` — singleton-инвентарь (`AddItem/GetCount/HasItem/RemoveItem`, предмет «Руда»).
- `L0CraftingUI.cs` — окно крафта у кузнеца (OnGUI), открывается по E.
- `FireArrowMode.cs` — статфлаги огненных стрел, читает `ArrowProjectile`.

### Поток
1. Поговорить с кузнецом (`talkedToBlacksmith`) → `TrySpawnOres()` спавнит руду + интро-реплика.
2. 3 кристалла руды на дороге (Z=-50/-70/-90, X=4/2/6) со свечением, лейбл «Огненная руда [E]».
3. Подбор по E → `L0Inventory.AddItem("Руда")` → HUD «⛏ Огненная руда: 2/3».
4. Вернуться к кузнецу (SmithPos 5.75,0.78,-24.5, range 4м) → E → окно `L0CraftingUI`.
5. Кнопка «Создать» (нужно 3 руды) → ковка ~2.5с → `FireArrowMode.Activate(5)` = 5 огненных стрел.

### Подключение
- `CastlePrologueBuilder.Start()` → `L0CraftingStation.Ensure()` (станция сама добавляет `L0CraftingUI`).
- Гейт окна — `GameProgressManager.talkedToBlacksmith`. Диалоги — `CastlePrologueBuilder.Instance.ShowDialogue()`.

---

## 🏰 ЗАМОК — ПАТТЕРНЫ РАБОТЫ (CastleGenerator)

- **Класс:** `CastleGenerator : MonoBehaviour` → `Assets/MedievalCastle/Assets/Scripts/CastleGenerator.cs`
- **НЕ ТРОГАТЬ.** Root: `"=== GENERATED_CASTLE_LAB3_BLOCKOUT ==="` / `"=== GENERATED_CASTLE_LAB4_ASSETS ==="`.
- **Порядок Start():** не гарантирован → `Level0CastleDressingBuilder` использует `Invoke("TintCastle", 0.1f)`.
- `CastleGenerator.SetupLighting()` перебивается `L0Layout.SetupWorldLighting()` — это правильно.

---

## 🌲 ДЕРЕВЬЯ И КОЛЛАЙДЕРЫ — ПРАВИЛА

### Когда RemoveAllColliders, когда нет
| Объект | Коллайдер |
|--------|-----------|
| PP-деревья, Polytope-деревья | **ОСТАВИТЬ** (keepColliders=true) |
| PP-камни | **ОСТАВИТЬ** (keepColliders=true) |
| Трава, луга, цветы | убрать (default=false) |
| Горы (PP_Forest_Mountain) | убрать (фон) |
| Dungeon пропсы на арене | `L0Util.Place()` убирает сам |

### Зоны исключения в ForestBuilder
```csharp
IsInsideCastle(x,z)     → |X|<17 && -12<Z<26  // замок
IsInsideBlacksmith(x,z) → X>-3 && X<13.5 && Z>-32 && Z<-17  // кузница (расширено под кроны)
// Оба вызываются в PlaceTree(), PlaceGrass(), PlaceRock()
```

---

## ГРАФ КОДА — КАРТА ФАЙЛОВ И СВЯЗЕЙ

### Цепочка вызовов (кто кого зовёт)
```
НЕЗАВИСИМЫЕ КОМПОНЕНТЫ (Start() в сцене):
  CastleGenerator              ← замок (НЕ ТРОГАТЬ)
  Level0CastleDressingBuilder  ← декор замка + TintCastle + L0BlacksmithArea.Build()
  Level0ForestBuilder          ← лес, горы, луга (seed 44018)

CastlePrologueBuilder (Start, coroutine):
  ├── BuildStory()             ← NPC (+коллайдеры) + оружие + записка
  ├── L0CraftingStation.Ensure() ← крафт: руда→огненные стрелы (+L0Inventory, L0CraftingUI)
  └── Update() → E-interact   ← диалоги, подбор оружия, указ

L0Layout.Build() — ГЛАВНЫЙ ОРКЕСТРАТОР (Start())
  ├── SetupWorldLighting()     ← солнце, туман, ambient
  ├── L0Village                ← деревня
  ├── L0Road                   ← дорога
  └── L0OrcCleanArena.Build()  ← АРЕНА-ОРКЕСТРАТОР
        ├── L0OrcArenaGateModule        ← ворота
        ├── L0OrcArenaCampModule.Build()
        │     ├── BuildArenaFortifications() → L0DungeonFort (seed 70451)
        │     ├── BuildPalisade()
        │     └── BuildRealAssetDressing()
        ├── L0OrcArenaCombatLayoutModule ← 8 strongpoints
        ├── L0OrcArenaEnhancement       ← котёл + портальный путь
        ├── L0OrcArenaPortalModule       ← портал
        └── L0OrcArenaBackdropModule     ← задник

Level0GameFlow (Start, фазы):
  ├── Level0OrcRaidManager     ← рейд у ворот (фаза 1–3)
  ├── Level0OrcVillageManager  ← арена, 3 зоны, 24 орка
  │     └── L0OrcFactory       ← создание орков (визуал + AI)
  │           └── L0OrcAnimator ← псевдо-анимации (покачивание/парение/пульсация)
  └── Level0ExitPortal         ← переход Level0→Level1

KingAbility (Update, Q):
  └── Fireball                 ← снаряд + взрыв AoE

FeedbackManager (singleton):
  ← EnemyAI.TakeDamage()      ← hit markers, floating text, ragdoll
  ← WeaponController           ← camera shake, crit flash
```

### Утилиты (общие для всех билдеров)
```
L0Util.cs                    ← Place(), PlaceSmoke(), FixMaterials(), TintTree(), TintMaterials()
L0OrcArenaConfig.cs          ← координаты/зоны/радиусы (НЕ ТРОГАТЬ)
L0OrcArenaPlacementGuard.cs  ← TryReserve() — антиколлизия (НЕ ТРОГАТЬ)
L0OrcArenaPrimitiveKit.cs    ← CreatePrimitive, CreateGroundPatch, CreateGroup
L0OrcArenaMaterials.cs       ← цвета/материалы
L0Props.cs                   ← фабрика пропсов (хижины, тенты, костры, барабаны, полы)
CastleCharacterVisualFactory ← визуал NPC (кузнец, староста и др.)
CastlePropFactory            ← пропсы у замка (факелы, лавки, бочки)
```

### Папки скриптов → что где
```
Assets/Scripts/
  Combat/         ← WeaponController, EnemyAI, EnemyHitDetector, FeedbackManager,
                     KingAbility, Fireball, ArrowProjectile, SpearProjectile, CameraShake
  Core/           ← PlayerController, PlayerHealth, GameManager, MouseLook, PauseMenu,
                     RunScoreManager, SaveSystem, SceneFader, MainMenu
  Level0Story/    ← CastlePrologueBuilder, Level0GameFlow, GameProgressManager,
                     Level0OrcVillageManager, L0OrcFactory, L0OrcAnimator,
                     L0QuickExitToOne, Level0ExitPortal,
                     Level0OrcRaidManager, L0RaidOrcRunner, L0ObjectiveGuide,
                     L0PlayerHud, CastleIntroGate, L0GateBlocker, L0GateAllies
  Level0Dressing/ ← Level0CastleDressingBuilder, Level0ForestBuilder,
                     L0BlacksmithArea, L0Util, SimpleBillboardLabel,
                     CastleCharacterVisualFactory, CastlePropFactory
  Level0WorldLayout/ ← L0Layout, L0Village, L0Road, L0OrcCleanArena,
                     L0OrcArenaCampModule, L0OrcArenaGateModule, L0OrcArenaEnhancement,
                     L0OrcArenaPortalModule, L0OrcArenaBackdropModule,
                     L0OrcArenaCombatLayoutModule, L0OrcArenaSetDressingModule,
                     L0OrcArenaCombatPropsModule, L0DungeonFort,
                     L0OrcArenaConfig, L0OrcArenaPlacementGuard,
                     L0OrcArenaPrimitiveKit, L0OrcArenaMaterials,
                     L0Props, L0Dragon, L0DragonFly, L0OrcBase, L0Exit
  Level0WorldDressing/ ← Level0WorldDressingBuilder (ОТКЛЮЧЁН), Level0EnvironmentAtmosphere,
                     Level0OrcSiegeCampDressing, WorldDressingPropFactory
  Crafting/       ← L0CraftingStation, L0CraftingUI, L0Inventory, FireArrowMode (крафт кузнеца)
  Transitions/    ← LevelTransitionPortal, TransitionBridgeBuilder
```

### Ассет-паки → где что
```
Assets/DungeonAssetPack/Prefabs/     ← ОСНОВА арены (стены, клетки, факелы, обломки)
Assets/Synty/PolygonGeneric/Prefabs/ ← ДОБОР (камни, огонь, деревья)
Assets/Polytope Studio/              ← ПРИРОДА (мёртвые деревья, кусты, пни)
Assets/Pure Poly/                    ← ЛЕС/ГОРЫ (PP_Tree, PP_Forest_Mountain, PP_Meadow)
Assets/New Folder/Low Poly Medieval Weapons/ ← ОРУЖИЕ (Sword, 2Spear, axe, shield, maul)
Assets/msVFX_Free Smoke Effects Pack/        ← ДЫМ (Smoke 1/2/4)
Assets/MedievalCastle/               ← ЗАМОК (CastleGenerator + ассеты) — НЕ ТРОГАТЬ
```

---

## ⛔ КАКИЕ СКРИПТЫ НЕ ТРОГАТЬ
- `Level0GameFlow.cs` — прогресс и логика фаз
- `Level0OrcRaidManager.cs` / `L0RaidOrcRunner.cs` — рейд у ворот
- `Level0ExitPortal.cs` — переход между сценами (логика)
- `L0OrcArenaConfig.cs` — контракт координат
- `L0OrcArenaPlacementGuard.cs` — система резервации
- `PlayerHealth.cs`, `PlayerController.cs` — ядро геймплея
- `EnemyAI.cs` — боевая система (State, hit detection, ragdoll — НЕ трогать)
- `EnemyHitDetector.cs` — детектор попаданий
- `CastleGenerator.cs` — генератор замка
- `GameProgressManager.cs` — прогресс (флаги talked/weapons/note)

### РАЗРЕШЕНО менять (но осторожно)
- `Level0OrcVillageManager.cs` — числа/позиции спавна орков
- `L0QuickExitToOne.cs` — fallbackPosition
- `WeaponController.cs` — визуал, bonus*Damage поля (не ломать struct Weapon)
- `L0OrcFactory.cs` — визуал орков, масштаб, цвет (не менять CreateOrc контракт)
- `CastlePrologueBuilder.cs` — добавлять поверх (не ломать диалоги/квесты)

---

## 🔨 ЗОНА КУЗНЕЦА (`L0BlacksmithArea.cs`)
- **Центр:** (5.75, -24.5), PY=0.78
- **Bounds:** X [2.95, 8.55], Z [-22.1, -26.9]
- **Расчистка:** 10×9 м EARTH на Y=0.18 (перекрывает луга)
- **Элементы:** платформа → бордюры → забор U → горн (back-wall + пламя + дымоход) → наковальня → верстак → полки → бочка → навес → NPC → арка + вывеска → свет
- **Подключение:** `Level0CastleDressingBuilder.BuildDressing()` → `L0BlacksmithArea.Build(root)`

---

## ПРАВИЛА РАЗМЕЩЕНИЯ И КООРДИНАТ

### Как безопасно поставить объект
1. **На арене (крупное):** `placementGuard.TryReserve(cat, id, pos, footprint, zone)` → false = skip
2. **Арена декор:** `L0Util.Place(path, pos, rot, scale, parent, name)` напрямую (снимает коллайдеры)
3. **Лицом к центру:** `FaceCenter(pos)` или `Quaternion.LookRotation(BaseCenter - pos)`
4. **Масштаб Dungeon:** стены 2×2×0.25 м при scale 1. Для арены scale 3.7–4.3.
5. **Масштаб PP горы:** огромные, ставить |X|>150, scale 2-4.

### Единый источник координат
- **Замок/деревня:** `L0Layout.castleGatePoint` (0, 0, -16.25), `refugeeVillageCenter` (-24, 0, -45).
- **Кузнец:** `L0BlacksmithArea` center (5.75, 0.78, -24.5).
- **Дорога:** `RoadStart` (0, -35) → `RoadMid` (4, -92) → `BaseEntrance` (8, -120).
- **Арена:** `BaseCenter` (8, -175), все зоны радиальные.
- **Портал:** `PortalShrine` (10, -275).

---

## FIXED SEEDS
- **44018** — лес/горизонт (`Level0ForestBuilder.cs`).
- **70451** — кольцо фортификаций (`L0DungeonFort.cs`).

---

## ПОРЯДОК РАБОТЫ

**РЕЖИМ: батчинг, экономия лимитов.**
1. Пользователь задаёт **2-3 шага сразу**.
2. Запускает Unity / Play Mode **один раз** на батч.
3. Кидает скрины → правим разом → дальше.

**Запуск Unity / Play Mode / build / тесты — делает ТОЛЬКО пользователь.** Claude не запускает Unity.

---

## 🎬 ПОТОК СЦЕН (канон — подтверждён 2026-06-28)

```
MainMenu.unity ──► Level0_Castl.unity ──► two.unity ──► Victory
   ▲                    │                     │            │
   │                [смерть]              [смерть]         │
   │                    ▼                     ▼            │
   └──────────────── Death Menu ◄────────────┘            │
   └───────────────────────────────────────────────────────┘
```

- **Вся игра = 2 игровых уровня:** Level0 (замок→рейд→арена орков→портал) + two (4 зоны + босс).
- **`one.unity` — МЁРТВАЯ** (Missing Script `Assembly-CSharp::SceneBuilder`). НЕ в цепочке, не чинить.
- **`two.unity` = финал.** «Царство орков» из дизайн-дока — это и есть эта одна сцена (не многоуровневое).
- Сборка: Build Settings — MainMenu (index 0), Level0_Castl, two. (`one` исключить из билда.)
- Переход Level0→two: gameplay-логика `Level0ExitPortal.cs` (НЕ `L0QuickExitToOne` — это legacy debug).
- Один run = один забег, сейвов нет (дизайн-решение). Смерть → Death Menu → restart грузит Level0_Castl.

---

## 🗺 LEVEL 2 — two.unity / SceneBuilder.cs (полная карта)

> **Источник истины:** `Assets/Scripts/Core/SceneBuilder.cs` (≈3500+ строк, монолит). Читай перед правкой L2.
> `Start()` → `StartCoroutine(BuildRoutine())`: Cleanup → SetupMaterials (11 шт) → Sky → Player(0,1.2,2)
> → Managers → BuildZone1..4 → connectors → borders → `BakeNavMesh()` (runtime NavMeshSurface).

### Зоны (Z-ось, всё завязано на локальные константы z0)
| Зона | Z | Тема | Враги | Ворота открываются |
|------|---|------|-------|-------------------|
| Z1 Холмы | 0–40.5 | Равнина, овраг+мост | 2 Grunt (+ план: +Berserker, +Archer) | после убийства всех |
| Z2 Форт | 42–93.5 | Ров, двор, рампарты, яма | 7: Guard×3, Archer×3, Berserker | после 7 |
| Z3 Храм | 94–149.5 | Лестницы, галереи, алтарь | 6: Guard×2, Archer×2, Shaman×2 | после 6 |
| Z4 Кузня+Собор+Арена | 148–270+ | Лава→собор→арена босса | ~13 + BOSS | кузня→собор после 5; арена без ворот |

- **`RitualGateWithTotems(name,pos,w,h,enemies[])`** — блокирует проход пока `enemies[]` живы. НЕ трогать `RitualGate.cs`.
- **`_walkRoot` vs `_decorRoot`:** NavMesh печётся ТОЛЬКО с `_walkRoot`. Весь декор → `_decorRoot`. Путать нельзя.
- **Шейдеры:** только `static Shader LitShader()` (URP/Lit → Standard → Diffuse). НИКОГДА `Shader.Find("Standard")` напрямую (= розовый в URP).
- **PP-ассеты scale:** PP_Meadow МАКС **0.18** (они огромные!), PP_Tree 1.0–1.6, PP_Rock 0.4–0.55.
- **Хелперы SceneBuilder:** `OrcGrunt/OrcArcher/...()` фабрики, `Coin()`, `HealthPack()`, `Fire()`, `Trap_()`, `Barricade()`, `LightAt()`, `MakeMat(...emission)`, `V23PulsingLight()`. Inner-классы: `LavaPulse`, `V23BobSpin`, `FireFlicker`, `SimpleMessageTrigger`.
- **НЕ ТРОГАТЬ:** `MakeOrcRoot()` сигнатуру, `BakeNavMesh()` порядок, `Player()`+`Managers()`, константы z0.

### Враги two.unity (база `MakeOrcRoot(name,pos,wps,SPEED,DMG,HP,CHASE,ATTACK)`)
| Тип | Scale | Spd | DMG | HP | Chase | Ranged | Score-тип |
|-----|-------|-----|-----|----|----|--------|-----------|
| OrcGrunt | 1.0 | 3.0 | 10 | 42 | 13 | — | OrcWarrior |
| OrcShieldGuard | 1.18 | 2.55 | 14 | 80 | 14 | — | Elite |
| OrcArcher | 0.92 | 3.25 | 9 | 48 | 22 | d8 r19 cd2.15 | OrcArcher |
| OrcBerserker | 1.28 | 3.75 | 20 | 90 | 15.5 | — | Elite |
| OrcShaman | 1.18 | 2.35 | 18 | 95 | 19 | d14 r17 фиолет | Elite |
| OrcWarlord | 1.55 | 2.85 | 26 | 145 | 18 | — | Elite |
| OrcForgeMaster | 1.55 | 2.85 | 30 | 180 | 22 | d18 r15.5 оранж | Elite |
| **OrcFinalBoss** | **2.35** | 2.55 | **42** | **550** | 28 | d22 r22 красный | Boss |

### Босс — `BossArenaController.cs` (3 фазы, НЕ создавать нового босса)
- Триггер арены (0,3.5,z0+100) → Phase1. `isFinalBoss=true` → `OnEnemyKilled` → `TriggerWin()` → Victory.
- **Phase1** (HP>60%): базовый + редкий таран (раз в 14с, ×3.2 speed, первый через 10с).
- **Phase2** (≤60%): speed ×1.4, оранжевая аура, таран раз 7с (×4.2).
- **Phase3** (≤30%): speed ×1.85, красная аура, GroundSlam (AoE 35 урона R3.5м, предупреждение 1.4с), волна `SpawnPhase3Wave()` (4 Grunt+2 Archer+1 Tank), «КРОВАВАЯ ЯРОСТЬ!».
- Boss HP bar — OnGUI верх-центр когда игрок в радиусе. Rage mode при 30% HP встроен в `EnemyAI`.

---

## 🏆 СИСТЕМА ОЧКОВ И РЕКОРДОВ (готова, Phase 1–8 ✅)

**Файлы:** `RunScoreManager.cs` (singleton, DontDestroyOnLoad), `RunScoreData.cs` (plain class), `RecordsManager.cs` (PlayerPrefs).
- **Очки за убийство:** Warrior/Archer +100, Elite +250, Boss +1000.
- **Бонусы:** Headshot +75 (стрела в верхние 25% коллайдера), Crit +50 (15% меч/10% копьё, ×1.5), Long Range +50 (15–29м), Sniper +100 (30м+), Stomp +50.
- **Kill streak** (окно 5с): 3 kills +100, 5 +250, 10 +500.
- **Time bonus** при победе: <10мин +1000, <12 +500, <15 +250.
- **Рекорды в PlayerPrefs:** BestScore, BestKillCount, BestTime, BestKillStreak, BestKillDistance, TotalRuns, TotalKills, BossDefeated.
- **UI (всё через OnGUI, отдельных файлов нет):** Death menu → `PlayerHealth.DrawSimpleDeathMenu`, Victory → `GameManager.DrawWinScreen`, Records → `MainMenu.DrawRecords`, hit marker/kill feed/flash → `FeedbackManager`, Boss HP → `GameManager`/`FeedbackManager`.
- **HUD единый на обеих сценах** (Phase 8): legacy HUD основной, `L0HudCleaner` suppress выключен. HP верх-лево, Score под HP, weapon низ-право.

---

## ⚔ БОЕВЫЕ МЕХАНИКИ ИГРОКА (Батч А, ✅ 2026-06-27)

> Реализовано в `PlayerController.cs` + `WeaponController.cs`. `EnemyAI`/`PlayerHealth`/`FeedbackManager` НЕ тронуты.
- **Dodge Roll:** `Space+WASD` = уклон (force 13, dur 0.20с, cd 1.2с), `Space` стоя = прыжок. Голубой flash + camera tilt ±10° + «⚡ УКЛОН». (Iframes пока НЕТ — план L2-A.)
- **Меч 3-удар комбо:** горизонт(cd0.19,×0.85) → диагональ(cd0.19,×0.85) → тяжёлый(cd0.50,×1.65). Сброс через 0.65с. COMBO x2/x3 в HUD.
- **Конусный хитбокс:** `Vector3.Dot(camFwd, toEnemy.norm) > 0.30` (~106°). За спину больше не бьёт. MELEE_RANGE 3.2м.
- **Hit-freeze:** timeScale=0 на 0.030с (лёгкий) / 0.055с (тяжёлый) / 0.050с (крит) — realtime.
- **Stagger врага:** через `nav.speed=0` 0.22/0.45с (+0.15 крит) из WeaponController. Восстановление `Mathf.Max(saved, nav.speed)` (не ломает rage).
- **Оружие:** меч (комбо), копьё (метание `SpearProjectile`, cd 0.85с), лук (hold-to-charge план L2-B; trail золотой, spread 0.016, speed 60).
- **Звук:** `GameAudioManager.cs` — 11 процедурных SFX (`AudioClip.Create`). Регистрируется в `Managers()`.

---

## 🚧 РОАДМАП «БОЕВОГО РЕЖИМА» (приоритет 2026-06-28)

> Цель пользователя: довести игру до боевого/релизного состояния. Порядок сверху вниз.

### 🔴 1. ЗВУК — игра молчит (КРИТИЧНО)
- `GameAudioManager.cs` есть в two.unity. Убедиться, что подключён и в **Level0** (удары, смерть, урон игроку, подбор, шаги, ambient).
- Файлы: `GameAudioManager.cs`, хуки в `WeaponController`, `EnemyAI`(только вызовы), `PlayerHealth`, `Collectible`. План деталей — см. бывший G-1 (звуки описаны в GAME_DESIGN_DOC).
- Бонус: `MusicManager.cs` — фоновая музыка с нарастанием (Level0 спокойно→рейд→арена; two: Z1→Z4→босс).

### 🔴 2. ПОЛНЫЙ ПРОГОН БЕЗ БАГОВ (КРИТИЧНО)
- 3 чистых прохождения MainMenu→Victory. Проверить: невидимые блокеры на арене орков и входе, переход Level0→two, переход two→Victory.
- L2-A: **Iframes при dodge** (`PlayerHealth.SetInvincible(dur)` + вызов из `PlayerController`).
- L2-C: экран победы над боссом с кнопками (сейчас застывает) — `GameManager.SetGameWon` + меню.

### 🟡 3. БАЛАНС И РИТМ БОЯ (ВЫСОКИЙ)
- Финальная настройка HP/урона/таймингов спавна → честные 12–15 мин.
- Z1 усиление (+берсерк +лучник), Z2 засада при входе во двор. Файлы: `SceneBuilder.cs`, `Level0OrcVillageManager.cs`.
- L2-B: лук hold-to-charge (`WeaponController.cs`).

### 🟢 4. ПОЛИРОВКА ВИЗУАЛА / JUICE (СРЕДНИЙ)
- Атмосфера Z2/Z3/Z4 (дым, руны, лава-частицы). Файл: `SceneBuilder.cs`.
- Огненные партиклы для факелов/костров (`L0Props`, `CastlePropFactory`).
- Level0 финал: дракон (орбита 62→40, scale ×1.5, краснее — `L0Dragon`/`L0DragonFly`), NPC-окружение у замка (`CastlePrologueBuilder`/`L0Props`), градиент земли/света (`L0Layout`).
- Настройки игры (`SettingsMenu.cs`): чувствительность, громкость, качество, FOV.

---

## 📁 ПОЛНАЯ КАРТА КОДА ПО РИСКУ (навигация «куда лезть»)

> **Высокий риск = ядро, менять только по явной задаче.** Полные таблицы по ролям были в удалённом
> `CODE_ARCHITECTURE_AUDIT.md`; здесь — практическая выжимка «что менять для механики X».

| Хочу менять… | Иду в… |
|--------------|--------|
| Сюжетную цель/квест/разблокировку Level0 | `GameProgressManager`, `Level0GameFlow`, `CastleIntroGate`, `Level0ExitPortal` |
| Рейд у ворот замка | `Level0OrcRaidManager`, `L0GateAllies`, `L0RaidOrcRunner`, `L0OrcFactory` |
| Бой на арене орков (Level0) | `Level0OrcVillageManager`, `L0OrcFactory`, `EnemyAI` |
| Геометрию/проходы/коллайдеры арены орков | `L0OrcArenaConfig`, `L0OrcCleanArena`, `L0OrcArenaCampModule`, `L0OrcArenaPrimitiveKit` |
| Оружие/урон игрока | `WeaponController`, `ArrowProjectile`, `SpearProjectile`, `EnemyHitDetector` |
| HUD Level0 | `L0PlayerHud`, `L0ObjectiveGuide`, `L0HudCleaner` + legacy `GameManager`/`PlayerHealth` |
| Всю геометрию/врагов two.unity | `SceneBuilder.cs` (+ `BossArenaController`, `Level2Zone1Dressing`) |
| Очки/рекорды | `RunScoreManager`, `RunScoreData`, `RecordsManager` |
| Меню/переходы | `MainMenu`, `PauseMenu`, `SceneFader` |

**Ядро (НЕ трогать без явной задачи):** `PlayerController`, `MouseLook`, `PlayerHealth`, `EnemyAI`, `EnemyHitDetector`, `Level0GameFlow`, `GameProgressManager`, `Level0OrcRaidManager`, `Level0ExitPortal`, `L0OrcArenaConfig`, `L0OrcArenaPlacementGuard`, `RitualGate`, `BakeNavMesh()`, `CastleGenerator`.
**Legacy-монолиты:** `SceneBuilder.cs` (для two — основной, но не вешать туда сюжет Level0), `MedievalCastle/` (отдельная подсистема).
**Отключено:** `Level0WorldDressingBuilder`, `Level0EnvironmentAtmosphere`, `CastleForestBuilder`/`CastleMountainBuilder` (addForestEnvironment=0).

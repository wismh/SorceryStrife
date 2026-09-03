# Sorcery Strife (MiniJam) — Architecture & DOTS/ECS Migration Plan

> Мета: підняти архітектуру/стиль коду до рівня **tactics-cards** (`C:/Projects/RaccoonsGames/tactics-cards/TacticsCards`), як якщо б проєкт зростив з **Lumenwake Unity Template** (`C:/Projects/Lumenwake/template`), і перевести важку рантайм-логіку на **Unity DOTS/ECS**. Це план для узгодження — конкретні PR підуть після затвердження.

## 0. Референси, з яких береться конвенція

- **Lumenwake template** — bootstrap-ланцюг, Zenject-модулі, поділ `_Studio` (спільне) / `_Project` (гра), source-generated константи.
- **tactics-cards** — C# code style (`CLAUDE.md`), поділ `Scripts/Game/Runtime/{Entities, Gateways, Mechanics, UI}`, стейт-машини потоку гри, конвенція комітів/PR (`.claude/skills/pr`), правило "SerializeField & Inject завжди заповнені — без захисних null-перевірок".

Поточний стан MiniJam (для контрасту) задокументований у [`Docs/GDD.md`](GDD.md).

### 0.1 Паритет стеку з Lumenwake template (поза нумерованими кроками)

Окремо від DOTS-плану, за прямим проханням Женi підтягнутий інструментарій з Lumenwake template один-в-один:

- **BetterFolders** (MIT), **HierarchyDecorator** (git-пакет) — Editor-only, скопійовано з template.
- **Odin Inspector** (платний, Женя підтвердив ліцензію) — скопійовано, поки ніде не використовується.
- **DOTween** замінив **PrimeTween** повністю (5 файлів, `PrimeTween` видалено з проєкту).
- **UniTask** замінив **Coroutines** усюди, де вони були (11 файлів) — `GetCancellationTokenOnDestroy()` скрізь, щоб поведінка "зупиняється при знищенні об'єкта" лишилась ідентичною.
- **BootstrapScene** — окрема порожня сцена (Build Settings індекс 0, `Bootstrap.cs` в `Mechanics/GameBootstrap/`), яка на `Start()` одразу вантажить `MainMenu` — той самий `Bootstrap → MainScene`-хендофф, що в template. Раніше проєкт стартував напряму з `MainMenu.unity` як індексу 0; тепер `MainMenu` = індекс 1, `SampleScene` (Gameplay) = індекс 2. `BootstrapScene` без `SceneContext` — `ProjectContext` резолвиться, як і раніше, коли перша сцена з `SceneContext` (`MainMenu`) завантажиться.

Деталі — коміти `7b4bc7e`, `bbe00f3`, `441a1f1`.

## 1. Цільова структура папок

Замінити плоский `Assets/MiniJam/Scripts/...` на:

```
Assets/_Project/
  Scripts/
    Game/
      Runtime/
        Entities/          # Player, Enemy, Friend — authoring + (згодом) ECS-компоненти
        Gateways/           # заглушки під майбутнє: SaveService, Analytics — поки не потрібні джему, але місце зарезервувати
        Mechanics/
          Casting/          # Caster/Spell/CastersRegister (було CastSystem)
          Inventory/        # PlayerInventory/ItemsRegister (було InventorySystem)
          EnemySpawn/        # EnemySpawner, Wave-дані
          Combat/            # EntityDamagable, урон, ECS combat-системи
          RunFlowStateMachine/  # новий стейт-машина забігу
          GlobalGameStateMachine/ # сцени/меню
          CameraControl/
        UI/
          HUD/ SpellSlots/ ItemSlots/ UpgradeScreen/ ItemSelectionScreen/ MainMenu/
    Tools/                  # спільні утиліти (Utils, ListOfObject тощо)
    _Generated/             # SceneInBuild/PrefabNames через source-generator (як у Lumenwake template)
  Configs/                  # ScriptableObject-и (замість Resources/Spells, Resources/Items — перейти на прямі референси/Addressables пізніше)
  Prefabs/ Art/ Audio/ Scenes/ Settings/
```

Перейменування `MiniJam` → `_Project` — окремий, ізольований коміт (`chore: restructure...`), без логічних змін, щоб diff був чистим і рев'ювся окремо від решти. ✅ зроблено, див. крок 1 нижче.

## 2. Bootstrap / DI

- Ввести `Bootstrap` → `ProjectContextInstaller` (Zenject `ProjectContext`, `Resources`), за зразком Lumenwake template, замість одного `GameInstaller` на сцену.
- `GameInstaller` лишається як `SceneContext`-інсталлер геймплею, розбити на кілька дрібних інсталлерів за фічею (Casting, Inventory, Spawning) — по одному класу на модуль, як у tactics-cards.
- Застосувати контракт tactics-cards: обов'язкові `[SerializeField]`/`[Inject]` — без `if (x != null)`, без `Null*`-заглушок.

## 3. Стейт-машини потоку гри

Зараз потік гри — неявний (`SceneManager.LoadScene` напряму з `Player`/`MainMenu`). Ввести дві машини за зразком tactics-cards (`GlobalGameSM` / `GamePlayFlowSM`):

- **GlobalGameStateMachine** — `MenuState` ↔ `GameplayState` (завантаження сцен, замінить прямі виклики `SceneManager.LoadScene`).
- **RunFlowStateMachine** — `RunPrepareState → WaveState (loop) → LevelUpInterstitialState → RunOverState`. Дає явне місце для ECS-систем "вмикатись/вимикатись" за фазою (наприклад, спавн ворогів працює тільки в `WaveState`).

## 4. Код-стиль

Перенести `CLAUDE.md`/`CodeStyle.md` з tactics-cards практично 1:1 (Allman-style, guard clauses, регіони для великих класів, `SerializeField`/`Inject`-контракт, заборона `async void` крім обробників подій, XML-докстрінги лише для публічного API). Створити `CLAUDE.md` у корені `minijam`, адаптований під:
- Zenject + DOTS/ECS специфіку (замість Spine — тут анімації через `Animator`/`SkinnedMeshRenderer` + `VisualEffect`/`PrimeTween`, треба буде розписати ECS-гібридний контракт окремо, п.5).
- Заборону `Debug.Log` напряму — завести мінімальний `LoggingSystem`-еквівалент (як у tactics-cards).

## 5. DOTS/ECS — обсяг конверсії

Обрано **гібридний підхід**: ECS для масової рантайм-симуляції (де реально є проблема продуктивності — вороги/снаряди зараз не пуляться взагалі), MonoBehaviour+Zenject лишається для orchestration/UI/meta — так само, як у tactics-cards (яка взагалі не використовує DOTS). Повна конверсія всього проєкту (включно з UI, стейт-машинами, інвентарем) в ECS суперечила б і цілі "рівень tactics-cards", і здоровому глузду для джем-гри такого масштабу.

### 5.1 У ECS переїжджає

| Було (MonoBehaviour) | Стає (ECS) |
|---|---|
| `Entity`/`EntityCharacteristics` (стати) | `IComponentData`: `MoveSpeed`, `Attack`, `Health`, `RangeOfAttack`, `AttackSpeed`, `RangeOfPickUp`, `Team` |
| `EntityDamagable.Damage()` | `DamageBufferElement`/`DamageEventSystem` — командний буфер урону, обробка смерті в системі |
| `EnemyMoveController` (FixedUpdate) | `IJobEntity`-система руху до `TargetPositionComponent` (Burst) |
| `EnemyMeleeFight` / `EnemyRangeFight` | `AttackCooldownComponent` + `CombatSystem` (ISystem, Burst) |
| `Projectile`/`FireBallProjectile`/`IceArrowProjectile`/`DevilProjectile`/`ExplosionProjectile`/`MagicFieldProjectile`/`MeteorProjectile` | окремі `IComponentData` + спільна `ProjectileMovementSystem`/`ProjectileCollisionSystem` (snapshot-фізика через `Unity.Physics` або проста overlap-перевірка в job) |
| `Experience` (магніт) | `PickupMagnetSystem` (Burst job по `LocalTransform`) |
| `EnemySpawner` (Coroutine) | `WaveSpawnSystem` (ECS, дані хвиль лишаються в ScriptableObject/`IBufferElementData`, spawn через `EntityCommandBuffer`) |
| `PoolOfObject`/`ListOfObject<Enemy/Projectile>` | природний пулінг ECS (Entity Destroy/Create дешевий, `EntityQuery` замість списків) |

### 5.2 Лишається MonoBehaviour + Zenject

- Гравець-контролер вводу/анімації (`MoveController`, `PlayerAnimator`) — унікальний об'єкт, немає сенсу в ECS.
- `Friend` (компаньйон) — один об'єкт, не масовий.
- Уся UI-шар (HUD, SpellSlots, ItemSlots, UpgradeScreen, ItemSelectionScreen, MainMenu).
- `PlayerInventory`, `CastersRegister`, `ItemsRegister` — orchestration/дані, не perf-критичні.
- `CameraController`, стейт-машини, `Chest`.
- Анімації/VFX (`Animator`, `SkinnedMeshRenderer`, `VisualEffect`, `PrimeTween`) — лишаються GameObject-компаньйонами (hybrid renderer), синхронізованими з ECS-трансформом через baked companion link. Повна конверсія скін-анімацій в ECS (Entities Graphics + Kinematica/подібне) — поза обсягом джем-проєкту.

### 5.3 Рендер: як тримати продуктивність при рості кількості ворогів/ефектів

Прямий запит — більше ворогів і більше ефектів, без просідання FPS. Рекомендація — **не** переводити рендер повністю на `com.unity.entities.graphics` одразу, а розділити за типом актора:

| Тип актора | Кількість на екрані | Рендер |
|---|---|---|
| Снаряди, пікапи (`Experience`), прості VFX-маркери | сотні | **Entities Graphics** (DOTS instancing) — прості non-skinned меші, ECS-transform напряму керує рендером, без жодного GameObject на інстанс |
| Рядові вороги (Minion/Mutant/Ogr/OldMutant/Devil/HotDevil) | десятки-сотні логічних, обмежена кількість видимих одночасно | **ECS-логіка** (рух/AI/бій/health) + **бюджетований пул GameObject-компаньйонів** для візуалу/анімації — компаньйони перевикористовуються між найближчими до гравця/камери entity, а не по одному на кожну сутність. Це дає "нескінченну" кількість симульованих ворогів при фіксованій вартості рендеру/анімації |
| Гравець, Friend, мінібоси (Eye/BigEye) | 1–3 одночасно | лишаються звичайними GameObject + `Animator`/`SkinnedMeshRenderer`, як зараз — кількість мала, якість важливіша за перформанс |
| VFX (вибухи, каст-ефекти) | багато коротких сплесків | пул `VisualEffect`/`ParticleSystem`-префабів через Zenject memory pool (той самий патерн "prefab + pool", що вже описаний у tactics-cards `CLAUDE.md` — **Runtime visuals: prefab + pooling**), тригериться з ECS-систем як one-shot подія, не по одному VFX-об'єкту на снаряд |

Це дає основний виграш (пулінг+ECS для мас-контенту) без інвестиції в GPU skeletal-анімацію під ECS (Vertex Animation Textures / Kinemation-подібні рішення) — той шлях лишаю як **стретч-ціль кроку 12 (perf)**: якщо після профайлінгу бюджетованих компаньйонів `Animator` все ще є вузьким місцем, тоді розглядаємо VAT-бейкинг існуючих анімацій під `Entities Graphics`. Не інвестую в це одразу, бо це окремий пайплайн (потрібен бейкер анімацій, нових шейдерів) — недешево для джем-проєкту, поки немає доказу, що воно справді потрібне.

### 5.4 Пакети

Додати `com.unity.entities` + `com.unity.entities.graphics` (на Unity 6, після апгрейду з п.6) для снарядів/пікапів/простих VFX-маркерів одразу. Ворогів/мінібосів рендеримо гібридно (компаньйони), як описано вище.

### 5.5 Фази міграції (кожна — окремий PR/коміт)

> Нумерація тут — **порядковий номер кроку в плані**, не фінальний `ss-N` з комітів. Реальний `ss-N` кожен коміт отримає лише під час переписування всієї історії (розділ 7) — на `main` вже є 44 коміти (10 з них мердж, два автори: wismh/NeutrinoZh — це одна людина під різними іменами, і Oleksandr Panchenko — співавтор), і поки цей масив не переписаний, нові коміти теж не можуть отримати "справжній" номер. Тому зараз коміти йдуть з чистими `chore:`/`feat:`-повідомленнями без префікса, а `ss-N` проставиться заднім числом одразу всім (і старим, і новим) в останньому кроці.

1. **крок-1** `chore`: реструктуризація папок під `_Project` (без зміни логіки). ✅ зроблено (коміт `7a2e43c`, поки без `ss-N` префікса — нумерація проставиться під час фінального переписування історії, п.7). **Потрібно відкрити проєкт в Editor і перевірити консоль/Build Settings** — я робив перенос через `git mv` без візуальної верифікації.
2. **крок-2** `chore`: `CLAUDE.md` + код-стиль + Zenject SerializeField/Inject контракт застосовані до існючого коду. ✅ зроблено (коміт `20ee0ba`) — прибрано 3 захисні null-перевірки на гарантованих `[Inject]`-залежностях (HUD/CameraController/Experience), доданий `ListExtensions.ValueAtLevel` замість повторюваного тернарника в 7 файлах, `HealthRegeneration.cs` перейменовано під клас `HealthRegenerationCaster`, прибрано 2 невикористані `using`. Reflection-баф у `PlayerInventory` навмисно не займав — під ніж підуть цілком у кроці 3.
3. **крок-3** `refactor`: заміна reflection-баф-системи на явну `StatType`-модель (див. розділ 9) — потрібно **до** ECS-бою, бо reflection несумісний з Burst. ✅ зроблено (коміт `bc154a3`) — `StatType`/`ModifierOp`/`StatModifier`, 4 item-асети переписані під нову структуру (числа ідентичні), `PlayerInventory.ApplyModifiers` замість `GetSumOfBuff`, `Utils.GetAllListProperties` видалено разом з `Utils.cs` (для цього довелось замінити reflection-рендер і в `UpgradeCard` — доданий `Spell.GetDisplayStats()`). Під час рефакторингу знайшов off-by-one у нарахуванні рівня айтема (перший, найслабший тір бафу був недосяжний у грі) — спершу заскопив як окрему задачу, потім за проханням Женi виправив одразу (коміт `4cf590f`): рівень айтема тепер стартує з 0 і працює так само, як `Caster.Level` у заклять (усі 3 тіри кожного айтема тепер досяжні: pickup → upgrade → upgrade).
4. **крок-4** `feat`: Bootstrap/ProjectContextInstaller, GlobalGameStateMachine (заміна прямих `SceneManager.LoadScene`). ✅ зроблено — **справжній** Zenject `ProjectContext`, той самий стек, що в Lumenwake template (коміт `38d03dd` початковий стан-машина/`SceneInBuild`, потім `1463f58` замінив тимчасовий `[RuntimeInitializeOnLoadMethod]`-заглушку на реальний `ProjectContext`). Виявилось, що `Assets/_Project/Resources/ProjectContext.prefab` вже існував у репо з першого коміту (порожній, ніколи нікуди не підключений) — додав туди дочірній `ProjectContextInstaller`, що біндить `GlobalGameStateMachine`. Обидві сцени (`MainMenu`, `SampleScene`) вже мали свій (порожній) `SceneContext`, тож `[Inject] GlobalGameStateMachine` запрацював в обох без додаткового підключення — Zenject резолвить `ProjectContext` як батьківський контейнер автоматично. `MainMenu`/`Player` тепер інжектять стейт-машину напряму, `Bootstrap.cs` видалено за непотрібністю.
5. **крок-5** `chore`: апгрейд Unity 2022.3.53f1 → Unity 6 LTS (окрема ізольована гілка, без інших змін одночасно). ✅ зроблено (Женя запустив і підтвердив, що після реструктуризації + апгрейду все ок).
6. **крок-6** `feat`: підключення `com.unity.entities` + `com.unity.entities.graphics`, базові `IComponentData` + baking для `Entity`/`EntityCharacteristics` (паралельно з MonoBehaviour-версією, не видаляємо одразу). ✅ зроблено — пакети `1.4.8`/`1.4.21` в `manifest.json` (Unity сам резолвне при відкритті, я не міг перевірити тут), `Entities/Ecs/{MoveSpeed,AttackStats,Health,PickupRadius,UnitTeam}` + `EntityStatsAuthoring`+`Baker`. Навмисно ще ніде не підключено (жоден префаб, жодна SubScene) — це просто форма даних, готова для кроку 7.
7. **крок-7** `feat`: конверсія снарядів в ECS + Entities Graphics рендер + Burst job на рух/колізію.
8. **крок-8** `feat`: конверсія ворогів (рух, melee/range атака, здоров'я/смерть) в ECS + бюджетований пул GameObject-компаньйонів для анімацій.
9. **крок-9** `feat`: `WaveSpawnSystem` — переніс `EnemySpawner` на ECB-спавн.
10. **крок-10** `feat`: `PickupMagnetSystem` для `Experience` + Entities Graphics рендер пікапів.
11. **крок-11** `refactor`: прибрати мертвий MonoBehaviour-код (`PoolOfObject<Enemy>`, старі контролери) після того, як ECS-шлях підтверджено стабільним.
12. **крок-12** `perf`: Burst/Job-профайлінг, `RunFlowStateMachine`, оцінка VAT-стретчу для компаньйонів, фінальний прохід по code style.

Кожен крок — окрема гілка `feature/крок-N-опис` від `main`, PR на review, мержиться послідовно (щоб можна було грати на кожному кроці — hybrid дозволяє це).

## 6. Unity-версія

**Підтверджено:** апгрейд `minijam` з **2022.3.53f1** на **Unity 6 LTS**, щоб зрівнятись із tactics-cards і Lumenwake template та отримати актуальний `com.unity.entities`/`com.unity.entities.graphics`. Робимо це окремим ізольованим кроком (**крок-5**) — тільки апгрейд, без інших змін в тому ж PR, щоб у разі проблем було легко звузити причину.

## 7. Переписування історії комітів

**Підтверджено:** переписуємо існуючу історію `main` на місці — **дати комітів (author/committer date) лишаються оригінальні**, змінюється тільки **текст повідомлення**, за новим стандартом і трохи інформативніше, ніж зараз (напр. `save commit` / `lot of work` → конкретний опис того, що змінилось у коміті, судячи з diff).

Конвенція — як у tactics-cards `.claude/skills/pr`, адаптована під префікс **`ss-`**:

- Коміт: `ss-<n> <kind>: <опис>` (`feat`/`fix`/`chore`/`refactor`/`docs`/`perf`), опис — маленькими літерами, по суті diff'а, не просто "lot of work".
- Нумерація `ss-<n>` — наскрізна по хронології `main` (старі коміти отримують `ss-1`, `ss-2`, ... у порядку часу; нові коміти рефакторингу продовжують нумерацію далі).
- `Co-Authored-By: Claude` трейлер зберігається/додається там, де відповідно до історії це змінення дійсно робилось зі мною (для решти старих комітів — не додається заднім числом).

**Механіка (щоб дати не зсунулись):** `git rebase -i --root` з `reword` на кожному коміті з кодом, або `git filter-repo --message-callback` — обидва лишають `GIT_AUTHOR_DATE`/`GIT_COMMITTER_DATE` не займаними, міняється лише message. Порядок виконання:

1. Витягнути повний `git log` з diff по кожному коміту на `main`.
2. Скласти список `старий SHA → новий message` (я підготую і покажу на перегляд **перед** запуском rebase).
3. Rewrite лише комітів, що торкаються коду проєкту (`Assets/`, `Packages/`, `ProjectSettings/`) — мердж-коміти й чисто асет-пак коміти (арт/звук без коду) лишаються як є, тільки з приведеним під стандарт message, без штучного `ss-N`, якщо в них нема коду.
4. Локальний rebase → показати результат (`git log --oneline`) на підтвердження.
5. Force-push **тільки після явного "так/ок/push"** окремим повідомленням у сесії — це переписує `origin/main`, ламає всі існуючі локальні клони/форки (треба буде `git fetch && git reset --hard origin/main` деінде).

Це **останній великий крок** (після кроків 1–12, коли рефакторинг стабілізувався) — переписувати історію, яка ще рухається, означає робити це двічі.

## 8. Рефакторинг системи бафів (апгрейди/айтеми)

Погоджуюсь, що поточний підхід — не той, з яким варто йти в ECS.

### 8.1 Що не так із поточним

`PlayerInventory.GetSumOfBuff(string key)` шукає в кожному триманому `Item` публічну `List<float>`-властивість **з такою ж назвою**, як обчислювана властивість у `Caster` (`Damage`, `Cooldown`, `Radius`, `Projectiles`), і сумує значення (+1 база). Це:

- **Неявний контракт по імені рядка** — ніякої компіляційної перевірки; звідси і вже задокументований баг у GDD ([§12](GDD.md)): `Lens of Gods` називається "магніфікує магію", а фактично впливає лише на `Radius`, бо тільки `MagicFieldCaster` має властивість з такою назвою — суто випадковість найменування, не дизайн-рішення.
- **Не масштабується**: додати новий тип бафу = придумати правильну назву властивості в обох місцях (Item і Caster) і сподіватись, що не буде колізії/тайпо. Немає жодного місця, де видно "які айтеми на що впливають", крім читання коду обох класів.
- **Не сумісно з ECS/Burst**: reflection (`GetType().GetProperties()`, boxing у `List<float>`) взагалі не можна викликати з Burst-скомпільованої job-системи. Якщо лишити як є, бій доведеться або тримати поза Burst (втрата продуктивності — саме та, заради якої переїжджаємо на ECS), або переписувати систему бафів все одно пізніше, тільки вже під тиском, посеред ECS-фази.
- **Одна арифметика на всі бафи** ("+1 і сумуємо") — працює лише тому, що `Ring of Arcane` хитро використовує від'ємні значення для "зменшення" кулдауну. Немає явного способу сказати "це множник", "це флет-бонус", "це відсоток".

### 8.2 Пропоноване рішення — явна `StatType`-модель

```csharp
public enum StatType
{
    Damage,
    Cooldown,
    Radius,
    ProjectileCount,
    HealAmount,
    // додавати сюди при потребі — закритий, явний список
}

public enum ModifierOp
{
    AdditivePercent,  // база * (1 + сума значень) — те, що зараз де-факто відбувається
    Flat,             // база + сума значень
}

[Serializable]
public struct StatModifier
{
    public StatType Stat;
    public ModifierOp Op;
    public List<float> ValuePerLevel;
}
```

- `Item` замість набору довільних `List<float>`-властивостей отримує один `[SerializeField] List<StatModifier> _modifiers` — видно в інспекторі одразу, без відкривання коду Caster'ів, які саме стати й як міняються.
- `PlayerInventory` будує (і кешує, як зараз) не `Dictionary<string,float>`, а `Dictionary<StatType,float>` (а для ECS-фази — плоский компонент `BuffContainer : IComponentData` з полем на кожен `StatType`, або `FixedList64Bytes<float>`, індексований `(int)StatType` — без reflection, без boxing, читається напряму в Burst job).
- Caster: `baseDamage * inventory.GetModifier(StatType.Damage)` замість `PlayerInventory.GetSumOfBuff(nameof(Damage))` — той самий результат, але типобезпечно (перейменування `enum`-значення підхопить рефактор IDE, тайпо неможливе).
- UI-картки (`ItemCard`/`UpgradeCard`), які зараз рендерять назви/значення через той самий reflection-хелпер (`Utils.GetAllListProperties`) — переходять на ітерацію по `_modifiers` з явним `DisplayName` в `StatModifier` (або мапою `StatType → назва` для локалізації пізніше).
- Заодно: винести повторюваний патерн "`Level >= list.Count ? list.Last() : list[Level]`" (він в кожному Caster для власних `Spell`-значень — це окремо від бафів, лишається списками, бо це базові per-level дані, не reflection) в один extension-метод `ValueAtLevel(this List<float> values, int level)` — дрібниця, але прибирає дублікат коду в 5 місцях.

### 8.3 Обсяг міграції

Торкається: 4 `Item`-асети (переписати дані в новому форматі — числа ті самі), 4 `Caster`-класи (заміна виклику баф-лукапу), `PlayerInventory` (заміна словника/методу), `ItemCard`/`UpgradeCard` (заміна reflection-рендеру), видалення `Utils.GetAllListProperties`. Контрольований, ізольований рефакторинг — саме тому він тепер **крок-3**, одразу після код-стилю і **до** будь-якого ECS-бою (п. 5.5), а не як пізній cleanup.

## 9. Відкриті питання

Усі питання закриті. Крок 1 виконано через `git mv` (розділ 5.5) і перевірено в Editor разом з апгрейдом на Unity 6 (крок 5, зроблено достроково — Unity-апгрейд виявився безболісним, тож немає сенсу штучно тримати його на своєму місці в черзі).

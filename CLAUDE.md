Address me as "Zhenya".

# Sorcery Strife (MiniJam) — Quick Context

**Game:** Top-down arena survival, auto-cast spells, finite hand-authored waves, level-up/item-pick screens between fights.

**Code:** `Assets/_Project/` · Unity 6 (6000.3.10f1) · Zenject DI · UniTask (async, not Coroutines) · DOTween (tweens, not PrimeTween) · Odin Inspector · New Input System · URP · Entities/Entities Graphics (groundwork only, migration plan step 6+) · no networking · no tests yet.

Full design reference (implemented scope only): [`Docs/GDD.md`](Docs/GDD.md). Rework plan and current step: [`Docs/Architecture-DOTS-Migration-Plan.md`](Docs/Architecture-DOTS-Migration-Plan.md).

## Boot chain

Real Zenject `ProjectContext`, matching the Lumenwake template's `Bootstrap → ProjectContextInstaller` pattern:

- `BootstrapScene.unity` (Build Settings index 0) is content-free — a camera, an empty `SceneContext` (no local installers; needed only so Zenject injects `Bootstrap` itself), and one `Bootstrap` GameObject (`Mechanics/GameBootstrap/Bootstrap.cs`) whose `Start()` `[Inject]`s `GlobalGameStateMachine` and awaits `Enter<MenuState>()`, mirroring the template's `Bootstrap → MainScene` handoff.
- `Resources/ProjectContext.prefab` — a `ProjectContext` component (Zenject) with a child `ProjectContextInstaller` GameObject wired into its Mono Installers list. This prefab predates this rework (it's in the repo's first commit) but was never actually installed on anything until now.
- `ProjectContextInstaller` (`Mechanics/GameBootstrap/ProjectContextInstaller.cs`) binds `MenuState`/`GameplayState`/`GlobalGameStateMachine` as project-wide singletons. `GlobalGameStateMachine : StateMachineBehaviour<StateBase>` (`Tools/Runtime/StateMachineBehaviour.cs`) takes both states as constructor parameters and just `SetStates(...)` — no initial `Enter()` call, since `Bootstrap` is always a genuinely separate scene from `MainMenu` now, so there's no "already there, don't reload" case to special-case.
- Zenject instantiates `ProjectContext` lazily and `DontDestroyOnLoad`s it the first time any `SceneContext` resolves its parent container — `BootstrapScene.unity`, `MainMenu.unity`, and `SampleScene.unity` all have a `SceneContext`, so `GlobalGameStateMachine` is `[Inject]`-able from any of them automatically (first resolved when `Bootstrap` loads), no per-scene rebinding needed.
- `MainMenu`'s Start button and `Player`'s death handler both `[Inject]` the state machine and call `_stateMachine.Enter<GameplayState>()` / `Enter<MenuState>()` (awaited via `async UniTaskVoid` + `.Forget()`) rather than reaching for a static or `new`-ing a state. `MenuState`/`GameplayState : StateBase` constructor-inject `ISceneLoaderService` (`Gateways/SceneLoading/`) and call `LoadSceneAsync(SceneInBuild.*, unloadRedundant: true)` in `override UniTask Enter()` — no raw `SceneManager.LoadScene` left outside that one gateway.

`GameInstaller` (`SceneContext`, `Mechanics/GameBootstrap/GameInstaller.cs`) still binds gameplay-only singletons on `SampleScene`; nothing there changed — `ProjectContext` bindings simply flow down as a parent container.

## Main systems (folder under `Scripts/Game/Runtime/`)

| System | Path | Notes |
|--------|------|-------|
| Entities | `Entities/{Entity,EntityCharacteristics}.cs`, `Entities/{Player,Enemy,Friend}/` | shared stat base for player/enemies via `EntityCharacteristics` ScriptableObject |
| Casting | `Mechanics/Casting/` | `Spell`/`Caster`/`CastersRegister`, `Casters/`, `Spells/`, `Projectiles/` |
| Inventory | `Mechanics/Inventory/` | `PlayerInventory`, `ItemsRegister`, `Items/`, `StatType`/`ModifierOp`/`StatModifier` — items declare explicit `StatModifier`s; `PlayerInventory.ApplyModifiers(StatType, baseValue)` aggregates them, no reflection (migration plan §8) |
| Combat | `Mechanics/Combat/` | `EntityDamagable`, `Team` |
| EnemySpawn | `Mechanics/EnemySpawn/EnemySpawner.cs` | coroutine-driven finite wave list |
| Chests / Experience / CameraControl / Audio / Input / GameBootstrap | `Mechanics/*` | one folder per mechanic |
| UI | `UI/{HUD,SpellSlots,ItemSlots,UpgradeScreen,ItemSelectionScreen,MainMenu,Common}/` | `Common/` holds shared widgets (`ValueBar`, `TimerLabel`, `DamageNumber`) |
| Tools | `Tools/Runtime/` | generic utilities with no gameplay dependency (`Utils`, `ListExtensions`, `ListOfObject`, `PoolOfObject`, `TempObject`, `ForBetter`) |

## Agent rules

- **SerializeField / Inject:** required inspector and DI fields are always wired — no `Null*` fallbacks or `if (x)` guards in installers or `Construct` methods (see "SerializeField & Inject contract" below). `GetComponent<T>()` results that are genuinely optional per prefab (e.g. an enemy has either `EnemyMeleeFight` *or* `EnemyRangeFight`, never both) are **not** covered by this rule — that null check is correct and should stay.
- Match `Game`/`Game.InventorySystem` namespaces and existing folder-per-feature layout under `Mechanics/`
- Keep diffs minimal; no drive-by refactors — flag opportunistic cleanups instead of bundling them
- Pooling: only `Experience` is pooled today (`PoolOfObject<T>`); enemies/projectiles `Instantiate`/`Destroy` directly — this is a known gap the DOTS/ECS migration addresses (see plan §5)

## Runtime visuals: prefab + pooling

For gameplay visuals spawned during runtime (projectiles, popups, particles, temporary FX):

- Do not create view `GameObject` hierarchies via `new GameObject(...)` in gameplay systems.
- Prefer a Zenject memory pool (`FromMonoPoolableMemoryPool` / `PoolOfObject<T>`) over raw `Instantiate`/`Destroy` for anything spawned repeatedly — most of the current codebase does not follow this yet (see migration plan §5.1); new code should.

## SerializeField & Inject contract

Required `[SerializeField]` on installers/MonoBehaviours and scene/prefab references are **always assigned in the Unity Inspector**. Code must not defend against them being missing.

`[Inject]` dependencies resolved by Zenject are **always bound** in `GameInstaller`/`ProjectContextInstaller` for the scope they're used in. Do not add runtime null checks or no-op implementations "just in case" DI failed.

**Constructor injection for plain classes, `[Inject] Construct()` only for MonoBehaviours** — Unity blocks custom constructors on `MonoBehaviour`, so `Construct()` is the only option there; every other class (`PlayerInventory`, `CastersRegister`, every `Caster`) takes its dependencies through a real constructor, and installers should let `AsSingle()` construct it rather than hand-building an instance and binding it with `FromInstance()`. Full rule + when `FromInstance()` is actually correct: [`.claude/skills/zenject-ui-subscriptions`](.claude/skills/zenject-ui-subscriptions/SKILL.md). Also see [`.claude/skills/unity-meta-files`](.claude/skills/unity-meta-files/SKILL.md) before hand-editing any `.meta`/`.prefab`/`.unity` file, and [`.claude/skills/pr`](.claude/skills/pr/SKILL.md) for the branch/commit/PR workflow.

### Do not

- Wrap `Construct(...)` bodies in `if (dependency)` when the parameter is a guaranteed `[Inject]`
- Add `if (!_field) return;` at the top of `Update()` for a field that was assigned from a guaranteed `[Inject]`/`[SerializeField]` and is never destroyed while the object holding it is alive
- Introduce `Null*` services as fallbacks for missing prefabs
- Use `prefab ? prefab : otherPrefab` for a second required serialized prefab

### Do

- Bind and use dependencies directly
- Reserve null checks for **optional** data: a `GetComponent<T>()` that legitimately may not exist on every prefab variant, a dictionary/list lookup that can miss (`PlayerCaster.GetCasterOfSpell`, `ItemsRegister.GetItemByType`), or a singleton-guard pattern (`BackgroundMusic`)

### Example

```csharp
// BAD — treats a guaranteed Inject as optional
[Inject]
public void Construct(Player player)
{
    if (!player)
        return;

    _player = player;
}

private void Update()
{
    if (!_player)
        return;

    _healthBar.Value = _player.Health / _player.MaxHealth;
}

// GOOD — Player is bound as a singleton in GameInstaller and never destroyed
// while this object's Update() can run
[Inject]
public void Construct(Player player)
{
    _player = player;
}

private void Update()
{
    _healthBar.Value = _player.Health / _player.MaxHealth;
}
```

```csharp
// GOOD — GetComponent<T>() here is genuinely optional: an enemy prefab has
// either EnemyMeleeFight or EnemyRangeFight, never both
if (_enemyMeleeFight) _enemyMeleeFight.OnAttack += AttackHandle;
if (_enemyRangeFight) _enemyRangeFight.OnAttack += AttackHandle;
```

# C# Code Style & Best Practices

Applies to `**/*.cs`.

## General Principles

- Use interfaces where more than one implementation is plausible (not everywhere by default)
- Readable > Clever
- Explicit > Implicit (but smart use of `var` or `new()` is encouraged)
- Fail fast: validate inputs early
- Favor immutability and single responsibility
- Code should explain itself: comments only when necessary

## Formatting & Structure

### Braces

Allman style (braces on a new line):

```csharp
if (condition)
{
    DoSomething();
}
```

- Single-line bodies still use braces for `if`, `for`, `while`
- Prefer fewer symbols: `if (entity.IsAlive)` not `if (entity.IsAlive == true)`

### Indentation

4 spaces, no tabs.

### Line Length

Max 120 characters per line when possible.

### Empty Lines

Use empty lines to separate logical blocks; avoid excessive spacing.

### Regions

Most classes in this codebase are small — skip regions. Reserve `#region` for genuinely large classes (2–4 regions max); never wrap a handful of methods in one.

### Spacing

- Space after keywords: `if (`, `for (`, `while (`
- Space around operators: `a = b`, `i < 10`
- No space before `;` in `for`: `for (int i = 0; i < n; i++)`

## Naming Conventions

| Element        | Style      | Example           |
|----------------|------------|--------------------|
| Class / Struct | PascalCase | `EnemySpawner`    |
| Method         | PascalCase | `SpawnEnemy()`    |
| Variable       | camelCase  | `playerHealth`    |
| Private Field  | _camelCase | `_spawnDelay`     |
| Constant       | PascalCase | `BaseAttackDuration` |
| Interface      | I + PascalCase | `IDamageable`  |
| Enum           | PascalCase | `Team.Ally`       |

`[SerializeField] private` also uses camelCase. File name matches the type it declares (`HealthRegenerationCaster.cs` holds `HealthRegenerationCaster`, not `HealthRegeneration.cs`).

## var and new() Usage

- Use `var` when the type is obvious: `var enemy = new Enemy();`
- Prefer explicit type when unclear: `Dictionary<StatType, float> buffs = BuildBuffs();`
- `new()` without a type is fine for field/property init: `private readonly List<int> _ids = new();`

## Control Flow

Prefer guard clauses over nesting:

```csharp
if (!entity.IsAlive)
    return;

if (!_attacking)
    return;

// main logic here
```

Avoid deeply nested code; max 2 levels of nesting.

## Clean Methods

- Method name describes what it does
- One thing per method
- ~20–40 lines max as a rough guide

## Performance

- Avoid allocations in `Update`/`FixedUpdate`
- Cache component references in `Awake`: `_rigidbody = GetComponent<Rigidbody>();`
- Prefer string interpolation over `string.Format`
- Prefer `foreach` over `for` unless indexing is needed

## Null & Error Handling

- Use null-coalescing (`??`) and null-conditional (`?.`) for genuinely optional data
- Do not add defensive null checks for required dependencies guaranteed by design — see "SerializeField & Inject contract" above

## Commenting

- Don't describe *what* — describe *why*, and only when non-obvious
- XML docs for public APIs that aren't self-explanatory from their name

## Rule: avoid returning a raw multi-condition boolean expression

```csharp
// Bad
return !attacking && entity.IsAlive;

// Good
var canCast = !attacking && entity.IsAlive;
return canCast;
```

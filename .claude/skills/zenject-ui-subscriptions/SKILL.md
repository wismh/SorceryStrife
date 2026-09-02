---
name: zenject-ui-subscriptions
description: >-
  Zenject-injected MonoBehaviour views must subscribe to services/events in
  Start, not OnEnable, and must get their dependencies through [Inject]
  Construct() (MonoBehaviours) or a real constructor (plain C# classes) - never
  a manually pre-built instance handed to FromInstance() when Zenject could
  just construct it. Use when fixing NullReferenceException on injected
  fields, wiring event handlers on views, adding a new installer binding, or
  reviewing Zenject + Unity lifecycle code.
---

# Zenject injection & subscription rules (Sorcery Strife / MiniJam)

Adapted from tactics-cards' `zenject-ui-subscriptions` skill, plus a dependency-injection rule of our own.

## Rule 1 — Subscribe in Start, not OnEnable

On scene `MonoBehaviour` views with `[Inject]` dependencies:

- **Subscribe in `Start()`** — Zenject has injected by then.
- **Unsubscribe in `OnDisable()`** — avoid leaks when the view is hidden. (This codebase's existing convention is `OnDestroy()` for permanent scene objects that are never toggled — see `HUD`, `ItemSlots`, `SpellSlots`. Use `OnDisable()` only for views that actually get shown/hidden via `SetActive`, e.g. `ItemSelectionScreen`/`UpgradeScreen` cards.)
- **Do not subscribe in `OnEnable()`** — it can run before `[Inject] Construct`, causing null refs on injected fields.
- **Do not use `TrySubscribe` / guarded subscribe helpers** — that is an antipattern here; per the SerializeField & Inject contract in `CLAUDE.md`, an injected dependency is guaranteed present once `Start()` runs.

### Pattern

```csharp
[Inject]
public void Construct(IMyService myService)
{
    _myService = myService;
}

private void Start()
{
    _myService.Changed += OnChanged;
    Refresh();
}

private void OnDestroy() // or OnDisable() for toggled views — see note above
{
    _myService.Changed -= OnChanged;
}
```

### Anti-pattern (do not add)

```csharp
// BAD — OnEnable before inject
private void OnEnable()
{
    _myService.Changed += OnChanged;
}

// BAD — try-subscribe guards
private void TrySubscribe() { ... }
```

## Rule 2 — Constructor injection for plain classes; `[Inject] Construct()` only for MonoBehaviours

Unity does not allow custom constructors on `MonoBehaviour` subclasses, so `[Inject] public void Construct(...)` is the only option there (see every `Entities/`/`UI/` view in this project, and tactics-cards' own `AGENTS.md`/skills — this is their convention too, not something to avoid).

For a **plain C# class** (not a `MonoBehaviour`), Zenject should construct it and inject its dependencies through its own constructor — that's what `[Inject] public ClassName(Dep dep)` on the constructor means, and what every existing service in this codebase already does (`PlayerInventory`, `CastersRegister`, `ItemsRegister`, every `Caster` subclass).

**Do not** hand-construct a plain class yourself in an installer and register the pre-built instance:

```csharp
// BAD — bypasses Zenject's own construction; init logic doesn't belong in InstallBindings
public override void InstallBindings()
{
    var stateMachine = new GlobalGameStateMachine();
    stateMachine.SetInitial(new MenuState());

    Container.Bind<GlobalGameStateMachine>().FromInstance(stateMachine).AsSingle();
}
```

Instead, put the initialization in the class's own constructor and let `AsSingle()` construct it:

```csharp
// GOOD
public class GlobalGameStateMachine : StateMachine
{
    public GlobalGameStateMachine()
    {
        SetInitial(new MenuState());
    }
}

public override void InstallBindings()
{
    Container.Bind<GlobalGameStateMachine>().AsSingle();
}
```

`FromInstance(...)` stays correct — and necessary — for binding something Zenject genuinely cannot construct itself: an existing scene MonoBehaviour assigned via `[SerializeField]` (`Container.Bind<Player>().FromInstance(_player).AsSingle();` in `GameInstaller`), or a type whose constructor needs a concrete Inspector value alongside injectable dependencies (`PoolOfObject<Experience>` needs `_experiencePrefab`, which nothing can resolve generically). The test: if every constructor argument is either resolvable by Zenject or absent, use `AsSingle()` and let the constructor do the work; reach for `FromInstance()` only when something in the mix genuinely can't be resolved that way.

---
name: unity-meta-files
description: >-
  Create and fix Unity .meta files with valid GUIDs and importer blocks. Use when
  adding Unity assets from outside the Editor, hand-writing .meta files, fixing
  missing script references, or updating prefab/scene script GUID references.
---

# Unity .meta files (Sorcery Strife / MiniJam)

Adapted from tactics-cards' `unity-meta-files` skill.

## Do not confuse compile errors with broken meta

| Symptom | Likely cause | Fix |
|--------|----------------|-----|
| `CS0246: type or namespace name 'X' could not be found` on a type in `Assets/_Project/Scripts/` | Missing `using`, typo, or deleted type — **not** a broken `.meta` | Restore the correct `using` / reference; recompile |
| `Missing (Mono Script)` on a prefab or scene component | Script `.meta` GUID mismatch or placeholder GUID | Align prefab `m_Script` GUID with the script `.meta` |
| New `.cs` added outside Unity with no `.meta` | Unity has not imported the asset yet | Add a proper `.meta` or let Unity regenerate |

**Rule:** When editing C# files, never remove a `using` just because one type from that namespace was deleted. Search the file for remaining references first.

## C# script `.meta` template

Always keep the existing `guid:` when renaming/moving a file — GUID identity is what keeps scene/prefab references intact, not the path. Only generate a new GUID for a brand-new script.

```yaml
fileFormatVersion: 2
guid: <32 lowercase hex chars>
MonoImporter:
  externalObjects: {}
  serializedVersion: 2
  defaultReferences: []
  executionOrder: 0
  icon: {instanceID: 0}
  userData:
  assetBundleName:
  assetBundleVariant:
```

### Invalid patterns (do not use)

- Placeholder GUIDs: `a1b2c3d4e5f6789012345678abcdef01`
- GUID-only stubs with no `MonoImporter:` block on **new** agent-created script metas
- Reusing the same GUID for two different assets

Generate a real GUID (this repo, from Bash): `node -e "console.log(require('crypto').randomBytes(16).toString('hex'))"` — check the output is exactly 32 hex characters before using it.

## ScriptableObject asset `.meta` and data

`Item`/`Spell` subclasses use `[field: SerializeField]` auto-properties, so their YAML backing field is `<PropertyName>k__BackingField:`, not the bare property name — see `Assets/_Project/Resources/Items/*.asset` for the pattern. A plain `[SerializeField] private` field (no auto-property) serializes under its own field name instead — see `EnemySpawner.Wave`/`StatModifier` for that pattern. Match whichever style the class actually uses; mixing them up silently loses the data on next Editor save.

## Prefab and scene script reference

MonoBehaviour component on a prefab or in a scene:

```yaml
m_Script: {fileID: 11500000, guid: <script-meta-guid>, type: 3}
```

After creating or rewriting a script `.meta`, grep the repo for the old GUID and update prefabs/scenes if the GUID changed.

## Hand-editing prefabs and scenes

Prefer editing a `.asset`/script `.meta` over a `.prefab`/`.unity` file — the latter carry cross-referenced `fileID`s between GameObject/Transform/Component blocks within the same file, which is easy to get subtly wrong with no Editor open to catch it. When a prefab/scene edit is genuinely necessary:

1. Find a real, working block of the same shape already in this project (e.g. `SampleScene.unity`'s `SceneContext` + child installer GameObject) and mirror its exact structure — GameObject `m_Component` list, Transform `m_Father`/`m_Children`, MonoBehaviour `m_GameObject` — rather than guessing the format from memory.
2. Pick new `fileID`s that obviously can't collide with the ones already in that file (the IDs only need to be unique **within** the file).
3. Re-read the finished file once and manually trace every `fileID` cross-reference before treating the edit as done.
4. Tell Zhenya this specific file needs an Editor open + Play Mode check — it's the one class of edit in this repo that can't be self-verified from a diff.

## Audit checklist (after hand-editing metas or usings)

1. `grep` the edited `.cs` for types from each removed `using` namespace.
2. For touched script metas: `guid` is 32 hex chars; `MonoImporter:` present for new files.
3. For touched prefabs/scenes: `m_Script` GUID matches the script `.meta`; every `fileID` referenced actually exists in the file.
4. For touched `.asset`/prefab data files: confirm which serialization style applies (see "ScriptableObject asset" above) before writing field names.
5. Prefer fixing compile errors in `.cs` first; only chase meta when Unity reports missing scripts or broken references.

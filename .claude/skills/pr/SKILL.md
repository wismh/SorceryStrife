---
name: pr
description: >-
  Creates a GitHub pull request from main: branch kind/code-description,
  commit "code kind: description", push, then create the PR with gh against
  main. Use when the user asks to create a PR, open a pull request, /pr,
  зробити PR, створити пул реквест, or push a feature branch for review.
---

# Create PR (Sorcery Strife / MiniJam)

Address the user as Zhenya. This skill overrides the default "run git immediately / in parallel" workflow. Adapted from tactics-cards' `pr` skill — see [`Docs/Architecture-DOTS-Migration-Plan.md`](../../../Docs/Architecture-DOTS-Migration-Plan.md) §7 for why the ticket prefix below is provisional.

## Confirmation

Run inspect, branch, and commit **without asking**. Show planned branch name and commit subject in the recap.

**Ask only before push.** Show the exact push command, then wait:

1. Ask: чи все добре, пушити?
2. **Stop.** Do not push in the same turn as the question.
3. Push only after a clear yes (`так`, `ok`, `go`, `роби`, `пуш`).

If Zhenya already said to push in the same message (`пуш`, `push`), skip the question and push.

Do not use TodoWrite or Task for this workflow.

## Names

| Role | Pattern | Example |
|------|---------|---------|
| Branch | `kind/description` | `feature/enemy-ecs-conversion` |
| Commit | `kind: description` | `feat: convert enemy movement to ECS` |
| PR title | same as commit subject | `feat: convert enemy movement to ECS` |

- Branch `kind`: full word (`feature`, `fix`, `chore`, `refactor`, `docs`, `perf`).
- Commit `kind`: short form (`feat`, `fix`, `chore`, `refactor`, `docs`, `perf`).
- Branch description: kebab-case. Commit description: lowercase words, same meaning.
- Base branch is always `main` (no `develop` branch in this repo).

**`ss-<n>` ticket prefix:** the target convention is `ss-<n> kind: description`, matching tactics-cards' `tac-<n>` — but the real numbering only gets assigned once the pre-rework history is rewritten (migration plan §7, not done yet). Until then, commit with a plain `kind: description` subject, no `ss-` prefix — do not invent a number.

## Flow

### 1. Inspect

```powershell
git branch --show-current
git status
git diff
git log -8 --oneline
```

Do not commit secrets (`.env`, credentials). Do not empty-commit.

### 2. Branch

**On `main`:** create a new branch from current HEAD, then continue with commit + push.

```powershell
git checkout -b feature/enemy-ecs-conversion
```

**Not on `main`:** do **not** create a branch. Commit and push on the current branch.

Uncommitted files follow `checkout -b`. Do not switch away from `main` onto some other existing branch to "start" the PR.

If the current branch is unrelated work and this change must land on `main`, stash, `checkout main`, `git pull`, then `checkout -b` the new branch and restore the stash.

### 3. Commit (Windows PowerShell)

Stage only the files that belong in this PR. Use `git add` paths explicitly.

```powershell
git add path/to/file
git commit -m "feat: convert enemy movement to ECS"
```

Use a single `-m` string. Do not use bash HEREDOC. Do not `git add -i`. Do not `git commit --amend` unless Zhenya asked and the usual amend safety checks pass.

Claude appends a `Co-Authored-By: Claude` trailer. Leave it. Do not strip trailers, do not add extra `-m` co-author lines by hand, and do not use commit-template hacks.

Follow the repo commit user rules: no `git config`, no `--no-verify`, no `--no-gpg-sign`.

After commit, run `git status` to verify.

Do **not** stage `.claude/skills/pr/` unless Zhenya asked to commit the PR skill itself.

### 4. Push

Ask first (unless Zhenya already said to push):

```powershell
git push -u origin HEAD
```

Use `-u` when the branch has no upstream (first push). Later pushes: `git push`.

Never force-push to `main`. Never `--force` unless Zhenya explicitly asks, and still warn.

### 5. Create the GitHub PR with `gh`

After a **first** push succeeds (new upstream), create the pull request with `gh`. Do **not** open the compare page and wait for Zhenya to fill the form.

```powershell
gh pr view --json url -q .url
```

If that already returns a URL, the PR exists. Print it and stop.

Otherwise create it against `main`:

```powershell
$title = "feat: convert enemy movement to ECS"
$body = @"
## Summary
- One to three bullets of why this change exists.

## Test plan
- [ ] Concrete in-game or editor checks for this change
"@
gh pr create --base main --title $title --body $body
```

- Title = commit subject (or the main commit if there are several).
- Body: `## Summary` then `## Test plan`.
- `--draft` only if Zhenya asked for a draft.
- Do **not** use `gh pr create --web` unless `gh pr create` fails (auth/repo) and Zhenya confirms opening the browser.
- If `gh` is missing or auth fails, report the error, then fall back to:

```powershell
Start-Process "https://github.com/wismh/minijam/compare/main...BRANCH?expand=1"
```

Print the PR URL in the recap.

If the branch already had an upstream, just push. Then `gh pr view --json url -q .url` and create only when there is no PR yet.

## Unity package-reimport noise

Editor/package version bumps (TextMesh Pro, URP, VFX Graph, Input System, Zenject) can silently touch dozens of files on first open after a Unity or package upgrade. Only include that churn in a commit when the commit *is* the upgrade (see the `chore: upgrade project to Unity 6` commit for the pattern) — never let it ride along in an unrelated feature/fix commit. Check `git status` for unexpected reimport diffs before staging.

## Safety

- Never `git config`
- Never skip hooks
- Never `git add -i` / `git rebase -i` (no interactive git)
- Never destructive git unless Zhenya explicitly requested it in this message
- Leave the `Co-Authored-By: Claude` trailer on commits

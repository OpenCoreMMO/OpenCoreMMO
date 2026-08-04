---
name: pull-request
description: >
  Create a pull request for OpenCoreMMO following the project's PR template.
  Use this skill when the user wants to open a PR, publish changes, or create
  a merge request. Covers branch naming, commit messages, PR title style, and
  the expected description format.
compatibility: Requires GitHub CLI (`gh`) or GitHub API access. Designed for
  NeoServer project conventions.
---

## Workflow

- [ ] 1. **Branch** — create a feature branch from `develop` named `feat/<short-description>` or `fix/<short-description>`
- [ ] 2. **Commit** — use conventional commits: `feat:`, `fix:`, `refactor:`, `test:`, `chore:`, etc.
- [ ] 3. **Push** — `git push origin <branch-name>`
- [ ] 4. **Open PR** — use `github_create_pull_request` with the template below

## PR Title

Must be **feature/domain oriented**, not implementation-oriented. Describe **what the user or game benefits from**, not the code mechanism.

| Good (feature/domain) | Bad (technical) |
|---|---|
| `feat: persist haste conditions between player sessions` | `feat: implement HasteCondition CaptureState/Restore and parser` |
| `fix: prevent duplicate creature spawns on map reload` | `fix: add null check in SpawnMonsterRoutine` |
| `feat: allow players to trade while sitting` | `feat: add IsSeated check to TradeService` |

## PR Template

Use the exact structure below. Keep descriptions **minimalistic and direct**.

```markdown
### Description
<one or two sentences describing what was changed and why>

### Key Changes
- <bullet points of the most relevant changes, not an exhaustive list>

### Types of Changes
- [ ] Bug fix (non-breaking change which fixes an issue)
- [ ] New feature (non-breaking change which adds functionality)
- [ ] Breaking change (fix or feature that would cause existing functionality to change)
- [ ] Documentation fix (typos, incorrect content, missing content, etc.)
- [ ] Code refactoring
- [ ] Merge Down

### Test Case
<how the change was tested — "dotnet test tests/" or specific project>
```

### Guidelines

- **Description**: 1-2 sentences. No fluff. Say what changed and why.
- **Key Changes**: 3-5 bullets max. Focus on what matters to a reviewer — omit trivial refactors.
- **Types of Changes**: Mark exactly one box (rarely two). `Merge Down` for syncing branches.
- **Test Case**: Be specific. Usually `dotnet test tests/` or a single project.
- Do not include implementation details that don't affect behavior (e.g. "added new file", "moved method").
- Do state domain benefits: "players no longer lose haste on logout", "reduces server startup memory by 20%" etc.

## Branch Naming

```
feat/<short-kebab-description>
fix/<short-kebab-description>
refactor/<short-kebab-description>
```

Examples: `feat/haste-condition-persistence`, `fix/login-race-condition`, `refactor/condition-parser-registration`

## Commit Style

Follow conventional commits as described in AGENTS.md. The commit message should match the PR title in scope but can be slightly more technical.

## PR Creation

Use `github_create_pull_request` with:
- `owner`: `OpenCoreMMO`
- `repo`: `OpenCoreMMO`
- `base`: `develop`
- `head`: the feature branch name
- `title`: the domain-oriented title
- `body`: the formatted template above

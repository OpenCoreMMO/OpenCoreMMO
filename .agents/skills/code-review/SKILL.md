---
name: code-review
description: >
  Review pull requests and code changes for OpenCoreMMO against the
  project's architecture, conventions, and style defined in AGENTS.md.
  Covers DDD layering, naming, testing policy, security, CI/CD, Lua
  scripting, and the EventAggregator / Factory / DI patterns. Works
  with the GitHub CLI for PR review workflows.
compatibility: Requires GitHub CLI (`gh`) for PR review workflows.
  Designed for NeoServer project conventions.
---

## Scope

This skill enforces the conventions documented in `AGENTS.md` during code review. Use it when:

- Reviewing a pull request (yours or someone else's)
- Auditing code for consistency before committing
- Checking whether a change follows project architecture
- Writing a PR review comment that cites project policy

## Cross-References

For deeper guidance on specific areas, use these sibling skills:

| Area | Skill |
|---|---|
| Writing unit tests | `unit-testing` (tests/NeoServer.Domain.Tests etc.) |
| Writing functional/integration tests | `functional-testing` (tests/NeoServer.Server.Tests) |
| Opening a new PR | `pull-request` (branch naming, PR template) |

---

## Choosing a Mode

Pick one before starting:

| Mode | When to use |
|---|---|
| **Review PR** | Someone opened a pull request. You fetch it, inspect, and leave feedback. |
| **Review Staging** | You have local changes (staged or unstaged) and want to audit them before committing. |

---

### Review PR

- [ ] 1. **Fetch the PR** — `gh pr checkout <number>`
- [ ] 2. **Build** — `dotnet build src/Standalone --configuration Release`
- [ ] 3. **Run tests** — `dotnet test tests/` (or single project if the change is scoped)
- [ ] 4. **Walk the checklist** below
- [ ] 5. **Submit review** — `gh pr review <number> --approve|--comment|--request-changes --body "$(cat review.md)"`

---

### Review Staging

Use before committing to catch issues early. Works on staged and/or unstaged changes.

- [ ] 1. **Check status** — `git status` to see what's staged and unstaged
- [ ] 2. **Review the diff**
     - Staged: `git diff --staged`
     - Unstaged: `git diff`
     - Both: `git diff HEAD`
- [ ] 3. **List new files** — `git diff --staged --name-status` or `git ls-files --others --exclude-standard` for untracked
- [ ] 4. **Build** — `dotnet build src/Standalone --configuration Release`
- [ ] 5. **Run tests** — `dotnet test tests/` (or the relevant project)
- [ ] 6. **Walk the checklist** below against the diff
- [ ] 7. **Fix issues** or commit if clean

> Pro tip: pipe the diff into a file and annotate it with checklist findings:
> ```bash
> git diff HEAD > review.diff
> # walk checklist, annotate review.diff, then:
> git commit -m "feat: ..."
> ```

---

## Review Checklist

### 1. Architecture & Layering

No domain project (`NeoServer.Domain`) may depend on infrastructure (networking, database, IoC).

- [ ] **Domain purity** — Does `NeoServer.Domain` reference any `NeoServer.Data`, `NeoServer.Networking`, or IoC types? If so, it must be moved.
- [ ] **Layer direction** — Dependencies flow inward (Infrastructure → Application → Domain). No inward → outward references.
- [ ] **Domain services** — Business logic lives in domain services (`*Service`), not in entities or handlers. Entities own identity and state, not workflows.
- [ ] **EventAggregator** — Cross-component communication uses `IEventAggregator.Invoke()`, not direct handler-to-handler calls. Events are `record` types implementing `IEvent`.
- [ ] **Handler order** — Network handlers implement `INetworkingEventHandler<T>`; application handlers implement `IApplicationEventHandler<T>`. Network handlers fire first. Verify the correct interface is used.
- [ ] **Factory creation** — Game entities (items, creatures, monsters, NPCs, tiles) are created through their factory (`IItemFactory`, `ICreatureFactory`, etc.), not directly with `new`. Factories handle wiring and data store lookups.

### 2. Dependency Injection

All game services are **singletons**.

- [ ] **Registration** — Is every new service/factory/handler registered in the correct module under `src/Standalone/IoC/Modules/`? (`ServiceInjection.cs`, `FactoryInjection.cs`, `EventInjection.cs`, etc.)
- [ ] **Module placement** — Does the registration belong in the existing module or needs a new one?
- [ ] **Lifetime** — Verify singletons don't hold request-scoped state (game services are singletons by policy).
- [ ] **Interface extraction** — Does the service have an `I*` interface consumed by other layers?

### 3. Naming & Code Style

- [ ] **Namespaces** — Match folder structure exactly. Root: `NeoServer.*`.
- [ ] **Interface prefix** — Always `I` (e.g., `IItemMovementService`, not `ItemMovementServiceInterface`).
- [ ] **Suffix conventions**:
  | Type | Suffix | Example |
  |---|---|---|
  | Event record | `Event` | `PlayerWalkedEvent` |
  | Event handler | `EventHandler` | `PlayerWalkedEventHandler` |
  | Service | `Service` | `DecayService` |
  | Factory | `Factory` | `MonsterFactory` |
  | Data store | `Store` | `ItemTypeStore` |
  | Loader | `Loader` | `ItemTypeLoader` |
  | Routine | `Routine` | `GameCreatureRoutine` |
- [ ] **File-scoped namespaces** — Use `namespace NeoServer.X.Y;` not block-scoped `namespace ... { }`.
- [ ] **Records for DTOs** — Events and data-transfer types use `record`, not `class`.
- [ ] **Composition over inheritance** — Entity behavior is composed via injected services, not base class hierarchy.
- [ ] **No magic strings/numbers** — Configuration values come from `appsettings.json` via the options pattern.

### 4. Testing

- [ ] **Tests added or updated** — Does the PR include tests? New features and bug fixes must be covered.
- [ ] **xUnit + FluentAssertions** — No NUnit, MSTest, or raw `Assert.Equal()`. Use `Should().BeX()`.
- [ ] **AAA structure** — Every `[Fact]` is Arrange-Act-Assert clearly separated by blank lines.
- [ ] **Behavioral naming** — Format: `{Actor}_does_{something}_when_{condition}` (e.g., `Player_cannot_push_null_creature`).
- [ ] **Test isolation** — No shared objects between tests. Each test creates its own instances.
- [ ] **Mocking policy** — Domain entities, services, and commands are real objects. Only mock `I*Repository` and infrastructure interfaces (`IConnection`, `IEventAggregator`).
- [ ] **Test project placement** — Does the test file live in the correct test project under `tests/`? The folder structure mirrors `src/`:
  - Domain → `tests/NeoServer.Domain.Tests`
  - Networking → `tests/NeoServer.Networking.Tests`
  - Server → `tests/NeoServer.Server.Tests`
  - etc.
- [ ] **Traits** — `[Trait("Category", "<HappyPath|Validation|EdgeCase|ErrorCondition|Integration|...>")]` present on each test.
- [ ] **Build + test pass** — Run `dotnet test tests/` before approving.

### 5. Security

- [ ] **RSA key** — New code must not hardcode or commit the RSA key. It stays in `data/key.pem` and is loaded via `Rsa.LoadPem()`.
- [ ] **Credentials** — Database passwords and secrets come from environment variables, not `appsettings.json` or code.
- [ ] **Input validation** — Network packets, login, chat, trade, and any user-facing input is validated before processing. Invalid input is rejected, not silently ignored.
- [ ] **Permissions** — Player actions respect group/permission checks (`groups.json`). Verify new commands check `Player.AccessLevel` or equivalent.

### 6. In-Memory Data Stores

- [ ] **Immutability** — Data stores (`ItemTypeStore`, `MonsterTypeStore`, etc.) are populated at startup by loaders and are read-only at runtime. No new code may mutate them after load.
- [ ] **Thread safety** — If a new store is added, verify it uses appropriate concurrency protection (or stays immutable after initialization).

### 7. Dispatcher / Scheduler / Persistence

- [ ] **Thread affinity** — Game actions go through `IDispatcher`. Time-based events through `IScheduler`. Database writes through `IPersistenceDispatcher`. No raw `Task.Run` or `new Thread` for game logic.
- [ ] **Blocking** — Avoid blocking the dispatcher thread with long operations.

### 8. Lua Scripting (if applicable)

- [ ] **Hot-reload** — Lua scripts in `data/scripts/` must be safe to reload at runtime. No hard state assumptions in script-side globals.
- [ ] **Security sandbox** — Lua bindings must not expose dangerous host operations (file I/O, process execution, raw SQL) to scripts.

### 9. Configurability

- [ ] **New settings** — If adding a configurable value, does it go into `appsettings.json` under the appropriate section (`server`, `game`, `client`, `database`)?
- [ ] **Database provider** — New data-access code must support all active providers (`INMEMORY`, `SQLITE`, `POSTGRESQL`) or document the limitation.

### 10. Commit Hygiene

- [ ] **Conventional commits** — PR commits use `feat:`, `fix:`, `refactor:`, `test:`, `chore:`, etc.
- [ ] **Single concern** — Each commit does one thing. No mixed refactor + feature in one commit.

---

## Common Review Comments

Use these as templates for `gh pr review <number> --request-changes --body "$(...)"` or `--comment --body "$(...)"`.

### Domain dependency leak

> **Architecture:** `NeoServer.Domain` must not depend on infrastructure projects. The reference to `NeoServer.Data` / `NeoServer.Networking` in [file] violates DDD layering. Move this logic to an application service or inject the dependency via an interface defined in the domain.

### Missing factory usage

> **Pattern:** Game entities should be created through their factory (`IItemFactory`, `ICreatureFactory`, etc.) rather than with `new`. Factories handle data store lookups and proper initialization. Please use `IItemFactory.CreateItem(typeId, location)` here.

### Wrong handler interface

> **Design:** Network handlers implement `INetworkingEventHandler<T>` (higher priority), application handlers implement `IApplicationEventHandler<T>`. This handler is in the networking layer but implements `IApplicationEventHandler` — use `INetworkingEventHandler` instead.

### Missing DI registration

> **DI:** The new service `[Name]` is not registered in any IoC module under `src/Standalone/IoC/Modules/`. Add it to the appropriate module (e.g., `ServiceInjection.cs` for game services).

### Test uses mocks for domain types

> **Testing policy:** Domain entities, services, and commands should be real objects in tests. Only mock `I*Repository` and infrastructure interfaces (`IConnection`, `IEventAggregator`). Replace the mock of `[DomainType]` with a real instance.

### No tests

> **Testing:** This change introduces new behavior but no test coverage. Please add tests — see `AGENTS.md` for conventions (AAA, behavioral naming, xUnit + FluentAssertions).

### Test isolation violation

> **Testing:** Tests must not share mutable objects. Each `[Fact]` should create its own instances. Shared state between tests leads to order-dependent failures.

### Missing Trait

> **Style:** Every `[Fact]` should include `[Trait("Category", "<value>")]`. Valid values: `HappyPath`, `Validation`, `EdgeCase`, `ErrorCondition`, `Integration`.

### Data store mutation after load

> **Architecture:** `[StoreName]` is populated at startup and treated as read-only at runtime. This code mutates store contents after initialization, which is unsafe. Load the data at startup or add a new store.

### Wrong thread for game action

> **Threading:** Game actions must be dispatched through `IDispatcher`. Direct `Task.Run` or background threads bypass the game loop and cause race conditions on game state.

---

## CLI Reference

### PR Review

```bash
# Checkout the PR locally
gh pr checkout <number>

# View changes
gh pr diff <number>
gh pr diff <number> --name-only

# Get structured data (files changed, status checks, etc.)
gh pr view <number> --json title,body,files,additions,deletions,reviews,statusCheckRollup

# See only files changed with line counts
gh pr view <number> --json files --jq '.files[] | {path, additions, deletions, status}'

# Submit review
gh pr review <number> --approve --body "LGTM. All checks pass."
gh pr review <number> --comment --body "$(cat review-notes.md)"
gh pr review <number> --request-changes --body "$(cat review-issues.md)"

# Check CI status
gh pr checks <number>
```

Pro tip: run `gh pr view <number> --json files --jq '.files[].path'` for a quick file list, then cross-reference with the checklist.

### Staging Review

```bash
# What's changed
git status
git diff --name-status HEAD     # all changed files

# Full diff
git diff --staged               # only staged (about to commit)
git diff                         # only unstaged
git diff HEAD                   # all changes since last commit

# New untracked files
git ls-files --others --exclude-standard

# Per-file review
git diff HEAD -- src/NeoServer.Domain/SomeFile.cs

# Reset if something is wrong
git restore --staged <file>     # unstage
git checkout -- <file>          # discard unstaged changes
```

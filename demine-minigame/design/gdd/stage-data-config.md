# Stage Data Config

> **Status**: Approved
> **Author**: User + Claude Code agents
> **Last Updated**: 2026-03-27
> **Implements Pillar**: Data-Driven Authoring

## Overview

Stage Data Config is the data schema layer for the Demine Field event. It defines all static, authoring-time data required to instantiate an event: the event envelope (id, title, timer settings), the ordered list of stage definitions (board dimensions, mine positions, move budget, reward payload, unlock condition), and the tool catalog (tool id, cost, effect type). No gameplay logic lives here — this system is read-only at runtime. Every other system that needs configuration reads it; nothing writes to it after load. A new event configuration can be authored entirely in data without any code changes.

## Player Fantasy

The player never sees Stage Data Config — but they feel it. Every time a new event drops with a fresh board layout, a different theme, or a harder stage, that's this system working. The fantasy it enables is: *"This event feels designed for me — each stage is a distinct, hand-crafted challenge."* Stage Data Config is what makes the game feel like content, not a procedural toy. It also ensures that a designer can ship a new event variant — harder boards, different rewards, seasonal theme — without waiting on an engineer.

## Detailed Design

### Core Rules

1. Stage Data Config is a **read-only data container** loaded once at event entry. No runtime system may modify it.
2. The config is structured as three nested objects: **EventConfig**, **StageConfig[]**, and **ToolConfig[]**.
3. Each `StageConfig` has a `placementMode` field that determines whether mines are fixed or randomized:
   - `fixed` — an explicit `minePositions` array is required; the same positions are used every attempt
   - `randomized` — no position array; runtime Mine Placement System generates positions using `mineCount` and `randomSeed` (optional fixed seed for reproducibility)
4. All gameplay-sensitive values (board size, move budget, mine count, reward) are in the config. No default values are hardcoded in game logic — missing required fields are a config authoring error.
5. Event cycles are independent configs. A new event run loads a new config asset; the system does not maintain history of prior configs.

### States and Transitions

Stage Data Config is stateless at runtime — it is pure data. The only lifecycle is:

| State | Description |
|-------|-------------|
| **Unloaded** | Config asset not yet requested |
| **Loading** | Async load in progress (Addressables) |
| **Loaded** | Config is in memory and valid |
| **Invalid** | Config failed validation; event cannot start |

Transitions: Unloaded → Loading (on event entry) → Loaded (on success) or Invalid (on validation failure). No transitions after Loaded.

### Interactions with Other Systems

| System | Data Flowing Out | Interface Owner |
|--------|-----------------|-----------------|
| Board State System | `StageConfig.rowCount`, `columnCount`, `placementMode`, `minePositions` / `mineCount` | Board State System reads on stage init |
| Mine Placement System | `StageConfig.placementMode`, `minePositions`, `mineCount`, `randomSeed` | Mine Placement System reads on board setup |
| Event State Manager | `EventConfig.eventId`, `eventTitle`, `eventDurationSeconds`, `stages.length` (stage count derived) | Event State Manager reads on event entry |
| Stage Progression System | `StageConfig.stageId`, `unlockCondition`, stage ordering | Stage Progression System reads on stage transition |
| Reward System | `StageConfig.rewardType`, `rewardId`, `rewardAmount` | Reward System reads on stage completion |
| Tool System | `ToolConfig[].toolId`, `cost`, `effectType` | Tool System reads on event entry |

## Formulas

Stage Data Config contains no runtime calculations. Its role is to define the data contract that feeds formulas in downstream systems. The complete field schema is the specification:

### EventConfig

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `eventId` | string | Yes | Unique identifier for this event run |
| `eventTitle` | string | Yes | Display name shown in the event header |
| `eventDurationSeconds` | int | Yes | Total time window for the event in seconds |
| `stages` | StageConfig[] | Yes | Ordered list of stages; index 0 = Stage 1 |
| `tools` | ToolConfig[] | No | Available tools; empty array = no tools offered |
| `backgroundTheme` | string | No | Default theme token applied to all stages unless overridden |

### StageConfig

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `stageId` | string | Yes | Unique identifier within this event |
| `displayName` | string | Yes | Label shown in the Stage List Panel |
| `rowCount` | int | Yes | Board row count; min 2, max 10 |
| `columnCount` | int | Yes | Board column count; min 2, max 10 |
| `placementMode` | enum | Yes | `fixed` or `randomized` |
| `minePositions` | int[][] | If fixed | Array of [row, col] pairs; required when placementMode = fixed |
| `mineCount` | int | If randomized | Total mines to place; required when placementMode = randomized |
| `randomSeed` | int | No | Optional fixed seed for randomized placement; omit for true random |
| `requiredMineCount` | int | Yes | Mines the player must find to win; ≤ total mine count |
| `moveBudget` | int | Yes | Starting move count; min 1 |
| `rewardType` | string | Yes | Reward category token (e.g. "resource", "token") |
| `rewardId` | string | Yes | Specific reward item identifier |
| `rewardAmount` | int | Yes | Quantity granted on stage completion; min 1 |
| `backgroundTheme` | string | No | Per-stage theme override; falls back to EventConfig theme |
| `unlockCondition` | string | Yes | `none` (always unlocked) or `stage:{stageId}` (unlocks after that stage completes) |

### ToolConfig

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `toolId` | string | Yes | Unique identifier matching Tool System registry |
| `displayName` | string | Yes | Label shown on the Tool Bar button |
| `cost` | int | Yes | Currency cost to purchase; min 1 |
| `effectType` | enum | Yes | `area_reveal` or `guaranteed_mine` |

## Edge Cases

1. **`placementMode = fixed` but `minePositions` is empty or missing** — Config validation must reject this at load time. Event cannot start. Surface an authoring error log.
2. **`placementMode = randomized` but `mineCount` is missing** — Config validation rejects; event cannot start.
3. **`requiredMineCount > total mine count`** — Invalid; a stage where the win condition can never be reached must be rejected at load time. For `fixed` mode: `requiredMineCount ≤ minePositions.length`. For `randomized` mode: `requiredMineCount ≤ mineCount`.
4. **`minePositions` contains out-of-bounds coordinates** — A position where `row ≥ rowCount` or `col ≥ columnCount` is invalid. Reject at load.
5. **`minePositions` contains duplicate coordinates** — Two mines cannot occupy the same tile. Reject at load; silent deduplication could change intended difficulty.
6. **`moveBudget` is zero or negative** — Rejected at load. A stage with zero moves is unplayable.
7. **Stage with `unlockCondition = stage:{stageId}` where the referenced `stageId` does not exist in the event** — Reject at load; a broken unlock chain leaves later stages permanently locked.
8. **`tools` array references a `toolId` not registered in the Tool System** — Log a warning at load; do not crash. The tool is silently excluded from the event. (Defensive: Tool System may not have loaded yet.)
9. **`rowCount` or `columnCount` exceeds max (10)** — Reject at load. Oversized boards exceed UI budget and have not been tested.
10. **Event config asset not found** — Addressable load fails; show an error screen and exit to the host game. Never enter an event with no config.

## Dependencies

**Upstream (what Stage Data Config depends on):**
— None. Stage Data Config is the foundation layer. It has no runtime dependencies on other game systems.

**Downstream (what depends on Stage Data Config):**

| System | Dependency Type | Data Required |
|--------|----------------|---------------|
| Board State System | Hard | `StageConfig.rowCount`, `columnCount`, `placementMode`, `minePositions` / `mineCount`, `randomSeed` |
| Mine Placement System | Hard | `StageConfig.placementMode`, `minePositions`, `mineCount`, `randomSeed` |
| Event State Manager | Hard | `EventConfig.eventId`, `eventTitle`, `eventDurationSeconds`, `stages.length` (stage count derived) |
| Stage Progression System | Hard | `StageConfig.stageId`, `unlockCondition`, stage ordering (array index) |
| Reward System | Hard | `StageConfig.rewardType`, `rewardId`, `rewardAmount` |
| Tool System | Soft | `ToolConfig[].toolId`, `cost`, `effectType` — event works without tools if array is empty |

*Hard dependency = system cannot function without this data. Soft = system degrades gracefully if data is absent.*

**External dependency:**
Config assets are loaded via **Unity Addressables**. Stage Data Config assumes the Addressables system is initialized before event entry. If Addressables is not ready, the config cannot load (see Edge Case #10).

## Tuning Knobs

All Stage Data Config fields are technically tunable by data authoring. The following are the knobs designers will touch most often, with safe ranges and notes on what breaks at extremes:

| Knob | Location | Safe Range | Too Low | Too High |
|------|----------|-----------|---------|----------|
| `rowCount` × `columnCount` (board area) | StageConfig | 6–40 tiles total | Board feels trivial; mine density too obvious | UI tiles become unreadably small on mobile; performance risk |
| `requiredMineCount` | StageConfig | 20–60% of total tiles | Stage too easy; player finds mines immediately | Stage feels unfair; mines fill most of the board |
| `moveBudget` | StageConfig | ≥ `requiredMineCount` + 3 | Below this, stage is near-impossible on first attempt | Very large budgets remove tension |
| `mineCount` (randomized mode) | StageConfig | ≤ (rowCount × columnCount) − 1 | No lower bound concern | Must leave at least one non-mine tile; board cannot be entirely mines |
| `cost` (tool) | ToolConfig | 50–500 currency units | Tools become free and trivially used | Tools become never-purchased; invisible feature |
| `eventDurationSeconds` | EventConfig | 3,600–604,800 (1 hour to 7 days) | Event expires before most players see it | Event runs so long it loses urgency |

**Interactions between knobs:**
- `moveBudget` and `requiredMineCount` are coupled — reducing `moveBudget` while increasing `requiredMineCount` rapidly approaches impossible. Test both together.
- `mineCount` and `requiredMineCount` in randomized mode: if they are equal, the player must find every mine. If `requiredMineCount` is lower, the player can succeed before all mines are revealed (easier, but less conclusive).

## Visual/Audio Requirements

[To be designed]

## UI Requirements

[To be designed]

## Acceptance Criteria

A QA tester can verify Stage Data Config is working correctly by checking all of the following:

**Schema Loading**
- [ ] Given a valid event config asset, the system loads without errors and all three sub-configs (EventConfig, StageConfig[], ToolConfig[]) are accessible in memory
- [ ] Given an Addressable key for a non-existent asset, the system surfaces an error and exits to the host game without crashing

**Validation — Rejection cases (each must log an error and block event entry)**
- [ ] Config with `placementMode = fixed` and no `minePositions` is rejected
- [ ] Config with `placementMode = randomized` and no `mineCount` is rejected
- [ ] Config with `requiredMineCount > minePositions.length` (fixed) is rejected
- [ ] Config with `requiredMineCount > mineCount` (randomized) is rejected
- [ ] Config with a `minePosition` coordinate outside board bounds is rejected
- [ ] Config with duplicate coordinates in `minePositions` is rejected
- [ ] Config with `moveBudget ≤ 0` is rejected
- [ ] Config with an `unlockCondition` referencing a non-existent `stageId` is rejected
- [ ] Config with `rowCount` or `columnCount` > 10 is rejected

**Validation — Warning cases (must log warning but allow event entry)**
- [ ] Config with a `toolId` not registered in the Tool System logs a warning and the tool is excluded; the event still starts

**Data Integrity**
- [ ] All three stages from the reference content (3×4/3 mines, 4×5/5 mines, 4×7/7 mines) load correctly with their fields intact
- [ ] A config with `tools` as an empty array loads without error; the event starts with no tools offered
- [ ] A config using `placementMode = randomized` with no `randomSeed` produces different board layouts on separate loads
- [ ] A config using `placementMode = randomized` with a fixed `randomSeed` produces the same board layout on every load

**Data-Driven Authoring**
- [ ] A new stage can be added to the config data file and appear in the game without any code changes
- [ ] Stage reward values can be changed in data and reflect correctly in the Reward System at runtime

## Open Questions

[To be designed]

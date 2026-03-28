# Systems Index: Sapper — Demine Field

> **Status**: Approved
> **Created**: 2026-03-27
> **Last Updated**: 2026-03-27 (Stage Data Config approved)
> **Source Concept**: docs/sapper-demine-field-product-document.md

---

## Overview

Sapper: Demine Field is a mobile event mini game built as a lightweight, session-friendly
side activity inside a larger game economy. The mechanical scope centers on a hidden-tile
board with randomized mine placement, a move budget, sequential stage progression, and
optional economy tools. Systems are grouped into five layers: a data foundation, a game
state core, feature-layer gameplay and economy systems, a presentation layer of UI and
persistence, and a polish layer of onboarding and analytics. All gameplay values are
data-driven — no code changes should be required to author new event configurations.

---

## Systems Enumeration

| # | System Name | Category | Priority | Status | Design Doc | Depends On |
|---|-------------|----------|----------|--------|------------|------------|
| 1 | Stage Data Config | Core | MVP | Approved | [stage-data-config.md](stage-data-config.md) | — |
| 2 | Board State System | Core | MVP | Not Started | — | Stage Data Config |
| 3 | Mine Placement System | Core | MVP | Not Started | — | Board State System |
| 4 | Event State Manager | Core | MVP | Not Started | — | Stage Data Config |
| 5 | Tile Interaction System | Gameplay | MVP | Not Started | — | Board State System, Mine Placement System |
| 6 | Stage Progression System | Gameplay | MVP | Not Started | — | Event State Manager, Board State System |
| 7 | Board View | UI | MVP | Not Started | — | Board State System, Tile Interaction System |
| 8 | Game HUD | UI | MVP | Not Started | — | Board State System |
| 9 | Event Screen Layout | UI | MVP | Not Started | — | Board View, Game HUD, Stage List Panel, Tool Bar |
| 10 | Currency Interface | Economy | Vertical Slice | Not Started | — | — |
| 11 | Tool System | Gameplay | Vertical Slice | Not Started | — | Board State System, Tile Interaction System, Currency Interface |
| 12 | Reward System | Economy | Vertical Slice | Not Started | — | Stage Progression System, Stage Data Config |
| 13 | Failure & Retry System | Gameplay | Vertical Slice | Not Started | — | Board State System, Stage Progression System |
| 14 | Stage List Panel | UI | Vertical Slice | Not Started | — | Stage Progression System, Event State Manager |
| 15 | Tool Bar | UI | Vertical Slice | Not Started | — | Tool System |
| 16 | Reward Modal | UI | Vertical Slice | Not Started | — | Reward System |
| 17 | Failure Modal | UI | Vertical Slice | Not Started | — | Failure & Retry System |
| 18 | Event Persistence | Persistence | Alpha | Not Started | — | Event State Manager, Board State System, Stage Progression System |
| 19 | Event Completion Modal | UI | Alpha | Not Started | — | Stage Progression System |
| 20 | Tutorial Overlay | Meta | Full Vision | Not Started | — | Event Screen Layout |
| 21 | Analytics | Meta | Full Vision | Not Started | — | Tile Interaction System, Tool System, Stage Progression System, Failure & Retry System, Reward System |

---

## Categories

| Category | Description | Systems |
|----------|-------------|---------|
| **Core** | Foundation systems everything depends on | Stage Data Config, Board State System, Mine Placement System, Event State Manager |
| **Gameplay** | Systems that make the game work | Tile Interaction System, Stage Progression System, Tool System, Failure & Retry System |
| **Economy** | Resource and reward flow | Currency Interface, Reward System |
| **Persistence** | Save state and session continuity | Event Persistence |
| **UI** | All player-facing displays | Board View, Game HUD, Event Screen Layout, Stage List Panel, Tool Bar, Reward Modal, Failure Modal, Event Completion Modal |
| **Meta** | Systems outside the core loop | Tutorial Overlay, Analytics |

---

## Priority Tiers

| Tier | Definition | Systems |
|------|------------|---------|
| **MVP** | Core loop functional — tap tiles, find mines, count moves, win a stage | 1–9 (9 systems) |
| **Vertical Slice** | Complete playable event — tools, rewards, failure, all UI panels | 10–17 (8 systems) |
| **Alpha** | Feature complete — persistence, event completion state | 18–19 (2 systems) |
| **Full Vision** | Polished and shippable — onboarding, instrumentation | 20–21 (2 systems) |

---

## Dependency Map

### Foundation Layer (no dependencies)

1. **Stage Data Config** — pure data schema; all other systems read from it
2. **Currency Interface** — bridge to host game currency; no internal dependencies

### Core Layer (depends on Foundation)

3. **Event State Manager** — depends on: Stage Data Config
4. **Board State System** — depends on: Stage Data Config
5. **Mine Placement System** — depends on: Board State System (needs board dimensions)

### Feature Layer (depends on Core)

6. **Tile Interaction System** — depends on: Board State System, Mine Placement System
7. **Stage Progression System** — depends on: Event State Manager, Board State System
8. **Tool System** — depends on: Board State System, Tile Interaction System, Currency Interface
9. **Failure & Retry System** — depends on: Board State System, Stage Progression System
10. **Reward System** — depends on: Stage Progression System, Stage Data Config

### Presentation Layer (depends on Features)

11. **Event Persistence** — depends on: Event State Manager, Board State System, Stage Progression System
12. **Board View** — depends on: Board State System, Tile Interaction System
13. **Game HUD** — depends on: Board State System
14. **Stage List Panel** — depends on: Stage Progression System, Event State Manager
15. **Tool Bar** — depends on: Tool System
16. **Reward Modal** — depends on: Reward System
17. **Failure Modal** — depends on: Failure & Retry System
18. **Event Completion Modal** — depends on: Stage Progression System
19. **Event Screen Layout** — depends on: Board View, Game HUD, Stage List Panel, Tool Bar

### Polish Layer

20. **Tutorial Overlay** — depends on: Event Screen Layout
21. **Analytics** — depends on: Tile Interaction System, Tool System, Stage Progression System, Failure & Retry System, Reward System

---

## Circular Dependencies

None found.

---

## High-Risk Systems

| System | Risk Type | Risk Description | Mitigation |
|--------|-----------|-----------------|------------|
| Mine Placement System | Technical | Randomized placement must guarantee solvability within move budget; first-click safe guarantee has edge cases on small boards | Prototype the generator before Stage Data Config is finalized |
| Tool System | Design | Explosive area size and Detector guarantee could be OP or useless depending on board density | Tune against actual stage 1–3 layouts during Vertical Slice |
| Event Persistence | Technical | Mid-session board state restoration is the most complex persistence pattern; tile-by-tile reveal state must survive app restart | Design the full save schema before implementation begins |
| Currency Interface | Technical | Depends on host game architecture not controlled by this feature; may require adapter pattern | Define interface contract as first design step |

---

## Recommended Design Order

| Order | System | Priority | Layer | Est. Effort |
|-------|--------|----------|-------|-------------|
| 1 | Stage Data Config | MVP | Foundation | S |
| 2 | Board State System | MVP | Core | M |
| 3 | Mine Placement System | MVP | Core | S |
| 4 | Event State Manager | MVP | Core | S |
| 5 | Tile Interaction System | MVP | Feature | M |
| 6 | Stage Progression System | MVP | Feature | S |
| 7 | Board View | MVP | Presentation | M |
| 8 | Game HUD | MVP | Presentation | S |
| 9 | Event Screen Layout | MVP | Presentation | S |
| 10 | Currency Interface | Vertical Slice | Foundation | S |
| 11 | Tool System | Vertical Slice | Feature | M |
| 12 | Reward System | Vertical Slice | Feature | S |
| 13 | Failure & Retry System | Vertical Slice | Feature | S |
| 14 | Stage List Panel | Vertical Slice | Presentation | S |
| 15 | Tool Bar | Vertical Slice | Presentation | S |
| 16 | Reward Modal | Vertical Slice | Presentation | S |
| 17 | Failure Modal | Vertical Slice | Presentation | S |
| 18 | Event Persistence | Alpha | Presentation | L |
| 19 | Event Completion Modal | Alpha | Presentation | S |
| 20 | Tutorial Overlay | Full Vision | Polish | S |
| 21 | Analytics | Full Vision | Polish | M |

*Effort: S = 1 session, M = 2–3 sessions, L = 4+ sessions*

---

## Progress Tracker

| Metric | Count |
|--------|-------|
| Total systems identified | 21 |
| Design docs started | 1 |
| Design docs reviewed | 1 |
| Design docs approved | 1 |
| MVP systems designed | 1 / 9 |
| Vertical Slice systems designed | 0 / 8 |

---

## Next Steps

- [ ] Design MVP-tier systems first — start with `/design-system stage-data-config`
- [ ] Run `/design-review` on each completed GDD
- [ ] Run `/gate-check pre-production` when all MVP systems are designed
- [ ] Prototype Mine Placement System early — highest technical risk
- [ ] Plan first implementation sprint with `/sprint-plan new`

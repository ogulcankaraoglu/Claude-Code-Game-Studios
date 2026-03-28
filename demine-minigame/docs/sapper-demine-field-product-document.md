# Sapper - Demine Field Product Document

**Document Type:** Product Requirements Document  
**Audience:** Game developer, UI developer, technical designer, producer  
**Status:** Draft for implementation  
**Based On:** Reverse analysis of reference screenshots

---

## 1. Product Summary

`Demine Field` is a limited-time `Sapper` event mini game in which the player uncovers hidden tiles to find all mines in a stage before running out of moves. The feature is built as a lightweight, session-friendly side activity inside a larger game economy. It combines simple tap interaction, sequential stage progression, reward payout, and optional purchasable tools.

The goal of this feature is not to create a full Minesweeper clone. The intended experience is faster, more readable, more event-driven, and easier to monetize or tune through moves, stage layouts, and tools.

---

## 2. Problem Statement

The game needs a compact event feature that:

- Gives players a short-term objective during a live event window
- Creates a satisfying reward loop with minimal onboarding
- Uses simple UI interaction suitable for mobile
- Supports economy sinks through consumable tools
- Can be configured as content through data instead of custom code per stage

`Demine Field` solves this by offering a hidden-tile search mode with clear goals, predictable stage progression, and strong completion feedback.

---

## 3. Product Goals

- Deliver a mini game that can be understood in under 30 seconds
- Make each stage completable in a short play session
- Support a 3-stage progression chain with escalating board size
- Provide clear reward motivation before each stage starts
- Create room for monetization or soft-economy spending through tools
- Make the feature straightforward to author and rebalance with data

## 3.1 Non-Goals

- Not a full logic-puzzle Minesweeper implementation
- Not an open-ended sandbox system
- Not a PvP or leaderboard feature
- Not a feature requiring long-form tutorialization

---

## 4. Target Player Experience

The player should feel:

- Curious about where mines are hidden
- Comfortable making quick choices without reading rules text
- Rewarded often through progress bars, stage completion, and visible loot
- Motivated to finish the whole event chain
- Willing to use tools when a stage becomes tight on moves

The session should feel:

- Short
- Tactile
- Clear
- Reward-forward

---

## 5. Core Feature Definition

The player is presented with:

- A stage-based hidden tile board
- A required number of mines to discover
- A remaining move counter
- A stage reward preview
- Optional purchasable tools
- A vertical progression panel for all stages in the event

The player taps tiles to reveal outcomes. Each tap consumes one move. A stage ends in success when all required mines are found. The broader event ends when all available stages are completed.

---

## 6. Primary User Story

As a player, I want to enter a limited-time Sapper event, quickly understand what I need to do, uncover hidden mines across a few stages, and earn rewards for finishing them.

## 6.1 Supporting User Stories

- As a player, I want to see how many mines remain in the current stage.
- As a player, I want to know how many moves I have left.
- As a player, I want wrong picks to be clearly visible.
- As a player, I want to preview what reward I will earn before I complete a stage.
- As a player, I want later stages to unlock in sequence.
- As a player, I want tools to help me if I get stuck or want to optimize progress.
- As a player, I want a strong completion moment when a stage or event is finished.

---

## 7. Gameplay Rules

## 7.1 Stage Objective

Each stage specifies:

- Board dimensions
- Hidden mine positions
- Number of mines required to complete the stage
- Move budget
- Reward payload

The player completes the stage by finding all required mines before moves reach zero.

## 7.2 Tile Resolution

When the player taps a hidden tile, the tile resolves into one of the following:

- **Mine found**
  - Tile reveals a mine icon
  - Tile receives a positive state marker
  - Progress increases by `+1`

- **Miss**
  - Tile reveals a failed state, shown as a red `X`
  - No progress is awarded

- **Cleared empty tile**
  - Tile becomes opened but contains no mine
  - No progress is awarded

Every valid reveal consumes exactly one move unless modified by a tool effect.

## 7.3 Success Condition

The stage succeeds immediately when found mine count equals required mine count.

On success:

- Board interaction is disabled
- Reward modal is shown
- Stage is marked completed
- Next stage is unlocked if one exists

## 7.4 Failure Condition

If remaining moves reach `0` before required mines are found, the stage fails.

Failure behavior is not visible in the source screenshots, so implementation should define:

- Retry behavior
- Reset behavior
- Whether tools can still be used at zero moves
- Whether the player can spend currency to continue

Recommended default:

- Show failure modal
- Allow stage retry
- Reset board to stage start state

---

## 8. Stage Content Specification

Based on the reference screenshots, the initial content set should include three stages.

| Stage | Grid Size | Required Mines | Reward |
|---|---:|---:|---|
| 1 | 3x4 | 3 | Resource x50 |
| 2 | 4x5 | 5 | Purple card/token x2 |
| 3 | 4x7 | 7 | Insignia/token x3 |

## 8.1 Difficulty Intent

- **Stage 1:** onboarding stage, forgiving, teaches board language
- **Stage 2:** mid-tier challenge, introduces more uncertainty
- **Stage 3:** highest complexity, larger board, stronger reward payoff

## 8.2 Stage Data Requirements

Each stage needs:

- `stageId`
- `displayName`
- `rowCount`
- `columnCount`
- `minePositions`
- `requiredMineCount`
- `moveBudget`
- `rewardType`
- `rewardId`
- `rewardAmount`
- `backgroundTheme`
- `unlockCondition`

---

## 9. Economy and Tools

Two paid tools are visible in the references.

| Tool | Observed Cost | Product Intent |
|---|---:|---|
| Explosive tool | 150 | Speed up or widen reveal outcome |
| Detector tool | 100 | Provide safer or more targeted mine discovery |

Because the screenshots do not confirm exact behavior, the implementation should choose effects that are:

- Easy to understand
- High-value in low-move situations
- Easy to communicate visually
- Safe to tune independently

## 9.1 Recommended Tool Behaviors

- **Explosive tool**
  - Player selects a target tile
  - Reveals a small area around it
  - If mines are in the area, they are discovered

- **Detector tool**
  - Reveals one guaranteed mine tile
  - Or highlights a correct tile before the next move

## 9.2 Economy Constraints

- Tool costs should be configurable by data
- Tool usage should check player currency before activation
- Purchase and use flows should be one interaction if possible
- Tool effects must not break stage completion logic

---

## 10. Event Structure

This feature sits inside a time-limited event shell.

The event shell must support:

- Event title
- Remaining event time
- Entry and exit navigation
- Stage progression list
- Locked and completed stage states
- Event end / reset messaging

## 10.1 Event End State

When all stages are completed:

- Show a completion overlay
- Confirm that all rewards have been earned
- Inform the player when the next event or cycle starts

Recommended implementation:

- Store event completion separately from stage completion
- Support reset by timer or content refresh

---

## 11. UX Requirements

## 11.1 Main Screen Layout

The main event screen should contain:

- Top bar
  - Back button
  - Event name
  - Help/info button
  - Countdown timer
  - Currency display
  - Home button if needed by app shell

- Main board area
  - Stage title
  - Move counter
  - Interactive grid

- Right-side or secondary panel
  - Stage list
  - Reward preview
  - Current progress
  - Locked/completed states

- Bottom utility area
  - Tool buttons
  - Tool prices

## 11.2 Visual Feedback Requirements

- Selected tile should have a temporary active highlight
- Found mine should be immediately distinguishable from wrong pick
- Wrong pick should remain visible
- Reward modal should feel celebratory
- Current stage should be more prominent than locked future stages

## 11.3 Readability Requirements

- Coordinates on tiles should remain readable or be removable on small screens
- Move counter must be visible at all times during board play
- Progress text must be readable without relying only on iconography

---

## 12. Functional Requirements

- System can load event config and current progression state
- System can render stage boards of varying dimensions
- System can track hidden mine positions
- System can process tile tap input
- System can resolve tile outcome deterministically
- System can decrement moves
- System can increment found mine progress
- System can disable already resolved tiles
- System can unlock next stage after success
- System can show stage reward modal
- System can show event completion modal
- System can purchase and apply tools
- System can persist event and stage progress
- System can restore session state after app restart

---

## 13. Technical Requirements

## 13.1 Suggested System Breakdown

- `SapperEventController`
  - owns event state, stage progression, timer integration

- `DemineStageController`
  - owns current board state, move count, stage completion/failure rules

- `DemineBoardView`
  - renders grid and tile visuals

- `DemineTileView`
  - handles tile state presentation and click feedback

- `SapperToolController`
  - validates currency and applies tool effects

- `SapperRewardFlow`
  - shows reward popup and grants payout

- `SapperEventData`
  - stage definitions, reward setup, tool costs, themes

## 13.2 Data-Driven Authoring Requirements

The feature should be data-authored where possible:

- board dimensions
- mine positions
- move budgets
- reward definitions
- tool costs
- stage ordering
- background themes
- event timer settings

This avoids needing code changes for every new event configuration.

## 13.3 Persistence Requirements

Persist at minimum:

- current event id
- unlocked stages
- completed stages
- current stage progress
- board revealed state
- moves remaining
- rewards claimed
- event completion state

---

## 14. Analytics Recommendations

Track:

- event entry count
- stage start count
- stage completion count
- stage failure count
- average moves remaining on success
- tool purchase count by tool type
- tool usage count by tool type
- abandonment by stage
- time to complete full event

These metrics will help tune move budgets, reward value, and tool usefulness.

---

## 15. Edge Cases

- Player taps an already revealed tile
- Player tries to use a tool without enough currency
- Player closes app during reward modal
- Event expires while player is inside the screen
- Player completes a stage, but reward grant fails
- Player enters after all stages are complete
- Stage data and required mine count do not match
- Tool effect reveals the final mine and stage completes mid-animation

---

## 16. Product Decisions — RESOLVED

All open decisions closed 2026-03-28 via director vote (Game Designer, UX Designer, Economy Designer).

| # | Decision | Ruling | Rationale |
|---|---|---|---|
| 1 | Mine positions | **Randomized per attempt** with first-click safety guarantee | Preserves tool value on retry; prevents memorization exploit; extends content lifespan of 3 stages |
| 2 | Failed stages retry | **Free retry, no cost** | Keeps players in the loop; free retry directs spending to tools, not retry access |
| 3 | Buy extra moves | **No** | Competes with tools; near-miss monetization is an ethical dark pattern; tools already cover the "stuck" use case |
| 4 | Reward claim | **Manual tap to claim** | Preserves the completion moment; prevents double-grant edge cases on app restart |
| 5 | Tool consume model | **Instant-use on purchase** | Eliminates inventory hoarding; closes the purchase-to-effect loop immediately; aligns with "one interaction" goal from §9.2 |
| 6 | Tutorial on first entry | **Yes — contextual overlay, first entry only, skippable** | Required for tool education; players who don't understand the Detector won't spend 100 currency on it |

### Implications for Implementation

- **Mine Placement System**: must implement randomized placement with first-click safety (no mine on or adjacent to first tapped tile)
- **Failure & Retry System**: retry is always free; board resets with new randomized layout
- **Tool System**: purchase = immediate activation; no inventory state required
- **Reward System**: reward modal requires explicit player tap to grant and dismiss
- **Tutorial Overlay**: single-pass contextual callouts on real board UI; flag stored in player prefs, never shown again after first dismissal

---

## 17. Acceptance Criteria

The feature is ready for implementation handoff when:

- A developer can build the full feature from this document without needing to infer the basic loop
- Stage content can be created from data
- Success and failure conditions are explicit
- Required UI areas and states are defined
- Reward and progression behavior is specified
- Tool behavior has a default implementation path
- Remaining open decisions are clearly listed

The feature is ready for QA when:

- All three stages are playable in sequence
- Rewards grant correctly
- Tool usage is functional
- Progress persists correctly
- Event completion and reset states behave correctly

---

## 18. Recommended Next Documents

To move from product definition to production, the following follow-up docs are recommended:

- Gameplay rules spec
- Unity technical design
- UI layout/wireframe spec
- Data schema definition
- QA test cases

---

## 19. Implementation Summary

Build `Demine Field` as a small, stage-based event mini game with:

- sequential unlocks
- hidden mine boards
- move-limited interaction
- stage rewards
- optional paid tools
- event timer framing

Keep the implementation data-driven, readable, and lightweight. Prioritize clear feedback and short session length over deep puzzle complexity.

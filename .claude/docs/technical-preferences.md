# Technical Preferences

<!-- Populated by /setup-engine. Updated as the user makes decisions throughout development. -->
<!-- All agents reference this file for project-specific standards and conventions. -->

## Engine & Language

- **Engine**: Unity 2022.3.62f2 (LTS)
- **Language**: C#
- **Rendering**: Universal Render Pipeline (URP)
- **Physics**: PhysX (Unity Physics)

## Naming Conventions

- **Classes**: PascalCase (e.g., `PlayerController`)
- **Public fields/properties**: PascalCase (e.g., `MoveSpeed`)
- **Private fields**: _camelCase (e.g., `_moveSpeed`)
- **Methods**: PascalCase (e.g., `TakeDamage()`)
- **Files**: PascalCase matching class (e.g., `PlayerController.cs`)
- **Scenes/Prefabs**: PascalCase matching root object (e.g., `MainMenu.unity`, `Player.prefab`)
- **Constants**: UPPER_SNAKE_CASE (e.g., `MAX_HEALTH`)

## Performance Budgets

- **Target Framerate**: [TO BE CONFIGURED — typical mobile: 60fps]
- **Frame Budget**: [TO BE CONFIGURED — typical: 16.6ms]
- **Draw Calls**: [TO BE CONFIGURED — typical mobile: <100]
- **Memory Ceiling**: [TO BE CONFIGURED — typical mobile: <512MB]

## Testing

- **Framework**: Unity Test Framework (NUnit)
- **Minimum Coverage**: [TO BE CONFIGURED]
- **Required Tests**: Balance formulas, gameplay systems, networking (if applicable)

## Forbidden Patterns

<!-- Add patterns that should never appear in this project's codebase -->
- [None configured yet — add as architectural decisions are made]

## Allowed Libraries / Addons

<!-- Add approved third-party dependencies here -->
- [None configured yet — add as dependencies are approved]

## Architecture Decisions Log

<!-- Quick reference linking to full ADRs in docs/architecture/ -->
- [No ADRs yet — use /architecture-decision to create one]

# Unity Engine — Version Reference

| Field | Value |
|-------|-------|
| **Engine Version** | Unity 2022.3.62f2 (LTS) |
| **Project Pinned** | 2026-03-27 |
| **LLM Knowledge Cutoff** | May 2025 |
| **Risk Level** | LOW — version is within LLM training data |

## Note

Unity 2022.3 LTS is well within the LLM's training data. Full reference docs
are not required. Agents can rely on their built-in knowledge for this version.

If agents suggest incorrect APIs, run `/setup-engine refresh` to populate
detailed reference docs at any time.

## Version Context

Unity 2022.3 LTS is the last LTS before the Unity 6 series (formerly 2023 Tech
Stream). Key characteristics relevant to this project:

- **UI**: UGUI (Canvas) is standard; UI Toolkit is available but still maturing
- **Input**: New Input System package recommended over legacy Input Manager
- **Rendering**: URP is production-ready and recommended for mobile
- **Physics**: PhysX 4.x via Unity Physics
- **C#**: C# 9 support
- **Addressables**: 1.x, stable for asset management

## Stale Reference Docs Warning

The following files in this directory were authored for Unity 6.3 LTS and are
NOT valid for Unity 2022.3.62f2. Do not use them as references:

- `breaking-changes.md`
- `deprecated-apis.md`
- `current-best-practices.md`
- `modules/` (all files)
- `plugins/` (all files)
- `PLUGINS.md`

These files can be safely deleted or archived.

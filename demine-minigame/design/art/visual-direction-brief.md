# Visual Direction Brief: Sapper — Demine Field

**Authored by:** Art Director
**Date:** 2026-03-28
**Status:** Approved — ready for implementation

---

## 1. Theme and Tone

**Stylized Tactical — "Cold War Field Operations"**

Tense, probabilistic, strategic — controlled urgency, not horror. Sits at the intersection of:
- Military field equipment aesthetics (stencil type, olive drab, signal orange)
- Tactile physical puzzle toys (chunky geometry, satisfying press-in depth)
- Mobile strategy game UI clarity (never obscure information)

**Visual hierarchy rule:** danger reads first, opportunity reads second, chrome reads third.

---

## 2. Color Palette

| Role | Name | Hex | Usage |
|---|---|---|---|
| Background Deep | Field Black | `#0F1117` | Canvas background, modal overlays |
| UI Chrome | Gunmetal | `#1E2330` | Board bg, toolbar, all panel surfaces |
| Tile Hidden | Slate Steel | `#2E3A4E` | Default unrevealed tile face |
| Tile Mine Found | Detonation Amber | `#E8A020` | Mine-found state — primary reward color |
| Tile Miss | Dud Gray | `#455060` | Miss/empty-revealed — muted, not alarming |
| Accent / Signal | Sapper Orange | `#FF5C1A` | Active tool, low-moves warning, CTAs |

Secondary:
- Primary text: `#F0EDE8`
- Secondary text: `#8A96A8`
- Disabled: `#3A3F4A` bg / `#8A96A8` text

---

## 3. Tile Design

**Form:** 80×80px, 6px corner radius
**Depth:** Top-left inner highlight bevel (linear gradient, white 10% → transparent). 1px inner border: top/left `rgba(255,255,255,0.12)`, bottom/right `rgba(0,0,0,0.35)`

| State | Background | Overlay | Icon |
|---|---|---|---|
| Hidden | `#2E3A4E` + bevel | none | none |
| Mine Found | `#E8A020` | radial amber glow 40% | mine icon, white 48px |
| Miss | `#455060` | none | X, `#8A96A8`, 40px |
| Empty Revealed | `#1E2330` | none | none |

**Low-moves warning:** `MovesRemaining <= 3` → moves text switches to `#FF5C1A`
**Tool-active highlight:** pulsing 1px orange border on hidden tiles (0.4→0.8 alpha, 1.2s sine cycle)

---

## 4. Board Direction

- Base: `#1A2233`
- Procedural dot grid overlay: 4px dots at `rgba(255,255,255,0.04)`, tile-pitch spacing (shader)
- Inner vignette: radial gradient transparent → `rgba(0,0,0,0.3)` at edges
- 2px outer border: `#2E3A4E`
- 8px drop shadow: `rgba(0,0,0,0.6)`

Stage theme color tints via `BackgroundTheme` field:
- `default` → `#1A2233` | `jungle` → `#162218` | `urban` → `#1E1E22` | `arctic` → `#182030`

---

## 5. VFX Priority Order

1. **Tile tap press-down spring** — scale 1.0→0.93 (80ms) → 1.05 (60ms) → 1.0 (80ms). Most important.
2. **Mine reveal particle burst** — 12–16 sparks, amber→warm white, radial outward, 0.3–0.5s lifetime
3. **Stage complete celebration** — 40–60 confetti particles, modal slides in from bottom with ease-out-back
4. **Idle board shimmer** — diagonal light sweep, `rgba(255,255,255,0.03)`, every 8–12s. Cut if perf risk.

---

## 6. Achievable Without External Art Assets

**Tiles:** UI shader (bevel gradient, inner border), nested Image for glow, Animator for press spring
**Board:** Shader Graph dot grid + vignette, single reusable shader for all themes
**HUD:** C# color swap for low-moves warning, Animator pulse for tool active state
**VFX:** Unity Particle System (built-in Default-Particle sprite), no custom sprites needed
**Post-processing (URP Volume):**
- Vignette: intensity 0.25, smoothness 0.4
- Color Adjustments: saturation −5, contrast +8
- Bloom: disabled during play, half-intensity on stage complete only

**One custom shader required:** UI bevel/glow (Shader Graph, UI subgraph). Serves all tile states.
**One particle prefab:** reused for mine burst and stage complete variants.

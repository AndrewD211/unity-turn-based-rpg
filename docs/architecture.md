# Architecture Notes

This repository is a curated source-code extract from a larger Unity RPG project. The public version focuses on gameplay logic and intentionally excludes licensed/fan-project media, generated build artifacts, and scene-specific content.

## Responsibilities

### `BattleCharacter`
Owns character combat state:

- hit points
- base attack damage
- hit chance
- evasion
- death notification

The class exposes damage/healing operations and raises an event when HP reaches zero.

### `CombatResolver`
Owns attack math instead of mixing probability and damage calculations directly into turn-flow code.

For each normal attack it:

1. Calculates net hit chance from attacker accuracy and defender evasion.
2. Rolls for hit/miss.
3. Classifies a successful hit as graze, normal, strong, or perfect.
4. Applies damage variance and a quality multiplier.
5. Returns the resulting special-meter gain when appropriate.

This makes the combat model independently tunable from the battle state machine.

### `Inventory`
Uses a singleton that persists across scenes with `DontDestroyOnLoad`. Items are stored by name and quantity and can be added, queried, and consumed.

The public sample includes one generic healing item. The private game project contains additional item behavior and item-selection UI.

### `BattleManager`
Coordinates the battle state machine:

```text
Begin player round
    |
    v
Hero 1 action --> Hero 2 action --> ...
    |                                |
    +--------------------------------+
                     |
                     v
                Enemy phase
                     |
                     v
             Begin next round
```

It also owns:

- current hero index
- selected enemy index
- acted-this-round flags
- per-hero special meter
- enemy target selection
- win/loss detection

## Original private-project systems

The larger project additionally contains scripts for:

- enemy spawning
- clickable hero/enemy selection
- battle HUD updates
- floating damage indicators
- attack animation/sprite swaps
- dialogue management
- cutscene control
- scene loading
- 2D player movement
- encounter-specific behavior

Those files and associated media are omitted here where they are too tightly coupled to fan-project content or do not add useful signal to a source-code portfolio.

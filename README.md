# Unity Turn-Based RPG Systems

A curated, portfolio-safe extract of a larger Unity/C# 2D RPG project. This repository focuses on the systems I implemented rather than the game's licensed/fan-project media assets.

## What this demonstrates

- Turn-based combat with player and enemy phases
- Accuracy, evasion, hit quality, damage variance, and critical/perfect hits
- Per-character special-meter generation and spending
- Enemy target selection and basic enemy AI
- Persistent inventory state across Unity scenes
- Item use and healing/damage effects
- Character health/death events
- Separation of combat resolution from battle-flow state

## Why this is a separate portfolio repository

The original game project contains fan-project artwork, music/audio, build files, and other media that are not appropriate for a public source-code portfolio. This repository intentionally contains only a cleaned, genericized subset of my own C# gameplay-system work.

It is a **source-code portfolio extract**, not a complete playable build.

## Structure

```text
Assets/Scripts/
  BattleCharacter.cs   Character stats, damage, healing, and death events
  CombatResolver.cs    Hit chance, hit quality, damage, and meter-gain logic
  Inventory.cs         Persistent inventory and item consumption
  BattleManager.cs     Player/enemy phase flow, targeting, specials, and state

docs/
  architecture.md      Design notes and system relationships
```

## Combat model

A normal attack is resolved in two stages:

1. **Hit check** — attacker hit chance is reduced by defender evasion.
2. **Hit quality** — successful attacks are classified as graze, normal, strong, or perfect, which changes damage and special-meter gain.

The battle manager owns turn state while `CombatResolver` owns the math, keeping combat calculations reusable and easier to reason about.

## Tech

- C#
- Unity 2021 LTS
- Unity coroutines and event-driven component interactions
- Object-oriented gameplay-system design

## Original project scope

The larger private project also includes scene transitions, dialogue/cutscene controllers, 2D movement, enemy spawning, clickable target selection, inventory UI, floating damage text, battle UI, audio feedback, and multiple encounter scenes.

## Note

Names and item labels in this public version were genericized and media assets were intentionally excluded. The underlying systems are based on gameplay code from my original Unity project.

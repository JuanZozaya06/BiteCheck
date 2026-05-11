# Bite Check Project Brief

## Title

Bite Check

## Concept

Bite Check is a fast 3D mobile casual game where the player works at the last quarantine checkpoint before a survivor shelter. Survivors approach one by one. The player must quickly decide whether they are human or infected.

## Controls

- Swipe right = Admit into shelter
- Swipe left = Send to quarantine

## Core Fun

- Fast moral/visual decisions
- Suspicious symptoms and dialogue
- Physical ragdoll throw after each swipe
- Funny zombie checkpoint chaos
- Short sessions
- Mobile portrait gameplay

## MVP Loop

1. Spawn survivor.
2. Survivor walks toward checkpoint.
3. Show survivor info: name, age, dialogue, symptoms.
4. Player swipes left or right.
5. Evaluate decision.
6. Trigger ragdoll throw.
7. Update stats.
8. Spawn next survivor.
9. End day after 10 survivors.
10. Show summary.

## Stats

- Security starts at 100
- Morale starts at 100
- Resources start at 0
- Day starts at 1

## Wrong Decisions

- Admit infected: security decreases.
- Quarantine human: morale decreases.
- Correct decision: resources increase.

## Lose Condition

- Security <= 0
- Morale <= 0

## Technical Target

- Unity
- Android portrait first
- Editor playable with mouse drag
- Mobile playable with touch swipe
- Code-first prototype
- Placeholder visuals allowed
- No paid assets required

# Art prompts (Codex) — IdleBike

Pixel art. Placeholder procedural sprites are generated in code (`PixelSprites.cs`); these prompts produce the real assets that replace them. Sprite sheets get sliced manually in Unity.

## Global style (prepend to every prompt)

> Pixel art, crisp pixels, NO anti-aliasing, NO blur, transparent background (PNG). Limited palette (~24 colors), soft outlines in a darker shade of the fill color (not pure black). Light source top-left. Side view, facing RIGHT. Consistent scale across the whole set: one bike + rider ≈ 96×72 px inside a 128×96 px frame.

## 1. Riding animation sprite sheets (one per bike tier)

Format for each: **one horizontal row of 8 frames, frame size 128×96 px (sheet 1024×96 px)**. 8-frame pedal cycle (full crank rotation), rider bobs subtly, wheels can have a simple spoke-blur hint on frames 3–6. Rider wears a **pure white jersey and pure white helmet** (they get tinted in-game for cosmetics), dark gray shorts, skin tone medium.

1. **Rusty Trike** — old child's tricycle, oversized adult rider looking cramped, rusty brown frame, tiny wheels, slightly comedic.
2. **Kid's Bike** — small red children's bike, adult rider with knees out, training-wheel mounts visible (no training wheels).
3. **Old Clunker** — heavy gray city bike, bent basket on front, squeaky look, rider upright.
4. **BMX** — green BMX, compact frame, rider slightly crouched, chunky tires.
5. **Mountain Bike** — blue hardtail MTB, knobby tires, straight handlebar, rider athletic.
6. **City Cruiser** — yellow cruiser with fenders and rear rack, relaxed upright rider.
7. **Road Bike** — magenta road bike with drop bars, rider leaning forward, thin tires.
8. **Gravel Racer** — teal gravel bike, drop bars, slightly wider tires, bottle cage.
9. **Track Bike** — white fixed-gear track bike, minimalist, deep aero rims, rider low.
10. **Aero Superbike** — near-black time-trial superbike, disc rear wheel, aero helmet silhouette, rider in full tuck.

## 2. Bike upgrade icons (for the garage/upgrade UI)

**64×64 px each, one 640×64 sheet or separate files.** Icon = the bike alone (no rider), side view, centered, reading clearly at small size. Same 10 bikes as above, same colors.

## 3. Cosmetics (visible to other players)

Jerseys/helmets are tinted white sprites, so no art needed for colors. Extra cosmetic items, each worn by the same rider proportions as the animation sheets:

- **Helmet styles**: classic vented helmet, retro leather cap, aero teardrop helmet — 3 sheets of 8 frames matching rider head position, white base for tinting, 128×96 frames (helmet layer only, rest transparent).
- **Trail effects** (behind rear wheel): sparkle trail, flame trail, rainbow ribbon — 8-frame loops, 96×48 px frames.

## 4. Environment set (single PNG sheet, grid-aligned 16 px cells)

Same palette family, sunny day:
- Tileable road strip (side view): asphalt with white dashed line, 64×16 px, must tile horizontally.
- 3 trees (oak, poplar, pine), 2 bushes, flower patch — 16–48 px tall.
- Rolling green hill silhouette, tileable, 128×32 px.
- Far mountain ridge with snow caps, tileable, 128×48 px, desaturated blue.
- 3 clouds, 24–48 px wide.
- Km marker post, 8×24 px.

## 5. UI icons (white monochrome, tinted in-game)

**32×32 px each**: skill tree (branching node), shopping cart, bicycle, gear/settings, coin, lightning bolt, speaker on, speaker off, vibration/phone-buzz, play, X close.

# IdleBike

Incremental idle cycling game. Unity 6 (6000.3.19f1), URP 2D renderer, Input System only, portrait mobile (Android/iOS). Social features + server support planned.

## Conventions

- **Everything is built from code** — `GameBootstrap` (RuntimeInitializeOnLoadMethod) constructs the world, UI, audio at startup. No scene setup needed; open any scene (e.g. `Assets/Scenes/SampleScene.unity`) and press Play.
- Game text: **English only**. Art style: **pixel art**. Placeholder sprites/audio are procedural (`PixelSprites.cs`, `ProceduralSfx.cs`); real assets come from prompts in `docs/ART_PROMPTS.md` and `docs/SOUND_PROMPTS.md` (user generates via Codex and slices sheets manually).
- 1 world unit = 1 meter. Player is always at x=0; the world scrolls by `GameState.Data.totalDistance`.
- Save: JSON at `Application.persistentDataPath/idlebike_save.json`, autosaved every 5 s and on pause/quit.
- Haptics via `Haptics` (Android JNI + iOS plugin `Assets/Plugins/iOS/IdleBikeHaptics.mm`), always respect the vibration setting.
- New UI is code-built via `UIFactory`; panels derive from `UIPanel` and are opened through `UIRoot` (animated transitions).

## Workflow

- Never commit to main. Feature branch → push → PR (compare URL; no gh CLI). User reviews; "PR māsterā" = merged, then sync main.
- Compile check: `& "C:\Program Files\Unity\Hub\Editor\6000.3.19f1\Editor\Unity.exe" -batchmode -quit -projectPath . -logFile <log>` (fails if the editor has the project open).

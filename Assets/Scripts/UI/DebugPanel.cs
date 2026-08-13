using UnityEngine;
using UnityEngine.UI;

namespace IdleBike
{
    /// <summary>Dev-only panel: cosmetic overlay toggles, live tuning, cheats.</summary>
    public class DebugPanel : UIPanel
    {
        protected override string Title => "DEV TOOLS";
        protected override Vector2 WindowSize => new Vector2(960f, 1150f);

        DebugTools _tools;

        public void SetTools(DebugTools tools) => _tools = tools;

        protected override void BuildContent()
        {
            AddToggleRow(-170f, "HELMETS VISIBLE", () => !DebugFlags.HideHelmets, v => DebugFlags.HideHelmets = !v);
            AddToggleRow(-280f, "TRAILS VISIBLE", () => !DebugFlags.HideTrails, v => DebugFlags.HideTrails = !v);
            AddToggleRow(-390f, "LIVE TUNING (WATCH SO EDITS)", () => DebugFlags.LiveTuning, v => DebugFlags.LiveTuning = v);

            var apply = UIFactory.Button(Window, "Apply", "RE-APPLY VISUAL TUNING NOW", 34,
                UIFactory.AccentBlue, () => { if (_tools != null) _tools.ApplyVisuals(); });
            UIFactory.SetPoint(apply.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0f, -520f), new Vector2(760f, 90f));

            var coins = UIFactory.Button(Window, "Coins", "+ COINS (10x UPGRADE COST)", 34,
                UIFactory.Accent, () => GameState.AddCoins(Upgrades.NextCost * 10.0));
            UIFactory.SetPoint(coins.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0f, -640f), new Vector2(760f, 90f));

            var level = UIFactory.Button(Window, "Level", "+1 BIKE LEVEL (FREE)", 34,
                UIFactory.Accent, () =>
                {
                    GameState.Data.bikeLevel++;
                    GameState.NotifyBikeLevelChanged();
                });
            UIFactory.SetPoint(level.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0f, -760f), new Vector2(760f, 90f));

            var buff = UIFactory.Button(Window, "Buff", "GIVE SPEED BUFF + FULL SPRINT", 34,
                UIFactory.Accent, () =>
                {
                    GameState.BuffTimeLeft = Tuning.Balance.buffDuration;
                    GameState.SprintEnergy = SkillEffects.EffectiveSprintMax;
                });
            UIFactory.SetPoint(buff.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0f, -880f), new Vector2(760f, 90f));

            var note = UIFactory.Text(Window, "Note", "EDITOR / DEVELOPMENT BUILDS ONLY", 24, UIFactory.TextDim);
            UIFactory.SetPoint(note.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 40f), new Vector2(760f, 32f));
        }

        void AddToggleRow(float y, string label, System.Func<bool> get, System.Action<bool> set)
        {
            var text = UIFactory.Text(Window, "L" + label, label, 32, UIFactory.TextMain, TextAnchor.MiddleLeft);
            UIFactory.SetPoint(text.rectTransform, new Vector2(0f, 1f), new Vector2(60f, y - 45f), new Vector2(640f, 90f));

            var toggle = UIFactory.Toggle(Window, "T" + label, get(), set);
            var rt = toggle.GetComponent<RectTransform>();
            UIFactory.SetPoint(rt, new Vector2(1f, 1f), new Vector2(-60f, y - 45f), new Vector2(80f, 80f));

            _refreshers.Add(() => toggle.SetIsOnWithoutNotify(get()));
        }

        readonly System.Collections.Generic.List<System.Action> _refreshers = new System.Collections.Generic.List<System.Action>();

        public override void OnOpened()
        {
            foreach (var r in _refreshers) r();
        }
    }
}

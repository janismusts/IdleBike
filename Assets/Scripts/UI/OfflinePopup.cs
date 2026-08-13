using UnityEngine;

namespace IdleBike
{
    public class OfflinePopup : UIPanel
    {
        protected override string Title => "WELCOME BACK!";
        protected override Vector2 WindowSize => new Vector2(900f, 640f);

        UnityEngine.UI.Text _body;

        protected override void BuildContent()
        {
            _body = UIFactory.Text(Window, "Body", "", 36, UIFactory.TextMain);
            UIFactory.SetPoint(_body.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, 30f), new Vector2(820f, 220f));

            var collect = UIFactory.Button(Window, "Collect", "COLLECT", 44, UIFactory.Accent, () =>
            {
                Root.Manager.CollectOffline();
                AudioManager.I.PlayOfflineCollect();
                Haptics.Medium();
                Root.ClosePanel();
            });
            UIFactory.SetPoint(collect.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0f, 50f), new Vector2(500f, 110f));
        }

        public override void OnOpened()
        {
            var m = Root.Manager;
            string time = FormatTime(m.OfflineSeconds);
            _body.text = $"WHILE YOU WERE AWAY ({time})\nYOU KEPT ON RIDING:\n\n{NumberFormat.Distance(m.OfflineMeters)}   |   +{NumberFormat.Coins(m.OfflineCoins)} COINS";
        }

        // Dismissing via the dim scrim or X must not discard the reward —
        // collect on any close (idempotent: COLLECT already zeroed the fields).
        public override void OnClosed() => Root.Manager.CollectOffline();

        static string FormatTime(double seconds)
        {
            if (seconds >= 3600) return $"{(int)(seconds / 3600)}h {(int)(seconds % 3600 / 60)}m";
            if (seconds >= 60) return $"{(int)(seconds / 60)}m";
            return $"{(int)seconds}s";
        }
    }
}

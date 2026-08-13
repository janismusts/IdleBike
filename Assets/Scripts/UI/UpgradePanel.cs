using UnityEngine;
using UnityEngine.UI;

namespace IdleBike
{
    public class UpgradePanel : UIPanel
    {
        protected override string Title => "BIKE UPGRADES";

        Text _bikeName;
        Text _levelText;
        Text _speedText;
        Button _buyButton;
        Text _buyLabel;
        Image _buyBg;
        RectTransform _tierContent;
        float _refreshTimer;

        protected override void BuildContent()
        {
            _bikeName = UIFactory.Text(Window, "BikeName", "", 52, UIFactory.Accent);
            UIFactory.SetPoint(_bikeName.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -160f), new Vector2(860f, 64f));

            _levelText = UIFactory.Text(Window, "Level", "", 40, UIFactory.TextMain);
            UIFactory.SetPoint(_levelText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -225f), new Vector2(860f, 50f));

            _speedText = UIFactory.Text(Window, "SpeedInfo", "", 36, UIFactory.TextDim);
            UIFactory.SetPoint(_speedText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -280f), new Vector2(860f, 46f));

            _buyButton = UIFactory.Button(Window, "BuyBtn", "", 0, UIFactory.Accent, Buy);
            var buyRt = _buyButton.GetComponent<RectTransform>();
            UIFactory.SetPoint(buyRt, new Vector2(0.5f, 1f), new Vector2(0f, -345f), new Vector2(860f, 110f));
            _buyBg = _buyButton.GetComponent<Image>();
            _buyLabel = UIFactory.Text(_buyButton.transform, "Label", "", 44, Color.white);
            UIFactory.Fill(_buyLabel.rectTransform);

            var listTitle = UIFactory.Text(Window, "ListTitle", "GARAGE", 38, UIFactory.TextDim);
            UIFactory.SetPoint(listTitle.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -500f), new Vector2(860f, 44f));

            _tierContent = UIFactory.ScrollView(Window, "TierList", out _);
            var scrollRt = Window.Find("TierList").GetComponent<RectTransform>();
            scrollRt.anchorMin = new Vector2(0f, 0f);
            scrollRt.anchorMax = new Vector2(1f, 1f);
            scrollRt.offsetMin = new Vector2(40f, 40f);
            scrollRt.offsetMax = new Vector2(-40f, -540f);

            BuildTierRows();
        }

        void BuildTierRows()
        {
            foreach (Transform child in _tierContent) Destroy(child.gameObject);
            int level = GameState.Data.bikeLevel;
            var current = BikeDefs.TierForLevel(level);
            for (int t = 0; t < BikeDefs.Tiers.Length; t++)
            {
                var tier = BikeDefs.Tiers[t];
                var row = UIFactory.Image(_tierContent, "Row", UIFactory.RowBg, PixelSprites.White());
                var rt = row.rectTransform;
                rt.sizeDelta = new Vector2(0f, 96f);

                var iconArt = ArtLibrary.BikeIcon(t);
                var swatch = UIFactory.Image(row.transform, "Swatch",
                    iconArt != null ? Color.white : (Color)tier.FrameColor,
                    iconArt != null ? iconArt : PixelSprites.White());
                UIFactory.SetPoint(swatch.rectTransform, new Vector2(0f, 0.5f), new Vector2(20f, 0f),
                    iconArt != null ? new Vector2(72f, 72f) : new Vector2(56f, 56f));

                var name = UIFactory.Text(row.transform, "Name", tier.Name, 38,
                    tier == current ? UIFactory.Accent : UIFactory.TextMain, TextAnchor.MiddleLeft);
                UIFactory.SetPoint(name.rectTransform, new Vector2(0f, 0.5f), new Vector2(100f, 12f), new Vector2(500f, 44f));

                string status = tier == current ? "CURRENT"
                    : level >= tier.UnlockLevel ? "OWNED"
                    : $"UNLOCKS AT LV {tier.UnlockLevel}";
                var st = UIFactory.Text(row.transform, "Status", status, 28, UIFactory.TextDim, TextAnchor.MiddleLeft);
                UIFactory.SetPoint(st.rectTransform, new Vector2(0f, 0.5f), new Vector2(100f, -26f), new Vector2(500f, 36f));
            }
        }

        void Buy()
        {
            if (Upgrades.BuyLevel(out bool tierChanged))
            {
                if (tierChanged)
                {
                    AudioManager.I.PlayUpgrade();
                    Haptics.Heavy();
                    BuildTierRows();
                }
                else
                {
                    AudioManager.I.PlayCoin();
                    Haptics.Medium();
                }
                Refresh();
            }
            else
            {
                AudioManager.I.PlayError();
            }
        }

        public override void OnOpened()
        {
            BuildTierRows();
            Refresh();
        }

        void Update()
        {
            _refreshTimer += Time.unscaledDeltaTime;
            if (_refreshTimer >= 0.2f)
            {
                _refreshTimer = 0f;
                Refresh();
            }
        }

        void Refresh()
        {
            if (GameState.Data == null) return;
            int level = GameState.Data.bikeLevel;
            var tier = BikeDefs.TierForLevel(level);
            var next = BikeDefs.NextTier(level);

            _bikeName.text = tier.Name.ToUpperInvariant();
            _levelText.text = $"LEVEL {level}";
            float now = BikeDefs.CruiseSpeed(level);
            float after = BikeDefs.CruiseSpeed(level + 1);
            _speedText.text = $"{NumberFormat.Speed(now)}  >  {NumberFormat.Speed(after)}" +
                (next != null ? $"   |   NEXT BIKE AT LV {next.UnlockLevel}" : "");

            bool afford = Upgrades.CanAfford;
            _buyLabel.text = $"UPGRADE   {NumberFormat.Coins(Upgrades.NextCost)} COINS";
            _buyBg.color = afford ? UIFactory.Accent : new Color(0.3f, 0.32f, 0.36f);
            _buyButton.interactable = true; // always clickable; failure plays error sound
        }
    }
}

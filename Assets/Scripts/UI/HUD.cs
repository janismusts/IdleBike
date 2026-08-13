using UnityEngine;
using UnityEngine.UI;

namespace IdleBike
{
    /// <summary>In-ride HUD: distance, speed, coins, sprint bar, bottom bar + ad banner.</summary>
    public class HUD : MonoBehaviour
    {
        UIRoot _root;

        Text _distanceText;
        Text _speedText;
        Text _coinsText;
        Image _sprintFill;
        Text _draftChip;
        Image _draftChipBg;
        Text _buffChip;
        Image _buffChipBg;
        Image _bikeBadge;

        const float BannerH = 140f;
        const float BarH = 200f;

        public void Build(UIRoot root, Transform canvasTr, RectTransform safeArea, SafeAreaFitter fitter)
        {
            _root = root;
            Transform canvas = safeArea; // HUD strips live inside the safe area

            // --- Top: coins (left), settings (right) ---
            var coinIcon = UIFactory.Image(canvas, "CoinIcon", Color.white, PixelSprites.Coin());
            UIFactory.SetPoint(coinIcon.rectTransform, new Vector2(0f, 1f), new Vector2(36f, -36f), new Vector2(56f, 56f));
            coinIcon.raycastTarget = false;

            _coinsText = UIFactory.Text(canvas, "Coins", "0", 52, UIFactory.TextMain, TextAnchor.MiddleLeft);
            UIFactory.SetPoint(_coinsText.rectTransform, new Vector2(0f, 1f), new Vector2(108f, -36f), new Vector2(400f, 56f));

            var settings = UIFactory.Button(canvas, "SettingsBtn", "", 0, new Color(0f, 0f, 0f, 0.35f),
                () => _root.OpenSettings());
            UIFactory.SetPoint(settings.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(-28f, -28f), new Vector2(104f, 104f));
            var gear = UIFactory.Image(settings.transform, "Icon", UIFactory.TextMain, PixelSprites.IconGear());
            UIFactory.SetPoint(gear.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(64f, 64f));
            gear.raycastTarget = false;

            // --- Distance + speed ---
            _distanceText = UIFactory.Text(canvas, "Distance", "0 m", 78, UIFactory.TextMain);
            UIFactory.SetPoint(_distanceText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -150f), new Vector2(800f, 90f));
            _distanceText.raycastTarget = false;

            _speedText = UIFactory.Text(canvas, "Speed", "0.0 km/h", 44, UIFactory.TextDim);
            UIFactory.SetPoint(_speedText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -230f), new Vector2(600f, 60f));

            // --- Status chips ---
            _draftChipBg = UIFactory.Image(canvas, "DraftChip", new Color(0.2f, 0.6f, 0.3f, 0.85f), PixelSprites.White());
            UIFactory.SetPoint(_draftChipBg.rectTransform, new Vector2(0.5f, 1f), new Vector2(-130f, -305f), new Vector2(230f, 54f));
            _draftChipBg.raycastTarget = false; // must not swallow sprint holds
            _draftChip = UIFactory.Text(_draftChipBg.transform, "Label", "DRAFTING", 32, Color.white);
            UIFactory.Fill(_draftChip.rectTransform);

            _buffChipBg = UIFactory.Image(canvas, "BuffChip", new Color(0.95f, 0.6f, 0.15f, 0.9f), PixelSprites.White());
            UIFactory.SetPoint(_buffChipBg.rectTransform, new Vector2(0.5f, 1f), new Vector2(130f, -305f), new Vector2(230f, 54f));
            _buffChipBg.raycastTarget = false;
            _buffChip = UIFactory.Text(_buffChipBg.transform, "Label", "SPEED x1.5", 32, Color.white);
            UIFactory.Fill(_buffChip.rectTransform);

            // --- Sprint bar ---
            float sprintY = BannerH + BarH + 40f;
            var sprintBg = UIFactory.Image(canvas, "SprintBar", new Color(0f, 0f, 0f, 0.5f), PixelSprites.White());
            UIFactory.SetPoint(sprintBg.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, sprintY), new Vector2(760f, 44f));
            sprintBg.raycastTarget = false;

            _sprintFill = UIFactory.Image(sprintBg.transform, "Fill", new Color(0.95f, 0.8f, 0.2f), PixelSprites.White());
            _sprintFill.rectTransform.anchorMin = Vector2.zero;
            _sprintFill.rectTransform.anchorMax = Vector2.one;
            _sprintFill.rectTransform.offsetMin = new Vector2(4f, 4f);
            _sprintFill.rectTransform.offsetMax = new Vector2(-4f, -4f);
            _sprintFill.type = Image.Type.Filled;
            _sprintFill.fillMethod = Image.FillMethod.Horizontal;
            _sprintFill.raycastTarget = false;

            var hint = UIFactory.Text(canvas, "SprintHint", "HOLD SCREEN TO SPRINT", 28, UIFactory.TextDim);
            UIFactory.SetPoint(hint.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, sprintY - 38f), new Vector2(600f, 36f));

            // --- Bottom bar: Skills / Bike / Shop ---
            var bar = UIFactory.Image(canvas, "BottomBar", new Color(0.08f, 0.09f, 0.13f, 0.95f), PixelSprites.White());
            bar.rectTransform.anchorMin = new Vector2(0f, 0f);
            bar.rectTransform.anchorMax = new Vector2(1f, 0f);
            bar.rectTransform.pivot = new Vector2(0.5f, 0f);
            bar.rectTransform.anchoredPosition = new Vector2(0f, BannerH);
            bar.rectTransform.sizeDelta = new Vector2(0f, BarH);

            BuildBarButton(bar.transform, 0, "SKILLS", PixelSprites.IconSkills(), () => _root.OpenSkills());
            var bikeBtn = BuildBarButton(bar.transform, 1, "BIKE", PixelSprites.IconBike(), () => _root.OpenUpgrades());
            BuildBarButton(bar.transform, 2, "SHOP", PixelSprites.IconShop(), () => _root.OpenShop());

            _bikeBadge = UIFactory.Image(bikeBtn.transform, "Badge", UIFactory.Accent, PixelSprites.Coin());
            UIFactory.SetPoint(_bikeBadge.rectTransform, new Vector2(1f, 1f), new Vector2(-40f, -12f), new Vector2(40f, 40f));
            _bikeBadge.raycastTarget = false;

            // --- Ad banner placeholder (full-bleed: raw bottom edge up to safe-bottom + BannerH) ---
            var banner = UIFactory.Image(canvasTr, "AdBanner", new Color(0.16f, 0.16f, 0.18f), PixelSprites.White());
            banner.rectTransform.anchorMin = new Vector2(0f, 0f);
            banner.rectTransform.anchorMax = new Vector2(1f, 0f);
            banner.rectTransform.pivot = new Vector2(0.5f, 0f);
            banner.rectTransform.anchoredPosition = Vector2.zero;
            banner.rectTransform.sizeDelta = new Vector2(0f, BannerH);
            fitter.BannerRect = banner.rectTransform;
            fitter.BannerHeight = BannerH;
            var bannerLabel = UIFactory.Text(banner.transform, "Label", "AD BANNER", 34, new Color(0.45f, 0.45f, 0.5f));
            UIFactory.SetPoint(bannerLabel.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -BannerH * 0.5f), new Vector2(600f, 40f));
        }

        Button BuildBarButton(Transform bar, int index, string label, Sprite icon, System.Action onClick)
        {
            var btn = UIFactory.Button(bar, label + "Btn", "", 0, new Color(1f, 1f, 1f, 0.05f), onClick);
            var rt = btn.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(index / 3f, 0f);
            rt.anchorMax = new Vector2((index + 1) / 3f, 1f);
            rt.offsetMin = new Vector2(10f, 12f);
            rt.offsetMax = new Vector2(-10f, -12f);

            var img = UIFactory.Image(btn.transform, "Icon", UIFactory.TextMain, icon);
            UIFactory.SetPoint(img.rectTransform, new Vector2(0.5f, 0.62f), Vector2.zero, new Vector2(84f, 84f));
            img.raycastTarget = false;

            var txt = UIFactory.Text(btn.transform, "Label", label, 32, UIFactory.TextDim);
            UIFactory.SetPoint(txt.rectTransform, new Vector2(0.5f, 0.16f), Vector2.zero, new Vector2(220f, 40f));
            return btn;
        }

        void Update()
        {
            if (GameState.Data == null) return;
            _distanceText.text = NumberFormat.Distance(GameState.Data.totalDistance);
            _speedText.text = NumberFormat.Speed(GameState.CurrentSpeed);
            _coinsText.text = NumberFormat.Coins(GameState.Data.coins);
            _sprintFill.fillAmount = GameState.SprintEnergy / Tuning.Balance.sprintMax;
            _sprintFill.color = GameState.IsSprinting
                ? new Color(1f, 0.55f, 0.15f)
                : new Color(0.95f, 0.8f, 0.2f);

            _draftChipBg.gameObject.SetActive(GameState.IsDrafting);
            bool buffOn = GameState.BuffTimeLeft > 0f;
            _buffChipBg.gameObject.SetActive(buffOn);
            if (buffOn) _buffChip.text = $"SPEED x{Tuning.Balance.buffMultiplier:0.0} {GameState.BuffTimeLeft:0}s";

            _bikeBadge.gameObject.SetActive(Upgrades.CanAfford);
        }
    }
}

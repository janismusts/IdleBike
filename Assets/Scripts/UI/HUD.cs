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
        Text _gradeChip;
        Image _gradeChipBg;
        Image _teamChipBg;
        Image _bikeBadge;
        Button _refillBtn;
        Image _sprintBtnBg;
        Text _sprintBtnLabel;
        Sprite _sprintBtnReady;
        Sprite _sprintBtnActive;
        Sprite _sprintBtnTired;
        GameObject _emotePicker;
        float _emoteCooldown;

        static readonly Color UphillColor = new Color(0.85f, 0.4f, 0.25f, 0.9f);
        static readonly Color DownhillColor = new Color(0.3f, 0.55f, 0.9f, 0.9f);

        const float BannerH = 140f;
        const float BarH = 200f;

        public void Build(UIRoot root, Transform canvasTr, RectTransform safeArea, SafeAreaFitter fitter)
        {
            _root = root;
            Transform canvas = safeArea; // HUD strips live inside the safe area

            // --- Top: coins (left), settings (right) ---
            var coinArt = ArtLibrary.Icon(ArtLibrary.UiIcon.Coin);
            var coinIcon = UIFactory.Image(canvas, "CoinIcon",
                coinArt != null ? new Color(1f, 0.85f, 0.3f) : Color.white,
                coinArt != null ? coinArt : PixelSprites.Coin());
            UIFactory.SetPoint(coinIcon.rectTransform, new Vector2(0f, 1f), new Vector2(36f, -36f), new Vector2(56f, 56f));
            coinIcon.preserveAspect = true;
            coinIcon.raycastTarget = false;

            _coinsText = UIFactory.Text(canvas, "Coins", "0", 52, UIFactory.TextMain, TextAnchor.MiddleLeft);
            UIFactory.SetPoint(_coinsText.rectTransform, new Vector2(0f, 1f), new Vector2(108f, -36f), new Vector2(400f, 56f));

            var settings = UIFactory.Button(canvas, "SettingsBtn", "", 0, new Color(0f, 0f, 0f, 0.35f),
                () => _root.OpenSettings());
            UIFactory.SetPoint(settings.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(-28f, -28f), new Vector2(104f, 104f));
            var gearArt = ArtLibrary.Icon(ArtLibrary.UiIcon.Gear);
            var gear = UIFactory.Image(settings.transform, "Icon", UIFactory.TextMain,
                gearArt != null ? gearArt : PixelSprites.IconGear());
            UIFactory.SetPoint(gear.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero,
                Vector2.one * Tuning.Visual.hudTopIconSize);
            gear.preserveAspect = true;
            gear.raycastTarget = false;

            // --- Distance + speed ---
            _distanceText = UIFactory.Text(canvas, "Distance", "0 m", 78, UIFactory.TextMain);
            UIFactory.SetPoint(_distanceText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -150f), new Vector2(800f, 90f));
            _distanceText.raycastTarget = false;

            _speedText = UIFactory.Text(canvas, "Speed", "0.0 km/h", 44, UIFactory.TextDim);
            UIFactory.SetPoint(_speedText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -230f), new Vector2(600f, 60f));

            // --- Status chips ---
            _draftChipBg = UIFactory.Image(canvas, "DraftChip", new Color(0.2f, 0.6f, 0.3f, 0.85f), PixelSprites.White());
            UIFactory.SetPoint(_draftChipBg.rectTransform, new Vector2(0.5f, 1f), new Vector2(-360f, -305f), new Vector2(230f, 54f));
            _draftChipBg.raycastTarget = false; // must not swallow sprint holds
            _draftChip = UIFactory.Text(_draftChipBg.transform, "Label", "DRAFTING", 32, Color.white);
            UIFactory.Fill(_draftChip.rectTransform);

            _gradeChipBg = UIFactory.Image(canvas, "GradeChip", UphillColor, PixelSprites.White());
            UIFactory.SetPoint(_gradeChipBg.rectTransform, new Vector2(0.5f, 1f), new Vector2(-120f, -305f), new Vector2(230f, 54f));
            _gradeChipBg.raycastTarget = false;
            _gradeChip = UIFactory.Text(_gradeChipBg.transform, "Label", "UPHILL 5%", 32, Color.white);
            UIFactory.Fill(_gradeChip.rectTransform);

            _buffChipBg = UIFactory.Image(canvas, "BuffChip", new Color(0.95f, 0.6f, 0.15f, 0.9f), PixelSprites.White());
            UIFactory.SetPoint(_buffChipBg.rectTransform, new Vector2(0.5f, 1f), new Vector2(120f, -305f), new Vector2(230f, 54f));
            _buffChipBg.raycastTarget = false;
            _buffChip = UIFactory.Text(_buffChipBg.transform, "Label", "SPEED x1.5", 32, Color.white);
            UIFactory.Fill(_buffChip.rectTransform);

            _teamChipBg = UIFactory.Image(canvas, "TeamChip", new Color(0.25f, 0.5f, 0.75f, 0.9f), PixelSprites.White());
            UIFactory.SetPoint(_teamChipBg.rectTransform, new Vector2(0.5f, 1f), new Vector2(360f, -305f), new Vector2(230f, 54f));
            _teamChipBg.raycastTarget = false;
            var teamChipLabel = UIFactory.Text(_teamChipBg.transform, "Label", "TEAM RIDE", 32, Color.white);
            UIFactory.Fill(teamChipLabel.rectTransform);

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

            var hint = UIFactory.Text(canvas, "SprintHint", "DRAG UP/DOWN TO STEER", 28, UIFactory.TextDim);
            UIFactory.SetPoint(hint.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, sprintY - 38f), new Vector2(600f, 36f));

            // --- Dedicated hold-to-sprint button (right thumb zone) ---
            _sprintBtnBg = UIFactory.Image(canvas, "SprintBtn", new Color(0.95f, 0.8f, 0.2f, 0.95f), PixelSprites.White());
            UIFactory.SetPoint(_sprintBtnBg.rectTransform, new Vector2(1f, 0f), new Vector2(-24f, sprintY + 70f), new Vector2(250f, 160f));
            _sprintBtnBg.gameObject.AddComponent<SprintHoldButton>();
            _sprintBtnReady = ArtLibrary.SprintButton(0);
            _sprintBtnActive = ArtLibrary.SprintButton(1);
            _sprintBtnTired = ArtLibrary.SprintButton(2);
            _sprintBtnLabel = UIFactory.Text(_sprintBtnBg.transform, "Label", "SPRINT", 40, new Color(0.15f, 0.12f, 0.05f));
            UIFactory.Fill(_sprintBtnLabel.rectTransform);
            var holdHint = UIFactory.Text(_sprintBtnBg.transform, "Hint", "HOLD", 22, new Color(0.15f, 0.12f, 0.05f, 0.7f));
            UIFactory.SetPoint(holdHint.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 18f), new Vector2(200f, 26f));

            // Rewarded refill (placeholder ad) — shows when the sprint bar runs low
            _refillBtn = UIFactory.Button(canvas, "RefillBtn", "", 0, new Color(0.95f, 0.6f, 0.15f, 0.95f), () =>
            {
                _root.ShowRewardedAd(() =>
                {
                    GameState.SprintEnergy = SkillEffects.EffectiveSprintMax;
                    AudioManager.I.PlayBuff();
                    Haptics.Medium();
                });
            });
            UIFactory.SetPoint(_refillBtn.GetComponent<RectTransform>(), new Vector2(0.5f, 0f),
                new Vector2(0f, sprintY + 62f), new Vector2(420f, 64f));
            var boltArt = ArtLibrary.Icon(ArtLibrary.UiIcon.Bolt);
            var bolt = UIFactory.Image(_refillBtn.transform, "Icon", Color.white,
                boltArt != null ? boltArt : PixelSprites.BuffBolt());
            UIFactory.SetPoint(bolt.rectTransform, new Vector2(0f, 0.5f), new Vector2(26f, 0f), new Vector2(40f, 40f));
            bolt.preserveAspect = true;
            bolt.raycastTarget = false;
            var refillLabel = UIFactory.Text(_refillBtn.transform, "Label", "REFILL SPRINT - FREE (AD)", 28, Color.white);
            UIFactory.Fill(refillLabel.rectTransform);

            BuildEmoteUi(canvas, sprintY);

            // --- Bottom bar: Skills / Bike / Shop ---
            var bar = UIFactory.Image(canvas, "BottomBar", new Color(0.08f, 0.09f, 0.13f, 0.95f), PixelSprites.White());
            bar.rectTransform.anchorMin = new Vector2(0f, 0f);
            bar.rectTransform.anchorMax = new Vector2(1f, 0f);
            bar.rectTransform.pivot = new Vector2(0.5f, 0f);
            bar.rectTransform.anchoredPosition = new Vector2(0f, BannerH);
            bar.rectTransform.sizeDelta = new Vector2(0f, BarH);

            BuildBarButton(bar.transform, 0, "SKILLS", BarIcon(ArtLibrary.UiIcon.Skills, PixelSprites.IconSkills()), () => _root.OpenSkills());
            var bikeBtn = BuildBarButton(bar.transform, 1, "BIKE", BarIcon(ArtLibrary.UiIcon.Bike, PixelSprites.IconBike()), () => _root.OpenUpgrades());
            BuildBarButton(bar.transform, 2, "SHOP", BarIcon(ArtLibrary.UiIcon.Shop, PixelSprites.IconShop()), () => _root.OpenShop());
            var teamArt = ArtLibrary.Social(ArtLibrary.SocialIcon.Team);
            BuildBarButton(bar.transform, 3, "TEAM", teamArt != null ? teamArt : PixelSprites.IconTeam(), () => _root.OpenTeam());

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

        // --- Emotes: button next to the sprint bar + a 6x2 picker grid ---
        void BuildEmoteUi(Transform canvas, float sprintY)
        {
            var smileyArt = ArtLibrary.Social(ArtLibrary.SocialIcon.Smiley);
            var emoteBtn = UIFactory.Button(canvas, "EmoteBtn", "", 0, new Color(0f, 0f, 0f, 0.45f), ToggleEmotePicker);
            UIFactory.SetPoint(emoteBtn.GetComponent<RectTransform>(), new Vector2(0f, 0f),
                new Vector2(24f, sprintY + 70f), new Vector2(104f, 88f));
            var smiley = UIFactory.Image(emoteBtn.transform, "Icon", UIFactory.TextMain,
                smileyArt != null ? smileyArt : PixelSprites.IconSmiley());
            UIFactory.SetPoint(smiley.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(56f, 56f));
            smiley.preserveAspect = true;
            smiley.raycastTarget = false;

            _emotePicker = new GameObject("EmotePicker");
            _emotePicker.transform.SetParent(canvas, false);
            _emotePicker.layer = LayerMask.NameToLayer("UI");
            var pickerBg = _emotePicker.AddComponent<Image>();
            pickerBg.sprite = PixelSprites.White();
            pickerBg.color = new Color(0.08f, 0.09f, 0.13f, 0.97f);
            UIFactory.SetPoint(pickerBg.rectTransform, new Vector2(0f, 0f),
                new Vector2(24f, sprintY + 172f), new Vector2(590f, 208f));

            for (int i = 0; i < Emotes.Count; i++)
            {
                int idx = i;
                var art = ArtLibrary.Emote(idx);
                var b = UIFactory.Button(_emotePicker.transform, "Emote" + idx, "", 0,
                    new Color(1f, 1f, 1f, 0.06f), () => OnEmotePicked(idx));
                var rt = b.GetComponent<RectTransform>();
                int col = idx % 6, row = idx / 6;
                UIFactory.SetPoint(rt, new Vector2(0f, 1f), new Vector2(10f + col * 96f, -10f - row * 96f), new Vector2(88f, 88f));
                rt.pivot = new Vector2(0f, 1f);
                var icon = UIFactory.Image(b.transform, "Icon", Color.white, art != null ? art : PixelSprites.Emote(idx));
                UIFactory.SetPoint(icon.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero,
                    Vector2.one * Tuning.Visual.emotePickerIconSize);
                icon.preserveAspect = true;
                icon.raycastTarget = false;
            }
            _emotePicker.SetActive(false);
        }

        void ToggleEmotePicker()
        {
            _emotePicker.SetActive(!_emotePicker.activeSelf);
        }

        void OnEmotePicked(int index)
        {
            _emotePicker.SetActive(false);
            if (_emoteCooldown > 0f) return;
            _emoteCooldown = 1.2f;
            var manager = GameManager.I;
            if (manager != null && manager.PlayerVisual != null)
            {
                manager.PlayerVisual.ShowEmote(index);
                AudioManager.I.PlayEmotePop();
            }
            // later: broadcast to nearby players via the server
        }

        static Sprite BarIcon(ArtLibrary.UiIcon artIcon, Sprite fallback)
        {
            var art = ArtLibrary.Icon(artIcon);
            return art != null ? art : fallback;
        }

        Button BuildBarButton(Transform bar, int index, string label, Sprite icon, System.Action onClick)
        {
            var btn = UIFactory.Button(bar, label + "Btn", "", 0, new Color(1f, 1f, 1f, 0.05f), onClick);
            var rt = btn.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(index / 4f, 0f);
            rt.anchorMax = new Vector2((index + 1) / 4f, 1f);
            rt.offsetMin = new Vector2(10f, 12f);
            rt.offsetMax = new Vector2(-10f, -12f);

            var img = UIFactory.Image(btn.transform, "Icon", UIFactory.TextMain, icon);
            UIFactory.SetPoint(img.rectTransform, new Vector2(0.5f, 0.62f), Vector2.zero,
                Vector2.one * Tuning.Visual.barIconSize);
            img.preserveAspect = true;
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
            float sprintMax = SkillEffects.EffectiveSprintMax;
            _sprintFill.fillAmount = sprintMax > 0f ? GameState.SprintEnergy / sprintMax : 0f;
            _sprintFill.color = GameState.IsSprinting
                ? new Color(1f, 0.55f, 0.15f)
                : new Color(0.95f, 0.8f, 0.2f);

            _draftChipBg.gameObject.SetActive(GameState.IsDrafting);
            bool buffOn = GameState.BuffTimeLeft > 0f;
            _buffChipBg.gameObject.SetActive(buffOn);
            if (buffOn) _buffChip.text = $"SPEED x{Tuning.Balance.buffMultiplier:0.0} {GameState.BuffTimeLeft:0}s";

            var terrain = GameManager.I != null ? GameManager.I.Terrain : null;
            bool hill = terrain != null && !terrain.IsFlat;
            _gradeChipBg.gameObject.SetActive(hill);
            if (hill)
            {
                int pct = Mathf.RoundToInt(Mathf.Abs(terrain.CurrentGrade) * 100f);
                bool up = terrain.IsUphill;
                _gradeChip.text = (up ? "UPHILL " : "DOWNHILL ") + pct + "%";
                _gradeChipBg.color = up ? UphillColor : DownhillColor;
            }

            _teamChipBg.gameObject.SetActive(GameState.TeamNearby);
            if (_emoteCooldown > 0f) _emoteCooldown -= Time.unscaledDeltaTime;
            _refillBtn.gameObject.SetActive(GameState.SprintEnergy < sprintMax * 0.25f && !GameState.IsSprinting);
            _bikeBadge.gameObject.SetActive(Upgrades.CanAfford);

            // sprint button: only a FULL bar can start a sprint
            bool hasArt = _sprintBtnReady != null;
            if (GameState.IsSprinting)
            {
                SetSprintButton(_sprintBtnActive, new Color(1f, 0.55f, 0.15f, 0.95f), "SPRINTING", hasArt);
            }
            else if (GameState.SprintEnergy < sprintMax - 0.01f)
            {
                int pct = Mathf.FloorToInt(GameState.SprintEnergy / sprintMax * 100f);
                SetSprintButton(_sprintBtnTired, new Color(0.35f, 0.36f, 0.4f, 0.95f), "CHARGING " + pct + "%", hasArt);
            }
            else
            {
                SetSprintButton(_sprintBtnReady, new Color(0.95f, 0.8f, 0.2f, 0.95f), "SPRINT", hasArt);
            }
        }

        void SetSprintButton(Sprite artSprite, Color fallbackColor, string label, bool hasArt)
        {
            if (hasArt && artSprite != null)
            {
                _sprintBtnBg.sprite = artSprite;
                _sprintBtnBg.color = Color.white;
            }
            else
            {
                _sprintBtnBg.sprite = PixelSprites.White();
                _sprintBtnBg.color = fallbackColor;
            }
            _sprintBtnLabel.text = label;
        }
    }
}

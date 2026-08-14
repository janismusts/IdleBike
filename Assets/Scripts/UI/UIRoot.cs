using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace IdleBike
{
    /// <summary>Builds the whole UI: HUD, bottom bar, panels, fader.</summary>
    public class UIRoot : MonoBehaviour
    {
        public GameManager Manager { get; private set; }
        public ScreenFader Fader { get; private set; }

        Canvas _canvas;
        HUD _hud;
        UpgradePanel _upgradePanel;
        ShopPanel _shopPanel;
        SkillsPanel _skillsPanel;
        TeamPanel _teamPanel;
        SettingsPanel _settingsPanel;
        OfflinePopup _offlinePopup;

        UIPanel _current;
        Coroutine _anim;
        UIPanel _animPanel;
        bool _animClosing;
        RewardedAdOverlay _rewardedAd;
        DebugPanel _debugPanel;

        public void Build(GameManager manager)
        {
            Manager = manager;
            manager.UI = this;
            UIFactory.EnsureEventSystem();
            _canvas = UIFactory.CreateCanvas("Canvas", 10, transform);

            // Steering zone (behind everything else on the canvas): drag up/down moves the rider
            var zone = UIFactory.Image(_canvas.transform, "SteerZone", new Color(0f, 0f, 0f, 0f));
            zone.raycastTarget = true;
            UIFactory.Fill(zone.rectTransform);
            zone.gameObject.AddComponent<SteerTouchZone>();

            // Safe-area container for HUD content (notches, home indicator)
            var safeGo = new GameObject("SafeArea");
            safeGo.transform.SetParent(_canvas.transform, false);
            safeGo.layer = LayerMask.NameToLayer("UI");
            var safeRt = safeGo.AddComponent<RectTransform>();
            UIFactory.Fill(safeRt);
            var fitter = safeGo.AddComponent<SafeAreaFitter>();

            // HUD (top/bottom strips inside the safe area; banner full-bleed on the canvas)
            _hud = _canvas.gameObject.AddComponent<HUD>();
            _hud.Build(this, _canvas.transform, safeRt, fitter);

            // Panels (above HUD)
            _upgradePanel = NewPanel<UpgradePanel>("UpgradePanel");
            _shopPanel = NewPanel<ShopPanel>("ShopPanel");
            _skillsPanel = NewPanel<SkillsPanel>("SkillsPanel");
            _teamPanel = NewPanel<TeamPanel>("TeamPanel");
            _settingsPanel = NewPanel<SettingsPanel>("SettingsPanel");
            _offlinePopup = NewPanel<OfflinePopup>("OfflinePopup");
            if (DebugTools.Enabled) _debugPanel = NewPanel<DebugPanel>("DebugPanel");

            // Rewarded ad placeholder (above panels)
            _rewardedAd = RewardedAdOverlay.Create(_canvas.transform);

            // Fader on top of everything
            Fader = ScreenFader.Create(transform);
            Fader.FadeIn(Tuning.Anim.startFadeDuration);

            if (Manager.OfflineCoins >= 1.0)
                ShowOfflinePopup();
        }

        T NewPanel<T>(string name) where T : UIPanel
        {
            var go = new GameObject(name);
            go.transform.SetParent(_canvas.transform, false);
            var panel = go.AddComponent<T>();
            panel.Create(this);
            return panel;
        }

        public void OpenUpgrades() => TogglePanel(_upgradePanel);
        public void OpenShop() => TogglePanel(_shopPanel);
        public void OpenSkills() => TogglePanel(_skillsPanel);
        public void OpenTeam() => TogglePanel(_teamPanel);
        public void OpenSettings() => TogglePanel(_settingsPanel);

        public void ShowRewardedAd(System.Action onReward) => _rewardedAd.Show(onReward);

        public void OpenDebug(DebugTools tools)
        {
            if (_debugPanel == null) return;
            _debugPanel.SetTools(tools);
            TogglePanel(_debugPanel);
        }

        /// <summary>Small always-on-top DEV button (editor / development builds only).</summary>
        public GameObject AttachDebugButton(DebugTools tools)
        {
            var btn = UIFactory.Button(_canvas.transform, "DevBtn", "DEV", 26,
                new Color(0.55f, 0.25f, 0.6f, 0.85f), () => OpenDebug(tools));
            UIFactory.SetPoint(btn.GetComponent<RectTransform>(), new Vector2(0f, 1f),
                new Vector2(24f, -120f), new Vector2(110f, 56f));
            btn.gameObject.SetActive(Tuning.Visual.showDevButton);
            return btn.gameObject;
        }

        /// <summary>Open (or refresh) the offline earnings popup.</summary>
        public void ShowOfflinePopup()
        {
            if (_current == _offlinePopup)
            {
                _offlinePopup.OnOpened(); // refresh amounts
                return;
            }
            SnapCloseCurrent();
            OpenPanel(_offlinePopup);
        }

        void TogglePanel(UIPanel panel)
        {
            if (_current == panel) { ClosePanel(); return; }
            SnapCloseCurrent();
            OpenPanel(panel);
        }

        public void OpenPanel(UIPanel panel)
        {
            FinishAnim();
            _current = panel;
            panel.OnOpened();
            _animPanel = panel;
            _animClosing = false;
            _anim = StartCoroutine(AnimateOpen(panel));
        }

        public void ClosePanel()
        {
            if (_current == null) return;
            FinishAnim();
            var panel = _current;
            _current = null;
            _animPanel = panel;
            _animClosing = true;
            _anim = StartCoroutine(AnimateClose(panel));
        }

        /// <summary>Instantly close whatever is open (no animation), running OnClosed.</summary>
        void SnapCloseCurrent()
        {
            FinishAnim();
            if (_current == null) return;
            var old = _current;
            _current = null;
            old.gameObject.SetActive(false);
            old.OnClosed();
        }

        /// <summary>Snap a running open/close animation to its end state.</summary>
        void FinishAnim()
        {
            if (_anim != null)
            {
                StopCoroutine(_anim);
                _anim = null;
            }
            if (_animPanel == null) return;
            var p = _animPanel;
            _animPanel = null;
            p.Group.alpha = 1f;
            p.transform.localScale = Vector3.one;
            if (_animClosing)
            {
                p.gameObject.SetActive(false);
                p.OnClosed();
            }
        }

        IEnumerator AnimateOpen(UIPanel panel)
        {
            panel.gameObject.SetActive(true);
            float dur = Tuning.Anim.panelOpenDuration;
            float startScale = Tuning.Anim.panelStartScale;
            float t = 0f;
            while (t < dur)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / dur);
                panel.Group.alpha = k;
                panel.transform.localScale = Vector3.one * Mathf.Lerp(startScale, 1f, k);
                yield return null;
            }
            panel.Group.alpha = 1f;
            panel.transform.localScale = Vector3.one;
            _anim = null;
            _animPanel = null;
        }

        IEnumerator AnimateClose(UIPanel panel)
        {
            float dur = Tuning.Anim.panelCloseDuration;
            float startScale = Tuning.Anim.panelStartScale;
            float t = 0f;
            while (t < dur)
            {
                t += Time.unscaledDeltaTime;
                float k = 1f - Mathf.Clamp01(t / dur);
                panel.Group.alpha = k;
                panel.transform.localScale = Vector3.one * Mathf.Lerp(startScale, 1f, k);
                yield return null;
            }
            panel.gameObject.SetActive(false);
            panel.transform.localScale = Vector3.one;
            panel.Group.alpha = 1f;
            _anim = null;
            _animPanel = null;
            panel.OnClosed();
        }
    }
}

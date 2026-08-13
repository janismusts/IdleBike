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
        SettingsPanel _settingsPanel;
        OfflinePopup _offlinePopup;

        UIPanel _current;
        Coroutine _anim;

        public void Build(GameManager manager)
        {
            Manager = manager;
            UIFactory.EnsureEventSystem();
            _canvas = UIFactory.CreateCanvas("Canvas", 10, transform);

            // Sprint zone (behind everything else on the canvas)
            var zone = UIFactory.Image(_canvas.transform, "SprintZone", new Color(0f, 0f, 0f, 0f));
            zone.raycastTarget = true;
            UIFactory.Fill(zone.rectTransform);
            zone.gameObject.AddComponent<SprintTouchZone>();

            // HUD (hud creates its own children on the canvas)
            _hud = _canvas.gameObject.AddComponent<HUD>();
            _hud.Build(this, _canvas.transform);

            // Panels (above HUD)
            _upgradePanel = NewPanel<UpgradePanel>("UpgradePanel");
            _shopPanel = NewPanel<ShopPanel>("ShopPanel");
            _skillsPanel = NewPanel<SkillsPanel>("SkillsPanel");
            _settingsPanel = NewPanel<SettingsPanel>("SettingsPanel");
            _offlinePopup = NewPanel<OfflinePopup>("OfflinePopup");

            // Fader on top of everything
            Fader = ScreenFader.Create(transform);
            Fader.FadeIn(0.6f);

            if (Manager.OfflineCoins >= 1.0)
                OpenPanel(_offlinePopup);
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
        public void OpenSettings() => TogglePanel(_settingsPanel);

        void TogglePanel(UIPanel panel)
        {
            if (_current == panel) { ClosePanel(); return; }
            if (_current != null)
            {
                var old = _current;
                _current = null;
                old.gameObject.SetActive(false);
                old.OnClosed();
            }
            OpenPanel(panel);
        }

        public void OpenPanel(UIPanel panel)
        {
            _current = panel;
            panel.OnOpened();
            if (_anim != null) StopCoroutine(_anim);
            _anim = StartCoroutine(AnimateOpen(panel));
        }

        public void ClosePanel()
        {
            if (_current == null) return;
            var panel = _current;
            _current = null;
            if (_anim != null) StopCoroutine(_anim);
            _anim = StartCoroutine(AnimateClose(panel));
        }

        IEnumerator AnimateOpen(UIPanel panel)
        {
            panel.gameObject.SetActive(true);
            const float dur = 0.16f;
            float t = 0f;
            while (t < dur)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / dur);
                panel.Group.alpha = k;
                panel.transform.localScale = Vector3.one * Mathf.Lerp(0.94f, 1f, k);
                yield return null;
            }
            panel.Group.alpha = 1f;
            panel.transform.localScale = Vector3.one;
        }

        IEnumerator AnimateClose(UIPanel panel)
        {
            const float dur = 0.12f;
            float t = 0f;
            while (t < dur)
            {
                t += Time.unscaledDeltaTime;
                float k = 1f - Mathf.Clamp01(t / dur);
                panel.Group.alpha = k;
                panel.transform.localScale = Vector3.one * Mathf.Lerp(0.94f, 1f, k);
                yield return null;
            }
            panel.gameObject.SetActive(false);
            panel.transform.localScale = Vector3.one;
            panel.Group.alpha = 1f;
            panel.OnClosed();
        }
    }
}

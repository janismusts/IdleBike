using UnityEngine;
using UnityEngine.UI;

namespace IdleBike
{
    public class SettingsPanel : UIPanel
    {
        protected override string Title => "SETTINGS";
        protected override Vector2 WindowSize => new Vector2(960f, 1150f);

        Slider _music;
        Slider _sfx;
        Toggle _vibration;
        GameObject _confirmBox;

        protected override void BuildContent()
        {
            AddLabel("MUSIC", -170f);
            _music = UIFactory.Slider(Window, "MusicSlider", 0.6f, v => AudioManager.I.MusicVolume = v);
            PlaceControl(_music.GetComponent<RectTransform>(), -235f, new Vector2(760f, 56f));

            AddLabel("SOUND EFFECTS", -330f);
            _sfx = UIFactory.Slider(Window, "SfxSlider", 0.8f, v => AudioManager.I.SfxVolume = v);
            PlaceControl(_sfx.GetComponent<RectTransform>(), -395f, new Vector2(760f, 56f));

            AddLabel("VIBRATION", -500f);
            _vibration = UIFactory.Toggle(Window, "VibrationToggle", true, v =>
            {
                GameState.Data.vibration = v;
                if (v) Haptics.Medium();
            });
            PlaceControl(_vibration.GetComponent<RectTransform>(), -575f, new Vector2(90f, 90f));

            var reset = UIFactory.Button(Window, "ResetBtn", "RESET ALL PROGRESS", 40, UIFactory.Danger, ShowConfirm);
            PlaceControl(reset.GetComponent<RectTransform>(), -760f, new Vector2(760f, 100f));

            var version = UIFactory.Text(Window, "Version", "v" + GameConfig.Version, 26, UIFactory.TextDim);
            UIFactory.SetPoint(version.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 30f), new Vector2(400f, 34f));

            BuildConfirmBox();
        }

        void BuildConfirmBox()
        {
            var box = UIFactory.Image(Window, "ConfirmBox", new Color(0.16f, 0.10f, 0.10f, 0.99f), PixelSprites.White());
            UIFactory.SetPoint(box.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, -160f), new Vector2(860f, 360f));
            _confirmBox = box.gameObject;

            var msg = UIFactory.Text(box.transform, "Msg", "RESET ALL PROGRESS?\nTHIS CANNOT BE UNDONE.", 38, UIFactory.TextMain);
            UIFactory.SetPoint(msg.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -80f), new Vector2(800f, 120f));

            var yes = UIFactory.Button(box.transform, "Yes", "RESET", 38, UIFactory.Danger, DoReset);
            UIFactory.SetPoint(yes.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(-200f, 40f), new Vector2(340f, 96f));

            var no = UIFactory.Button(box.transform, "No", "CANCEL", 38, new Color(0.3f, 0.32f, 0.36f),
                () => _confirmBox.SetActive(false));
            UIFactory.SetPoint(no.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(200f, 40f), new Vector2(340f, 96f));

            _confirmBox.SetActive(false);
        }

        void ShowConfirm() => _confirmBox.SetActive(true);

        void DoReset()
        {
            _confirmBox.SetActive(false);
            Root.ClosePanel();
            Root.Fader.Flash(() =>
            {
                SaveSystem.ResetProgress();
                AudioManager.I.MusicVolume = GameState.Data.musicVolume;
                AudioManager.I.SfxVolume = GameState.Data.sfxVolume;
            });
        }

        public override void OnOpened()
        {
            _confirmBox.SetActive(false);
            _music.SetValueWithoutNotify(GameState.Data.musicVolume);
            _sfx.SetValueWithoutNotify(GameState.Data.sfxVolume);
            _vibration.SetIsOnWithoutNotify(GameState.Data.vibration);
        }

        public override void OnClosed() => SaveSystem.Save();

        void AddLabel(string text, float y)
        {
            var t = UIFactory.Text(Window, "L" + text, text, 36, UIFactory.TextMain);
            UIFactory.SetPoint(t.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, y), new Vector2(760f, 44f));
        }

        void PlaceControl(RectTransform rt, float y, Vector2 size)
        {
            UIFactory.SetPoint(rt, new Vector2(0.5f, 1f), new Vector2(0f, y), size);
        }
    }
}

using UnityEngine;
using UnityEngine.UI;

namespace IdleBike
{
    /// <summary>
    /// Team panel: create a team, see teammates, claim and send gifts.
    /// Local simulation for now; the server replaces TeamService internals later.
    /// </summary>
    public class TeamPanel : UIPanel
    {
        protected override string Title => "TEAM";

        RectTransform _content;

        protected override void BuildContent()
        {
            _content = UIFactory.ScrollView(Window, "TeamList", out _);
            var scrollRt = Window.Find("TeamList").GetComponent<RectTransform>();
            scrollRt.anchorMin = new Vector2(0f, 0f);
            scrollRt.anchorMax = new Vector2(1f, 1f);
            scrollRt.offsetMin = new Vector2(40f, 40f);
            scrollRt.offsetMax = new Vector2(-40f, -140f);
        }

        public override void OnOpened() => Rebuild();

        void Rebuild()
        {
            foreach (Transform child in _content) Destroy(child.gameObject);
            if (TeamService.InTeam) BuildTeamView();
            else BuildCreateView();
        }

        // ---------- no team yet ----------

        InputField _nameInput;

        void BuildCreateView()
        {
            AddText("RIDE TOGETHER", 44, UIFactory.Accent, 70f);
            AddText("TEAMMATES SHELTER EACH OTHER FROM THE WIND:\nLESS DRAG WHEN THE TEAM RIDES TOGETHER.\nTEAMMATES ALSO SEND GIFTS THAT HELP YOU PROGRESS.",
                28, UIFactory.TextDim, 130f);

            var inputRow = UIFactory.Image(_content, "InputRow", new Color(0f, 0f, 0f, 0f));
            inputRow.rectTransform.sizeDelta = new Vector2(0f, 100f);
            _nameInput = UIFactory.InputField(inputRow.transform, "TeamName", "TEAM NAME...");
            var inRt = _nameInput.GetComponent<RectTransform>();
            UIFactory.Fill(inRt);
            inRt.offsetMin = new Vector2(20f, 10f);
            inRt.offsetMax = new Vector2(-20f, -10f);

            var btnRow = UIFactory.Image(_content, "BtnRow", new Color(0f, 0f, 0f, 0f));
            btnRow.rectTransform.sizeDelta = new Vector2(0f, 120f);
            var create = UIFactory.Button(btnRow.transform, "Create", "CREATE TEAM", 40, UIFactory.Accent, () =>
            {
                TeamService.Create(_nameInput != null ? _nameInput.text : "");
                AudioManager.I.PlayTeamJoin();
                Haptics.Medium();
                Rebuild();
            });
            UIFactory.SetPoint(create.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(560f, 100f));

            AddText("JOINING OTHER PLAYERS' TEAMS ARRIVES WITH THE SERVER UPDATE.\nFOR NOW YOUR TEAM RIDES WITH CLUB MEMBERS (BOTS).",
                24, UIFactory.TextDim, 90f);
        }

        // ---------- in a team ----------

        void BuildTeamView()
        {
            AddText($"[{TeamService.TeamTag}] {TeamService.TeamName.ToUpperInvariant()}", 46, UIFactory.Accent, 66f);
            AddText("RIDE WITHIN " + Mathf.RoundToInt(Tuning.Balance.teamTogetherRange) +
                " M OF A TEAMMATE: -" + Mathf.RoundToInt(Tuning.Balance.teamDragReduction * 100f) + "% DRAG",
                26, UIFactory.TextDim, 40f);

            // Gifts inbox
            AddText("GIFTS", 38, UIFactory.TextMain, 60f);
            var inbox = GameState.Data.giftInbox;
            if (inbox.Count == 0)
            {
                AddText("NO GIFTS WAITING", 26, UIFactory.TextDim, 44f);
            }
            else
            {
                // copy: claiming mutates the list
                foreach (var gift in inbox.ToArray())
                {
                    var g = gift;
                    var row = AddRow(96f);
                    var icon = UIFactory.Image(row.transform, "Icon", UIFactory.Accent, GiftIcon());
                    UIFactory.SetPoint(icon.rectTransform, new Vector2(0f, 0.5f), new Vector2(24f, 0f), new Vector2(56f, 56f));
                    icon.preserveAspect = true;
                    var name = UIFactory.Text(row.transform, "Name", TeamService.GiftLabel(g.type), 30, UIFactory.TextMain, TextAnchor.MiddleLeft);
                    UIFactory.SetPoint(name.rectTransform, new Vector2(0f, 0.5f), new Vector2(104f, 14f), new Vector2(420f, 40f));
                    var from = UIFactory.Text(row.transform, "From", "FROM " + g.fromName.ToUpperInvariant(), 24, UIFactory.TextDim, TextAnchor.MiddleLeft);
                    UIFactory.SetPoint(from.rectTransform, new Vector2(0f, 0.5f), new Vector2(104f, -22f), new Vector2(420f, 32f));
                    var claim = UIFactory.Button(row.transform, "Claim", "CLAIM", 30, UIFactory.Accent, () =>
                    {
                        TeamService.ClaimGift(g);
                        AudioManager.I.PlayGiftReceive();
                        Haptics.Medium();
                        Rebuild();
                    });
                    UIFactory.SetPoint(claim.GetComponent<RectTransform>(), new Vector2(1f, 0.5f), new Vector2(-20f, 0f), new Vector2(200f, 70f));
                }
            }

            // Teammates
            AddText("TEAMMATES", 38, UIFactory.TextMain, 60f);
            bool canSend = TeamService.CanSendGift;
            foreach (var mate in TeamService.Teammates)
            {
                var m = mate;
                var row = AddRow(96f);
                var swatch = UIFactory.Image(row.transform, "Swatch", m.Jersey, PixelSprites.White());
                UIFactory.SetPoint(swatch.rectTransform, new Vector2(0f, 0.5f), new Vector2(24f, 0f), new Vector2(48f, 48f));
                var name = UIFactory.Text(row.transform, "Name", m.Name.ToUpperInvariant(), 30, UIFactory.TextMain, TextAnchor.MiddleLeft);
                UIFactory.SetPoint(name.rectTransform, new Vector2(0f, 0.5f), new Vector2(96f, 0f), new Vector2(420f, 40f));

                var send = UIFactory.Button(row.transform, "Send", canSend ? "SEND GIFT" : "SENT", 26,
                    canSend ? UIFactory.AccentBlue : new Color(0.3f, 0.32f, 0.36f), () =>
                    {
                        if (!TeamService.CanSendGift)
                        {
                            AudioManager.I.PlayError();
                            return;
                        }
                        TeamService.SendGift(m);
                        AudioManager.I.PlayGiftSend();
                        Haptics.Light();
                        Rebuild();
                    });
                UIFactory.SetPoint(send.GetComponent<RectTransform>(), new Vector2(1f, 0.5f), new Vector2(-20f, 0f), new Vector2(220f, 70f));
            }
            AddText("SENDING A GIFT IS FREE (COOLDOWN " + Mathf.RoundToInt(Tuning.Balance.sendGiftCooldownHours) + "H)",
                24, UIFactory.TextDim, 40f);

            // Leave
            var leaveRow = UIFactory.Image(_content, "LeaveRow", new Color(0f, 0f, 0f, 0f));
            leaveRow.rectTransform.sizeDelta = new Vector2(0f, 110f);
            var leave = UIFactory.Button(leaveRow.transform, "Leave", "LEAVE TEAM", 32, UIFactory.Danger, () =>
            {
                TeamService.Leave();
                Haptics.Medium();
                Rebuild();
            });
            UIFactory.SetPoint(leave.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(420f, 84f));
        }

        // ---------- helpers ----------

        static Sprite GiftIcon()
        {
            var art = ArtLibrary.Social(ArtLibrary.SocialIcon.Gift);
            return art != null ? art : PixelSprites.IconGift();
        }

        void AddText(string text, int size, Color color, float height)
        {
            var t = UIFactory.Text(_content, "T", text, size, color);
            t.rectTransform.sizeDelta = new Vector2(0f, height);
        }

        Image AddRow(float height)
        {
            var row = UIFactory.Image(_content, "Row", UIFactory.RowBg, PixelSprites.White());
            row.rectTransform.sizeDelta = new Vector2(0f, height);
            return row;
        }
    }
}

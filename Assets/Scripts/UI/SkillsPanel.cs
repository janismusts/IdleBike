using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace IdleBike
{
    /// <summary>Skill trees: Climbing, Flats, Endurance. Ranks bought with coins.</summary>
    public class SkillsPanel : UIPanel
    {
        protected override string Title => "SKILLS";

        RectTransform _content;
        readonly List<Row> _rows = new List<Row>();
        float _refreshTimer;

        class Row
        {
            public SkillDef Def;
            public Text Rank;
            public Text Cost;
            public Image BuyBg;
        }

        static Color TreeColor(SkillTree tree)
        {
            switch (tree)
            {
                case SkillTree.Climbing: return new Color(0.85f, 0.45f, 0.25f);
                case SkillTree.Flats: return new Color(0.3f, 0.6f, 0.9f);
                default: return new Color(0.35f, 0.75f, 0.4f);
            }
        }

        protected override void BuildContent()
        {
            _content = UIFactory.ScrollView(Window, "SkillList", out _);
            var scrollRt = Window.Find("SkillList").GetComponent<RectTransform>();
            scrollRt.anchorMin = new Vector2(0f, 0f);
            scrollRt.anchorMax = new Vector2(1f, 1f);
            scrollRt.offsetMin = new Vector2(40f, 40f);
            scrollRt.offsetMax = new Vector2(-40f, -140f);

            foreach (SkillTree tree in System.Enum.GetValues(typeof(SkillTree)))
            {
                AddHeader(SkillDefs.TreeName(tree), TreeColor(tree));
                foreach (var def in SkillDefs.All)
                    if (def.Tree == tree) AddRow(def);
            }
        }

        void AddHeader(string label, Color color)
        {
            var txt = UIFactory.Text(_content, "Header", label, 40, color, TextAnchor.MiddleLeft);
            txt.rectTransform.sizeDelta = new Vector2(0f, 70f);
        }

        void AddRow(SkillDef def)
        {
            var row = UIFactory.Image(_content, "Row", UIFactory.RowBg, PixelSprites.White());
            row.rectTransform.sizeDelta = new Vector2(0f, 120f);

            var stripe = UIFactory.Image(row.transform, "Stripe", TreeColor(def.Tree), PixelSprites.White());
            UIFactory.SetPoint(stripe.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0f), new Vector2(10f, 120f));

            var name = UIFactory.Text(row.transform, "Name", def.Name.ToUpperInvariant(), 34, UIFactory.TextMain, TextAnchor.MiddleLeft);
            UIFactory.SetPoint(name.rectTransform, new Vector2(0f, 0.5f), new Vector2(34f, 26f), new Vector2(430f, 42f));

            var desc = UIFactory.Text(row.transform, "Desc", def.DescFor(1), 26, UIFactory.TextDim, TextAnchor.MiddleLeft);
            UIFactory.SetPoint(desc.rectTransform, new Vector2(0f, 0.5f), new Vector2(34f, -16f), new Vector2(430f, 36f));

            var rank = UIFactory.Text(row.transform, "Rank", "", 28, UIFactory.TextDim, TextAnchor.MiddleLeft);
            UIFactory.SetPoint(rank.rectTransform, new Vector2(0f, 0.5f), new Vector2(34f, -46f), new Vector2(430f, 32f));

            var buy = UIFactory.Button(row.transform, "Buy", "", 0, UIFactory.Accent, () => Buy(def));
            UIFactory.SetPoint(buy.GetComponent<RectTransform>(), new Vector2(1f, 0.5f), new Vector2(-20f, 0f), new Vector2(250f, 84f));
            var buyBg = buy.GetComponent<Image>();
            var cost = UIFactory.Text(buy.transform, "Label", "", 28, Color.white);
            UIFactory.Fill(cost.rectTransform);

            _rows.Add(new Row { Def = def, Rank = rank, Cost = cost, BuyBg = buyBg });
        }

        void Buy(SkillDef def)
        {
            if (SkillSystem.Buy(def))
            {
                AudioManager.I.PlayUpgrade();
                Haptics.Medium();
            }
            else
            {
                AudioManager.I.PlayError();
            }
            Refresh();
        }

        public override void OnOpened() => Refresh();

        void Update()
        {
            _refreshTimer += Time.unscaledDeltaTime;
            if (_refreshTimer >= 0.25f)
            {
                _refreshTimer = 0f;
                Refresh();
            }
        }

        void Refresh()
        {
            if (GameState.Data == null) return;
            foreach (var row in _rows)
            {
                int rank = SkillSystem.Rank(row.Def.Id);
                string pips = "";
                for (int i = 0; i < row.Def.MaxRank; i++) pips += i < rank ? "#" : "-";
                row.Rank.text = $"RANK {rank}/{row.Def.MaxRank}  [{pips}]";

                if (SkillSystem.IsMaxed(row.Def))
                {
                    row.Cost.text = "MAX";
                    row.BuyBg.color = new Color(0.3f, 0.32f, 0.36f);
                }
                else
                {
                    double cost = SkillSystem.CostFor(row.Def);
                    row.Cost.text = $"{NumberFormat.Coins(cost)} C";
                    row.BuyBg.color = GameState.Data.coins >= cost
                        ? UIFactory.Accent
                        : new Color(0.3f, 0.32f, 0.36f);
                }
            }
        }
    }
}

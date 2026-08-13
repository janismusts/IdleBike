using UnityEngine;
using UnityEngine.UI;

namespace IdleBike
{
    /// <summary>Cosmetic shop. Cosmetics change the rider's look (later visible to other players).</summary>
    public class ShopPanel : UIPanel
    {
        protected override string Title => "SHOP";

        RectTransform _content;

        protected override void BuildContent()
        {
            var info = UIFactory.Text(Window, "Info", "COSMETICS — OTHER RIDERS WILL SEE YOUR STYLE", 28, UIFactory.TextDim);
            UIFactory.SetPoint(info.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -150f), new Vector2(880f, 40f));

            _content = UIFactory.ScrollView(Window, "ItemList", out _);
            var scrollRt = Window.Find("ItemList").GetComponent<RectTransform>();
            scrollRt.anchorMin = new Vector2(0f, 0f);
            scrollRt.anchorMax = new Vector2(1f, 1f);
            scrollRt.offsetMin = new Vector2(40f, 40f);
            scrollRt.offsetMax = new Vector2(-40f, -190f);
        }

        public override void OnOpened() => Rebuild();

        void Rebuild()
        {
            foreach (Transform child in _content) Destroy(child.gameObject);

            AddHeader("JERSEYS");
            foreach (var c in Cosmetics.All)
                if (c.Slot == CosmeticSlot.Jersey) AddItem(c);
            AddHeader("HELMETS");
            foreach (var c in Cosmetics.All)
                if (c.Slot == CosmeticSlot.Helmet) AddItem(c);
        }

        void AddHeader(string label)
        {
            var txt = UIFactory.Text(_content, "Header", label, 36, UIFactory.TextDim, TextAnchor.MiddleLeft);
            txt.rectTransform.sizeDelta = new Vector2(0f, 60f);
        }

        void AddItem(CosmeticDef def)
        {
            var row = UIFactory.Image(_content, "Row", UIFactory.RowBg, PixelSprites.White());
            row.rectTransform.sizeDelta = new Vector2(0f, 110f);

            var swatch = UIFactory.Image(row.transform, "Swatch", def.Color, PixelSprites.White());
            UIFactory.SetPoint(swatch.rectTransform, new Vector2(0f, 0.5f), new Vector2(20f, 0f), new Vector2(64f, 64f));

            var name = UIFactory.Text(row.transform, "Name", def.Name, 36, UIFactory.TextMain, TextAnchor.MiddleLeft);
            UIFactory.SetPoint(name.rectTransform, new Vector2(0f, 0.5f), new Vector2(110f, 14f), new Vector2(440f, 44f));

            bool owned = Cosmetics.IsOwned(def.Id);
            bool equipped = Cosmetics.IsEquipped(def.Id);
            string sub = equipped ? "EQUIPPED" : owned ? "OWNED" : $"{NumberFormat.Coins(def.Price)} COINS";
            var subT = UIFactory.Text(row.transform, "Sub", sub, 28, UIFactory.TextDim, TextAnchor.MiddleLeft);
            UIFactory.SetPoint(subT.rectTransform, new Vector2(0f, 0.5f), new Vector2(110f, -26f), new Vector2(440f, 36f));

            if (equipped) return;

            string btnLabel = owned ? "EQUIP" : "BUY";
            var color = owned ? UIFactory.AccentBlue : UIFactory.Accent;
            var btn = UIFactory.Button(row.transform, "Action", btnLabel, 34, color, () =>
            {
                bool ok = owned ? EquipAction(def) : BuyAction(def);
                if (ok) Rebuild();
            });
            UIFactory.SetPoint(btn.GetComponent<RectTransform>(), new Vector2(1f, 0.5f), new Vector2(-20f, 0f), new Vector2(220f, 76f));
        }

        bool EquipAction(CosmeticDef def)
        {
            Cosmetics.Equip(def.Id);
            Haptics.Selection();
            return true;
        }

        bool BuyAction(CosmeticDef def)
        {
            if (Cosmetics.Buy(def.Id))
            {
                AudioManager.I.PlayUpgrade();
                Haptics.Medium();
                return true;
            }
            AudioManager.I.PlayError();
            return false;
        }
    }
}

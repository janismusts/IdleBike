using UnityEngine;

namespace IdleBike
{
    /// <summary>Skill trees preview. Functional trees land in a follow-up update.</summary>
    public class SkillsPanel : UIPanel
    {
        protected override string Title => "SKILLS";
        protected override Vector2 WindowSize => new Vector2(960f, 1200f);

        protected override void BuildContent()
        {
            AddCard(0, "CLIMBING", "Conquer the hills. Less slowdown on climbs,\nstronger legs on steep gradients.");
            AddCard(1, "FLATS", "Master the flat roads. Higher cruising speed\nand efficiency on even ground.");
            AddCard(2, "ENDURANCE", "Sprint longer, recover faster, resist drag.\nGeneral abilities for every ride.");

            var note = UIFactory.Text(Window, "Note", "SKILL TREES ARRIVE IN THE NEXT UPDATE", 30, UIFactory.TextDim);
            UIFactory.SetPoint(note.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 50f), new Vector2(880f, 40f));
        }

        void AddCard(int index, string title, string desc)
        {
            var card = UIFactory.Image(Window, "Card" + index, UIFactory.RowBg, PixelSprites.White());
            UIFactory.SetPoint(card.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -170f - index * 310f), new Vector2(860f, 280f));

            var t = UIFactory.Text(card.transform, "Title", title, 44, UIFactory.TextMain);
            UIFactory.SetPoint(t.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -46f), new Vector2(800f, 52f));

            var d = UIFactory.Text(card.transform, "Desc", desc, 30, UIFactory.TextDim);
            UIFactory.SetPoint(d.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, -14f), new Vector2(800f, 100f));

            var tag = UIFactory.Image(card.transform, "Tag", new Color(0.95f, 0.6f, 0.15f, 0.9f), PixelSprites.White());
            UIFactory.SetPoint(tag.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 34f), new Vector2(280f, 48f));
            var tagT = UIFactory.Text(tag.transform, "Label", "COMING SOON", 26, Color.white);
            UIFactory.Fill(tagT.rectTransform);
        }
    }
}

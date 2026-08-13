using UnityEngine;

namespace IdleBike
{
    public enum CosmeticSlot { Jersey, Helmet, Trail }

    public class CosmeticDef
    {
        public string Id;
        public string Name;
        public CosmeticSlot Slot;
        public double Price;
        public Color32 Color;
        /// <summary>Art sheet variant: helmet style ("classic"/"retro"/"aero") or trail ("sparkle"/"flame"/"rainbow"). Empty = none.</summary>
        public string Style;

        public CosmeticDef(string id, string name, CosmeticSlot slot, double price, Color32 color, string style = "")
        {
            Id = id; Name = name; Slot = slot; Price = price; Color = color; Style = style;
        }
    }

    /// <summary>
    /// Cosmetic upgrades. Visible on the player's rider; later synced to the server so
    /// other players see them too.
    /// </summary>
    public static class Cosmetics
    {
        public static readonly CosmeticDef[] All =
        {
            new CosmeticDef("jersey_red",    "Classic Red",    CosmeticSlot.Jersey, 0,      new Color32(210,  60,  60, 255)),
            new CosmeticDef("jersey_blue",   "Team Blue",      CosmeticSlot.Jersey, 500,    new Color32( 60, 110, 220, 255)),
            new CosmeticDef("jersey_green",  "Neon Green",     CosmeticSlot.Jersey, 2500,   new Color32( 80, 230, 100, 255)),
            new CosmeticDef("jersey_gold",   "Golden Jersey",  CosmeticSlot.Jersey, 20000,  new Color32(240, 200,  50, 255)),
            new CosmeticDef("jersey_night",  "Night Rider",    CosmeticSlot.Jersey, 100000, new Color32( 40,  40,  55, 255)),

            new CosmeticDef("helmet_white",  "White Classic",  CosmeticSlot.Helmet, 0,      new Color32(235, 235, 235, 255), "classic"),
            new CosmeticDef("helmet_red",    "Racing Red",     CosmeticSlot.Helmet, 1500,   new Color32(220,  70,  60, 255), "classic"),
            new CosmeticDef("helmet_retro",  "Retro Leather",  CosmeticSlot.Helmet, 8000,   new Color32(150, 105,  60, 255), "retro"),
            new CosmeticDef("helmet_aero",   "Aero Black",     CosmeticSlot.Helmet, 50000,  new Color32( 45,  45,  55, 255), "aero"),

            new CosmeticDef("trail_none",    "No Trail",       CosmeticSlot.Trail,  0,      new Color32(120, 120, 130, 255)),
            new CosmeticDef("trail_sparkle", "Sparkle Trail",  CosmeticSlot.Trail,  5000,   new Color32(235, 235, 140, 255), "sparkle"),
            new CosmeticDef("trail_flame",   "Flame Trail",    CosmeticSlot.Trail,  25000,  new Color32(240, 130,  50, 255), "flame"),
            new CosmeticDef("trail_rainbow", "Rainbow Ribbon", CosmeticSlot.Trail,  120000, new Color32(160, 100, 220, 255), "rainbow"),
        };

        public static CosmeticDef Get(string id)
        {
            foreach (var c in All) if (c.Id == id) return c;
            return null;
        }

        public static bool IsOwned(string id) => GameState.Data.ownedCosmetics.Contains(id);

        public static string EquippedId(CosmeticSlot slot)
        {
            switch (slot)
            {
                case CosmeticSlot.Jersey: return GameState.Data.equippedJersey;
                case CosmeticSlot.Helmet: return GameState.Data.equippedHelmet;
                default: return GameState.Data.equippedTrail;
            }
        }

        public static bool IsEquipped(string id)
        {
            var def = Get(id);
            return def != null && EquippedId(def.Slot) == id;
        }

        public static bool Buy(string id)
        {
            var def = Get(id);
            if (def == null || IsOwned(id)) return false;
            if (!GameState.SpendCoins(def.Price)) return false;
            GameState.Data.ownedCosmetics.Add(id);
            Equip(id);
            SaveSystem.Save();
            return true;
        }

        public static void Equip(string id)
        {
            var def = Get(id);
            if (def == null || !IsOwned(id)) return;
            switch (def.Slot)
            {
                case CosmeticSlot.Jersey: GameState.Data.equippedJersey = id; break;
                case CosmeticSlot.Helmet: GameState.Data.equippedHelmet = id; break;
                default: GameState.Data.equippedTrail = id; break;
            }
            GameState.NotifyCosmeticsChanged();
            SaveSystem.Save();
        }

        public static CosmeticDef Equipped(CosmeticSlot slot)
        {
            var def = Get(EquippedId(slot));
            if (def != null) return def;
            return slot == CosmeticSlot.Jersey ? Get("jersey_red")
                 : slot == CosmeticSlot.Helmet ? Get("helmet_white")
                 : Get("trail_none");
        }

        public static Color32 EquippedColor(CosmeticSlot slot) => Equipped(slot).Color;
    }
}

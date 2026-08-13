using UnityEngine;

namespace IdleBike
{
    public enum CosmeticSlot { Jersey, Helmet }

    public class CosmeticDef
    {
        public string Id;
        public string Name;
        public CosmeticSlot Slot;
        public double Price;
        public Color32 Color;

        public CosmeticDef(string id, string name, CosmeticSlot slot, double price, Color32 color)
        {
            Id = id; Name = name; Slot = slot; Price = price; Color = color;
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
            new CosmeticDef("jersey_red",    "Classic Red",   CosmeticSlot.Jersey, 0,      new Color32(210,  60,  60, 255)),
            new CosmeticDef("jersey_blue",   "Team Blue",     CosmeticSlot.Jersey, 500,    new Color32( 60, 110, 220, 255)),
            new CosmeticDef("jersey_green",  "Neon Green",    CosmeticSlot.Jersey, 2500,   new Color32( 80, 230, 100, 255)),
            new CosmeticDef("jersey_gold",   "Golden Jersey", CosmeticSlot.Jersey, 20000,  new Color32(240, 200,  50, 255)),
            new CosmeticDef("jersey_night",  "Night Rider",   CosmeticSlot.Jersey, 100000, new Color32( 40,  40,  55, 255)),
            new CosmeticDef("helmet_white",  "White Helmet",  CosmeticSlot.Helmet, 0,      new Color32(235, 235, 235, 255)),
            new CosmeticDef("helmet_red",    "Racing Red",    CosmeticSlot.Helmet, 1500,   new Color32(220,  70,  60, 255)),
            new CosmeticDef("helmet_aero",   "Aero Black",    CosmeticSlot.Helmet, 50000,  new Color32( 35,  35,  45, 255)),
        };

        public static CosmeticDef Get(string id)
        {
            foreach (var c in All) if (c.Id == id) return c;
            return null;
        }

        public static bool IsOwned(string id) => GameState.Data.ownedCosmetics.Contains(id);

        public static bool IsEquipped(string id) =>
            GameState.Data.equippedJersey == id || GameState.Data.equippedHelmet == id;

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
            if (def.Slot == CosmeticSlot.Jersey) GameState.Data.equippedJersey = id;
            else GameState.Data.equippedHelmet = id;
            GameState.NotifyCosmeticsChanged();
            SaveSystem.Save();
        }

        public static Color32 EquippedColor(CosmeticSlot slot)
        {
            var id = slot == CosmeticSlot.Jersey ? GameState.Data.equippedJersey : GameState.Data.equippedHelmet;
            var def = Get(id);
            if (def != null) return def.Color;
            return slot == CosmeticSlot.Jersey ? new Color32(210, 60, 60, 255) : new Color32(235, 235, 235, 255);
        }
    }
}

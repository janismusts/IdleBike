using System.Collections.Generic;
using UnityEngine;

namespace IdleBike
{
    public class Teammate
    {
        public string Name;
        public Color32 Jersey;
        public int TierIndex;
        public float PreferredOffset;   // meters relative to the player
    }

    /// <summary>
    /// Team state. Fully local for now — teammates are simulated riders and gifts are
    /// generated on a timer. The API shape (create/leave/gifts) is what the server
    /// implementation will fill in later.
    /// </summary>
    public static class TeamService
    {
        static readonly string[] BotNames =
        {
            "PedalPete", "WindMaster", "TurboTina", "SpokeLord", "ChainChamp",
            "GearGirl", "SprintKing", "DraftDodger", "HillHawk", "RollingRita",
        };

        static readonly Color32[] TeamColors =
        {
            new Color32(230,  90,  60, 255),
            new Color32( 70, 140, 230, 255),
            new Color32( 90, 200, 110, 255),
            new Color32(235, 190,  60, 255),
            new Color32(180,  90, 220, 255),
        };

        static List<Teammate> _teammates;

        public static event System.Action TeamChanged;
        public static event System.Action GiftsChanged;

        public static bool InTeam => GameState.Data != null && GameState.Data.inTeam;
        public static string TeamName => GameState.Data.teamName;
        public static string TeamTag => GameState.Data.teamTag;

        public static Color32 TeamColor
        {
            get
            {
                if (!InTeam) return TeamColors[0];
                return TeamColors[Mathf.Abs(StableHash(GameState.Data.teamName)) % TeamColors.Length];
            }
        }

        public static List<Teammate> Teammates
        {
            get
            {
                if (_teammates == null) RebuildTeammates();
                return _teammates;
            }
        }

        public static void Create(string name)
        {
            name = (name ?? "").Trim();
            if (name.Length == 0) name = "TEAM " + Random.Range(100, 999);
            if (name.Length > 18) name = name.Substring(0, 18);

            GameState.Data.inTeam = true;
            GameState.Data.teamName = name;
            GameState.Data.teamTag = MakeTag(name);
            GameState.Data.lastGiftGenUnix = SaveSystem.NowUnix();
            RebuildTeammates();
            SaveSystem.Save();
            TeamChanged?.Invoke();
        }

        /// <summary>Drop cached local state (after a save reset/reload).</summary>
        public static void ResetLocal()
        {
            _teammates = null;
        }

        public static void Leave()
        {
            GameState.Data.inTeam = false;
            GameState.Data.teamName = "";
            GameState.Data.teamTag = "";
            GameState.Data.giftInbox.Clear();
            _teammates = null;
            SaveSystem.Save();
            TeamChanged?.Invoke();
        }

        static string MakeTag(string name)
        {
            string letters = "";
            foreach (char ch in name.ToUpperInvariant())
                if (char.IsLetterOrDigit(ch)) { letters += ch; if (letters.Length >= 3) break; }
            return letters.Length > 0 ? letters : "TM";
        }

        static void RebuildTeammates()
        {
            _teammates = new List<Teammate>();
            if (!InTeam) return;
            var b = Tuning.Balance;
            int seed = StableHash(GameState.Data.teamName);
            var rng = new System.Random(seed);
            int start = (seed & 0x7FFFFFFF) % BotNames.Length;
            int count = Mathf.Clamp(b.teamSize, 1, 8);
            for (int i = 0; i < count; i++)
            {
                _teammates.Add(new Teammate
                {
                    Name = BotNames[(start + i * 3 + 1) % BotNames.Length],
                    Jersey = TeamColor,
                    TierIndex = -1, // resolved near the player's tier at spawn
                    PreferredOffset = Mathf.Lerp(-14f, 20f, (float)rng.NextDouble()),
                });
            }
            TeamChanged?.Invoke();
        }

        // ---------- gifts (simulated locally) ----------

        /// <summary>Generate pending gifts for the time passed. Call on boot/resume.</summary>
        public static void GenerateGifts()
        {
            if (!InTeam || Teammates.Count == 0) return;
            var b = Tuning.Balance;
            long now = SaveSystem.NowUnix();
            if (GameState.Data.lastGiftGenUnix <= 0) GameState.Data.lastGiftGenUnix = now;

            double intervalSec = System.Math.Max(600.0, b.giftIntervalHours * 3600.0);
            int intervals = (int)((now - GameState.Data.lastGiftGenUnix) / intervalSec);
            if (intervals <= 0) return;

            var inbox = GameState.Data.giftInbox;
            var rng = new System.Random((int)(now & 0x7FFFFFFF));
            for (int i = 0; i < intervals && inbox.Count < b.giftInboxCap; i++)
            {
                if (rng.NextDouble() > 0.7) continue; // teammates aren't gift machines
                var mate = Teammates[rng.Next(Teammates.Count)];
                inbox.Add(new GiftEntry
                {
                    fromName = mate.Name,
                    type = rng.Next(3),
                    sentUnix = now,
                });
            }
            GameState.Data.lastGiftGenUnix += (long)(intervals * intervalSec);
            SaveSystem.Save();
            GiftsChanged?.Invoke();
        }

        public static string GiftLabel(int type)
        {
            switch (type)
            {
                case 0: return "COIN POUCH";
                case 1: return "ENERGY DRINK";
                default: return "SPEED TONIC";
            }
        }

        /// <summary>Claim a gift from the inbox. Returns a short result text.</summary>
        public static string ClaimGift(GiftEntry gift)
        {
            var b = Tuning.Balance;
            GameState.Data.giftInbox.Remove(gift);
            switch (gift.type)
            {
                case 0:
                    int level = GameState.Data.bikeLevel;
                    double perSec = BikeDefs.CruiseSpeed(level) * (1f - SkillEffects.EffectiveDragPenalty)
                                    * BikeDefs.CoinsPerMeter(level);
                    double coins = System.Math.Max(10.0, perSec * 60.0 * b.giftCoinsMinutes);
                    GameState.AddCoins(coins);
                    SaveSystem.Save();
                    GiftsChanged?.Invoke();
                    return "+" + NumberFormat.Coins(coins) + " COINS";
                case 1:
                    GameState.SprintEnergy = SkillEffects.EffectiveSprintMax;
                    SaveSystem.Save();
                    GiftsChanged?.Invoke();
                    return "SPRINT REFILLED";
                default:
                    GameState.BuffTimeLeft = Mathf.Max(GameState.BuffTimeLeft, b.giftBuffSeconds);
                    SaveSystem.Save();
                    GiftsChanged?.Invoke();
                    return "SPEED BUFF " + Mathf.RoundToInt(b.giftBuffSeconds) + "s";
            }
        }

        public static bool CanSendGift =>
            InTeam && SaveSystem.NowUnix() - GameState.Data.lastGiftSentUnix
                >= Tuning.Balance.sendGiftCooldownHours * 3600.0;

        /// <summary>Send a gift to a teammate (server later; local just tracks the cooldown).</summary>
        public static void SendGift(Teammate mate)
        {
            GameState.Data.lastGiftSentUnix = SaveSystem.NowUnix();
            SaveSystem.Save();
            GiftsChanged?.Invoke();
        }

        static int StableHash(string s)
        {
            unchecked
            {
                int h = 23;
                foreach (char c in s) h = h * 31 + c;
                return h;
            }
        }
    }
}

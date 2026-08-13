namespace IdleBike
{
    public static class Upgrades
    {
        public static double NextCost => BikeDefs.UpgradeCost(GameState.Data.bikeLevel);

        public static bool CanAfford => GameState.Data.coins >= NextCost;

        /// <summary>Try to buy the next bike level. Returns true and whether tier changed.</summary>
        public static bool BuyLevel(out bool tierChanged)
        {
            tierChanged = false;
            var tierBefore = BikeDefs.TierForLevel(GameState.Data.bikeLevel);
            if (!GameState.SpendCoins(NextCost)) return false;
            GameState.Data.bikeLevel++;
            tierChanged = BikeDefs.TierForLevel(GameState.Data.bikeLevel) != tierBefore;
            GameState.NotifyBikeLevelChanged();
            SaveSystem.Save();
            return true;
        }
    }
}

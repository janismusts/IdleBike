using System.Globalization;

namespace IdleBike
{
    public static class NumberFormat
    {
        static readonly string[] Suffixes = { "", "K", "M", "B", "T", "aa", "ab", "ac" };

        public static string Coins(double v)
        {
            if (v < 1000) return ((long)v).ToString(CultureInfo.InvariantCulture);
            int idx = 0;
            while (v >= 1000 && idx < Suffixes.Length - 1) { v /= 1000; idx++; }
            return v.ToString(v >= 100 ? "0" : v >= 10 ? "0.#" : "0.##", CultureInfo.InvariantCulture) + Suffixes[idx];
        }

        public static string Distance(double meters)
        {
            if (meters < 1000) return ((int)meters) + " m";
            double km = meters / 1000.0;
            if (km < 100) return km.ToString("0.00", CultureInfo.InvariantCulture) + " km";
            if (km < 10000) return km.ToString("0.#", CultureInfo.InvariantCulture) + " km";
            return Coins(km) + " km";
        }

        public static string Speed(float metersPerSec)
        {
            return (metersPerSec * 3.6f).ToString("0.0", CultureInfo.InvariantCulture) + " km/h";
        }
    }
}

namespace IdleBike
{
    /// <summary>
    /// Emote reactions shown in a speech bubble above a rider. Later these are
    /// broadcast to nearby players through the server.
    /// </summary>
    public static class Emotes
    {
        public class EmoteDef
        {
            public string Id;
            public string Name;
            public EmoteDef(string id, string name) { Id = id; Name = name; }
        }

        // Order == index in the emote sprite sheet (Art/social/emotes.png).
        public static readonly EmoteDef[] All =
        {
            new EmoteDef("wave",    "Wave"),
            new EmoteDef("thumbs",  "Thumbs Up"),
            new EmoteDef("heart",   "Heart"),
            new EmoteDef("laugh",   "Laugh"),
            new EmoteDef("angry",   "Angry"),
            new EmoteDef("sweat",   "Sweating"),
            new EmoteDef("turtle",  "Turtle"),
            new EmoteDef("rocket",  "Rocket"),
            new EmoteDef("muscle",  "Strong"),
            new EmoteDef("zzz",     "Sleepy"),
            new EmoteDef("trophy",  "Trophy"),
            new EmoteDef("fire",    "On Fire"),
        };

        public const int Count = 12;
    }
}

namespace Beursspel
{
    public static class Settings
    {
        /// <summary>
        /// De hoeveelheid geld die een speler heeft bij aanvang van het spel
        /// </summary>
        public const double StartSpelerGeld = 1_000;

        /// <summary>
        /// De waarde van een enkel aandeel van een beurs bij aanvang van het spel
        /// </summary>
        public const double StartBeursWaarde = 20_000;

        /// <summary>
        /// Het aantal aandelen dat een beurs beschikbaar heeft bij aanvang van het spel
        /// </summary>
        public const int StartBeursBeschikbareAandelen = 200;

        public static bool IsOpen { get; set; }
    }
}
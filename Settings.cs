using System.Threading.Tasks;
using Beursspel.Data;
using Microsoft.EntityFrameworkCore;

namespace Beursspel
{
    public static class Settings
    {
        /// <summary>
        /// De hoeveelheid geld die een speler heeft bij aanvang van het spel
        /// </summary>
        public static double StartSpelerGeld
        {
            get => _settings.StartSpelerGeld;
            set
            {
                _settings.StartSpelerGeld = value;
                SaveSettings();
            }
        }

        /// <summary>
        /// De waarde van een enkel aandeel van een beurs bij aanvang van het spel
        /// </summary>
        public static double StartBeursWaarde
        {
            get => _settings.StartBeursWaarde;
            set
            {
                _settings.StartBeursWaarde = value;
                SaveSettings();
            }
        }

        /// <summary>
        /// Het aantal aandelen dat een beurs beschikbaar heeft bij aanvang van het spel
        /// </summary>
        public static int StartBeursBeschikbareAandelen
        {
            get => _settings.StartBeursBeschikbareAandelen;
            set
            {
                _settings.StartBeursBeschikbareAandelen = value;
                SaveSettings();
            }
        }

        public static bool IsOpen
        {
            get => _settings.IsOpen;
            set
            {
                _settings.IsOpen = value;
                SaveSettings();
            }
        }



        public static async Task LoadSettings()
        {
            using (var db = new ApplicationDbContext())
            {
                _settings = await db.Settings.FirstOrDefaultAsync();
                if (_settings == null)
                {
                    _settings = new SettingsHolder();
                    await db.Settings.AddAsync(_settings);
                    await db.SaveChangesAsync();
                }
            }
        }

        private static void SaveSettings()
        {
            using (var db = new ApplicationDbContext())
            {
                db.Update(_settings);
                db.SaveChanges();
            }
        }

        private static SettingsHolder _settings;

        public class SettingsHolder
        {
            public int SettingsHolderId { get; set; }
            public double StartSpelerGeld { get; set; } = 1_000;
            public double StartBeursWaarde { get; set; } = 20_000;
            public int StartBeursBeschikbareAandelen { get; set; } = 200;
            public bool IsOpen { get; set; } = false;
        }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Newtonsoft.Json;

namespace Beursspel.Models.Beurzen
{
    public class Beurs
    {
        [Key]
        public int BeursId { get; set; }
        [Required]
        public string Naam { get; set; }
        [DataType(DataType.MultilineText)]
        public string Omschrijving { get; set; }
        [Required]
        [Range(1,100)]
        public int AantalLeden { get; set; }

        [NotMapped]
        public double HuidigeWaarde
        {
            get
            {
                if (Waardes == null)
                    return 0;
                var last = Waardes.LastOrDefault();
                return last?.Waarde ?? 0;
            }
        }

        public int BeschikbareAandelen { get; set; } = Settings.StartBeursBeschikbareAandelen;

        public double AandeelPrijs => HuidigeWaarde / Settings.StartBeursBeschikbareAandelen;

        public virtual List<BeursWaardes> Waardes { get; set; }
    }

    public class BeursWaardes
    {
        public int BeursWaardesId { get; set; }
        public DateTime Tijd { get; set; }
        public double Waarde { get; set; }

        public enum WaardeType
        {
            Onbekend,
            Aanwezigheid,
            VraagAanbod
        }
        public WaardeType Type { get; set; }

        [JsonIgnore]
        public Beurs Beurs { get; set; }
        public int BeursId { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

        public double HuidigeWaarde { get; set; } = Settings.StartBeursWaarde;
        public int BeschikbareAandelen { get; set; } = Settings.StartBeursBeschikbareAandelen;

        public double AandeelPrijs => HuidigeWaarde / Settings.StartBeursBeschikbareAandelen;

        public List<BeursWaardes> OudeWaardes { get; set; }
    }

    public class BeursWaardes
    {
        public int BeursWaardesId { get; set; }
        public DateTime Tijd { get; set; }
        public double Waarde { get; set; }

        [JsonIgnore]
        public Beurs Beurs { get; set; }
        public int BeursId { get; set; }
    }
}
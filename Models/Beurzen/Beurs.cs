using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;

namespace Beursspel.Models.Beurzen
{
    public class Beurs
    {
        [Key]
        public int BeursId { get; set; }
        [Required]
        public string Naam { get; set; }
        public string Omschrijving { get; set; }
        public double HuidigeWaarde { get; set; }

        public List<BeursWaardes> OudeWaardes { get; set; }
    }

    public class BeursWaardes
    {
        public int BeursWaardesId { get; set; }
        public DateTime Tijd { get; set; }
        public double Waarde { get; set; }

        public Beurs Beurs { get; set; }
        public int BeursId { get; set; }
    }
}
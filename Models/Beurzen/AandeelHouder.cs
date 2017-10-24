using System;

namespace Beursspel.Models.Beurzen
{
    public class AandeelHouder
    {
        public int AandeelHouderId { get; set; }
        public int BeursId { get; set; }
        public int Aantal { get; set; }

        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
    }
}
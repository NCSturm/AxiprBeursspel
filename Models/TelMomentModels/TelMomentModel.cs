using System;
using System.Collections.Generic;

namespace Beursspel.Models.TelMomentModels
{
    public class TelMomentModel
    {
        public int TelMomentModelId { get; set; }

        public int BeursId { get; set; }
        public string BeursNaam { get; set; }
        public int Aantal { get; set; }
    }

    public class TelMomentHouder
    {
        public int TelMomentHouderId { get; set; }

        public virtual List<TelMomentModel> Beurzen { get; set; }
        public DateTime Tijd { get; set; }
        public DateTime LaatstBewerkt { get; set; }
        public string Invoerder { get; set; }
        public string Bewerker { get; set; }
    }
}
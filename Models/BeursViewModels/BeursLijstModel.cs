using System.Collections.Generic;
using Beursspel.Models.Beurzen;

namespace Beursspel.Models.BeursViewModels
{
    public class BeursLijstModel
    {
        public List<Beurs> Beurzen { get; set; }
        public Dictionary<int, int> AandeelAantallen { get; set; }
    }
}
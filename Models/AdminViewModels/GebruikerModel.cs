using System.Collections.Generic;

namespace Beursspel.Models.AdminViewModels
{
    public class GebruikerModel
    {
        public ApplicationUser User { get; set; }
        public Dictionary<int, string> BeursNamen { get; set; }
    }
}
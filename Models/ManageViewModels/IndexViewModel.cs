using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Beursspel.Models.ManageViewModels
{
    public class IndexViewModel
    {
        [Display(Name = "Gebruikersnaam")]
        public string Username { get; set; }
        public string Naam { get; set; }

        public string StatusMessage { get; set; }
    }
}

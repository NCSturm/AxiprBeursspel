using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Beursspel.Models.AccountViewModels
{
    public class RegisterViewModel
    {
        [Required]
        [StringLength(30, ErrorMessage = "Je gebruikersnaam moet tenminste {2}, en maXImaal {1} letters hebben", MinimumLength = 4)]
        [Display(Name = "Gebruikersnaam")]
        public string Username { get; set; }

        [Required]
        [Display(Name = "Naam")]
        public string Naam { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Je wachtwoord moet tenminste {2}, en maXImaal {1} letters hebben.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "Wachtwoord")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Bevestig Wachtwoord")]
        [Compare("Password", ErrorMessage = "Je wachtwoorden komen niet overeen.")]
        public string ConfirmPassword { get; set; }
    }
}

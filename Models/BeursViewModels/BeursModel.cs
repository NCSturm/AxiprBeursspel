using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Beursspel.Models.Beurzen;
using Microsoft.AspNetCore.Mvc;

namespace Beursspel.Models.BeursViewModels
{
    public class BeursModel
    {
        public Beurs Beurs { get; set; }

        public int BeursId { get; set; }
        [Display(Name = "Aantal")]
        public int Aantal { get; set; }
        public double GebruikerGeld { get; set; }
    }
}
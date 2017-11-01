using System;
using System.ComponentModel.DataAnnotations;

namespace Beursspel.Models
{
    public class GeplandTelMoment
    {
        public int GeplandTelMomentId { get; set; }
        [Required]
        [DataType(DataType.Date)]
        public DateTime Tijd { get; set; }
        [Required]
        public string Reden { get; set; }
    }
}
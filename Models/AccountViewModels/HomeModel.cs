using System.Collections.Generic;

namespace Beursspel.Models.AccountViewModels
{
    public class HomeModel
    {
        public List<TopScoreModel> TopScores { get; set; }
        public List<GeplandTelMoment> TelMomenten { get; set; }
    }
}
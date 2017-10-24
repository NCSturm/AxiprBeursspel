using System.Collections.Generic;

namespace Beursspel.Models.AdminViewModels
{
    public class UserListModel
    {
        public string UserId;
        public string Username;
        public string Naam;
        public double HuidigeWaarde;
        public List<string> Rollen;
    }
}
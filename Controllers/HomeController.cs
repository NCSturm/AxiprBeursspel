using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Beursspel.Data;
using Microsoft.AspNetCore.Mvc;
using Beursspel.Models;

namespace Beursspel.Controllers
{
    public class HomeController : Controller
    {
        private static string _adminRoleId;
        public IActionResult Index()
        {
            List<TopScoreModel> users;
            using (var db = new ApplicationDbContext())
            {
                if (_adminRoleId == null)
                    _adminRoleId = db.Roles.SingleOrDefault(x => x.NormalizedName == "ADMIN").Id;
                users = db.Users.Where(x => !db.UserRoles.Any(y => y.UserId == x.Id && y.RoleId != _adminRoleId))
                .OrderByDescending(x => x.Waarde).Take(10).Select(x => new TopScoreModel
                {
                    Naam = x.Naam,
                    Waarde = x.Waarde
                }).ToList();
            }
            return View(users);
        }


        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

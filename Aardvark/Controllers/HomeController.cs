using Aardvark.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;

namespace Aardvark.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            // Determine roles of current user...
            ApplicationDbContext db = new ApplicationDbContext();
            UserRolesViewModel Roles = new UserRolesViewModel(db.Users.Find(User.Identity.GetUserId()).Roles);
            return View(Roles);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}
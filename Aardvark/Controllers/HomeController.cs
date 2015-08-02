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
            //// Code to get rid of all cookies...
            //UserRolesHelper helper = new UserRolesHelper();
            //helper.ZapCookies();

            // Create model...
            var id = User.Identity.GetUserId();
            var altId = HttpContext.User.Identity.GetUserId();
            UserRolesViewModel Model = new UserRolesViewModel(id);
            return View(Model);
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

        public ActionResult ManageUsers()
        {
            ViewBag.Message = "Manage users.";

            return View();
        }
    }
}
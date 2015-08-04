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
        private ApplicationDbContext db = new ApplicationDbContext();

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


        // GET: Home/ManageUsers
        [Authorize(Roles="Admin, Guest")]
        public ActionResult ManageUsers()
        {
            ViewBag.Message = "Manage users.";
            UserRolesHelper helper = new UserRolesHelper();

            // Create list of all roles
            List<string> roles = db.Roles.Select(r => r.Name).ToList();
            int nRoles = roles.Count;

            // Create list of users
            List<ManageUsersData> users = db.Users.Select(u => new ManageUsersData()
            {
                Id = u.Id,
                UserName = u.UserName,
                Email = u.Email,
                DisplayName = u.DisplayName,
                First = u.FirstName,
                Last = u.LastName
            }).ToList();

            // And for each user, create list of roles and generate 
            foreach (var user in users)
            {
                user.OrigRoles = new bool[nRoles];
                user.NewRoles = new bool[nRoles];

                var userRoles = helper.ListUserRoles(user.Id);
                for (var i = 0; i < nRoles; i++)
                    user.OrigRoles[i] = user.NewRoles[i] = userRoles.Contains(roles[i]);
            }

            // Now for each user, get list of roles and create origRoles data
            ViewBag.UsersAndRoles = users;
            ViewBag.Roles = roles;
            return View(new ManageUsersModel(users, roles));
        }

        // GET: Home/ManageUsers
        [HttpPost]
        [Authorize(Roles = "Admin, Guest")]
        [ValidateAntiForgeryToken]
        //public ActionResult ManageUsers(IEnumerable<UsersCheckboxes> UserInfo)
        public ActionResult ManageUsers(string[] Select)
        {
            if (ModelState.IsValid)
            {
                // Make sure there is still somebody with Admin privileges!!
                // So, scoop everything up first and recreate the original model





                //ticket.OwnerUserId = User.Identity.GetUserId();
                //ticket.AssignedToUserId = null;
                //db.Tickets.Add(ticket);
                //db.SaveChanges();
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }
    }
}
using Aardvark.Helpers;
using Aardvark.Models;
using Aardvark.ViewModels;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Aardvark.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index(string message)
        {
            //// Code to get rid of all cookies...
            //UserRolesHelper helper = new UserRolesHelper();
            //helper.ZapCookies();

            // Create ViewBag model...
            var id = User.Identity.GetUserId();
            var altId = HttpContext.User.Identity.GetUserId();
            UserRolesViewModel Model = new UserRolesViewModel(id);
            //ViewBag.SuppressDefaultLayout = true;

            // Do this in every GET action...
            ViewBag.UserModel = ProjectsHelper.LoadUserModel();
            ViewBag.Message = message;
            return View(Model);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            // Do this in every GET action...
            ViewBag.UserModel = ProjectsHelper.LoadUserModel();
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            // Do this in every GET action...
            ViewBag.UserModel = ProjectsHelper.LoadUserModel();
            return View();
        }

        public ActionResult Dashboard()
        {
            //// Code to get rid of all cookies...
            //UserRolesHelper helper = new UserRolesHelper();
            //helper.ZapCookies();

            // Create ViewBag model...
            var id = User.Identity.GetUserId();
            var altId = HttpContext.User.Identity.GetUserId();
            //ViewBag.SuppressDefaultLayout = true;

            // Do this in every GET action...
            ProjectsHelper.UserModel userModel = ProjectsHelper.LoadUserModel();
            DashboardModel model = new DashboardModel(userModel, db);
            ViewBag.UserModel = userModel;
            return View(model);
        }

        // GET: Home/ManageUsers
        [HttpPost]
        public ActionResult Dashboard(string[] checks)
        {
            // 'checks' is a list of the ticket Ids that are being pulled


            return RedirectToAction("Dashboard");
        }

        // GET: Home/ManageUsers
        [Authorize(Roles="Admin, Guest")]
        public ActionResult ManageUsers()
        {
            ViewBag.Message = "Manage users.";
            UserRolesHelper helper = new UserRolesHelper();

            // Create list of all roles
            List<string> roles = db.Roles.Select(r => r.Name).ToList();

            // We don't need Guest or NewUser, so remove them
            roles.Remove(R.Guest);
            roles.Remove(R.NewUser);
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
            // Do this in every GET action...
            ViewBag.UserModel = ProjectsHelper.LoadUserModel();
            return View(new ManageUsersModel(users, roles));
        }

        // POST: Home/ManageUsers
        [HttpPost]
        [Authorize(Roles = "Admin, Guest")]
        [ValidateAntiForgeryToken]
        //public ActionResult ManageUsers(IEnumerable<UsersCheckboxes> UserInfo)
        public ActionResult ManageUsers(string submit, string[] Select)
        {
            if (submit == "Cancel")
                return RedirectToAction("Index");

            if (ModelState.IsValid)
            {
                // Make sure there is still somebody with Admin privileges!!
                // So, scoop everything up first and recreate the original model
                if (Select != null)
                {
                    // The first string should be the actions, to parse it
                    string[] roles = Select[0].Split('~');
                    // Find where Admin is so we can make sure somebody is still Admin!
                    int posAdmin = Array.IndexOf(roles, R.Admin);
                    int nRoles;
                    List<ManageUsersData> muList = new List<ManageUsersData>();
                    bool isAdminChecked = false;
                    
                    if (posAdmin > -1)
                    {
                        // We found the Admin role, assume at this point the list of roles is correct
                        nRoles = roles.Count() - 1;

                        // Now, each User is represented by his Id, followed by a T or F for each role he is
                        // currently enrolled in. So now, reconvert all the data back to the original mode, with
                        // the new 
                        int index = 1;
                        while (index < Select.Length)
                        {
                            // Scoop up all the returned data
                            ManageUsersData mu = new ManageUsersData(nRoles);

                            // Get the user Id
                            mu.Id = Select[index++];

                            // Now get all the original settings for roles...
                            for (int z = 0; z < nRoles; z++)
                            {
                                mu.OrigRoles[z] = Select[index][z] == 'T' ? true : false;
                            }
                            index++;        // Advance to next line

                            // Now get any checked roles...
                            int num;
                            while (index < Select.Length && Select[index].Length < 32)
                            {
                                // The next char is an index represented a selected role
                                if (Int32.TryParse(Select[index], out num))
                                {
                                    mu.NewRoles[num] = true;
                                    index++;
                                }
                                else
                                {
                                    break;
                                }
                            }
                            if (mu.NewRoles[posAdmin])
                            {
                                isAdminChecked = true;
                            }

                            // Add this mu to the list
                            muList.Add(mu);
                        }
                        if (!isAdminChecked)
                        {
                            // Need to keep an Admin!
                            return RedirectToAction("NeedAdmin");
                        }

                        // Now, we are ready to update all the roles for each user
                        UserRolesHelper helper = new UserRolesHelper();
                        foreach (var mu in muList)
                        {
                            // See if any role changed...
                            for (int i = 0; i < nRoles; i++)
                            {
                                if (mu.NewRoles[i] != mu.OrigRoles[i])
                                {
                                    // Role changed...
                                    if (mu.NewRoles[i])
                                    {
                                        helper.AddUserToRole(mu.Id, roles[i]);
                                    }
                                    else
                                    {
                                        helper.RemoveUserFromRole(mu.Id, roles[i]);
                                    }
                                }
                            }
                        }

                        // All went according to plan, so return to Main Menu!
                        return RedirectToAction("Index");
                    }
                }
            }
            return RedirectToAction("UsersPostError");
        }

        // Home/NeedAdmin
        public ActionResult NeedAdmin()
        {
            ViewBag.Message = "Always need at least one user with Admin role";
            // Do this in every GET action...
            ViewBag.UserModel = ProjectsHelper.LoadUserModel();
            return View();
        }

        // Home/NeedAdmin
        public ActionResult UsersPostError()
        {
            ViewBag.Message = "An error occurred during POST -- no changes mad.";
            // Do this in every GET action...
            ViewBag.UserModel = ProjectsHelper.LoadUserModel();
            return View();
        }
    }
}
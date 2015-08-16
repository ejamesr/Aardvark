using Aardvark.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Aardvark.Helpers
{
    // Avoid errors in typing the role names -- use this class
    public static class R
    {
        // This must be manually adjusted, but I didn't think Roles would be changing that often.
        // And if so, it's still easy, just make sure that _list lists them in the right order.
        // And this avoids the performance issues using .ToString() with reflection in an enum.

        // These are all the available roles
        private static string[] roles = {"Admin", "Guest", "ProjectManager", "Developer", "Submitter", "NewUser"};

        // Return all...
        public static ICollection<string> List() { return roles;}
        public static string[] ToList() { return roles; }

        // And here's how to retrieve them one at a time... some shortcuts (PM, Dev) also allowed
        public static string Admin
        {
            get { return roles[0]; }
        }
        public static string Guest
        {
            get { return roles[1]; }
        }
        public static string Demo
        {
            get { return roles[1]; }   // Same as Guest
        }
        public static string ProjectManager
        {
            get { return roles[2]; }
        }
        public static string PM
        {
            get { return roles[2]; }
        }
        public static string Developer
        {
            get { return roles[3]; }
        }
        public static string Dev
        {
            get { return roles[3]; }
        }
        public static string Submitter
        {
            get { return roles[4]; }
        }
        public static string NewUser
        {
            get { return roles[5]; }
        }
    }

    // Avoid errors in typing the SkillRequired names -- use this class
    public static class Skills
    {
        // These are all the available skills
        private static string[] skills = { "Junior", "Mid-Level", "Senior" };

        // Return all...
        public static ICollection<string> List() { return skills; }
        public static string[] ToList() { return skills; }

        // And here's how to retrieve them one at a time...
        public static string Junior
        {
            get { return skills[0]; }
        }
        public static string Mid
        {
            get { return skills[1]; }
        }
        public static string MidLevel
        {
            get { return skills[1]; }   // Same as Mid
        }
        public static string Senior
        {
            get { return skills[2]; }
        }
    }

    public class UserRolesHelper
    {
        private UserManager<ApplicationUser> manager =
            new UserManager<ApplicationUser>(
                new UserStore<ApplicationUser>(
                    new ApplicationDbContext()));

        public string GetCurrentUserId()
        {
            return HttpContext.Current.User.Identity.GetUserId();
        }

        public ApplicationUser GetCurrentUser()
        {
            var db = new ApplicationDbContext();
            return db.Users.Find(HttpContext.Current.User.Identity.GetUserId());
        }

        public bool IsUserInRole(string userId, string roleName)
        {
            return manager.IsInRole(userId, roleName);
        }

        public IList<string> ListUserRoles(string userId)
        {
            UserManager<ApplicationUser> manager2 =
            new UserManager<ApplicationUser>(
                new UserStore<ApplicationUser>(
                    new ApplicationDbContext()));
            IList<string> roles = new List<string>();
            try
            {
                roles = manager2.GetRoles(userId);
            }
            catch
            {
                // No role specified, so make the person R.NewUser
                roles.Add(R.NewUser);
            }
            return roles;
        }

        public string GetHighestRole(string userId)
        {
            if (manager.IsInRole(userId, R.Admin))
                return R.Admin;
            if (manager.IsInRole(userId, R.Guest))
                return R.Guest;
            if (manager.IsInRole(userId, R.PM))
                return R.PM;
            if (manager.IsInRole(userId, R.Developer))
                return R.Developer;
            if (manager.IsInRole(userId, R.Submitter))
                return R.Submitter;

            // Not in any role...
            return "";
        }

        public string GetRoleId(string roleName)
        {
            var db = new ApplicationDbContext();
            return db.Roles.FirstOrDefault(r => r.Name == roleName).Id;
        }

        public string GetDisplayName(string userId)
        {
            if (userId == null || userId == "")
                return "(no user)";
            var db = new ApplicationDbContext();
            var user = db.Users.Find(userId);
            return GetDisplayName(user);
        }
        public string GetDisplayName(ApplicationUser user)
        {
            if (user == null)
                return "(no user)";
            return user.DisplayName != "" ? user.DisplayName : user.UserName;

            //string name = "";
            //if (user.FirstName != "")
            //{
            //    name += user.FirstName + "-";
            //}
            //if (user.LastName != "")
            //{
            //    name += user.LastName + "-";
            //}
            //if (user.DisplayName != "")
            //{
            //    name += user.DisplayName + "-";
            //}
            //name += user.UserName;
            //return name;
        }

        public string GetProjectManagerDisplayName(int projectId)
        {
            return GetDisplayName(ProjectsHelper.GetProjectManager(projectId));
        }

        //public string GetDisplayName(ManageUsersData user)
        //{
        //    if (user == null)
        //        return "(no user)";
        //    string name = "";
        //    if (user.First != "")
        //    {
        //        name += user.First + "-";
        //    }
        //    if (user.Last != "")
        //    {
        //        name += user.Last + "-";
        //    }
        //    if (user.DisplayName != "")
        //    {
        //        name += user.DisplayName + "-";
        //    }
        //    name += user.UserName;
        //    return name;
        //}

        public bool AddUserToRole(string userId, string roleName)
        {
            var result = manager.AddToRole(userId, roleName);
            return result.Succeeded;
        }

        public bool RemoveUserFromRole(string userId, string roleName)
        {
            var result = manager.RemoveFromRole(userId, roleName);
            return result.Succeeded;
        }

        public IList<ApplicationUser> UsersInRole(string roleName)
        {
            var db = new ApplicationDbContext();
            string roleId = db.Roles.SingleOrDefault(n => n.Name == roleName).Id; 
            return db.Users.Where(u => u.Roles.Any(r => r.RoleId == roleId)).ToList();
            //return db.Users.Where(u => IsUserInRole(u.Id, roleName)).ToList();
        }

        public IList<ApplicationUser> UsersNotInRole(string roleName)
        {
            var db = new ApplicationDbContext();
            string roleId = db.Roles.SingleOrDefault(n => n.Name == roleName).Id;
            return db.Users.Where(u => !u.Roles.Any(r => r.RoleId == roleId)).ToList();
        }

        // Zap all cookies, see if that clears things up...
        public void ZapCookies()
        {
            string[] myCookies = HttpContext.Current.Request.Cookies.AllKeys;
            foreach (string cookie in myCookies)
            {
                HttpContext.Current.Response.Cookies[cookie].Expires = DateTime.Now.AddDays(-1);
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Aardvark.Models
{
    // Avoid errors in typing the role names -- use this class
    public static class R
    {
        // This must be manually adjusted, but I didn't think Roles would be changing that often.
        // And if so, it's still easy, just make sure that _list lists them in the right order.
        // And this avoids the performance issues using .ToString() with reflection in an enum.

        // These are all the available roles
        private const string _admin = "Admin";
        private const string _guest = "Guest";
        private const string _project_manager = "Project Manager";
        private const string _developer = "Developer";
        private const string _submitter = "Submitter";

        // Get a list of all roles...
        private static readonly ICollection<string> _list = new[] {
        _admin, _guest, _project_manager, _developer, _submitter };

        // Return all...
        public static ICollection<string> List() { return _list;}

        // And here's how to retrieve them one at a time... some shortcuts (PM, Dev) also allowed
        public static string Admin
        {
            get { return _admin; }
        }
        public static string Guest
        {
            get { return _guest; }
        }
        public static string Demo
        {
            get { return _guest; }
        }
        public static string ProjectManager
        {
            get { return _project_manager; }
        }
        public static string PM
        {
            get { return _project_manager; }
        }
        public static string Developer
        {
            get { return _developer; }
        }
        public static string Dev
        {
            get { return _developer; }
        }
        public static string Submitter
        {
            get { return _submitter; }
        }
    }

    public class UserRolesHelper
    {
        private UserManager<ApplicationUser> manager =
            new UserManager<ApplicationUser>(
                new UserStore<ApplicationUser>(
                    new ApplicationDbContext()));

        public bool IsUserInRole(string userId, string roleName)
        {
            return manager.IsInRole(userId, roleName);
        }

        public IList<string> ListUserRoles(string userId)
        {
            return manager.GetRoles(userId);
        }

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
            var resultList = new List<ApplicationUser>();

            // Not sure of difference between x and y below
            var x = db.Users.Where(u => IsUserInRole(u.Id, roleName));
            var y = manager.Users.Where(u => IsUserInRole(u.Id, roleName));
            
            return (IList<ApplicationUser>) x;
        }

        public IList<ApplicationUser> UsersNotInRole(string roleName)
        {
            var db = new ApplicationDbContext();
            var resultList = new List<ApplicationUser>();

            // Not sure of difference between x and y below
            var x = db.Users.Where(u => IsUserInRole(u.Id, roleName) == false);
            var y = manager.Users.Where(u => IsUserInRole(u.Id, roleName) == false);

            return (IList<ApplicationUser>)x;

        }

    }
}
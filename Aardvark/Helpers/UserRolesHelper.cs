﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Web;
using System.Web.Mvc;
using Aardvark.Models;

namespace Aardvark.Helpers
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
            UserManager<ApplicationUser> manager2 =
            new UserManager<ApplicationUser>(
                new UserStore<ApplicationUser>(
                    new ApplicationDbContext()));
            var roles = manager2.GetRoles(userId);
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


        // Two different versions, depending on user model
        public string GetDisplayName(ApplicationUser user)
        {
            string name = "";
            if (user.FirstName != "")
            {
                name += user.FirstName + "-";
            }
            if (user.LastName != "")
            {
                name += user.LastName + "-";
            }
            if (user.DisplayName != "")
            {
                name += user.DisplayName + "-";
            }
            name += user.UserName;
            return name;
        }

        public string GetDisplayName(ManageUsersData user)
        {
            string name = "";
            if (user.First != "")
            {
                name += user.First + "-";
            }
            if (user.Last != "")
            {
                name += user.Last + "-";
            }
            if (user.DisplayName != "")
            {
                name += user.DisplayName + "-";
            }
            name += user.UserName;
            return name;
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
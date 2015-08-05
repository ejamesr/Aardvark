using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Aardvark.Helpers;

namespace Aardvark.Models
{
    public class UserRolesViewModel
    {
        public UserRolesHelper Helper = new UserRolesHelper();
        public IList<string> Roles;
        public string UserId { get; set; }

        public UserRolesViewModel(string userId)
        {
            // List all roles for current user
            UserId = userId;
            Roles = userId == null ? null : Helper.ListUserRoles(userId);
        }
    }
}
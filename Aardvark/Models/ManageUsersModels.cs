using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Aardvark.Models
{
    public class ManageUsersData
    {
        // Use this to keep track of users and roles, structure so it's easy to use table
        
        public string Id;          // User id
        public string UserName;
        public string Email;
        public string DisplayName;
        public string First;
        public string Last;
        public bool[] OrigRoles;
        public bool[] NewRoles;

    }

       public class ManageUsersModel
    {
        public List<ManageUsersData> UserInfo = null;
        public List<string> UserRoles = null;

        public ManageUsersModel(List<ManageUsersData> data, List<string> roles)
        {
            UserInfo = data;
            UserRoles = roles;
        }
    }
}
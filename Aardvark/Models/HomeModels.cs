using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Aardvark.Models
{
    public class UserRolesViewModel
    {
        public ICollection<Microsoft.AspNet.Identity.EntityFramework.IdentityUserRole> Roles;

        public UserRolesViewModel(ICollection<Microsoft.AspNet.Identity.EntityFramework.IdentityUserRole> roles)
        {
            Roles = roles;
        }

    }
}
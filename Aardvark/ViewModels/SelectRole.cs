using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Aardvark.ViewModels
{
    public class SelectRole
    {
        public string Roles { get; set; }
        public SelectRole(string roles)
        {
            Roles = roles;
        }
    }
}
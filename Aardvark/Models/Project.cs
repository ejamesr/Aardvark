using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Aardvark.Models
{
    /* Describes all Projects
     * */
    public class Project : Lookup
    {
        public string Description { get; set; }

        // To track the ProjectManager, search the User table for the Projects
        // and filter the person whose roles include "ProjectManager"

        public virtual ICollection<ApplicationUser> Users { get; set; }

        public Project()
            : base()
        {
            this.Users = new HashSet<ApplicationUser>();
        }
    }
}
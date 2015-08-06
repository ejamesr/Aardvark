using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Aardvark.Models
{
    /* List of all SkillLevels
    * 
    * "Junior", 1
    * "Mid-level", 2
    * "Senior", 3
    * 
    * */
    public class SkillLevel : Lookup
    {
        public SkillLevel()
            : base()
        {
            this.Users = new HashSet<ApplicationUser>();
        }
        public int Weight { get; set; }     // Higher number = higher relative skill

        public virtual ICollection<ApplicationUser> Users { get; set; }
    }
}
using Aardvark.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Aardvark.Models
{
    /* Roles to seed:
     * 
     * "Admin",
     * "Project Manager",
     * "Developer",
     * "Submitter",
     * "Guest/Demo"
     * 
     * */

    public class TicketCreateModel : Ticket
    {
        [NotMapped]
        public SelectList ProjectList { get; set; }
        [NotMapped]
        public SelectList TypeList { get; set; }
        [NotMapped]
        public SelectList PriorityList { get; set; }
        [NotMapped]
        public SelectList StatusList { get; set; }
        [NotMapped]
        public SelectList AssigneesList { get; set; }
        [Display(Name="Skill Level")]
        [NotMapped]
        public SelectList SkillLevelList { get; set; }
        [NotMapped]
        public string HighestUserRole { get; set; }


        // Try NOT using this model, go to ViewBag instead
        public TicketCreateModel()
        {
            ApplicationDbContext db = new ApplicationDbContext();
            UserRolesHelper helper = new UserRolesHelper();
            DueDate = DateTimeOffset.UtcNow.AddDays(1);
            var id = HttpContext.Current.User.Identity.GetUserId();
            var x = helper.IsUserInRole(id, R.Admin);
            HighestUserRole = helper.GetHighestRole(id);
            TypeList = new SelectList(db.TicketTypes, "Id", "Name");
            PriorityList = new SelectList(db.TicketPriorities, "Id", "Name");
            StatusList = new SelectList(db.TicketStatuses, "Id", "Name");
            SkillLevelList = new SelectList(db.SkillLevels, "Id", "Name");
            ProjectList = new SelectList(db.Projects, "Id", "Name");
            HoursToComplete = 1;

            if (HighestUserRole == R.Admin || HighestUserRole == R.Guest || HighestUserRole == R.PM)
            {
                // OK to assign list of users...
                var roleDev = db.Roles.FirstOrDefault(r => r.Name == R.Developer);
                if (roleDev != null)
                {
                    AssigneesList = new SelectList(
                        db.Users
                            .Where(d => d.Roles.FirstOrDefault(r => r.RoleId == roleDev.Id) != null),
                            "Id", "UserName");
                            //.Select(assignee => 
                            //    new SelectListItem 
                            //    { 
                            //        Selected = false,
                            //        Text = assignee.UserName,
                            //        Value = assignee.Id
                            //    }
                            //));
                }
            }
            else AssigneesList = null;
        }
    }


    // This class is inherited by several others, all needing an Id, Name, and ICollection<Ticket> Ticket.
    public class Lookup
    {
        public Lookup()
        {
            this.Tickets = new HashSet<Ticket>();
        }
        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Ticket> Tickets { get; set; }
    }


    /* Valid reasons to resolve a ticket early (each of these results in "Resolved" status):
     *
     * "Rejected"           // Was bogus, didn't make sense, etc.
     * "Conforms to Spec"   // Not a bug, not an enhancement, but is according to specifications
     * 
     * */

    /* List of all Ticket statuses
     * 
     * "New"                    // 0
     * "Ready to Assign"        // 5
     * "Assigned to Developer"  // 10
     * "In Development"         // 15
     * "Ready to Test"          // 20
     * "Assigned to Tester"     // 25
     * "In Testing"             // 30
     * "Ready to Review"        // 35
     * "Resolved"               // 100
     * "Unable to Reproduce"    // 40
     * "Deferred/On Hold",      // 50 - Requires update to Ticket.DueDate to show when to get back to it
     * 
     * */

    [Table("TicketStatuses")]
    public class TicketStatus : Lookup
    {
        public int Step { get; set; }
        public TicketStatus() : base(){}
    }

    /* List of all Ticket types
     * 
     * "Bug",
     * "Enhancement",
     * "Not Sure"
     * 
     * */
    public class TicketType : Lookup
    {
        public TicketType() : base(){}
    }

    /* List of all TicketPriorities
     * 
     * "Show Stopper",
     * "Essential",
     * "Desireable",
     * "Optional"
     * 
     * */
    public class TicketPriority : Lookup
    {
        public int Val { get; set; }
        public TicketPriority() : base() { }
    }

    /* Describes all Projects
     * */
    public class Project : Lookup
    {
        public Project(string PM_Id) : base()
        {
            Init();
            ProjectMgrId = PM_Id;
        }
        public Project() : base()
        {
            Init();
        }
        private void Init()
        {
            this.Users = new HashSet<ApplicationUser>();
        }

        public string Description { get; set; }
        public string ProjectMgrId { get; set; }

        public virtual ApplicationUser ProjectMgr { get; set; }
        public virtual ICollection<ApplicationUser> Users { get; set; }
    }

    /* Describes all ProjectUsers
     * */
    // Let Entity Framework automatically create this... so comment it out
    //public class ProjectUser
    //{
    //    public ProjectUser(int projectId, string userId)
    //    {
    //        ProjectId = projectId;
    //        UserId = userId;
    //    }

    //    public int Id { get; set; }
    //    public int ProjectId { get; set; }
    //    public string UserId { get; set; }

    //    public virtual Project Project { get; set; }
    //    public virtual ApplicationUser User { get; set; }

    //}

    /* List of all SkillLevels
     * 
     * "Junior", 1
     * "Mid-level", 2
     * "Senior", 3
     * 
     * */
    public class SkillLevel : Lookup
    {
        public SkillLevel() : base()
        {
            this.Users = new HashSet<ApplicationUser>();
        }
        public int Weight { get; set; }     // Higher number = higher relative skill

        public virtual ICollection<ApplicationUser> Users { get; set; }
    }

    // List of all notifications sent
    public class TicketNotification
    {
        public TicketNotification(Ticket ticket)
        {
            TicketId = ticket.Id;

            UsedEmail = User.EmailNotification;
            UsedText = User.TextNotification;
        }
        public int Id { get; set; }
        public int TicketId { get; set; }
        public string UserId { get; set; }
        public DateTimeOffset Created { get; set; }
        public bool UsedEmail { get; set; }
        public bool UsedText { get; set; }

        public virtual Ticket Ticket { get; set; }
        public virtual ApplicationUser User { get; set; }
    }

    // History of all changes to comments, status, etc.
    public class TicketHistory
    {
        public TicketHistory()
        {

        }
        public int Id { get; set; }
        public int TicketId { get; set; }       // Which ticket had a change event?
        public string UserId { get; set; } // Who initiated the change?
        public string Property { get; set; }    // Which element was modified?
        public string OldValue { get; set; }    // The original property value
        public string NewValue { get; set; }    // New value for that property
        public DateTimeOffset ChangeDate { get; set; }  // The date/time of the change

        public virtual Ticket Ticket { get; set; }
        public virtual ApplicationUser User { get; set; }
    }

    // Table of comments
    public class TicketComment
    {
        public TicketComment()
        {

        }
        public int Id { get; set; }
        public string Comment { get; set; }
        public DateTimeOffset Created { get; set; }     // Time this was created
        public int TicketId { get; set; }
        public string UserId { get; set; }

        public virtual Ticket Ticket { get; set; }
        public virtual ApplicationUser User { get; set; }
    }

    public class TicketAttachment
    {
        public TicketAttachment()
        {

        }
        public int Id { get; set; }
        public int TicketId { get; set; }
        public string FilePath { get; set; }
        public string Description { get; set; }
        public DateTimeOffset Created { get; set; }
        public string UserId { get; set; }
        public string FileUrl { get; set; }

        public virtual Ticket Ticket { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
}
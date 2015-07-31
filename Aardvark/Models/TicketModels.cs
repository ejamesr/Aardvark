using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

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

    public class Ticket
    {
        public Ticket()
        {
           
        }
        public int Id { get; set; }
        public Project ProjectId { get; set; }
        public TicketType TicketTypeId { get; set; }
        public TicketPriority TicketPriorityId { get; set; }
        public TicketStatus TicketStatusId { get; set; }
        public string OwnerUserId { get; set; }
        public string AssignedToUserId { get; set; }
        public SkillLevel SkillRequiredId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTimeOffset Created { get; set; }
        public Nullable<DateTimeOffset> Updated { get; set; }
        public DateTimeOffset DueDate { get; set; }
        public int HoursToComplete { get; set; }

        public virtual Project Project { get; set; }
        public virtual TicketType TicketType { get; set; }
        public virtual TicketPriority TicketPriority { get; set; }
        public virtual TicketStatus TicketStatus { get; set; }
        public virtual ApplicationUser OwnerUser { get; set; }
        public virtual ApplicationUser AssignedToUser { get; set; }
        public virtual SkillLevel SkillRequired { get; set; }

        // And show any/all tickets related to this one
        public virtual ICollection<TicketAttachment> Attachments { get; set; }
        public virtual ICollection<TicketComment> Comments { get; set; }
        public virtual ICollection<TicketHistory> Histories { get; set; }
        public virtual ICollection<TicketNotification> Notifications { get; set; }
        public virtual ICollection<Ticket> RelatedTickets { get; set; }

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
    public class TicketStatus
    {
        public TicketStatus()
        {

        }
        public int Id { get; set; }
        public string Name { get; set; }    // Name of this status
        public int Step { get; set; }       // Numerical sequence until Reseolved (final step)

        public virtual ICollection<Ticket> Tickets { get; set; }
    }

    /* List of all Ticket types
     * 
     * "Bug",
     * "Enhancement",
     * "Not Sure"
     * 
     * */
    public class TicketType
    {
        public TicketType()
        {

        }
        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Ticket> Tickets { get; set; }
    }

    /* List of all TicketPriorities
     * 
     * "Show Stopper",
     * "Essential",
     * "Desireable",
     * "Optional"
     * 
     * */
    public class TicketPriority
    {
        public TicketPriority()
        {

        }
        public int Id { get; set; }
        public string Name { get; set; }
        public int Val { get; set; }

        public virtual ICollection<Ticket> Tickets { get; set; }
    }

    /* Describes all Projects
     * */
    public class Project
    {
        public Project(string PM_Id)
        {
            ProjectMgrId = PM_Id;
        }
        public Project()
        {
           
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ProjectMgrId { get; set; }

        public virtual ApplicationUser ProjectMgr { get; set; }
        public virtual ICollection<ProjectUser> ProjectUsers { get; set; }
        public virtual ICollection<Ticket> Tickets { get; set; }
    }

    /* Describes all ProjectUsers
     * */
    public class ProjectUser
    {
        public ProjectUser()
        {

        }
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public int UserId { get; set; }

        public virtual Project Project { get; set; }
        public virtual ApplicationUser User { get; set; }

    }

    /* List of all SkillLevels
     * 
     * "Junior", 1
     * "Mid-level", 2
     * "Senior", 3
     * 
     * */
    public class SkillLevel
    {
        public SkillLevel()
        {

        }
        public int Id { get; set; }
        public string Name { get; set; }
        public int Weight { get; set; }     // Higher number = higher relative skill

        public virtual ICollection<Ticket> Tickets { get; set; }
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
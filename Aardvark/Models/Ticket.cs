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
    public class Ticket
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        [Display(Name="Ticket Type")]
        public int TicketTypeId { get; set; }
        [Display(Name="Ticket Priority")]
        public int TicketPriorityId { get; set; }
        [Display(Name="Ticket Status")]
        public int TicketStatusId { get; set; }
        [Display(Name="Skill Required")]
        public int SkillRequiredId { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Description { get; set; }
        public DateTimeOffset Created { get; set; }
        public Nullable<DateTimeOffset> Updated { get; set; }

        // This field keeps track of the most recent activity to occur:
        //   Ticket was created, updated, comment or attachment created.
        public DateTimeOffset MostRecentUpdate { get; set; }

        public DateTimeOffset DueDate { get; set; }
        [Display(Name="Hours to Complete")]
        public int HoursToComplete { get; set; }

        public string OwnerUserId { get; set; }
        public virtual ApplicationUser OwnerUser { get; set; }

        [Display(Name="Assigned to User")]
        public string AssignedToUserId { get; set; }
        public virtual ApplicationUser AssignedToUser { get; set; }

        public virtual Project Project { get; set; }
        public virtual TicketType TicketType { get; set; }
        public virtual TicketPriority TicketPriority { get; set; }
        public virtual TicketStatus TicketStatus { get; set; }
        public virtual SkillLevel SkillRequired { get; set; }

        // And show any/all tickets related to this one
        public virtual ICollection<TicketAttachment> Attachments { get; set; }
        public virtual ICollection<TicketComment> Comments { get; set; }
        public virtual ICollection<TicketHistory> Histories { get; set; }
        public virtual ICollection<TicketNotification> Notifications { get; set; }

        public Ticket()
        {
            AssignedToUserId = null;
            this.Attachments = new HashSet<TicketAttachment>();
            this.Comments = new HashSet<TicketComment>();
            this.Histories = new HashSet<TicketHistory>();
            this.Notifications = new HashSet<TicketNotification>();
        }

        public void SetCreated()
        {
            Created = MostRecentUpdate = DateTimeOffset.UtcNow;
        }
        public void SetUpdated()
        {
            Updated = MostRecentUpdate = DateTimeOffset.UtcNow;
        }
        public void SetMostRecentUpdate()
        {
            MostRecentUpdate = DateTimeOffset.UtcNow;
        }
        public void SetMostRecentUpdate(DateTimeOffset now)
        {
            MostRecentUpdate = now;
        }
    }
}
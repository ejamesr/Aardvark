using Aardvark.Helpers;
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
        public enum Prop:int
        {
            ProjectId = 1,
            TicketTypeId,
            TicketPriorityId,
            TicketStatusId,
            SkillRequiredId,
            Title,
            Description,
            DueDate,
            HoursToComplete,
            AssignedToDevId
        }

        private void CreateHistoryElement(ApplicationDbContext db, DateTimeOffset date,
                Prop prop, Ticket origTicket, string oldVal, string newVal)
        {
            TicketHistory history = new TicketHistory();
            history.TicketId = origTicket.Id;
            history.TypeProperty = prop;
            history.OldValue = oldVal;
            history.NewValue = newVal;
            history.UserId = new UserRolesHelper().GetCurrentUserId();
            history.SetDate(date);
            db.TicketHistories.Add(history);
        }

        private bool CheckProperties(ApplicationDbContext db, DateTimeOffset date, 
                Prop prop, Ticket origTicket, int oldVal, int newVal)
        {
            if (oldVal != newVal)
            {
                CreateHistoryElement(db, date, prop, origTicket, oldVal.ToString(), newVal.ToString());
                return true;
            }
            return false;
        }

        private bool CheckProperties(ApplicationDbContext db, DateTimeOffset date, 
                Prop prop, Ticket origTicket, DateTimeOffset oldVal, DateTimeOffset newVal)
        {
            if (oldVal != newVal)
            {
                CreateHistoryElement(db, date, prop, origTicket, oldVal.ToString(), newVal.ToString());
                return true;
            }
            return false;
        }

        private bool CheckProperties(ApplicationDbContext db, DateTimeOffset date, 
                Prop prop, Ticket origTicket, string oldVal, string newVal){
            if (oldVal != newVal){
                CreateHistoryElement(db, date, prop, origTicket, oldVal, newVal);
                return true;
            }
            return false;
        }
        public DateTimeOffset WasChanged(ApplicationDbContext db, Ticket origTicket){
            // Compare each field of 'this' with 'origTicket'. If any field changed,
            //   create History record and copy the new value to origTicket

            // Something could have changed, so this is the date to store in each record
            DateTimeOffset date = DateTimeOffset.UtcNow;
            bool changed = false;
            if (CheckProperties(db, date, Prop.ProjectId, origTicket, origTicket.ProjectId, this.ProjectId))
            {
                changed = true;
                origTicket.ProjectId = this.ProjectId;
            }
            if (CheckProperties(db, date, Prop.TicketTypeId, origTicket, origTicket.TicketTypeId, this.TicketTypeId))
            {
                changed = true;
                origTicket.TicketTypeId = this.TicketTypeId;
            }
            if (CheckProperties(db, date, Prop.TicketPriorityId, origTicket, origTicket.TicketPriorityId, this.TicketPriorityId))
            {
                changed = true;
                origTicket.TicketPriorityId = this.TicketPriorityId;
            }
            if (CheckProperties(db, date, Prop.TicketStatusId, origTicket, origTicket.TicketStatusId, this.TicketStatusId))
            {
                changed = true;
                origTicket.TicketStatusId = this.TicketStatusId;
            }
            if (CheckProperties(db, date, Prop.SkillRequiredId, origTicket, origTicket.SkillRequiredId, this.SkillRequiredId))
            {
                changed = true;
                origTicket.SkillRequiredId = this.SkillRequiredId;
            }
            if (CheckProperties(db, date, Prop.Title, origTicket, origTicket.Title, this.Title))
            {
                changed = true;
                origTicket.Title = this.Title;
            }
            if (CheckProperties(db, date, Prop.Description, origTicket, origTicket.Description, this.Description))
            {
                changed = true;
                origTicket.Description = this.Description;
            }
            if (CheckProperties(db, date, Prop.DueDate, origTicket, origTicket.DueDate, this.DueDate))
            {
                changed = true;
                origTicket.DueDate = this.DueDate;
            }
            if (CheckProperties(db, date, Prop.HoursToComplete, origTicket, origTicket.HoursToComplete, this.HoursToComplete))
            {
                changed = true;
                origTicket.HoursToComplete = this.HoursToComplete;
            }
            if (CheckProperties(db, date, Prop.AssignedToDevId, origTicket, origTicket.AssignedToDevId, this.AssignedToDevId))
            {
                changed = true;
                origTicket.AssignedToDevId = this.AssignedToDevId;
            }

            // Show whether any changes were made
            return changed ? date : DateTimeOffset.MinValue;
        }
      
        public int Id { get; set; }
        public int ProjectId { get; set;}
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

        [Display(Name="Assigned to Developer")]
        public string AssignedToDevId { get; set; }
        public virtual ApplicationUser AssignedToDev { get; set; }

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
            AssignedToDevId = null;
            this.Attachments = new HashSet<TicketAttachment>();
            this.Comments = new HashSet<TicketComment>();
            this.Histories = new HashSet<TicketHistory>();
            this.Notifications = new HashSet<TicketNotification>();
        }

        public void SetCreated()
        {
            DateTimeOffset now = DateTimeOffset.UtcNow;
            Created = MostRecentUpdate = now;
        }
        public void SetUpdated()
        {
            Updated = MostRecentUpdate = DateTimeOffset.UtcNow;
        }
        public void SetUpdated(DateTimeOffset date)
        {
            Updated = MostRecentUpdate = date;
        }
        public void SetMostRecentUpdate()
        {
            MostRecentUpdate = DateTimeOffset.UtcNow;
        }
        public void SetMostRecentUpdate(DateTimeOffset now)
        {
            MostRecentUpdate = now;
        }
        public void NotifyNewTicket(ApplicationDbContext db)
        {
            TicketNotification.Notify(db, this, this.Created, 
                Aardvark.Models.Notifications.AssignedToTicket);

        }
    }
}
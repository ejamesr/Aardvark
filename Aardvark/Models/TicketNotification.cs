using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Aardvark.Helpers;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aardvark.Models
{
    // List of all notifications sent
    public class TicketNotification
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public string UserId { get; set; }

        /// <summary>
        /// The notification type from the enumeration 'E'
        /// </summary>
        public EType Type { get; set; }
        public DateTimeOffset Created { get; set; }
        public NotificationMethod Method { get; set; }
        public bool HasBeenRead { get; private set; }
        public bool UsedEmail { get; set; }
        public bool UsedText { get; set; }

        public virtual Ticket Ticket { get; set; }
        public virtual ApplicationUser User { get; set; }

        public TicketNotification()
        {

        }
        public TicketNotification(EType type)
        {
            Type = type;
        }

        /// <summary>
        /// The enumeration of TicketNotification types.  The same names are used to help generate
        /// the type name as a string (class Str), the messsage associated with the type (class Msg),
        /// and to return all the names in an array (field List).
        /// </summary>
        public enum EType
        {
            AssignedToTicket = 1,
            RemovedFromTicket,
            TicketModified,
            TicketDeleted,
            CommentCreated,
            CommentModified,
            CommentDeleted,
            AttachmentCreated,
            AttachmentModified,
            AttachmentDeleted,
        }

        [NotMapped]
        private static readonly string[] msgs = {
            "You have been assigned to this ticket",
            "You have been removed from this ticket",
            "Ticket has been modified",
            "Ticket has beem deleted",
            "Comment has been added to this ticket",
            "Comment for this ticket has been modified",
            "Comment for this ticket has been deleted",
            "Attachment has been added to this ticket",
            "Attachment for this ticket has been modified",
            "Attachment for this ticket has been deleted"
        };
        //[NotMapped]
        //private static int[] steps = { 10, 20, 30, 40, 50, 60, 70, 80, 90, 100 };
        [NotMapped]
        private static string[] vals = Enum.GetNames(typeof(EType));


        [NotMapped]
        public string Msg { get { return msgs[(int)Type-1];}}
        [NotMapped]
        public string Str { get { return vals[(int)Type-1]; } }
        //[NotMapped]
        //public int Step { get { return steps[(int)Type]; } }


        // Static methods to get list, msg, str, step
        public static string[] GetList()
        {
            return vals;
        }
        public static string GetMsg(EType type)
        {
            return msgs[(int)type-1];
        }
        public static string GetStr(EType type)
        {
            return vals[(int)type - 1];
        }
        //public static int GetStep(EType type){
        //    return steps[(int)type];
        //}


        public enum NotificationMethod
        {
            ByDefault = 1,          // Default is to simply list the pending notifications
            ByEmail = 2,
            ByText = 3,
        }

        public override string ToString()
        {
            string str =  "Ticket # " + TicketId
                + " (" + Ticket.Title + "): " + Str;
            return str;
        }

        public string ToDescription()
        {
            string desc = "Tk#" + TicketId + ":" + Ticket.Title + "-" + Msg;
            return desc;
        }

        // Create a notification entry...
        public static void Notify(ApplicationDbContext db, int ticketId, DateTimeOffset date, EType type)
        {
            Ticket ticket = db.Tickets.Find(ticketId);
            Notify(db, ticket, date, type);
        }

        public static void Notify(ApplicationDbContext db, Ticket ticket, DateTimeOffset date, EType type)
        {
            // Create new object, init fields and save
            TicketNotification tn = new TicketNotification();
            // Don't need to create notification if the Developer is same as the User who generated the event
            if (ticket.AssignedToDevId != null)
            {
                if (ticket.AssignedToDevId != ticket.OwnerUserId)
                {
                    tn.TicketId = ticket.Id;
                    tn.UserId = ticket.AssignedToDevId;
                    tn.Type = type;
                    tn.Created = DateTimeOffset.UtcNow;
                    db.TicketNotifications.Add(tn);
                    db.SaveChanges();
                }
            }
        }
        //public static void NotifyAttachmentCreated(ApplicationDbContext db, int ticketId, DateTimeOffset date)
        //{
        //    TicketNotification.Notify(db, ticketId, date, TicketNotification.EType.AttachmentCreated);
        //}
        //public static void NotifyAttachmentEdited(ApplicationDbContext db, int ticketId, string userId, DateTimeOffset date)
        //{
        //    TicketNotification.Notify(db, ticketId, date, TicketNotification.EType.AttachmentModified);
        //}
        //public static void NotifyAttachmentDeleted(ApplicationDbContext db, int ticketId, string userId, DateTimeOffset date)
        //{
        //    TicketNotification.Notify(db, ticketId, date, TicketNotification.EType.AttachmentDeleted);
        //}
        //public static void NotifyCommentCreated(ApplicationDbContext db, int ticketId, string userId, DateTimeOffset date)
        //{
        //    TicketNotification.Notify(db, ticketId, date, TicketNotification.EType.CommentCreated);
        //}
        //public static void NotifyCommentEdited(ApplicationDbContext db, int ticketId, string userId, DateTimeOffset date)
        //{
        //    TicketNotification.Notify(db, ticketId, date, TicketNotification.EType.CommentModified);
        //}
        //public static void NotifyCommentDeleted(ApplicationDbContext db, int ticketId, string userId, DateTimeOffset date)
        //{
        //    TicketNotification.Notify(db, ticketId, date, TicketNotification.EType.CommentDeleted);
        //}
        //public static void NotifyAssignedToTicket(ApplicationDbContext db, int ticketId, string userId, DateTimeOffset date)
        //{
        //    TicketNotification.Notify(db, ticketId, date, TicketNotification.EType.AssignedToTicket);
        //}
        //public static void NotifyTicketModified(ApplicationDbContext db, int ticketId, string userId, DateTimeOffset date)
        //{
        //    TicketNotification tn = new TicketNotification();
        //    TicketNotification.Notify(db, ticketId, date, TicketNotification.EType.TicketModified);
        //}
        public void ThisHasBeenRead()
        {
            // This can only be modified by the person assigned to it, by clicking a button
            if (new UserRolesHelper().GetCurrentUserId() == this.UserId)
                HasBeenRead = true; 
        }
    }
}
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Aardvark.Helpers;

namespace Aardvark.Models
{
    public abstract class Enumeration
    {
        // This gives me an enum class like in Java
        private readonly int _value;
        private readonly string _displayName;
        private readonly string _msg;

        protected Enumeration() { }
        protected Enumeration(Notifications type, string msg)
        {
            _value = (int)type;
            _displayName = type.ToString();
            _msg = msg;
        }
        public int Value
        {
            get { return _value; }
        }
        public string DisplayName
        {
            get { return _displayName; }
        }
        public string Msg
        {
            get { return _msg; }
        }
        public override string ToString()
        {
            return DisplayName;
        }
    }

    public enum Notifications : int
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
        AttachmentDeleted
    }

    // List of all notifications sent
    public class TN
    {
        public enum NotificationMethod
        {
            ByDefault = 1,          // Default is to simply list the pending notifications
            ByEmail = 2,
            ByText = 3,
        }

        public class NotificationType : Enumeration
        {
            public NotificationType() { }
            public NotificationType(Notifications type, string msg) : base(type, msg)
            {

            }
        }

        public static List<NotificationType> NotificationTypes = new List<NotificationType> {
            // Use a dummy entry so first 'real' type is item 1
            new NotificationType(Notifications.AssignedToTicket, "Dummy"),
            // A Developer should be notified of these events:
            new NotificationType(Notifications.AssignedToTicket, "You have been assigned to this ticket"),
            new NotificationType(Notifications.RemovedFromTicket, "You have been removed from this ticket"),
        
            // Notifications sent for these only when initiated by another person
            new NotificationType(Notifications.TicketModified, "Ticket has been modified"),
            new NotificationType(Notifications.TicketDeleted, "Ticket has beem deleted"),
            new NotificationType(Notifications.CommentCreated, "Comment has been added to this ticket"),
            new NotificationType(Notifications.CommentModified, "Comment for this ticket has been modified"),
            new NotificationType(Notifications.CommentDeleted, "Comment for this ticket has been deleted"),
            new NotificationType(Notifications.AttachmentCreated, "Attachment has been added to this ticket"),
            new NotificationType(Notifications.AttachmentModified, "Attachment for this ticket has been modified"),
            new NotificationType(Notifications.AttachmentDeleted, "Attachment for this ticket has been deleted")
        };

        public int Id { get; set; }
        public int TicketId { get; set; }
        public string UserId { get; set; }
        
        public Notifications Type { get; set; }
        public DateTimeOffset Created { get; set; }
        public NotificationMethod Method { get; set; }
        public bool HasBeenRead { get; private set; }
        public bool UsedEmail { get; set; }
        public bool UsedText { get; set; }
        public virtual Ticket Ticket { get; set; }
        public virtual ApplicationUser User { get; set; }

        public override string ToString()
        {
            return "Ticket # " + TicketId
                + " (" + Ticket.Title + "): " + NotificationTypes[(int)Type].Msg;
        }

        public string ToDescription()
        {
            string desc = NotificationTypes[(int)Type].DisplayName;
            desc = "Tk#" + TicketId + ":" + Ticket.Title + "-" + desc;
            return desc;
        }

        // Create a notification entry...
        public static void Notify(ApplicationDbContext db, int ticketId, DateTimeOffset date, Notifications type)
        {
            Ticket ticket = db.Tickets.Find(ticketId);
            Notify(db, ticket, date, type);
        }

        public static void Notify(ApplicationDbContext db, Ticket ticket, DateTimeOffset date, Notifications type)
        {
            // Create new object, init fields and save
            TN tn = new TN();
            // Don't need to create notification if the Developer is same as the User who generated the event
            if (ticket.AssignedToDevId != ticket.OwnerUserId)
            {
                tn.TicketId = ticket.Id;
                tn.UserId = ticket.OwnerUserId;
                tn.Type = type;
                tn.Created = DateTimeOffset.UtcNow;
                db.TicketNotifications.Add(tn);
                db.SaveChanges();
            }
        }
        //public static void NotifyAttachmentCreated(ApplicationDbContext db, int ticketId, string userId, DateTimeOffset date)
        //{
        //    TicketNotification tn = new TicketNotification();
        //    tn.Notify(db, ticketId, userId, date, Notifications.AttachmentAdded);
        //}
        //public static void NotifyAttachmentEdited(ApplicationDbContext db, int ticketId, string userId, DateTimeOffset date)
        //{
        //    TicketNotification tn = new TicketNotification();
        //    tn.Notify(db, ticketId, userId, date, Notifications.AttachmentModified);
        //}
        //public static void NotifyAttachmentDeleted(ApplicationDbContext db, int ticketId, string userId, DateTimeOffset date)
        //{
        //    TicketNotification tn = new TicketNotification();
        //    tn.Notify(db, ticketId, userId, date, Notifications.AttachmentDeleted);
        //}
        //public static void NotifyCommentCreated(ApplicationDbContext db, int ticketId, string userId, DateTimeOffset date)
        //{
        //    TicketNotification tn = new TicketNotification();
        //    tn.Notify(db, ticketId, userId, date, Notifications.CommentAdded);
        //}
        //public static void NotifyCommentEdited(ApplicationDbContext db, int ticketId, string userId, DateTimeOffset date)
        //{
        //    TicketNotification tn = new TicketNotification();
        //    tn.Notify(db, ticketId, userId, date, Notifications.CommentAdded);
        //}
        //public static void NotifyCommentDeleted(ApplicationDbContext db, int ticketId, string userId, DateTimeOffset date)
        //{
        //    TicketNotification tn = new TicketNotification();
        //    tn.Notify(db, ticketId, userId, date, Notifications.CommentAdded);
        //}
        //public static void NotifyAssignedToTicket(ApplicationDbContext db, int ticketId, string userId, DateTimeOffset date)
        //{
        //    TicketNotification tn = new TicketNotification();
        //    tn.Notify(db, ticketId, userId, date, Notifications.AssignedToTicket);
        //}
        //public static void NotifyTicketModified(ApplicationDbContext db, int ticketId, string userId, DateTimeOffset date)
        //{
        //    TicketNotification tn = new TicketNotification();
        //    tn.Notify(db, ticketId, userId, date, Notifications.TicketModified);
        //}
        public void ThisHasBeenRead()
        {
            // This can only be modified by the person assigned to it, by clicking a button
            if (new UserRolesHelper().GetCurrentUserId() == this.UserId)
                HasBeenRead = true; 
        }
    }
}
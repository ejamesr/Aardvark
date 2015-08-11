using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Aardvark.Models
{
    // List of all notifications sent
    public class TicketNotification
    {
        public enum Types
        {
            TicketChanged = 1,
            CommentAdded,
            AttachmentAdded
        }
        public TicketNotification(Ticket ticket)
        {
            TicketId = ticket.Id;

            UsedEmail = User.EmailNotification;
            UsedText = User.TextNotification;
        }
        public int Id { get; set; }
        public int TicketId { get; set; }
        public string UserId { get; set; }
        public int Type { get; set; }
        public DateTimeOffset Created { get; set; }
        public bool UsedEmail { get; set; }
        public bool UsedText { get; set; }

        public virtual Ticket Ticket { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
}
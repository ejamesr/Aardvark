using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Aardvark.Models
{
    // Table of comments
    public class TicketComment
    {
        public TicketComment()
        {

        }
        public int Id { get; set; }
        public string Body { get; set; }
        public DateTimeOffset Created { get; set; }     // Time this was created
        public Nullable<DateTimeOffset> Updated { get; set; }
        public int TicketId { get; set; }
        public string UserId { get; set; }
        public Nullable<int> ParentCommentId { get; set; }
        public Nullable<int> Level { get; set; } // the nesting depth
        public bool Deleted { get; set; }

        public virtual Ticket Ticket { get; set; }
        public virtual ApplicationUser User { get; set; }
        public virtual TicketComment ParentComment { get; set; }
        public virtual ICollection<TicketComment> TicketComments { get; set; }

        public void SetCreated()
        {
            Created = DateTimeOffset.UtcNow;
            var db = new ApplicationDbContext();
            var ticket = db.Tickets.Find(TicketId);
            ticket.MostRecentUpdate = Created;
            db.Entry(ticket).State = EntityState.Modified;
        }
        public void SetUpdated()
        {
            DateTimeOffset dt = DateTimeOffset.UtcNow;
            Updated = dt;
            var db = new ApplicationDbContext();
            var ticket = db.Tickets.Find(TicketId);
            db.Tickets.Find(TicketId);
            ticket.MostRecentUpdate = dt;
            db.Entry(ticket).State = EntityState.Modified;
        }
    }
}

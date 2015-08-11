using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Aardvark.Models
{
    // History of all changes to comments, status, etc.
    public class TicketHistory
    {
        public TicketHistory()
        {

        }
        public int Id { get; set; }
        public int TicketId { get; set; }       // Which ticket had a change event?
        public string UserId { get; set; }      // Who initiated the change?
        public Ticket.Prop TypeProperty { get; set; }   // Which element was modified?
        public string OldValue { get; set; }    // The original property value
        public string NewValue { get; set; }    // New value for that property
        public DateTimeOffset ChangeDate { get; set; }  // The date/time of the change

        public virtual Ticket Ticket { get; set; }
        public virtual ApplicationUser User { get; set; }

        public void SetDate(DateTimeOffset date)
        {
            ChangeDate = date;
        }
        public void SetDate()
        {
            ChangeDate = DateTimeOffset.UtcNow;
        }
    }

}
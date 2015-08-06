using System;
using System.Collections.Generic;
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
        public string Comment { get; set; }
        public DateTimeOffset Created { get; set; }     // Time this was created
        public int TicketId { get; set; }
        public string UserId { get; set; }

        public virtual Ticket Ticket { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
}
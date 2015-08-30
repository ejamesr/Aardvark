using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Aardvark.Models
{
    public class TicketAttachment
    {
        public TicketAttachment()
        {

        }
        public int Id { get; set; }
        public int TicketId { get; set; }
        public string FilePath { get; set; }
        [Required]
        public string Description { get; set; }
        public string ContentType { get; set; }
        public int ContentLength { get; set; }
        public DateTimeOffset Created { get; set; }
        public string UserId { get; set; }
        public string FileUrl { get; set; }

        public virtual Ticket Ticket { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
}
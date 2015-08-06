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
    // This class is inherited by several others, all needing an Id, Name, and ICollection<Ticket> Ticket.
    public class Lookup
    {
        public Lookup()
        {
            this.Tickets = new HashSet<Ticket>();
        }
        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Ticket> Tickets { get; set; }
    }
}
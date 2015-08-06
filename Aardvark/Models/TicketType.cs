using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Aardvark.Models
{
    /* List of all Ticket types
     * 
     * "Bug",
     * "Enhancement",
     * "Not Sure"
     * 
     * */
    public class TicketType : Lookup
    {
        public TicketType() : base() { }
    }
}
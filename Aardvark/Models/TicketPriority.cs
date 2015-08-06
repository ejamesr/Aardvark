using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Aardvark.Models
{
    /* List of all TicketPriorities
      * 
      * "Show Stopper",
      * "Essential",
      * "Desireable",
      * "Optional"
      * 
      * */
    public class TicketPriority : Lookup
    {
        public int Val { get; set; }
        public TicketPriority() : base() { }
    }

}
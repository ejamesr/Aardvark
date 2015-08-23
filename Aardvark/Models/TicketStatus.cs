using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Aardvark.Models
{

    /* Valid reasons to resolve a ticket early (each of these results in "Resolved" status):
     *
     * "Rejected"           // Was bogus, didn't make sense, etc.
     * "Conforms to Spec"   // Not a bug, not an enhancement, but is according to specifications
     * 
     * */

    /* List of all Ticket statuses
     * 
     * "New"                    // 0
     * "Ready to Assign"        // 5
     * "Assigned to Developer"  // 10
     * "In Development"         // 15
     * "Ready to Test"          // 20
     * "Assigned to Tester"     // 25
     * "In Testing"             // 30
     * "Ready to Review"        // 35
     * "Resolved"               // 100
     * "Unable to Reproduce"    // 40
     * "Deferred",              // 50 - Requires update to Ticket.DueDate to show when to get back to it
     * 
     * */

    [Table("TicketStatuses")]
    public class TicketStatus : Lookup
    {
        public int Step { get; set; }
        public TicketStatus() : base() { }
    }

}
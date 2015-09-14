using Aardvark.Helpers;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web;

namespace Aardvark.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DisplayName { get; set; }
        public bool RequireNewLogin { get; set; }
        public string TextMsgNumber { get; set; }       // Number to use to send message
        public Nullable<int> SkillLevelId { get; set; }
        public bool EmailNotification { get; set; }
        public bool TextNotification { get; set; }
        /// <summary>
        /// This is the active role being used by current User; helpful when User has multiple roles
        /// </summary>
        public string ActiveRole { get; set; }

        public virtual SkillLevel SkillLevel { get; set; }
        [InverseProperty("AssignedToDev")]
        public virtual ICollection<Ticket> TicketsAssigned { get; set; }
        [InverseProperty("OwnerUser")]
        public virtual ICollection<Ticket> TicketsOwned { get; set; }
        public virtual ICollection<Project> Projects { get; set; }

        public ApplicationUser()
        {
            this.Projects = new HashSet<Project>();
            this.TicketsOwned = new HashSet<Ticket>();
            this.TicketsAssigned = new HashSet<Ticket>();
            SkillLevelId = null;
            TextMsgNumber = "";
            EmailNotification = true;           // Default notification method
            TextNotification = false;
        }
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
            
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        // The db model will be DefaultGuestConnection if user is in role Guest, else will be DefaultConnection
        //public ApplicationDbContext()
        //    : base(
        //    // If User is null, use default connection
        //    (HttpContext.Current.User == null ? "DefaultConnection" 
        //        // Otherwise if not null, see if in Guest role...
        //        : (HttpContext.Current.User.IsInRole(R.Guest) ? "DefaultGuestConnection" : "DefaultConnection")), throwIfV1Schema: false)
        //{
        //}
        // Copy/Paste one of these into base below...
        //
        // DefaultConnection    - local copy
        // aardvark-ejr         - on website
        //
#if false
        // Set to true to use local DefaultConnection
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }
#else
        // Or set to false to use website aardvark-ejr
        public ApplicationDbContext()
            : base("aardvark-ejr", throwIfV1Schema: false)
        {
        }
#endif
        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<TicketStatus> TicketStatuses { get; set; }
        public DbSet<TicketType> TicketTypes { get; set; }
        public DbSet<TicketPriority> TicketPriorities { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<SkillLevel> SkillLevels { get; set; }
        public DbSet<TicketNotification> TicketNotifications { get; set; }
        public DbSet<TicketHistory> TicketHistories { get; set; }
        public DbSet<TicketComment> TicketComments { get; set; }
        public DbSet<TicketAttachment> TicketAttachments { get; set; }
        public DbSet<Log> Logs { get; set; }
    }

    // Create context with Eager loading as default
    public class DbFast : ApplicationDbContext
    {
        public DbFast()
        {
            this.Configuration.LazyLoadingEnabled = false;
        }
    }
}
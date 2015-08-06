﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

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

        public virtual SkillLevel SkillLevel { get; set; }
        public virtual ICollection<Ticket> TicketsAssigned { get; set; }
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
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

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
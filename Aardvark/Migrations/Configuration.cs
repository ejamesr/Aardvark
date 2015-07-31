using Aardvark.Models;
namespace Aardvark.Migrations
{
    using Microsoft.AspNet.Identity.EntityFramework;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using Microsoft.AspNet.Identity;

    internal sealed class Configuration : DbMigrationsConfiguration<Aardvark.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        // Class to help make sure key people are seeded into the User database...
        class NewPerson
        {
            public string First {get;set;}
            public string Last { get; set; }
            public string Display { get; set; }
            public string Email { get; set; }
            public string Role { get; set; }
            public string Password { get; set; }
            public NewPerson(string first, string last, string display, string email, string role, string password)
            {
                First = first;
                Last = last;
                Display = display;
                Email = email;
                Role = role;
                Password = password;
            }
        }

        // Used to establish increments...
        class Stepper
        {
            public int Step { get; set; }

            private int _val;
            public int Val {
                get
                {
                    int x = _val;
                    _val += Step;
                    return x;
                }
                set { _val = value; } }

            public Stepper(int val, int step){
                Step = step;
                _val = val;
            }
        }

        protected override void Seed(Aardvark.Models.ApplicationDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //

            //
            // Comment out these two lines to stop debugger
            //
            //if (System.Diagnostics.Debugger.IsAttached == false)
            //    System.Diagnostics.Debugger.Launch();
            //

            Stepper Step = new Stepper(20, 10);
            context.SkillLevels.AddOrUpdate(
                skill => new { skill.Name, skill.Weight },
                new SkillLevel { Name = "Junior", Weight = Step.Val },
                new SkillLevel { Name = "Mid-Level", Weight = Step.Val },
                new SkillLevel { Name = "Senior", Weight = Step.Val }
                );

            Step = new Stepper(0, 10);
            context.TicketStatuses.AddOrUpdate(
                status => new { status.Name, status.Step },
                new TicketStatus { Name = "New", Step = Step.Val },
                new TicketStatus { Name = "Unable to Reproduce", Step = Step.Val },
                new TicketStatus { Name = "Deferred/On Hold", Step = Step.Val },
                new TicketStatus { Name = "Ready to Assign", Step = Step.Val },
                new TicketStatus { Name = "Assigned to Developer", Step = Step.Val },
                new TicketStatus { Name = "In Development", Step = Step.Val },
                new TicketStatus { Name = "Ready to Test", Step = Step.Val },
                new TicketStatus { Name = "Assigned to Tester", Step = Step.Val },
                new TicketStatus { Name = "In Testing", Step = Step.Val },
                new TicketStatus { Name = "Ready to Review", Step = Step.Val },
                new TicketStatus { Name = "Resolved", Step = Step.Val }
                );

            Step = new Stepper(0, 3);
            context.TicketPriorities.AddOrUpdate(
                priority => priority.Name,
                new TicketPriority { Name = "Optional", Val = Step.Val },
                new TicketPriority { Name = "Desirable", Val = Step.Val },
                new TicketPriority { Name = "Essential", Val = Step.Val },
                new TicketPriority { Name = "Showstopper", Val = Step.Val }
                );
            context.TicketTypes.AddOrUpdate(
                t => t.Name,
                new TicketType { Name = "Bug" },
                new TicketType { Name = "Enhancement" },
                new TicketType { Name = "Unknown" }
                );

            SeedRoles(context,
                new string[] {      // List in hierarchical order - higher has more power
                        "Guest/Demo",
                        "Admin",
                        "Project Manger",
                        "Developer",
                        "Submitter"});

            // Seed these users...
            NewPerson[] users = {
                new NewPerson("Eric-Dev", "Ruff", "Eric-Dev", "ejamesr@yahoo.com", "Developer", "Eric7777!"),
                new NewPerson("Eric", "Ruff", "Eric", "ejames.ruff@gmail.com", "Admin", "Eric7777!")
                          };
            NewUserWithRole(context, users);
        }

        private void SeedRoles(ApplicationDbContext context, string[] roles)
        {
            // Update the Roles and Users...
            var store = new UserStore<ApplicationUser>(context);
            UserManager<ApplicationUser> manager = new UserManager<ApplicationUser>(store);

            // Make sure the desired roles exist...
            var roleManager = new Microsoft.AspNet.Identity
                .RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));

            // Now process the list of roles...
            foreach (var role in roles)
            {
                if (!context.Roles.Any(r => r.Name == role))
                {
                    roleManager.Create(new IdentityRole { Name = role });
                }
            }
        }
        
        void NewUserWithRole(ApplicationDbContext context, NewPerson[] people)
        {
            var store = new UserStore<ApplicationUser>(context);
            UserManager<ApplicationUser> manager = new UserManager<ApplicationUser>(store);

            foreach (var user in people)
            {
                if (!context.Users.Any(u => u.Email == user.Email))
                {
                    ApplicationUser au = new ApplicationUser
                    {
                        UserName = user.Email,
                        Email = user.Email,
                        FirstName = user.First,
                        LastName = user.Last,
                        DisplayName = user.Display
                    };
                    // Additional info for me...
                    if (user.Email == "ejames.ruff@gmail.com" || user.Email == "ejamesr@yahoo.com")
                    {
                        au.EmailNotification = au.TextNotification = true;
                        au.TextMsgNumber = au.PhoneNumber = "801.318.5889";
                        au.PhoneNumberConfirmed = true;
                    }
                    manager.Create(au, user.Password);
                    manager.AddToRole(au.Id, user.Role);
                }
                var me = context.Users.Where(m => m.Email == "ejames.ruff@gmail.com");
            }
        }
    }
}



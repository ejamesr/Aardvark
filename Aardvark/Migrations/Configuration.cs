using Aardvark.Models;
namespace Aardvark.Migrations
{
    using Microsoft.AspNet.Identity.EntityFramework;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using Microsoft.AspNet.Identity;
    using Aardvark.Helpers;

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

            //context.SkillLevels.AddOrUpdate(
            //    skill => new { skill.Name, skill.Weight },
            //    new SkillLevel { Name = "Jun", Weight = Step.Val },                 //"Junior", Weight = Step.Val },
            //    new SkillLevel { Name = "Mid", Weight = Step.Val },                 //"Mid-Level", Weight = Step.Val },
            //    new SkillLevel { Name = "Sen", Weight = Step.Val }                  //"Senior", Weight = Step.Val }
            //    );
            context.SkillLevels.AddOrUpdate(
                skill => new { skill.Name, skill.Weight },
                new SkillLevel { Name = "Junior", Weight = Step.Val },
                new SkillLevel { Name = "Mid-Level", Weight = Step.Val },
                new SkillLevel { Name = "Senior", Weight = Step.Val }
                );

            Step = new Stepper(0, 10);
            //context.TicketStatuses.AddOrUpdate(
            //    status => new { status.Name, status.Step },
            //    new TicketStatus { Name = "New", Step = Step.Val },                 //"New", Step = Step.Val },
            //    new TicketStatus { Name = "UnRep", Step = Step.Val },               //"UnableToReproduce", Step = Step.Val },
            //    new TicketStatus { Name = "Deferred", Step = Step.Val },            //"Deferred/OnHold", Step = Step.Val },
            //    new TicketStatus { Name = "R-Dev", Step = Step.Val },               //"ReadyToAssign", Step = Step.Val },
            //    new TicketStatus { Name = "AtoDev", Step = Step.Val },              //"AssignedToDeveloper", Step = Step.Val },
            //    new TicketStatus { Name = "InDev", Step = Step.Val },               //"InDevelopment", Step = Step.Val },
            //    new TicketStatus { Name = "R-Test", Step = Step.Val },              //"ReadyToTest", Step = Step.Val },
            //    new TicketStatus { Name = "AtoTest", Step = Step.Val },             //"AssignedToTester", Step = Step.Val },
            //    new TicketStatus { Name = "InTest", Step = Step.Val },              //"InTesting", Step = Step.Val },
            //    new TicketStatus { Name = "Review", Step = Step.Val },              //"ReadyToReview", Step = Step.Val },
            //    new TicketStatus { Name = "Resolved", Step = Step.Val }             //"Resolved", Step = Step.Val }
            //    );
            context.TicketStatuses.AddOrUpdate(
                status => new { status.Name, status.Step },
                new TicketStatus { Name = "New", Step = Step.Val },
                new TicketStatus { Name = "UnableToReproduce", Step = Step.Val },
                new TicketStatus { Name = "Deferred/OnHold", Step = Step.Val },
                new TicketStatus { Name = "ReadyToAssign", Step = Step.Val },
                new TicketStatus { Name = "AssignedToDeveloper", Step = Step.Val },
                new TicketStatus { Name = "InDevelopment", Step = Step.Val },
                new TicketStatus { Name = "ReadyToTest", Step = Step.Val },
                new TicketStatus { Name = "AssignedToTester", Step = Step.Val },
                new TicketStatus { Name = "InTesting", Step = Step.Val },
                new TicketStatus { Name = "ReadyToReview", Step = Step.Val },
                new TicketStatus { Name = "Resolved", Step = Step.Val }
                );

            Step = new Stepper(0, 3);
            //context.TicketPriorities.AddOrUpdate(
            //    priority => priority.Name,
            //    new TicketPriority { Name = "Opti", Val = Step.Val },               //"Optional", Val = Step.Val },
            //    new TicketPriority { Name = "Desi", Val = Step.Val },               //"Desirable", Val = Step.Val },
            //    new TicketPriority { Name = "Esse", Val = Step.Val },               //"Essential", Val = Step.Val },
            //    new TicketPriority { Name = "Crit", Val = Step.Val },               //"Critical", Val = Step.Val },
            //    new TicketPriority { Name = "Seve", Val = Step.Val },               //"Severe/DataLoss", Val = Step.Val },
            //    new TicketPriority { Name = "Stop", Val = Step.Val }                //"Showstopper", Val = Step.Val }
            //    );
            context.TicketPriorities.AddOrUpdate(
                priority => priority.Name,
                new TicketPriority { Name = "Optional", Val = Step.Val },
                new TicketPriority { Name = "Desirable", Val = Step.Val },
                new TicketPriority { Name = "Essential", Val = Step.Val },
                new TicketPriority { Name = "Critical", Val = Step.Val },
                new TicketPriority { Name = "Severe/DataLoss", Val = Step.Val },
                new TicketPriority { Name = "Showstopper", Val = Step.Val }
                );
            //context.TicketTypes.AddOrUpdate(
            //    t => t.Name,
            //    new TicketType { Name = "Bug" },                                    //"Bug" },         
            //    new TicketType { Name = "Enh" },                                    //"Enhancement" },
            //    new TicketType { Name = "???" }                                     //"Unknown" }
            //    );
            context.TicketTypes.AddOrUpdate(
                t => t.Name,
                new TicketType { Name = "Bug" },         
                new TicketType { Name = "Enhancement" },
                new TicketType { Name = "Unknown" }
                );

            SeedRoles(context,
                new string[] {      // List in hierarchical order - higher has more power
                    R.Guest,
                    R.Admin,
                    R.ProjectManager,
                    R.Developer,
                    R.Submitter,
                    R.NewUser       // This is the default role if none other specified
                });

            // Seed these users...
            string mainAdmin = "ejames.ruff@gmail.com";
            NewPerson[] users = {
                new NewPerson("Eric-Dev", "Ruff", "Eric-Dev", "ejamesr@yahoo.com", R.Dev, "Eric7777!"),
                new NewPerson("Patty", "Whack", "Patty-PM", "PattyWhack@ThisOldMan.com", R.PM, "Ab1234."),
                new NewPerson("John", "Smith", "John", "john@me.com", R.Submitter, "Ab1234."),
                new NewPerson("Eric", "Ruff", "Eric", mainAdmin, R.Admin, "Eric7777!"),
                new NewPerson("Guest", "", "Guest", "Guest@me.com", R.Guest, "Ab1234.")
                          };
            NewUserWithRole(context, users);
            context.SaveChanges();      // Update...

            // Finally, create a first project, if none there yet
            // Assign it to the Admin...
            var admin = context.Users.First(a=>a.UserName == mainAdmin);
            Project project = null;
            // Now if no projects, add one now.
            if (context.Projects.Count() == 0)
            {
                // Create a project, get its id
                project = new Project();

                // Need to fix -- add creator name to list -- ejr
                project.Name = "Our Main Product";
                project.Description = "All bug and enhancement tickets for our main product";
                context.Projects.Add(project);

                // Preserve many-to-many link by adding 'admin' to the project.Users table
                project.Users.Add(admin);
                context.SaveChanges();

                // This next step should be handled automatically by EF
                //// And generate ProjectUsers entry...
                //ProjectUser pu = new ProjectUser(project.Id, admin.Id);
                //context.ProjectUsers.Add(pu);
                //context.SaveChanges();
            }
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
                    // Add additional roles for me...
                    if (user.Email == "ejames.ruff@gmail.com")
                    {
                        manager.AddToRole(au.Id, R.Dev);
                        manager.AddToRole(au.Id, R.PM);
                        manager.AddToRole(au.Id, R.Submitter);
                    }
                }
            }
        }
    }
}



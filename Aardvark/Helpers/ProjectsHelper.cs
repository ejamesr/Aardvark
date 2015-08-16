using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Aardvark.Models;

namespace Aardvark.Helpers
{
    public static class ProjectsHelper
    {
        private static ApplicationDbContext db = new ApplicationDbContext();

        public static IList<Project> ListAllProjects()
        {
            return db.Projects.ToList();
        }

        public static string[] TicketDueStatus(DateTimeOffset date)
        {
            var diff = date - DateTimeOffset.UtcNow;
            int days = diff.Days;
            int hrs = diff.Hours;
            if (days < -1)
            {
                // Overdue!
                return new string[] {
                    String.Format("Ticket {0}d {1}h overdue!", -days, -hrs),
                    "wayOverdue" };
            }
            else if (days < 0)
            {
                return new string[] {
                    String.Format("Ticket {0}d {1}h overdue", -days, -hrs),
                    "overdue"};
            }
            else if (days < 1)
            {
                return new string[] { "Ticket due today", "dueToday" };
            }
            else
            {
                string d = String.Format("Ticket due {0}d {1}h", days, hrs);
                if (days < 7)
                {
                    return new string[] { d, "dueWeek" };
                }
                else if (days < 30)
                {
                    return new string[] { d, "dueMonth" };
                }
                else
                {
                    return new string[] { d, "dueLater" };
                }
            }
        }

        public class UserModel {
            public bool IsLoggedIn;
            public ApplicationUser User;
            public string UserName;
            public string DisplayName;
            public string Role;
            public bool IsAdmin;
            public bool IsGuest;
            public bool IsPM;
            public bool IsDeveloper;
            public bool IsSubmitter;
        }

        public static UserModel LoadUserModel()
        {
            UserRolesHelper helper = new UserRolesHelper();
            UserModel model = new UserModel();
            var user = helper.GetCurrentUser();
            if (user == null)
            {
                model.IsLoggedIn = false;
                model.User = user;
                model.UserName = model.DisplayName = "Not logged in";
                model.Role = "None";
                model.IsAdmin = model.IsDeveloper = model.IsPM = model.IsSubmitter = model.IsGuest = false;
            }
            else
            {
                model.IsLoggedIn = true;
                model.User = user;
                model.UserName = user.UserName;
                model.Role = helper.GetHighestRole(user.Id);
                model.DisplayName = user.DisplayName.Length > 0 ? user.DisplayName : user.UserName;
                model.IsAdmin = helper.IsUserInRole(user.Id, R.Admin);
                model.IsGuest = helper.IsUserInRole(user.Id, R.Guest);
                model.IsPM = helper.IsUserInRole(user.Id, R.PM);
                model.IsDeveloper = helper.IsUserInRole(user.Id, R.Developer);
                model.IsSubmitter = helper.IsUserInRole(user.Id, R.Submitter);
            }
            return (model);
        }

        public static ApplicationUser GetProjectManager(int projectId)
        {
            // Return the User object for the ProjectManager of this project
            UserRolesHelper helper = new UserRolesHelper();
            return db.Projects.Find(projectId).Users.
                FirstOrDefault(u => u.Roles.Any(r => r.RoleId == helper.GetRoleId(R.PM)));
        }

        public static bool IsUserOnProject(string userId, int projectId)
        {
            return db.Projects.SingleOrDefault(p => p.Id == projectId).Users.Any(u => u.Id == userId);
        }

        public static bool AddUserToProject(string userId, int projectId)
        {
            if (!IsUserOnProject(userId, projectId))
            {
                var project = db.Projects.Find(projectId);
                project.Users.Add(db.Users.Find(userId));
                //db.Entry(project).State = EntityState.Modified;
                db.SaveChanges();
                return true;        // Show user was added
            }
            return false;           // Show user was not added
        }

        public static bool RemoveUserFromProject(string userId, int projectId)
        {
            if (IsUserOnProject(userId, projectId))
            {
                bool retVal;
                var project = db.Projects.Find(projectId);
                retVal = project.Users.Remove(db.Users.Find(userId));
                //db.Entry(project).State = EntityState.Modified;
                db.SaveChanges();
                return true;        // Show user was removed
            }
            return false;           // Show user was not removed
        }

        public static ICollection<Project> ListProjectsForUser(string userId)
        {
            return db.Users.Find(userId).Projects;
        }

        public static ICollection<Ticket> ListTicketsForUser(string userId)
        {
            return db.Users.Find(userId).TicketsAssigned;
        }

        public static ICollection<Project> ListProjectsNotForUser(string userId)
        {
            // Find projects where, for each user, the entire list of users (all the users) does not have
            // any whose Id == userId
            return db.Projects.Where(p => p.Users.All(x => x.Id != userId)).ToList();
        }

        public static ICollection<ApplicationUser> ListUsersOnProject(int projectId)
        {
            return db.Projects.Find(projectId).Users;
        }

        public static ICollection<ApplicationUser> ListUsersNotOnProject(int projectId)
        {
            return db.Users.Where(u => !u.Projects.Any(x => x.Id == projectId)).ToList();
        }

        public static ICollection<ApplicationUser> ListDevelopersOnProject(int projectId)
        {
            var devRoleId = db.Roles.SingleOrDefault(r => r.Name == R.Dev).Id;
            return db.Projects.Find(projectId).Users.Where(u => u.Roles.Any(r => r.RoleId == devRoleId)).ToList();
        }

        public static ICollection<ApplicationUser> ListDevelopersNotOnProject(int projectId)
        {
            var devRoleId = db.Roles.SingleOrDefault(r => r.Name == R.Dev).Id;
            return db.Users
                .Where(u => u.Roles.Any(r => r.RoleId == devRoleId))
                .Where(u => u.Projects.All(p => p.Id != projectId)).ToList();
        }

        public static AssignDevelopersModel GetDevStats(int projectId)
        {
            // Collect all the stats for each developer
            // So first, collect all developers...
            var devsAssigned = ListDevelopersOnProject(projectId);
            var devsNotAssigned = ListDevelopersNotOnProject(projectId);

            // For each developer, determine total tickets, total projects, and
            //   total tickets this project
            AssignDevelopersModel model = 
                new AssignDevelopersModel(db.Projects.Find(projectId));
            foreach (var dev in devsAssigned)
            {
                Developer d = new Developer(dev, projectId, true);
                model.Assigned.Add(d);
            }
            foreach (var dev in devsNotAssigned)
            {
                Developer d = new Developer(dev, projectId, false);
                model.NotAssigned.Add(d);
            }
            return model;
        }
    }
    // Create new model to help in assigning developers...
    public class Developer
    {
        private UserRolesHelper rHelper = new UserRolesHelper();
        public string Id { get; set; }        // User Id
        public string Name { get; set; }
        public int NumProjects { get; set; }
        public int NumTickets { get; set; }
        public int NumTicketsThisProject { get; set; }

        // Remember original status
        public bool OrigWasAssigned { get; set; }

        // Collect new status
        public bool NewIsAssigned { get; set; }

        public Developer(ApplicationUser dev, int projectId, bool isNowAssigned)
        {
            Id = dev.Id;
            Name = rHelper.GetDisplayName(dev);
            OrigWasAssigned = NewIsAssigned = isNowAssigned;
            NumProjects = ProjectsHelper.ListProjectsForUser(dev.Id).Count;
            var tickets = ProjectsHelper.ListTicketsForUser(dev.Id);
            NumTickets = tickets.Count;
            NumTicketsThisProject = tickets.Where(t => t.ProjectId == projectId).Count();
        }
    }

    public class AssignDevelopersModel
    {
        public ICollection<Developer> Assigned = new List<Developer>();
        public ICollection<Developer> NotAssigned = new List<Developer>();
        public Project Project { get; set; }
        public string NamePM { get; set; }
        public AssignDevelopersModel(Project project)
        {
            UserRolesHelper helper = new UserRolesHelper();
            string roleId = helper.GetRoleId(R.ProjectManager);
            var PM = project.Users.FirstOrDefault();    // Need to fix - ejr
            Project = project;
            NamePM = helper.GetDisplayName(PM);
        }
    }
}
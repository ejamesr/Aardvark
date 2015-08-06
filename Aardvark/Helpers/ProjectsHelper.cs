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

        public static void AddUserToProject(string userId, int projectId)
        {
            if (!IsUserOnProject(userId, projectId))
            {
                var project = db.Projects.Find(projectId);
                project.Users.Add(db.Users.Find(userId));
                db.Entry(project).State = EntityState.Modified;
                db.SaveChanges();
            }
        }

        public static void RemoveUserFromProject(string userId, int projectId)
        {
            if (IsUserOnProject(userId, projectId))
            {
                var project = db.Projects.Find(projectId);
                project.Users.Remove(db.Users.Find(userId));
                db.Entry(project).State = EntityState.Modified;
                db.SaveChanges();
            }
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
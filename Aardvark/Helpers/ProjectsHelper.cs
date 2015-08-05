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
    public class ProjectsHelper
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public IList<Project> ListAllProjects()
        {
            return db.Projects.ToList();
        }

        public bool IsUserOnProject(string userId, int projectId)
        {
            return db.Projects.SingleOrDefault(p => p.Id == projectId).Users.Any(u => u.Id == userId);
        }

        public void AddUserToProject(string userId, int projectId)
        {
            if (!IsUserOnProject(userId, projectId))
            {
                var project = db.Projects.Find(projectId);
                project.Users.Add(db.Users.Find(userId));
                db.Entry(project).State = EntityState.Modified;
                db.SaveChanges();
            }
        }

        public void RemoveUserFromProject(string userId, int projectId)
        {
            if (IsUserOnProject(userId, projectId))
            {
                var project = db.Projects.Find(projectId);
                project.Users.Remove(db.Users.Find(userId));
                db.Entry(project).State = EntityState.Modified;
                db.SaveChanges();
            }
        }

        public ICollection<Project> ListProjectsForUser(string userId)
        {
            return db.Users.Find(userId).Projects;
        }

        public ICollection<Project> ListProjectsNotForUser(string userId)
        {
            // Find projects where, for each user, the entire list of users (all the users) does not have
            // any whose Id == userId
            return db.Projects.Where(p => p.Users.All(x => x.Id != userId)).ToList();
        }

        public ICollection<ApplicationUser> ListUsersOnProject(int projectId)
        {
            return db.Projects.Find(projectId).Users;
        }

        public ICollection<ApplicationUser> ListUsersNotOnProject(int projectId)
        {
            return db.Users.Where(u => !u.Projects.Any(x => x.Id == projectId)).ToList();
        }

        // Create new model to help in assigning developers...
        public class Developer{
            public string Id {get; set;}        // User Id
            public string Name {get; set;}
            public bool OrigWasAssigned {get; set;}
            public bool NewIsAssigned {get; set;}
            public int NumTicketsThisProject {get; set;}
            public int NumTicketsTotal {get; set;}
        }
        public class AssignDevelopersModel
        {
            public ICollection<Developer> Assigned = null;
            public ICollection<Developer> NotAssigned = null;
            public AssignDevelopersModel()
            {
               
            }
        }
        private ICollection<Developer> QueryDevStats(int id)
        {
            // Process all the stats for the specified id
        }
        public AssignDevelopersModel GetDevStats(int id)
        {
            // Collect all the stats for each developer
            AssignDevelopersModel model = new AssignDevelopersModel();
            var listDev = ListUsersOnProject((int)id);
            var listNotDev = ListUsersNotOnProject((int)id);

            // Now process each list and gather some stats on each developer
            int x = 34;
            model.Assigned = QueryDevStats((int)id);
            model.NotAssigned = QueryDevStats(0);
            return model;
        }
    }
}
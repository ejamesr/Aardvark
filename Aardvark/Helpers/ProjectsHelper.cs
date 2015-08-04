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

        public IList<Project> ListProjects()
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
    }
}
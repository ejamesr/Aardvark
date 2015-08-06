﻿using Aardvark.Helpers;
using Aardvark.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Aardvark.Controllers
{
    public class ProjectsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Projects
        [Authorize(Roles="Admin,Project Manager,Guest,Developer")]
        public ActionResult Index()
        {
            string uId = User.Identity.GetUserId();
            var Roles = new UserRolesHelper().ListUserRoles(uId);
            ViewBag.Roles = Roles;
            if (Roles.Contains(R.Admin) || Roles.Contains(R.Guest))
            {
                // Show all projects without restriction
                var projects = db.Projects.Include(p => p.Users);
                return View(projects.ToList());
            }
            else if (Roles.Contains(R.ProjectManager))
            {
                // Person is both PM and Dev, so show union...ed
                var projects = db.Users.Find(uId).Projects
                    .Union(db.Projects.Where(r => r.Users.Any(u => u.Id == uId)));
                return View(projects.ToList());
            }
            else
            {
                // Show only projects assigned to Dev
                var projects = db.Users.Find(uId).Projects;
                return View(projects.ToList());
            }
        }

        // GET: Projects/AssignPM
        public ActionResult AssignPM()
        {
            return View();
        }

        // GET: Projects/ProjectsPostError
        public ActionResult ProjectsPostError()
        {
            return View();
        }

        // GET: Projects/AssignDev
        [Authorize(Roles="Admin, Guest, Project Manager")]
        public ActionResult AssignDev(int? id)      // Assign developers to this project
        {
            // Assign developers to this project
            if (id != null)
            {
                // First, get complete list of all developers, whether assigned or not
                var stats = ProjectsHelper.GetDevStats((int)id);
                // Now, pass all this to the View model...
                return View(stats);
            }

            return View();
        }


        // POST: Home/ManageUsers
        [HttpPost]
        [Authorize(Roles = "Admin, Guest, Project Manager")]
        [ValidateAntiForgeryToken]
        //public ActionResult ManageUsers(IEnumerable<UsersCheckboxes> UserInfo)
        public ActionResult AssignDev(string[] Select, int Id)
        {
            if (ModelState.IsValid)
            {
                // Select has data strings that come in the following order:
                //
                //   1st: UserId
                //   2nd: "T" if developer was originally assigned to project, else "F"
                //   3rd: Optional: "T" only if the checkbox was checked at time of Submit, else no string
                //
                //   If there is a 3rd string (which will only be "T"), there was no change if 2nd string is also "T".
                //   - If 2nd is "F", this user needs to be added to the project
                //
                //   If there is not a third string (i.e., end of list encountered, or next is a long UserId), then:
                //   - if the 2nd is "F", there was no change (i.e., developer still not in project)
                //   - else if the 2nd is "T", the developer needs to be removed from the project
                //
                // So, scoop everything up first and recreate the original model
                if (Select != null && Select.Length > 0)
                {
                    int index = 0;
                    string userId;
                    string origVal;

                    while (index < Select.Length)
                    {
                        // Get a user
                        if (index + 1 >= Select.Length)
                            return RedirectToAction("ProjectsPostError");
                        // Grab id and origVal
                        userId = Select[index];
                        origVal = Select[index + 1];
                        index += 2;
                        if (index >= Select.Length) 
                        {
                            // No more data, so if origVal was "F' we are finished
                            if (origVal == "T") {
                                // User was just desselected
                                ProjectsHelper.RemoveUserFromProject(userId, Id);
                            }
                            // Since this is the end of the list, exit now
                            break;
                        }

                        // If the next item is one char wide, grab it, else its a userId
                        if (Select[index].Length == 1) {
                            // This developer was checked, so see if we need to add him
                            index++;        // Skip over
                            if (origVal == "F")
                                // User was just added
                                ProjectsHelper.AddUserToProject(userId, Id);
                        }
                        else
                        {
                            // The next item is a userId... so check origVal
                            if (origVal == "T")
                                ProjectsHelper.RemoveUserFromProject(userId, Id);
                        }
                    }
                    // All went according to plan, so return to Main Menu!
                    return RedirectToAction("Index");
                }
            }
            // There was an error of some kind, so show it
            return RedirectToAction("ProjectsPostError");
        }


        // GET: Projects/Details/
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Project project = db.Projects.Find(id);
            if (project == null)
            {
                return HttpNotFound();
            }
            UserRolesHelper helper = new UserRolesHelper();
            ViewBag.ProjectManagerDisplayName = helper.GetProjectManagerDisplayName((int)id);
            var Roles = helper.ListUserRoles(User.Identity.GetUserId());
            ViewBag.Roles = Roles;
            return View(project);
        }

        // GET: Projects/Create
        [Authorize(Roles="Admin")]
        public ActionResult Create()
        {
            // Practice getting list of all Project Managers, and list of all except PMs...
            UserRolesHelper helper = new UserRolesHelper();
            var pmList = helper.UsersInRole(R.ProjectManager);
            var nonPmList = helper.UsersNotInRole(R.ProjectManager);

            string rolePm = db.Roles.FirstOrDefault(r => r.Name == R.ProjectManager).Id;

            ViewBag.ProjectMgrId = pmList != null
                ? new SelectList(pmList, "Id", "UserName")
                : null;
            return View();
        }

        // POST: Projects/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Create([Bind(Include = "Id,Name,Description,ProjectMgrId")] Project project)
        {
            if (ModelState.IsValid)
            {
                var pmId = Request["ProjectMgrId"];
                UserRolesHelper helper = new UserRolesHelper();
                db.Projects.Add(project);
                project.Users.Add(db.Users.Find(pmId));
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ProjectMgrId = new SelectList(db.Users, "Id", "FirstName");//, project.ProjectMgr.Id);
            return View(project);
        }

        // GET: Projects/Edit/5
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Project project = db.Projects.Find(id);
            if (project == null)
            {
                return HttpNotFound();
            }
            string rolePm = db.Roles.FirstOrDefault(r => r.Name == R.ProjectManager).Id;
            if (rolePm != null)
            {
                ViewBag.ProjectMgrId =
                    new SelectList(db.Users
                        .Where(d => d.Roles.FirstOrDefault(r => r.RoleId == rolePm) != null), 
                        "Id", "UserName");//, project.ProjectMgr.Id);
            }
            else ViewBag.ProjectMgrId = null;
            return View(project);
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Edit([Bind(Include = "Id,Name,Description,ProjectMgrId")] Project project)
        {
            if (ModelState.IsValid)
            {
                db.Entry(project).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ProjectMgrId = new SelectList(db.Users, "Id", "FirstName");//, project.ProjectMgr.Id);
            return View(project);
        }

        // GET: Projects/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Project project = db.Projects.Find(id);
            if (project == null)
            {
                return HttpNotFound();
            }
            return View(project);
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Project project = db.Projects.Find(id);
            db.Projects.Remove(project);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

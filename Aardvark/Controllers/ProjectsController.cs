using Aardvark.Helpers;
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
                var projects = db.Projects.Include(p => p.ProjectMgr);
                return View(projects.ToList());
            }
            else if (Roles.Contains(R.ProjectManager))
            {
                // Person is both PM and Dev, so show union...ed
                var projects = db.Users.Find(uId).Projects
                    .Union(db.Projects.Where(u => u.ProjectMgrId == uId));
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

        // GET: Projects/AssignDev
        [Authorize(Roles="Admin, Project Manager")]
        public ActionResult AssignDev(int? id)
        {
            // Assign developers to this project
            if (id != null)
            {
                // First, get complete list of all developers, whether assigned or not
                ProjectsHelper ph = new ProjectsHelper();
                var stats = ph.GetDevStats((int)id);
                // Now, pass all this to the View model...
                return View(stats);
            }

            return View();
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
            var Roles = new UserRolesHelper().ListUserRoles(User.Identity.GetUserId());
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
            if (rolePm != null)
            {
                ViewBag.ProjectMgrId =
                    new SelectList(db.Users
                        .Where(d => d.Roles.FirstOrDefault(r => r.RoleId == rolePm) != null), "Id", "UserName");
            }
            else ViewBag.ProjectMgrId = null;
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
                db.Projects.Add(project);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ProjectMgrId = new SelectList(db.Users, "Id", "FirstName", project.ProjectMgrId);
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
                        "Id", "UserName", project.ProjectMgrId);
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
            ViewBag.ProjectMgrId = new SelectList(db.Users, "Id", "FirstName", project.ProjectMgrId);
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

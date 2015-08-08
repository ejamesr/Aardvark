using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Aardvark.Models;
using Microsoft.AspNet.Identity;
using Aardvark.Helpers;

namespace Aardvark.Controllers
{
    public class TicketsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Tickets
        public ActionResult Index()
        {
            // Fix the Created date for all tickets...
            //var tk = db.Tickets;
            //var zero = new DateTimeOffset();
            //var now = DateTimeOffset.UtcNow.AddDays(-2.0);
            //DateTimeOffset max;
            //foreach (var ticket in tk) {
            //    if (ticket.Created == zero)
            //        ticket.Created = now;
            //    max = ticket.Created;
            //    if (ticket.Updated != null)
            //        max = ticket.Updated > max ? ticket.Updated.Value : max;
            //    if (max > ticket.MostRecentUpdate)
            //        ticket.MostRecentUpdate = max;
            //}
            //db.SaveChanges();


            var tickets = db.Tickets.Include(t => t.AssignedToUser).Include(t => t.OwnerUser).Include(t => t.Project).Include(t => t.SkillRequired).Include(t => t.TicketPriority).Include(t => t.TicketStatus).Include(t => t.TicketType);
            return View(tickets.ToList());
        }

        // GET: Tickets/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Tickets.Find(id);
            if (ticket == null)
            {
                return HttpNotFound();
            }
            return View(ticket);
        }

        // GET: Tickets/Create
        public ActionResult Create()
        {
            Ticket model = new Ticket();
            model.DueDate = DateTimeOffset.UtcNow.AddDays(1);
            model.HoursToComplete = 1;
            UserRolesHelper helper = new UserRolesHelper();
            var id = User.Identity.GetUserId();
            var highest = helper.GetHighestRole(id);
            ViewBag.HighestUserRole = highest;

            // If Admin, allow to select Developer when creating the ticket
            var roles = helper.ListUserRoles(id);
            if (roles.Contains(R.Admin) || roles.Contains(R.Guest) || roles.Contains(R.ProjectManager))
            {
                var roleDev = db.Roles.FirstOrDefault(r => r.Name == R.Developer);
                ViewBag.CanAssignDeveloper = true;
                if (roleDev != null)
                {
                    ViewBag.AssignedToUserId =
                        new SelectList(db.Users
                            .Where(d => d.Roles.FirstOrDefault(r => r.RoleId == roleDev.Id) != null), "Id", "UserName");
                }
                else ViewBag.AssignedToUserId = Enumerable.Empty<SelectListItem>();
            }
            else
            {
                ViewBag.AssignedToUserId = Enumerable.Empty<SelectListItem>();
                ViewBag.CanAssignDeveloper = false;
            }

            ViewBag.OwnerUserId = new SelectList(db.Users, "Id", "FirstName");
            ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Name");
            ViewBag.SkillRequiredId = new SelectList(db.SkillLevels, "Id", "Name");
            ViewBag.TicketPriorityId = new SelectList(db.TicketPriorities, "Id", "Name");
            ViewBag.TicketStatusId = new SelectList(db.TicketStatuses, "Id", "Name");
            ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name");
            return View(model);
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ProjectId,TicketTypeId,TicketPriorityId,TicketStatusId,OwnerUserId,AssignedToUserId,SkillRequiredId,Title,Description,DueDate,HoursToComplete")] Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                ticket.OwnerUserId = User.Identity.GetUserId();
                ticket.Created = DateTimeOffset.UtcNow;
                if (ticket.TicketStatusId == 0)
                    ticket.TicketStatusId = 1;  // Point to New status
                db.Tickets.Add(ticket);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.AssignedToUserId = new SelectList(db.Users, "Id", "FirstName", ticket.AssignedToUserId);
            ViewBag.OwnerUserId = new SelectList(db.Users, "Id", "FirstName", ticket.OwnerUserId);
            ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Name", ticket.ProjectId);
            ViewBag.SkillRequiredId = new SelectList(db.SkillLevels, "Id", "Name", ticket.SkillRequiredId);
            ViewBag.TicketPriorityId = new SelectList(db.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            ViewBag.TicketStatusId = new SelectList(db.TicketStatuses, "Id", "Name", ticket.TicketStatusId);
            ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            return View(ticket);
        }

        // GET: Tickets/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Tickets.Find(id);
            if (ticket == null)
            {
                return HttpNotFound();
            }

            UserRolesHelper helper = new UserRolesHelper();
            var userId = helper.GetCurrentUserId();
            var roles = helper.ListUserRoles(userId);

            // If Admin, allow to select Developer when creating the ticket
            if (roles.Contains(R.Admin) || roles.Contains(R.Guest) || roles.Contains(R.ProjectManager))
            {
                var roleDev = db.Roles.FirstOrDefault(r => r.Name == R.Developer);
                ViewBag.CanAssignDeveloper = true;
                if (roleDev != null)
                {
                    ViewBag.AssignedToUserId =
                        new SelectList(db.Users
                            .Where(d => d.Roles.FirstOrDefault(r => r.RoleId == roleDev.Id) != null), "Id", "UserName",
                            ticket.AssignedToUserId);
                }
                else ViewBag.AssignedToUserId = Enumerable.Empty<SelectListItem>();
            }
            else
            {
                ViewBag.AssignedToUserId = Enumerable.Empty<SelectListItem>();
                ViewBag.CanAssignDeveloper = false;
            }

            ViewBag.OwnerUserId = new SelectList(db.Users, "Id", "FirstName", ticket.OwnerUserId);
            ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Name", ticket.ProjectId);
            ViewBag.SkillRequiredId = new SelectList(db.SkillLevels, "Id", "Name", ticket.SkillRequiredId);
            ViewBag.TicketPriorityId = new SelectList(db.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            ViewBag.TicketStatusId = new SelectList(db.TicketStatuses, "Id", "Name", ticket.TicketStatusId);
            ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            return View(ticket);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,ProjectId,TicketTypeId,TicketPriorityId,TicketStatusId,OwnerUserId,AssignedToUserId,SkillRequiredId,Title,Description,Created,Updated,DueDate,HoursToComplete")] Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                ticket.Updated = DateTimeOffset.UtcNow;
                db.Entry(ticket).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.AssignedToUserId = new SelectList(db.Users, "Id", "FirstName", ticket.AssignedToUserId);
            ViewBag.OwnerUserId = new SelectList(db.Users, "Id", "FirstName", ticket.OwnerUserId);
            ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Name", ticket.ProjectId);
            ViewBag.SkillRequiredId = new SelectList(db.SkillLevels, "Id", "Name", ticket.SkillRequiredId);
            ViewBag.TicketPriorityId = new SelectList(db.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            ViewBag.TicketStatusId = new SelectList(db.TicketStatuses, "Id", "Name", ticket.TicketStatusId);
            ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            return View(ticket);
        }

        // GET: Tickets/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Tickets.Find(id);
            if (ticket == null)
            {
                return HttpNotFound();
            }
            return View(ticket);
        }

        // POST: Tickets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Ticket ticket = db.Tickets.Find(id);
            db.Tickets.Remove(ticket);
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

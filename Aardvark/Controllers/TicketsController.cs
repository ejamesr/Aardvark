﻿using Aardvark.Helpers;
using Aardvark.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;

namespace Aardvark.Controllers
{
    public class TicketsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Tickets
        public ActionResult Index(string scope)
        {
            //
            // Code to run to fix the Created date for all tickets...
            //
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
            //

            // First, get this user...
            UserRolesHelper helper = new UserRolesHelper();
            var userId = helper.GetCurrentUserId();
            // Do this in every GET action...
            ViewBag.UserModel = ProjectsHelper.LoadUserModel();
            ViewBag.Scope = scope ?? "";
            if (scope == null)
                scope = "";

            DateTimeOffset now = DateTimeOffset.UtcNow;
            DateTimeOffset start, end;
            switch (scope)
            {
                case "My":
                    // Need to track all places where User could be attached to a ticket:
                    // - as PM (then follow ticket chain)
                    // - as Developer (follow ticket chain)
                    // - as ticket creator
                    // - as ticket commenter
                    var myTickets = db.Users.Find(userId).TicketsAssigned
                        .Union(db.Users.Find(userId).TicketsOwned);
                    return View(myTickets.ToList());
                case "All":
                    // Come here for all tickets
                    var allTickets = db.Tickets.Include(t => t.AssignedToDev).Include(t => t.OwnerUser).Include(t => t.Project).Include(t => t.SkillRequired).Include(t => t.TicketPriority).Include(t => t.TicketStatus).Include(t => t.TicketType);
                    return View(allTickets.ToList());
                case "NotAssigned":
                // Come here for all not yet assigned
                    var notTickets = db.Tickets
                        .Where(tic => tic.TicketStatus.Step <= 30)
                        .Include(t => t.AssignedToDev).Include(t => t.OwnerUser).Include(t => t.Project).Include(t => t.SkillRequired).Include(t => t.TicketPriority).Include(t => t.TicketStatus).Include(t => t.TicketType);
                    return View(notTickets.ToList());
                case "MyNew":
                    var newTickets = db.Tickets
                        .Where(t => t.AssignedToDevId == userId && t.TicketStatusId == (int)TS.Status.AssignedToDeveloper);
                    return View(newTickets.ToList());
                case "MyDue7":
                    end = now.AddDays(7);
                    var due7Tickets = db.Tickets
                        .Where(t => t.AssignedToDevId == userId
                            && t.TicketStatusId != (int)TS.Status.Resolved
                            && t.DueDate >= now && t.DueDate < end);
                    return View(due7Tickets.ToList());
                case "MyDue24":
                    end = now.AddDays(1);
                    var due24Tickets = db.Tickets
                        .Where(t => t.AssignedToDevId == userId
                            && t.TicketStatusId != (int)TS.Status.Resolved
                            && t.DueDate >= now && t.DueDate < end);
                    return View(due24Tickets.ToList());
                case "MyDue30":
                    end = now.AddDays(30);
                    var due30Tickets = db.Tickets
                        .Where(t => t.AssignedToDevId == userId
                            && t.TicketStatusId != (int)TS.Status.Resolved
                            && t.DueDate >= now && t.DueDate < end);
                    return View(due30Tickets.ToList());
                case "MyOverdue":
                    start = now.AddDays(-1);
                    var overdueTickets = db.Tickets
                        .Where(t => t.AssignedToDevId == userId
                            && t.DueDate >= start
                            && t.DueDate < now);
                    return View(overdueTickets.ToList());
                case "MyInDevelopment":
                    var devTickets = db.Tickets
                        .Where(t => t.AssignedToDevId == userId
                            && t.TicketStatusId == (int)TS.Status.InDevelopment);
                    return View(devTickets.ToList());
                case "MyTesting":
                    var testingTickets = db.Tickets
                        .Where(t => t.AssignedToDevId == userId 
                            && t.TicketStatusId >= (int)TS.Status.ReadyToTest
                            && t.TicketStatusId != (int)TS.Status.Resolved);
                    return View(testingTickets.ToList());
                default:
                    // For all other scopes, come here
                    var tickets = db.Tickets
                        .Where(tic => tic.TicketStatus.Name == scope)
                        .Include(t => t.AssignedToDev).Include(t => t.OwnerUser).Include(t => t.Project).Include(t => t.SkillRequired).Include(t => t.TicketPriority).Include(t => t.TicketStatus).Include(t => t.TicketType);
                    return View(tickets.ToList());
            }
        }

        // GET: Tickets/Details/5
        public ActionResult Details(string anchor, int? id, int? page)
        {
            // Do this in every GET action...
            ViewBag.UserModel = ProjectsHelper.LoadUserModel();
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
        [Authorize(Roles="Admin,Guest,ProjectManager,Developer,Submitter")]
        public ActionResult Create()
        {
            Ticket model = new Ticket();
            model.DueDate = DateTimeOffset.UtcNow.AddDays(10);
            model.HoursToComplete = 1;
            UserRolesHelper helper = new UserRolesHelper();
            var id = User.Identity.GetUserId();
            var roles = helper.ListUserRoles(id);
            var highest = helper.GetHighestRole(id);
            ViewBag.HighestUserRole = highest;

            // If user is Submitter only (or has no role), don't allow Skill, Due Date, or HoursToComplete to show
            ViewBag.BaseOptionsOnly = (roles == null || ((roles.Count == 1) && (roles[0] == R.Submitter))) ? true : false;

            // If Admin, allow to select Developer when creating the ticket
            if (roles.Contains(R.Admin) || roles.Contains(R.Guest) || roles.Contains(R.ProjectManager))
            {
                var roleDev = db.Roles.FirstOrDefault(r => r.Name == R.Developer);
                ViewBag.CanAssignDeveloper = true;
                if (roleDev != null)
                {
                    ViewBag.AssignedToDevId =
                        new SelectList(db.Users
                            .Where(d => d.Roles.FirstOrDefault(r => r.RoleId == roleDev.Id) != null), "Id", "UserName");
                }
                else ViewBag.AssignedToDevId = Enumerable.Empty<SelectListItem>();
            }
            else
            {
                ViewBag.AssignedToDevId = Enumerable.Empty<SelectListItem>();
                ViewBag.CanAssignDeveloper = false;
            }

            // Do this in every GET action...
            ViewBag.UserModel = ProjectsHelper.LoadUserModel();
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
        public ActionResult Create([Bind(Include = "ProjectId,TicketTypeId,TicketPriorityId,TicketStatusId,OwnerUserId,AssignedToDevId,SkillRequiredId,Title,Description,DueDate,HoursToComplete")] Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                // Set default info...
                ticket.OwnerUserId = User.Identity.GetUserId();
                if (ticket.SkillRequiredId == 0)
                    ticket.SkillRequiredId = 1;     // Set to default
                if (ticket.TicketStatusId == 0)
                    ticket.TicketStatusId = 1;  // Point to New status
                ticket.SetCreated();

                // Make sure there's a DueDate...
                if (ticket.DueDate == DateTimeOffset.MinValue)
                    ticket.DueDate = ticket.Created.AddDays(10.0);
                // Make sure deadline is at 5pm!
                TimeSpan time = ticket.DueDate.TimeOfDay;
                ticket.DueDate = ticket.DueDate.Subtract(time);
                time = new TimeSpan(17, 0, 0);
                ticket.DueDate = ticket.DueDate.Add(time);

                db.Tickets.Add(ticket);
                db.SaveChanges();
                TicketNotification.Notify(db, ticket,
                    ticket.Created, Notifications.AssignedToTicket);
                return RedirectToAction("Index");
            }

            // Do this in every GET action...
            ViewBag.UserModel = ProjectsHelper.LoadUserModel();
            ViewBag.AssignedToDevId = new SelectList(db.Users, "Id", "FirstName", ticket.AssignedToDevId);
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
            // Do this in every GET action...
            ViewBag.UserModel = ProjectsHelper.LoadUserModel();

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
                    var dev = 
                        new SelectList(db.Users
                            .Where(d => d.Roles.Any(r => r.RoleId == roleDev.Id)), "Id", "UserName",
                            ticket.AssignedToDevId);
                    ViewBag.AssignedToDevId = dev;
                }
                else ViewBag.AssignedToDevId = Enumerable.Empty<SelectListItem>();
            }
            else
            {
                ViewBag.AssignedToDevId = Enumerable.Empty<SelectListItem>();
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
        public ActionResult Edit([Bind(Include = "Id,ProjectId,TicketTypeId,TicketPriorityId,TicketStatusId,AssignedToDevId,SkillRequiredId,Title,Description,DueDate,HoursToComplete")] Ticket ticket, string submit)
        {
            if (ModelState.IsValid)
            {
                Ticket origTicket = db.Tickets.Find(ticket.Id);

                if (submit == "Ready for Testing")
                {
                    // Update status...
                    origTicket.TicketStatusId = (int)TS.Status.ReadyToTest;
                    db.Entry(origTicket).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index", new { scope = "My" });
                }

                // Check each field to see if changed.  If so, copy new value to origTicket and create TicketHistory
                // But first, remember the current Developer...
                string origDevId = origTicket.AssignedToDevId;

                DateTimeOffset dtChanged = ticket.WasChanged(db, origTicket);
                if (dtChanged != DateTimeOffset.MinValue)
                {
                    origTicket.SetUpdated(dtChanged);
                    db.Entry(origTicket).State = EntityState.Modified;
                    db.SaveChanges();

                    // Need to see if the Assigned Dev was changed... if so, send out 
                    //  one notification to previous Dev, two notifications to new Dev
                    if (origTicket.AssignedToDevId != origDevId)
                    {
                        // Remember the current Dev, swap vars in order to remove...
                        string newDevId = origTicket.AssignedToDevId;
                        origTicket.AssignedToDevId = origDevId;
                        TicketNotification.Notify(db, origTicket,
                            origTicket.Updated.Value, Notifications.RemovedFromTicket);

                        // And now reset to correct new Dev and issue notifications
                        origTicket.AssignedToDevId = newDevId;
                        TicketNotification.Notify(db, origTicket,
                            origTicket.Updated.Value, Notifications.AssignedToTicket);
                        TicketNotification.Notify(db, origTicket,
                            origTicket.Updated.Value, Notifications.TicketModified);
                    }
                }
                return RedirectToAction("Index", new { scope = "My" });
            }
            // Do this in every GET action...
            ViewBag.UserModel = ProjectsHelper.LoadUserModel();
            ViewBag.AssignedToDevId = new SelectList(db.Users, "Id", "FirstName", ticket.AssignedToDevId);
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
            // Do this in every GET action...
            ViewBag.UserModel = ProjectsHelper.LoadUserModel();
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
            // Do this in every GET action...
            ViewBag.UserModel = ProjectsHelper.LoadUserModel();
            Ticket ticket = db.Tickets.Find(id);
            ticket.SetUpdated();
            TicketNotification.Notify(db, ticket,
                ticket.Updated.Value, Notifications.TicketDeleted);
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

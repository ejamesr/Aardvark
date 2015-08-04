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

namespace Aardvark.Controllers
{
    public class TicketsController_orig : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Tickets
        public ActionResult Index()
        {
            var tickets = db.Tickets.Include(p => p.TicketPriority).Include(p => p.TicketType).ToList();
            return View(tickets);
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
            //TicketCreateModel model = new TicketCreateModel();
            ////ViewBag.AssignedToUserId = new SelectList(db.Users, "Id", "UserName");
            ////ViewBag.OwnerUserId = new SelectList(db.Users, "Id", "UserName");
            ////ViewBag.Due = DateTime.UtcNow;

            Ticket model = new Ticket();
            model.DueDate = DateTimeOffset.UtcNow.AddDays(1);
            model.HoursToComplete = 1;
            UserRolesHelper helper = new UserRolesHelper();
            var id = HttpContext.User.Identity.GetUserId();
            var highest = helper.GetHighestRole(id);
            ViewBag.HighestUserRole = highest;
            ViewBag.TypeList = new SelectList(db.TicketTypes, "Id", "Name");
            ViewBag.PriorityList = new SelectList(db.TicketPriorities, "Id", "Name");
            ViewBag.StatusList = new SelectList(db.TicketStatuses, "Id", "Name");
            ViewBag.SkillLevelList = new SelectList(db.SkillLevels, "Id", "Name");
            ViewBag.ProjectList = new SelectList(db.Projects, "Id", "Name");

            if (highest == R.Admin || highest == R.Guest || highest == R.PM)
            {
                // OK to assign list of users...
                var roleDev = db.Roles.FirstOrDefault(r => r.Name == R.Developer);
                if (roleDev != null)
                {
                    ViewBag.AssigneesList = new SelectList(
                        db.Users
                            .Where(d => d.Roles.FirstOrDefault(r => r.RoleId == roleDev.Id) != null),
                            "Id", "UserName");
                    //.Select(assignee => 
                    //    new SelectListItem 
                    //    { 
                    //        Selected = false,
                    //        Text = assignee.UserName,
                    //        Value = assignee.Id
                    //    }
                    //));
                }
            }
            else ViewBag.AssigneesList = null;
            return View(model);
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Title,ProjectId,AssignedToUserId,SkillRequiredId,TicketPriorityId,TicketStatusId,TicketTypeId,Description,DueDate,HoursToComplete")] Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                ticket.Created = DateTimeOffset.UtcNow;
                ticket.OwnerUserId = User.Identity.GetUserId();
                db.Tickets.Add(ticket);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.AssignedToUserId = new SelectList(db.Users, "Id", "FirstName", ticket.AssignedToUserId);
            ViewBag.OwnerUserId = new SelectList(db.Users, "Id", "FirstName", ticket.OwnerUserId);
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
            ViewBag.AssignedToUserId = new SelectList(db.Users, "Id", "FirstName", ticket.AssignedToUserId);
            ViewBag.OwnerUserId = new SelectList(db.Users, "Id", "FirstName", ticket.OwnerUserId);
            return View(ticket);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,OwnerUserId,AssignedToUserId,Title,Description,Created,Updated,DueDate,HoursToComplete")] Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                db.Entry(ticket).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.AssignedToUserId = new SelectList(db.Users, "Id", "FirstName", ticket.AssignedToUserId);
            ViewBag.OwnerUserId = new SelectList(db.Users, "Id", "FirstName", ticket.OwnerUserId);
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

using Aardvark.Models;
using Aardvark.Helpers;
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
    public class TicketNotificationsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: TicketNotifications
        public ActionResult Index(string scope)
        {
            ViewBag.UserModel = ProjectsHelper.LoadUserModel();
            ViewBag.Scope = scope ?? "";

            if (scope == "My")
            {
                // First, get this user...
                UserRolesHelper helper = new UserRolesHelper();
                var userId = helper.GetCurrentUserId();

                // Now get My notifications...
                var ticketNotifications = db.TicketNotifications.Where(n => n.UserId == userId)
                    .OrderBy(n => n.HasBeenRead).ThenByDescending(n => n.Created);
                return View(ticketNotifications.ToList());
            }
            else if (scope == "All")
            {
                var ticketNotifications = db.TicketNotifications.Include(t => t.Ticket).Include(t => t.User)
                    .OrderBy(n => n.HasBeenRead).ThenByDescending(n => n.Created);
                return View(ticketNotifications.ToList());
            }
            else if (scope == "MyAssignedToTicket")
            {
                var notificationType = Notifications.AssignedToTicket;
                var ticketNotifications = db.TicketNotifications.Where(n => n.Type == notificationType);
                return View(ticketNotifications.ToList());
            }
            else
            {
                var notificationType = (Notifications)Enum.Parse(typeof(Notifications), scope);
                var ticketNotifications = db.TicketNotifications.Where(n => n.Type == notificationType);
                return View(ticketNotifications.ToList());
            }
            //else
            //{
            //    var notificationType = (Notifications)Enum.Parse(typeof(Notifications), scope);
            //    var ticketNotifications = db.TicketNotifications.Where(n => n.Type == notificationType);
            //    return View(ticketNotifications.ToList());
            //}
            //else
            //{
            //    var notificationType = (Notifications)Enum.Parse(typeof(Notifications), scope);
            //    var ticketNotifications = db.TicketNotifications.Where(n => n.Type == notificationType);
            //    return View(ticketNotifications.ToList());
            //}
            //else
            //{
            //    var notificationType = (Notifications)Enum.Parse(typeof(Notifications), scope);
            //    var ticketNotifications = db.TicketNotifications.Where(n => n.Type == notificationType);
            //    return View(ticketNotifications.ToList());
            //}
            //else
            //{
            //    var notificationType = (Notifications)Enum.Parse(typeof(Notifications), scope);
            //    var ticketNotifications = db.TicketNotifications.Where(n => n.Type == notificationType);
            //    return View(ticketNotifications.ToList());
            //}
        }

        // GET: TicketNotifications/Details/5
        public ActionResult Details(int? id)
        {
            ViewBag.UserModel = ProjectsHelper.LoadUserModel();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TN ticketNotification = db.TicketNotifications.Find(id);
            if (ticketNotification == null)
            {
                return HttpNotFound();
            }
            return View(ticketNotification);
        }

        // GET: TicketNotifications/Create
        public ActionResult Create()
        {
            ViewBag.UserModel = ProjectsHelper.LoadUserModel();
            ViewBag.TicketId = new SelectList(db.Tickets, "Id", "Title");
            ViewBag.UserId = new SelectList(db.Users, "Id", "FirstName");
            return View();
        }

        // POST: TicketNotifications/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,TicketId,UserId,Type,Created,Method,HasBeenRead,UsedEmail,UsedText")] TN ticketNotification)
        {
            if (ModelState.IsValid)
            {
                db.TicketNotifications.Add(ticketNotification);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.UserModel = ProjectsHelper.LoadUserModel();
            ViewBag.TicketId = new SelectList(db.Tickets, "Id", "Title", ticketNotification.TicketId);
            ViewBag.UserId = new SelectList(db.Users, "Id", "FirstName", ticketNotification.UserId);
            return View(ticketNotification);
        }

        // GET: TicketNotifications/Edit/5
        public ActionResult Edit(int? id)
        {
            ViewBag.UserModel = ProjectsHelper.LoadUserModel();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TN ticketNotification = db.TicketNotifications.Find(id);
            if (ticketNotification == null)
            {
                return HttpNotFound();
            }
            ViewBag.TicketId = new SelectList(db.Tickets, "Id", "Title", ticketNotification.TicketId);
            ViewBag.UserId = new SelectList(db.Users, "Id", "FirstName", ticketNotification.UserId);
            return View(ticketNotification);
        }

        // POST: TicketNotifications/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,TicketId,UserId,Type,Created,Method,HasBeenRead,UsedEmail,UsedText")] TN ticketNotification)
        {
            if (ModelState.IsValid)
            {
                db.Entry(ticketNotification).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.UserModel = ProjectsHelper.LoadUserModel();
            ViewBag.TicketId = new SelectList(db.Tickets, "Id", "Title", ticketNotification.TicketId);
            ViewBag.UserId = new SelectList(db.Users, "Id", "FirstName", ticketNotification.UserId);
            return View(ticketNotification);
        }

        // GET: TicketNotifications/Delete/5
        public ActionResult Delete(int? id)
        {
            ViewBag.UserModel = ProjectsHelper.LoadUserModel();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TN ticketNotification = db.TicketNotifications.Find(id);
            if (ticketNotification == null)
            {
                return HttpNotFound();
            }
            return View(ticketNotification);
        }

        // POST: TicketNotifications/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            TN ticketNotification = db.TicketNotifications.Find(id);
            db.TicketNotifications.Remove(ticketNotification);
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

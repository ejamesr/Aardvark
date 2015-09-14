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
            ViewBag.Scope = scope ?? "My";

            // First, get this user...
            UserRolesHelper helper = new UserRolesHelper();
            var userId = helper.GetCurrentUserId();

            switch (scope) {
                case "My":
                    // Now get My notifications...
                    var My = db.TicketNotifications.Where(n => n.UserId == userId)
                        .OrderBy(n => n.HasBeenRead).ThenByDescending(n => n.Created);
                    return View(My.ToList());
                case "All":
                    var All = db.TicketNotifications.Include(t => t.Ticket).Include(t => t.User)
                        .OrderBy(n => n.HasBeenRead).ThenByDescending(n => n.Created);
                    return View(All.ToList());
                case "MyAssignedToTicket":
                    var MyAssignedToTicket = db.TicketNotifications
                        .Where(n => n.UserId == userId 
                            && n.Type == TicketNotification.EType.AssignedToTicket);
                    return View(MyAssignedToTicket.ToList());
                case "MyRemovedFromTicket":
                    var MyRemovedFromTicket = db.TicketNotifications
                        .Where(n => n.UserId == userId 
                            && n.Type == TicketNotification.EType.RemovedFromTicket);
                    return View(MyRemovedFromTicket.ToList());
                case "MyTicketModified":
                    var MyTicketModified = db.TicketNotifications
                        .Where(n => n.UserId == userId 
                            && n.Type == TicketNotification.EType.TicketModified);
                    return View(MyTicketModified.ToList());
                case "MyComments":
                    var MyComments = db.TicketNotifications
                        .Where(n => n.UserId == userId 
                            && (n.Type == TicketNotification.EType.CommentCreated
                                || n.Type == TicketNotification.EType.CommentDeleted
                                || n.Type == TicketNotification.EType.CommentModified));
                    return View(MyComments.ToList());
                case "MyAttachments":
                    var MyAttachments = db.TicketNotifications
                        .Where(n => n.UserId == userId 
                            && (n.Type == TicketNotification.EType.AttachmentCreated
                                || n.Type == TicketNotification.EType.AttachmentDeleted
                                || n.Type == TicketNotification.EType.AttachmentModified));
                    return View(MyAttachments.ToList());
                default:
                    // A normal type without "My" in front
                    TicketNotification.EType type = TicketNotification.EType.AssignedToTicket;
                    bool gotIt = Enum.TryParse<TicketNotification.EType>(scope, out type);
                    if (gotIt)
                    {
                        var ticketNotifications = db.TicketNotifications.Where(n => n.Type == type);
                        return View(ticketNotifications.ToList());
                    }
                    else
                    {
                        var noneHere = db.TicketNotifications.Where(n => n.TicketId == -1);
                        return View(noneHere.ToList());
                    }
            }
        }

        // GET: TicketNotifications/Details/5
        public ActionResult Details(int? id)
        {
            ViewBag.UserModel = ProjectsHelper.LoadUserModel();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TicketNotification tn = db.TicketNotifications.Find(id);
            if (tn == null)
            {
                return HttpNotFound();
            }
            return View(tn);
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
        public ActionResult Create([Bind(Include = "Id,TicketId,UserId,Type,Created,Method,HasBeenRead,UsedEmail,UsedText")] TicketNotification ticketNotification)
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
            TicketNotification tn = db.TicketNotifications.Find(id);
            if (tn == null)
            {
                return HttpNotFound();
            }
            ViewBag.TicketId = new SelectList(db.Tickets, "Id", "Title", tn.TicketId);
            ViewBag.UserId = new SelectList(db.Users, "Id", "FirstName", tn.UserId);
            return View(tn);
        }

        // POST: TicketNotifications/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,TicketId,UserId,Type,Created,Method,HasBeenRead,UsedEmail,UsedText")] TicketNotification ticketNotification)
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
            TicketNotification ticketNotification = db.TicketNotifications.Find(id);
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
            TicketNotification ticketNotification = db.TicketNotifications.Find(id);
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



using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Aardvark.Models;
using System.IO;

namespace Aardvark.Controllers
{
    public class AttachmentsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Attachments -- 'id' is the Ticket.Id
        public ActionResult Index(int? id)
        {
            if (id == null)
                return View();

            // id is valid...
            var ticket = db.Tickets.Find(id);
            ViewBag.Ticket = ticket;
            return View(ticket.Attachments.ToList());
            //var ticketAttachments = db.TicketAttachments.Include(t => t.Ticket).Include(t => t.User);
            //return View(ticketAttachments.ToList());
        }

        // GET: Attachments/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TicketAttachment ticketAttachment = db.TicketAttachments.Find(id);
            if (ticketAttachment == null)
            {
                return HttpNotFound();
            }
            return View(ticketAttachment);
        }

        // GET: Attachments/Create
        public ActionResult Create(int? id)
        {
            if (id == null)
                return RedirectToAction("Index");

            TicketAttachment ta = new TicketAttachment();
            ta.UserId = new Helpers.UserRolesHelper().GetCurrentUserId();
            ta.TicketId = (int)id;
            return View(ta);
        }

        // POST: Attachments/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "TicketId,FilePath,Description,UserId,FileUrl")] TicketAttachment ticketAttachment, HttpPostedFileBase fileUpload)
        {
            if (ModelState.IsValid)
            {
                // restrict the valid file formats to images only
                if (fileUpload != null && fileUpload.ContentLength > 0)
                {
                    // Don't need to restrict to images only, allow ANY file to be uploaded
                    //if (!fileUpload.ContentType.Contains("image"))
                    //{
                    //    return new HttpStatusCodeResult(HttpStatusCode.UnsupportedMediaType);
                    //}
                    var fileName = Path.GetFileName(fileUpload.FileName);
                    var p = Path.Combine(Server.MapPath("~/attachments/"), fileName);
                    fileUpload.SaveAs(p);
                    ticketAttachment.FilePath = "~/attachments/" + fileName;
                }

                ticketAttachment.Created = DateTimeOffset.UtcNow;
                db.TicketAttachments.Add(ticketAttachment);
                db.SaveChanges();
                TicketNotification.Notify(db, ticketAttachment.Ticket, 
                    ticketAttachment.Created, Notifications.AttachmentCreated);
                return RedirectToAction("Index", new { id = @ticketAttachment.TicketId });
            }

            ViewBag.TicketId = new SelectList(db.Tickets, "Id", "Title", ticketAttachment.TicketId);
            ViewBag.UserId = new SelectList(db.Users, "Id", "FirstName", ticketAttachment.UserId);
            return View(ticketAttachment);
        }

        // GET: Attachments/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TicketAttachment ticketAttachment = db.TicketAttachments.Find(id);
            if (ticketAttachment == null)
            {
                return HttpNotFound();
            }
            ViewBag.TicketId = new SelectList(db.Tickets, "Id", "Title", ticketAttachment.TicketId);
            ViewBag.UserId = new SelectList(db.Users, "Id", "FirstName", ticketAttachment.UserId);
            return View(ticketAttachment);
        }

        // POST: Attachments/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,TicketId,FilePath,Description,Created,UserId,FileUrl")] TicketAttachment ticketAttachment)
        {
            if (ModelState.IsValid)
            {
                db.Entry(ticketAttachment).State = EntityState.Modified;
                db.SaveChanges();
                TicketNotification.Notify(db, ticketAttachment.Ticket,
                    DateTimeOffset.UtcNow, Notifications.AttachmentEdited);  // No Updated date to pull, so get current
                return RedirectToAction("Index");
            }
            ViewBag.TicketId = new SelectList(db.Tickets, "Id", "Title", ticketAttachment.TicketId);
            ViewBag.UserId = new SelectList(db.Users, "Id", "FirstName", ticketAttachment.UserId);
            return View(ticketAttachment);
        }

        // GET: Attachments/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TicketAttachment ticketAttachment = db.TicketAttachments.Find(id);
            if (ticketAttachment == null)
            {
                return HttpNotFound();
            }
            return View(ticketAttachment);
        }

        // POST: Attachments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            TicketAttachment ticketAttachment = db.TicketAttachments.Find(id);
            TicketNotification.Notify(db, ticketAttachment.Ticket,
                ticketAttachment.Created, Notifications.AttachmentDeleted);
            db.TicketAttachments.Remove(ticketAttachment);
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

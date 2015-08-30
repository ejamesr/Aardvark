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
using System.IO;

namespace Aardvark.Controllers
{
    public class AttachmentsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private const string BaseFilePath = "~/attachments/";

        // GET: Attachments -- 'id' is the Ticket.Id
        public ActionResult Index(int? id)
        {
            // Do this in every GET action...
            ViewBag.UserModel = ProjectsHelper.LoadUserModel();
            if (id == null)
            {
                return View();
            }

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
            // Do this in every GET action...
            ViewBag.UserModel = ProjectsHelper.LoadUserModel();
            return View(ticketAttachment);
        }

        // GET: Attachments/Dowload/5
        public ActionResult Download(int? id)
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

            // Now, download this item!
            // Get the full path first...
            var fpr = new FilePathResult(ticketAttachment.FilePath, ticketAttachment.ContentType);
            //Response.ContentType = ticketAttachment.ContentType;
            //Response.AddHeader("Content-Disposition", "inline");
            var fName = Path.GetFileName(ticketAttachment.FilePath);
            fpr.FileDownloadName = fName;
            return fpr;

            //// Do this in every GET action...
            //ViewBag.UserModel = ProjectsHelper.LoadUserModel();
            //return RedirectToAction("Index", new { Id = ticketAttachment.TicketId });
        }

        // GET: Attachments/Create
        public ActionResult Create(int? id)
        {
            if (id == null)
                return RedirectToAction("Index");

            // Do this in every GET action...
            ViewBag.UserModel = ProjectsHelper.LoadUserModel();
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
            // One of 'fileUpload' or ticketAttachment.fileUrl must be non-null, else show error
            bool valid = true;
            if (fileUpload == null && string.IsNullOrEmpty(ticketAttachment.FileUrl))
            {
                ViewBag.ErrMsg = "You select either a file to upload, or a Url path.";
                valid = false;
            }
            if (ModelState.IsValid && valid)
            {
                // restrict the valid file formats to images only
                if (fileUpload != null && fileUpload.ContentLength > 0)
                {
                    // Don't need to restrict to images only, allow ANY file to be uploaded
                    var fileName = Path.GetFileName(fileUpload.FileName);
                    var p = Path.Combine(Server.MapPath(BaseFilePath), fileName);
                    fileUpload.SaveAs(p);
                    ticketAttachment.FilePath = BaseFilePath + fileName;
                    ticketAttachment.ContentType = fileUpload.ContentType;
                    ticketAttachment.ContentLength = fileUpload.ContentLength;
                }

                ticketAttachment.Created = DateTimeOffset.UtcNow;
                db.TicketAttachments.Add(ticketAttachment);
                db.SaveChanges();
                TicketNotification.Notify(db, ticketAttachment.TicketId, 
                    ticketAttachment.Created, Notifications.AttachmentCreated);
                return RedirectToAction("Index", new { id = @ticketAttachment.TicketId });
            }

            // Do this in every GET action...
            ViewBag.UserModel = ProjectsHelper.LoadUserModel();
            ViewBag.TicketId = new SelectList(db.Tickets, "Id", "Title", ticketAttachment.TicketId);
            ViewBag.UserId = new SelectList(db.Users, "Id", "FirstName", ticketAttachment.UserId);
            return View(ticketAttachment);
        }

        // GET: Attachments/Edit/5
        public ActionResult Edit(int? id)
        {
            // Do this in every GET action...
            ViewBag.UserModel = ProjectsHelper.LoadUserModel();
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
            //ViewBag.UserId = new SelectList(db.Users, "Id", "FirstName", ticketAttachment.UserId);
            return View(ticketAttachment);
        }

        // POST: Attachments/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,TicketId,FilePath,Description,Created,UserId,FileUrl")] TicketAttachment ticketAttachment)
        {
            // Do this in every GET action...
            ViewBag.UserModel = ProjectsHelper.LoadUserModel();
            if (ModelState.IsValid)
            {
                db.Entry(ticketAttachment).State = EntityState.Modified;
                db.SaveChanges();
                TicketNotification.Notify(db, ticketAttachment.TicketId,
                    DateTimeOffset.UtcNow, Notifications.AttachmentModified);  // No Updated date to pull, so get current
                return RedirectToAction("Index");
            }
            ViewBag.TicketId = new SelectList(db.Tickets, "Id", "Title", ticketAttachment.TicketId);
            ViewBag.UserId = new SelectList(db.Users, "Id", "FirstName", ticketAttachment.UserId);
            return View(ticketAttachment);
        }

        // GET: Attachments/Delete/5
        public ActionResult Delete(int? id)
        {
            // Do this in every GET action...
            ViewBag.UserModel = ProjectsHelper.LoadUserModel();
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
            TicketNotification.Notify(db, ticketAttachment.TicketId,
                ticketAttachment.Created, Notifications.AttachmentDeleted);
            db.TicketAttachments.Remove(ticketAttachment);
            db.SaveChanges();
            // Do this in every GET action...
            ViewBag.UserModel = ProjectsHelper.LoadUserModel();
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

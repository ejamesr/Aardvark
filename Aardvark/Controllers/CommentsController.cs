using Aardvark.Helpers;
using Aardvark.Models;
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
    public class CommentsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Comments
        public ActionResult Index()
        {
            // Do this in every GET action...
            ViewBag.UserModel = ProjectsHelper.LoadUserModel();
            var comments = db.TicketComments.Include(c => c.User);
            return View(comments.ToList());
        }

        // GET: Comments/Details/5
        public ActionResult Details(int? id)
        {
            // Do this in every GET action...
            ViewBag.UserModel = ProjectsHelper.LoadUserModel();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TicketComment comment = db.TicketComments.Find(id);
            if (comment == null)
            {
                return HttpNotFound();
            }
            return View(comment);
        }

        // GET: Comments/Create
        [Authorize]
        public ActionResult Create(int? id, string anchor, int? page, int? cid = null)    // 
        {
            // Do this in every GET action...
            ViewBag.UserModel = ProjectsHelper.LoadUserModel();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Creating comment for main ticket
            ViewBag.id = (int)id;
            ViewBag.anchor = anchor;
            ViewBag.page = page;

            TicketComment newComment = new TicketComment();
            var user = new UserRolesHelper().GetCurrentUser();
            newComment.UserId = user.Id;
            newComment.DisplayName = user.DisplayName != "" ? user.DisplayName : user.UserName;

            if (cid == null)
            {
                newComment.ParentCommentId = null;
                newComment.TicketId = (int)id;
                newComment.Level = 0;
            }
            else
            {
                TicketComment oldComment = db.TicketComments.Find(cid);
                newComment.ParentCommentId = oldComment.Id;
                newComment.TicketId = oldComment.TicketId;
                newComment.Level = oldComment.Level + 1 ?? 1;
            }
            ViewBag.Ticket = db.Tickets.Find(newComment.TicketId);
            ViewBag.Parent = db.TicketComments.Find(newComment.ParentCommentId);
            return View(newComment);
        }

        // POST: Comments/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "TicketId,DisplayName,UserId,ParentCommentId,Level,Body")] TicketComment comment,
            int? id, int? page, string anchor)
        {
            if (ModelState.IsValid)
            {
                comment.SetCreated();
                comment.TicketId = (int)id;
                if (comment.DisplayName == "")
                    comment.DisplayName = "(no name)";
                db.TicketComments.Add(comment);
                db.SaveChanges();
                TicketNotification.Notify(db, comment.TicketId,
                    comment.Created, TicketNotification.EType.CommentCreated);

                                //            @Html.ActionLink("Details", "Details", "Posts", null, null,
                                //anchor, new { id = ViewBag.id, page = ViewBag.page }, null)


                var route = new System.Web.Routing.RouteValueDictionary();
                route.Add("id", id);
                route.Add("page", page);
                route.Add("anchor", "#"+anchor);

                // The next line keeps failing, work on it later
                // var url = UrlHelper.GenerateUrl("", "Details", "Posts","http","",anchor,
                //  route, null,Url.RequestContext, false);
                // return new RedirectResult(url);

                return RedirectToAction("Details", "Tickets", route);
            }
            // Do this in every GET action...
            ViewBag.UserModel = ProjectsHelper.LoadUserModel();
            return View(comment);
        }

        // GET: Comments/Edit/5
        [Authorize(Roles="Admin, Moderator")]
        public ActionResult Edit(int? id, string anchor, int page, int? cid = null)
        {
            // Do this in every GET action...
            ViewBag.UserModel = ProjectsHelper.LoadUserModel();
            if (id == null || cid == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            // Creating comment for main post
            ViewBag.id = (int)id;
            ViewBag.anchor = anchor;
            ViewBag.page = page;

            TicketComment comment = db.TicketComments.Find(cid);
            if (comment == null)
            {
                return HttpNotFound();
            }
            return View(comment);
        }

        // POST: Comments/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin, Moderator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Body,DisplayName,UpdateReason")] TicketComment comment,
            int? id, string anchor, int page, int? cid = null)
        {
            // Do this in every GET action...
            ViewBag.UserModel = ProjectsHelper.LoadUserModel();
            if (cid == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var route = new System.Web.Routing.RouteValueDictionary();
            route.Add("id", id);
            route.Add("page", page);
            route.Add("anchor", "#" + anchor);

            if (ModelState.IsValid)
            {
                TicketComment orig = db.TicketComments.Find(cid);
                orig.Body = comment.Body;
                orig.Updated = DateTime.UtcNow;
                db.Entry(orig).State = EntityState.Modified;
                db.SaveChanges();
                TicketNotification.Notify(db, orig.TicketId,
                    orig.Updated.Value, TicketNotification.EType.CommentModified);
                return RedirectToAction("Details", "Posts", route);
            }

            return RedirectToAction("Details", "Posts", route);
        }

        // GET: Comments/Delete/5
        [Authorize(Roles = "Admin, Moderator")]
        public ActionResult Delete(int? id, string anchor, int page, int? cid = null)
        {
            // Do this in every GET action...
            ViewBag.UserModel = ProjectsHelper.LoadUserModel();
            if (id == null || cid == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ViewBag.id = (int)id;
            ViewBag.anchor = anchor;
            ViewBag.page = page;
            TicketComment comment = db.TicketComments.Find(cid);
            if (comment == null)
            {
                return HttpNotFound();
            }
            return View(comment);
        }

        // POST: Comments/Delete/5
        //[HttpPost, ActionName("Delete")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Moderator")]
        public ActionResult DeleteConfirmed(int? id, string anchor, int page, int? cid = null)
        {
            // Want to delete the comment (cid is its id, NOT id!)
            TicketComment comment = db.TicketComments.Find(cid);
            comment.SetUpdated();
            comment.Deleted = true;
            //db.TicketComments.Remove(comment);
            db.SaveChanges();
            TicketNotification.Notify(db, comment.TicketId,
                comment.Updated.Value, TicketNotification.EType.CommentDeleted);

            var route = new System.Web.Routing.RouteValueDictionary();
            route.Add("id", id);
            route.Add("page", page);
            route.Add("anchor", "#" + anchor);

            // The next line keeps failing, work on it later
            // var url = UrlHelper.GenerateUrl("", "Details", "Posts","http","",anchor,
            //  route, null,Url.RequestContext, false);
            // return new RedirectResult(url);

            return RedirectToAction("Details", "Posts", route);
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

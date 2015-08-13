using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Windows;
using Aardvark.Models;
using System.Text;

namespace Aardvark.Controllers
{
    public class UtilitiesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Utilities/ImportData
        [Authorize(Roles="Admin,Guest")]
        public ActionResult ImportData()
        {
            return View();
        }


        public class Section
        {
            private static string[] UserHeadings = { "FirstName", "LastName", "UserName", "DisplayName", "Email", "Phone", "RoleAdmin", "RoleProjectManager", "RoleDeveloper", "RoleSubmitter" };
            private static string[] ProjectHeadings = { "ProjectName", "Description", "ProjectManagerUserName" };
            private static string[] ProjDevHeadings = { "ProjectName", "DeveloperUserName" };
            private static string[] TicketHeadings = { "Title", "Description", "DateCreated", "ProjectName", "TicketCreatorUserName", "AssignedDeveloperUserName", "DueDate", "HoursToComplete", "TicketType", "TicketPriority", "TicketStatus", "SkillRequired" };

            public string Target;       // The type this is updating
            public int Start;
            public int Count;
            public string[] Headings;
            public int nHeadings;
            public int[] HeadingOffsets;

            public static int FindPos(string src, string[] strings){
                // Return -1 if not found, else offset in string array
                for (int i = 0; i < strings.Length; i++) {
                    if (src.Equals(strings[i]))
                        return i;
                }

                // Not found...
                return -1;
            }
            
            public static Section Init(string[] src, string target)
            {
                // Find
                Section section = new Section();
                int counter = 0;
                foreach (string s in src)
                {
                    if (s.StartsWith(target))
                        break;
                    counter++;
                }
                if (counter >= src.Length)
                    return null;        // Section was not there, or target was invalid

                // Found start of section, now find headings (first non-commented-out row)
                do
                {
                    if (++counter >= src.Length)
                        return null;
                } while (src[counter].StartsWith(";"));
                if (src[counter].StartsWith("#"))
                    return null;

                // Found headings...
                section.Headings = src[counter].Split('\t');
                section.HeadingOffsets = new int[section.Headings.Length];
                section.Start = ++counter;

                // Now count # rows...
                while (counter < src.Length && src[counter][0] != '#'){
                    counter++;
                }
                section.Count = counter - section.Start;

                // Now process column headings...
                string[] ptrHeadings = null;
                switch (target)
                {
                    case "#Users":
                        ptrHeadings = UserHeadings;
                        break;
                    case "#Projects":
                        ptrHeadings = ProjectHeadings;
                        break;
                    case "#ProjectDevelopers":
                        ptrHeadings = ProjDevHeadings;
                        break;
                    case "#Tickets":
                        ptrHeadings = TicketHeadings;
                        break;
                }
                int i, pos;
                for (i = 0; i < section.Headings.Length; i++)
                {
                    if (section.Headings[i] == "")
                        break;
                    pos = FindPos(section.Headings[i], ptrHeadings);
                    if (pos < 0)
                        return null;
                    section.HeadingOffsets[i] = pos;
                }
                section.nHeadings = i;
                return section;
            }

            public static void LogErr(ApplicationDbContext db, string msg){
                Log log = new Log("Error during DataImport", msg);
                db.Logs.Add(log);
                db.SaveChanges();
            }
            public static void LogSuccess(ApplicationDbContext db, string msg)
            {
                Log log = new Log("DataImport was successful", msg);
                db.Logs.Add(log);
                db.SaveChanges();
            }

            public static bool ProcessSection(ApplicationDbContext db, Section section)
            {



                return false;
            }
        }


        // POST: Utilities/ImportData
        [HttpPost]
        [Authorize(Roles="Admin,Guest")]
        public ActionResult ImportData(string DataToImport)
        {
            StringBuilder sb = new StringBuilder(DataToImport.Length);
            bool err = false;
            // First, strip out all quotes and CR chars
            for (int i = 0; i < DataToImport.Length; i++) {
                char c = DataToImport[i];
                if (c != '"' && c != '\r')
                    sb.Append(c);
            }
            var list = sb.ToString().Split('\n'); 

            // 'list' has the lines just as we need them. Now find the main sections...
            Section users = Section.Init(list, "#Users");
            Section projects = Section.Init(list, "#Projects");
            Section projectDevs = Section.Init(list, "#ProjectDevelopers");
            Section tickets = Section.Init(list, "#Tickets");
             
            // Make sure each section is good...
            if (users == null)
            {
                err = true;
                Section.LogErr(db, "#Users section incorrect form");
            }
            if (projects == null)
            {
                err = true;
                Section.LogErr(db, "#Projects section incorrect form");
            }
            if (projectDevs == null)
            {
                err = true;
                Section.LogErr(db, "#ProjectDevelopers section incorrect form");
            }
            if (tickets == null)
            {
                err = true;
                Section.LogErr(db, "#Tickets section incorrect form");
            }

            // If no error so far, then process each group...
            if (!err)
            {
                // Process Users...
                if (!Section.ProcessSection(db, users))
                    err = true;
                if (!Section.ProcessSection(db, projects))
                    err = true;
                if (!Section.ProcessSection(db, projectDevs))
                    err = true;
                if (!Section.ProcessSection(db, tickets))
                    err = true;
            }

            // OK, decide what to do now -- which View should we go to?

            
            return View();
        }

        // GET: Utilities
        public ActionResult Index()
        {
            return View(db.Logs.ToList());
        }

        // GET: Utilities/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Log log = db.Logs.Find(id);
            if (log == null)
            {
                return HttpNotFound();
            }
            return View(log);
        }

        // GET: Utilities/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Utilities/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Created,Name,Msg")] Log log)
        {
            if (ModelState.IsValid)
            {
                db.Logs.Add(log);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(log);
        }

        // GET: Utilities/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Log log = db.Logs.Find(id);
            if (log == null)
            {
                return HttpNotFound();
            }
            return View(log);
        }

        // POST: Utilities/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Created,Name,Msg")] Log log)
        {
            if (ModelState.IsValid)
            {
                db.Entry(log).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(log);
        }

        // GET: Utilities/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Log log = db.Logs.Find(id);
            if (log == null)
            {
                return HttpNotFound();
            }
            return View(log);
        }

        // POST: Utilities/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Log log = db.Logs.Find(id);
            db.Logs.Remove(log);
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

using SendGrid;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Aardvark.Controllers
{
    public class EmailController : Controller
    {
        // GET: Email
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Email()
        {
            ViewBag.Title = "Email Test";
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Email(string emailTo, string emailFrom, string subject, string body)
        {
            var appSettings = ConfigurationManager.AppSettings;

            var credentials = new NetworkCredential(appSettings["SendGridUserName"], appSettings["SendGridUserPassword"]);

            SendGridMessage mymessage = new SendGridMessage();
            mymessage.AddTo(emailTo);
            mymessage.From = new MailAddress(emailFrom, "Eric Ruff");
            mymessage.Subject = subject;
            mymessage.Text = body;

            var transportWeb = new Web(credentials);
            await transportWeb.DeliverAsync(mymessage);
            return RedirectToAction("EmailSent", "Home", null);
        }
    }
}
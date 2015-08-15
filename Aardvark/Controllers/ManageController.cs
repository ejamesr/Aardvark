using Aardvark.Helpers;
using Aardvark.Models;
using Aardvark.ViewModels;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System.Web.Security;
using System.Security.Principal;

namespace Aardvark.Controllers
{
    [Authorize]
    public class ManageController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public ManageController()
        {
        }

        public ManageController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set 
            { 
                _signInManager = value; 
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //
        // GET: /Manage/Index
        public async Task<ActionResult> Index(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.SetTwoFactorSuccess ? "Your two-factor authentication provider has been set."
                : message == ManageMessageId.Error ? "An error has occurred."
                : message == ManageMessageId.AddPhoneSuccess ? "Your phone number was added."
                : message == ManageMessageId.RemovePhoneSuccess ? "Your phone number was removed."
                : message == ManageMessageId.ChangeDisplayNameSuccess ? "Your display name has been changed."
                : message == ManageMessageId.ChangeUserNameSuccess ? "Your user name has been changed."
                : message == ManageMessageId.ChangeUserNameAlreadyTaken ? "Could not change - that UserName already exists."
                : message == ManageMessageId.ChangeTxtMsgNumberSuccess ? "Your text-message number has been changed."
                : message == ManageMessageId.SetDisplayNameSuccess ? "Your display name has been set."
                : message == ManageMessageId.SetUserNameSuccess ? "Your user name has been set."
                : message == ManageMessageId.SetTxtMsgNumberSuccess ? "Your text-message number has been set."
                : "";

            var userId = User.Identity.GetUserId();
            var model = new IndexViewModel
            { 
                DisplayName = GetDisplayName(),
                UserName = GetUserName(),
                TextMsgNumber = GetTextMsgNumber(),
                HasPassword = HasPassword(),
                PhoneNumber = await UserManager.GetPhoneNumberAsync(userId),
                TwoFactor = await UserManager.GetTwoFactorEnabledAsync(userId),
                Logins = await UserManager.GetLoginsAsync(userId),
                BrowserRemembered = await AuthenticationManager.TwoFactorBrowserRememberedAsync(userId)
            };
            return View(model);
        }

        //
        // POST: /Manage/RemoveLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveLogin(string loginProvider, string providerKey)
        {
            ManageMessageId? message;
            var result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(), new UserLoginInfo(loginProvider, providerKey));
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                message = ManageMessageId.RemoveLoginSuccess;
            }
            else
            {
                message = ManageMessageId.Error;
            }
            return RedirectToAction("ManageLogins", new { Message = message });
        }

        //
        // GET: /Manage/AddPhoneNumber
        public ActionResult AddPhoneNumber()
        {
            return View();
        }

        //
        // GET: /Manage/ChangeDisplayName
        public ActionResult ChangeDisplayName()
        {
            return View();
        }

        //
        // GET: /Manage/ChangeUserName
        public ActionResult ChangeUserName()
        {
            ApplicationDbContext db = new ApplicationDbContext();
            var a = User;
            var i = a.Identity;
            var n = i.GetUserName();
            var userRec = db.Users.Find(User.Identity.GetUserId());
            if (userRec != null)
                return View(new ChangeSetNameViewModel(userRec, "User Name"));

            // user is null, so return error
            return RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
        }

        //
        // POST: /Manage/ChangeUserName
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangeUserName(ChangeSetNameViewModel model)
        {
            // Update db, then go back to index
            // The model contains the full record, so just update it now
            ApplicationDbContext db = new ApplicationDbContext();
            var userRec = db.Users.Find(User.Identity.GetUserId());
            if (userRec != null)
            {
                // See if the new UserName has not yet been spoken for
                var alreadyThere = db.Users.FirstOrDefault(u => u.UserName == model.UserRec.UserName);
                if (alreadyThere != null)
                {
                    // This name is already taken, so don't try to save it!
                    return RedirectToAction("Index", new { Message = ManageMessageId.ChangeUserNameAlreadyTaken });
                }
                // Update the field and then save
                userRec.UserName = model.UserRec.UserName;
                // Let other processes know this has changed...
                db.Entry(userRec).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                // Something like this will signout the user... but it's not currently working.
                AuthenticationManager.SignOut();
                //FormsAuthentication.SignOut();
                //Response.Cookies[FormsAuthentication.FormsCookieName].Expires = DateTime.Now.AddYears(-1); 
                //HttpContext.User = null;// new GenericPrincipal(new GenericIdentity(string.Empty), null);

                //Roles.DeleteCookie();
                //Session.Clear();
                return RedirectToAction("Index", new { Message = ManageMessageId.ChangeUserNameSuccess });
            }
            return Redirect("Index");
        }

        //
        // GET: /Manage/ChangeTxtMsgName
        public ActionResult ChangeTxtMsgName()
        {
            return View();
        }

        //
        // GET: /Manage/Profile
        public ActionResult UserProfile()
        {
            ApplicationDbContext db = new ApplicationDbContext();
            var user = db.Users.Find(User.Identity.GetUserId());
            if (user != null)
            {
                ProfileView profile = new ProfileView();
                profile.Id = user.Id;
                profile.FirstName = user.FirstName;
                profile.LastName = user.LastName;
                profile.DisplayName = user.DisplayName;
                profile.UserName = user.UserName;
                profile.Email = user.Email;
                profile.Phone = user.PhoneNumber;
                profile.NotifyByEmail = user.EmailNotification;
                profile.NotifyByText = user.TextNotification;
                profile.TextMsgNumber = user.TextMsgNumber;

                // Do this in every action prior to view...
                ViewBag.UserModel = ProjectsHelper.LoadUserModel();

                return View(profile);
            }
            return View(user);
        }

                //
        // POST: /Manage/Profile
        [HttpPost]
        public ActionResult UserProfile(ProfileView profile)
        {

            if (!ModelState.IsValid)
            {
                // Do this in every action prior to view...
                ViewBag.UserModel = ProjectsHelper.LoadUserModel();

                // Need to reset the fields that could be null
                profile.NotifyByText = profile.NotifyByText ?? false;
                profile.NotifyByEmail = profile.NotifyByEmail ?? false;
                return View(profile);
            }

            // Values are good, need to take care with nullable items
            ApplicationDbContext db = new ApplicationDbContext();
            var user = db.Users.Find(profile.Id);

            user.FirstName = profile.FirstName;
            user.LastName = profile.LastName;
            user.DisplayName = profile.DisplayName;
            user.UserName = profile.UserName;
            user.Email = profile.Email;
            user.PhoneNumber = profile.Phone;
            user.TextMsgNumber = profile.SameAsPhone == null ? profile.TextMsgNumber : profile.Phone;
            user.EmailNotification = profile.NotifyByEmail ?? false;
            user.TextNotification = profile.NotifyByText ?? false;
            db.Entry(user).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index", "Home");
        } 

        //
        // GET: /Manage/Settings
        public ActionResult Settings()
        {
            return View();
        }

        //
        // POST: /Manage/AddPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddPhoneNumber(AddPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            // Generate the token and send it
            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), model.Number);
            if (UserManager.SmsService != null)
            {
                var message = new IdentityMessage
                {
                    Destination = model.Number,
                    Body = "Your security code is: " + code
                };
                await UserManager.SmsService.SendAsync(message);
            }
            return RedirectToAction("VerifyPhoneNumber", new { PhoneNumber = model.Number });
        }

        //
        // POST: /Manage/EnableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EnableTwoFactorAuthentication()
        {
            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), true);
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", "Manage");
        }

        //
        // POST: /Manage/DisableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DisableTwoFactorAuthentication()
        {
            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), false);
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", "Manage");
        }

        //
        // GET: /Manage/VerifyPhoneNumber
        public async Task<ActionResult> VerifyPhoneNumber(string phoneNumber)
        {
            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), phoneNumber);
            // Send an SMS through the SMS provider to verify the phone number
            return phoneNumber == null ? View("Error") : View(new VerifyPhoneNumberViewModel { PhoneNumber = phoneNumber });
        }

        //
        // POST: /Manage/VerifyPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyPhoneNumber(VerifyPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePhoneNumberAsync(User.Identity.GetUserId(), model.PhoneNumber, model.Code);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.AddPhoneSuccess });
            }
            // If we got this far, something failed, redisplay form
            ModelState.AddModelError("", "Failed to verify phone");
            return View(model);
        }

        //
        // GET: /Manage/RemovePhoneNumber
        public async Task<ActionResult> RemovePhoneNumber()
        {
            var result = await UserManager.SetPhoneNumberAsync(User.Identity.GetUserId(), null);
            if (!result.Succeeded)
            {
                return RedirectToAction("Index", new { Message = ManageMessageId.Error });
            }
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", new { Message = ManageMessageId.RemovePhoneSuccess });
        }

        //
        // GET: /Manage/ChangePassword
        public ActionResult ChangePassword()
        {
            // Do this in every action prior to view...
            ViewBag.UserModel = ProjectsHelper.LoadUserModel();
            return View();
        }

        //
        // POST: /Manage/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Do this in every action prior to view...
                ViewBag.UserModel = ProjectsHelper.LoadUserModel();
                return View(model);
            }
            var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.ChangePasswordSuccess });
            }
            AddErrors(result);
            // Do this in every action prior to view...
            ViewBag.UserModel = ProjectsHelper.LoadUserModel();
            return View(model);
        }

        //
        // GET: /Manage/SetPassword
        public ActionResult SetPassword()
        {
            return View();
        }

        //
        // POST: /Manage/SetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SetPassword(SetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);
                if (result.Succeeded)
                {
                    var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                    if (user != null)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    }
                    return RedirectToAction("Index", new { Message = ManageMessageId.SetPasswordSuccess });
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Manage/ManageLogins
        public async Task<ActionResult> ManageLogins(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                : message == ManageMessageId.Error ? "An error has occurred."
                : "";
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user == null)
            {
                return View("Error");
            }
            var userLogins = await UserManager.GetLoginsAsync(User.Identity.GetUserId());
            var otherLogins = AuthenticationManager.GetExternalAuthenticationTypes().Where(auth => userLogins.All(ul => auth.AuthenticationType != ul.LoginProvider)).ToList();
            ViewBag.ShowRemoveButton = user.PasswordHash != null || userLogins.Count > 1;
            return View(new ManageLoginsViewModel
            {
                CurrentLogins = userLogins,
                OtherLogins = otherLogins
            });
        }

        //
        // POST: /Manage/LinkLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LinkLogin(string provider)
        {
            // Request a redirect to the external login provider to link a login for the current user
            return new AccountController.ChallengeResult(provider, Url.Action("LinkLoginCallback", "Manage"), User.Identity.GetUserId());
        }

        //
        // GET: /Manage/LinkLoginCallback
        public async Task<ActionResult> LinkLoginCallback()
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync(XsrfKey, User.Identity.GetUserId());
            if (loginInfo == null)
            {
                return RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
            }
            var result = await UserManager.AddLoginAsync(User.Identity.GetUserId(), loginInfo.Login);
            return result.Succeeded ? RedirectToAction("ManageLogins") : RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }

            base.Dispose(disposing);
        }

#region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private bool HasPassword()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
        }

        private string GetDisplayName()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.DisplayName ?? "";
            }
            return "";
        }

        private string GetUserName()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.UserName ?? "";
            }
            return "";
        }

        private string GetTextMsgNumber()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.TextMsgNumber ?? "";
            }
            return "";
        }

        private bool HasPhoneNumber()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PhoneNumber != null;
            }
            return false;
        }

        public enum ManageMessageId
        {
            AddPhoneSuccess,
            ChangeDisplayNameSuccess,
            ChangeUserNameSuccess,
            ChangeUserNameAlreadyTaken,
            ChangeTxtMsgNumberSuccess,
            ChangePasswordSuccess,
            SetTwoFactorSuccess,
            SetPasswordSuccess,
            SetDisplayNameSuccess,
            SetUserNameSuccess,
            SetTxtMsgNumberSuccess,
            RemoveLoginSuccess,
            RemovePhoneSuccess,
            Error
        }

#endregion
    }
}
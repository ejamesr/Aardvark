using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Aardvark.ViewModels
{
    public class ProfileView
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [Required]
        public string DisplayName { get; set; }
        [Required]
        public string UserName { get; set; }
        public string Phone { get; set; }
        [Required(ErrorMessage="The email address, in proper form (such as Name@Domain.com), is required")]
        [EmailAddress(ErrorMessage="Invalid email address")]
        public string Email { get; set; }
        public string TextMsgNumber { get; set; }
        public Nullable<bool> SameAsPhone { get; set; }
        public Nullable<bool> NotifyByEmail { get; set; }
        public Nullable<bool> NotifyByText { get; set; }
    }

}
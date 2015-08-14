using Aardvark.Helpers;
using Aardvark.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Aardvark.Models
{
    /* Roles to seed:
     * 
     * "Admin",
     * "ProjectManager",
     * "Developer",
     * "Submitter",
     * "Guest/Demo"
     * 
     * */

    //public class TicketCreateModel : Ticket
    //{
    //    [NotMapped]
    //    public SelectList ProjectList { get; set; }
    //    [NotMapped]
    //    public SelectList TypeList { get; set; }
    //    [NotMapped]
    //    public SelectList PriorityList { get; set; }
    //    [NotMapped]
    //    public SelectList StatusList { get; set; }
    //    [NotMapped]
    //    public SelectList AssigneesList { get; set; }
    //    [Display(Name="Skill Level")]
    //    [NotMapped]
    //    public SelectList SkillLevelList { get; set; }
    //    [NotMapped]
    //    public string HighestUserRole { get; set; }


    //    // Try NOT using this model, go to ViewBag instead
    //    public TicketCreateModel()
    //    {
    //        ApplicationDbContext db = new ApplicationDbContext();
    //        UserRolesHelper helper = new UserRolesHelper();
    //        DueDate = DateTimeOffset.UtcNow.AddDays(1);
    //        var id = HttpContext.Current.User.Identity.GetUserId();
    //        var x = helper.IsUserInRole(id, R.Admin);
    //        HighestUserRole = helper.GetHighestRole(id);
    //        TypeList = new SelectList(db.TicketTypes, "Id", "Name");
    //        PriorityList = new SelectList(db.TicketPriorities, "Id", "Name");
    //        StatusList = new SelectList(db.TicketStatuses, "Id", "Name");
    //        SkillLevelList = new SelectList(db.SkillLevels, "Id", "Name");
    //        ProjectList = new SelectList(db.Projects, "Id", "Name");
    //        HoursToComplete = 1;

    //        if (HighestUserRole == R.Admin || HighestUserRole == R.Guest || HighestUserRole == R.PM)
    //        {
    //            // OK to assign list of users...
    //            var roleDev = db.Roles.FirstOrDefault(r => r.Name == R.Developer);
    //            if (roleDev != null)
    //            {
    //                AssigneesList = new SelectList(
    //                    db.Users
    //                        .Where(d => d.Roles.FirstOrDefault(r => r.RoleId == roleDev.Id) != null),
    //                        "Id", "UserName");
    //                        //.Select(assignee => 
    //                        //    new SelectListItem 
    //                        //    { 
    //                        //        Selected = false,
    //                        //        Text = assignee.UserName,
    //                        //        Value = assignee.Id
    //                        //    }
    //                        //));
    //            }
    //        }
    //        else AssigneesList = null;
    //    }
    //}
}
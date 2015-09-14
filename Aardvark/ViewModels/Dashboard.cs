using Aardvark.Helpers;
using Aardvark.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Aardvark.ViewModels
{
    public class DashboardItem
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public bool Selected { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTimeOffset Date { get; set; }
        public TicketNotification.EType Type { get; set; }
        public string Scope { get; set; }
        public int CountItems { get; set; }
        public int CountHours { get; set; }

        public DashboardItem() { }
        public DashboardItem(string name) { Name = name; }
        public DashboardItem(string name, string scope)
        {
            Name = name;
            Scope = scope;
        }
    }

    public class DashboardModel
    {
        public const int MaxTopProjects = 5;
        public const int MaxTopNewTickets = 5;
        public const int MaxNotifications = 10;

        // For developer
        public int NumProjects { get; set; }
        public int NumActiveTickets { get; set; }
        public int NumNewTickets { get; set; }
        public int NumNewNotifications { get; set; }

        public DashboardItem[] MyProjects = new DashboardItem[MaxTopProjects];
        public DashboardItem[] MyNewTickets = new DashboardItem[MaxTopNewTickets];
        public DashboardItem[] MyNewNotifications = new DashboardItem[MaxNotifications];
        public List<DashboardItem> MyActiveTickets = new List<DashboardItem>();

        public DashboardItem New;
        public DashboardItem Due24Hours;
        public DashboardItem Due7Days;
        public DashboardItem Due30Days;
        public DashboardItem Overdue;
        public DashboardItem InTest;

        public DashboardModel(ProjectsHelper.UserModel userModel, ApplicationDbContext db)
        {
            // Load up the data for display on dashboard
            // For everyone, show Notifications...
            //
            // Get Notifications info...
            //
            var notifies = db.TicketNotifications.Where(n => n.UserId == userModel.User.Id && !n.HasBeenRead);
            NumNewNotifications = notifies.Count();
            if (NumNewNotifications > 0)
            {
                MyNewNotifications = notifies
                    .OrderBy(n => n.Created)
                    .Take(MaxNotifications)
                    .Select(n => new DashboardItem()
                    {
                        Id = n.Id,
                        TicketId = n.TicketId,
                        Type = n.Type,
                        Date = n.Created
                    })
                    .ToArray<DashboardItem>();
                // Now, fill out Description field for each...
                for (int i = 0; i < MyNewNotifications.Length; i++)
                {
                    var notice = db.TicketNotifications.Find(MyNewNotifications[i].Id);
                    MyNewNotifications[i].Description = notice.ToDescription();
                }
            }

            if (userModel.IsAdmin || userModel.IsGuest)
            {
                userModel.DashboardTitle = userModel.IsGuest ? "Dashboard - Guest" : "Dashboard - Admin";
            }
            else if (userModel.IsPM)
            {
                userModel.DashboardTitle = "Dashboard - Project Manager";
            }
            else if (userModel.IsDeveloper)
            {
                userModel.DashboardTitle = "Dashboard - Developer";
                //
                // Get Projects info...
                //
                var projects = db.Users.Find(userModel.User.Id).Projects;
                NumProjects = projects.Count;
                if (NumProjects > 0)
                {
                    MyProjects = db.Users.Find(userModel.User.Id)
                        .Projects.Take(MaxTopProjects)
                        .Select(p => new DashboardItem() { Id = p.Id, Name = p.Name, 
                            CountHours = p.Tickets.Sum(h => h.HoursToComplete),
                            CountItems = p.Tickets.Count})
                        .ToArray<DashboardItem>();
                }

                //
                // Get Tickets info...
                //
                var tickets = db.Tickets
                    .Where(t => t.AssignedToDevId == userModel.User.Id 
                        && t.TicketStatusId != (int)TS.Status.Resolved
                        && t.TicketStatusId >= (int)TS.Status.AssignedToDev);
                if ((NumActiveTickets = tickets.Count()) > 0)
                {
                    // Get new tickets, then setup info on active tickets
                    var newTickets = tickets.Where(t => t.TicketStatusId == (int)TS.Status.AssignedToDev);
                    NumNewTickets = newTickets.Count();
                    MyNewTickets = newTickets
                        .Take(MaxTopNewTickets)
                        .Select(n => new DashboardItem()
                        {
                            Id = n.Id,
                            Name = n.Title,
                            Description = n.TicketType.Name + "-" + n.TicketPriority.Name + "-" + n.SkillRequired.Name,
                            CountHours = n.HoursToComplete
                        })
                        .ToArray<DashboardItem>();

                    // And get details on new tickets
                    DashboardItem di = new DashboardItem("Newly assigned/to pull", "MyNew");
                    if (NumNewTickets > 0)
                    {
                        di.CountItems = NumNewTickets;
                        di.CountHours = newTickets.Sum(t => t.HoursToComplete);
                    }
                    MyActiveTickets.Add(di);

                    // Show total tickets in development
                    di = new DashboardItem("In development", "MyInDevelopment");
                    var inDev = tickets.Where(t => t.TicketStatusId == (int)TS.Status.InDevelopment);
                    if ((di.CountItems = inDev.Count()) > 0)
                        di.CountHours = inDev.Sum(hrs => hrs.HoursToComplete);
                    MyActiveTickets.Add(di);

                    // Get the tickets according to due date/time
                    DateTimeOffset now = DateTimeOffset.UtcNow;
                    DateTimeOffset start, end;
                    start = now.AddDays(-1);
                    di = new DashboardItem("Overdue", "MyOverdue");
                    var due = tickets.Where(t => t.DueDate >= start && t.DueDate < now);
                    if ((di.CountItems = due.Count()) > 0)
                        di.CountHours = due.Sum(t => t.HoursToComplete);
                    MyActiveTickets.Add(di);

                    end = now.AddDays(1);
                    di = new DashboardItem("Due within 24 hours", "MyDue24");
                    due = tickets.Where(t => t.DueDate >= now && t.DueDate < end);
                    if ((di.CountItems = due.Count()) > 0)
                        di.CountHours = due.Sum(t => t.HoursToComplete);
                    MyActiveTickets.Add(di);

                    di = new DashboardItem("Due within 7 days", "MyDue7");
                    end = now.AddDays(7);
                    due = tickets.Where(t => t.DueDate >= now && t.DueDate < end);
                    if ((di.CountItems = due.Count()) > 0)
                        di.CountHours = due.Sum(t => t.HoursToComplete);
                    MyActiveTickets.Add(di);

                    di = new DashboardItem("Due within 30 days", "MyDue30");
                    end = now.AddDays(30);
                    due = tickets.Where(t => t.DueDate >= now && t.DueDate < end);
                    if ((di.CountItems = due.Count()) > 0)
                        di.CountHours = due.Sum(t => t.HoursToComplete);
                    MyActiveTickets.Add(di);

                    // Get number of tickets in test...
                    di = new DashboardItem("Sent to testing", "MyTesting");
                    due = tickets.Where(t => t.TicketStatusId >= (int)TS.Status.ReadyToTest);
                    if ((di.CountItems = due.Count()) > 0)
                        di.CountHours = due.Sum(t => t.HoursToComplete);
                    MyActiveTickets.Add(di);
                }
            }
            else
            {
                // Do this for anybody...
            }

        }
    }
}

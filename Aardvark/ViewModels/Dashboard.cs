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
        public bool Selected { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
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
        public const int MaxTicketTypes = 6;
        public const int MaxNotifications = 5;

        // For developer
        public int NumProjects { get; set; }
        public int NumActiveTickets { get; set; }
        public int NumNewTickets { get; set; }
        public int NumNotifications { get; set; }

        public DashboardItem[] MyProjects = new DashboardItem[MaxTopProjects];
        public DashboardItem[] MyNewTickets = new DashboardItem[MaxTicketTypes];
        public DashboardItem[] MyNotifications = new DashboardItem[MaxNotifications];
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
            if (userModel.IsAdmin || userModel.IsGuest)
            {

            }
            else if (userModel.IsPM)
            {

            }
            else if (userModel.IsDeveloper)
            {
                // Get Projects info...
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

                // Get Tickets info...
                var tickets = db.Tickets
                    .Where(t => t.AssignedToDevId == userModel.User.Id && t.TicketStatusId != (int)TS.Status.Resolved );
                if ((NumActiveTickets = tickets.Count()) > 0)
                {
                    // Get new tickets, then setup info on active tickets
                    var newTickets = tickets.Where(t => t.TicketStatusId == (int)TS.Status.New);
                    NumNewTickets = newTickets.Count();
                    MyNewTickets = newTickets
                        .Take(MaxTopNewTickets)
                        .Select(n => new DashboardItem()
                        {
                            Id = n.Id,
                            Name = n.TicketType.Name + "-" + n.TicketPriority.Name + "-" + n.SkillRequired.Name,
                            CountHours = n.HoursToComplete
                        })
                        .ToArray<DashboardItem>();

                    // And get tickets in various categories
                    DashboardItem di = new DashboardItem("New tickets", "MyNew");
                    di.CountItems = NumNewTickets;
                    di.CountHours = newTickets.Sum(t => t.HoursToComplete);
                    MyActiveTickets.Add(di);

                    // Get the tickets according to due date/time
                    DateTimeOffset now = DateTimeOffset.UtcNow;
                    DateTimeOffset start, end;
                    end = now.AddDays(1);
                    di = new DashboardItem("Due within 24 hours", "MyDue24");
                    var due = tickets.Where(t => t.DueDate >= now && t.DueDate < end);
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

                    start = now.AddDays(-1);
                    di = new DashboardItem("Overdue", "MyOverdue");
                    due = tickets.Where(t => t.DueDate >= start && t.DueDate < now);
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

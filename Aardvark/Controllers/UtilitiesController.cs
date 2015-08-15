using Aardvark.Models;
using Aardvark.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Windows;
using System.Text;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity;

namespace Aardvark.Controllers
{
    public class UtilitiesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Utilities/ImportData
        [Authorize(Roles="Admin,Guest")]
        public ActionResult ImportData()
        {
            // Do this in every GET action...
            ViewBag.UserModel = ProjectsHelper.LoadUserModel();
            ViewBag.Msg = "";
            return View();
        }


        public class Section
        {
            // Enums for dealing with headings
            public enum H : int
            {
                // Enums for importing User 
                U_FirstName = 0,
                U_LastName,
                U_UserName,
                U_DisplayName,
                U_SkillLevel,
                U_Email,
                U_Phone,
                U_RoleAdmin,
                U_RoleProjectManager,
                U_RoleDeveloper,
                U_RoleSubmitter,

                // Enums for importing Projects
                P_ProjectName = 0,
                P_Description,
                P_ProjectManagerUserName,

                // Enums for importing ProjectDevelopers
                PD_ProjectName = 0,
                PD_Description,
                PD_DeveloperUserName,

                // Enums for importing Tickets
                T_Title = 0,
                T_Description,
                T_DateCreated,
                T_ProjectName,
                T_ProjectDescription,
                T_TicketCreatorUserName,
                T_AssignedDeveloperUserName,
                T_DueDate,
                T_HoursToComplete,
                T_TicketType,
                T_TicketPriority,
                T_TicketStatus,
                T_SkillRequired
            }

            private ApplicationDbContext db = new ApplicationDbContext();
            private static readonly string[] UserHeadings = { "FirstName", "LastName", "UserName", "DisplayName", "SkillLevel", "Email", "Phone", "RoleAdmin", "RoleProjectManager", "RoleDeveloper", "RoleSubmitter" };
            private static readonly string[] ProjectHeadings = { "ProjectName", "Description", "ProjectManagerUserName" };
            private static readonly string[] ProjDevHeadings = { "ProjectName", "Description", "DeveloperUserName" };
            private static readonly string[] TicketHeadings = { "Title", "Description", "DateCreated", "ProjectName", "ProjectDescription", "TicketCreatorUserName", "AssignedDeveloperUserName", "DueDate", "HoursToComplete", "TicketType", "TicketPriority", "TicketStatus", "SkillRequired" };
            public const string SectionUsers = "#Users";
            public const string SectionProjects = "#Projects";
            public const string SectionProjDevs = "#ProjectDevelopers";
            public const string SectionTickets = "#Tickets";


            public string Target;       // The type this is updating
            public string[] Source;     // List of all rows
            public int Start;
            public int Count;
            public string[] Headings;
            public int[] HeadingOffsets;

            public Section(ApplicationDbContext db)
            {
                this.db = db;
            }

            public static int FindPos(string src, string[] strings){
                // Return -1 if not found, else offset in string array
                for (int i = 0; i < strings.Length; i++) {
                    if (src.Equals(strings[i]))
                        return i;
                }

                // Not found...
                return -1;
            }
            
            public static Section Init(ApplicationDbContext db, string[] src, string target)
            {
                // Find
                Section section = new Section(db);
                section.Target = target;
                section.Source = src;
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
                section.Headings = src[counter].Trim().Split('\t');
                section.Start = ++counter;

                // Now count # rows...
                while (counter < src.Length && !src[counter].StartsWith("#")){
                    counter++;
                }
                section.Count = counter - section.Start;

                // Now process column headings; the goal is to determine exactly where
                // each of the required headings is, in the user's data. When finished,
                // HeadingOffsets will point to the proper position in Headings.
                string[] ptrHeadings = null;
                switch (target)
                {
                    case SectionUsers:
                        ptrHeadings = UserHeadings;
                        break;
                    case SectionProjects:
                        ptrHeadings = ProjectHeadings;
                        break;
                    case SectionProjDevs:
                        ptrHeadings = ProjDevHeadings;
                        break;
                    case SectionTickets:
                        ptrHeadings = TicketHeadings;
                        break;
                }
                int i;
                section.HeadingOffsets = new int[ptrHeadings.Length];
                for (i = 0; i < ptrHeadings.Length; i++)
                {
                    section.HeadingOffsets[i] = FindPos(ptrHeadings[i], section.Headings);
                    // Note that if one of the required headings is not found, its offset is -1
                }
                return section;
            }

            public static void LogErr(ApplicationDbContext db, string msg){
                Log log = new Log("** Error during DataImport **", msg);
                db.Logs.Add(log);
                db.SaveChanges();
            }
            public static void LogAlert(ApplicationDbContext db, string msg)
            {
                Log log = new Log("DataImport alert", msg);
                db.Logs.Add(log);
                db.SaveChanges();
            }
            public static void LogSuccess(ApplicationDbContext db, string msg)
            {
                Log log = new Log("DataImport successful", msg);
                db.Logs.Add(log);
                db.SaveChanges();
            }

            public int ProcessSection()
            {
                // Process each Section, update log with error message if any
                switch (Target)
                {
                    case SectionUsers:
                        return AddNewUsers();
                    case SectionProjects:
                        return AddNewProjects();
                    case SectionProjDevs:
                        return AddNewProjDevs();
                    case SectionTickets:
                        return AddNewTickets();
                    default:
                        return 0;
                }
            }

            private int AddNewTickets()
            {
                // Process each row, create new user
                int nTickets = 0;
                int row;
                string[] colData;
                string Creator, Dev, Name, Desc;
                row = Start - 1;
                UserRolesHelper helper = new UserRolesHelper();
                var statuses = db.TicketStatuses.ToList();
                var priorities = db.TicketPriorities.ToList();
                var types = db.TicketTypes.ToList();
                var skills = db.SkillLevels.ToList();
                bool devOk = false;         // Set to true if validated and ready to add to ticket

                // Use same time stamp for all tickets
                DateTimeOffset now = DateTimeOffset.UtcNow;

                // First, make sure all needed columns are present
                if (HeadingOffsets[(int)H.T_Title] < 0
                    || HeadingOffsets[(int)H.T_Description] < 0)
                {
                    Section.LogErr(db, "Ticket on row " + (row + 1) + " is missing a column " +
                        "(Title and Description are required) -- cannot process any Tickets");
                    return 0;
                }

                while (++row < (Count + Start))
                {
                    if (Source[row][0] != ';')
                    {
                        // Valid row to process, so get columns
                        //
                        // DON'T TRIM THE ROW!! If it's trimmed, the column positions will NOT line up with
                        // what is expected, resulting in a crash!
                        //
                        colData = Source[row].Split('\t');

                        // These rules must be followed:
                        // [] Title and Description must be non-empty
                        // [] No uniqueness test -- OK to enter tickets with same Title/Description(??)
                        // [] Developer must be in database
                        // [] Developer must be in Developer role
                        // [] TicketCreator must be in database
                        // [] If ProjectTitle/Description cannot be found, leave as null
                        // [] Parse DateCreated - default is Now
                        // [] Parse DueDate - default is Now + 1 day
                        // [] Parse HoursToComplete - default is 1
                        // [] Parse Type, Priority, Status, Skill - default is 1
                        //    - load all values from db, find the match, if not found use default
                        //
                        // If rules not followed, show error and skip

                        Ticket ticket = new Ticket();
                        ticket.Title = colData[HeadingOffsets[(int)H.T_Title]];
                        ticket.Description = colData[HeadingOffsets[(int)H.T_Description]];
                        Dev = colData[HeadingOffsets[(int)H.T_AssignedDeveloperUserName]];
                        Creator = colData[HeadingOffsets[(int)H.T_TicketCreatorUserName]];

                        ApplicationUser user = null;

                        // Test column requirements now...
                        if (ticket.Title.Length == 0 || ticket.Description.Length == 0)
                        {
                            Section.LogErr(db, "Ticket on row " + (row + 1) + " needs both a Title and a Description -- will be skipped");
                            continue;
                        }

                        // Validate Developer
                        user = db.Users.FirstOrDefault(u => u.UserName == Dev);
                        if (user == null)
                        {
                            if (Dev != "")
                                Section.LogAlert(db, "DeveloperUser on row " + (row + 1) + " has UserName [" + Dev
                                    + "] not in database -- leaving field blank, will continue");
                        }
                        else
                        {
                            // Dev is in database, now check role
                            if (!helper.IsUserInRole(user.Id, R.Developer))
                            {
                                Section.LogErr(db, "Ticket on row " + (row + 1) + " has AssignedDeveloper [" + user.UserName
                                    + "] not in 'Developer' role -- leaving field blank will continue");
                            }
                            else
                            {
                                devOk = true;       // Flags us to create Notification if/when ticket added to table
                                ticket.AssignedToDevId = user.Id;
                            }
                        }

                        // Validate Creator
                        user = db.Users.FirstOrDefault(u => u.UserName == Creator);
                        if (user == null)
                        {
                            Section.LogAlert(db, "TicketCreator on row " + (row + 1) + " has UserName [" + Dev
                                + "] not in database -- using name of current user for this Ticket");
                            ticket.OwnerUserId = helper.GetCurrentUserId();
                        }
                        else
                        {
                            ticket.OwnerUserId = user.Id;
                        }

                        // Validate Project -- if not found, leave as null
                        Name = colData[HeadingOffsets[(int)H.T_ProjectName]];
                        Desc = colData[HeadingOffsets[(int)H.T_ProjectDescription]];
                        var project = db.Projects.FirstOrDefault(p => p.Name == Name && p.Description == Desc);
                        if (project != null)
                            ticket.ProjectId = project.Id;
                        else {
                            if (Name != "" && Desc != "")
                                Section.LogAlert(db, "Ticket on row " + (row + 1)
                                + " has project not found in Projects table -- setting to null, will continue");
                        }

                        // Check HoursToComplete
                        int num;
                        int.TryParse(colData[HeadingOffsets[(int)H.T_HoursToComplete]], out num);
                        ticket.HoursToComplete = num < 1 ? 1 : num;

                        // Parse Dates...
                        DateTimeOffset date;
                        DateTimeOffset.TryParse(colData[HeadingOffsets[(int)H.T_DateCreated]], out date);
                        if (date == DateTimeOffset.MinValue)
                        {
                            ticket.Created = ticket.MostRecentUpdate = now;
                            Section.LogAlert(db, "Ticket on row " + (row + 1) + " had invalid DateCreated -- using today's date");
                        }
                        else ticket.Created = date;
                        DateTimeOffset.TryParse(colData[HeadingOffsets[(int)H.T_DueDate]], out date);
                        if (date == DateTimeOffset.MinValue)
                        {
                            ticket.DueDate = now.AddDays(10);
                            Section.LogAlert(db, "Ticket on row " + (row + 1) + " had invalid DueDate -- set default date to 10 days from now");
                        }
                        else ticket.DueDate = date;

                        // Validate Type, Priority, Status, Skill -- set to 1 as default
                        // Type
                        string type = colData[HeadingOffsets[(int)H.T_TicketType]].ToUpper();
                        var tempType = types.Where(t => t.Name.ToUpper().Contains(type));
                        if (tempType.Count() == 1)
                            ticket.TicketTypeId = tempType.ElementAt(0).Id;
                        else
                        {
                            // Set to default, issue alert if invalid type specified
                            ticket.TicketTypeId = 1;
                            if (type != "")
                                Section.LogAlert(db, "Ticket on row " + (row + 1) + " had invalid TicketType -- set to Bug");
                        }

                        // Priority
                        string priority = colData[HeadingOffsets[(int)H.T_TicketPriority]].ToUpper();
                        var tempPriority = priorities.Where(t => t.Name.ToUpper().Contains(priority));
                        if (tempPriority.Count() == 1)
                            ticket.TicketPriorityId = tempPriority.ElementAt(0).Id;
                        else
                        {
                            // Set to default, issue alert if invalid type specified
                            ticket.TicketPriorityId = 3;
                            if (type != "")
                                Section.LogAlert(db, "Ticket on row " + (row + 1) + " had invalid TicketPriority -- set to Essential");
                        }

                        // Status
                        string status = colData[HeadingOffsets[(int)H.T_TicketStatus]].ToUpper();
                        var tempStatus = statuses.Where(t => t.Name.ToUpper().Contains(status));
                        if (tempStatus.Count() == 1)
                            ticket.TicketStatusId = tempStatus.ElementAt(0).Id;
                        else
                        {
                            // Set to default, issue alert if invalid type specified
                            ticket.TicketStatusId = 1;
                            if (type != "")
                                Section.LogAlert(db, "Ticket on row " + (row + 1) + " had invalid TicketStatus -- set to New");
                        }

                        // Skill
                        string skill = colData[HeadingOffsets[(int)H.T_SkillRequired]].ToUpper();
                        var tempSkill = skills.Where(s => s.Name.ToUpper().Contains(skill));
                        if (tempSkill.Count() == 1)
                            ticket.SkillRequiredId = tempSkill.ElementAt(0).Id;
                        else
                        {
                            // Set to default, issue alert if invalid type specified
                            ticket.SkillRequiredId = 1;
                            if (type != "")
                                Section.LogAlert(db, "Ticket on row " + (row + 1) + " had invalid SkillRequired -- set to Junior");
                        }

                        // We can add the Ticket now
                        db.Tickets.Add(ticket);
                        db.SaveChanges();
                        if (devOk)
                            ticket.NotifyNewTicket(db);
                        LogSuccess(db, "Added Ticket from row " + (row + 1));
                        nTickets++;
                    }
                }
                return nTickets;
            }


            private int AddNewProjDevs()
            {
                int nDevs = 0;
                // Process each row, add Developer to Project
                int row;
                string[] colData;
                string Name, Desc, Dev;
                row = Start - 1;
                UserRolesHelper helper = new UserRolesHelper();

                // First, make sure all needed columns are present
                if (HeadingOffsets[(int)H.PD_ProjectName] < 0
                    || HeadingOffsets[(int)H.PD_Description] < 0
                    || HeadingOffsets[(int)H.PD_DeveloperUserName] < 0)
                {
                    Section.LogErr(db, "ProjectDeveloper on row " + (row + 1) + " is missing a column " +
                        "(ProjectName, Description, and DeveloperUserName are required) -- cannot process any ProjectDevelopers");
                    return 0;
                }

                while (++row < (Count + Start))
                {
                    if (Source[row][0] != ';')
                    {
                        // Valid row to process, so get columns
                        //
                        // DON'T TRIM THE ROW!! If it's trimmed, the column positions will NOT line up with
                        // what is expected, resulting in a crash!
                        //
                        colData = Source[row].Split('\t');

                        // These rules must be followed:
                        // [] All three columns (ProjectName, Description, and DeveloperUserName) must exist
                        // [] ProjectName/Description must be in database
                        // [] Developer must be in Developer role
                        // [] There can be no other project having the same Title+Description
                        //
                        // If rules not followed, show error and skip
                        Name = colData[HeadingOffsets[(int)H.PD_ProjectName]];
                        Desc = colData[HeadingOffsets[(int)H.PD_Description]];
                        Dev = colData[HeadingOffsets[(int)H.PD_DeveloperUserName]];
                        ApplicationUser user = null;

                        // Test requirements now...
                        if (Name.Length == 0 || Desc.Length == 0 || Dev.Length == 0)
                        {
                            Section.LogErr(db, "ProjectDeveloper info on row " + (row + 1) + " is missing data in a column -- cannot process");
                            continue;
                        }
                        user = db.Users.FirstOrDefault(u => u.UserName == Dev);
                        if (user == null)
                        {
                            Section.LogErr(db, "DeveloperUser on row " + (row + 1) + " has UserName [" + Dev
                                + "] not in database -- cannot process");
                            continue;
                        }
                        else
                        {
                            // Dev is in database, now check role
                            if (!helper.IsUserInRole(user.Id, R.Developer))
                            {
                                Section.LogErr(db, "Project on row " + (row + 1) + " has UserName [" + user.UserName
                                    + "] not in 'Developer' role -- cannot process");
                                continue;
                            }
                        }
                        // Make sure there is a unique ProjectTitle/Description...
                        var project = db.Projects.FirstOrDefault(p => p.Name == Name && p.Description == Desc);
                        if (project == null)
                        {
                            Section.LogErr(db, "Project on row " + (row + 1)
                                + " not found in Projects table -- cannot add DeveloperUser to project");
                            continue;
                        }

                        // We can add the Developer now
                        if (ProjectsHelper.AddUserToProject(user.Id, project.Id))
                        {
                            LogSuccess(db, "Added Developer from row " + (row + 1));
                            nDevs++;
                        }
                        else LogAlert(db, "Developer on row " + (row + 1) + " already on Project");
                    }
                }
                return nDevs;
            }


            private int AddNewProjects()
            {
                int nProjects = 0;
                // Process each row, create new Project
                int row;
                string[] colData;
                string PM;
                row = Start-1;
                UserRolesHelper helper = new UserRolesHelper();

                // First, make sure all needed columns are present
                if (HeadingOffsets[(int)H.P_ProjectName] < 0 
                    || HeadingOffsets[(int)H.P_Description] < 0
                    || HeadingOffsets[(int)H.P_ProjectManagerUserName] < 0)
                {
                    Section.LogErr(db, "Project on row " + (row + 1) + " is missing a column " +
                        "(ProjectName, Description, and ProjectManagerUserName are required) -- cannot process any projects");
                    return 0;
                }


                while (++row < (Count + Start))
                {
                    if (Source[row][0] != ';')
                    {
                        // Valid row to process, so get columns
                        //
                        // DON'T TRIM THE ROW!! If it's trimmed, the column positions will NOT line up with
                        // what is expected, resulting in a crash!
                        //
                        colData = Source[row].Split('\t');

                        // These rules must be followed:
                        // [] All three columns (ProjectName, Description, and ProjectManagerUserName) must exist
                        // [] ProjectManager must be in database
                        // [] ProjectManager must be in PM role
                        // [] There can be no other project having the same Title+Description
                        //
                        // If rules not followed, show error and skip
                        Project project = new Project();
                        project.Name = colData[HeadingOffsets[(int)H.P_ProjectName]];
                        project.Description = colData[HeadingOffsets[(int)H.P_Description]];
                        PM = colData[HeadingOffsets[(int)H.P_ProjectManagerUserName]];
                        ApplicationUser user = null;

                        // Test requirements now...
                        if (project.Name.Length == 0 || project.Description.Length == 0 || PM.Length == 0)
                        {
                            Section.LogErr(db, "Project on row " + (row + 1) + " is missing data in a column -- cannot process");
                            continue;
                        }
                        user = db.Users.FirstOrDefault(u => u.UserName == PM);
                        if (user == null) {
                            Section.LogErr(db, "Project on row " + (row+1) + " has UserName [" + PM
                                + "] not in database -- cannot process");
                            continue;
                        } else {
                            // PM is in database, now check role
                            if (!helper.IsUserInRole(user.Id, R.PM)) {
                                Section.LogErr(db, "Project on row " + (row+1) + " has UserName [" + user.UserName 
                                    + "] not in 'ProjectManager' role -- cannot process");
                                continue;
                            }
                        }
                        // Make sure Title and Description are unique
                        var proj = db.Projects.FirstOrDefault(p => p.Name == project.Name && p.Description == project.Description);
                        if (proj != null)
                        {
                            Section.LogErr(db, "Project on row " + (row + 1) 
                                + " has same name and description as project # " + proj.Id + " in Projects table -- cannot process");
                            continue;
                        }

                        // We can create the project now
                        db.Projects.Add(project);
                        project.Users.Add(db.Users.Find(user.Id));
                        db.SaveChanges();
                        LogSuccess(db, "Added Project from row " + (row + 1));
                        nProjects++;
                    }
                }
                return nProjects;
            }

            private int AddNewUsers()
            {
                int nUsers = 0;
                // Process each row, create new user
                int row;
                string[] colData;
                bool skip = false;
                row = Start-1;
                UserRolesHelper helper = new UserRolesHelper();
                var store = new UserStore<ApplicationUser>(db);
                UserManager<ApplicationUser> manager = new UserManager<ApplicationUser>(store);
                string defaultPassword = "Ab1234.";

                while (++row < (Count + Start))
                {
                    skip = false;
                    if (Source[row][0] != ';')
                    {
                        // Valid row to process, so get columns
                        //
                        // DON'T TRIM THE ROW!! If it's trimmed, the column positions will NOT line up with
                        // what is expected, resulting in a crash!
                        //
                        colData = Source[row].Split('\t');

                        // Create a new User object, fill in the data
                        int nRequireds = 0;         // Used to show all required fields exist
                        string data;
                        ApplicationUser user = new ApplicationUser();

                        // FirstName
                        if (this.HeadingOffsets[(int)H.U_FirstName] >= 0)
                        {
                            if ((user.FirstName = colData[HeadingOffsets[(int)H.U_FirstName]]) != "")
                            {
                                nRequireds++;
                            }
                        }

                        // LastName
                        if (this.HeadingOffsets[(int)H.U_LastName] >= 0)
                        {
                            if ((user.LastName = colData[HeadingOffsets[(int)H.U_LastName]]) != "")
                            {
                                nRequireds++;
                            }
                        }
                        // If we don't have either a First or Last name, show error..
                        if (nRequireds == 0)
                        {
                            Section.LogErr(db, "Row " + (row+1) + " needs either FirstName or LastName -- cannot process this row");
                            skip = false;
                        }

                        // UserName
                        if (this.HeadingOffsets[(int)H.U_UserName] >= 0)
                        {
                            if ((user.UserName = colData[HeadingOffsets[(int)H.U_UserName]]) == "")
                            {
                                // If there's no UserName, exit now!
                                Section.LogErr(db, "Row " + row + " must have a unique UserName -- cannot process this row");
                                skip = false;
                            }
                        }

                        // DisplayName
                        if (this.HeadingOffsets[(int)H.U_DisplayName] >= 0)
                        {
                            // If no display name, use UserName
                            if ((data = colData[HeadingOffsets[(int)H.U_DisplayName]]) == "")
                                data = user.UserName;
                            user.DisplayName = data;
                        }

                        // SkillLevel
                        if (this.HeadingOffsets[(int)H.U_SkillLevel] >= 0)
                        {
                            // Setup SkillLevel
                            data = colData[HeadingOffsets[(int)H.U_SkillLevel]];
                            int pos = FindPos(data, Skills.ToList());
                            if (pos < 0)
                            {
                                // Not valid, show error
                                Section.LogAlert(db, "Row " + (row+1) + ": setting default SkillLevel to Junior");
                                pos = 0;
                            }
                            user.SkillLevelId = pos + 1;
                        }
                        else user.SkillLevelId = 1;

                        // Email
                        if (this.HeadingOffsets[(int)H.U_Email] >= 0)
                        {
                            if ((user.Email = colData[HeadingOffsets[(int)H.U_Email]]) == "")
                            {
                                Section.LogErr(db, "Row " + (row+1) + " must have a valid Email -- cannot process this row");
                                skip = true;
                            }
                        }

                        // PhoneNumber
                        if (this.HeadingOffsets[(int)H.U_Phone] >= 0)
                        {
                            user.PhoneNumber = user.TextMsgNumber = colData[HeadingOffsets[(int)H.U_Phone]];
                        }

                        // User is complete, so save if unique

                        if (!skip)
                        {
                            // Before creating an account, see if this user name is already being used
                            var x2 = db.Users.FirstOrDefault(u => u.UserName == user.UserName);
                            if (x2 == null)
                            {
                                // OK to continue, this will be a new user
                                bool addedUser = false;
                                try
                                {
                                    manager.Create(user, defaultPassword);
                                    addedUser = true;
                                }
                                catch (Exception e)
                                {
                                    skip = true;
                                    addedUser = false;
                                }

                                // Need to ensure the user was created
                                try
                                {
                                    // See if user is in the db... if not, save again!
                                    var x = db.Users.FirstOrDefault(u => u.UserName == user.UserName);
                                    if (x == null)
                                    {
                                        var u = db.Users.Add(user);
                                        db.SaveChanges();
                                        addedUser = true;
                                    }
                                }
                                catch (Exception e)
                                {
                                    string msg = "Row " + (row + 1) + " could not be processed";
                                    LogErr(db, msg);
                                    skip = true;
                                    addedUser = false;
                                }
                                if (addedUser)
                                    nUsers++;

                                // Now add any user roles specified...
                                if (!skip)
                                {
                                    LogSuccess(db, "Added User [" + user.UserName + "] from row " + (row + 1));
                                    int nRoles = 0;
                                    // Admin
                                    if (this.HeadingOffsets[(int)H.U_RoleAdmin] > 0)
                                    {
                                        data = colData[HeadingOffsets[(int)H.U_RoleAdmin]].ToUpper();
                                        if (data == "Y")
                                        {
                                            if (!helper.IsUserInRole(user.Id, R.Admin))
                                            {
                                                helper.AddUserToRole(user.Id, R.Admin);
                                                nRoles++;
                                            }
                                        }
                                    }

                                    // PM
                                    if (this.HeadingOffsets[(int)H.U_RoleProjectManager] > 0)
                                    {
                                        data = colData[HeadingOffsets[(int)H.U_RoleProjectManager]].ToUpper();
                                        if (data == "Y")
                                        {
                                            if (!helper.IsUserInRole(user.Id, R.PM))
                                            {
                                                helper.AddUserToRole(user.Id, R.PM);
                                                nRoles++;
                                            }
                                        }
                                    }

                                    // Dev
                                    if (this.HeadingOffsets[(int)H.U_RoleDeveloper] > 0)
                                    {
                                        data = colData[HeadingOffsets[(int)H.U_RoleDeveloper]].ToUpper();
                                        if (data == "Y")
                                        {
                                            if (!helper.IsUserInRole(user.Id, R.Dev))
                                            {
                                                helper.AddUserToRole(user.Id, R.Dev);
                                                nRoles++;
                                            }
                                        }
                                    }

                                    // Submitter
                                    if (this.HeadingOffsets[(int)H.U_RoleSubmitter] > 0)
                                    {
                                        data = colData[HeadingOffsets[(int)H.U_RoleSubmitter]].ToUpper();
                                        if (data == "Y")
                                        {
                                            if (!helper.IsUserInRole(user.Id, R.Submitter))
                                            {
                                                helper.AddUserToRole(user.Id, R.Submitter);
                                                nRoles++;
                                            }
                                        }
                                    }

                                    // If no roles, add Submitter
                                    if (nRoles == 0)
                                        if (!helper.IsUserInRole(user.Id, R.Submitter))
                                            helper.AddUserToRole(user.Id, R.Submitter);
                                }
                            }
                            else
                            {
                                Section.LogAlert(db, "UserName [" + user.UserName + "] on row " + (row + 1) + " already in database, will be skipped");
                            }
                        }
                    }
                }
                return nUsers;
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
            Section users = Section.Init(db, list, Section.SectionUsers);
            Section projects = Section.Init(db, list, Section.SectionProjects);
            Section projectDevs = Section.Init(db, list, Section.SectionProjDevs);
            Section tickets = Section.Init(db, list, Section.SectionTickets);
             
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
            int nUsers = 0, nProjects = 0, nDevs = 0, nTickets = 0;
            if (!err)
            {
                // Process all sections...
                nUsers = users.ProcessSection();
                nProjects = projects.ProcessSection();
                nDevs = projectDevs.ProcessSection();
                nTickets = tickets.ProcessSection();
            }

            // Do this in every GET action...
            ViewBag.UserModel = ProjectsHelper.LoadUserModel();
            // OK, decide what to do now -- which View should we go to?
            ViewBag.Msg = "Imported " + nUsers + (nUsers == 1 ? " user" : " users")
                + ", " + nProjects + (nProjects == 1 ? " project" : " projects")
                + ", " + nTickets + (nTickets == 1 ? " ticket" : " tickets")
                + ", and assigned " + nDevs + (nDevs == 1 ? " developer." : " developers.");
            return View();
        }

        // GET: Utilities
        public ActionResult Index()
        {
            // Do this in every GET action...
            ViewBag.UserModel = ProjectsHelper.LoadUserModel();
            return View(db.Logs.ToList());
        }

        // GET: Utilities/Details/5
        public ActionResult Details(int? id)
        {
            // Do this in every GET action...
            ViewBag.UserModel = ProjectsHelper.LoadUserModel();
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

            // Do this in every GET action...
            ViewBag.UserModel = ProjectsHelper.LoadUserModel();
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

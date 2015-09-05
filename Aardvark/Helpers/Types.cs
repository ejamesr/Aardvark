using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Aardvark.Helpers
{
    public class Types
    {
        public string Name { get; set; }
        public string Desc { get; set; }

    }

    public static class Pri
    {
        // Each class will have a short and a long name
        // The first valid value starts at index 1, to match the file Id
        private enum Priority
        {
            Zero,
            Bug,
            Enh,
            Unk,
            Count
        }
        private enum PriorityL
        {
            Zero,
            Bug,
            Enhancement,
            Unknown,
            Count
        }
        private static string[] Val = new string[(int)Priority.Count] {
            "", Priority.Enh.ToString(), Priority.Bug.ToString(), Priority.Unk.ToString()
        };
        private static string[] ValL = new string[(int)PriorityL.Count] {
            "", PriorityL.Enhancement.ToString(), PriorityL.Bug.ToString(), PriorityL.Unknown.ToString()
        };

        public static string Unk { get { return Val[(int)Priority.Unk]; } }
        public static string Bug { get { return Val[(int)Priority.Bug]; } }
        public static string Enh { get { return Val[(int)Priority.Enh]; } }

        public static string BugL { get { return ValL[(int)PriorityL.Bug]; } }
        public static string EnhL { get { return ValL[(int)PriorityL.Enhancement]; } }
        public static string UnkL { get { return ValL[(int)PriorityL.Unknown]; } }
    }

    /// <summary>
    /// TicketNotifications - static class helps manage strings and enums
    /// </summary>
    public static class TNT
    {
        // Type for TicketNotifications
        public enum Type : int
        {
            Dummy = 0,
            AssignedToTicket = 1,
            RemovedFromTicket,
            TicketModified,
            TicketDeleted,
            CommentCreated,
            CommentModified,
            CommentDeleted,
            AttachmentCreated,
            AttachmentModified,
            AttachmentDeleted,
            Count
        }

        public static class Str
        {
            public static string AssignedToTicket = Type.AssignedToTicket.ToString();
            public static string RemovedFromTicket = Type.RemovedFromTicket.ToString();
            public static string TicketModified = Type.TicketModified.ToString();
            public static string TicketDeleted = Type.TicketDeleted.ToString();
            public static string CommentCreated = Type.CommentCreated.ToString();
            public static string CommentModified = Type.CommentModified.ToString();
            public static string CommentDeleted = Type.CommentDeleted.ToString();
            public static string AttachmentCreated = Type.AttachmentCreated.ToString();
            public static string AttachmentModified = Type.AttachmentModified.ToString();
            public static string AttachmentDeleted = Type.AttachmentDeleted.ToString();
        }

        public static class Desc
        { 
            public static string AssignedToTicket { get { return "You have been assigned to this ticket"; } }
            public static string RemovedFromTicket { get { return "You have been removed from this ticket"; } }
            public static string TicketModified { get { return "Ticket has been modified"; } }
            public static string TicketDeleted { get { return "Ticket has beem deleted"; } }
            public static string CommentCreated { get { return "Comment has been added to this ticket"; } }
            public static string CommentModified { get { return "Comment for this ticket has been modified"; } }
            public static string CommentDeleted { get { return "Comment for this ticket has been deleted"; } }
            public static string AttachmentCreated { get { return "Attachment has been added to this ticket"; } }
            public static string AttachmentModified { get { return "Attachment for this ticket has been modified"; } }
            public static string AttachmentDeleted { get { return "Attachment for this ticket has been deleted"; } }
        }

        private static int[] step = new int[(int)Type.Count + 1];

        //
        // Eventually use TNT and phase out TN.
        //
        static TNT()
        {
            step[0] = 0;
            for (int i = 1, val = 0; i < step.Length; i++, val += 10)
            {
                step[i] = val;
            }

            // Test the data...
            string s;
            s = Str.AssignedToTicket;
            s = Str.RemovedFromTicket;
            s = Str.TicketModified;
            s = Str.TicketDeleted;
            s = Str.CommentCreated;
            s = Str.CommentModified;
            s = Str.CommentDeleted;
            s = Str.AttachmentCreated;
            s = Str.AttachmentModified;
            s = Str.AttachmentDeleted;
        }


        public static int Step(Type status) { return step[(int)status]; }
    }

}
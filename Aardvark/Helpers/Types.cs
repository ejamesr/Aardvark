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
}
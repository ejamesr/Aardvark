using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Aardvark.Models
{
    public class Log
    {
        // Keep track of status of various stuff
        public int Id { get; set; }
        public DateTimeOffset Created { get; set; }
        public string Name { get; set; }
        public string Msg { get; set; }

        public Log() { }

        public Log(string name, string msg)
        {
            Name = name;
            Msg = msg;
            Created = DateTimeOffset.UtcNow;
        }
    }
}
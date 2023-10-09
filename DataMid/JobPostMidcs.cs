using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataMid
{
    public class JobPostMidcs
    {
        public int? JobId { get; set; }
        public int? FromClient { get; set; }
        public int? ToClient { get; set; }
        public string Job { get; set; }
        public string JobVariables { get; set; } // seperated by |
        public int? JobSuccess { get; set; } // -1 : Not Done, 0 : progress, 1 : Complete
        public string JobResult { get; set; }
    }
}

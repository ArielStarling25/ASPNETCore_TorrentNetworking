﻿using System;
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
        public int? JobSuccess { get; set; }
        public string JobResult { get; set; }
    }
}
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckCarsAPI.Models
{
    public class IssueReport: Report
    {
        public string? Details { get; set; }
        public string? Priority { get; set; }
        public string? Type { get; set; }
        public bool IsResolved { get; set; } = false;

    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace commitor.src.pears
{
    public class RepositoryInfo
    {
        public string name { get; set; }
        public string tagbri { get; set; }
        public string commitbri { get; set; }
        public string issuebri { get; set; }
    }
    public class Repositories
    {
        public List<RepositoryInfo> platform { get; set;}
    }
}

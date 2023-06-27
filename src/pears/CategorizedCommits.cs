using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace commitor.src.pears
{
    public class TypedCommits
    {
        public string Type { get; set; }
        public string TypeDescription { get; set; }
        public List<string> Values { get; set; }
    }
    public class CategorizedCommits
    {
        public string TagName { get; set; }
        public List<TypedCommits> GroupedCommits { get; set; }
    }
}

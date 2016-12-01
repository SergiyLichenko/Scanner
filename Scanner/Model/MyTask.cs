using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scanner.Model
{
    public class MyTask
    {
        public string URL { get; set; }
        public string Searched { get; set; }
        public int MaxCountThreads { get; set; }
        public int MaxCountUrls { get; set; }
    }
}

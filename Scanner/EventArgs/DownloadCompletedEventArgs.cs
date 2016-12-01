using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scanner.Infrastructure
{
    public class DownloadCompletedEventArgs:EventArgs
    {
        public string Result { get; set; }

        public DownloadCompletedEventArgs(string result)
        {
            Result = result;
        }
    }
}

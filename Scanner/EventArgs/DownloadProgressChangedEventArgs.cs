using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scanner.Infrastructure
{
   public class DownloadProgressChangedEventArgs:EventArgs
    {
        public int Progress { get; private set; }

       public DownloadProgressChangedEventArgs(int progress)
       {
           Progress = progress;
       }
    }
}

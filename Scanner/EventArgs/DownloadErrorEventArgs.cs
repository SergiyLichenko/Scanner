using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scanner.Infrastructure
{
    public class DownloadErrorEventArgs : EventArgs
    {
        public string ErrorMessage { get; private set; }

        public DownloadErrorEventArgs(string errorMessage)
        {
            this.ErrorMessage = errorMessage;
        }
    }
}

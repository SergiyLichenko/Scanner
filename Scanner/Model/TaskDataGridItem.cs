using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Scanner.Annotations;

namespace Scanner.Model
{
    public class TaskDataGridItem : INotifyPropertyChanged
    {
        private string _url;
        private string _downloadStatus;
        private int _progress;
        private string _searchStatus;
        public string Url
        {
            get { return this._url; }
            set
            {
                this._url = value;
                OnPropertyChanged(nameof(Url));
            }

        }

        public string DownloadStatus
        {
            get { return _downloadStatus; }
            set
            {
                this._downloadStatus = value;
                OnPropertyChanged(nameof(DownloadStatus));
            }
        }

        public int Progress
        {
            get { return _progress; }
            set
            {
                this._progress = value;
                OnPropertyChanged(nameof(Progress));
            }
        }

        public string SearchStatus
        {
            get { return this._searchStatus; }
            set
            {
                this._searchStatus = value;
                OnPropertyChanged(nameof(SearchStatus));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

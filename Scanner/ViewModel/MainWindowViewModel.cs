using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Caliburn.Micro;
using GalaSoft.MvvmLight.Messaging;
using Scanner.Annotations;
using Scanner.Infrastructure;
using Scanner.Model;

namespace Scanner.ViewModel
{
    public partial class MainWindowViewModel : ViewModelBase, IDisposable
    {
        public MainWindowViewModel()
        {
            StatusBoarVisibility = Visibility.Hidden;
            NewTaskEnabled = true;
        }

        #region Fields
        private const int ChunkSize = 4096;

        //Assume that url can contain only these characters: ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-._~:/?#[]@!$&'()*+,;=
        private Regex _urlRegex = new Regex(@"https?://[!#$&-;=?-\[\]_a-z~]+");
        private Regex _textRegex;
        private List<FileDownload> _downloads;
        private static object _lockObject = new object();

        private volatile MyTask _myTask;
        private volatile ConcurrentQueue<string> _urlQueue;
        private volatile int _currentRunningThreads;
        private volatile int _countUrlsDone;
        private volatile int _countFoundText;
        private BindableCollection<TaskDataGridItem> _gridItems;
        private Mutex _mutex = new Mutex(false);

        #endregion

        #region Properties
        public BindableCollection<TaskDataGridItem> GridItems
        {
            get
            {
                if (this._gridItems == null)
                    _gridItems = new BindableCollection<TaskDataGridItem>();
                return _gridItems;
            }
        }
        public int DataGridSelectedIndex { get; set; }
        public string SearchedTextToShow => "Searched Text: " + _myTask?.Searched;
        public string CurrentThreadsToShow => $"Current Running Threads: {_currentRunningThreads} / {_myTask.MaxCountThreads} ";
        public string CountUrlsDoneToShow => $"Urls Checked: {_countUrlsDone} / {_myTask.MaxCountUrls}";

        public string CountFoundText => $"Found in {_countFoundText} / {_countUrlsDone} Urls";
        #endregion



        public void AddTask(MyTask myTask)
        {
            _urlQueue = new ConcurrentQueue<string>();
            this.GridItems.Clear();

            this._myTask = myTask;
            _textRegex = new Regex(@"[\s\p{P}]?" + myTask.Searched + @"[\s\p{P}]?");
            _urlQueue.Enqueue(myTask.URL);
            Cleaning();

            Task.Run(() => AccessTheWeb());
        }

        private void Cleaning()
        {
            StatusBoarVisibility = Visibility.Visible;
            PauseAllEnabled = true;
            StopAllEnabled = true;
            NewTaskEnabled = false;
            _countFoundText = 0;
            _countUrlsDone = 0;
            _mutex = new Mutex();
            _downloads = new List<FileDownload>();

            OnPropertyChanged(nameof(SearchedTextToShow));
            OnPropertyChanged(nameof(CountFoundText));
            OnPropertyChanged(nameof(CurrentThreadsToShow));
            OnPropertyChanged(nameof(CountUrlsDoneToShow));
        }

        private void AccessTheWeb()
        {
            if (_urlQueue.Count == 0 || _countUrlsDone + _currentRunningThreads > _myTask.MaxCountUrls - 1)
                return;

            _mutex.WaitOne();
            List<Task<FileDownload>> downloads = new List<Task<FileDownload>>();
            for (int i = 0; i < _myTask.MaxCountThreads - _currentRunningThreads; i++)
            {
                if (_urlQueue.Count == 0)
                    break;

                string temp = String.Empty;
                if (_urlQueue.TryDequeue(out temp))
                    downloads.Add(ProcessUrl(temp));
            }
            _mutex.ReleaseMutex();

        }


        public async Task<FileDownload> ProcessUrl(string url)
        {
            if (!PauseAllEnabled)
                return null;
            var item = new TaskDataGridItem
            {
                Url = url,
                DownloadStatus = Enum.GetName(typeof(DownloadStatus), DownloadStatus.Downloading),
                Progress = 0
            };

            var tokenSource = new CancellationTokenSource();
            FileDownload fileDownload = new FileDownload(url, ChunkSize, tokenSource);
            fileDownload.DownloadProgressChanged += FileDownload_DownloadProgressChanged;
            fileDownload.DownloadCompleted += FileDownload_DownloadCompleted;
            fileDownload.DownloadError += FileDownload_DownloadError;

            Application.Current.Dispatcher.Invoke(() =>
                        GridItems.Add(item));
            this._downloads.Add(fileDownload);
            Interlocked.Increment(ref _currentRunningThreads);

            OnPropertyChanged(nameof(CurrentThreadsToShow));
            await fileDownload.Start();

            return fileDownload;
        }




        #region FileDownloadEvents

        private void FileDownload_DownloadError(object sender, DownloadErrorEventArgs e)
        {
            var fileDownload = sender as FileDownload;
            if (fileDownload.DownloadStatus == DownloadStatus.Cancelled) return;

            lock (_lockObject)
            {
                var index = _downloads.FindIndex(x => x == fileDownload);
                GridItems[index].DownloadStatus = e.ErrorMessage;
                GridItems[index].SearchStatus = Enum.GetName(typeof(SearchResult), SearchResult.Error); ;
                _currentRunningThreads = _currentRunningThreads <= 0 ? 0 : _currentRunningThreads - 1;

                _countUrlsDone++;
            }


            OnPropertyChanged(nameof(CurrentThreadsToShow));
            OnPropertyChanged(nameof(CountUrlsDoneToShow));

            if (fileDownload.DownloadStatus == DownloadStatus.Downloading ||
                fileDownload.DownloadStatus == DownloadStatus.Downloaded ||
                fileDownload.DownloadStatus == DownloadStatus.Error)
                AccessTheWeb();

        }

        private void FileDownload_DownloadCompleted(object sender, DownloadCompletedEventArgs e)
        {
            var fileDownload = sender as FileDownload;
            MatchCollection matches = _urlRegex.Matches(fileDownload.ResultStr);
            MatchCollection textMatches = _textRegex.Matches(fileDownload.ResultStr);

            lock (_lockObject)
            {
                var index = _downloads.FindIndex(x => x == fileDownload);
                GridItems[index].DownloadStatus = Enum.GetName(typeof(DownloadStatus), DownloadStatus.Downloaded);
                GridItems[index].Progress = 100;

                if (textMatches.Count > 0)
                {
                    _countFoundText++;
                    GridItems[index].SearchStatus = Enum.GetName(typeof(SearchResult), SearchResult.Found);
                }
                else
                    GridItems[index].SearchStatus = Enum.GetName(typeof(SearchResult), SearchResult.NotFound);

                foreach (Match item in matches)
                    if (!_urlQueue.Contains(item.Value))
                        _urlQueue.Enqueue(item.Value);

                _countUrlsDone++;
                _currentRunningThreads = _currentRunningThreads <= 0 ? 0 : _currentRunningThreads - 1;
            }


            OnPropertyChanged(nameof(CountUrlsDoneToShow));
            OnPropertyChanged(nameof(CurrentThreadsToShow));
            OnPropertyChanged(nameof(CountFoundText));

            if (fileDownload.DownloadStatus == DownloadStatus.Downloading || fileDownload.DownloadStatus == DownloadStatus.Downloaded)
                AccessTheWeb();
        }

        private void FileDownload_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            var fileDownload = sender as FileDownload;

            lock (_lockObject)
            {
                var index = this._downloads.FindIndex(x => x == fileDownload);
                GridItems[index].Progress = e.Progress;
            }

        }

      
        #endregion

        public void Dispose()
        {
            GridItems.Clear();
            _mutex.Dispose();
            _downloads.Clear();

            base.Dispose();
        }
    }
}

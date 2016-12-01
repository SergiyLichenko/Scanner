using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight.Messaging;
using Scanner.Infrastructure;

namespace Scanner.ViewModel
{
    public partial class MainWindowViewModel
    {

        private RelayCommand _addTaskButtonClick;
        private RelayCommand _startButtonClick;
        private RelayCommand _pauseButtonClick;
        private RelayCommand _stopButtonClick;
        private RelayCommand _startAllButtonClick;
        private RelayCommand _pauseAllButtonClick;
        private RelayCommand _stopAllButtonClick;

        private bool _startAllEnabled;
        private bool _pauseAllEnabled;
        private bool _stopALlEnabled;
        private bool _newTaskEnabled;
        private bool _isScrollable;
        private Visibility _statusBarVisibility;

        public bool StartAllEnabled
        {
            get { return _startAllEnabled; }
            set
            {
                _startAllEnabled = value;
                OnPropertyChanged(nameof(StartAllEnabled));
            }
        }

        public bool IsScrollable
        {
            get { return _isScrollable; }
            set
            {
                _isScrollable = value; 
                OnPropertyChanged(nameof(IsScrollable));
            }
        }
        public bool PauseAllEnabled
        {
            get { return _pauseAllEnabled; }
            set
            {
                _pauseAllEnabled = value;
                OnPropertyChanged(nameof(PauseAllEnabled));
            }
        }
        public bool StopAllEnabled
        {
            get { return _stopALlEnabled; }
            set
            {
                _stopALlEnabled = value;
                OnPropertyChanged(nameof(StopAllEnabled));
            }
        }
        public bool NewTaskEnabled
        {
            get { return _newTaskEnabled; }
            set
            {
                _newTaskEnabled = value;
                OnPropertyChanged(nameof(NewTaskEnabled));
            }
        }

        public Visibility StatusBoarVisibility
        {
            get { return _statusBarVisibility; }
            set
            {
                _statusBarVisibility = value;
                OnPropertyChanged(nameof(StatusBoarVisibility));
            }
        }
        public ICommand AddTaskButtonClickButtonClick
        {
            get
            {
                if (_addTaskButtonClick == null)
                    _addTaskButtonClick = new RelayCommand(ExecuteAddTaskButtonClick);
                return _addTaskButtonClick;
            }
        }

        public ICommand StartButtonClick
        {
            get
            {
                if (_startButtonClick == null)
                    _startButtonClick = new RelayCommand((obj) => Task.Run(() => Start(obj)));
                return _startButtonClick;
            }
        }
        public ICommand PauseButtonClick
        {
            get
            {
                if (_pauseButtonClick == null)
                    _pauseButtonClick = new RelayCommand((obj) => Task.Run(() => Pause(obj)));
                return _pauseButtonClick;
            }
        }
        public ICommand StopButtonClick
        {
            get
            {
                if (_stopButtonClick == null)
                    _stopButtonClick = new RelayCommand((obj) => Task.Run(() => Stop(obj)));
                return _stopButtonClick;
            }
        }
        public ICommand StartAllButtonClick
        {
            get
            {
                if (_startAllButtonClick == null)
                    _startAllButtonClick = new RelayCommand((obj) => Task.Run(() => StartAll(obj)));
                return _startAllButtonClick;
            }
        }
        public ICommand PauseAllButtonClick
        {
            get
            {
                if (_pauseAllButtonClick == null)
                    _pauseAllButtonClick = new RelayCommand((obj) => Task.Run(() => PauseAll(obj)));
                return _pauseAllButtonClick;
            }
        }
        public ICommand StopAllButtonClick
        {
            get
            {
                if (_stopAllButtonClick == null)
                    _stopAllButtonClick = new RelayCommand((obj) => Task.Run(() => StopAll(obj)));
                return _stopAllButtonClick;
            }
        }


        private void ExecuteAddTaskButtonClick(object obj)
        {
            Messenger.Default.Send<string>(String.Empty);
        }

        private void Start(object index)
        {
            int selectedIndex = (int?)index ?? DataGridSelectedIndex;
            var item = this._downloads[selectedIndex];

            if (selectedIndex < 0 || selectedIndex > _downloads.Count - 1 || item.DownloadStatus == DownloadStatus.Cancelled
                || item.DownloadStatus == DownloadStatus.Downloaded || item.DownloadStatus == DownloadStatus.Error) return;

            _downloads[selectedIndex].Start();
            this.GridItems[selectedIndex].DownloadStatus = Enum.GetName(typeof(DownloadStatus), DownloadStatus.Downloading); ;

        }

        private void Pause(object index)
        {

            int selectedIndex = (int?)index ?? DataGridSelectedIndex;

            if (selectedIndex < 0 || selectedIndex > _downloads.Count - 1 ||
                this._downloads[selectedIndex].DownloadStatus != DownloadStatus.Downloading) return;


            this._downloads[selectedIndex].Pause();
            this.GridItems[selectedIndex].DownloadStatus = Enum.GetName(typeof(DownloadStatus), DownloadStatus.Paused);

        }

       
        private void Stop(object index)
        {
            int selectedIndex = (int?)index ?? DataGridSelectedIndex;
            var item = _downloads[selectedIndex];
            if (selectedIndex < 0 || selectedIndex > _downloads.Count - 1 || item.DownloadStatus != DownloadStatus.Downloading)
                return;

            _mutex.WaitOne();
            this.GridItems[selectedIndex].DownloadStatus = Enum.GetName(typeof(DownloadStatus), DownloadStatus.Cancelled);
            this.GridItems[selectedIndex].SearchStatus = Enum.GetName(typeof(SearchResult), SearchResult.Cancelled);
            this._downloads[selectedIndex].Stop();
            _currentRunningThreads--;

            _mutex.ReleaseMutex();
            OnPropertyChanged(nameof(CurrentThreadsToShow));
        }

        private void StartAll(object obj)
        {
            Task.Run(() =>
           {
               StartAllEnabled = false;
               for (int i = 0; i < _downloads.Count; i++)
                   Start(i);
               _mutex.ReleaseMutex();
           }).ContinueWith((task) =>
           {
               PauseAllEnabled = true;
               StopAllEnabled = true;
           });

         

        }

        private void PauseAll(object obj)
        {
            Task.Run(() =>
                  {
                      PauseAllEnabled = false;
                      _mutex.WaitOne();
                      for (int i = 0; i < _downloads.Count; i++)
                          Pause(i);
                  }).ContinueWith((task) =>
                  {
                      _mutex.ReleaseMutex();
                  });
            StopAllEnabled = false;
            StartAllEnabled = true;

        }

        private void StopAll(object obj)
        {
            Task.Run(() =>
            {
                StopAllEnabled = false;
                _mutex.WaitOne();
                for (int i = 0; i < _downloads.Count; i++)
                    Stop(i);
                _mutex.ReleaseMutex();

            });
            _urlQueue = new ConcurrentQueue<string>();
            NewTaskEnabled = true;
            PauseAllEnabled = false;
            StartAllEnabled = false;
        }
    }
}

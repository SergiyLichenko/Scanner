using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight.Messaging;
using Scanner.Infrastructure;

namespace Scanner.ViewModel
{
    public class AddTaskWindowViewModel : ViewModelBase
    {
        private RelayCommand _okButtonClick;
        private ObservableCollection<int> _comboBoxNumbers;

        public ObservableCollection<int> ComboBoxNumbers
        {
            get
            {
                if (_comboBoxNumbers == null)
                {
                    _comboBoxNumbers = new ObservableCollection<int>();
                    for (int i = 1; i <= 50; i++)
                        _comboBoxNumbers.Add(i);
                }
                return _comboBoxNumbers;
            }
            set
            {
                _comboBoxNumbers = value;
                //OnPropertyChanged(nameof(ComboBoxNumbers));
            }
        }
        public int ComboBoxSelectedValue { get; set; }
        public string Text { get; set; }
        public string SearchedText { get; set; }
        public int MaxScannedUrls { get; set; }

        public AddTaskWindowViewModel()
        {
            this.Text = "https://msn.com";
            this.SearchedText = "Microsoft";

            this.MaxScannedUrls = 250;
            ComboBoxSelectedValue = 10;
        }
        public ICommand OkButtonClick
        {
            get
            {
                if (_okButtonClick == null)
                    _okButtonClick = new RelayCommand(ExecuteAddTaskButtonClick);
                return _okButtonClick;
            }
        }


        private void ExecuteAddTaskButtonClick(object obj)
        {
            Model.MyTask myTask = null;

            myTask = new Model.MyTask
            {
                URL = this.Text,
                Searched = this.SearchedText,
                MaxCountThreads = ComboBoxSelectedValue,
                MaxCountUrls = MaxScannedUrls
            };
            Messenger.Default.Send(myTask);
        }
    }
}

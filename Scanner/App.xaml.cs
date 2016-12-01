using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using GalaSoft.MvvmLight.Messaging;
using Scanner.Model;
using Scanner.ViewModel;
using Scanner.Views;

namespace Scanner
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        private AddTaskWindow _addTaskWindow;
        private MainWindow _mainWindow;
        public App()
        {
            _mainWindow = new MainWindow();
            Messenger.Default.Register(_mainWindow, new Action<string>(ShowAddTaskWindow));

            _addTaskWindow_Closed(null, null);
            _mainWindow.Closed += _mainWindow_Closed;
        }

        private void _mainWindow_Closed(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void _addTaskWindow_Closed(object sender, EventArgs e)
        {
            _addTaskWindow = new AddTaskWindow();
            _addTaskWindow.Closed += _addTaskWindow_Closed;
            Messenger.Default.Register(_addTaskWindow, new Action<MyTask>(StartTask));
        }

   

        private void StartTask(MyTask obj)
        {
            _addTaskWindow.Hide();
            ((MainWindowViewModel)_mainWindow.DataContext).AddTask(obj);
        }

        private void ShowAddTaskWindow(string input)
        {
            _addTaskWindow.Show();
        }

        private void EntryPoint(object sender, StartupEventArgs e)
        {
            _mainWindow.Show();
        }
    }


}

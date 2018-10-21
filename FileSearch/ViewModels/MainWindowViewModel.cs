using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace FileSearch.ViewModels
{
    public class MainWindowViewModel : FileSearch.EventINotifyPropertyChanged
    {


        private ICollection<string> discs = new List<string>();
        private FileSearch.Command.Command commandTheSearch;
        private FileSearch.Command.Command commandStopThread;
        private FileSearch.Command.Command commandPauseThread;
        private ICollection<FileViewModel> files = new ObservableCollection<FileViewModel>();
        private string mainDirectory;
        private string selectedItem;
        private Thread thread = null;

        public MainWindowViewModel()
        {
            DisksLoading();

            commandTheSearch = new FileSearch.Command.DelegateCommand(Search);
            commandStopThread = new FileSearch.Command.DelegateCommand(StopThread);
            commandPauseThread = new FileSearch.Command.DelegateCommand(PauseThread);
        }

        public ICommand CommandTheSearch => commandTheSearch;
        public IEnumerable<string> Discs => discs;
        public string SelectedItem
        {
            get => selectedItem;
            set
            {
                selectedItem = value;
                mainDirectory = selectedItem;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs(nameof(SelectedItem)));
            }
        }
        public IEnumerable<FileViewModel> Files => files;


        private void Search()
        {
           thread = new Thread(GetItemsSearching) { IsBackground = true };
            thread.Start();
        }

        private void StopThread()
        {
            thread.Abort();
        }

        private void PauseThread()
        {
            if (thread.IsAlive)
            {
                thread.Suspend();
            }
            else
            {
                thread.Resume();
            }
        }

        public void GetItemsSearching()
        {
            GetDirectories(selectedItem);
        }

        public void GetDirectories(string path)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (path != null)
                {
                    try
                    {
                        DirectoryInfo directoryInfo = new DirectoryInfo(path);
                        DirectoryInfo[] directoryInfos = directoryInfo.GetDirectories();

                        foreach (DirectoryInfo directory in directoryInfos)
                        {
                            FileInfo[] currentDirectoryFiles = directory.GetFiles();

                            foreach (FileInfo item in currentDirectoryFiles)
                            {
                                files.Add(new FileViewModel(item));
                            }

                            GetDirectories(directory.FullName);
                        }
                    }
                    catch (UnauthorizedAccessException)
                    {
                    }
                }
            });
        }

        public void DisksLoading()//+
        {
            DriveInfo[] driveInfos = DriveInfo.GetDrives();

            foreach (DriveInfo disc in driveInfos)
            {
                discs.Add(disc.Name);
            }
            selectedItem = driveInfos[0].Name;
        }
    }
}
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
        private bool commandSearchOn = false;
        private FileSearch.Command.Command commandStopThread;
        private bool commandStopOn = false;
        private FileSearch.Command.Command commandPauseThread;
        private bool commandPauseOn = false;
        private FileSearch.Command.Command commandResumeThread;
        private bool commandResumeOn = false;
        private ICollection<FileViewModel> files = new ObservableCollection<FileViewModel>();
        private ICollection<FileViewModel> fileBroker = new List<FileViewModel>();
        private string mainDirectory;
        private string selectedItem;
        private Thread threadForFileSearch = null;
        private string pathToFile = string.Empty;

        public MainWindowViewModel()
        {
            DisksLoading();
            commandTheSearch = new FileSearch.Command.DelegateCommand(Search, CommandSearchOn);
            commandStopThread = new FileSearch.Command.DelegateCommand(StopThread, CommandStopOn);
            commandPauseThread = new FileSearch.Command.DelegateCommand(PauseThread, CommandPauseOn);
            commandResumeThread = new FileSearch.Command.DelegateCommand(ResumeThread, CommandResumeOn);
        }

        public ICommand CommandTheSearch => commandTheSearch;

        public ICommand CommandStopThread => commandStopThread;

        public ICommand CommandPauseThread => commandPauseThread;

        public ICommand CommandResumeThread => commandResumeThread;

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

        public string PathToFile
        {
            get => pathToFile;
            set
            {
                if (pathToFile != value)
                {
                    pathToFile = value;
                    OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs(nameof(PathToFile)));
                    commandSearchOn = PathToFile.Length > 0;
                    commandTheSearch.OnCanExecuteChanged(EventArgs.Empty);
                }
            }
        }

        private bool CommandStopOn()
        {
            return commandStopOn;
        }

        private bool CommandPauseOn()
        {
            return commandPauseOn;
        }

        private bool CommandSearchOn()
        {
            return commandSearchOn;
        }

        private bool CommandResumeOn()
        {
            return commandResumeOn;
        }

        private void Search()
        {
            threadForFileSearch = new Thread(GetDirectories) { IsBackground = true };
            threadForFileSearch.Start(selectedItem);
            commandPauseOn = true;
            commandPauseThread.OnCanExecuteChanged(EventArgs.Empty);
            commandStopOn = true;
            commandStopThread.OnCanExecuteChanged(EventArgs.Empty);
        }

        private void StopThread()
        {
            threadForFileSearch.Abort();
            files.Clear();

            commandSearchOn = PathToFile.Length > 0;
            commandTheSearch.OnCanExecuteChanged(EventArgs.Empty);
            commandStopOn = false;
            commandStopThread.OnCanExecuteChanged(EventArgs.Empty);
            commandPauseOn = false;
            commandPauseThread.OnCanExecuteChanged(EventArgs.Empty);
            commandResumeOn = false;
            commandResumeThread.OnCanExecuteChanged(EventArgs.Empty);
        }

        private void PauseThread()
        {
            if (threadForFileSearch.IsAlive)
            {
                threadForFileSearch.Suspend();
                commandPauseOn = false;
                commandResumeOn = true;
                commandResumeThread.OnCanExecuteChanged(EventArgs.Empty);
            }
        }

        private void ResumeThread()
        {
            threadForFileSearch.Resume();
            commandPauseOn = true;
            commandResumeOn = false;
        }

        public void GetItemSearching(FileViewModel fileView)
        {
            files.Add(fileView);
        }

        public void GetDirectories(object stringPath)
        {
            string path = (string)stringPath;

            if (path != null)
            {
                try
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(path);
                    DirectoryInfo[] directoryInfos = directoryInfo.GetDirectories();

                    foreach (DirectoryInfo directory in directoryInfos)
                    {
                          FileInfo[] currentDirectoryFiles = directory.GetFiles(pathToFile);
                       // FileInfo[] currentDirectoryFiles = directory.GetFiles();

                        foreach (FileInfo item in currentDirectoryFiles)
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                GetItemSearching(new FileViewModel(item));
                            });
                        }

                        GetDirectories(directory.FullName);
                    }
                }
                catch (UnauthorizedAccessException)
                {
                }
            }
        }

        public void DisksLoading()
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
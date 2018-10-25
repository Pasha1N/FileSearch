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
        private object sync = new object();
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
        private string fileName = string.Empty;

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
            get => fileName;
            set
            {
                if (fileName != value)
                {
                    fileName = value;
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
            commandPauseOn = true;
            commandPauseThread.OnCanExecuteChanged(EventArgs.Empty);
            commandStopOn = true;
            commandStopThread.OnCanExecuteChanged(EventArgs.Empty);
            commandSearchOn = false;
            commandTheSearch.OnCanExecuteChanged(EventArgs.Empty);

            threadForFileSearch = new Thread(GetDirectories) { IsBackground = true };
            threadForFileSearch.Start(selectedItem);
        }

        private void StopThread()
        {
            files.Clear();
            if (threadForFileSearch.ThreadState.HasFlag(ThreadState.Suspended))
            {
                threadForFileSearch.Resume();
            }
            threadForFileSearch.Abort();
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
                commandPauseThread.OnCanExecuteChanged(EventArgs.Empty);
            }
        }

        private void ResumeThread()
        {
            threadForFileSearch.Resume();
            commandPauseOn = true;
            commandResumeOn = false;
            commandResumeThread.OnCanExecuteChanged(EventArgs.Empty);
            commandPauseThread.OnCanExecuteChanged(EventArgs.Empty);
        }

        public void GetItemSearching(FileViewModel fileView)
        {
            files.Add(fileView);
        }

        public void GetDirectories(object stringPath)
        {

            string path = (string)stringPath;
            DirectoryInfo directoryInfo = null;
            DirectoryInfo[] directoryInfos = null;

            if (path != null)
            {
                try
                {
                    directoryInfo = new DirectoryInfo(path);
                    directoryInfos = directoryInfo.GetDirectories();
                }
                catch (UnauthorizedAccessException)
                {
                }

                if (directoryInfos != null)
                {
                    foreach (DirectoryInfo directory in directoryInfos)
                    {
                        try
                        {
                            FileInfo[] currentDirectoryFiles = directory.GetFiles(fileName);

                            foreach (FileInfo item in currentDirectoryFiles)
                            {
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    GetItemSearching(new FileViewModel(item));
                                });
                            }
                        }
                        catch (UnauthorizedAccessException)
                        {
                        }

                        GetDirectories(directory.FullName);
                    }
                }
            }

            if (!commandPauseOn)
            {
                commandPauseOn = false;
                commandResumeOn = false;
                commandStopOn = false;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    commandResumeThread.OnCanExecuteChanged(EventArgs.Empty);
                    commandPauseThread.OnCanExecuteChanged(EventArgs.Empty);
                    commandStopThread.OnCanExecuteChanged(EventArgs.Empty);
                });
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
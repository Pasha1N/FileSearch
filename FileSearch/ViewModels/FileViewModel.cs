using System.IO;

namespace FileSearch.ViewModels
{
    public class FileViewModel
    {
        private string name;
        private string path;

        public FileViewModel(FileInfo fileInfo)
        {
            name = fileInfo.Name;
            path = fileInfo.FullName;
        }
        public string Name => name;
        public string Path => path;
    }
}
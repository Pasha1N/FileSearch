using System.IO;

namespace FileSearch.ViewModels
{
    public class FileViewModel
    {
        private readonly string name;
        private readonly string path;

        public FileViewModel(FileInfo fileInfo)
        {
            name = fileInfo.Name;
            path = fileInfo.FullName;
        }

        public string Name => name;

        public string Path => path;
    }
}
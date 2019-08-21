using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PP.Wpf
{
    public class FileAbstraction : IFileAbstraction
    {
        public IEnumerable<string> EnumerateTopDirectories(string path)
        {
            return Directory.EnumerateDirectories(path, "*", SearchOption.TopDirectoryOnly);
        }

        public IEnumerable<string> EnumerateFilesInDirectory(string path)
        {
            return Directory.EnumerateFiles(path, "*", SearchOption.TopDirectoryOnly);
        }

        public IEnumerable<string> GetDrives()
        {
            return DriveInfo.GetDrives().Where(d => d.DriveType == DriveType.Fixed).Select(d => d.RootDirectory.FullName);
        }
    }
}
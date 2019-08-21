using System.Collections.Generic;

namespace PP.Wpf
{
    public interface IFileAbstraction
    {
        IEnumerable<string> EnumerateTopDirectories(string path);
        IEnumerable<string> EnumerateFilesInDirectory(string path);
        IEnumerable<string> GetDrives();
    }
}

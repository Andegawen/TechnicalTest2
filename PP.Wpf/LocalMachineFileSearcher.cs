using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PP.Wpf
{
    public class LocalMachineFileSearcher
    {
        private readonly IFileAbstraction _fileAbstraction;

        public LocalMachineFileSearcher(IFileAbstraction fileAbstraction)
        {
            _fileAbstraction = fileAbstraction;
        }

        public IEnumerable<string> GetAll(CancellationToken token)
        {
            return GetPartial(_fileAbstraction.GetDrives(), token);
        }

        private IEnumerable<string> GetPartial(IEnumerable<string> directories, CancellationToken token)
        {
            foreach (var directory in directories)
            {
                foreach (var file in _fileAbstraction.EnumerateFilesInDirectory(directory))
                {
                    token.ThrowIfCancellationRequested();
                    yield return file;
                }

                foreach (var file in GetPartial(_fileAbstraction.EnumerateTopDirectories(directory), token))
                {
                    token.ThrowIfCancellationRequested();
                    yield return file;
                }
            }
        }

    }

}

using System;
using System.Collections.Generic;
using System.Threading;

namespace PP.Wpf
{
    public class LocalMachineFileSearcher
    {
        private readonly IFileAbstraction _fileAbstraction;

        public LocalMachineFileSearcher(IFileAbstraction fileAbstraction)
        {
            _fileAbstraction = fileAbstraction;
        }

        public void Start(CancellationToken token)
        {
            ResultCollection = new List<string>();
            try
            {
                GetPartial(_fileAbstraction.GetDrives(), token);
            }
            catch (OperationCanceledException e)
            {
                Console.WriteLine(e);
            }
        }

        public IList<string> ResultCollection { get; private set; }

        private void GetPartial(IEnumerable<string> directories, CancellationToken token)
        {
            foreach (var directory in directories)
            {
                token.ThrowIfCancellationRequested();

                foreach (var file in _fileAbstraction.EnumerateFilesInDirectory(directory))
                {
                    token.ThrowIfCancellationRequested();
                    ResultCollection.Add(file);
                }

                GetPartial(_fileAbstraction.EnumerateTopDirectories(directory), token);
            }
        }
    }

}

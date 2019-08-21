using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PP.Wpf
{
    public class LocalMachineFileSearcher
    {
        private readonly IFileAbstraction _fileAbstraction;

        public event Action<ProgressInfo> NotifyProgress;

        public LocalMachineFileSearcher(IFileAbstraction fileAbstraction)
        {
            _fileAbstraction = fileAbstraction;
        }

        public void Start(CancellationToken token)
        {
            ResultCollection = new List<string>();
            var t = Task.Factory.StartNew(() =>
            {
                try
                {
                    GetPartial(_fileAbstraction.GetDrives(), token);
                }
                catch (OperationCanceledException e)
                {
                    Console.WriteLine(e);
                }

                NotifyProgress?.Invoke(new ProgressInfo(100));
            });
        }

        public IList<string> ResultCollection { get; private set; }

        private void GetPartial(IEnumerable<string> directories, CancellationToken token)
        {
            try
            {
                foreach (var directory in directories)
                {
                    token.ThrowIfCancellationRequested();

                    try
                    {
                        foreach (var file in _fileAbstraction.EnumerateFilesInDirectory(directory))
                        {
                            token.ThrowIfCancellationRequested();
                            ResultCollection.Add(file);
                        }
                    }
                    catch (UnauthorizedAccessException e)
                    {
                        Console.WriteLine(e);
                    }

                    GetPartial(_fileAbstraction.EnumerateTopDirectories(directory), token);
                }
            }
            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine(e);
            }
        }
    }

    public class ProgressInfo
    {
        public int PercentageValue { get; }

        public ProgressInfo(int percentageValue)
        {
            PercentageValue = percentageValue;
        }
    }
}

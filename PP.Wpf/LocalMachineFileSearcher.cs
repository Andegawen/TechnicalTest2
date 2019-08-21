using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PP.Wpf
{
    public class LocalMachineFileSearcher
    {
        private readonly IFileAbstraction _fileAbstraction;

        public event Action<Completion> NotifyAboutCompletion;

        public LocalMachineFileSearcher(IFileAbstraction fileAbstraction)
        {
            _fileAbstraction = fileAbstraction;
        }

        public void Start(CancellationToken token)
        {
            ResultCollection = new ConcurrentBag<string>();
            var task = Task.Factory.StartNew(() =>
            {
                try
                {
                    GetPartial(_fileAbstraction.GetDrives(), token);
                }
                catch (OperationCanceledException e)
                {
                    Console.WriteLine(e);
                }
            }, token);

            task.ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    NotifyAboutCompletion?.Invoke(new Completion.ErrorOccured(t.Exception));
                }
                NotifyAboutCompletion?.Invoke(new Completion.Ended());
            });
        }

        public ConcurrentBag<string> ResultCollection { get; private set; }

        private void GetPartial(IEnumerable<string> directories, CancellationToken token)
        {
            try
            {
                var tasks = directories.Select(directory =>
                {
                    return Task.Factory.StartNew(() =>
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
                    });
                });
                var task = Task.WhenAll(tasks);
                task.GetAwaiter().GetResult();
            }
            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine(e);
            }
        }
    }

    public abstract class Completion
    {
        public sealed class Ended : Completion { }

        public sealed class ErrorOccured : Completion
        {
            public Exception Ex { get; }

            public ErrorOccured(Exception ex)
            {
                Ex = ex;
            }
        }
    }
}

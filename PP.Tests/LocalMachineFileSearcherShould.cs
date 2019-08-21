using System;
using System.Collections.Generic;
using System.Threading;
using FakeItEasy;
using NUnit.Framework;
using PP.Wpf;

namespace PP.Tests
{
    [TestFixture]
    public class LocalMachineFileSearcherShould
    {
        private Fake<IFileAbstraction> _fileAbstraction;
        private LocalMachineFileSearcher _sut;
        private AutoResetEvent _wait;

        [SetUp]
        public void InitializeFakeFileAbstraction()
        {
            _fileAbstraction = new Fake<IFileAbstraction>();
            _fileAbstraction
                .CallsTo(x => x.GetDrives()).Returns(new[] { "C:\\", "D:\\" });
            _sut = new LocalMachineFileSearcher(_fileAbstraction.FakedObject);
            _wait = new AutoResetEvent(false);
            _sut.NotifyAboutCompletion += (p) => { _wait.Set(); };
        }

        [Test]
        public void ReturnsAllFiles()
        {
            ProvideProperDirectories();
            ProvideFilesInDirectories();
            
            _sut.Start(CancellationToken.None);

            var expected = new[]
            {
                "C:\\f1.txt",
                "C:\\f2.txt",
                "C:\\Dir1\\f3.txt",
                "C:\\Dir1\\f4.txt",
                "C:\\Dir1\\Dir1_1\\f5.txt","D:\\f_d.txt"
            };
            Assert.IsTrue(_wait.WaitOne(TimeSpan.FromSeconds(1)),"Test should ended in less than 1 sec");
            Assert.That(_sut.ResultCollection, Is.EquivalentTo(expected));
        }

        [Test]
        public void ReturnsEmptyCollection_WhenThereIsNoFilesInAnyDirectory()
        {
            ProvideProperDirectories();

            _sut.Start(CancellationToken.None);

            Assert.IsTrue(_wait.WaitOne(TimeSpan.FromSeconds(1)), "Test should ended in less than 1 sec");
            Assert.That(_sut.ResultCollection, Is.EquivalentTo(new string[0]));
        }

        [Test]
        public void ShouldStopImmediatelyOnCancel()
        {
            var cts = new CancellationTokenSource();
            cts.Cancel();

            _sut.Start(cts.Token);

            Assert.IsTrue(_wait.WaitOne(TimeSpan.FromSeconds(1)), "Test should ended in less than 1 sec");
            Assert.That(_sut.ResultCollection, Is.EquivalentTo(new string[0]));
            _fileAbstraction.CallsTo(x => x.EnumerateTopDirectories(A<string>._))
                .MustNotHaveHappened();
            _fileAbstraction.CallsTo(x => x.EnumerateFilesInDirectory(A<string>._))
                .MustNotHaveHappened();
        }

        [Test]
        public void ReturnPartialResult_WhenCancelInvoked()
        {
            var cts = new CancellationTokenSource();
            IEnumerable<string> CancelInTheMiddleOfFileEnumeration()
            {
                yield return "C:\\f1.txt";
                cts.Cancel();
                yield return "C:\\f2.txt";
            }
            _fileAbstraction
                .CallsTo(x => x.EnumerateFilesInDirectory("C:\\"))
                .ReturnsLazily(CancelInTheMiddleOfFileEnumeration);

            _sut.Start(cts.Token);

            Assert.IsTrue(_wait.WaitOne(TimeSpan.FromSeconds(1)), "Test should ended in less than 1 sec");
            Assert.That(_sut.ResultCollection, Is.EquivalentTo(new[]{"C:\\f1.txt"}));
        }

        private void ProvideFilesInDirectories()
        {
            _fileAbstraction
                .CallsTo(x => x.EnumerateFilesInDirectory("C:\\"))
                .Returns(new[] { "C:\\f1.txt", "C:\\f2.txt" });
            _fileAbstraction
                .CallsTo(x => x.EnumerateFilesInDirectory("C:\\Dir1"))
                .Returns(new[] { "C:\\Dir1\\f3.txt", "C:\\Dir1\\f4.txt" });
            _fileAbstraction
                .CallsTo(x => x.EnumerateFilesInDirectory("C:\\Dir1\\Dir1_1"))
                .Returns(new[] { "C:\\Dir1\\Dir1_1\\f5.txt" });
            _fileAbstraction
                .CallsTo(x => x.EnumerateFilesInDirectory("C:\\Dir2"))
                .Returns(new string[0] );
            _fileAbstraction
                .CallsTo(x => x.EnumerateFilesInDirectory("D:\\"))
                .Returns(new[] { "D:\\f_d.txt" });

        }

        private void ProvideProperDirectories()
        {
            _fileAbstraction
                .CallsTo(x => x.EnumerateTopDirectories("C:\\"))
                .Returns(new[] { "C:\\Dir1", "C:\\Dir2" });
            _fileAbstraction
                .CallsTo(x => x.EnumerateTopDirectories("C:\\Dir1"))
                .Returns(new[] { "C:\\Dir1\\Dir1_1" });
            _fileAbstraction
                .CallsTo(x => x.EnumerateTopDirectories("C:\\Dir1\\Dir1_1"))
                .Returns(new string[0]);
            _fileAbstraction
                .CallsTo(x => x.EnumerateTopDirectories("C:\\Dir2"))
                .Returns(new string[0]);
            _fileAbstraction
                .CallsTo(x => x.EnumerateTopDirectories("D:\\"))
                .Returns(new string[0]);
        }
    }
}

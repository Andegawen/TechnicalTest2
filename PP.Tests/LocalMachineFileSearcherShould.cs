using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using NUnit.Framework;
using PP.Wpf;

namespace PP.Tests
{
    [TestFixture]
    public class LocalMachineFileSearcherShould
    {
        private Fake<IFileAbstraction> _fileAbstraction;

        [SetUp]
        public void InitializeFakeFileAbstraction()
        {
            _fileAbstraction = new Fake<IFileAbstraction>();
            _fileAbstraction
                .CallsTo(x => x.GetDrives()).Returns(new[] { "C:\\", "D:\\" });
        }

        [Test]
        public void ReturnsAllFiles()
        {
            ProvideProperDirectories();
            ProvideFilesInDirectories();
            var sut = new LocalMachineFileSearcher(_fileAbstraction.FakedObject);

            sut.Start(CancellationToken.None);

            var expected = new[]
            {
                "C:\\f1.txt",
                "C:\\f2.txt",
                "C:\\Dir1\\f3.txt",
                "C:\\Dir1\\f4.txt",
                "C:\\Dir1\\Dir1_1\\f5.txt","D:\\f_d.txt"
            };
            Assert.That(sut.ResultCollection, Is.EquivalentTo(expected));
        }

        [Test]
        public void ReturnsEmptyCollection_WhenThereIsNoFilesInAnyDirectory()
        {
            ProvideProperDirectories();
            var sut = new LocalMachineFileSearcher(_fileAbstraction.FakedObject);

            sut.Start(CancellationToken.None);

            Assert.That(sut.ResultCollection, Is.EquivalentTo(new string[0]));
        }

        [Test]
        public void BeAbleToStopRunningProcessImmediately()
        {
            _fileAbstraction
                .CallsTo(x => x.EnumerateFilesInDirectory("C:\\"))
                .Returns(new[] { "C:\\f1.txt", "C:\\f2.txt" });
            var sut = new LocalMachineFileSearcher(_fileAbstraction.FakedObject);

            var cts = new CancellationTokenSource();
            cts.Cancel();
            sut.Start(cts.Token);

            Assert.That(sut.ResultCollection, Is.EquivalentTo(new string[0]));
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
            var sut = new LocalMachineFileSearcher(_fileAbstraction.FakedObject);

            sut.Start(cts.Token);

            Assert.That(sut.ResultCollection, Is.EquivalentTo(new[]{"C:\\f1.txt"}));
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

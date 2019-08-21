using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using PP.Wpf.UI;

namespace PP.Wpf
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public MainViewModel()
        {
            Collection = new ObservableCollection<string>();
            Collection.Add("b");
            Collection.Add("xxx");
            Collection.Add("yyy");
            Collection.Add("a");
        }

        public MainViewModel(LocalMachineFileSearcher fileSearcher)
        {
            Collection = new ObservableCollection<string>();
            _fileSearcher = fileSearcher;
            _fileSearcher.NotifyAboutCompletion += NotifyAboutCompletion;
            StartCommand = new DelegateCommand(_=>
            {
                _currentCancellationTokenSource = new CancellationTokenSource();
                Pending = true;
                CurrentDirectory = "";
                Collection = new ObservableCollection<string>();
                _dispatcherTimer.Start();
                _fileSearcher.Start(_currentCancellationTokenSource.Token);
            });
            StopCommand = new DelegateCommand(_ =>
            {
                _currentCancellationTokenSource?.Cancel();
            });

            _dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            _dispatcherTimer.Tick += dispatcherTimer_Tick;
            _dispatcherTimer.Interval = TimeSpan.FromSeconds(5);
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            _fileSearcher.ResultCollection.ToList()
                .Except(Collection).ToList()
                .ForEach(el=>Collection.Add(el));
        }

        private void NotifyAboutCompletion(Completion completion)
        {
            switch (completion)
            {
                case Completion.ErrorOccured error:
                    MessageBox.Show(error.Ex.Message, "Unexpected error occured");
                    break;
                case Completion.Ended _:
                    Pending = false;
                    Collection = new ObservableCollection<string>(_fileSearcher.ResultCollection);
                    break;
                default:
                    throw new NotSupportedException("No other Completion possible");
            }
        }

        public bool Pending
        {
            get => _pending;
            set
            {
                _pending = value;
                OnPropertyChanged();
            }
        }

        private readonly LocalMachineFileSearcher _fileSearcher;

        private CancellationTokenSource _currentCancellationTokenSource = null;
        private bool _pending;
        private ObservableCollection<string> _collection;
        private string _currentDirectory;
        private readonly DispatcherTimer _dispatcherTimer;
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<string> Collection
        {
            get => _collection;
            set
            {
                _collection = value;
                OnPropertyChanged();
            }
        }

        public DelegateCommand StartCommand { get; set; }

        public DelegateCommand StopCommand { get; set; }

        public string CurrentDirectory
        {
            get => _currentDirectory;
            set
            {
                _currentDirectory = value;
                OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
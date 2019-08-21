using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Input;

namespace PP.Wpf
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public MainViewModel()
        {
            Collection = new ObservableCollection<string>();
            Collection.Add("xxx");
            Collection.Add("yyy");
        }

        public MainViewModel(LocalMachineFileSearcher fileSearcher)
        {
            Collection = new ObservableCollection<string>();
            _fileSearcher = fileSearcher;
            _fileSearcher.NotifyProgress += NotifyProgress;
            StartCommand = new DelegateCommand(_=>
            {
                _currentCancellationTokenSource = new CancellationTokenSource();
                Pending = true;
                _fileSearcher.Start(_currentCancellationTokenSource.Token);
            });
            StopCommand = new DelegateCommand(_ =>
            {
                _currentCancellationTokenSource?.Cancel();
            });
        }

        private void NotifyProgress(ProgressInfo obj)
        {
            if (obj.PercentageValue == 100)
            {
                Pending = false;
                Collection = new ObservableCollection<string>(_fileSearcher.ResultCollection);
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

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
using Livet;
using Livet.EventListeners;
using Livet.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Metrooz.ViewModels
{
    using Models;

    public class StreamManagerViewModel : ViewModel
    {
        public StreamManagerViewModel(Account account)
        {
            _selectedIndex = -1;
            _accountModel = account;
            _streamManagerModel = account.Stream;
            _streams = ViewModelHelper.CreateReadOnlyDispatcherCollection(
                _streamManagerModel.Streams, item => new StreamViewModel(item), App.Current.Dispatcher);

            CompositeDisposable.Add(_thisPropChangedEventListener = new PropertyChangedEventListener(this));
            _thisPropChangedEventListener.Add(() => IsActive, IsActive_PropertyChanged);
        }
        public StreamManagerViewModel()
        {
            _selectedIndex = 0;
            _streams = new ReadOnlyDispatcherCollection<StreamViewModel>(
                new DispatcherCollection<StreamViewModel>(
                new ObservableCollection<StreamViewModel>(
                    Enumerable.Range(0, 3).Select(idx => new StreamViewModel())), App.Current.Dispatcher));
        }
        readonly object lockObj_selectedIndex = new object();
        readonly PropertyChangedEventListener _thisPropChangedEventListener;
        readonly Account _accountModel;
        readonly StreamManager _streamManagerModel;
        readonly ReadOnlyDispatcherCollection<StreamViewModel> _streams;
        bool _isActive;
        int _selectedIndex, _subSelectedIndex;

        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                _isActive = value;
                RaisePropertyChanged(() => IsActive);
            }
        }
        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                lock (lockObj_selectedIndex)
                {
                    var newValue = value;
                    var oldValue = _selectedIndex;
                    if (newValue == oldValue)
                        return;
                    _selectedIndex = value;
                    if (oldValue >= 0 && oldValue < Items.Count)
                        Task.Run(() => Items[oldValue].Activate(false));
                    RaisePropertyChanged(() => SelectedIndex);
                    if (newValue >= 0 && newValue < Items.Count)
                        Task.Run(() => Items[newValue].Activate(true));
                }
            }
        }
        public ReadOnlyDispatcherCollection<StreamViewModel> Items { get { return _streams; } }
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            Items.Dispose();
        }
        void IsActive_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (IsActive)
                SelectedIndex = _subSelectedIndex;
            else
            {
                _subSelectedIndex = _selectedIndex;
                SelectedIndex = -1;
            }   
        }
    }
}
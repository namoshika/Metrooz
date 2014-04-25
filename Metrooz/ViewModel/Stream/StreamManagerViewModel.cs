using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using SunokoLibrary.Collections.ObjectModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Windows.Threading;

namespace Metrooz.ViewModel
{
    using Model;

    public class StreamManagerViewModel : ViewModelBase
    {
        public StreamManagerViewModel(Account account)
        {
            _selectedCircleIndex = -1;
            _accountModel = account;
            _streamManagerModel = account.Stream;
            _streams = new ObservableCollection<StreamViewModel>();
            _streamSyncer = _streamManagerModel.Streams.SyncWith(
                _streams, item => new StreamViewModel(item, _accountModel, _streamManagerModel),
                _stream_ChangedDisplayStreams, item => item.Cleanup(), App.Current.Dispatcher);
            PropertyChanged += StreamManagerViewModel_PropertyChanged;
        }
        readonly Account _accountModel;
        readonly StreamManager _streamManagerModel;
        readonly ObservableCollection<StreamViewModel> _streams;
        readonly System.Threading.SemaphoreSlim _syncerStreams = new System.Threading.SemaphoreSlim(1, 1);
        bool _isActive;
        int _selectedCircleIndex, _subSelectedCircleIndex;
        IDisposable _streamSyncer;

        public bool IsActive
        {
            get { return _isActive; }
            set { Set(() => IsActive, ref _isActive, value); }
        }
        public int SelectedIndex
        {
            get { return _selectedCircleIndex; }
            set { Set(() => SelectedIndex, ref _selectedCircleIndex, value); }
        }
        public ObservableCollection<StreamViewModel> Items { get { return _streams; } }
        public async override void Cleanup()
        {
            try
            {
                await _syncerStreams.WaitAsync();
                base.Cleanup();
                if (_streamSyncer != null)
                    _streamSyncer.Dispose();
                foreach (var item in Items)
                    item.Cleanup();
            }
            finally { _syncerStreams.Release(); }
        }

        async void _stream_ChangedDisplayStreams(Func<System.Threading.Tasks.Task> syncProc, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            try
            {
                await _syncerStreams.WaitAsync().ConfigureAwait(false);
                await syncProc();
                if (SelectedIndex < 0 && _streams.Count > 0)
                    SelectedIndex = 0;
                else if (_streams.Count == 0)
                    SelectedIndex = -1;
            }
            finally { _syncerStreams.Release(); }
        }
        void StreamManagerViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch(e.PropertyName)
            {
                case "IsActive":
                    if (IsActive == false)
                    {
                        _subSelectedCircleIndex = _selectedCircleIndex;
                        SelectedIndex = -1;
                    }
                    else
                        SelectedIndex = _subSelectedCircleIndex;
                    break;
            }
        }
    }
}
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Windows.Threading;

namespace GPlusBrowser.ViewModel
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

            _streamManagerModel.Streams.CollectionChanged += _stream_ChangedDisplayStreams;
        }
        int _selectedCircleIndex;
        Account _accountModel;
        StreamManager _streamManagerModel;
        ObservableCollection<StreamViewModel> _streams;
        readonly System.Threading.SemaphoreSlim _syncerStreams = new System.Threading.SemaphoreSlim(1, 1);

        public int SelectedIndex
        {
            get { return _selectedCircleIndex; }
            set { Set(() => SelectedIndex, ref _selectedCircleIndex, value); }
        }
        public ObservableCollection<StreamViewModel> Items
        {
            get { return _streams; }
            set { Set(() => Items, ref _streams, value); }
        }
        public async override void Cleanup()
        {
            try
            {
                await _syncerStreams.WaitAsync();
                base.Cleanup();
                _streamManagerModel.Streams.CollectionChanged -= _stream_ChangedDisplayStreams;
                foreach (var item in Items)
                    item.Cleanup();
            }
            finally { _syncerStreams.Release(); }
        }

        async void _stream_ChangedDisplayStreams(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            try
            {
                await _syncerStreams.WaitAsync().ConfigureAwait(false);
                switch (e.Action)
                {
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                        for (var i = 0; i < e.NewItems.Count; i++)
                        {
                            var circle = (Stream)e.NewItems[i];
                            var circleVm = new StreamViewModel(circle, _accountModel, _streamManagerModel);
                            await Items.InsertOnDispatcher(e.NewStartingIndex + i, circleVm).ConfigureAwait(false);
                        }
                        break;
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                        await Items.MoveOnDispatcher(e.OldStartingIndex, e.NewStartingIndex).ConfigureAwait(false);
                        break;
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                        for (var i = 0; i < e.OldItems.Count; i++)
                            await Items.RemoveAtOnDispatcher(e.OldStartingIndex).ConfigureAwait(false);
                        break;
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                        await Items.ClearOnDispatcher().ConfigureAwait(false);
                        break;
                }
                if (SelectedIndex < 0 && _streams.Count > 0)
                    SelectedIndex = 0;
                else if (_streams.Count == 0)
                    SelectedIndex = -1;
            }
            finally { _syncerStreams.Release(); }
        }
    }
}
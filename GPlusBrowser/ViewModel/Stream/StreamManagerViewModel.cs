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
        public StreamManagerViewModel(StreamManager streamManager, Account account)
        {
            _selectedCircleIndex = -1;
            _accountModel = account;
            _streamManagerModel = streamManager;
            _displayStreams = new ObservableCollection<StreamViewModel>(
                _streamManagerModel.Streams.Select(vm => new StreamViewModel(vm, account, streamManager))); ;

            _streamManagerModel.Streams.CollectionChanged += _stream_ChangedDisplayStreams;

            if (_displayStreams.Count > 0)
                SelectedCircleIndex = 0;
        }
        int _selectedCircleIndex;
        Account _accountModel;
        StreamManager _streamManagerModel;
        ObservableCollection<StreamViewModel> _displayStreams;

        public int SelectedCircleIndex
        {
            get { return _selectedCircleIndex; }
            set { Set(() => SelectedCircleIndex, ref _selectedCircleIndex, value); }
        }
        public ObservableCollection<StreamViewModel> DisplayStreams
        {
            get { return _displayStreams; }
            set { Set(() => DisplayStreams, ref _displayStreams, value); }
        }
        public override void Cleanup()
        {
            lock (_displayStreams)
            {
                base.Cleanup();
                _streamManagerModel.Streams.CollectionChanged -= _stream_ChangedDisplayStreams;
                foreach (var item in DisplayStreams)
                    item.Cleanup();
            }
        }

        void _stream_ChangedDisplayStreams(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            lock (_displayStreams)
                switch (e.Action)
                {
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                        for (var i = 0; i < e.NewItems.Count; i++)
                        {
                            var circle = (Stream)e.NewItems[i];
                            var circleVm = new StreamViewModel(circle, _accountModel, _streamManagerModel);
                            DisplayStreams.InsertOnDispatcher(e.NewStartingIndex + i, circleVm);
                        }
                        break;
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                        DisplayStreams.MoveOnDispatcher(e.OldStartingIndex, e.NewStartingIndex);
                        break;
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                        for (var i = 0; i < e.OldItems.Count; i++)
                            DisplayStreams.RemoveAtOnDispatcher(e.OldStartingIndex);
                        break;
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                        DisplayStreams.ClearOnDispatcher();
                        break;
                }
        }
    }
}
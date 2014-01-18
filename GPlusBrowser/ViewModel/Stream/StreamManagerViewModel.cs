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
        public StreamManagerViewModel(StreamManager streamManager)
        {
            _selectedCircleIndex = -1;
            _streamManagerModel = streamManager;
            ((INotifyCollectionChanged)_streamManagerModel.Streams).CollectionChanged += _stream_ChangedDisplayStreams;
            _displayStreams = new ObservableCollection<StreamViewModel>(
                _streamManagerModel.Streams.Select(vm => new StreamViewModel(vm))); ;

            if (_displayStreams.Count > 0)
                SelectedCircleIndex = 0;
        }
        bool _isError;
        int _selectedCircleIndex;
        StreamManager _streamManagerModel;
        ObservableCollection<StreamViewModel> _displayStreams;

        public bool IsError
        {
            get { return _isError; }
            set { Set(() => IsError, ref _isError, value); }
        }
        public int SelectedCircleIndex
        {
            get { return _selectedCircleIndex; }
            set
            {
                if (value > -1)
                    _displayStreams[value].Connect();
                Set(() => SelectedCircleIndex, ref _selectedCircleIndex, value);
            }
        }
        public ObservableCollection<StreamViewModel> DisplayStreams
        {
            get { return _displayStreams; }
            set { Set(() => DisplayStreams, ref _displayStreams, value); }
        }
        public override void Cleanup()
        {
            base.Cleanup();
            if (_streamManagerModel == null)
                return;

            SelectedCircleIndex = -1;
            ((INotifyCollectionChanged)_streamManagerModel.Streams).CollectionChanged -= _stream_ChangedDisplayStreams;
            _streamManagerModel = null;

            foreach (var item in DisplayStreams)
                item.Cleanup();
            DisplayStreams = null;
        }

        void _stream_ChangedDisplayStreams(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            lock(_displayStreams)
                switch (e.Action)
                {
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                        for (var i = 0; i < e.NewItems.Count; i++)
                        {
                            var circle = (Stream)e.NewItems[i];
                            var circleVm = new StreamViewModel(circle);
                            DisplayStreams.InsertAsync(e.NewStartingIndex + i, circleVm, App.Current.Dispatcher);
                        }
                        break;
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                        DisplayStreams.MoveAsync(e.OldStartingIndex, e.NewStartingIndex, App.Current.Dispatcher);
                        break;
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                        for (var i = 0; i < e.OldItems.Count; i++)
                            DisplayStreams.RemoveAtAsync(e.OldStartingIndex, App.Current.Dispatcher);
                        break;
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                        DisplayStreams.ClearAsync(App.Current.Dispatcher);
                        break;
                }
        }
    }
}
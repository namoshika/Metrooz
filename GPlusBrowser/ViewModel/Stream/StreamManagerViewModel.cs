using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Threading;

namespace GPlusBrowser.ViewModel
{
    using Model;

    public class StreamManagerViewModel : ViewModelBase, IDisposable
    {
        public StreamManagerViewModel(StreamManager streamManager, Dispatcher uiThreadDispatcher)
            : base(uiThreadDispatcher)
        {
            _streamManagerModel = streamManager;
            _streamManagerModel.ChangedDisplayStreams += _stream_ChangedDisplayStreams;
            _streamManagerModel.ChangedSelectedCircleIndex += _streamManagerModel_ChangedSelectedCircleIndex;
            _displayStreams = new DispatchObservableCollection<StreamViewModel>(uiThreadDispatcher);
            _selectedCircleIndex = -1;
        }
        int _selectedCircleIndex;
        StreamManager _streamManagerModel;
        DispatchObservableCollection<StreamViewModel> _displayStreams;

        public int SelectedCircleIndex
        {
            get { return _selectedCircleIndex; }
            set
            {
                if (_selectedCircleIndex == value)
                    return;
                _selectedCircleIndex = value;
                _streamManagerModel.SelectedCircleIndex = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("SelectedCircleIndex"));
            }
        }
        public DispatchObservableCollection<StreamViewModel> DisplayStreams
        {
            get { return _displayStreams; }
            set
            {
                _displayStreams = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("DisplayStreams"));
            }
        }
        public void Dispose()
        {
            foreach (var item in DisplayStreams)
                item.Dispose();
        }

        void _stream_ChangedDisplayStreams(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    foreach (var item in _displayStreams
                        .Where(streamVm => streamVm.Order >= e.NewStartingIndex + 1))
                        item.Order += e.NewItems.Count;
                    for (var i = 0; i < e.NewItems.Count; i++)
                    {
                        var circle = (Stream)e.NewItems[i];
                        var circleVm = new CircleStreamViewModel(circle, e.NewStartingIndex + i, UiThreadDispatcher);
                        circleVm.Order = e.NewStartingIndex + i + 1;
                        DisplayStreams.Add(circleVm);
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                    var max = Math.Max(e.NewStartingIndex, e.OldStartingIndex);
                    var min = Math.Min(e.NewStartingIndex, e.OldStartingIndex);
                    foreach (var item in _displayStreams.Where(strmVm => strmVm.Order >= min && strmVm.Order < max))
                        item.Order++;
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    for (var i = 0; i < e.OldItems.Count; i++)
                        DisplayStreams.RemoveAt(e.OldStartingIndex);
                    foreach (var item in _displayStreams.Where(strmVm => strmVm.Order >= e.OldStartingIndex))
                        item.Order -= e.NewItems.Count;
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    DisplayStreams.Clear();
                    break;
            }
        }
        void _streamManagerModel_ChangedSelectedCircleIndex(object sender, EventArgs e)
        {
            SelectedCircleIndex = _streamManagerModel.SelectedCircleIndex;
        }
    }
}
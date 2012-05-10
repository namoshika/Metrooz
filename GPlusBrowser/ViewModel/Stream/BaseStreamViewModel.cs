using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using SunokoLibrary.GooglePlus;

namespace GPlusBrowser.ViewModel
{
    using Model;

    public class BaseStreamViewModel : StreamViewModel
    {
        public BaseStreamViewModel(StreamManager streamManagerModel, Dispatcher uiThreadDispatcher)
            : base(uiThreadDispatcher)
        {
            _streamManagerModel = streamManagerModel;
            _streamManagerModel.Initialized += _streamManagerModel_Initialized;
            _streamManagerModel.ChangedSelectedCircleIndex += _manager_ChangedSelectedCircleIndex;
            _selectedCircleIndex = -1;
            DisplayStreams = new DispatchObservableCollection<StreamViewModel>(uiThreadDispatcher);
        }
        StreamManager _streamManagerModel;
        int _selectedCircleIndex;
        DispatchObservableCollection<StreamViewModel> _displayStreams;

        public int SelectedCircleIndex
        {
            get { return _selectedCircleIndex; }
            set
            {
                if (_selectedCircleIndex == value)
                    return;
                _selectedCircleIndex = value;
                if (value >= 0)
                {
                    CircleName = string.Format("BaseStream({0})", DisplayStreams[value].CircleName);
                    if (_streamManagerModel.CircleStreams.Count > 0 && !_streamManagerModel.CircleStreams[value].IsRefreshed)
                    {
                        _streamManagerModel.CircleStreams[value].Refresh();
                    }
                }
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
        public override void Dispose()
        {
            foreach (var item in DisplayStreams)
                item.Dispose();
        }
        void _streamManagerModel_Initialized(object sender, EventArgs e)
        {
            DisplayStreams.Clear();
            var orderIdx = 0;
            foreach (var item in _streamManagerModel.CircleStreams)
                DisplayStreams.Add(new CircleStreamViewModel(item, orderIdx++, UiThreadDispatcher));
        }
        void _manager_ChangedSelectedCircleIndex(object sender, EventArgs e)
        {
            SelectedCircleIndex = _streamManagerModel.SelectedCircleIndex;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows.Threading;

namespace GPlusBrowser.ViewModel
{
    using Model;

    public class StreamItemViewModel : ViewModelBase
    {
        public StreamItemViewModel(StreamManager streamManagerModel, int targetStreamIndex, Dispatcher uiThreadDispatcher)
            : base(uiThreadDispatcher)
        {
            _targetStreamIndex = targetStreamIndex;
            _targetStreamName = streamManagerModel.CircleStreams[targetStreamIndex].Name;
            _streamManagerModel = streamManagerModel;
            _openTargetStreamCommand = new RelayCommand(OpenTargetStreamCommand_Executed);
        }
        int _targetStreamIndex;
        string _targetStreamName;
        StreamManager _streamManagerModel;
        ICommand _openTargetStreamCommand;

        public int TargetStreamIndex
        {
            get { return _targetStreamIndex; }
            set
            {
                _targetStreamIndex = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("TargetStreamIndex"));
            }
        }
        public string TargetStreamName
        {
            get { return _targetStreamName; }
            set
            {
                _targetStreamName = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("TargetStreamName"));
            }
        }
        public ICommand OpenTargetStreamCommand
        {
            get { return _openTargetStreamCommand; }
            set
            {
                _openTargetStreamCommand = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("OpenTargetStreamCommand"));
            }
        }

        void OpenTargetStreamCommand_Executed(object arg)
        {
            _streamManagerModel.SelectedCircleIndex = TargetStreamIndex;
        }
    }
}

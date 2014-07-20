using Livet;
using Livet.EventListeners;
using Livet.Commands;
using SunokoLibrary.Collections.ObjectModel;
using SunokoLibrary.Web.GooglePlus;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace Metrooz.ViewModels
{
    using Models;

    public class StreamViewModel : ViewModel
    {
        public StreamViewModel(Stream circle)
        {
            _circleModel = circle;
            _name = circle.Name;
            _status = StreamStateType.UnLoaded;
            ReconnectCommand = new ViewModelCommand(ReconnectCommand_Executed);
            ResumeCommand = new ViewModelCommand(ReconnectCommand_Executed);
            ResumeButton = new ResumeButtonViewModel() { ClickCommand = ReconnectCommand };
            CompositeDisposable.Add(_modelPropChangedEventListener = new PropertyChangedEventListener(circle));
            CompositeDisposable.Add(_thisPropChangedEventListener = new PropertyChangedEventListener(this));
            CompositeDisposable.Add(Activities = ViewModelHelper.CreateReadOnlyDispatcherCollection<Activity, ViewModel>(
                _circleModel.Activities, item => new ActivityViewModel(item, _isActive), App.Current.Dispatcher));

            _modelPropChangedEventListener.Add(() => circle.Status, Model_Status_PropertyChanged);
            _modelPropChangedEventListener.Add(() => circle.ChangedActivityCount, Model_ChangedActivityCount_PropertyChanged);
            _thisPropChangedEventListener.Add(() => IsActive, IsActive_PropertyChanged);
            _thisPropChangedEventListener.Add(() => ScrollOffset, ScrollOffset_PropertyChanged);
        }
        public StreamViewModel()
        {
            _name = "StreamName";
            _status = StreamStateType.UnLoaded;
            CompositeDisposable.Add(Activities = new ReadOnlyDispatcherCollection<ViewModel>(
                new DispatcherCollection<ViewModel>(
                new ObservableCollection<ViewModel>(
                    Enumerable.Range(0, 3).Select(idx => new ActivityViewModel())), App.Current.Dispatcher)));
            ResumeButton = new ResumeButtonViewModel();
        }
        const double SCROLL_OFFSET_PAUSE_STREAM = 150;
        readonly PropertyChangedEventListener _modelPropChangedEventListener;
        readonly PropertyChangedEventListener _thisPropChangedEventListener;
        readonly System.Threading.SemaphoreSlim _activitiesSemaph = new System.Threading.SemaphoreSlim(1, 1);
        readonly Stream _circleModel;
        bool _isActive;
        double _scrollOffset;
        string _name;
        StreamStateType _status;

        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                _isActive = value;
                RaisePropertyChanged(() => IsActive);
            }
        }
        public double ScrollOffset
        {
            get { return _scrollOffset; }
            set
            {
                _scrollOffset = value;
                RaisePropertyChanged(() => ScrollOffset);
            }
        }
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                RaisePropertyChanged(() => Name);
            }
        }
        public StreamStateType Status
        {
            get { return _status; }
            set
            {
                _status = value;
                RaisePropertyChanged();
            }
        }
        public ReadOnlyDispatcherCollection<ViewModel> Activities { get; private set; }
        public ResumeButtonViewModel ResumeButton { get; private set; }
        public ICommand ReconnectCommand { get; private set; }
        public ICommand ResumeCommand { get; private set; }

        async void Activate()
        {
            await Task.Run(async () =>
                {
                    try
                    {
                        await _activitiesSemaph.WaitAsync().ConfigureAwait(false);
                        await App.Current.Dispatcher.InvokeAsync(async () =>
                            {
                                //Activitiesが変更されるのはDispatcher上なので、こちらもDispatcher上で
                                //処理する事で列挙中にActivities変更に出くわさないようにする。
                                if (Activities.Count > 0)
                                    await Task.Factory.ContinueWhenAll(Activities.OfType<ActivityViewModel>()
                                        .Select(item => item.Refresh(IsActive)).ToArray(), tsks => { });
                            });
                        if (IsActive == false)
                            return;
                        switch (_circleModel.Status)
                        {
                            case StreamStateType.UnLoaded:
                            case StreamStateType.Paused:
                                await _circleModel.Activate();
                                break;
                        }
                    }
                    finally
                    { _activitiesSemaph.Release(); }
                });
        }
        protected async override void Dispose(bool disposing)
        {
            try
            {
                await _activitiesSemaph.WaitAsync().ConfigureAwait(false);
                if (Activities != null)
                    Activities.Dispose();
                base.Dispose(disposing);
            }
            finally
            { _activitiesSemaph.Release(); }
        }

        async void IsActive_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (ViewModelUtility.IsDesginMode)
                return;
            await Task.Factory.StartNew(Activate);
        }
        async void ScrollOffset_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (ViewModelUtility.IsDesginMode)
                return;
            if (ScrollOffset > SCROLL_OFFSET_PAUSE_STREAM)
                await Task.Run(async () => await _circleModel.Pause());
            else if (_circleModel.Status == StreamStateType.Connected)
                await Task.Run(async () => await _circleModel.Activate());
        }
        void Model_Status_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Status = _circleModel.Status;
            if (Status == StreamStateType.Paused)
                Activities.SourceCollection.Insert(0, ResumeButton);
            else
                Activities.SourceCollection.Remove(ResumeButton);
        }
        void Model_ChangedActivityCount_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        { ResumeButton.UnreadActivityCount = _circleModel.ChangedActivityCount; }
        async void ReconnectCommand_Executed() { await Task.Factory.StartNew(Activate); }
    }
    public class ResumeButtonViewModel : ViewModel
    {
        int _unreadActivityCount;
        public int UnreadActivityCount
        {
            get { return _unreadActivityCount; }
            set
            {
                _unreadActivityCount = value;
                RaisePropertyChanged(() => UnreadActivityCount);
            }
        }
        public ICommand ClickCommand { get; set; }
    }
}
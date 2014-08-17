using Livet;
using Livet.EventListeners;
using Livet.Commands;
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
            CompositeDisposable.Add(_activities = ViewModelHelper.CreateReadOnlyDispatcherCollection<Activity, ViewModel>(
                _circleModel.Activities, item => new ActivityViewModel(item, _isActive), App.Current.Dispatcher));

            _modelPropChangedEventListener.Add(() => circle.Status, Model_Status_PropertyChanged);
            _modelPropChangedEventListener.Add(() => circle.ChangedActivityCount, Model_ChangedActivityCount_PropertyChanged);
            _thisPropChangedEventListener.Add(() => ScrollOffset, ScrollOffset_PropertyChanged);
        }
        public StreamViewModel()
        {
            _name = "StreamName";
            _status = StreamStateType.UnLoaded;
            CompositeDisposable.Add((IDisposable)(Items = new ReadOnlyDispatcherCollection<ViewModel>(
                new DispatcherCollection<ViewModel>(
                new ObservableCollection<ViewModel>(
                    Enumerable.Range(0, 3).Select(idx => new ActivityViewModel())), App.Current.Dispatcher))));
            ResumeButton = new ResumeButtonViewModel();
        }
        const double SCROLL_OFFSET_PAUSE_STREAM = 150;
        readonly PropertyChangedEventListener _modelPropChangedEventListener;
        readonly PropertyChangedEventListener _thisPropChangedEventListener;
        readonly System.Threading.SemaphoreSlim _activitiesSemaph = new System.Threading.SemaphoreSlim(1, 1);
        readonly Stream _circleModel;
        readonly ReadOnlyDispatcherCollection<ViewModel> _activities;
        bool _isActive;
        double _scrollOffset;
        string _name;
        StreamStateType _status;
        ICollection<ViewModel> _items;

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
        public ICollection<ViewModel> Items
        {
            get { return _items; }
            set
            {
                if (_items == value)
                    return;
                _items = value;
                RaisePropertyChanged();
            }
        }
        public ResumeButtonViewModel ResumeButton { get; private set; }
        public ICommand ReconnectCommand { get; private set; }
        public ICommand ResumeCommand { get; private set; }

        public async Task Activate(bool isActive)
        {
            try
            {
                await _activitiesSemaph.WaitAsync().ConfigureAwait(false);
                await App.Current.Dispatcher.InvokeAsync(async () =>
                {
                    //Activitiesが変更されるのはDispatcher上なので、こちらもDispatcher上で
                    //処理する事で列挙中にActivities変更に出くわさないようにする。
                    if (Items.Count > 0)
                        await Task.Factory.ContinueWhenAll(Items.OfType<ActivityViewModel>()
                            .Select(item => Task.Run(() => item.Refresh(isActive))).ToArray(), tsks => { });
                });
                _isActive = isActive;
                if (isActive == false)
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
        }
        protected override void Dispose(bool disposing)
        {
            _isActive = false;
            base.Dispose(disposing);
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
            var value = _circleModel.Status;
            if (value == StreamStateType.Paused)
            {
                var timeline = new List<ViewModel>(_activities);
                timeline.Insert(0, ResumeButton);
                Items = timeline;
            }
            else
                Items = _activities;
            Status = value;
        }
        void Model_ChangedActivityCount_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        { ResumeButton.UnreadActivityCount = _circleModel.ChangedActivityCount; }
        async void ReconnectCommand_Executed() { await Task.Run(async () => await Activate(true)); }
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
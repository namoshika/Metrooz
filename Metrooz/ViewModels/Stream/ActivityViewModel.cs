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
using System.Windows.Media;
using System.Windows.Threading;

namespace Metrooz.ViewModels
{
    using Metrooz.Models;

    public class ActivityViewModel : ViewModel
    {
        public ActivityViewModel(Activity activity, bool isActive)
        {
            _model = activity;
            _isActive = isActive;
            _comments = ViewModelHelper.CreateReadOnlyDispatcherCollection(
                _model.Comments, item => new CommentViewModel(item, _isActive), App.Current.Dispatcher);
            _comments.CollectionChanged += _comments_CollectionChanged;
            SendCommentCommand = new ViewModelCommand(SendCommentCommand_Executed);
            CancelCommentCommand = new ViewModelCommand(CancelCommentCommand_Executed);
            CompositeDisposable.Add(_thisPropChangedEventListener = new PropertyChangedEventListener(this));

            var tsk = Refresh(isActive);
            _model.Updated += _activity_Refreshed;
            _thisPropChangedEventListener.Add(() => IsCheckedCommentsHeader, IsCheckedCommentsHeader_PropertyChanged);
        }
        public ActivityViewModel()
        {
            ActorIcon = SampleData.DataLoader.LoadImage("accountIcon00.png").Result;
            ActivityUrl = new Uri("https://plus.google.com");
            IsEnableCommentsHeader = true;
            IsCheckedCommentsHeader = false;
            IsOpenedCommentList = false;
            IsLoadingCommentList = true;
            CommentLength = 5;
            PostUserName = "PostUserName";
            PostDate = "00:00";
            _comments = new ReadOnlyDispatcherCollection<CommentViewModel>(
                new DispatcherCollection<CommentViewModel>(
                new ObservableCollection<CommentViewModel>(
                    Enumerable.Range(0, 2).Select(_ => new CommentViewModel())), App.Current.Dispatcher));
            PostContentInline = new StyleElement(StyleType.None, new[] { new TextElement("Activity Content Text") });
        }
        readonly System.Threading.SemaphoreSlim _activitySemaph = new System.Threading.SemaphoreSlim(1, 1);
        readonly System.Threading.SemaphoreSlim _loadingSemaph = new System.Threading.SemaphoreSlim(1, 1);
        readonly Activity _model;
        readonly PropertyChangedEventListener _thisPropChangedEventListener;
        readonly ReadOnlyDispatcherCollection<CommentViewModel> _comments;
        bool _isActive;
        bool _isEnableCommentsHeader, _isCheckedCommentsHeader;
        bool _isOpenedCommentList, _isLoadingCommentList;
        int _commentLength;
        string _postCommentText;
        string _postUserName;
        string _postDate;
        Uri _activityUrl;
        ImageSource _actorIcon;
        StyleElement _postContentInline;
        CommentPostBoxState _shareBoxStatus;
        AttachedContentViewModel _attachedContent;

        public ImageSource ActorIcon
        {
            get { return _actorIcon; }
            set
            {
                _actorIcon = value;
                RaisePropertyChanged(() => ActorIcon);
            }
        }
        public Uri ActivityUrl
        {
            get { return _activityUrl; }
            set
            {
                _activityUrl = value;
                RaisePropertyChanged(() => ActivityUrl);
            }
        }
        public bool IsEnableCommentsHeader
        {
            get { return _isEnableCommentsHeader; }
            set
            {
                _isEnableCommentsHeader = value;
                RaisePropertyChanged(() => IsEnableCommentsHeader);
            }
        }
        public bool IsCheckedCommentsHeader
        {
            get { return _isCheckedCommentsHeader; }
            set
            {
                _isCheckedCommentsHeader = value;
                RaisePropertyChanged(() => IsCheckedCommentsHeader);
            }
        }
        public bool IsOpenedCommentList
        {
            get { return _isOpenedCommentList; }
            set
            {
                _isOpenedCommentList = value;
                RaisePropertyChanged(() => IsOpenedCommentList);
            }
        }
        public bool IsLoadingCommentList
        {
            get { return _isLoadingCommentList; }
            set
            {
                _isLoadingCommentList = value;
                RaisePropertyChanged(() => IsLoadingCommentList);
            }
        }
        public int CommentLength
        {
            get { return _commentLength; }
            set
            {
                _commentLength = value;
                RaisePropertyChanged(() => CommentLength);
            }
        }
        public string PostUserName
        {
            get { return _postUserName; }
            set
            {
                _postUserName = value;
                RaisePropertyChanged(() => PostUserName);
            }
        }
        public string PostDate
        {
            get { return _postDate; }
            set
            {
                _postDate = value;
                RaisePropertyChanged(() => PostDate);
            }
        }
        public string PostCommentText
        {
            get { return _postCommentText; }
            set
            {
                _postCommentText = value;
                RaisePropertyChanged(() => PostCommentText);
            }
        }
        public AttachedContentViewModel AttachedContent
        {
            get { return _attachedContent; }
            set
            {
                _attachedContent = value;
                RaisePropertyChanged(() => AttachedContent);
            }
        }
        public ReadOnlyDispatcherCollection<CommentViewModel> Comments
        { get { return _comments; } }
        public StyleElement PostContentInline
        {
            get { return _postContentInline; }
            set
            {
                _postContentInline = value;
                RaisePropertyChanged(() => PostContentInline);
            }
        }
        public CommentPostBoxState ShareBoxStatus
        {
            get { return _shareBoxStatus; }
            set
            {
                _shareBoxStatus = value;
                RaisePropertyChanged(() => ShareBoxStatus);
            }
        }
        public ICommand SendCommentCommand { get; private set; }
        public ICommand CancelCommentCommand { get; private set; }
        public async Task Refresh(bool isActive)
        {
            try
            {
                await _activitySemaph.WaitAsync();
                await _model.CoreInfo.UpdateGetActivityAsync(false, ActivityUpdateApiFlag.GetActivities);
                if (_model.CoreInfo.PostStatus != PostStatusType.Removed)
                {
                    var postDate = TimeZone.CurrentTimeZone.ToLocalTime(_model.CoreInfo.PostDate);
                    ActivityUrl = _model.CoreInfo.PostUrl;
                    CommentLength = _model.CoreInfo.CommentLength;
                    IsEnableCommentsHeader = _model.CoreInfo.CommentLength > 2;
                    PostUserName = _model.CoreInfo.PostUser.Name;
                    PostDate = postDate >= DateTime.Today ? postDate.ToString("HH:mm") : postDate.ToString("yyyy/MM/dd");
                    PostContentInline = _model.CoreInfo.ParsedText;
                    _isActive = isActive;
                    if (isActive)
                    {
                        ActorIcon = await DataCacheDictionary.DownloadImage(
                            new Uri(_model.CoreInfo.PostUser.IconImageUrl.Replace("$SIZE_SEGMENT", "s40-c-k").Replace("$SIZE_NUM", "80")));
                        if (_model.CoreInfo.AttachedContent != null)
                            AttachedContent = await AttachedContentViewModel.Create(_model.CoreInfo.AttachedContent);
                    }
                    else
                    {
                        ActorIcon = null;
                        if (AttachedContent == null)
                        {
                            AttachedContent.Dispose();
                            AttachedContent = null;
                        }
                    }
                    if (_comments.Count > 0)
                        await App.Current.Dispatcher.InvokeAsync(async () =>
                            {
                                //Activitiesが変更されるのはDispatcher上なので、こちらもDispatcher上で
                                //処理する事で列挙中にActivities変更に出くわさないようにする。
                                await Task.Factory.ContinueWhenAll(
                                    _comments.Select(item => item.Refresh(isActive)).ToArray(), tsks => { });
                            });
                }
            }
            catch (FailToOperationException) { }
            finally { _activitySemaph.Release(); }
        }
        protected async override void Dispose(bool disposing)
        {
            try
            {
                await _activitySemaph.WaitAsync();
                base.Dispose(disposing);
                if (_model != null)
                    _model.Updated -= _activity_Refreshed;
                if (_comments != null)
                    _comments.Dispose();
            }
            finally { _activitySemaph.Release(); }
        }

        void _activity_Refreshed(object sender, EventArgs e)
        { var tsk = App.Current.Dispatcher.InvokeAsync(async () => await Refresh(_isActive)); }
        async void _comments_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            try
            {
                await _activitySemaph.WaitAsync();
                CommentLength = _model.CoreInfo.CommentLength;
                IsEnableCommentsHeader = _model.CoreInfo.CommentLength > 2;
            }
            finally { _activitySemaph.Release(); }
        }
        async void IsCheckedCommentsHeader_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (ViewModelUtility.IsDesginMode)
                return;
            if (IsCheckedCommentsHeader)
                try
                {
                    await _loadingSemaph.WaitAsync();
                    IsLoadingCommentList = true;
                    await Task.Run(() => _model.CoreInfo.UpdateGetActivityAsync(false, ActivityUpdateApiFlag.GetActivity));
                    IsLoadingCommentList = false;
                    IsOpenedCommentList = true;
                }
                finally { _loadingSemaph.Release(); }
            else
                IsOpenedCommentList = false;
        }
        async void SendCommentCommand_Executed()
        {
            if (ViewModelUtility.IsDesginMode
                || string.IsNullOrEmpty(PostCommentText) || ShareBoxStatus != CommentPostBoxState.Writing)
                return;
            try
            {
                ShareBoxStatus = CommentPostBoxState.Sending;
                await Task.Run(() => _model.CommentPost(PostCommentText));
            }
            finally
            {
                PostCommentText = null;
                ShareBoxStatus = CommentPostBoxState.Deactive;
            }
        }
        void CancelCommentCommand_Executed()
        {
            if (ViewModelUtility.IsDesginMode)
                return;
            PostCommentText = null;
            ShareBoxStatus = CommentPostBoxState.Deactive;
        }
    }
    public enum CommentPostBoxState
    { Deactive, Writing, Sending }
}
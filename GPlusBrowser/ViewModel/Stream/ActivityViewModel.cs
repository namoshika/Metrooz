using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using SunokoLibrary.Web.GooglePlus;
using SunokoLibrary.Web.GooglePlus.Primitive;

namespace GPlusBrowser.ViewModel
{
    using GPlusBrowser.Model;

    public class ActivityViewModel : ViewModelBase
    {
        public ActivityViewModel(Activity activity)
        {
            _model = activity;
            Comments = new ObservableCollection<CommentViewModel>();
            SendCommentCommand = new RelayCommand(SendCommentCommand_Executed);
            CancelCommentCommand = new RelayCommand(CancelCommentCommand_Executed);

            var tsk = Refresh();
            PropertyChanged += ActivityViewModel_PropertyChanged;
        }
        CommentPostBoxState _shareBoxStatus;
        Activity _model;
        BitmapImage _actorIcon;
        Uri _activityUrl;
        bool _isEnableCommentsHeader, _isCheckedCommentsHeader;
        bool _isOpenedCommentList, _isLoadingCommentList;
        string _postUserName;
        string _postDate;
        string _postText;
        string _postCommentText;
        AttachedContentViewModel _attachedContent;
        ObservableCollection<CommentViewModel> _comments;
        StyleElement _postContentInline;
        readonly System.Threading.SemaphoreSlim _activitySyncer = new System.Threading.SemaphoreSlim(1, 1);
        readonly System.Threading.SemaphoreSlim _loadingSyncer = new System.Threading.SemaphoreSlim(1, 1);

        public BitmapImage ActorIcon
        {
            get { return _actorIcon; }
            set { Set(() => ActorIcon, ref _actorIcon, value); }
        }
        public Uri ActivityUrl
        {
            get { return _activityUrl; }
            set { Set(() => ActivityUrl, ref _activityUrl, value); }
        }
        public bool IsEnableCommentsHeader
        {
            get { return _isEnableCommentsHeader; }
            set { Set(() => IsEnableCommentsHeader, ref _isEnableCommentsHeader, value); }
        }
        public bool IsCheckedCommentsHeader
        {
            get { return _isCheckedCommentsHeader; }
            set { Set(() => IsCheckedCommentsHeader, ref _isCheckedCommentsHeader, value); }
        }
        public bool IsOpenedCommentList
        {
            get { return _isOpenedCommentList; }
            set { Set(() => IsOpenedCommentList, ref _isOpenedCommentList, value); }
        }
        public bool IsLoadingCommentList
        {
            get { return _isLoadingCommentList; }
            set { Set(() => IsLoadingCommentList, ref _isLoadingCommentList, value); }
        }
        public string PostUserName
        {
            get { return _postUserName; }
            set { Set(() => PostUserName, ref _postUserName, value); }
        }
        public string PostDate
        {
            get { return _postDate; }
            set { Set(() => PostDate, ref _postDate, value); }
        }
        public string PostText
        {
            get { return _postText; }
            set { Set(() => PostText, ref _postText, value); }
        }
        public string PostCommentText
        {
            get { return _postCommentText; }
            set { Set(() => PostCommentText, ref _postCommentText, value); }
        }
        public AttachedContentViewModel AttachedContent
        {
            get { return _attachedContent; }
            set { Set(() => AttachedContent, ref _attachedContent, value); }
        }
        public ObservableCollection<CommentViewModel> Comments
        {
            get { return _comments; }
            set { Set(() => Comments, ref _comments, value); }
        }
        public StyleElement PostContentInline
        {
            get { return _postContentInline; }
            set { Set(() => PostContentInline, ref _postContentInline, value); }
        }
        public CommentPostBoxState ShareBoxStatus
        {
            get { return _shareBoxStatus; }
            set { Set(() => ShareBoxStatus, ref _shareBoxStatus, value); }
        }
        public ICommand SendCommentCommand { get; private set; }
        public ICommand CancelCommentCommand { get; private set; }
        public async Task Refresh()
        {
            _model.Updated -= _activity_Refreshed;
            if (_model.CoreInfo.PostStatus != PostStatusType.Removed)
            {
                StyleElement content;
                var postDate = TimeZone.CurrentTimeZone.ToLocalTime(_model.CoreInfo.PostDate);
                PostUserName = _model.CoreInfo.PostUser.Name;
                PostDate = postDate >= DateTime.Today ? postDate.ToString("HH:mm") : postDate.ToString("yyyy/MM/dd");
                content = _model.CoreInfo.ParsedText;
                ActivityUrl = _model.CoreInfo.PostUrl;
                PostText = _model.CoreInfo.Text;
                PostContentInline = content;

                await Task.Run(async () =>
                {
                    try
                    {
                        await _activitySyncer.WaitAsync();
                        _model.Comments.CollectionChanged -= _activity_Comments_CollectionChanged;
                        var commes = new ObservableCollection<CommentViewModel>(
                            _model.Comments.Select(item => new CommentViewModel(item)));
                        _model.Comments.CollectionChanged += _activity_Comments_CollectionChanged;
                        Comments = commes;
                        IsEnableCommentsHeader = _comments.Count > 2;

                        if (_model.CoreInfo.AttachedContent != null)
                            AttachedContent = await AttachedContentViewModel.Create(_model.CoreInfo.AttachedContent).ConfigureAwait(false);
                        ActorIcon = await DataCacheDictionary
                            .DownloadImage(new Uri(_model.CoreInfo.PostUser.IconImageUrl.Replace("$SIZE_SEGMENT", "s40-c-k").Replace("$SIZE_NUM", "80")))
                            .ConfigureAwait(false);
                    }
                    finally { _activitySyncer.Release(); }
                });
                _model.Updated += _activity_Refreshed;
            }
        }
        public async override void Cleanup()
        {
            try
            {
                await _activitySyncer.WaitAsync();
                base.Cleanup();
                _model.Updated -= _activity_Refreshed;
                _model.Comments.CollectionChanged -= _activity_Comments_CollectionChanged;
                foreach (var item in _comments)
                    item.Cleanup();
            }
            finally { _activitySyncer.Release(); }
        }

        async void _activity_Comments_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            try
            {
                await _activitySyncer.WaitAsync();
                switch (e.Action)
                {
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                        foreach (var item in e.NewItems)
                            await _comments.AddOnDispatcher(new CommentViewModel((Comment)item)).ConfigureAwait(false);
                        break;
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                        for (var i = 0; i < e.OldItems.Count; i++)
                        {
                            var tmp = _comments.First(vm => vm.Id == ((Comment)e.OldItems[i]).CommentInfo.Id);
                            await _comments.RemoveOnDispatcher(tmp).ConfigureAwait(false);
                        }
                        break;
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                        await _comments.ClearOnDispatcher().ConfigureAwait(false);
                        break;
                }
                IsEnableCommentsHeader = _comments.Count > 2;
            }
            finally { _activitySyncer.Release(); }
        }
        void _activity_Refreshed(object sender, EventArgs e) { var tsk = Refresh().ConfigureAwait(false); }
        void ActivityViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch(e.PropertyName)
            {
                case "IsCheckedCommentsHeader":
                    Task.Run(async () =>
                        {
                            if (IsCheckedCommentsHeader)
                                try
                                {
                                    await _loadingSyncer.WaitAsync();
                                    IsLoadingCommentList = true;
                                    await _model.CoreInfo.UpdateGetActivityAsync(false, ActivityUpdateApiFlag.GetActivity);
                                    await Refresh();
                                    IsLoadingCommentList = false;
                                    IsOpenedCommentList = true;
                                }
                                finally { _loadingSyncer.Release(); }
                            else
                            {
                                IsOpenedCommentList = false;
                            }
                        });
                    break;
            }
        }
        void SendCommentCommand_Executed()
        {
            if (string.IsNullOrEmpty(PostCommentText) || ShareBoxStatus != CommentPostBoxState.Writing)
                return;
            Task.Run(async () =>
                {
                    try
                    {
                        ShareBoxStatus = CommentPostBoxState.Sending;
                        await _model.CommentPost(PostCommentText).ConfigureAwait(false);
                    }
                    finally
                    {
                        PostCommentText = null;
                        ShareBoxStatus = CommentPostBoxState.Deactive;
                    }
                });
        }
        void CancelCommentCommand_Executed()
        {
            PostCommentText = null;
            ShareBoxStatus = CommentPostBoxState.Deactive;
        }
    }
    public enum CommentPostBoxState
    { Deactive, Writing, Sending }
}
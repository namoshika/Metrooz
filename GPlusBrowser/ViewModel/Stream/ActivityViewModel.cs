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
            var tsk = Refresh();
            SendCommentCommand = new RelayCommand(SendCommentCommand_Executed);
            CancelCommentCommand = new RelayCommand(CancelCommentCommand_Executed);
        }
        CommentPostBoxState _shareBoxStatus;
        Activity _model;
        BitmapImage _actorIcon;
        Uri _activityUrl;
        string _postUserName;
        string _postDate;
        string _postText;
        string _postCommentText;
        AttachedContentViewModel _attachedContent;
        ObservableCollection<CommentViewModel> _comments;
        StyleElement _postContentInline;
        readonly System.Threading.SemaphoreSlim _syncer = new System.Threading.SemaphoreSlim(1, 1);

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
            _model.Comments.CollectionChanged -= _activity_Comments_CollectionChanged;
            _model.Updated -= _activity_Refreshed;
            if (_model.CoreInfo.PostStatus != PostStatusType.Removed)
            {
                try
                {
                    await _syncer.WaitAsync();
                    if (_comments == null)
                    {
                        Comments = new ObservableCollection<CommentViewModel>();
                        foreach (var item in _model.Comments)
                            _comments.Add(new CommentViewModel(item));
                        _model.Comments.CollectionChanged += _activity_Comments_CollectionChanged;
                    }
                }
                finally { _syncer.Release(); }

                StyleElement content;
                var postDate = TimeZone.CurrentTimeZone.ToLocalTime(_model.CoreInfo.PostDate);
                PostUserName = _model.CoreInfo.PostUser.Name;
                PostDate = postDate >= DateTime.Today ? postDate.ToString("HH:mm") : postDate.ToString("yyyy/MM/dd");
                content = _model.CoreInfo.ParsedText;
                ActivityUrl = _model.CoreInfo.PostUrl;
                PostText = _model.CoreInfo.Text;
                PostContentInline = content;

                if (_model.CoreInfo.AttachedContent != null)
                    AttachedContent = await AttachedContentViewModel.Create(_model.CoreInfo.AttachedContent).ConfigureAwait(false);
                ActorIcon = await DataCacheDictionary
                    .DownloadImage(new Uri(_model.CoreInfo.PostUser.IconImageUrl.Replace("$SIZE_SEGMENT", "s40-c-k").Replace("$SIZE_NUM", "80")))
                    .ConfigureAwait(false);

                _model.Updated += _activity_Refreshed;
            }
        }
        public async override void Cleanup()
        {
            try
            {
                await _syncer.WaitAsync();
                base.Cleanup();
                _model.Updated -= _activity_Refreshed;
                _model.Comments.CollectionChanged -= _activity_Comments_CollectionChanged;
                foreach (var item in _comments)
                    item.Cleanup();
            }
            finally { _syncer.Release(); }
        }

        void _activity_Refreshed(object sender, EventArgs e) { var tsk = Refresh(); }
        async void _activity_Comments_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            try
            {
                await _syncer.WaitAsync();
                switch (e.Action)
                {
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                        foreach (var item in e.NewItems)
                            _comments.AddOnDispatcher(new CommentViewModel((Comment)item));
                        break;
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                        for (var i = 0; i < e.OldItems.Count; i++)
                        {
                            var tmp = _comments.First(vm => vm.Id == ((Comment)e.OldItems[i]).CommentInfo.Id);
                            _comments.RemoveOnDispatcher(tmp);
                        }
                        break;
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                        _comments.ClearOnDispatcher();
                        break;
                }
            }
            finally { _syncer.Release(); }
        }
        async void SendCommentCommand_Executed()
        {
            if (string.IsNullOrEmpty(PostCommentText) || ShareBoxStatus != CommentPostBoxState.Writing)
                return;
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
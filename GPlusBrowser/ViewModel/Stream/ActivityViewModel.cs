using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
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
            _comments = new ObservableCollection<CommentViewModel>();
            Refresh();
            lock (_model.Comments)
                foreach (var item in _model.Comments.Select(
                    comment => (CommentViewModel)new CommentViewModel(comment)))
                    Comments.Add(item);

            _model.Comments.CollectionChanged += _activity_Comments_CollectionChanged;
            _model.Updated += _activity_Refreshed;
            SendCommentCommand = new RelayCommand(SendCommentCommand_Executed);
            CancelCommentCommand = new RelayCommand(CancelCommentCommand_Executed);
        }
        CommentPostBoxState _shareBoxStatus;
        Activity _model;
        ImageSource _iconUrl;
        Uri _activityUrl;
        string _postUserName;
        string _postDate;
        string _postText;
        string _postCommentText;
        object _attachedContent;
        ObservableCollection<CommentViewModel> _comments;
        ContentElement _postContentInline;

        public ImageSource PostUserIconUrl
        {
            get { return _iconUrl; }
            set { Set(() => PostUserIconUrl, ref _iconUrl, value); }
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
        public object AttachedContent
        {
            get { return _attachedContent; }
            set { Set(() => AttachedContent, ref _attachedContent, value); }
        }
        public ObservableCollection<CommentViewModel> Comments
        {
            get { return _comments; }
            set { Set(() => Comments, ref _comments, value); }
        }
        public ContentElement PostContentInline
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
        public async void Refresh()
        {
            if (_model.CoreInfo.PostStatus != PostStatusType.Removed)
            {
                StyleElement content;
                var postDate = TimeZone.CurrentTimeZone.ToLocalTime(_model.CoreInfo.PostDate);
                PostUserName = _model.CoreInfo.PostUser.Name;
                PostDate = postDate >= DateTime.Today ? postDate.ToString("HH:mm") : postDate.ToString("yyyy/MM/dd");
                content = _model.CoreInfo.GetParsedContent();
                ActivityUrl = _model.CoreInfo.PostUrl;
                PostText = _model.CoreInfo.Text;
                PostContentInline = content;

                if (_model.CoreInfo.AttachedContent != null)
                    switch (_model.CoreInfo.AttachedContent.Type)
                    {
                        case ContentType.Album:
                            var attachedAlbum = (AttachedAlbum)_model.CoreInfo.AttachedContent;
                            AttachedContent = await AttachedAlbumViewModel.Create(attachedAlbum);
                            break;
                        case ContentType.Image:
                            var attachedImage = (AttachedImage)_model.CoreInfo.AttachedContent;
                            AttachedContent = await AttachedImageViewModel.Create(attachedImage);
                            break;
                        case ContentType.Link:
                            var attachedLink = (AttachedLink)_model.CoreInfo.AttachedContent;
                            AttachedContent = await AttachedLinkViewModel.Create(
                                attachedLink.Title,
                                string.IsNullOrEmpty(attachedLink.Summary) ? null : attachedLink.Summary.Trim('\n', '\r', ' '),
                                attachedLink.LinkUrl, attachedLink.FaviconUrl, attachedLink.OriginalThumbnailUrl);
                            break;
                    }
                PostUserIconUrl = await DataCacheDictionary.DownloadImage(
                    new Uri(_model.CoreInfo.PostUser.IconImageUrl.Replace("$SIZE_SEGMENT", "s40-c-k").Replace("$SIZE_NUM", "80")));
            }
        }
        public override void Cleanup()
        {
            base.Cleanup();
            _model.Updated -= _activity_Refreshed;
            _model.Comments.CollectionChanged -= _activity_Comments_CollectionChanged;
            foreach (var item in _comments)
                item.Cleanup();
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
        void _activity_Refreshed(object sender, EventArgs e) { Refresh(); }
        void _activity_Comments_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    foreach (var item in e.NewItems)
                        Comments.AddOnDispatcher(new CommentViewModel((Comment)item));
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    for (var i = 0; i < e.OldItems.Count; i++)
                    {
                        var tmp = Comments.First(vm => vm.Id == ((Comment)e.OldItems[i]).CommentInfo.Id);
                        Comments.RemoveOnDispatcher(tmp);
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    Comments.ClearOnDispatcher();
                    break;
            }
        }
    }
    public enum CommentPostBoxState
    { Deactive, Writing, Sending }
}
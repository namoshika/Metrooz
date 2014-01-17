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

    public class ActivityViewModel : ViewModelBase, IDisposable
    {
        public ActivityViewModel(Activity activity, AccountViewModel topLevel, Dispatcher uiThreadDispatcher)
            : base(uiThreadDispatcher, topLevel)
        {
            _model = activity;
            _comments = new ObservableCollection<CommentViewModel>();
            _activity_Refreshed(null, null);
            _comments.Clear();
            lock (_model.Comments)
                foreach (var item in _model.Comments.Select(
                    comment => (CommentViewModel)new CommentViewModel(comment, topLevel, uiThreadDispatcher)))
                    Comments.Add(item);

            _model.Comments.CollectionChanged += _activity_Comments_CollectionChanged;
            _model.Updated += _activity_Refreshed;
            PostCommentCommand = new RelayCommand(PostCommentCommand_Executed);
        }
        CommentPostBoxState _shareBoxStatus;
        Activity _model;
        ImageSource _iconUrl;
        Uri _activityUrl;
        int _isDisposed;
        string _postUserName;
        string _postContent;
        string _postDate;
        string _postCommentText;
        object _attachedContent;
        ObservableCollection<CommentViewModel> _comments;
        System.Windows.Documents.Inline _postContentInline;

        public ImageSource PostUserIconUrl
        {
            get { return _iconUrl; }
            set
            {
                _iconUrl = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("PostUserIconUrl"));
            }
        }
        public Uri ActivityUrl
        {
            get { return _activityUrl; }
            set
            {
                _activityUrl = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("ActivityUrl"));
            }
        }
        public string PostUserName
        {
            get { return _postUserName; }
            set
            {
                _postUserName = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("PostUserName"));
            }
        }
        public string PostContent
        {
            get { return _postContent; }
            set
            {
                _postContent = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("PostContent"));
            }
        }
        public string PostDate
        {
            get { return _postDate; }
            set
            {
                _postDate = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("PostDate"));
            }
        }
        public string PostCommentText
        {
            get { return _postCommentText; }
            set
            {
                _postCommentText = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("PostCommentText"));
            }
        }
        public object AttachedContent
        {
            get { return _attachedContent; }
            set
            {
                _attachedContent = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("AttachedContent"));
            }
        }
        public ObservableCollection<CommentViewModel> Comments
        {
            get { return _comments; }
            set
            {
                _comments = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("Comments"));
            }
        }
        public System.Windows.Documents.Inline PostContentInline
        {
            get { return _postContentInline; }
            set
            {
                _postContentInline = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("PostContentInline"));
            }
        }
        public CommentPostBoxState ShareBoxStatus
        {
            get { return _shareBoxStatus; }
            set
            {
                _shareBoxStatus = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("ShareBoxStatus"));
            }
        }
        public ICommand PostCommentCommand { get; private set; }
        public void Dispose()
        {
            if (System.Threading.Interlocked.CompareExchange(ref _isDisposed, 1, 0) == 1)
                return;

            _model.Updated -= _activity_Refreshed;
            _model.Comments.CollectionChanged -= _activity_Comments_CollectionChanged;
            foreach (var item in _comments)
                item.Dispose();
            _comments.ClearAsync(UiThreadDispatcher);
            _model = null;
            _iconUrl = null;
            _activityUrl = null;
            _postUserName = null;
            _postContent = null;
            _postDate = null;
            _postCommentText = null;
            _attachedContent = null;
            _comments = null;
            _postContentInline = null;
        }

        async void PostCommentCommand_Executed(object arg)
        {
            if (string.IsNullOrEmpty(PostCommentText) || ShareBoxStatus != CommentPostBoxState.Writing)
                return;
            try
            {
                ShareBoxStatus = CommentPostBoxState.Sending;
                await _model.CommentPost(PostCommentText).ConfigureAwait(false);
                ShareBoxStatus = CommentPostBoxState.Deactive;
            }
            finally
            { PostCommentText = null; }
        }
        async void _activity_Refreshed(object sender, EventArgs e)
        {
            if (_model.CoreInfo.PostStatus != PostStatusType.Removed)
            {
                StyleElement content;
                var postDate = TimeZone.CurrentTimeZone.ToLocalTime(_model.CoreInfo.PostDate);
                PostUserName = _model.CoreInfo.PostUser.Name;
                PostDate = postDate >= DateTime.Today ? postDate.ToString("HH:mm") : postDate.ToString("yyyy/MM/dd");
                content = _model.CoreInfo.GetParsedContent();
                ActivityUrl = _model.CoreInfo.PostUrl;

                if (_model.CoreInfo.AttachedContent != null)
                    if (_model.CoreInfo.AttachedContent.Type == ContentType.Album)
                    {
                        var attachedAlbum = (AttachedAlbum)_model.CoreInfo.AttachedContent;
                        AttachedContent = new AttachedAlbumViewModel(attachedAlbum, TopLevel, UiThreadDispatcher);
                    }
                    else if (_model.CoreInfo.AttachedContent.Type == ContentType.Link)
                    {
                        var attachedLink = (AttachedLink)_model.CoreInfo.AttachedContent;
                        AttachedContent = new AttachedLinkViewModel(
                            attachedLink.Title,
                            string.IsNullOrEmpty(attachedLink.Summary)
                                ? null : attachedLink.Summary.Trim('\n', '\r', ' '),
                            attachedLink.FaviconUrl, attachedLink.LinkUrl, attachedLink.OriginalThumbnailUrl,
                            TopLevel, UiThreadDispatcher);
                    }
                UiThreadDispatcher.Invoke(() => PostContentInline = PrivateConvertInlines(content));
                PostUserIconUrl = await TopLevel.DataCacheDict.DownloadImage(
                    new Uri(_model.CoreInfo.PostUser.IconImageUrl
                        .Replace("$SIZE_SEGMENT", "s25-c-k")
                        .Replace("$SIZE_NUM", "80")));
            }
        }
        void _activity_Comments_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    foreach (var item in e.NewItems)
                        Comments.AddAsync(new CommentViewModel((Comment)item, TopLevel, UiThreadDispatcher), UiThreadDispatcher);
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    for (var i = 0; i < e.OldItems.Count; i++)
                    {
                        var tmp = Comments.First(vm => vm.Id == ((Comment)e.OldItems[i]).CommentInfo.Id);
                        Comments.RemoveAsync(tmp, UiThreadDispatcher);
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    Comments.ClearAsync(UiThreadDispatcher);
                    break;
            }
        }

        public static System.Windows.Documents.Inline PrivateConvertInlines(ContentElement tree)
        {
            if (tree == null)
                return null;
            System.Windows.Documents.Inline inline = null;
            switch (tree.Type)
            {
                case ElementType.Style:
                    var styleEle = ((StyleElement)tree);
                    switch (styleEle.Style)
                    {
                        case StyleType.Bold:
                            inline = new System.Windows.Documents.Bold();
                            ((System.Windows.Documents.Bold)inline).Inlines.AddRange(
                                ((StyleElement)tree).Children.Select(ele => PrivateConvertInlines(ele)));
                            break;
                        case StyleType.Italic:
                            inline = new System.Windows.Documents.Italic();
                            ((System.Windows.Documents.Italic)inline).Inlines.AddRange(
                                ((StyleElement)tree).Children.Select(ele => PrivateConvertInlines(ele)));
                            break;
                        case StyleType.Middle:
                            inline = new System.Windows.Documents.Span();
                            inline.TextDecorations.Add(System.Windows.TextDecorations.Strikethrough);
                            ((System.Windows.Documents.Span)inline).Inlines.AddRange(
                                ((StyleElement)tree).Children.Select(ele => PrivateConvertInlines(ele)));
                            break;
                        default:
                            inline = new System.Windows.Documents.Span();
                            ((System.Windows.Documents.Span)inline).Inlines.AddRange(
                                ((StyleElement)tree).Children.Select(ele => PrivateConvertInlines(ele)));
                            break;
                    }
                    break;
                case ElementType.Hyperlink:
                    var hyperEle = (HyperlinkElement)tree;
                    var target = hyperEle.Target;
                    inline = new System.Windows.Documents.Hyperlink(
                        new System.Windows.Documents.Run(hyperEle.Text));
                    ((System.Windows.Documents.Hyperlink)inline).Click += (sender, e) =>
                        { System.Diagnostics.Process.Start(target.AbsoluteUri); };
                    break;
                case ElementType.Mension:
                    var spanInline = new System.Windows.Documents.Span();
                    spanInline.Inlines.AddRange(
                        new System.Windows.Documents.Inline[]
                        {
                            new System.Windows.Documents.Run("+"),
                            new System.Windows.Documents.Hyperlink(
                                new System.Windows.Documents.Run(((MensionElement)tree).Text.Substring(1)))
                                { TextDecorations = null }
                        });
                    inline = spanInline;
                    break;
                case ElementType.Text:
                    inline = new System.Windows.Documents.Run(((TextElement)tree).Text);
                    break;
                case ElementType.Break:
                    inline = new System.Windows.Documents.LineBreak();
                    break;
                default:
                    throw new Exception();
            }
            return inline;
        }
    }
    public enum CommentPostBoxState
    { Deactive, Writing, Sending }
}
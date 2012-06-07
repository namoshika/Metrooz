using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using SunokoLibrary.GooglePlus;

namespace GPlusBrowser.ViewModel
{
    using GPlusBrowser.Model;

    public class ActivityViewModel : ViewModelBase, IDisposable
    {
        public ActivityViewModel(Activity activity, Dispatcher uiThreadDispatcher)
            : base(uiThreadDispatcher)
        {
            _isWritableA = true;
            _activity = activity;
            _comments = new ObservableCollection<CommentViewModel>();
            _activity_Refreshed(null, null);
            _comments.Clear();
            lock (_activity.Comments)
                foreach (var item in _activity.Comments.Select(
                    comment => (CommentViewModel)new CommentViewModel(comment, uiThreadDispatcher)))
                    Comments.Add(item);

            _activity.Comments.CollectionChanged += Comments_CollectionChanged;
            _activity.Updated += _activity_Refreshed;
            PostCommentCommand = new RelayCommand(PostCommentCommand_Executed);
        }
        Activity _activity;
        Uri _iconUrl;
        Uri _activityUrl;
        int _isDisposed;
        bool _isExpandComment, _isWritableA, _isWriteModeA;
        string _postUserName;
        string _postContent;
        string _postDate;
        string _postCommentText;
        object _attachedContent;
        ObservableCollection<CommentViewModel> _comments;
        System.Windows.Documents.Inline _postContentInline;

        public Uri PostUserIconUrl
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
        public bool IsExpandComment
        {
            get { return _isExpandComment; }
            set
            {
                _isExpandComment = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("IsExpandComment"));
            }
        }
        public bool IsWritableA
        {
            get { return _isWritableA; }
            set
            {
                _isWritableA = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("IsWritableA"));
            }
        }
        public bool IsWriteModeA
        {
            get { return _isWriteModeA; }
            set
            {
                _isWriteModeA = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("IsWriteModeA"));
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
        public string PostCommentTextA
        {
            get { return _postCommentText; }
            set
            {
                _postCommentText = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("PostCommentTextA"));
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
        public ICommand PostCommentCommand { get; private set; }
        public void Dispose()
        {
            if (System.Threading.Interlocked.CompareExchange(ref _isDisposed, 1, 0) == 1)
                return;

            _activity.Updated -= _activity_Refreshed;
            _activity.Comments.CollectionChanged -= Comments_CollectionChanged;
            foreach (var item in _comments)
                item.Dispose();
            _comments.ClearAsync(UiThreadDispatcher);
            _activity = null;
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

        void _activity_Refreshed(object sender, EventArgs e)
        {
            if (_activity.ActivityInfo.PostStatus != PostStatusType.Removed)
            {
                StyleElement content;
                using (_activity.ActivityInfo.GetParseLocker())
                {
                    PostUserName = _activity.ActivityInfo.PostUser.Name;
                    PostUserIconUrl = new Uri(_activity.ActivityInfo.PostUser.IconImageUrlText.Replace("$SIZE_SEGMENT", "s25-c-k"));
                    PostDate = _activity.ActivityInfo.PostDate >= DateTime.Today
                        ? _activity.ActivityInfo.PostDate.ToString("HH:mm")
                        : _activity.ActivityInfo.PostDate.ToString("yyyy/MM/dd");
                    content = _activity.ActivityInfo.ParsedContent;
                    ActivityUrl = _activity.ActivityInfo.PostUrl;

                    switch (_activity.ActivityInfo.AttachedContentType)
                    {
                        case ContentType.Link:
                            var attachedLink = (AttachedLink)_activity.ActivityInfo.AttachedContent;
                            AttachedContent = new AttachedLinkViewModel(
                                attachedLink.AncourTitle,
                                string.IsNullOrEmpty(attachedLink.AncourBeginningText)
                                    ? null : attachedLink.AncourBeginningText.Trim('\n', '\r', ' '),
                                attachedLink.AncourFavicon, attachedLink.AncourUrl, attachedLink.Thumbnail,
                                UiThreadDispatcher);
                            break;
                    }
                }
                UiThreadDispatcher.InvokeAsync(() => PostContentInline = PrivateConvertInlines(content));
            }
        }
        async void PostCommentCommand_Executed(object arg)
        {
            if (string.IsNullOrEmpty(PostCommentTextA) || IsWritableA == false)
                return;
            IsWritableA = false;
            try
            { await _activity.CommentPost(PostCommentTextA).ConfigureAwait(false); }
            finally
            {
                PostCommentTextA = null;
                IsWritableA = true;
                IsWriteModeA = false;
            }
        }
        void Comments_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    foreach (var item in e.NewItems)
                        Comments.AddAsync(new CommentViewModel((Comment)item, UiThreadDispatcher), UiThreadDispatcher);
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
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            _activity = activity;
            _comments = new DispatchObservableCollection<CommentViewModel>(uiThreadDispatcher);
            _activity_Refreshed(null, null);
            _comments.Clear();
            foreach (var item in _activity.Comments.ToArray().Select(
                comment => (CommentViewModel)new CommentViewModel(comment, uiThreadDispatcher)))
                Comments.Add(item);

            _activity.Comments.CollectionChanged += Comments_CollectionChanged;
            _activity.Updated += _activity_Refreshed;
        }
        Activity _activity;
        Uri _iconUrl;
        Uri _activityUrl;
        bool _isExpandComment;
        string _postUserName;
        string _postContent;
        string _postDate;
        DispatchObservableCollection<CommentViewModel> _comments;
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
        public DispatchObservableCollection<CommentViewModel> Comments
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
        public void Dispose()
        {
            _activity.Updated -= _activity_Refreshed;
        }

        async void _activity_Refreshed(object sender, EventArgs e)
        {
            using (_activity.ActivityInfo.GetParseLocker())
            {
                if (_activity.ActivityInfo.PostStatus != PostStatusType.Removed)
                {
                    PostUserName = _activity.ActivityInfo.PostUser.Name;
                    PostUserIconUrl = new Uri(_activity.ActivityInfo.PostUser.IconImageUrlText.Replace("$SIZE_SEGMENT", "s25-c-k"));
                    PostDate = _activity.ActivityInfo.PostDate >= DateTime.Today
                        ? _activity.ActivityInfo.PostDate.ToString("HH:mm")
                        : _activity.ActivityInfo.PostDate.ToString("yyyy/MM/dd");
                    PostContentInline = await ConvertInlines(_activity.ActivityInfo.ParsedContent);
                    ActivityUrl = _activity.ActivityInfo.PostUrl;
                }
            }
        }
        void Comments_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (App.Current != null)
                switch (e.Action)
                {
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                        foreach (var item in e.NewItems)
                            Comments.Add(new CommentViewModel((Comment)item, UiThreadDispatcher));
                        break;
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                        for (var i = 0; i < e.OldItems.Count; i++)
                        {
                            var tmp = Comments.First(vm => vm.Id == ((Comment)e.OldItems[i]).CommentInfo.Id);
                            Comments.Remove(tmp);
                        }
                        break;
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                        Comments.Clear();
                        break;
                }
        }

        public static Task<System.Windows.Documents.Inline> ConvertInlines(ContentElement tree)
        { return App.Current.Dispatcher.InvokeAsync(() => PrivateConvertInlines(tree)); }
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
                    inline = new System.Windows.Documents.Hyperlink(
                        new System.Windows.Documents.Run(((HyperlinkElement)tree).Text));
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

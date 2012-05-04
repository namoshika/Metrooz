using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Threading;

namespace GPlusBrowser.ViewModel
{
    using Model;

    public class CommentViewModel : ViewModelBase
    {
        public CommentViewModel(Comment model, Dispatcher uiThreadDispatcher)
            : base(uiThreadDispatcher)
        {
            _model = model;
            Id = model.CommentInfo.Id;
            model_Refreshed(null, null);
            model.Refreshed += model_Refreshed;
        }
        Comment _model;
        Uri _ownerIconUrl;
        string _id;
        string _ownerName;
        string _commentContent;
        string _commentDate;
        System.Windows.Documents.Inline _postContentInline;

        public string Id
        {
            get { return _id; }
            set
            {
                _id = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("Id"));
            }
        }
        public string CommentContent
        {
            get { return _commentContent; }
            set
            {
                _commentContent = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("CommentContent"));
            }
        }
        public string OwnerName
        {
            get { return _ownerName; }
            set
            {
                _ownerName = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("OwnerName"));
            }
        }
        public string CommentDate
        {
            get { return _commentDate; }
            set
            {
                _commentDate = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("CommentDate"));
            }
        }
        public Uri OwnerIconUrl
        {
            get { return _ownerIconUrl; }
            set
            {
                _ownerIconUrl = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("OwnerIconUrl"));
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

        void model_Refreshed(object sender, EventArgs e)
        {
            CommentDate = _model.CommentInfo.CommentDate >= DateTime.Today
                ? _model.CommentInfo.CommentDate.ToString("HH:mm")
                : _model.CommentInfo.CommentDate.ToString("yyyy/MM/dd");
            OwnerName = _model.CommentInfo.Owner.Name;
            OwnerIconUrl = new Uri(_model.CommentInfo.Owner.IconImageUrlText
                .Replace("$SIZE_SEGMENT", "s25-c-k"));

            UiThreadDispatcher.InvokeAsync(
                () => PostContentInline = ActivityViewModel
                    .PrivateConvertInlines(_model.CommentInfo.ParsedContent));
        }
    }
}

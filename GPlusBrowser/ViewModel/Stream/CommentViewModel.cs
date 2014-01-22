using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Threading;
using SunokoLibrary.Web.GooglePlus;

namespace GPlusBrowser.ViewModel
{
    using Model;

    public class CommentViewModel : ViewModelBase
    {
        public CommentViewModel(Comment model)
        {
            _model = model;
            Id = model.CommentInfo.Id;
            model_Refreshed(null, null);
            model.Refreshed += model_Refreshed;
        }
        Comment _model;
        ImageSource _ownerIconUrl;
        string _id;
        string _ownerName;
        string _commentContent;
        string _commentDate;
        ContentElement _postContentInline;

        public string Id
        {
            get { return _id; }
            set { Set(() => Id, ref _id, value); }
        }
        public string CommentContent
        {
            get { return _commentContent; }
            set { Set(() => CommentContent, ref _commentContent, value); }
        }
        public string OwnerName
        {
            get { return _ownerName; }
            set { Set(() => OwnerName, ref _ownerName, value); }
        }
        public string CommentDate
        {
            get { return _commentDate; }
            set { Set(() => CommentDate, ref _commentDate, value); }
        }
        public ImageSource OwnerIconUrl
        {
            get { return _ownerIconUrl; }
            set { Set(() => OwnerIconUrl, ref _ownerIconUrl, value); }
        }
        public ContentElement PostContentInline
        {
            get { return _postContentInline; }
            set { Set(() => PostContentInline, ref _postContentInline, value); }
        }
        public override void Cleanup()
        {
            base.Cleanup();

            if (_model != null)
                _model.Refreshed -= model_Refreshed;
            _id = null;
            _ownerName = null;
            _commentContent = null;
            _commentDate = null;
            _postContentInline = null;
        }

        async void model_Refreshed(object sender, EventArgs e)
        {
            var postDate = TimeZone.CurrentTimeZone.ToLocalTime(_model.CommentInfo.PostDate);
            CommentDate = postDate >= DateTime.Today ? postDate.ToString("HH:mm") : postDate.ToString("yyyy/MM/dd");
            OwnerName = _model.CommentInfo.Owner.Name;
            OwnerIconUrl = await DataCacheDictionary.DownloadImage(new Uri(_model.CommentInfo.Owner.IconImageUrl
                .Replace("$SIZE_SEGMENT", "s25-c-k").Replace("$SIZE_NUM", "80")));
            PostContentInline = _model.CommentInfo.GetParsedContent();
        }
    }
}
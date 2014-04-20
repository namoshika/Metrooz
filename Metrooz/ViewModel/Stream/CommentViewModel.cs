using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Threading;
using SunokoLibrary.Web.GooglePlus;

namespace Metrooz.ViewModel
{
    using Model;

    public class CommentViewModel : ViewModelBase
    {
        public CommentViewModel(Comment model)
        {
            Id = model.CommentInfo.Id;
            _model = model;
            _model.Refreshed += model_Refreshed;
            var tsk = Refresh();
        }
        Comment _model;
        ImageSource _actorIcon;
        string _id;
        string _ownerName;
        string _commentContent;
        string _commentDate;
        StyleElement _postContentInline;

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
        public ImageSource ActorIcon
        {
            get { return _actorIcon; }
            set { Set(() => ActorIcon, ref _actorIcon, value); }
        }
        public StyleElement PostContentInline
        {
            get { return _postContentInline; }
            set { Set(() => PostContentInline, ref _postContentInline, value); }
        }
        public async Task Refresh()
        {
            var postDate = TimeZone.CurrentTimeZone.ToLocalTime(_model.CommentInfo.PostDate);
            CommentDate = postDate >= DateTime.Today ? postDate.ToString("HH:mm") : postDate.ToString("yyyy/MM/dd");
            OwnerName = _model.CommentInfo.Owner.Name;
            ActorIcon = await DataCacheDictionary.DownloadImage(new Uri(_model.CommentInfo.Owner.IconImageUrl
                .Replace("$SIZE_SEGMENT", "s25-c-k").Replace("$SIZE_NUM", "80"))).ConfigureAwait(false);
            PostContentInline = _model.CommentInfo.GetParsedContent();
        }
        public override void Cleanup()
        {
            base.Cleanup();
            _model.Refreshed -= model_Refreshed;
        }
        void model_Refreshed(object sender, EventArgs e)
        { var tsk = Refresh(); }
    }
}
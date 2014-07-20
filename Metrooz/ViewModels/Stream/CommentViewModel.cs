using Livet;
using Livet.Commands;
using SunokoLibrary.Web.GooglePlus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Threading;

namespace Metrooz.ViewModels
{
    using Models;

    public class CommentViewModel : ViewModel
    {
        public CommentViewModel(Comment model, bool isActive)
        {
            _model = model;
            _model.Refreshed += model_Refreshed;
            var tsk = Refresh(isActive);
        }
        public CommentViewModel()
        {
            OwnerName = "OwnerName";
            CommentDate = "00:00";
            ActorIcon = SampleData.DataLoader.LoadImage("accountIcon00.png").Result;
            PostContentInline = new StyleElement(StyleType.None, new[] { new TextElement("Comment Content Text") });
        }
        Comment _model;
        bool _isActive;
        string _ownerName;
        string _commentDate;
        ImageSource _actorIcon;
        StyleElement _postContentInline;

        public string OwnerName
        {
            get { return _ownerName; }
            set
            {
                _ownerName = value;
                RaisePropertyChanged(() => OwnerName);
            }
        }
        public string CommentDate
        {
            get { return _commentDate; }
            set
            {
                _commentDate = value;
                RaisePropertyChanged(() => CommentDate);
            }
        }
        public ImageSource ActorIcon
        {
            get { return _actorIcon; }
            set
            {
                _actorIcon = value;
                RaisePropertyChanged(() => ActorIcon);
            }
        }
        public StyleElement PostContentInline
        {
            get { return _postContentInline; }
            set
            {
                _postContentInline = value;
                RaisePropertyChanged(() => PostContentInline);
            }
        }
        public async Task Refresh(bool isActive)
        {
            var postDate = TimeZone.CurrentTimeZone.ToLocalTime(_model.CommentInfo.PostDate);
            CommentDate = postDate >= DateTime.Today ? postDate.ToString("HH:mm") : postDate.ToString("yyyy/MM/dd");
            OwnerName = _model.CommentInfo.Owner.Name;
            PostContentInline = _model.CommentInfo.GetParsedContent();
            _isActive = isActive;
            if (isActive)
                ActorIcon = await DataCacheDictionary.DownloadImage(new Uri(_model.CommentInfo.Owner.IconImageUrl
                    .Replace("$SIZE_SEGMENT", "s25-c-k").Replace("$SIZE_NUM", "80"))).ConfigureAwait(false);
            else
                ActorIcon = null;
        }
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _model.Refreshed -= model_Refreshed;
        }
        
        void model_Refreshed(object sender, EventArgs e)
        { var tsk = Refresh(_isActive); }
    }
}
using Livet;
using SunokoLibrary.Web.GooglePlus;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Metrooz.ViewModels
{
    using Metrooz.Models;

    public class NotificationWithProfileViewModel : NotificationViewModel
    {
        public NotificationWithProfileViewModel(NotificationInfoWithActor model, DateTime insertTime)
            : base(insertTime)
        {
            _notificationModel = model;
            model_Updated(this, null);
            model.Updated += model_Updated;
        }
        int _memberCount;
        string _noticeTitle, _noticeText;
        NotificationInfoWithActor _notificationModel;
        ProfileRegisterViewModel[] _members;

        public int MemberCount
        {
            get { return _memberCount; }
            set
            {
                _memberCount = value;
                RaisePropertyChanged(() => MemberCount);
            }
        }
        public string NoticeTitle
        {
            get { return _noticeTitle; }
            set
            {
                _noticeTitle = value;
                RaisePropertyChanged(() => NoticeTitle);
            }
        }
        public string NoticeText
        {
            get { return _noticeText; }
            set
            {
                _noticeText = value;
                RaisePropertyChanged(() => NoticeText);
            }
        }
        public ProfileRegisterViewModel[] Members
        {
            get { return _members; }
            set
            {
                _members = value;
                RaisePropertyChanged(() => Members);
            }
        }

        void model_Updated(object sender, EventArgs e)
        {
            MemberCount = _notificationModel.ActionLogs.Count;
            NoticeTitle = _notificationModel.Title;
            NoticeText = (string.Empty + _notificationModel.Summary).Replace("\r", "").Replace("\n", "");
            Members = _notificationModel.ActionLogs
                .Select(obj => new ProfileRegisterViewModel(obj))
                .ToArray();
        }
    }
    public class ProfileRegisterViewModel : ViewModel
    {
        public ProfileRegisterViewModel(NotificationItemInfo model)
        {
            _name = model.Actor.Name;
            _linkUrl = model.Actor.ProfileUrl;
            DataCacheDictionary.DownloadImage(new Uri(model.Actor.IconImageUrl.Replace("$SIZE_SEGMENT", "s80-c-k")))
                .ContinueWith(tsk => ProfileIconUrl = tsk.Result);
        }
        string _name;
        Uri _linkUrl;
        ImageSource _profileIconUrl;

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                RaisePropertyChanged(() => Name);
            }
        }
        public Uri LinkUrl
        {
            get { return _linkUrl; }
            set
            {
                _linkUrl = value;
                RaisePropertyChanged(() => LinkUrl);
            }
        }
        public ImageSource ProfileIconUrl
        {
            get { return _profileIconUrl; }
            set
            {
                _profileIconUrl = value;
                RaisePropertyChanged(() => ProfileIconUrl);
            }
        }
        public ICommand IgnoreCommand { get; private set; }
    }
}

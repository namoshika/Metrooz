using GalaSoft.MvvmLight;
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
using SunokoLibrary.Web.GooglePlus;
using SunokoLibrary.Web.GooglePlus.Primitive;

namespace GPlusBrowser.ViewModel
{
    using GPlusBrowser.Model;

    public class NotificationWithProfileViewModel : NotificationViewModel
    {
        public NotificationWithProfileViewModel(NotificationInfoWithActor model)
        {
            _notificationModel = model;
            model_Updated(this, null);
            model.Updated += model_Updated;
        }

        int _memberCount;
        NotificationInfoWithActor _notificationModel;
        ProfileRegisterViewModel[] _members;

        public int MemberCount
        {
            get { return _memberCount; }
            set { Set(() => MemberCount, ref _memberCount, value); }
        }
        public ProfileRegisterViewModel[] Members
        {
            get { return _members; }
            set { Set(() => Members, ref _members, value); }
        }

        void model_Updated(object sender, EventArgs e)
        {
            _memberCount = _notificationModel.ActionLogs.Count;
            _members = _notificationModel.ActionLogs
                .Select(obj => new ProfileRegisterViewModel(obj))
                .ToArray();
        }
    }
    public class ProfileRegisterViewModel : ViewModelBase
    {
        public ProfileRegisterViewModel(NotificationItemInfo model)
        {
            _name = model.Actor.Name;
            DataCacheDictionary.DownloadImage(new Uri(model.Actor.IconImageUrl.Replace("$SIZE_SEGMENT", "s80-c-k")))
                .ContinueWith(tsk => _profileIconUrl = tsk.Result);
        }
        string _name;
        ImageSource _profileIconUrl;

        public string Name
        {
            get { return _name; }
            set { Set(() => Name, ref _name, value); }
        }
        public ImageSource ProfileIconUrl
        {
            get { return _profileIconUrl; }
            set { Set(() => ProfileIconUrl, ref _profileIconUrl, value); }
        }
        public ICommand IgnoreCommand { get; private set; }
    }
}

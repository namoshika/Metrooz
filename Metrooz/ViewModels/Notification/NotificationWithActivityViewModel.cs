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

    public class NotificationWithActivityViewModel : NotificationViewModel
    {
        public NotificationWithActivityViewModel(NotificationInfoWithActivity model, Activity activityModel, DateTime insertTime)
            : base(insertTime)
        {
            _targetModel = activityModel;
            _notificationModel = model;
            _notificationModel_Updated(null, null);
            _notificationModel.Updated += _notificationModel_Updated;
        }
        readonly NotificationInfoWithActivity _notificationModel;
        readonly SemaphoreSlim _syncer = new SemaphoreSlim(1);
        Activity _targetModel;
        ActivityViewModel _target;
        string _noticeDate, _noticeTitle, _noticeText;

        public ICommand MuteCommand { get; private set; }
        public string NoticeDate
        {
            get { return _noticeDate; }
            set
            {
                _noticeDate = value;
                RaisePropertyChanged(() => NoticeDate);
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
        public ActivityViewModel Target
        {
            get { return _target; }
            set
            {
                _target = value;
                RaisePropertyChanged();
            }
        }
        public async void Activate()
        {
            try
            {
                await _syncer.WaitAsync();
                if (_targetModel == null || await _targetModel.Activate() == false)
                    return;
                switch (_targetModel.CoreInfo.PostStatus)
                {
                    case PostStatusType.First:
                    case PostStatusType.Edited:
                        //isActive = true。通知は非表示時にはNotificationManagerVMで
                        //それより下層が消えるので常時trueで問題ない。
                        Target = new ActivityViewModel(_targetModel, true);
                        break;
                    case PostStatusType.Removed:
                        _targetModel.Dispose();
                        _target.Dispose();
                        _targetModel = null;
                        Target = null;
                        break;
                }
            }
            finally { _syncer.Release(); }
        }
        protected async override void Dispose(bool disposing)
        {
            try
            {
                await _syncer.WaitAsync();
                base.Dispose(disposing);
                if (_target != null)
                    _target.Dispose();
                _targetModel.Dispose();
            }
            finally { _syncer.Release(); }
        }
        async void _notificationModel_Updated(object sender, EventArgs e)
        {
            NoticeTitle = _notificationModel.Title;
            NoticeText = (string.Empty + _notificationModel.Summary).Replace("\r", "").Replace("\n", "");
            DisplayIconUrl = await DataCacheDictionary.DownloadImage(new Uri(_notificationModel.ActionLogs.Last()
                .Actor.IconImageUrl.Replace("$SIZE_SEGMENT", "s35-c-k"))).ConfigureAwait(false);
            NoticeDate = _notificationModel.NoticedDate.ToString(
                _notificationModel.NoticedDate > DateTime.Today ? "HH:mm" : "yyyy:MM:dd");
        }
    }
}

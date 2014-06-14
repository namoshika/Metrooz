using GalaSoft.MvvmLight;
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

namespace Metrooz.ViewModel
{
    using Metrooz.Model;

    public class NotificationWithActivityViewModel : NotificationViewModel
    {
        public NotificationWithActivityViewModel(NotificationInfoWithActivity model, Activity activityModel, DateTime insertTime)
            : base(insertTime)
        {
            _target = new ActivityViewModel(activityModel);
            _targetModel = activityModel;
            _notificationModel = model;
            _notificationModel_Updated(null, null);
            _notificationModel.Updated += _notificationModel_Updated;
        }
        readonly NotificationInfoWithActivity _notificationModel;
        readonly Activity _targetModel;
        ActivityViewModel _target;
        string _noticeDate, _noticeTitle, _noticeText;

        public ICommand MuteCommand { get; private set; }
        public string NoticeDate
        {
            get { return _noticeDate; }
            set { Set(() => NoticeDate, ref _noticeDate, value); }
        }
        public string NoticeTitle
        {
            get { return _noticeTitle; }
            set { Set(() => NoticeTitle, ref _noticeTitle, value); }
        }
        public string NoticeText
        {
            get { return _noticeText; }
            set { Set(() => NoticeText, ref _noticeText, value); }
        }
        public ActivityViewModel Target
        {
            get { return _target; }
            set { Set(() => Target, ref _target, value); }
        }
        public override void Cleanup()
        {
            base.Cleanup();
            _target.Cleanup();
            _targetModel.Dispose();
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

        public async static Task<NotificationWithActivityViewModel> Create(NotificationInfoWithActivity model, DateTime insertTime)
        {
            await model.Activity.UpdateGetActivityAsync(false, ActivityUpdateApiFlag.GetActivities);
            var activity = model.Activity.PostStatus != PostStatusType.Removed ? new Activity(model.Activity) : null;
            return new NotificationWithActivityViewModel(model, activity, insertTime);
        }
    }
}

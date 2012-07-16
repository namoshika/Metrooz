﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows.Threading;
using SunokoLibrary.GooglePlus;

namespace GPlusBrowser.ViewModel
{
    using GPlusBrowser.Model;

    public class NotificationManagerViewModel : ViewModelBase
    {
        public NotificationManagerViewModel(NotificationManager model, Dispatcher uiThreadDispatcher)
            : base(uiThreadDispatcher)
        {
            _managerModel = model;
            _managerModel.Updated += model_Updated;
            _notifications = new List<NotificationViewModel>();
            Items = new ObservableCollection<NotificationViewModel>();
        }
        int _unreadItemCount;
        bool _existUnreadItem;
        double _viewportOffset, _viewportHeight;
        List<NotificationViewModel> _notifications;
        NotificationManager _managerModel;

        public double ViewportOffsetA
        {
            get { return _viewportOffset; }
            set
            {
                _viewportOffset = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("ViewportOffsetA"));
            }
        }
        public double ViewportHeightA
        {
            get { return _viewportHeight; }
            set
            {
                _viewportHeight = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("ViewportHeightA"));
            }
        }
        public ObservableCollection<NotificationViewModel> Items { get; private set; }
        public bool ExistUnreadItem
        {
            get { return _existUnreadItem; }
            set
            {
                _existUnreadItem = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("ExistUnreadItem"));
            }
        }
        public int UnreadItemCount
        {
            get { return _unreadItemCount; }
            set
            {
                _unreadItemCount = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("UnreadItemCount"));
            }
        }

        void model_Updated(object sender, NotificationContainerUpdatedEventArgs e)
        {
            UnreadItemCount = _managerModel.UnreadItemCount;
            ExistUnreadItem = _managerModel.UnreadItemCount > 0;

            var offset = 0.0;
            var viewportElements = new List<NotificationViewModel>();
            var updatedNotification = e.NewNotifications
                .Concat(e.UpdatedNotifications)
                .OrderByDescending(info => info.LatestNoticeDate)
                .ToArray();
            foreach (var item in _notifications)
            {
                if (offset < _viewportOffset + item.ElementHeight
                    && offset + item.ElementHeight > _viewportOffset)
                    viewportElements.Add(item);
                offset += item.ElementHeight;
            }
            foreach (var item in e.PushedOutNotifications)
            {
                var pushoutViewModel = _notifications.First(viewModel => viewModel.Model == item);
                _notifications.Remove(pushoutViewModel);
                Items.RemoveAsync(pushoutViewModel, UiThreadDispatcher);
            }
            for (var i = updatedNotification.Length - 1; i >= 0; i--)
            {
                var item = updatedNotification[i];
                if (item.ExistsNewUpdated)
                {
                    var updatedViewModel = _notifications.FirstOrDefault(viewModel => viewModel.Model == item);
                    var index = _notifications.IndexOf(updatedViewModel);
                    //新着通知だった場合は新しくVMを作成する
                    if (updatedViewModel == null)
                    {
                        NotificationViewModel obj;
                        switch (item.Type)
                        {
                            case NotificationsFilter.Mension:
                            case NotificationsFilter.OtherPost:
                            case NotificationsFilter.PostIntoYou:
                                obj = new NotificationWithActivityViewModel((NotificationInfoWithActivity)item, UiThreadDispatcher);
                                _notifications.Insert(0, obj);
                                Items.InsertAsync(0, obj, UiThreadDispatcher);
                                break;
                            case NotificationsFilter.CircleIn:
                                obj = new NotificationWithProfileViewModel(item, UiThreadDispatcher);
                                _notifications.Insert(0, obj);
                                Items.InsertAsync(0, obj, UiThreadDispatcher);
                                break;
                        }
                    }
                    else
                        //通知要素に後続通知があった場合は表示領域に入っていない通知は一番上に挿入する
                        if (!viewportElements.Select(viewModel => viewModel.Model).Contains(item))
                            Items.MoveAsync(index, 0, UiThreadDispatcher);
                }
            }
        }
    }
    public abstract class NotificationViewModel : ViewModelBase
    {
        public NotificationViewModel(Dispatcher uiThreadDispatcher) : base(uiThreadDispatcher) { }
        bool _existOverlaiedIcon;
        Uri _displayIconUrl;
        double _elementHeight;

        public abstract NotificationInfo Model { get; }
        public bool ExistOverlaiedIcon
        {
            get { return _existOverlaiedIcon; }
            protected set
            {
                _existOverlaiedIcon = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("ExistOverlaiedIcon"));
            }
        }
        public double ElementHeight
        {
            get { return _elementHeight; }
            set
            {
                _elementHeight = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("ElementHeight"));
            }
        }
        public Uri DisplayIconUrl
        {
            get { return _displayIconUrl; }
            protected set
            {
                _displayIconUrl = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("DisplayIconUrl"));
            }
        }
    }
    public class NotificationWithActivityViewModel : NotificationViewModel
    {
        public NotificationWithActivityViewModel(NotificationInfoWithActivity model, Dispatcher uiThreadDispatcher)
            : base(uiThreadDispatcher)
        {
            if (model.ActivityInfo.PostStatus != PostStatusType.Removed)
                _target = new ActivityViewModel(new Model.Activity(model.ActivityInfo), uiThreadDispatcher);
            _notificationModel = model;
            _notificationModel_Updated(null, null);
            //_notificationModel.Updated += _notificationModel_Updated;
        }
        NotificationInfoWithActivity _notificationModel;
        ActivityViewModel _target;
        string _noticeDate;
        string _noticeText;

        public ICommand MuteCommand { get; private set; }
        public string NoticeDate
        {
            get { return _noticeDate; }
            protected set
            {
                _noticeDate = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("NoticeDate"));
            }
        }
        public string NoticeText
        {
            get { return _noticeText; }
            protected set
            {
                _noticeText = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("NoticeText"));
            }
        }
        public ActivityViewModel Target
        {
            get { return _target; }
            private set
            {
                _target = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("Target"));
            }
        }
        public override NotificationInfo Model { get { return _notificationModel; } }
        void _notificationModel_Updated(object sender, NotificationUpdatedEventArgs e)
        {
            var count = 0;
            _noticeText = string.Format("{0}{1}が投稿にコメントしました。",
                string.Join(", ", _notificationModel.FollowingNotifications.Reverse().TakeWhile(model => count++ < 3).Select(notification => notification.Actor.Name)),
                _notificationModel.FollowingNotifications.Length > count
                    ? string.Format("他{0}人", _notificationModel.FollowingNotifications.Length - count)
                    : string.Empty);
            ExistOverlaiedIcon = _notificationModel.FollowingNotifications.Length > 1;
            DisplayIconUrl = new Uri(_notificationModel.FollowingNotifications.Last()
                .Actor.IconImageUrlText.Replace("$SIZE_SEGMENT", "s35-c-k"));
            _noticeDate = _notificationModel.LatestNoticeDate.ToString(
                _notificationModel.LatestNoticeDate > DateTime.Today ? "HH:mm" : "yyyy:MM:dd");
        }
    }
    public class NotificationWithProfileViewModel : NotificationViewModel
    {
        public NotificationWithProfileViewModel(NotificationInfo model, Dispatcher uiThreadDispatcher)
            : base(uiThreadDispatcher)
        {
            _notificationModel = model;
            //model.Updated += model_Updated;
        }
        int _memberCount;
        NotificationInfo _notificationModel;
        ProfileRegisterViewModel[] _members;

        public int MemberCount
        {
            get { return _memberCount; }
            set
            {
                _memberCount = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("MemberCount"));
            }
        }
        public ProfileRegisterViewModel[] Members
        {
            get { return _members; }
            set
            {
                _members = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("Members"));
            }
        }
        public override NotificationInfo Model { get { return _notificationModel; } }

        void model_Updated(object sender, NotificationUpdatedEventArgs e)
        {
            _memberCount = _notificationModel.FollowingNotifications.Length;
            _members = _notificationModel.FollowingNotifications
                .Select(obj => new ProfileRegisterViewModel(0, obj.Actor.Name, obj.Actor.IconImageUrl, UiThreadDispatcher))
                .ToArray();
        }
    }
    public class ProfileRegisterViewModel : ViewModelBase
    {
        public ProfileRegisterViewModel(int commonFriendLength, string name, Uri profileIconUrl, Dispatcher uiThreadDispatcher)
            : base(uiThreadDispatcher)
        {
            _commonFriendLength = commonFriendLength;
            _name = name;
            _profileIconUrl = profileIconUrl;
        }
        int _commonFriendLength;
        string _name;
        Uri _profileIconUrl;

        public int CommonFriendLength { get; private set; }
        public string Name { get; private set; }
        public Uri ProfileIconUrl { get; private set; }
        public ICommand IgnoreCommand { get; private set; }
    }
}

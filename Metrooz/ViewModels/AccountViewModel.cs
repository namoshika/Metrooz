using Livet;
using Livet.Commands;
using Livet.EventListeners;
using Livet.Messaging;
using MahApps.Metro.Controls.Dialogs;
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
    using Controls;
    using Models;

    public class AccountViewModel : ViewModel
    {
        public AccountViewModel(Account model, MainViewModel managerVM)
        {
            _accountModel = model;
            _userName = _accountModel.Builder.Name;
            _userMailAddress = _accountModel.Builder.Email;
            _manager = managerVM;
            _stream = new StreamManagerViewModel(_accountModel);
            _notification = new NotificationManagerViewModel(_accountModel.Notification);
            OpenAccountListCommand = new ViewModelCommand(OpenAccountListCommand_Execute);
            ActivateCommand = new ViewModelCommand(ActivateCommand_Execute);
            CompositeDisposable.Add(_thisPropChangedEventListener = new PropertyChangedEventListener(this));

            _thisPropChangedEventListener.Add(() => IsActive, IsActive_PropertyChanged);
            DataCacheDictionary
                .DownloadImage(new Uri(_accountModel.Builder.IconUrl.Replace("$SIZE_SEGMENT", "s38-c-k")))
                .ContinueWith(tsk => UserIconUrl = tsk.Result);
        }
        public AccountViewModel()
        {
            UserName = "hoge foo";
            UserMailAddress = "hoge@hoge.com";
            UserIconUrl = SampleData.DataLoader.LoadImage("accountIcon00.png").Result;
            Stream = new StreamManagerViewModel();
        }
        readonly SemaphoreSlim _syncerActivities = new SemaphoreSlim(1, 1);
        readonly PropertyChangedEventListener _thisPropChangedEventListener;
        Account _accountModel;
        MainViewModel _manager;
        StreamManagerViewModel _stream;
        NotificationManagerViewModel _notification;
        bool _isActive, _isLoading;
        string _userName, _userMailAddress;
        ImageSource _userIconUrl;

        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                _isActive = value;
                RaisePropertyChanged(() => IsActive);
            }
        }
        public bool IsLoading
        {
            get { return _isLoading; }
            set
            {
                _isLoading = value;
                RaisePropertyChanged(() => IsLoading);
            }
        }
        public string UserMailAddress
        {
            get { return _userMailAddress; }
            set
            {
                _userMailAddress = value;
                RaisePropertyChanged(() => UserMailAddress);
            }
        }
        public string UserName
        {
            get { return _userName; }
            set
            {
                _userName = value;
                RaisePropertyChanged(() => UserName);
            }
        }
        public ImageSource UserIconUrl
        {
            get { return _userIconUrl; }
            set
            {
                _userIconUrl = value;
                RaisePropertyChanged(() => UserIconUrl);
            }
        }
        public StreamManagerViewModel Stream
        {
            get { return _stream; }
            set
            {
                _stream = value;
                RaisePropertyChanged(() => Stream);
            }
        }
        public NotificationManagerViewModel Notification
        {
            get { return _notification; }
            set
            {
                _notification = value;
                RaisePropertyChanged(() => Notification);
            }
        }
        public ICommand OpenAccountListCommand { get; private set; }
        public ICommand ActivateCommand { get; private set; }
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (_stream != null)
                _stream.Dispose();
        }
        async Task Activate()
        {
            if (ViewModelUtility.IsDesginMode)
                return;
            try
            {
                await _syncerActivities.WaitAsync();
                if (IsActive == false)
                    return;

                IsLoading = true;
                if (await _accountModel.Activate())
                {
                    UserName = _accountModel.MyProfile.Name;
                    UserIconUrl = await DataCacheDictionary.DownloadImage(
                        new Uri(_accountModel.Builder.IconUrl
                            .Replace("$SIZE_SEGMENT", "s38-c-k")
                            .Replace("$SIZE_NUM", "80")));
                    Stream.SelectedIndex = 0;
                }
                else
                {
                    var message = new MetroDialogMessage(
                        "Error", "ストリームの初期化に失敗しました。ネットワークの設定を確認して下さい。", "Account/Dialog",
                        MessageDialogStyle.AffirmativeAndNegative,
                        new MetroDialogSettings() { AffirmativeButtonText = "再接続", NegativeButtonText = "別のアカウントを使う" });
                    message = await Messenger.GetResponseAsync(message);
                    switch (await message.Response)
                    {
                        case MessageDialogResult.Affirmative:
                            var tsk = Activate();
                            break;
                        case MessageDialogResult.Negative:
                            _manager.IsAccountSelectorMode = true;
                            _manager.SelectedAccountIndex = -1;
                            break;
                    }
                }
            }
            finally
            {
                IsLoading = false;
                _syncerActivities.Release();
            }
        }
        async Task Deactivate()
        {
            if (ViewModelUtility.IsDesginMode)
                return;
            try
            {
                await _syncerActivities.WaitAsync();
                await _accountModel.Deactivate();
            }
            finally { _syncerActivities.Release(); }
        }

        async void IsActive_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (ViewModelUtility.IsDesginMode)
                return;
            await Task.Run(async () =>
                {
                    if (IsActive)
                        await Activate();
                    else
                        await Deactivate();
                });
        }
        async void ActivateCommand_Execute()
        {
            if (ViewModelUtility.IsDesginMode)
                return;
            await Task.Run(async () =>
                {
                    _manager.IsAccountSelectorMode = false;
                    if (_manager.SelectedAccountIndex == _manager.Accounts.IndexOf(this))
                    {
                        //既に表示されている場合は再アクティベートする
                        await Deactivate();
                        await Activate();
                    }
                    else
                        _manager.SelectedAccountIndex = _manager.Accounts.IndexOf(this);
                });
        }
        void OpenAccountListCommand_Execute()
        {
            if (ViewModelUtility.IsDesginMode)
                return;
            _manager.IsAccountSelectorMode = !_manager.IsAccountSelectorMode;
        }
    }
}
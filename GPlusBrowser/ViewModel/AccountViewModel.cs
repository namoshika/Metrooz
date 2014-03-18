using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using MahApps.Metro.Controls.Dialogs;
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

namespace GPlusBrowser.ViewModel
{
    using Model;

    public class AccountViewModel : ViewModelBase
    {
        public AccountViewModel(Account model, MainViewModel managerVM)
        {
            _accountModel = model;
            _userName = _accountModel.Builder.Name;
            _userMailAddress = _accountModel.Builder.Email;
            _manager = managerVM;
            _stream = new StreamManagerViewModel(_accountModel);
            _notification = new NotificationManagerViewModel(_accountModel.Notification);
            OpenAccountListCommand = new RelayCommand(OpenAccountListCommand_Execute);
            ActivateCommand = new RelayCommand(ActivateCommand_Execute);

            PropertyChanged += AccountViewModel_PropertyChanged;
            DataCacheDictionary.DownloadImage(new Uri(_accountModel.Builder.IconUrl.Replace("$SIZE_SEGMENT", "s38-c-k")))
                .ContinueWith(tsk => UserIconUrl = tsk.Result);
        }
        readonly SemaphoreSlim _syncerActivities = new SemaphoreSlim(1, 1);
        Account _accountModel;
        MainViewModel _manager;
        StreamManagerViewModel _stream;
        NotificationManagerViewModel _notification;
        int _selectedIndex;
        bool _isActive, _isLoading;
        string _userName, _userMailAddress;
        ImageSource _userIconUrl;

        public bool IsActive
        {
            get { return _isActive; }
            set { Set(() => IsActive, ref _isActive, value); }
        }
        public bool IsLoading
        {
            get { return _isLoading; }
            set { Set(() => IsLoading, ref _isLoading, value); }
        }
        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set { Set(() => SelectedIndex, ref _selectedIndex, value); }
        }
        public string UserMailAddress
        {
            get { return _userMailAddress; }
            set { Set(() => UserMailAddress, ref _userMailAddress, value); }
        }
        public string UserName
        {
            get { return _userName; }
            set { Set(() => UserName, ref _userName, value); }
        }
        public ImageSource UserIconUrl
        {
            get { return _userIconUrl; }
            set { Set(() => UserIconUrl, ref _userIconUrl, value); }
        }
        public StreamManagerViewModel Stream
        {
            get { return _stream; }
            set { Set(() => Stream, ref _stream, value); }
        }
        public NotificationManagerViewModel Notification
        {
            get { return _notification; }
            set { Set(() => Notification, ref _notification, value); }
        }
        public ICommand OpenAccountListCommand { get; private set; }
        public ICommand ActivateCommand { get; private set; }
        public override void Cleanup()
        {
            base.Cleanup();
            if (_stream != null)
                _stream.Cleanup();
        }
        async Task Activate()
        {
            try
            {
                await _syncerActivities.WaitAsync().ConfigureAwait(false);
                if (IsActive == false)
                    return;

                SelectedIndex = 0;
                IsLoading = true;
                await _accountModel.Activate().ConfigureAwait(false);
                UserName = _accountModel.MyProfile.Name;
                UserIconUrl = await DataCacheDictionary.DownloadImage(
                    new Uri(_accountModel.Builder.IconUrl
                        .Replace("$SIZE_SEGMENT", "s38-c-k")
                        .Replace("$SIZE_NUM", "80"))).ConfigureAwait(false);
            }
            catch (FailToOperationException)
            {
                var message = new DialogOptionInfo(
                    "Error", "ストリームの初期化に失敗しました。ネットワークの設定を確認して下さい。",
                    setting: new MetroDialogSettings() { AffirmativeButtonText = "再接続" });
                Messenger.Default.Send(message);
                var tmp = message.CallbackTask.ContinueWith(tsk => Activate());
            }
            finally
            {
                IsLoading = false;
                _syncerActivities.Release();
            }
        }
        async Task Deactivate()
        {
            try
            {
                await _syncerActivities.WaitAsync().ConfigureAwait(false);
                SelectedIndex = -1;
                await _accountModel.Deactivate().ConfigureAwait(false);
            }
            finally { _syncerActivities.Release(); }
        }

        void AccountViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch(e.PropertyName)
            {
                case "IsActive":
                    Task.Run(async () =>
                        {
                            if (IsActive)
                                await Activate();
                            else
                                await Deactivate();
                        });
                    break;
            }
        }
        void OpenAccountListCommand_Execute()
        { _manager.IsAccountSelectorMode = !_manager.IsAccountSelectorMode; }
        async void ActivateCommand_Execute()
        {
            _manager.IsAccountSelectorMode = false;
            if (_manager.SelectedPageIndex == _manager.Pages.IndexOf(this))
            {
                //既に表示されている場合は再アクティベートする
                await Deactivate().ConfigureAwait(false);
                await Activate().ConfigureAwait(false);
            }
            else
                _manager.SelectedPageIndex = _manager.Pages.IndexOf(this);
        }
    }
}
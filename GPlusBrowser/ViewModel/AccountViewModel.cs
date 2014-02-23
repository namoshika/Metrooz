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
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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
            _stream = new StreamManagerViewModel(_accountModel.Stream, _accountModel);
            //Notification = new NotificationManagerViewModel(_accountModel.Notification, this, UiThreadDispatcher);
            ConnectStreamCommand = new RelayCommand(ConnectStreamCommand_Execute);
            OpenAccountListCommand = new RelayCommand(OpenAccountListCommand_Execute);
            ActivateCommand = new RelayCommand(ActivateCommand_Execute);

            PropertyChanged += AccountViewModel_PropertyChanged;
            DataCacheDictionary.DownloadImage(new Uri(_accountModel.Builder.IconUrl.Replace("$SIZE_SEGMENT", "s38-c-k")))
                .ContinueWith(tsk => UserIconUrl = tsk.Result);
        }
        MainViewModel _manager;
        Account _accountModel;
        StreamManagerViewModel _stream;
        BitmapImage _userIconUrl;
        string _userName, _userMailAddress;
        bool _isActive, _isLoading;
        readonly System.Threading.SemaphoreSlim _syncerActivities = new System.Threading.SemaphoreSlim(1, 1);

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
        public BitmapImage UserIconUrl
        {
            get { return _userIconUrl; }
            set { Set(() => UserIconUrl, ref _userIconUrl, value); }
        }
        public StreamManagerViewModel Stream
        {
            get { return _stream; }
            set { Set(() => Stream, ref _stream, value); }
        }
        public ICommand ConnectStreamCommand { get; private set; }
        public ICommand OpenAccountListCommand { get; private set; }
        public ICommand ActivateCommand { get; private set; }
        public async void Initialize()
        {
            try
            {
                await _syncerActivities.WaitAsync();
                if (IsActive == false)
                    return;

                IsLoading = true;
                await _accountModel.Initialize(false).ConfigureAwait(false);
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
                var tmp = message.CallbackTask.ContinueWith(tsk => Initialize());
            }
            finally
            {
                IsLoading = false;
                _syncerActivities.Release();
            }
        }
        public override void Cleanup()
        {
            base.Cleanup();
            if (_stream != null)
                _stream.Cleanup();
        }

        void AccountViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch(e.PropertyName)
            {
                case "IsActive":
                    Initialize();
                    break;
            }
        }
        void ConnectStreamCommand_Execute()
        { _accountModel.Connect(); }
        void OpenAccountListCommand_Execute()
        { _manager.IsAccountSelectorMode = !_manager.IsAccountSelectorMode; }
        void ActivateCommand_Execute()
        {
            _manager.IsAccountSelectorMode = false;
            _manager.SelectedPageIndex = _manager.Pages.IndexOf(this);
        }
    }
}
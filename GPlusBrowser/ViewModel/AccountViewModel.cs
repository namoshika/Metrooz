using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace GPlusBrowser.ViewModel
{
    using Model;

    public class AccountViewModel : ViewModelBase
    {
        public AccountViewModel(Account model)
        {
            _accountModel = model;
            _accountModel.Initialized += _accountModel_Initialized;
            _accountModel.ChangedConnectStatus += _accountModel_ChangedConnectStatus;
            _userName = _accountModel.Builder.Name;
            DataCacheDictionary.DownloadImage(new Uri(_accountModel.Builder.IconUrl.Replace("$SIZE_SEGMENT", "s35-c-k")))
                .ContinueWith(tsk => UserIconUrl = tsk.Result);

            OpenStreamPanelCommand = new RelayCommand(OpenStreamPanelCommand_Execute);
            BackToAccountManagerCommand = new RelayCommand(BackToAccountManagerCommand_Execute);
            ConnectStreamCommand = new RelayCommand(ConnectStreamCommand_Execute);
        }
        Account _accountModel;
        StreamManagerViewModel _stream;
        ImageSource _userIconUrl;
        bool _isShowStatusText;
        string _statusText;
        string _userName;

        public bool IsShowStatusText
        {
            get { return _isShowStatusText; }
            set { Set(() => IsShowStatusText, ref _isShowStatusText, value); }
        }
        public string StatusText
        {
            get { return _statusText; }
            set { Set(() => StatusText, ref _statusText, value); }
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
        public ICommand OpenStreamPanelCommand { get; private set; }
        public ICommand BackToAccountManagerCommand { get; private set; }
        public ICommand ConnectStreamCommand { get; private set; }
        public override void Cleanup()
        {
            base.Cleanup();

            _accountModel.Initialized -= _accountModel_Initialized;
            _accountModel = null;
            if (_stream != null)
                _stream.Cleanup();
            _stream = null;
        }

        async void _accountModel_Initialized(object sender, EventArgs e)
        {
            if (_accountModel.IsInitialized == false)
                Stream.SelectedCircleIndex = -1;
            else
            {
                Stream = new StreamManagerViewModel(_accountModel.Stream);
                //Notification = new NotificationManagerViewModel(_accountModel.Notification, this, UiThreadDispatcher);
                UserName = _accountModel.MyProfile.Name;
                UserIconUrl = await DataCacheDictionary.DownloadImage(
                    new Uri(_accountModel.Builder.IconUrl
                        .Replace("$SIZE_SEGMENT", "s35-c-k")
                        .Replace("$SIZE_NUM", "80")));
            }
        }
        void _accountModel_ChangedConnectStatus(object sender, EventArgs e)
        {
            if (_accountModel.IsConnected)
                IsShowStatusText = false;
            else
            {
                IsShowStatusText = true;
                StatusText = "ストリームへの接続が切断されました。";
            }
        }
        async void OpenStreamPanelCommand_Execute()
        {
            OnOpenedStreamPanel(new EventArgs());
            await _accountModel.Initialize(false);
        }
        void BackToAccountManagerCommand_Execute()
        { OnBackedToAccountManager(new EventArgs()); }
        void ConnectStreamCommand_Execute()
        { _accountModel.Connect(); }

        public event EventHandler OpenedStreamPanel;
        protected virtual void OnOpenedStreamPanel(EventArgs e)
        {
            if (OpenedStreamPanel != null)
                OpenedStreamPanel(this, e);
        }
        public event EventHandler BackedToAccountManager;
        protected virtual void OnBackedToAccountManager(EventArgs e)
        {
            if (BackedToAccountManager != null)
                BackedToAccountManager(this, e);
        }
    }
}
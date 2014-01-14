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

    public class AccountViewModel : ViewModelBase, IDisposable
    {
        public AccountViewModel(Account model, Dispatcher uiThreadDispatcher)
            : base(uiThreadDispatcher, null)
        {
            _accountModel = model;
            _accountModel.Initialized += _accountModel_Initialized;
            _accountModel.ChangedConnectStatus += _accountModel_ChangedConnectStatus;
            _userName = _accountModel.Builder.Name;
            //DataCacheDictionary.Default
            //    .DownloadImage(new Uri(_accountModel.Builder.IconUrl.Replace("$SIZE_SEGMENT", "s35-c-k")))
            //    .ContinueWith(tsk => UserIconUrl = tsk.Result);

            OpenStreamPanelCommand = new RelayCommand(OpenStreamPanelCommand_Execute);
            BackToAccountManagerCommand = new RelayCommand(BackToAccountManagerCommand_Execute);
            ConnectStreamCommand = new RelayCommand(ConnectStreamCommand_Execute);
        }
        Account _accountModel;
        //NotificationManagerViewModel _notification;
        StreamManagerViewModel _stream;
        ImageSource _userIconUrl;
        bool _isShowStatusText;
        string _statusText;
        string _userName;

        public DataCacheDictionary DataCacheDict { get; private set; }
        public bool IsShowStatusText
        {
            get { return _isShowStatusText; }
            set
            {
                _isShowStatusText = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("IsShowStatusText"));
            }
        }
        public string StatusText
        {
            get { return _statusText; }
            set
            {
                _statusText = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("StatusText"));
            }
        }
        public string UserName
        {
            get { return _userName; }
            set
            {
                _userName = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("UserName"));
            }
        }
        public ImageSource UserIconUrl
        {
            get { return _userIconUrl; }
            set
            {
                _userIconUrl = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("UserIconUrl"));
            }
        }
        public StreamManagerViewModel Stream
        {
            get { return _stream; }
            set
            {
                _stream = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("Stream"));
            }
        }
        //public NotificationManagerViewModel Notification
        //{
        //    get { return _notification; }
        //    set
        //    {
        //        _notification = value;
        //        OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("Notification"));
        //    }
        //}
        public ICommand OpenStreamPanelCommand { get; private set; }
        public ICommand BackToAccountManagerCommand { get; private set; }
        public ICommand ConnectStreamCommand { get; private set; }
        public void Dispose()
        {
            _accountModel.Initialized -= _accountModel_Initialized;
            _accountModel = null;
            if (_stream != null)
                _stream.Dispose();
            _stream = null;
        }

        async void _accountModel_Initialized(object sender, EventArgs e)
        {
            if (_accountModel.IsInitialized == false)
                Stream.SelectedCircleIndex = -1;
            else
            {
                DataCacheDict = new DataCacheDictionary(_accountModel.PlusClient.NormalHttpClient);
                Stream = new StreamManagerViewModel(_accountModel.Stream, this, UiThreadDispatcher);
                //Notification = new NotificationManagerViewModel(_accountModel.Notification, this, UiThreadDispatcher);
                UserName = _accountModel.MyProfile.Name;
                UserIconUrl = await DataCacheDict.DownloadImage(
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
        async void OpenStreamPanelCommand_Execute(object arg)
        {
            OnOpenedStreamPanel(new EventArgs());
            await _accountModel.Initialize(false);
        }
        void BackToAccountManagerCommand_Execute(object arg)
        { OnBackedToAccountManager(new EventArgs()); }
        void ConnectStreamCommand_Execute(object arg)
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
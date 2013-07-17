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
        public AccountViewModel(Account model, AccountManager manager, Dispatcher uiThreadDispatcher)
            : base(uiThreadDispatcher, null)
        {
            _accountModel = model;
            _accountModel.Initialized += _accountModel_Initialized;
            _accountModel.ChangedConnectStatus += _accountModel_ChangedConnectStatus;
            _accountManagerModel = manager;
            _userName = _accountModel.Setting.UserName;
            _loginer = new LoginerViewModel(manager, model, uiThreadDispatcher);
            DataCacheDictionary.Default.DownloadImage(new Uri(_accountModel.AccountIconUrl.Replace("$SIZE_SEGMENT", "s35-c-k")))
                .ContinueWith(tsk => _accountIconUrl = tsk.Result);

            OpenStreamPanelCommand = new RelayCommand(OpenStreamPanelCommand_Execute);
            BackToAccountManagerCommand = new RelayCommand(BackToAccountManagerCommand_Execute);
            ConnectStreamCommand = new RelayCommand(ConnectStreamCommand_Execute);
        }
        Account _accountModel;
        AccountManager _accountManagerModel;
        NotificationManagerViewModel _notification;
        StreamManagerViewModel _stream;
        LoginerViewModel _loginer;
        ImageSource _accountIconUrl;
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
        public LoginerViewModel Loginer
        {
            get { return _loginer; }
            set
            {
                _loginer = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("Loginer"));
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
        public NotificationManagerViewModel Notification
        {
            get { return _notification; }
            set
            {
                _notification = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("Notification"));
            }
        }
        public ImageSource AccountIconUrl
        {
            get { return _accountIconUrl; }
            set
            {
                _accountIconUrl = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("AccountIconUrl"));
            }
        }
        public ICommand OpenStreamPanelCommand { get; private set; }
        public ICommand BackToAccountManagerCommand { get; private set; }
        public ICommand ConnectStreamCommand { get; private set; }
        public void Dispose()
        {
            _accountModel.Initialized -= _accountModel_Initialized;
            _accountModel = null;
            _accountManagerModel = null;
            if (_stream != null)
                _stream.Dispose();
            _stream = null;
        }

        async void _accountModel_Initialized(object sender, EventArgs e)
        {
            //DisableSessionの場合はLoginer内部でログイン & 初期化が行われる
            if (_accountModel.InitializeSequenceStatus < AccountInitSeqStatus.DisableSession)
            {
                DataCacheDict = new DataCacheDictionary(_accountModel.PlusClient.NormalHttpClient);
                Stream = new StreamManagerViewModel(_accountModel.Stream, this, UiThreadDispatcher);
                Notification = new NotificationManagerViewModel(_accountModel.Notification, this, UiThreadDispatcher);
                UserName = _accountModel.MyProfile.Name;
                AccountIconUrl = await DataCacheDict.DownloadImage(
                    new Uri(_accountModel.AccountIconUrl.Replace("$SIZE_SEGMENT", "s35-c-k")));
            }
        }
        void _accountModel_ChangedConnectStatus(object sender, EventArgs e)
        {
            if (_accountModel.IsConnected != TalkGadgetBindStatus.DisableConnect)
                IsShowStatusText = false;
            else
            {
                IsShowStatusText = true;
                StatusText = "ストリームへの接続が切断されました。";
            }
        }
        async void OpenStreamPanelCommand_Execute(object arg)
        {
            var targetIdx = _accountManagerModel.Accounts.IndexOf(_accountModel);
            _accountManagerModel.SelectedAccountIndex = targetIdx;

            var seqStatus = _accountManagerModel.Accounts[targetIdx].InitializeSequenceStatus;
            if (seqStatus != AccountInitSeqStatus.UnLogined && seqStatus < AccountInitSeqStatus.LoadedHomeInit)
                await _accountManagerModel.Accounts[targetIdx].Initialize().ConfigureAwait(false);
        }
        void BackToAccountManagerCommand_Execute(object arg)
        { _accountManagerModel.SelectedAccountIndex = -1; }
        void ConnectStreamCommand_Execute(object arg)
        {
            var account = _accountManagerModel.Accounts[_accountManagerModel.SelectedAccountIndex];
            account.Connect();
        }
    }
}
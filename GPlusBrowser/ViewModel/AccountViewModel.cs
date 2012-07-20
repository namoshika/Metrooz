using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows.Threading;

namespace GPlusBrowser.ViewModel
{
    using Model;

    public class AccountViewModel : ViewModelBase, IDisposable
    {
        public AccountViewModel(Account model, AccountManager manager, Dispatcher uiThreadDispatcher)
            : base(uiThreadDispatcher)
        {
            _accountModel = model;
            _accountModel.Initialized += _accountModel_Initialized;
            _accountModel.ChangedConnectStatus += _accountModel_ChangedConnectStatus;
            _accountManagerModel = manager;
            Stream = new StreamManagerViewModel(model.Stream, uiThreadDispatcher);
            BackToAccountManagerCommand = new RelayCommand(BackToAccountManagerCommand_Execute);
            ConnectStreamCommand = new RelayCommand(ConnectStreamCommand_Execute);
        }
        Account _accountModel;
        AccountManager _accountManagerModel;
        StreamManagerViewModel _stream;
        Uri _accountIconUrl;
        bool _isShowStatusText;
        string _statusText;

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
        public StreamManagerViewModel Stream
        {
            get { return _stream; }
            set
            {
                _stream = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("Stream"));
            }
        }
        public Uri AccountIconUrl
        {
            get { return _accountIconUrl; }
            set
            {
                _accountIconUrl = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("AccountIconUrl"));
            }
        }
        public ICommand BackToAccountManagerCommand { get; private set; }
        public ICommand ConnectStreamCommand { get; private set; }
        public void Dispose()
        {
            _accountModel.Initialized -= _accountModel_Initialized;
            _accountModel = null;
            _accountManagerModel = null;
            _stream.Dispose();
            _stream = null;
        }

        void _accountModel_Initialized(object sender, EventArgs e)
        {
            AccountIconUrl = new Uri(_accountModel.AccountIconUrl.Replace("$SIZE_SEGMENT", "s35-c-k"));
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
        void BackToAccountManagerCommand_Execute(object arg)
        {
            _accountManagerModel.SelectedAccountIndex = -1;
        }
        void ConnectStreamCommand_Execute(object arg)
        {
            var account = _accountManagerModel.Accounts[_accountManagerModel.SelectedAccountIndex];
            account.Reconnect();
        }
    }
}
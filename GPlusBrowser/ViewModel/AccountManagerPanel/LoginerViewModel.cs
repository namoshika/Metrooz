using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace GPlusBrowser.ViewModel
{
    using Model;

    public class LoginerViewModel : ViewModelBase
    {
        public LoginerViewModel(AccountManager accountManagerModel, Account target, Dispatcher uiThreadDispatcher)
            : base(uiThreadDispatcher, null)
        {
            _accountManagerModel = accountManagerModel;
            IsShowLoginPanel = false;
            IsRelogin = target != null;
            TargetAccount = target;
            OpenAddAccountPanelCommand = new RelayCommand(OpenAddAccountPanelCommand_Executed);
            LoginCommand = new RelayCommand(LoginCommand_Executed);
            CancelCommand = new RelayCommand(CancelCommand_Executed);

            if(target != null)
                TargetAccount.Initialized += TargetAccount_Initialized;
        }
        bool _loginButtonIsEnabled, _isShowLoginPanel, _isRelogin;
        string _emailAddress;
        string _password;
        string _userName;
        string _notificationText;
        System.Windows.Media.ImageSource _iconImageUrl;
        AccountManager _accountManagerModel;
        LoginSequenceStatus _status;

        public ICommand OpenAddAccountPanelCommand { get; set; }
        public ICommand LoginCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }
        public Account TargetAccount { get; private set; }
        public bool LoginButtonIsEnabled
        {
            get { return _loginButtonIsEnabled; }
            set
            {
                _loginButtonIsEnabled = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("LoginButtonIsEnabled"));
            }
        }
        public bool IsShowLoginPanel
        {
            get { return _isShowLoginPanel; }
            set
            {
                _isShowLoginPanel = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("IsShowLoginPanel"));
            }
        }
        public bool IsRelogin
        {
            get { return _isRelogin; }
            set
            {
                _isRelogin = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("IsRelogin"));
            }
        }
        public string EmailAddress
        {
            get { return _emailAddress; }
            set
            {
                _emailAddress = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("EmailAddress"));
            }
        }
        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("Password"));
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
        public string NotificationText
        {
            get { return _notificationText; }
            set
            {
                _notificationText = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("NotificationText"));
            }
        }
        public ImageSource IconImageUrl
        {
            get { return _iconImageUrl; }
            set
            {
                _iconImageUrl = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("IconImageUrl"));
            }
        }
        public LoginSequenceStatus Status
        {
            get { return _status; }
            set
            {
                _status = value;
                CommandManager.InvalidateRequerySuggested();
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("Status"));
            }
        }

        void TargetAccount_Initialized(object sender, EventArgs e)
        {
            if (TargetAccount.InitializeSequenceStatus >= AccountInitSeqStatus.DisableSession)
                OpenAddAccountPanelCommand_Executed(null);
        }
        async void LoginCommand_Executed(object arg)
        {
            switch (Status)
            {
                case LoginSequenceStatus.Input:
                case LoginSequenceStatus.Fail:
                    if (string.IsNullOrEmpty(EmailAddress) || string.IsNullOrEmpty(Password))
                    {
                        Status = LoginSequenceStatus.Fail;
                        LoginButtonIsEnabled = true;
                        NotificationText = "メールアドレスやパスワードに未入力な項目があります。";
                    }
                    else
                    {
                        Status = LoginSequenceStatus.Authing;
                        LoginButtonIsEnabled = false;

                        await TargetAccount.Login(EmailAddress, Password).ConfigureAwait(false);
                        var isLogin = TargetAccount.InitializeSequenceStatus > AccountInitSeqStatus.UnLogined;
                        Status = isLogin ? LoginSequenceStatus.Success : LoginSequenceStatus.Fail;
                        LoginButtonIsEnabled = true;

                        if (isLogin)
                        {
                            IconImageUrl = await DataCacheDictionary.Default.DownloadImage(
                                new Uri(TargetAccount.AccountIconUrl.Replace("$SIZE_SEGMENT", "s120-c-k")));
                        }
                        else
                        {
                            NotificationText = isLogin
                                ? null : "ログインに失敗しました。メールアドレスやパスワードに間違いがある可能性があります。";
                        }
                    }
                    break;
                case LoginSequenceStatus.Success:
                    Status = LoginSequenceStatus.Registed;
                    IsShowLoginPanel = false;
                    if (IsRelogin == false)
                        _accountManagerModel.Add(TargetAccount);
                    else
                        _accountManagerModel.Initialize();
                    break;
            }
        }
        void OpenAddAccountPanelCommand_Executed(object arg)
        {
            TargetAccount = TargetAccount ?? _accountManagerModel.Create();
            IconImageUrl = null;
            EmailAddress = null;
            Password = null;
            UserName = null;
            IsShowLoginPanel = true;
            Status = LoginSequenceStatus.Input;
        }
        void CancelCommand_Executed(object arg)
        { IsShowLoginPanel = false; }
    }
    public enum DisplayMode { Login, Relogin, }
    public enum LoginSequenceStatus { Input, Authing, Fail, Success, Registed }

    public class LoginSequenceStatusToButtonText : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            switch ((LoginSequenceStatus)value)
            {
                case LoginSequenceStatus.Input:
                    return "ログイン";
                case LoginSequenceStatus.Authing:
                    return "認証中...";
                case LoginSequenceStatus.Fail:
                    return "ログイン";
                case LoginSequenceStatus.Success:
                    return "このアカウントを登録する";
                default:
                    return null;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        { throw new NotImplementedException(); }
    }
}
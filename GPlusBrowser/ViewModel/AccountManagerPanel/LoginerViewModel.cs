using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace GPlusBrowser.ViewModel
{
    using Model;

    public class LoginerViewModel : ViewModelBase
    {
        public LoginerViewModel(AccountManager accountManagerModel, Dispatcher uiThreadDispatcher)
            : base(uiThreadDispatcher)
        {
            _accountManagerModel = accountManagerModel;
            _status = LoginSequenceStatus.Hidden;
            LoginCommand = new RelayCommand(LoginCommand_Executed, LoginAndCancelCommand_CanExecuted);
            CancelCommand = new RelayCommand(CancelCommand_Executed, LoginAndCancelCommand_CanExecuted);
        }
        AccountManager _accountManagerModel;
        bool _loginButtonIsEnabled;
        bool _isRelogin;
        string _emailAddress;
        string _password;
        string _userName;
        string _notificationText;
        Uri _iconImageUrl;
        LoginSequenceStatus _status;

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
        public Uri IconImageUrl
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

        public void OpenLoginForm()
        {
            OpenReloginForm(_accountManagerModel.Create());
        }
        public void OpenReloginForm(Account item)
        {
            TargetAccount = item;
            Status = LoginSequenceStatus.Input;
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
                        { IconImageUrl = new Uri(TargetAccount.AccountIconUrl.Replace("$SIZE_SEGMENT", "s120-c-k")); }
                        else
                        {
                            NotificationText = isLogin
                                ? string.Empty : "ログインに失敗しました。メールアドレスやパスワードに間違いがある可能性があります。";
                        }
                    }
                    break;
                case LoginSequenceStatus.Success:
                    _accountManagerModel.Add(TargetAccount);
                    Status = LoginSequenceStatus.Hidden;
                    IconImageUrl = null;
                    EmailAddress = null;
                    Password = null;
                    UserName = null;
                    break;
            }
        }
        void CancelCommand_Executed(object arg)
        {
            Status = LoginSequenceStatus.Hidden;
            IconImageUrl = null;
            EmailAddress = null;
            Password = null;
            UserName = null;
            NotificationText = string.Empty;
        }
        bool LoginAndCancelCommand_CanExecuted(object arg)
        { return Status != LoginSequenceStatus.Authing; }
    }
    public enum DisplayMode { Login, Relogin, }
    public enum LoginSequenceStatus { Hidden, Input, Authing, Fail, Success, }

    public class LoginSequenceStatusToVisibility : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return ((LoginSequenceStatus)value) == LoginSequenceStatus.Hidden
                ? System.Windows.Visibility.Hidden : System.Windows.Visibility.Visible;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        { throw new NotImplementedException(); }
    }
    public class LoginSequenceStatusToButtonText : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            switch ((LoginSequenceStatus)value)
            {
                case LoginSequenceStatus.Hidden:
                    return "None";
                case LoginSequenceStatus.Input:
                    return "ログイン";
                case LoginSequenceStatus.Authing:
                    return "認証中...";
                case LoginSequenceStatus.Fail:
                    return "ログイン";
                case LoginSequenceStatus.Success:
                    return "このアカウントを登録する";
            }
            return ((LoginSequenceStatus)value) == LoginSequenceStatus.Hidden
                ? System.Windows.Visibility.Hidden : System.Windows.Visibility.Visible;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        { throw new NotImplementedException(); }
    }
}
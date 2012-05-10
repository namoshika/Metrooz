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
        bool _isCanceled;
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
        public bool IsCanceled
        {
            get { return _isCanceled; }
            set
            {
                _isCanceled = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("IsCanceled"));
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

        public void OpenPanel(Account item, bool isRelogin)
        {
            TargetAccount = item;
            IsRelogin = isRelogin;
            Status = LoginSequenceStatus.Input;
            NotificationText = string.Empty;
        }

        async void LoginCommand_Executed(object arg)
        {
            switch (Status)
            {
                case LoginSequenceStatus.Input:
                case LoginSequenceStatus.Fail:
                    Status = LoginSequenceStatus.Authing;
                    if (string.IsNullOrEmpty(EmailAddress) || string.IsNullOrEmpty(Password))
                    {
                        Status = LoginSequenceStatus.Fail;
                        NotificationText = "メールアドレスやパスワードに未入力な項目があります。";
                    }
                    else
                    {
                        await TargetAccount.Login(EmailAddress, Password).ConfigureAwait(false);
                        Status = TargetAccount.IsLogined ? LoginSequenceStatus.Success : LoginSequenceStatus.Fail;
                        if (TargetAccount.IsLogined)
                        {
                            IconImageUrl = new Uri(TargetAccount.AccountIconUrl.Replace("$SIZE_SEGMENT", "s120-c-k"));
                        }
                        else
                        {
                            NotificationText = TargetAccount.IsLogined
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
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace GPlusBrowser.ViewModel
{
    using GPlusBrowser.Model;

    public class SettingViewModel : ViewModelBase
    {
        public SettingViewModel(SettingModel setting, Account mainWinModel)
        {
            NotificationText = string.Empty;

            PropertyChanged += SettingViewModel_PropertyChanged;
            SaveConfigCommand = new RelayCommand(
                () =>
                {
                    NotificationText = string.Empty;
                    Status = SettingStatusType.Checking;
                    //if (EmailAddress != null && Password != null)
                    //{
                    //    await mainWinModel.Login(EmailAddress, Password).ConfigureAwait(false);
                    //    Status = SettingStatusType.FailLogin;
                    //    NotificationText = "ログインに失敗しました。";
                    //}
                    //else
                    //{
                    //    Status = SettingStatusType.Normal;
                    //    NotificationText = string.Empty;
                    //    IsModified = false;
                    //    IsExpanded = false;
                    //}
                });
            CancelConfigCommand = new RelayCommand(
                () =>
                {
                    EmailAddress = string.Empty;
                    Password = string.Empty;
                    NotificationText = string.Empty;
                    IsModified = false;
                    IsExpanded = false;
                });
        }

        bool _isExpanded;
        bool _isModified;
        string _emailAddress;
        string _password;
        string _notificationText;
        SettingStatusType _status;

        public SettingStatusType Status
        {
            get { return _status; }
            set { Set(() => Status, ref _status, value); }
        }
        public bool IsModified
        {
            get { return _isModified; }
            set { Set(() => IsModified, ref _isModified, value); }
        }
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set { Set(() => IsExpanded, ref _isExpanded, value); }
        }
        public string EmailAddress
        {
            get { return _emailAddress; }
            set { Set(() => EmailAddress, ref _emailAddress, value); }
        }
        public string Password
        {
            get { return _password; }
            set { Set(() => Password, ref _password, value); }
        }
        public string NotificationText
        {
            get { return _notificationText; }
            set { Set(() => NotificationText, ref _notificationText, value); }
        }
        public ICommand SaveConfigCommand { get; set; }
        public ICommand CancelConfigCommand { get; set; }

        void SettingViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "IsModified")
                IsModified = true;
        }
    }
    public enum SettingStatusType { Normal, Checking, FailLogin, FailSave }

    class SettingStatusToBooleanConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var status = (SettingStatusType)value;
            return status != SettingStatusType.Checking;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
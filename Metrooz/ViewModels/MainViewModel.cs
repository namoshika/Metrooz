using Livet;
using Livet.Messaging;
using MahApps.Metro.Controls.Dialogs;
using SunokoLibrary.Web.GooglePlus;
using SunokoLibrary.Collections.ObjectModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Metrooz.ViewModels
{
    using Controls;
    using Models;

    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModel
    {
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            if (ViewModelUtility.IsDesginMode)
            {
                _selectedAccountIndex = 0;
                _isAccountSelectorMode = true;
                _noSelectableAccount = false;
                Accounts = new ReadOnlyDispatcherCollection<AccountViewModel>(
                    new DispatcherCollection<AccountViewModel>(
                    new ObservableCollection<AccountViewModel>(
                        Enumerable.Range(0, 2).Select(idx => new AccountViewModel())), App.Current.Dispatcher));
            }
            else
            {
                _selectedAccountIndex = -1;
                _isAccountSelectorMode = false;
                _noSelectableAccount = false;
                _accountManagerModel = AccountManager.Current;
                CompositeDisposable.Add(Accounts = ViewModelHelper.CreateReadOnlyDispatcherCollection(
                    _accountManagerModel.Accounts, model => new AccountViewModel(model, this), App.Current.Dispatcher));
            }
        }
        AccountManager _accountManagerModel;
        bool _isAccountSelectorMode;
        bool _noSelectableAccount;
        int _selectedAccountIndex;

        public int SelectedAccountIndex
        {
            get { return _selectedAccountIndex; }
            set
            {
                _selectedAccountIndex = value;
                RaisePropertyChanged(() => SelectedAccountIndex);

                //直前に開かれているアカウントを次回起動時に開くために記録する
                if (value < 0 || value >= _accountManagerModel.Accounts.Count)
                    return;
                Properties.Settings.Default.RecentlyUsedAccountEMailAddress = _accountManagerModel.Accounts[value].Builder.Email;
                Properties.Settings.Default.Save();
            }
        }
        public bool IsAccountSelectorMode
        {
            get { return _isAccountSelectorMode; }
            set
            {
                _isAccountSelectorMode = value;
                RaisePropertyChanged(() => IsAccountSelectorMode);
            }
        }
        public bool NoSelectableAccount
        {
            get { return _noSelectableAccount; }
            set
            {
                _noSelectableAccount = value;
                RaisePropertyChanged(() => NoSelectableAccount);
            }
        }
        public ReadOnlyDispatcherCollection<AccountViewModel> Accounts { get; set; }
        public async void Activate()
        {
            if (ViewModelUtility.IsDesginMode)
                return;
            //エラー発生時は確認画面を表示して再試行を促す
            if(await _accountManagerModel.Activate() == false)
            {
                var message = new MetroDialogMessage(
                    "Error", "アカウント一覧の読み込みに失敗しました。ネットワークの設定を確認して下さい。",
                    "Main/Dialog", setting: new MetroDialogSettings() { AffirmativeButtonText = "再接続" });
                message = await Messenger.GetResponseAsync(message);
                await message.Response;
                Activate();
                return;
            }
            //初期化成功時は継続
            NoSelectableAccount = _accountManagerModel.Accounts.Count == 0;
            if (Properties.Settings.Default.RecentlyUsedAccountEMailAddress != null)
            {
                //前回起動時の最後に表示されていたアカウントのメールアドレスが
                //読み込まれているアカウントリストにある場合はこれを表示する
                var recentlyUsedAccount = _accountManagerModel.Accounts.FirstOrDefault(
                    account => account.Builder.Email == Properties.Settings.Default.RecentlyUsedAccountEMailAddress);
                if (recentlyUsedAccount != null)
                    SelectedAccountIndex = _accountManagerModel.Accounts.IndexOf(recentlyUsedAccount);
                else
                {
                    IsAccountSelectorMode = true;
                    SelectedAccountIndex = -1;
                }
            }
        }
    }
    public class IntToVisibilityConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        { return ((int)value) != -1 ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed; }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        { throw new NotImplementedException(); }
    }
}
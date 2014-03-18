using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Ioc;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using SunokoLibrary.Web.GooglePlus;

namespace GPlusBrowser.ViewModel
{
    using Model;

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
    public class MainViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel(AccountManager accountManagerModel)
        {
            _selectedAccountIndex = -1;
            _isAccountSelectorMode = false;
            _noSelectableAccount = false;
            _accountManagerModel = accountManagerModel;
            Accounts = new ObservableCollection<AccountViewModel>();
            Pages = new ObservableCollection<AccountViewModel>();
            Pages.Add(null);

            _accountManagerModel.Initialized += _accountManagerModel_Initialized;
            _accountManagerModel.Accounts.CollectionChanged += PageSwitcherViewModel_CollectionChanged;
            Initialize();
        }
        readonly SemaphoreSlim _accountSyncer = new SemaphoreSlim(1, 1);
        AccountManager _accountManagerModel;
        bool _isAccountSelectorMode;
        bool _noSelectableAccount;
        int _selectedAccountIndex;

        public int SelectedAccountIndex
        {
            get { return _selectedAccountIndex; }
            set
            {
                if (Set(() => SelectedAccountIndex, ref _selectedAccountIndex, value) == false)
                    return;
                RaisePropertyChanged(() => SelectedPageIndex);

                //直前に開かれているアカウントを次回起動時に開くために記録する
                if (value < 0 || value >= _accountManagerModel.Accounts.Count)
                    return;
                Properties.Settings.Default.RecentlyUsedAccountEMailAddress = _accountManagerModel.Accounts[value].Builder.Email;
                Properties.Settings.Default.Save();
            }
        }
        public int SelectedPageIndex
        {
            get { return _selectedAccountIndex + 1; }
            set { SelectedAccountIndex = value - 1; }
        }
        public bool IsAccountSelectorMode
        {
            get { return _isAccountSelectorMode; }
            set { Set(() => IsAccountSelectorMode, ref _isAccountSelectorMode, value); }
        }
        public bool NoSelectableAccount
        {
            get { return _noSelectableAccount; }
            set { Set(() => NoSelectableAccount, ref _noSelectableAccount, value); }
        }
        public ObservableCollection<AccountViewModel> Accounts { get; set; }
        public ObservableCollection<AccountViewModel> Pages { get; set; }

        public void Initialize()
        {
            Task.Run(async () =>
                {
                    try
                    {
                        if (IsInDesignMode)
                        {
                            // Code runs in Blend --> create design time data.
                        }
                        else
                            await _accountManagerModel.Initialize().ConfigureAwait(false);
                    }
                    catch (FailToOperationException)
                    {
                        var message = new DialogOptionInfo(
                            "Error", "アカウント一覧の読み込みに失敗しました。ネットワークの設定を確認して下さい。",
                            setting: new MetroDialogSettings() { AffirmativeButtonText = "再接続" });
                        Messenger.Default.Send(message);
                        var tmp = message.CallbackTask.ContinueWith(tsk => Initialize());
                    }
                });
        }
        public override void Cleanup()
        {
            lock (Pages)
            {
                base.Cleanup();
                foreach (AccountViewModel item in Pages.Skip(1))
                    item.Cleanup();
            }
        }
        async void _accountManagerModel_Initialized(object sender, EventArgs e)
        {
            try
            {
                await _accountSyncer.WaitAsync().ConfigureAwait(false);
                NoSelectableAccount = _accountManagerModel.Accounts.Count == 0;
                if (Properties.Settings.Default.RecentlyUsedAccountEMailAddress != null)
                {
                    //前回起動時の最後に表示されていたアカウントのメールアドレスが
                    //読み込まれているアカウントリストにある場合はこれを表示する
                    var recentlyUsedAccount = _accountManagerModel.Accounts.FirstOrDefault(
                        account => account.Builder.Email == Properties.Settings.Default.RecentlyUsedAccountEMailAddress);
                    if (recentlyUsedAccount != null)
                        SelectedPageIndex = 1 + _accountManagerModel.Accounts.IndexOf(recentlyUsedAccount);
                    else
                    {
                        IsAccountSelectorMode = true;
                        SelectedPageIndex = 0;
                    }
                }
            }
            finally { _accountSyncer.Release(); }
        }
        async void PageSwitcherViewModel_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            try
            {
                await _accountSyncer.WaitAsync().ConfigureAwait(false);
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        for (var i = 0; i < e.NewItems.Count; i++)
                        {
                            var accountModel = (Account)e.NewItems[i];
                            var account = new AccountViewModel(accountModel, this);
                            await Accounts.InsertOnDispatcher(e.NewStartingIndex + i, account).ConfigureAwait(false);
                            await Pages.InsertOnDispatcher(1 + e.NewStartingIndex + i, account).ConfigureAwait(false);
                        }
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        for (var i = 0; i < e.OldItems.Count; i++)
                        {
                            await Accounts.RemoveAtOnDispatcher(e.OldStartingIndex).ConfigureAwait(false);
                            await Pages.RemoveAtOnDispatcher(1 + e.OldStartingIndex).ConfigureAwait(false);
                            if (Pages.Count == 0)
                                SelectedPageIndex = 0;
                        }
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        if (Pages.Count > 0)
                        {
                            await Accounts.ClearOnDispatcher().ConfigureAwait(false);
                            await Pages.ClearOnDispatcher().ConfigureAwait(false);
                            await Pages.AddOnDispatcher(null).ConfigureAwait(false);
                            SelectedPageIndex = 0;
                        }
                        break;
                }
            }
            finally { _accountSyncer.Release(); }
        }
    }
    public class DialogOptionInfo
    {
        public DialogOptionInfo(string title, string message, MessageDialogStyle style = MessageDialogStyle.Affirmative, MetroDialogSettings setting = null)
        {
            Title = title;
            Message = message;
            Style = style;
            Settings = setting;
            CallbackTaskSource = new TaskCompletionSource<MessageDialogResult>();
        }
        public string Title { get; set; }
        public string Message { get; set; }
        public MessageDialogStyle Style { get; set; }
        public MetroDialogSettings Settings { get; set; }
        public TaskCompletionSource<MessageDialogResult> CallbackTaskSource { get; private set; }
        public Task<MessageDialogResult> CallbackTask { get { return CallbackTaskSource.Task; } }
    }
    public class IntToVisibilityConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        { return ((int)value) != 0 ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed; }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        { throw new NotImplementedException(); }
    }
    public class IntToIntConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        { return (int)value - 1; }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        { throw new NotImplementedException(); }
    }
}
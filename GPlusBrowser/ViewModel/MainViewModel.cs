using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Ioc;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
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
            _selectedMainPageIndex = -1;
            IsAccountSelectorMode = false;
            Pages = new ObservableCollection<AccountViewModel>();

            Messenger.Default.Register<DialogMessage>(this, Recieved_DialogMessage);
            Initialize(accountManagerModel);
        }
        AccountManager _accountManagerModel;
        bool _isAccountSelectorMode;
        int _selectedMainPageIndex;

        public int SelectedPageIndex
        {
            get { return _selectedMainPageIndex; }
            set { Set(() => SelectedPageIndex, ref _selectedMainPageIndex, value); }
        }
        public bool IsAccountSelectorMode
        {
            get { return _isAccountSelectorMode; }
            set { Set(() => IsAccountSelectorMode, ref _isAccountSelectorMode, value); }
        }
        public ObservableCollection<AccountViewModel> Pages { get; set; }

        public async void Initialize(AccountManager accountManagerModel)
        {
            try
            {
                if (IsInDesignMode)
                {
                    // Code runs in Blend --> create design time data.
                }
                else
                {
                    if (_accountManagerModel != null)
                        ((INotifyCollectionChanged)_accountManagerModel.Accounts)
                            .CollectionChanged -= PageSwitcherViewModel_CollectionChanged;

                    _accountManagerModel = accountManagerModel;
                    ((INotifyCollectionChanged)_accountManagerModel.Accounts)
                        .CollectionChanged += PageSwitcherViewModel_CollectionChanged;
                    await _accountManagerModel.Initialize().ConfigureAwait(false);
                }
            }
            catch (FailToOperationException)
            {
                Messenger.Default.Send(new DialogMessage(
                    "アカウント一覧の読み込みに失敗しました。ネットワークの設定を確認して下さい。",
                    res => Initialize(accountManagerModel)));
            }
        }
        public override void Cleanup()
        {
            lock (Pages)
            {
                base.Cleanup();
                Messenger.Default.Unregister<DialogMessage>(this, Recieved_DialogMessage);
                foreach (AccountViewModel item in Pages)
                    item.Cleanup();
            }
        }
        void Recieved_DialogMessage(DialogMessage message)
        {

        }
        void PageSwitcherViewModel_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            lock (Pages)
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        for (var i = 0; i < e.NewItems.Count; i++)
                        {
                            var accountModel = (Account)e.NewItems[i];
                            var account = new AccountViewModel(accountModel, this);
                            Pages.InsertOnDispatcher(e.NewStartingIndex + i, account);
                            if (SelectedPageIndex == -1)
                                SelectedPageIndex = 0;
                        }
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        for (var i = 0; i < e.OldItems.Count; i++)
                        {
                            Pages.RemoveAtOnDispatcher(e.OldStartingIndex);
                            if (Pages.Count == 0)
                                SelectedPageIndex = -1;
                        }
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        if (Pages.Count > 0)
                        {
                            Pages.Clear();
                            SelectedPageIndex = -1;
                        }
                        break;
                }
        }
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
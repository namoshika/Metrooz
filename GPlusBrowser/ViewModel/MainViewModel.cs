using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Windows.Threading;

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
            IsAccountSelectorMode = true;
            Pages = new ObservableCollection<AccountViewModel>();

            Initialize(accountManagerModel);
        }
        AccountManager _accountManagerModel;
        bool _isAccountSelectorMode;
        int _selectedMainPageIndex;

        public int SelectedPageIndex
        {
            get { return _selectedMainPageIndex; }
            set
            {
                IsAccountSelectorMode = value < 0;
                Set(() => SelectedPageIndex, ref _selectedMainPageIndex, value);
            }
        }
        public bool IsAccountSelectorMode
        {
            get { return _isAccountSelectorMode; }
            set { Set(() => IsAccountSelectorMode, ref _isAccountSelectorMode, value); }
        }
        public ObservableCollection<AccountViewModel> Pages { get; set; }

        public async void Initialize(AccountManager accountManagerModel)
        {
            if (IsInDesignMode)
            {
                // Code runs in Blend --> create design time data.
            }
            else
            {
                _accountManagerModel = accountManagerModel;
                ((INotifyCollectionChanged)_accountManagerModel.Accounts)
                    .CollectionChanged += PageSwitcherViewModel_CollectionChanged;
                await _accountManagerModel.Initialize();
            }
        }
        public override void Cleanup()
        {
            base.Cleanup();
            DataCacheDictionary.Clear();
            foreach (AccountViewModel item in Pages.Skip(1))
                item.Cleanup();
        }
        void PageSwitcherViewModel_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    for (var i = 0; i < e.NewItems.Count; i++)
                    {
                        var accountModel = (Account)e.NewItems[i];
                        var account = new AccountViewModel(accountModel);
                        account.OpenedStreamPanel += account_OpenedStreamPanel;
                        account.BackedToAccountManager += account_BackedToAccountManager;
                        Pages.InsertAsync(e.NewStartingIndex + i, account, App.Current.Dispatcher);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    for (var i = 0; i < e.OldItems.Count; i++)
                    {
                        var account = Pages[e.OldStartingIndex];
                        account.OpenedStreamPanel -= account_OpenedStreamPanel;
                        account.BackedToAccountManager -= account_BackedToAccountManager;
                        Pages.RemoveAtAsync(e.OldStartingIndex, App.Current.Dispatcher);
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    if (Pages.Count > 0)
                    {
                        var accountManager = Pages.First();
                        foreach (var item in Pages.OfType<AccountViewModel>())
                        {
                            item.OpenedStreamPanel -= account_OpenedStreamPanel;
                            item.BackedToAccountManager -= account_BackedToAccountManager;
                        }
                        Pages.Clear();
                        Pages.AddAsync(accountManager, App.Current.Dispatcher);
                        SelectedPageIndex = -1;
                    }
                    break;
            }
        }
        void account_OpenedStreamPanel(object sender, EventArgs e)
        {
            var idx = Pages.IndexOf((AccountViewModel)sender);
            SelectedPageIndex = idx;
        }
        void account_BackedToAccountManager(object sender, EventArgs e)
        { SelectedPageIndex = -1; }
    }
}
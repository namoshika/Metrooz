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

    public class PageSwitcherViewModel : ViewModelBase, IDisposable
    {
        public PageSwitcherViewModel(AccountManager accountManagerModel, Dispatcher uiThreadDispatcher)
            : base(uiThreadDispatcher, null)
        {
            _accountManagerModel = accountManagerModel;
            _accountManagerModel.ChangedAccounts += _accountManagerModel_ChangedAccounts;
            _accountManagerModel.ChangedSelectedAccountIndex += _accountManagerModel_ChangedSelectedAccountIndex;
            _selectedMainPageIndex = -1;
            IsAccountSelectorMode = true;
            AccountSelector = new AccountSelectorViewModel(accountManagerModel, this, uiThreadDispatcher);
            Pages = new ObservableCollection<AccountViewModel>();
        }
        AccountManager _accountManagerModel;
        bool _isAccountSelectorMode;
        int _selectedMainPageIndex;

        public int SelectedPageIndex
        {
            get { return _selectedMainPageIndex; }
            set
            {
                if (_selectedMainPageIndex == value)
                    return;
                IsAccountSelectorMode = value < 0;
                _selectedMainPageIndex = value;
                _accountManagerModel.SelectedAccountIndex = _selectedMainPageIndex;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("SelectedPageIndex"));
            }
        }
        public bool IsAccountSelectorMode
        {
            get { return _isAccountSelectorMode; }
            set
            {
                if (_isAccountSelectorMode == value)
                    return;
                _isAccountSelectorMode = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("IsAccountSelectorMode"));
            }
        }
        public AccountSelectorViewModel AccountSelector { get; set; }
        public ObservableCollection<AccountViewModel> Pages { get; set; }

        public void Dispose()
        {
            foreach (AccountViewModel item in Pages.Skip(1))
                item.Dispose();
        }
        void _accountManagerModel_ChangedAccounts(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    for (var i = 0; i < e.NewItems.Count; i++)
                    {
                        var accountModel = (Account)e.NewItems[i];
                        var account = new AccountViewModel(accountModel, _accountManagerModel, UiThreadDispatcher);
                        Pages.InsertAsync(e.NewStartingIndex + i, account, UiThreadDispatcher);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    for (var i = 0; i < e.OldItems.Count; i++)
                        Pages.RemoveAtAsync(e.OldStartingIndex, UiThreadDispatcher);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    var accountManager = Pages.First();
                    Pages.Clear();
                    Pages.AddAsync(accountManager, UiThreadDispatcher);
                    SelectedPageIndex = -1;
                    break;
            }
        }
        void _accountManagerModel_ChangedSelectedAccountIndex(object sender, EventArgs e)
        { SelectedPageIndex = _accountManagerModel.SelectedAccountIndex; }
    }
}
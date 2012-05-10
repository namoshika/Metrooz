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

    public class AccountSwitcherViewModel : ViewModelBase, IDisposable
    {
        public AccountSwitcherViewModel(AccountManager accountManagerModel, Dispatcher uiThreadDispatcher)
            : base(uiThreadDispatcher)
        {
            _accountManagerModel = accountManagerModel;
            _accountManagerModel.ChangedAccounts += _accountManagerModel_ChangedAccounts;
            _accountManagerModel.ChangedSelectedAccountIndex += _accountManagerModel_ChangedSelectedAccountIndex;
            Pages = new DispatchObservableCollection<object>(uiThreadDispatcher);
            Pages.Add(new AccountManagerViewModel(accountManagerModel, uiThreadDispatcher));
        }
        AccountManager _accountManagerModel;
        int _selectedAccountIndex;

        public int SelectedPageIndex
        {
            get { return _selectedAccountIndex; }
            set
            {
                if (_selectedAccountIndex == value)
                    return;
                _selectedAccountIndex = value;
                _accountManagerModel.SelectedAccountIndex = _selectedAccountIndex - 1;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("SelectedPageIndex"));
            }
        }
        public DispatchObservableCollection<object> Pages { get; set; }

        void _accountManagerModel_ChangedAccounts(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    for (var i = 0; i < e.NewItems.Count; i++)
                        Pages.Insert(e.NewStartingIndex + i + 1, new AccountViewModel((Account)e.NewItems[i], _accountManagerModel, UiThreadDispatcher));
                    break;
                case NotifyCollectionChangedAction.Remove:
                    for (var i = 0; i < e.OldItems.Count; i++)
                        Pages.RemoveAt(e.OldStartingIndex + 1);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    var accountManager = Pages.First();
                    Pages.Clear();
                    Pages.Add(accountManager);
                    break;
            }
        }
        void _accountManagerModel_ChangedSelectedAccountIndex(object sender, EventArgs e)
        {
            if (SelectedPageIndex == _accountManagerModel.SelectedAccountIndex + 1)
                return;
            SelectedPageIndex = _accountManagerModel.SelectedAccountIndex + 1;
        }

        public void Dispose()
        {
            foreach (AccountViewModel item in Pages.Skip(1))
                item.Dispose();
        }
    }
}
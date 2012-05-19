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
            Pages = new ObservableCollection<object>();
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
        public ObservableCollection<object> Pages { get; set; }

        void _accountManagerModel_ChangedAccounts(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    for (var i = 0; i < e.NewItems.Count; i++)
                        Pages.InsertAsync(e.NewStartingIndex + i + 1, new AccountViewModel((Account)e.NewItems[i], _accountManagerModel, UiThreadDispatcher), UiThreadDispatcher);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    for (var i = 0; i < e.OldItems.Count; i++)
                        Pages.RemoveAtAsync(e.OldStartingIndex + 1, UiThreadDispatcher);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    var accountManager = Pages.First();
                    Pages.Clear();
                    Pages.AddAsync(accountManager, UiThreadDispatcher);
                    break;
            }
        }
        void _accountManagerModel_ChangedSelectedAccountIndex(object sender, EventArgs e)
        {
            UiThreadDispatcher.BeginInvoke((Action)delegate()
            {
                if (SelectedPageIndex == _accountManagerModel.SelectedAccountIndex + 1)
                    return;
                SelectedPageIndex = _accountManagerModel.SelectedAccountIndex + 1;
            },
            _accountManagerModel.SelectedAccountIndex < Pages.Count - 1
                ? DispatcherPriority.DataBind : DispatcherPriority.ContextIdle);
        }

        public void Dispose()
        {
            foreach (AccountViewModel item in Pages.Skip(1))
                item.Dispose();
        }
    }
}
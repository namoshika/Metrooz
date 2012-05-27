using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows.Threading;

namespace GPlusBrowser.ViewModel
{
    using Model;

    public class AccountManagerViewModel : ViewModelBase
    {
        public AccountManagerViewModel(AccountManager accountManagerModel, Dispatcher uiThreadDispatcher)
            : base(uiThreadDispatcher)
        {
            _accountManagerModel = accountManagerModel;
            _accountManagerModel.ChangedAccounts += _accountManagerModel_ChangedAccounts;
            Loginer = new LoginerViewModel(accountManagerModel, uiThreadDispatcher);
            Accounts = new ObservableCollection<AccountPanelViewModel>();
            OpenAddAccountPanelCommand = new RelayCommand(
                OpenAddAccountPanelCommand_Executed, OpenAddAccountPanelCommand_CanExecuted);
        }

        AccountManager _accountManagerModel;
        public LoginerViewModel Loginer { get; set; }
        public ObservableCollection<AccountPanelViewModel> Accounts { get; set; }
        public ICommand OpenAddAccountPanelCommand { get; set; }

        void OpenAddAccountPanelCommand_Executed(object arg)
        { Loginer.OpenPanel(_accountManagerModel.Create(), false); }
        bool OpenAddAccountPanelCommand_CanExecuted(object arg)
        { return Loginer.Status == LoginSequenceStatus.Hidden; }
        void _accountManagerModel_ChangedAccounts(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    for (var i = 0; i < e.NewItems.Count; i++)
                    {
                        var circle = (Account)e.NewItems[i];
                        var circleVm = new AccountPanelViewModel(circle, _accountManagerModel, UiThreadDispatcher);
                        Accounts.InsertAsync(e.NewStartingIndex + i, circleVm, UiThreadDispatcher);
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    Accounts.MoveAsync(e.OldStartingIndex, e.NewStartingIndex, UiThreadDispatcher);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    for (var i = 0; i < e.OldItems.Count; i++)
                        Accounts.RemoveAtAsync(e.OldStartingIndex, UiThreadDispatcher);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    Accounts.ClearAsync(UiThreadDispatcher);
                    break;
            }
        }
    }
}
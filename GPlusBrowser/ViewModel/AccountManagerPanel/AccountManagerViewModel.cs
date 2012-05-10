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
            Accounts = new DispatchObservableCollection<AccountPanelViewModel>(uiThreadDispatcher);
            OpenAddAccountPanelCommand = new RelayCommand(
                OpenAddAccountPanelCommand_Executed, OpenAddAccountPanelCommand_CanExecuted);
        }

        AccountManager _accountManagerModel;
        public LoginerViewModel Loginer { get; set; }
        public DispatchObservableCollection<AccountPanelViewModel> Accounts { get; set; }
        public ICommand OpenAddAccountPanelCommand { get; set; }

        void OpenAddAccountPanelCommand_Executed(object arg)
        { Loginer.OpenPanel(_accountManagerModel.Create(), false); }
        bool OpenAddAccountPanelCommand_CanExecuted(object arg)
        { return Loginer.Status == LoginSequenceStatus.Hidden; }
        void _accountManagerModel_ChangedAccounts(
            object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    for (var i = 0; i < e.NewItems.Count; i++)
                    {
                        var circle = (Account)e.NewItems[i];
                        var circleVm = new AccountPanelViewModel(circle, _accountManagerModel, UiThreadDispatcher);
                        Accounts.Insert(
                            e.NewStartingIndex + i, circleVm);
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                    Accounts.Move(e.OldStartingIndex, e.NewStartingIndex);
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    for (var i = 0; i < e.OldItems.Count; i++)
                        Accounts.RemoveAt(e.OldStartingIndex);
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    Accounts.Clear();
                    break;
            }
        }
    }
}
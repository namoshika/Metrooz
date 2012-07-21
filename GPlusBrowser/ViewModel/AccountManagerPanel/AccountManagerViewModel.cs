using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace GPlusBrowser.ViewModel
{
    using Model;

    public class AccountManagerViewModel : ViewModelBase
    {
        public AccountManagerViewModel(AccountManager accountManagerModel, LoginerViewModel loginer, Dispatcher uiThreadDispatcher)
            : base(uiThreadDispatcher)
        {
            _accountManagerModel = accountManagerModel;
            _accountManagerModel.ChangedAccounts += _accountManagerModel_ChangedAccounts;
            _loginer = loginer;
            Accounts = new ObservableCollection<AccountPanelViewModel>();
            OpenAddAccountPanelCommand = new RelayCommand(OpenAddAccountPanelCommand_Executed);
        }

        AccountManager _accountManagerModel;
        LoginerViewModel _loginer;
        bool _isShowStatusText;
        string _statusText;

        public bool IsShowStatusText
        {
            get { return _isShowStatusText; }
            set
            {
                _isShowStatusText = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("IsShowStatusText"));
            }
        }
        public string StatusText
        {
            get { return _statusText; }
            set
            {
                _statusText = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("StatusText"));
            }
        }
        public ObservableCollection<AccountPanelViewModel> Accounts { get; set; }
        public ICommand OpenAddAccountPanelCommand { get; set; }

        async void OpenAddAccountPanelCommand_Executed(object arg)
        {
            var account = await _loginer
                .OpenLoginForm(_accountManagerModel.Create()).ConfigureAwait(false);
            if (account == null)
                return;
            _accountManagerModel.Add(account);
        }
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
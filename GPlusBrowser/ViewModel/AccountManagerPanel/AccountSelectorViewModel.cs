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

    public class AccountSelectorViewModel : ViewModelBase
    {
        public AccountSelectorViewModel(AccountManager accountManagerModel, AccountSwitcherViewModel accountManagerVM, Dispatcher uiThreadDispatcher)
            : base(uiThreadDispatcher, null)
        {
            _accountManagerModel = accountManagerModel;
            _accountManagerVM = accountManagerVM;
        }
        AccountManager _accountManagerModel;
        AccountSwitcherViewModel _accountManagerVM;
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
        public ObservableCollection<AccountViewModel> Accounts { get { return _accountManagerVM.Pages; } }
    }
}